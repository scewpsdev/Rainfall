using Rainfall;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


internal static class Renderer
{
	const int BLOOM_CHAIN_LENGTH = 6;


	enum RenderPass : int
	{
		Geometry,
		Shadow0,
		Shadow1,
		Shadow2,
		AmbientOcclusion,
		AmbientOcclusionBlur,
		Deferred,
		Forward,
		BloomDownsample,
		BloomUpsample = BloomDownsample + BLOOM_CHAIN_LENGTH,
		Tonemapping = BloomUpsample + BLOOM_CHAIN_LENGTH - 1,
		UI,
	}

	struct ModelDrawCommand
	{
		internal Model model;
		internal int meshID;
		internal Animator animator;
		internal Matrix transform;
	}

	struct TerrainDrawCommand
	{
		internal Model model;
		internal Matrix transform;
		internal Texture splatMap;
	}

	struct LeaveDrawCommand
	{
		internal Model model;
		internal int meshID;
		internal Matrix transform;
	}

	struct SkyDrawCommand
	{
		internal Cubemap cubemap;
		internal Matrix transform;
	}

	struct LightDrawCommand
	{
		internal Vector3 position;
		internal Vector3 color;
	}

	struct DirectionalLightDrawCommand
	{
		internal DirectionalLight light;
	}

	struct ParticleDrawCommand
	{
		internal Vector3 position;
		internal Texture atlas;
		internal int u0, v0, u1, v1;
		internal float size;
		internal float rotation;
		internal uint color;
		internal bool additive;
	}

	struct GrassDrawCommand
	{
		internal Vector2 position;
		internal GrassBladeData[] blades;
		internal Terrain terrain;
	}

	struct UITextureDrawCommand
	{
		internal int x, y;
		internal int width, height;
		internal Texture texture;
		internal int u0, v0, u1, v1;
		internal uint color;
	}

	struct TextDrawCommand
	{
		internal int x, y;
		internal float scale;
		internal string text;
		internal int length;
		internal Font font;
		internal uint color;
	}


	const int SSAO_KERNEL_SIZE = 64;
	const int MAX_LIGHTS_PER_PASS = 16;
	const int NUM_GRASS_BLADES = 1024;

	static GraphicsDevice graphics;

	static RenderTarget gbuffer;
	static RenderTarget composite;
	static VertexBuffer quad;
	static VertexBuffer skydome;
	static IndexBuffer skydomeIdx;

	static Texture dummyShadowMap;

	static RenderTarget ssaoRenderTarget;
	//static Vector4[] ssaoKernel;
	static Texture ssaoNoiseTexture;
	static RenderTarget ssaoBlurRenderTarget;

	static RenderTarget[] bloomDownsampleChain;
	static RenderTarget[] bloomUpsampleChain;

	static Shader modelShader;
	static Shader modelAnimShader;
	static Shader modelDepthShader;
	static Shader modelAnimDepthShader;
	static Shader terrainShader;
	static Shader foliageShader;
	static Shader ssaoShader;
	static Shader ssaoBlurShader;
	static Shader deferredShader;
	static Shader skyShader;
	static Shader particleShader;
	static Shader particleAdditiveShader;
	static Shader grassShader;
	static Shader bloomDownsampleShader;
	static Shader bloomUpsampleShader;
	static Shader tonemappingShader;
	static Shader uiTextureShader;
	static Shader textShader;

	static SpriteBatch particleBatch;
	static SpriteBatch uiTextureBatch;
	static SpriteBatch textBatch;

	public static Camera camera;
	public static Matrix projection, view;

	static Cubemap environmentMap;

	static VertexBuffer grassBlade;
	static IndexBuffer grassIndices;
	static Texture perlinTexture;

	static List<ModelDrawCommand> models = new List<ModelDrawCommand>();
	static List<TerrainDrawCommand> terrains = new List<TerrainDrawCommand>();
	static List<LeaveDrawCommand> foliage = new List<LeaveDrawCommand>();
	static List<SkyDrawCommand> skies = new List<SkyDrawCommand>();
	static List<LightDrawCommand> lights = new List<LightDrawCommand>();
	static List<DirectionalLightDrawCommand> directionalLights = new List<DirectionalLightDrawCommand>();
	static List<ParticleDrawCommand> particles = new List<ParticleDrawCommand>();
	static List<ParticleDrawCommand> particlesAdditive = new List<ParticleDrawCommand>();
	static List<GrassDrawCommand> grassPatches = new List<GrassDrawCommand>();
	static List<UITextureDrawCommand> uiTextures = new List<UITextureDrawCommand>();
	static List<TextDrawCommand> texts = new List<TextDrawCommand>();


	public static void Init(GraphicsDevice graphics)
	{
		Renderer.graphics = graphics;

		gbuffer = graphics.createRenderTarget(new RenderTargetAttachment[] {
			new RenderTargetAttachment(BackbufferRatio.Equal, TextureFormat.RGBA32F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp),
			new RenderTargetAttachment(BackbufferRatio.Equal, TextureFormat.RGBA16F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp),
			new RenderTargetAttachment(BackbufferRatio.Equal, TextureFormat.RGBA8, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp),
			new RenderTargetAttachment(BackbufferRatio.Equal, TextureFormat.RGBA8, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp),
			new RenderTargetAttachment(BackbufferRatio.Equal, TextureFormat.D16F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp)
		});

		composite = graphics.createRenderTarget(new RenderTargetAttachment[]
		{
			new RenderTargetAttachment(BackbufferRatio.Equal, TextureFormat.RG11B10F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp),
			new RenderTargetAttachment(BackbufferRatio.Equal, TextureFormat.D16F, (ulong)TextureFlags.RenderTarget | (ulong)TextureFlags.RenderTargetWriteOnly | (ulong)TextureFlags.BlitDst | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp),
		});

		quad = graphics.createVertexBuffer(
			graphics.createVideoMemory(new float[] { -3.0f, -1.0f, 1.0f, 1.0f, -1.0f, 1.0f, 1.0f, 3.0f, 1.0f }),
			new VertexLayout(new VertexElement[] { new VertexElement(VertexAttribute.Position, VertexAttributeType.Vector3, false) })
		);

		skydome = graphics.createVertexBuffer(
			graphics.createVideoMemory(new float[] { -100.0f, -100.0f, 100.0f, 100.0f, -100.0f, 100.0f, 0.0f, -100.0f, -100.0f, 0.0f, 100.0f, 0.0f }),
			new VertexLayout(new VertexElement[] { new VertexElement(VertexAttribute.Position, VertexAttributeType.Vector3, false) })
		);

		skydomeIdx = graphics.createIndexBuffer(graphics.createVideoMemory(new short[] { 0, 1, 2, 2, 1, 3, 1, 0, 3, 0, 2, 3 }));

		dummyShadowMap = graphics.createTexture(1, 1, TextureFormat.D16F, graphics.createVideoMemory(new Half[] { (Half)1.0f }), (uint)SamplerFlags.CompareLEqual);

		ssaoRenderTarget = graphics.createRenderTarget(new RenderTargetAttachment[]
		{
			new RenderTargetAttachment(BackbufferRatio.Equal, TextureFormat.R8, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp)
		});

		/*
		ssaoKernel = new Vector4[SSAO_KERNEL_SIZE];
		for (int i = 0; i < ssaoKernel.Length; i++)
		{
			Vector3 sample = new Vector3(
				Random.Shared.NextSingle() * 2.0f - 1.0f,
				Random.Shared.NextSingle() * 2.0f - 1.0f,
				Random.Shared.NextSingle()
			);

			float scale = i / (float)ssaoKernel.Length;
			scale = MathHelper.Lerp(0.1f, 1.0f, scale * scale);
			sample = sample.normalized * scale * Random.Shared.NextSingle();
			ssaoKernel[i] = new Vector4(sample, 0.0f);
		}
		*/

		byte[] ssaoNoiseData = new byte[4 * 4 * 2];
		Random.Shared.NextBytes(ssaoNoiseData);
		ssaoNoiseTexture = graphics.createTexture(4, 4, TextureFormat.RG8, graphics.createVideoMemory(ssaoNoiseData));

		ssaoBlurRenderTarget = graphics.createRenderTarget(new RenderTargetAttachment[]
		{
			new RenderTargetAttachment(BackbufferRatio.Equal, TextureFormat.R8, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp)
		});

		grassBlade = graphics.createVertexBuffer(
			graphics.createVideoMemory(new float[] {
				-0.05f, 0.0f, 0.0f,
				0.05f, 0.0f, 0.0f,
				0.0f, 1.0f, 0.0f,
				-0.5f, 1.0f, 0.0f,
			}),
			new VertexLayout(new VertexElement[] {
				new VertexElement(VertexAttribute.Position, VertexAttributeType.Vector3, false)
			})
		);
		grassIndices = graphics.createIndexBuffer(
			graphics.createVideoMemory(new short[] { 0, 1, 2 })
		);

		bloomDownsampleChain = new RenderTarget[BLOOM_CHAIN_LENGTH];
		bloomUpsampleChain = new RenderTarget[BLOOM_CHAIN_LENGTH - 1];
		for (int i = 0; i < BLOOM_CHAIN_LENGTH; i++)
		{
			int exp = (int)Math.Pow(2, i + 1);
			int width = Display.viewportSize.x / exp;
			int height = Display.viewportSize.y / exp;

			bloomDownsampleChain[i] = graphics.createRenderTarget(new RenderTargetAttachment[]
			{
				new RenderTargetAttachment(width, height, TextureFormat.RG11B10F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp)
			});

			if (i < BLOOM_CHAIN_LENGTH - 1)
			{
				bloomUpsampleChain[i] = graphics.createRenderTarget(new RenderTargetAttachment[]
				{
					new RenderTargetAttachment(width, height, TextureFormat.RG11B10F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp)
				});
			}
		}

		perlinTexture = Resource.GetTexture("res/texture/perlin.png");


		modelShader = Resource.GetShader("res/shaders/model/model.vs.shader", "res/shaders/model/model.fs.shader");
		modelAnimShader = Resource.GetShader("res/shaders/model_anim/model_anim.vs.shader", "res/shaders/model_anim/model_anim.fs.shader");
		modelDepthShader = Resource.GetShader("res/shaders/model/model_depth.vs.shader", "res/shaders/model/model_depth.fs.shader");
		modelAnimDepthShader = Resource.GetShader("res/shaders/model_anim/model_anim_depth.vs.shader", "res/shaders/model_anim/model_anim_depth.fs.shader");
		terrainShader = Resource.GetShader("res/shaders/terrain/terrain.vs.shader", "res/shaders/terrain/terrain.fs.shader");
		foliageShader = Resource.GetShader("res/shaders/foliage/foliage.vs.shader", "res/shaders/foliage/foliage.fs.shader");
		ssaoShader = Resource.GetShader("res/shaders/ssao/ssao.vs.shader", "res/shaders/ssao/ssao.fs.shader");
		ssaoBlurShader = Resource.GetShader("res/shaders/ssao/ssao_blur.vs.shader", "res/shaders/ssao/ssao_blur.fs.shader");
		deferredShader = Resource.GetShader("res/shaders/deferred/deferred.vs.shader", "res/shaders/deferred/deferred.fs.shader");
		skyShader = Resource.GetShader("res/shaders/sky/sky.vs.shader", "res/shaders/sky/sky.fs.shader");
		particleShader = Resource.GetShader("res/shaders/particle/particle.vs.shader", "res/shaders/particle/particle.fs.shader");
		particleAdditiveShader = Resource.GetShader("res/shaders/particle_additive/particle_additive.vs.shader", "res/shaders/particle_additive/particle_additive.fs.shader");
		grassShader = Resource.GetShader("res/shaders/grass/grass.vs.shader", "res/shaders/grass/grass.fs.shader");
		bloomDownsampleShader = Resource.GetShader("res/shaders/bloom/bloom.vs.shader", "res/shaders/bloom/bloom_downsample.fs.shader");
		bloomUpsampleShader = Resource.GetShader("res/shaders/bloom/bloom.vs.shader", "res/shaders/bloom/bloom_upsample.fs.shader");
		tonemappingShader = Resource.GetShader("res/shaders/tonemapping/tonemapping.vs.shader", "res/shaders/tonemapping/tonemapping.fs.shader");
		uiTextureShader = Resource.GetShader("res/shaders/ui/ui.vs.shader", "res/shaders/ui/ui.fs.shader");
		textShader = Resource.GetShader("res/shaders/text/text.vs.shader", "res/shaders/text/text.fs.shader");

		particleBatch = new SpriteBatch(graphics);
		uiTextureBatch = new SpriteBatch(graphics);
		textBatch = new SpriteBatch(graphics);
	}

	public static void DrawModel(Model model, Matrix transform, Animator animator = null)
	{
		models.Add(new ModelDrawCommand { model = model, meshID = -1, transform = transform, animator = animator });
	}

	public static void DrawMesh(Model model, int meshID, Matrix transform)
	{
		models.Add(new ModelDrawCommand { model = model, meshID = meshID, transform = transform });
	}

	public static void DrawTerrain(Model model, Matrix transform, Texture splatMap)
	{
		terrains.Add(new TerrainDrawCommand { model = model, transform = transform, splatMap = splatMap });
	}

	public static void DrawLeaves(Model model, int meshID, Matrix transform)
	{
		foliage.Add(new LeaveDrawCommand { model = model, meshID = meshID, transform = transform });
	}

	public static void DrawLight(Vector3 position, Vector3 color)
	{
		lights.Add(new LightDrawCommand { position = position, color = color });
	}

	public static void DrawDirectionalLight(DirectionalLight light)
	{
		directionalLights.Add(new DirectionalLightDrawCommand { light = light });
	}

	public static void DrawSky(Cubemap cubemap, Matrix transform)
	{
		skies.Add(new SkyDrawCommand { cubemap = cubemap, transform = transform });
	}

	public static void DrawParticle(Vector3 position, Texture atlas, int u0, int v0, int u1, int v1, float size, float rotation, uint color, bool additive)
	{
		if (additive)
			particlesAdditive.Add(new ParticleDrawCommand { position = position, atlas = atlas, u0 = u0, v0 = v0, u1 = u1, v1 = v1, size = size, rotation = rotation, color = color, additive = true });
		else
			particles.Add(new ParticleDrawCommand { position = position, atlas = atlas, u0 = u0, v0 = v0, u1 = u1, v1 = v1, size = size, rotation = rotation, color = color, additive = false });
	}

	public static void DrawGrassPatch(GrassBladeData[] blades, Terrain terrain, Vector2 position)
	{
		grassPatches.Add(new GrassDrawCommand { position = position, terrain = terrain, blades = blades });
	}

	public static void DrawUITexture(int x, int y, int width, int height, Texture texture, int u0, int v0, int u1, int v1, uint color)
	{
		uiTextures.Add(new UITextureDrawCommand { x = x, y = Display.viewportSize.y - y - height, width = width, height = height, texture = texture, u0 = u0, v0 = v0, u1 = u1, v1 = v1, color = color });
	}

	public static void DrawUITexture(int x, int y, int width, int height, Texture texture)
	{
		uiTextures.Add(new UITextureDrawCommand { x = x, y = Display.viewportSize.y - y - height, width = width, height = height, texture = texture, u0 = 0, v0 = 0, u1 = texture.info.width, v1 = texture.info.height, color = 0xffffffff });
	}

	public static void DrawUIRect(int x, int y, int width, int height, uint color)
	{
		uiTextures.Add(new UITextureDrawCommand { x = x, y = Display.viewportSize.y - y - height, width = width, height = height, texture = null, u0 = 0, v0 = 0, u1 = 0, v1 = 0, color = color });
	}

	public static void DrawText(int x, int y, float scale, string text, int length, Font font, uint color)
	{
		texts.Add(new TextDrawCommand { x = x, y = y, scale = scale, text = text, length = length, font = font, color = color });
	}

	public static void DrawText(int x, int y, float scale, string text, Font font, uint color)
	{
		DrawText(x, y, scale, text, text.Length, font, color);
	}

	public static void SetEnvironmentMap(Cubemap environmentMap)
	{
		Renderer.environmentMap = environmentMap;
	}

	public static void Begin()
	{
	}

	public static void SetCamera(Camera camera)
	{
		Renderer.camera = camera;

		Renderer.projection = camera.getProjectionMatrix();
		Renderer.view = camera.getViewMatrix();
	}

	static void RenderModels()
	{
		graphics.resetState();

		graphics.setViewTransform(projection, view);

		graphics.setCullState(CullState.ClockWise);

		// TODO frustum culling
		for (int i = 0; i < models.Count; i++)
		{
			Model model = models[i].model;
			int meshID = models[i].meshID;
			Animator animator = models[i].animator;
			Matrix transform = models[i].transform;
			if (meshID == -1)
				graphics.drawModel(model, modelShader, modelAnimShader, animator, transform);
			else
				graphics.drawMesh(model, meshID, modelShader, transform);
		}

		// TODO frustum culling
		for (int i = 0; i < terrains.Count; i++)
		{
			Model model = terrains[i].model;
			Matrix transform = terrains[i].transform;

			graphics.setTexture(terrainShader.getUniform("s_splatMap", UniformType.Sampler), 5, terrains[i].splatMap);

			graphics.drawModel(model, terrainShader, null, null, transform);
		}
	}

	static void RenderFoliage()
	{
		graphics.resetState();

		graphics.setViewTransform(projection, view);

		graphics.setCullState(CullState.ClockWise);

		graphics.setUniform(foliageShader.getUniform("u_animationData", UniformType.Vector4), new Vector4(Time.currentTime / 1e9f, 0.0f, 0.0f, 0.0f));

		// TODO frustum culling
		for (int i = 0; i < foliage.Count; i++)
		{
			Model model = foliage[i].model;
			int meshID = foliage[i].meshID;
			Matrix transform = foliage[i].transform;
			graphics.drawMesh(model, meshID, foliageShader, transform);
		}
	}

	static void RenderGrass()
	{
		if (grassPatches.Count > 0)
		{
			InstanceBuffer grassInstances = graphics.createInstanceBuffer(GrassPatch.NUM_GRASS_BLADES, 16);
			grassInstances.write(GrassPatch.grassData);

			for (int i = 0; i < grassPatches.Count; i++)
			{
				graphics.resetState();

				graphics.setBlendState(BlendState.Default);
				graphics.setCullState(CullState.None);

				graphics.setViewTransform(projection, view);

				graphics.setTransform(Matrix.CreateTranslation(grassPatches[i].position.x, 0.0f, grassPatches[i].position.y));

				graphics.setUniform(grassShader.getUniform("u_animationData", UniformType.Vector4), new Vector4(grassPatches[i].position - grassPatches[i].terrain.position, grassPatches[i].terrain.size, Time.currentTime / 1e9f));

				graphics.setTexture(grassShader.getUniform("s_heightmap", UniformType.Sampler), 0, grassPatches[i].terrain.heightmap);
				graphics.setTexture(grassShader.getUniform("s_normalmap", UniformType.Sampler), 1, grassPatches[i].terrain.normalmap);
				graphics.setTexture(grassShader.getUniform("s_splatMap", UniformType.Sampler), 2, grassPatches[i].terrain.splatMap);
				//graphics.setTexture(grassShader.getUniform("s_perlinTexture", UniformType.Sampler), 0, perlinTexture);

				graphics.setVertexBuffer(grassBlade);
				graphics.setIndexBuffer(grassIndices);

				//float distanceSq = (grassPatches[i].position + new Vector2(GrassPatch.TILE_SIZE) * 0.5f - camera.position.xz).lengthSquared;
				//int lod = distanceSq < GrassPatch.TILE_SIZE * GrassPatch.TILE_SIZE + GrassPatch.TILE_SIZE * GrassPatch.TILE_SIZE ? 0 :
				//	distanceSq < 20 * 20 + 20 * 20 ? 1 : 2;
				//lod = 0;
				//int numGrassBlades = GrassPatch.NUM_GRASS_BLADES / (int)Math.Pow(4, lod);
				graphics.setInstanceBuffer(grassInstances);

				graphics.draw(grassShader);
			}
		}
	}

	static void GeometryPass()
	{
		graphics.setPass((int)RenderPass.Geometry);
		graphics.setRenderTarget(gbuffer);

		RenderModels();
		RenderFoliage();
		RenderGrass();
	}

	static void ShadowPass()
	{
		if (directionalLights.Count > 0)
		{
			ShadowMap shadowMap = directionalLights[0].light.shadowMap;

			shadowMap.calculateCascadeTransforms(camera.position, camera.rotation, camera.fov, Display.aspectRatio);

			for (int i = 0; i < shadowMap.renderTargets.Length; i++)
			{
				graphics.resetState();
				graphics.setPass((int)RenderPass.Shadow0 + i);

				graphics.setRenderTarget(shadowMap.renderTargets[i]);

				//shadowMap.calculateViewTransform(camera.position, camera.rotation, camera.fov, Display.aspectRatio, out Matrix lightProjection, out Matrix lightView);
				graphics.setViewTransform(shadowMap.cascadeProjections[i], shadowMap.cascadeViews[i]);
				//shadowMapToLightSpace = shadowMap.cascadeProjections[i] * shadowMap.cascadeViews[i];

				graphics.setCullState(CullState.None);

				// TODO frustum culling
				for (int j = 0; j < models.Count; j++)
				{
					Model model = models[j].model;
					Animator animator = models[j].animator;
					Matrix transform = models[j].transform;
					graphics.drawModel(model, modelDepthShader, modelAnimDepthShader, animator, transform);
				}


				graphics.setUniform(foliageShader.getUniform("u_animationData", UniformType.Vector4), new Vector4(Time.currentTime / 1e9f, 0.0f, 0.0f, 0.0f));

				// TODO frustum culling
				for (int j = 0; j < foliage.Count; j++)
				{
					Model model = foliage[j].model;
					int meshID = foliage[j].meshID;
					Matrix transform = foliage[j].transform;
					// TODO use depth shader
					graphics.drawMesh(model, meshID, foliageShader, transform);
				}
			}
		}
	}

	static void AmbientOcclusionPass()
	{
		// AO
		{
			graphics.resetState();
			graphics.setPass((int)RenderPass.AmbientOcclusion);

			graphics.setRenderTarget(ssaoRenderTarget);

			graphics.setTexture(ssaoShader.getUniform("s_depthBuffer", UniformType.Sampler), 0, gbuffer.getAttachmentTexture(4));
			graphics.setTexture(ssaoShader.getUniform("s_normalsBuffer", UniformType.Sampler), 1, gbuffer.getAttachmentTexture(1));

			//graphics.setUniform(ssaoShader.getUniform("u_ssaoKernel", UniformType.Vector4, SSAO_KERNEL_SIZE), ssaoKernel, SSAO_KERNEL_SIZE);
			graphics.setTexture(ssaoShader.getUniform("s_ssaoNoise", UniformType.Sampler), 2, ssaoNoiseTexture);

			Vector4 cameraFrustum = new Vector4(camera.near, camera.far, 0.0f, 0.0f);
			Matrix pv = projection * view;
			Matrix pvInv = pv.inverted;
			Matrix viewInv = view.inverted;
			Matrix projectionInv = projection.inverted;

			graphics.setUniform(ssaoShader.getUniform("u_cameraFrustum", UniformType.Vector4), cameraFrustum);
			graphics.setUniform(ssaoShader.getUniform("u_viewMatrix", UniformType.Matrix4), view);
			graphics.setUniform(ssaoShader.getUniform("u_viewInv", UniformType.Matrix4), viewInv);
			graphics.setUniform(ssaoShader.getUniform("u_projectionView", UniformType.Matrix4), pv);
			graphics.setUniform(ssaoShader.getUniform("u_projectionInv", UniformType.Matrix4), projectionInv);
			graphics.setUniform(ssaoShader.getUniform("u_projectionViewInv", UniformType.Matrix4), pvInv);

			graphics.setVertexBuffer(quad);

			graphics.draw(ssaoShader);
		}
		// Blur
		{
			graphics.resetState();
			graphics.setPass((int)RenderPass.AmbientOcclusionBlur);

			graphics.setRenderTarget(ssaoBlurRenderTarget);

			graphics.setTexture(ssaoBlurShader.getUniform("s_depthBuffer", UniformType.Sampler), 0, gbuffer.getAttachmentTexture(4));
			graphics.setTexture(ssaoBlurShader.getUniform("s_ssao", UniformType.Sampler), 1, ssaoRenderTarget.getAttachmentTexture(0));

			Vector4 cameraFrustum = new Vector4(camera.near, camera.far, 0.0f, 0.0f);
			graphics.setUniform(ssaoBlurShader.getUniform("u_cameraFrustum", UniformType.Vector4), cameraFrustum);

			graphics.setVertexBuffer(quad);

			graphics.draw(ssaoBlurShader);
		}
	}

	static void DeferredPass()
	{
		graphics.resetState();
		graphics.setPass((int)RenderPass.Deferred);

		graphics.setRenderTarget(composite);

		Vector3 cameraPosition = view.inverted[3].xyz;
		Vector4[] lightPositionBuffer = new Vector4[MAX_LIGHTS_PER_PASS];
		Vector4[] lightColorBuffer = new Vector4[MAX_LIGHTS_PER_PASS];
		int numLightPasses = Math.Max((int)MathF.Ceiling(lights.Count / (float)MAX_LIGHTS_PER_PASS), 1);


		for (int i = 0; i < numLightPasses; i++)
		{
			graphics.setBlendState(BlendState.Additive);
			graphics.setDepthTest(DepthTest.None);

			graphics.setVertexBuffer(quad);

			graphics.setTexture(deferredShader.getUniform("s_gbuffer0", UniformType.Sampler), 0, gbuffer.getAttachmentTexture(0));
			graphics.setTexture(deferredShader.getUniform("s_gbuffer1", UniformType.Sampler), 1, gbuffer.getAttachmentTexture(1));
			graphics.setTexture(deferredShader.getUniform("s_gbuffer2", UniformType.Sampler), 2, gbuffer.getAttachmentTexture(2));
			graphics.setTexture(deferredShader.getUniform("s_gbuffer3", UniformType.Sampler), 3, gbuffer.getAttachmentTexture(3));

			graphics.setTexture(deferredShader.getUniform("s_ambientOcclusion", UniformType.Sampler), 4, ssaoBlurRenderTarget.getAttachmentTexture(0));

			graphics.setUniform(deferredShader, "u_cameraPosition", cameraPosition.x, cameraPosition.y, cameraPosition.z, 0.0f);

			if (environmentMap != null)
				graphics.setTexture(deferredShader.getUniform("s_environmentMap", UniformType.Sampler), 5, environmentMap);

			if (directionalLights.Count > 0)
			{
				graphics.setUniform(deferredShader.getUniform("u_directionalLightDirection", UniformType.Vector4), new Vector4(directionalLights[0].light.direction, 0.0f));
				graphics.setUniform(deferredShader.getUniform("u_directionalLightColor", UniformType.Vector4), new Vector4(directionalLights[0].light.color, 0.0f));

				graphics.setUniform(deferredShader.getUniform("u_directionalLightCascadeFarPlanes", UniformType.Vector4), new Vector4(ShadowMap.FAR_PLANES[0], ShadowMap.FAR_PLANES[1], ShadowMap.FAR_PLANES[2], 0.0f));

				ShadowMap shadowMap = directionalLights[0].light.shadowMap;
				for (int j = 0; j < shadowMap.renderTargets.Length; j++)
				{
					RenderTarget renderTarget = shadowMap.renderTargets[j];
					Matrix toLightSpace = shadowMap.cascadeProjections[j] * shadowMap.cascadeViews[j];
					graphics.setTexture(deferredShader.getUniform("s_directionalLightShadowMap" + j, UniformType.Sampler), 6 + j, renderTarget.getAttachmentTexture(0));
					graphics.setUniform(deferredShader.getUniform("u_directionalLightToLightSpace" + j, UniformType.Matrix4), toLightSpace);
				}
			}
			else
			{
				graphics.setUniform(deferredShader.getUniform("u_directionalLightDirection", UniformType.Vector4), new Vector4(0.0f));
				graphics.setUniform(deferredShader.getUniform("u_directionalLightColor", UniformType.Vector4), new Vector4(0.0f));

				graphics.setUniform(deferredShader.getUniform("u_directionalLightCascadeFarPlanes", UniformType.Vector4), new Vector4(ShadowMap.FAR_PLANES[0], ShadowMap.FAR_PLANES[1], ShadowMap.FAR_PLANES[2], 0.0f));

				for (int j = 0; j < ShadowMap.NUM_SHADOW_CASCADES; j++)
				{
					graphics.setTexture(deferredShader.getUniform("s_directionalLightShadowMap" + j, UniformType.Sampler), 6 + j, dummyShadowMap);
					graphics.setUniform(deferredShader.getUniform("u_directionalLightToLightSpace" + j, UniformType.Matrix4), Matrix.Identity);
				}
			}


			for (int j = 0; j < MAX_LIGHTS_PER_PASS; j++)
			{
				int lightID = i * MAX_LIGHTS_PER_PASS + j;
				lightPositionBuffer[j] = lightID < lights.Count ? new Vector4(lights[lightID].position, 0.0f) : new Vector4(0.0f);
				lightColorBuffer[j] = lightID < lights.Count ? new Vector4(lights[lightID].color, 0.0f) : new Vector4(0.0f);
			}
			graphics.setUniform(deferredShader.getUniform("u_lightPosition", UniformType.Vector4, MAX_LIGHTS_PER_PASS), lightPositionBuffer, MAX_LIGHTS_PER_PASS);
			graphics.setUniform(deferredShader.getUniform("u_lightColor", UniformType.Vector4, MAX_LIGHTS_PER_PASS), lightColorBuffer, MAX_LIGHTS_PER_PASS);

			graphics.draw(deferredShader);
		}
	}

	static void RenderSky()
	{
		graphics.resetState();

		graphics.setBlendState(BlendState.Alpha);
		//graphics.setDepthTest(DepthTest.);

		graphics.setViewTransform(projection, view);

		for (int i = 0; i < skies.Count; i++)
		{
			graphics.setVertexBuffer(skydome);
			graphics.setIndexBuffer(skydomeIdx);

			graphics.setTransform(skies[i].transform);

			graphics.setTexture(skyShader.getUniform("s_skyTexture", UniformType.Sampler), 0, skies[i].cubemap);

			graphics.draw(skyShader);
		}
	}

	static void RenderParticles()
	{
		Matrix cameraTransform = view.inverted;
		Vector3 cameraPosition = cameraTransform.translation;
		Vector3 cameraAxis = cameraTransform.rotation.forward;

		Comparison<ParticleDrawCommand> particleDepthComparator = (particle1, particle2) =>
		{
			if (particle1.position == particle2.position)
				return 0;

			Vector3 toParticle1 = particle1.position - cameraPosition;
			Vector3 projected1 = Vector3.Dot(toParticle1, cameraAxis) * cameraAxis;

			Vector3 toParticle2 = particle2.position - cameraPosition;
			Vector3 projected2 = Vector3.Dot(toParticle2, cameraAxis) * cameraAxis;

			float d1 = projected1.lengthSquared;
			float d2 = projected2.lengthSquared;

			return d1 < d2 ? 1 : d1 > d2 ? -1 : 0;
		};


		graphics.resetState();

		graphics.setBlendState(BlendState.Alpha);

		graphics.setViewTransform(projection, view);

		Vector4[] pointLightPositions = new Vector4[16];
		Vector4[] pointLightColors = new Vector4[16];
		for (int i = 0; i < lights.Count; i++)
		{
			pointLightPositions[i] = new Vector4(lights[i].position, 1.0f);
			pointLightColors[i] = new Vector4(lights[i].color, 1.0f);
		}
		graphics.setUniform(particleShader.getUniform("u_pointLight_position", UniformType.Vector4, 16), pointLightPositions, lights.Count);
		graphics.setUniform(particleShader.getUniform("u_pointLight_color", UniformType.Vector4, 16), pointLightColors, lights.Count);
		graphics.setUniform(particleShader.getUniform("u_lightInfo", UniformType.Vector4), new Vector4(lights.Count + 0.5f, 0.0f, 0.0f, 0.0f));

		particles.Sort(particleDepthComparator);

		particleBatch.begin(particles.Count);
		for (int i = 0; i < particles.Count; i++)
		{
			ParticleDrawCommand particle = particles[i];
			float u0 = particle.atlas != null ? particle.u0 / (float)particle.atlas.info.width : 0.0f;
			float v0 = particle.atlas != null ? particle.v0 / (float)particle.atlas.info.height : 0.0f;
			float u1 = particle.atlas != null ? particle.u1 / (float)particle.atlas.info.width : 0.0f;
			float v1 = particle.atlas != null ? particle.v1 / (float)particle.atlas.info.height : 0.0f;
			particleBatch.draw(particle.position.x, particle.position.y, particle.position.z,
				particle.size, particle.size,
				particle.rotation,
				true,
				particle.atlas,
				u0, v0, u1, v1,
				particle.color);
		}
		particleBatch.end(particleShader);


		graphics.resetState();

		graphics.setBlendState(BlendState.Additive);

		graphics.setViewTransform(projection, view);

		particlesAdditive.Sort(particleDepthComparator);

		particleBatch.begin(particlesAdditive.Count);
		for (int i = 0; i < particlesAdditive.Count; i++)
		{
			ParticleDrawCommand particle = particlesAdditive[i];
			float u0 = particle.atlas != null ? particle.u0 / (float)particle.atlas.info.width : 0.0f;
			float v0 = particle.atlas != null ? particle.v0 / (float)particle.atlas.info.height : 0.0f;
			float u1 = particle.atlas != null ? particle.u1 / (float)particle.atlas.info.width : 0.0f;
			float v1 = particle.atlas != null ? particle.v1 / (float)particle.atlas.info.height : 0.0f;
			particleBatch.draw(particle.position.x, particle.position.y, particle.position.z,
				particle.size, particle.size,
				particle.rotation,
				true,
				particle.atlas,
				u0, v0, u1, v1,
				particle.color);
		}
		particleBatch.end(particleAdditiveShader);
	}

	static void ForwardPass()
	{
		graphics.resetState();
		graphics.setPass((int)RenderPass.Forward);

		graphics.setRenderTarget(composite);


		RenderSky();
		RenderParticles();
		RenderGrass();
	}

	static void BloomDownsample(int idx, Texture texture, RenderTarget target)
	{
		graphics.resetState();
		graphics.setPass((int)RenderPass.BloomDownsample + idx);

		graphics.setRenderTarget(target);

		graphics.setTexture(bloomDownsampleShader.getUniform("s_input", UniformType.Sampler), 0, texture);

		graphics.setVertexBuffer(quad);
		graphics.draw(bloomDownsampleShader);
	}

	static void BloomUpsample(int idx, Texture texture0, Texture texture1, RenderTarget target)
	{
		graphics.resetState();
		graphics.setPass((int)RenderPass.BloomUpsample + idx);

		graphics.setRenderTarget(target);

		graphics.setTexture(bloomUpsampleShader.getUniform("s_input0", UniformType.Sampler), 0, texture0);
		graphics.setTexture(bloomUpsampleShader.getUniform("s_input1", UniformType.Sampler), 1, texture1);

		graphics.setVertexBuffer(quad);
		graphics.draw(bloomUpsampleShader);
	}

	static void PostProcessing()
	{
		// Bloom

		BloomDownsample(0, composite.getAttachmentTexture(0), bloomDownsampleChain[0]);
		BloomDownsample(1, bloomDownsampleChain[0].getAttachmentTexture(0), bloomDownsampleChain[1]);
		BloomDownsample(2, bloomDownsampleChain[1].getAttachmentTexture(0), bloomDownsampleChain[2]);
		BloomDownsample(3, bloomDownsampleChain[2].getAttachmentTexture(0), bloomDownsampleChain[3]);
		BloomDownsample(4, bloomDownsampleChain[3].getAttachmentTexture(0), bloomDownsampleChain[4]);
		BloomDownsample(5, bloomDownsampleChain[4].getAttachmentTexture(0), bloomDownsampleChain[5]);

		BloomUpsample(0, bloomDownsampleChain[5].getAttachmentTexture(0), bloomDownsampleChain[4].getAttachmentTexture(0), bloomUpsampleChain[4]);
		BloomUpsample(1, bloomUpsampleChain[4].getAttachmentTexture(0), bloomDownsampleChain[3].getAttachmentTexture(0), bloomUpsampleChain[3]);
		BloomUpsample(2, bloomUpsampleChain[3].getAttachmentTexture(0), bloomDownsampleChain[2].getAttachmentTexture(0), bloomUpsampleChain[2]);
		BloomUpsample(3, bloomUpsampleChain[2].getAttachmentTexture(0), bloomDownsampleChain[1].getAttachmentTexture(0), bloomUpsampleChain[1]);
		BloomUpsample(4, bloomUpsampleChain[1].getAttachmentTexture(0), bloomDownsampleChain[0].getAttachmentTexture(0), bloomUpsampleChain[0]);
	}

	static void TonemappingPass()
	{
		graphics.resetState();
		graphics.setPass((int)RenderPass.Tonemapping);

		graphics.setRenderTarget(null);

		graphics.setDepthTest(DepthTest.None);

		graphics.setVertexBuffer(quad);
		graphics.setTexture(tonemappingShader.getUniform("s_hdrBuffer", UniformType.Sampler), 0, composite.getAttachmentTexture(0));
		graphics.setTexture(tonemappingShader.getUniform("s_bloom", UniformType.Sampler), 1, bloomUpsampleChain[0].getAttachmentTexture(0));

		graphics.draw(tonemappingShader);
	}

	static void UIPass()
	{
		graphics.resetState();
		graphics.setPass((int)RenderPass.UI);

		graphics.setRenderTarget(null);

		graphics.setDepthTest(DepthTest.None);
		graphics.setBlendState(BlendState.Alpha);

		graphics.setViewTransform(Matrix.CreateOrthographic(0, Display.viewportSize.x, 0, Display.viewportSize.y, 1.0f, -1.0f), Matrix.Identity);


		uiTextureBatch.begin(uiTextures.Count);

		for (int i = 0; i < uiTextures.Count; i++)
		{
			float u0 = uiTextures[i].texture != null ? uiTextures[i].u0 / (float)uiTextures[i].texture.info.width : 0.0f;
			float v0 = uiTextures[i].texture != null ? uiTextures[i].v0 / (float)uiTextures[i].texture.info.height : 0.0f;
			float u1 = uiTextures[i].texture != null ? uiTextures[i].u1 / (float)uiTextures[i].texture.info.width : 0.0f;
			float v1 = uiTextures[i].texture != null ? uiTextures[i].v1 / (float)uiTextures[i].texture.info.height : 0.0f;

			uiTextureBatch.draw(
				uiTextures[i].x, uiTextures[i].y, 0.0f,
				uiTextures[i].width, uiTextures[i].height,
				0.0f,
				false,
				uiTextures[i].texture,
				u0, v0, u1, v1,
				uiTextures[i].color
				);
		}

		uiTextureBatch.end(uiTextureShader);


		int numCharacters = 0;
		foreach (TextDrawCommand text in texts)
			numCharacters += text.length;

		textBatch.begin(numCharacters);

		graphics.setDepthTest(DepthTest.None);

		for (int i = 0; i < texts.Count; i++)
		{
			int x = texts[i].x;
			int y = texts[i].y;
			float scale = texts[i].scale;

			string text = texts[i].text;
			int length = texts[i].length;

			Font font = texts[i].font;
			uint color = texts[i].color;

			graphics.drawText(x, y, scale, text, length, font, color, textBatch);
		}

		textBatch.end(textShader);
	}

	public static void End()
	{
		GeometryPass();
		ShadowPass();
		AmbientOcclusionPass();
		DeferredPass();

		graphics.blit(composite.getAttachmentTexture(composite.attachmentCount - 1), gbuffer.getAttachmentTexture(gbuffer.attachmentCount - 1));

		ForwardPass();
		PostProcessing();
		TonemappingPass();
		UIPass();

		models.Clear();
		terrains.Clear();
		foliage.Clear();
		skies.Clear();
		lights.Clear();
		directionalLights.Clear();
		particles.Clear();
		particlesAdditive.Clear();
		grassPatches.Clear();
		uiTextures.Clear();
		texts.Clear();
	}
}
