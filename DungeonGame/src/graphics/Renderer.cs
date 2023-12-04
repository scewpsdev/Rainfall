using Rainfall;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;


internal static class Renderer
{
	const int BLOOM_CHAIN_LENGTH = 6;
	const int MAX_REFLECTION_PROBES = 4;


	enum RenderPass : int
	{
		Geometry,
		Shadow0,
		Shadow1,
		Shadow2,
		ReflectionProbe,
		AmbientOcclusion = ReflectionProbe + MAX_REFLECTION_PROBES * 6,
		AmbientOcclusionBlur,
		Deferred,
		Forward,
		BloomDownsample,
		BloomUpsample = BloomDownsample + BLOOM_CHAIN_LENGTH,
		Composite = BloomUpsample + BLOOM_CHAIN_LENGTH - 1,
		Tonemapping,
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
		internal float intensity;
		internal Matrix transform;
	}

	struct WaterDrawCommand
	{
		internal Model model;
		internal Vector3 position;
		internal float size;
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

	struct ReflectionProbeDrawCommand
	{
		internal ReflectionProbe reflectionProbe;
		internal Vector3 position;
		internal Vector3 size;
		internal Vector3 origin;
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

	public static GraphicsDevice graphics { get; private set; }

	static RenderTarget gbuffer;
	static RenderTarget forward;
	static VertexBuffer quad;
	static VertexBuffer skydome;
	static IndexBuffer skydomeIdx;

	static Texture emptyShadowTexture;
	static Cubemap emptyCubemap;

	static RenderTarget ssaoRenderTarget;
	//static Vector4[] ssaoKernel;
	static Texture ssaoNoiseTexture;
	static RenderTarget ssaoBlurRenderTarget;

	static RenderTarget[] bloomDownsampleChain;
	static RenderTarget[] bloomUpsampleChain;

	static RenderTarget compositeRenderTarget;

	static Shader modelShader;
	static Shader modelDepthShader;
	static Shader modelSimpleShader;
	static Shader modelAnimShader;
	static Shader modelAnimDepthShader;
	static Shader terrainShader;
	static Shader foliageShader;
	static Shader ssaoShader;
	static Shader ssaoBlurShader;
	static Shader deferredShader;
	static Shader skyShader;
	static Shader waterShader;
	static Shader particleShader;
	static Shader particleAdditiveShader;
	static Shader grassShader;
	static Shader bloomDownsampleShader;
	static Shader bloomUpsampleShader;
	static Shader compositeShader;
	static Shader tonemappingShader;
	static Shader uiTextureShader;
	static Shader textShader;

	static SpriteBatch particleBatch;
	static SpriteBatch uiTextureBatch;
	static SpriteBatch textBatch;

	public static Camera camera;
	public static Matrix projection, view, pv;

	static Cubemap environmentMap;
	static float environmentMapIntensity = 1.0f;

	static Matrix[] cubemapFaceRotations = new Matrix[6];

	public static Vector3 fogColor = new Vector3(1.0f);
	public static float fogIntensity = 0.0f;

	public static Vector3 vignetteColor = new Vector3(0.0f);
	public static float vignetteFalloff = 0.0f; // default value: 0.37f

	static VertexBuffer grassBlade;
	static IndexBuffer grassIndices;
	static VertexBuffer waterTileVertexBuffer;
	static IndexBuffer waterTileIndexBuffer;
	static Texture perlinTexture;

	static List<ModelDrawCommand> models = new List<ModelDrawCommand>();
	static List<TerrainDrawCommand> terrains = new List<TerrainDrawCommand>();
	static List<LeaveDrawCommand> foliage = new List<LeaveDrawCommand>();
	static List<SkyDrawCommand> skies = new List<SkyDrawCommand>();
	static List<WaterDrawCommand> waterTiles = new List<WaterDrawCommand>();
	static List<LightDrawCommand> lights = new List<LightDrawCommand>();
	static List<DirectionalLightDrawCommand> directionalLights = new List<DirectionalLightDrawCommand>();
	static List<ReflectionProbeDrawCommand> reflectionProbes = new List<ReflectionProbeDrawCommand>();
	static List<ParticleDrawCommand> particles = new List<ParticleDrawCommand>();
	static List<ParticleDrawCommand> particlesAdditive = new List<ParticleDrawCommand>();
	static List<GrassDrawCommand> grassPatches = new List<GrassDrawCommand>();
	static List<UITextureDrawCommand> uiTextures = new List<UITextureDrawCommand>();
	static List<TextDrawCommand> texts = new List<TextDrawCommand>();

	public static int meshRenderCounter = 0;
	public static int meshCulledCounter = 0;


	public static void Init(GraphicsDevice graphics)
	{
		Renderer.graphics = graphics;

		gbuffer = graphics.createRenderTarget(new RenderTargetAttachment[] {
			new RenderTargetAttachment(BackbufferRatio.Equal, TextureFormat.RGBA32F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp),
			new RenderTargetAttachment(BackbufferRatio.Equal, TextureFormat.RGBA16F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp),
			new RenderTargetAttachment(BackbufferRatio.Equal, TextureFormat.RGBA8, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp),
			new RenderTargetAttachment(BackbufferRatio.Equal, TextureFormat.RGBA8, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp),
			new RenderTargetAttachment(BackbufferRatio.Equal, TextureFormat.D16F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp, true)
		});

		forward = graphics.createRenderTarget(new RenderTargetAttachment[]
		{
			new RenderTargetAttachment(BackbufferRatio.Equal, TextureFormat.RG11B10F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp),
			new RenderTargetAttachment(BackbufferRatio.Equal, TextureFormat.D16F, (ulong)TextureFlags.RenderTarget | (ulong)TextureFlags.RenderTargetWriteOnly | (ulong)TextureFlags.BlitDst | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp),
		});

		quad = graphics.createVertexBuffer(
			graphics.createVideoMemory(new float[] { -3.0f, -1.0f, 1.0f, 1.0f, -1.0f, 1.0f, 1.0f, 3.0f, 1.0f }),
			stackalloc VertexElement[] { new VertexElement(VertexAttribute.Position, VertexAttributeType.Vector3, false) }
		);

		skydome = graphics.createVertexBuffer(
			graphics.createVideoMemory(new float[] { -100.0f, -100.0f, 100.0f, 100.0f, -100.0f, 100.0f, 0.0f, -100.0f, -100.0f, 0.0f, 100.0f, 0.0f }),
			stackalloc VertexElement[] { new VertexElement(VertexAttribute.Position, VertexAttributeType.Vector3, false) }
		);

		skydomeIdx = graphics.createIndexBuffer(graphics.createVideoMemory(new short[] { 0, 1, 2, 2, 1, 3, 1, 0, 3, 0, 2, 3 }));

		emptyShadowTexture = graphics.createTexture(1, 1, TextureFormat.D16F, graphics.createVideoMemory(new Half[] { (Half)1.0f }), (uint)SamplerFlags.CompareLEqual);
		emptyCubemap = graphics.createCubemap(1, TextureFormat.RG11B10F);

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

		const int waterTileSubsections = 1024;
		const int waterTileVertexCount = waterTileSubsections + 1;
		Vector3[] waterTileVertices = new Vector3[waterTileVertexCount * waterTileVertexCount];
		int[] waterTileIndices = new int[waterTileSubsections * waterTileSubsections * 6];
		for (int z = 0; z < waterTileVertexCount; z++)
		{
			for (int x = 0; x < waterTileVertexCount; x++)
			{
				int i = x + z * waterTileVertexCount;

				Vector3 vertex;
				vertex.x = x / (float)(waterTileVertexCount - 1);
				vertex.y = 0.0f;
				vertex.z = z / (float)(waterTileVertexCount - 1);

				waterTileVertices[i] = vertex;
			}
		}
		for (int z = 0; z < waterTileSubsections; z++)
		{
			for (int x = 0; x < waterTileSubsections; x++)
			{
				int i = x + z * waterTileSubsections;

				int i00 = x + z * waterTileVertexCount;
				int i01 = x + (z + 1) * waterTileVertexCount;
				int i11 = x + 1 + (z + 1) * waterTileVertexCount;
				int i10 = x + 1 + z * waterTileVertexCount;

				waterTileIndices[i * 6 + 0] = i00;
				waterTileIndices[i * 6 + 1] = i01;
				waterTileIndices[i * 6 + 2] = i11;
				waterTileIndices[i * 6 + 3] = i11;
				waterTileIndices[i * 6 + 4] = i10;
				waterTileIndices[i * 6 + 5] = i00;
			}
		}
		waterTileVertexBuffer = graphics.createVertexBuffer(
			graphics.createVideoMemory(waterTileVertices),
			new VertexLayout(new VertexElement[]
			{
				new VertexElement(VertexAttribute.Position, VertexAttributeType.Vector3, false)
			})
		);
		waterTileIndexBuffer = graphics.createIndexBuffer(graphics.createVideoMemory(waterTileIndices), BufferFlags.Index32);


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

		compositeRenderTarget = graphics.createRenderTarget(new RenderTargetAttachment[]
		{
			new RenderTargetAttachment(BackbufferRatio.Equal, TextureFormat.RG11B10F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp)
		});

		perlinTexture = Resource.GetTexture("res/texture/perlin.png");

		cubemapFaceRotations[0] = Matrix.CreateScale(-1.0f, 1.0f, 1.0f) * Matrix.CreateRotation(Vector3.Up, MathF.PI * 0.5f);
		cubemapFaceRotations[1] = Matrix.CreateScale(-1.0f, 1.0f, 1.0f) * Matrix.CreateRotation(Vector3.Up, -MathF.PI * 0.5f);
		cubemapFaceRotations[2] = Matrix.CreateScale(-1.0f, 1.0f, 1.0f) * Matrix.CreateRotation(Vector3.UnitZ, MathF.PI) * Matrix.CreateRotation(Vector3.Right, -MathF.PI * 0.5f);
		cubemapFaceRotations[3] = Matrix.CreateScale(-1.0f, 1.0f, 1.0f) * Matrix.CreateRotation(Vector3.UnitZ, MathF.PI) * Matrix.CreateRotation(Vector3.Right, MathF.PI * 0.5f);
		cubemapFaceRotations[4] = Matrix.CreateScale(-1.0f, 1.0f, 1.0f) * Matrix.CreateRotation(Vector3.Up, -MathF.PI);
		cubemapFaceRotations[5] = Matrix.CreateScale(-1.0f, 1.0f, 1.0f) * Matrix.Identity;


		modelShader = Resource.GetShader("res/shaders/model/model.vs.shader", "res/shaders/model/model.fs.shader");
		modelDepthShader = Resource.GetShader("res/shaders/model/model_depth.vs.shader", "res/shaders/model/model_depth.fs.shader");
		modelSimpleShader = Resource.GetShader("res/shaders/model/model_simple.vs.shader", "res/shaders/model/model_simple.fs.shader");
		modelAnimShader = Resource.GetShader("res/shaders/model_anim/model_anim.vs.shader", "res/shaders/model_anim/model_anim.fs.shader");
		modelAnimDepthShader = Resource.GetShader("res/shaders/model_anim/model_anim_depth.vs.shader", "res/shaders/model_anim/model_anim_depth.fs.shader");
		terrainShader = Resource.GetShader("res/shaders/terrain/terrain.vs.shader", "res/shaders/terrain/terrain.fs.shader");
		foliageShader = Resource.GetShader("res/shaders/foliage/foliage.vs.shader", "res/shaders/foliage/foliage.fs.shader");
		ssaoShader = Resource.GetShader("res/shaders/ssao/ssao.vs.shader", "res/shaders/ssao/ssao.fs.shader");
		ssaoBlurShader = Resource.GetShader("res/shaders/ssao/ssao_blur.vs.shader", "res/shaders/ssao/ssao_blur.fs.shader");
		deferredShader = Resource.GetShader("res/shaders/deferred/deferred.vs.shader", "res/shaders/deferred/deferred.fs.shader");
		skyShader = Resource.GetShader("res/shaders/sky/sky.vs.shader", "res/shaders/sky/sky.fs.shader");
		waterShader = Resource.GetShader("res/shaders/water/water.vs.shader", "res/shaders/water/water.fs.shader");
		particleShader = Resource.GetShader("res/shaders/particle/particle.vs.shader", "res/shaders/particle/particle.fs.shader");
		particleAdditiveShader = Resource.GetShader("res/shaders/particle_additive/particle_additive.vs.shader", "res/shaders/particle_additive/particle_additive.fs.shader");
		grassShader = Resource.GetShader("res/shaders/grass/grass.vs.shader", "res/shaders/grass/grass.fs.shader");
		bloomDownsampleShader = Resource.GetShader("res/shaders/bloom/bloom.vs.shader", "res/shaders/bloom/bloom_downsample.fs.shader");
		bloomUpsampleShader = Resource.GetShader("res/shaders/bloom/bloom.vs.shader", "res/shaders/bloom/bloom_upsample.fs.shader");
		compositeShader = Resource.GetShader("res/shaders/composite/composite.vs.shader", "res/shaders/composite/composite.fs.shader");
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

	public static void DrawReflectionProbe(ReflectionProbe reflectionProbe)
	{
		reflectionProbes.Add(new ReflectionProbeDrawCommand { reflectionProbe = reflectionProbe, position = reflectionProbe.position, size = reflectionProbe.size, origin = reflectionProbe.origin });
	}

	public static void DrawSky(Cubemap cubemap, float intensity, Matrix transform)
	{
		skies.Add(new SkyDrawCommand { cubemap = cubemap, intensity = intensity, transform = transform });
	}

	public static void DrawWater(Vector3 position, float size)
	{
		waterTiles.Add(new WaterDrawCommand { position = position, size = size, model = null });
	}

	public static void DrawWater(Vector3 position, Model model)
	{
		waterTiles.Add(new WaterDrawCommand { position = position, size = 1.0f, model = model });
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

	public static void SetEnvironmentMap(Cubemap environmentMap, float intensity)
	{
		Renderer.environmentMap = environmentMap;
		Renderer.environmentMapIntensity = intensity;
	}

	public static void Begin()
	{
	}

	public static void SetCamera(Camera camera)
	{
		Renderer.camera = camera;

		Renderer.projection = camera.getProjectionMatrix();
		Renderer.view = camera.getViewMatrix();
		Renderer.pv = projection * view;
	}

	static void GetFrustumPlanes(Matrix matrix, Vector4[] planes)
	{
		// Left clipping plane
		planes[0].x = matrix.m03 + matrix.m00;
		planes[0].y = matrix.m13 + matrix.m10;
		planes[0].z = matrix.m23 + matrix.m20;
		planes[0].w = matrix.m33 + matrix.m30;
		// Right clipping plane
		planes[1].x = matrix.m03 - matrix.m00;
		planes[1].y = matrix.m13 - matrix.m10;
		planes[1].z = matrix.m23 - matrix.m20;
		planes[1].w = matrix.m33 - matrix.m30;
		// Bottom clipping plane
		planes[2].x = matrix.m03 + matrix.m01;
		planes[2].y = matrix.m13 + matrix.m11;
		planes[2].z = matrix.m23 + matrix.m21;
		planes[2].w = matrix.m33 + matrix.m31;
		// Top clipping plane
		planes[3].x = matrix.m03 - matrix.m01;
		planes[3].y = matrix.m13 - matrix.m11;
		planes[3].z = matrix.m23 - matrix.m21;
		planes[3].w = matrix.m33 - matrix.m31;
		// Near clipping plane
		planes[4].x = matrix.m03 + matrix.m02;
		planes[4].y = matrix.m13 + matrix.m12;
		planes[4].z = matrix.m23 + matrix.m22;
		planes[4].w = matrix.m33 + matrix.m32;
		// Far clipping plane
		planes[5].x = matrix.m03 - matrix.m02;
		planes[5].y = matrix.m13 - matrix.m12;
		planes[5].z = matrix.m23 - matrix.m22;
		planes[5].w = matrix.m33 - matrix.m32;

		for (int i = 0; i < 6; i++)
		{
			float l = 1.0f / planes[i].xyz.length;
			planes[i] *= l;
		}
	}

	static bool IntersectsFrustum(Vector3 p, float radius, Matrix transform, Matrix pv)
	{
		Vector4[] planes = new Vector4[6];
		GetFrustumPlanes(pv, planes);

		Vector3 boundingSpherePos = (transform * new Vector4(p, 1.0f)).xyz;
		float boundingSphereRadius = transform.column0.xyz.length * radius;

		for (int i = 0; i < 6; i++)
		{
			float distance = (Vector3.Dot(boundingSpherePos, planes[i].xyz) + planes[i].w) / planes[i].xyz.length;
			if (distance + boundingSphereRadius < 0.0f)
				return false;
		}
		return true;
	}

	static Matrix GetNodeTransform(Node node)
	{
		Matrix transform = node.transform;
		Node parent = node.parent;
		while (parent != null)
		{
			transform = parent.transform * transform;
			parent = parent.parent;
		}
		return transform;
	}

	static void SubmitMesh(Model model, int meshID, Animator animator, Shader shader, Shader animShader, Matrix transform, Matrix pv)
	{
		MeshData meshData = model.getMeshData(meshID).Value;
		Matrix meshTransform = transform * GetNodeTransform(model.skeleton.getNode(meshData.nodeID));
		bool isAnimated = meshData.hasSkeleton && animator != null && animShader != null;

		if (IntersectsFrustum(new Vector3(meshData.boundingSphere.xcenter, meshData.boundingSphere.ycenter, meshData.boundingSphere.zcenter), meshData.boundingSphere.radius * (isAnimated ? 3.0f : 1.0f), meshTransform, pv))
		{
			if (isAnimated)
				graphics.drawSubModelAnimated(model, meshID, animShader, animator, meshTransform);
			else
				graphics.drawSubModel(model, meshID, shader, meshTransform);

			meshRenderCounter++;
		}
		else
		{
			// reset state so we can submit new draw call data
			graphics.resetState();

			meshCulledCounter++;
		}
	}

	static void RenderModels()
	{
		graphics.resetState();

		graphics.setViewTransform(projection, view);

		for (int i = 0; i < models.Count; i++)
		{
			Model model = models[i].model;
			int meshID = models[i].meshID;
			Animator animator = models[i].animator;
			Matrix transform = models[i].transform;

			if (meshID != -1)
			{
				graphics.setCullState(CullState.ClockWise);

				SubmitMesh(model, meshID, animator, modelShader, modelAnimShader, transform, pv);
			}
			else
			{
				for (int j = 0; j < model.meshCount; j++)
				{
					graphics.setCullState(CullState.ClockWise);

					SubmitMesh(model, j, animator, modelShader, modelAnimShader, transform, pv);
				}
			}
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
			graphics.drawSubModel(model, meshID, foliageShader, transform);
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

				Matrix cascadePV = shadowMap.cascadeProjections[i] * shadowMap.cascadeViews[i];
				graphics.setViewTransform(shadowMap.cascadeProjections[i], shadowMap.cascadeViews[i]);

				for (int j = 0; j < models.Count; j++)
				{
					Model model = models[j].model;
					Animator animator = models[j].animator;
					Matrix transform = models[j].transform;
					//graphics.drawModel(model, modelDepthShader, modelAnimDepthShader, animator, transform);

					for (int k = 0; k < models[j].model.meshCount; k++)
					{
						graphics.setCullState(CullState.None);

						SubmitMesh(model, k, animator, modelDepthShader, modelAnimDepthShader, transform, cascadePV);
					}
				}


				// TODO frustum culling
				for (int j = 0; j < foliage.Count; j++)
				{
					Model model = foliage[j].model;
					int meshID = foliage[j].meshID;
					Matrix transform = foliage[j].transform;
					// TODO use depth shader

					graphics.setCullState(CullState.None);

					graphics.setUniform(foliageShader.getUniform("u_animationData", UniformType.Vector4), new Vector4(Time.currentTime / 1e9f, 0.0f, 0.0f, 0.0f));

					graphics.drawSubModel(model, meshID, foliageShader, transform);
				}
			}
		}
	}

	static void ReflectionProbePass()
	{
		for (int i = 0; i < reflectionProbes.Count; i++)
		{
			if (!reflectionProbes[i].reflectionProbe.needsUpdate)
				continue;
			reflectionProbes[i].reflectionProbe.needsUpdate = false;
			for (int j = 0; j < 6; j++)
			{
				graphics.resetState();
				graphics.setPass((int)RenderPass.ReflectionProbe + i * 6 + j);

				graphics.setRenderTarget(reflectionProbes[i].reflectionProbe.renderTargets[j], 0xff00ffff);

				graphics.setUniform(modelSimpleShader, "u_cameraPosition", new Vector4(reflectionProbes[i].origin, 0.0f));

				Matrix reflectionProbeProjection = Matrix.CreatePerspective(MathF.PI * 0.5f, 1.0f, 0.1f, 1000.0f);
				Matrix reflectionProbeView = cubemapFaceRotations[j] * Matrix.CreateTranslation(-reflectionProbes[i].origin);
				Matrix reflectionProbePV = reflectionProbeProjection * reflectionProbeView;
				graphics.setViewTransform(reflectionProbeProjection, reflectionProbeView);

				// TODO optimize meshes in resource compiler

				// TODO frustum culling
				for (int k = 0; k < models.Count; k++)
				{
					for (int l = 0; l < models[k].model.meshCount; l++)
					{
						graphics.setCullState(CullState.CounterClockWise);

						if (directionalLights.Count > 0)
						{
							graphics.setUniform(modelSimpleShader.getUniform("u_directionalLightDirection", UniformType.Vector4), new Vector4(directionalLights[0].light.direction, 0.0f));
							graphics.setUniform(modelSimpleShader.getUniform("u_directionalLightColor", UniformType.Vector4), new Vector4(directionalLights[0].light.color, 0.0f));

							graphics.setUniform(modelSimpleShader.getUniform("u_directionalLightFarPlane", UniformType.Vector4), new Vector4(ShadowMap.FAR_PLANES[2], 0.0f, 0.0f, 0.0f));

							ShadowMap shadowMap = directionalLights[0].light.shadowMap;
							int lastCascade = shadowMap.renderTargets.Length - 1;
							RenderTarget renderTarget = shadowMap.renderTargets[lastCascade];
							Matrix toLightSpace = shadowMap.cascadeProjections[lastCascade] * shadowMap.cascadeViews[lastCascade];
							graphics.setTexture(modelSimpleShader.getUniform("s_directionalLightShadowMap", UniformType.Sampler), 5, renderTarget.getAttachmentTexture(0));
							graphics.setUniform(modelSimpleShader.getUniform("u_directionalLightToLightSpace", UniformType.Matrix4), toLightSpace);
						}
						else
						{
							graphics.setUniform(modelSimpleShader.getUniform("u_directionalLightDirection", UniformType.Vector4), new Vector4(0.0f));
							graphics.setUniform(modelSimpleShader.getUniform("u_directionalLightColor", UniformType.Vector4), new Vector4(0.0f));

							graphics.setUniform(modelSimpleShader.getUniform("u_directionalLightFarPlane", UniformType.Vector4), new Vector4(ShadowMap.FAR_PLANES[2], 0.0f, 0.0f, 0.0f));

							graphics.setTexture(modelSimpleShader.getUniform("s_directionalLightShadowMap", UniformType.Sampler), 5, emptyShadowTexture);
							graphics.setUniform(modelSimpleShader.getUniform("u_directionalLightToLightSpace", UniformType.Matrix4), Matrix.Identity);
						}


						// TODO reuse these
						Vector4[] lightPositionBuffer = new Vector4[MAX_LIGHTS_PER_PASS];
						Vector4[] lightColorBuffer = new Vector4[MAX_LIGHTS_PER_PASS];
						for (int m = 0; m < MAX_LIGHTS_PER_PASS; m++)
						{
							int lightID = m;
							lightPositionBuffer[j] = lightID < lights.Count ? new Vector4(lights[lightID].position, 0.0f) : new Vector4(0.0f);
							lightColorBuffer[j] = lightID < lights.Count ? new Vector4(lights[lightID].color, 0.0f) : new Vector4(0.0f);
						}
						graphics.setUniform(modelSimpleShader.getUniform("u_lightPosition", UniformType.Vector4, MAX_LIGHTS_PER_PASS), lightPositionBuffer, MAX_LIGHTS_PER_PASS);
						graphics.setUniform(modelSimpleShader.getUniform("u_lightColor", UniformType.Vector4, MAX_LIGHTS_PER_PASS), lightColorBuffer, MAX_LIGHTS_PER_PASS);

						//graphics.drawSubModel(models[k].model, l, modelSimpleShader, models[k].transform);
						SubmitMesh(models[k].model, l, null, modelSimpleShader, null, models[k].transform, reflectionProbePV);
					}
				}

				for (int k = 0; k < skies.Count; k++)
				{
					graphics.setCullState(CullState.None);

					graphics.setVertexBuffer(skydome);
					graphics.setIndexBuffer(skydomeIdx);

					graphics.setTransform(skies[k].transform);

					Vector4 skyData = new Vector4(skies[k].intensity, 0.0f, 0.0f, 0.0f);
					graphics.setUniform(skyShader.getUniform("u_skyData", UniformType.Vector4), skyData);

					graphics.setTexture(skyShader.getUniform("s_skyTexture", UniformType.Sampler), 0, skies[k].cubemap);

					graphics.draw(skyShader);
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

		graphics.setRenderTarget(forward);

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

			graphics.setUniform(deferredShader, "u_cameraPosition", camera.position.x, camera.position.y, camera.position.z, 0.0f);

			if (environmentMap != null)
			{
				graphics.setTexture(deferredShader.getUniform("s_environmentMap", UniformType.Sampler), 5, environmentMap);

				Vector4 environmentMapIntensities = new Vector4(environmentMapIntensity, 0.0f, 0.0f, 0.0f);
				graphics.setUniform(deferredShader.getUniform("u_environmentMapIntensities", UniformType.Vector4), environmentMapIntensities);
			}

			reflectionProbes.Sort((ReflectionProbeDrawCommand cmd0, ReflectionProbeDrawCommand cmd1) =>
			{
				float distance0 = (cmd0.position - camera.position).length;
				float distance1 = (cmd1.position - camera.position).length;
				return distance0 < distance1 ? -1 : distance0 > distance1 ? 1 : 0;
			});

			for (int j = 0; j < MAX_REFLECTION_PROBES; j++)
			{
				if (j < reflectionProbes.Count)
				{
					graphics.setTexture(deferredShader.getUniform("s_reflectionProbe" + j, UniformType.Sampler), 6 + j, reflectionProbes[j].reflectionProbe.cubemap);
					graphics.setUniform(deferredShader.getUniform("u_reflectionProbePosition" + j, UniformType.Vector4), reflectionProbes[j].position);
					graphics.setUniform(deferredShader.getUniform("u_reflectionProbeSize" + j, UniformType.Vector4), reflectionProbes[j].size);
					graphics.setUniform(deferredShader.getUniform("u_reflectionProbeOrigin" + j, UniformType.Vector4), reflectionProbes[j].origin);
				}
				else
				{
					graphics.setTexture(deferredShader.getUniform("s_reflectionProbe" + j, UniformType.Sampler), 6 + j, emptyCubemap);
					graphics.setUniform(deferredShader.getUniform("u_reflectionProbePosition" + j, UniformType.Vector4), new Vector3(0.0f, -100000.0f, 0.0f));
					graphics.setUniform(deferredShader.getUniform("u_reflectionProbeSize" + j, UniformType.Vector4), Vector3.Zero);
					graphics.setUniform(deferredShader.getUniform("u_reflectionProbeOrigin" + j, UniformType.Vector4), Vector3.Zero);
				}
			}

			Vector4 fogData = new Vector4(fogColor, fogIntensity);
			graphics.setUniform(deferredShader.getUniform("u_fogData", UniformType.Vector4), fogData);

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
					graphics.setTexture(deferredShader.getUniform("s_directionalLightShadowMap" + j, UniformType.Sampler), 10 + j, renderTarget.getAttachmentTexture(0));
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
					graphics.setTexture(deferredShader.getUniform("s_directionalLightShadowMap" + j, UniformType.Sampler), 10 + j, emptyShadowTexture);
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

		graphics.setViewTransform(projection, view);

		for (int i = 0; i < skies.Count; i++)
		{
			graphics.setVertexBuffer(skydome);
			graphics.setIndexBuffer(skydomeIdx);

			graphics.setTransform(skies[i].transform);

			Vector4 skyData = new Vector4(skies[i].intensity, 0.0f, 0.0f, 0.0f);
			graphics.setUniform(skyShader.getUniform("u_skyData", UniformType.Vector4), skyData);

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
		graphics.setPass((int)RenderPass.Forward);

		graphics.setRenderTarget(forward);

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
		particleBatch.end();

		for (int i = 0; i < particleBatch.getNumDrawCalls(); i++)
		{
			graphics.setBlendState(BlendState.Alpha);

			graphics.setViewTransform(projection, view);

			Vector4[] pointLightPositions = new Vector4[16];
			Vector4[] pointLightColors = new Vector4[16];
			for (int j = 0; j < lights.Count; j++)
			{
				pointLightPositions[j] = new Vector4(lights[j].position, 1.0f);
				pointLightColors[j] = new Vector4(lights[j].color, 1.0f);
			}
			graphics.setUniform(particleShader.getUniform("u_pointLight_position", UniformType.Vector4, 16), pointLightPositions, lights.Count);
			graphics.setUniform(particleShader.getUniform("u_pointLight_color", UniformType.Vector4, 16), pointLightColors, lights.Count);
			graphics.setUniform(particleShader.getUniform("u_lightInfo", UniformType.Vector4), new Vector4(lights.Count + 0.5f, 0.0f, 0.0f, 0.0f));

			particleBatch.submitDrawCall(i, particleShader);
		}


		graphics.resetState();
		graphics.setPass((int)RenderPass.Forward);

		graphics.setRenderTarget(forward);

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
		particleBatch.end();

		for (int i = 0; i < particleBatch.getNumDrawCalls(); i++)
		{
			graphics.setBlendState(BlendState.Additive);

			graphics.setViewTransform(projection, view);

			particleBatch.submitDrawCall(i, particleAdditiveShader);
		}
	}

	static void SubmitWaterMesh(ushort vertexBuffer, ushort indexBuffer, Matrix transform)
	{
		graphics.setVertexBuffer(vertexBuffer);
		graphics.setIndexBuffer(indexBuffer);

		graphics.setTransform(transform);

		graphics.setUniform(waterShader.getUniform("u_time", UniformType.Vector4), new Vector4(Time.currentTime / 1e9f, 0.0f, 0.0f, 0.0f));

		graphics.setUniform(waterShader.getUniform("u_cameraPosition", UniformType.Vector4), new Vector4(camera.position, Time.currentTime / 1e9f));

		if (directionalLights.Count > 0)
		{
			graphics.setUniform(waterShader.getUniform("u_directionalLightDirection", UniformType.Vector4), new Vector4(directionalLights[0].light.direction, 0.0f));
			graphics.setUniform(waterShader.getUniform("u_directionalLightColor", UniformType.Vector4), new Vector4(directionalLights[0].light.color, 0.0f));

			graphics.setUniform(waterShader.getUniform("u_directionalLightFarPlane", UniformType.Vector4), new Vector4(ShadowMap.FAR_PLANES[2], 0.0f, 0.0f, 0.0f));

			ShadowMap shadowMap = directionalLights[0].light.shadowMap;
			int lastCascade = shadowMap.renderTargets.Length - 1;
			RenderTarget renderTarget = shadowMap.renderTargets[lastCascade];
			Matrix toLightSpace = shadowMap.cascadeProjections[lastCascade] * shadowMap.cascadeViews[lastCascade];
			graphics.setTexture(waterShader.getUniform("s_directionalLightShadowMap", UniformType.Sampler), 5, renderTarget.getAttachmentTexture(0));
			graphics.setUniform(waterShader.getUniform("u_directionalLightToLightSpace", UniformType.Matrix4), toLightSpace);
		}
		else
		{
			graphics.setUniform(waterShader.getUniform("u_directionalLightDirection", UniformType.Vector4), new Vector4(0.0f));
			graphics.setUniform(waterShader.getUniform("u_directionalLightColor", UniformType.Vector4), new Vector4(0.0f));

			graphics.setUniform(waterShader.getUniform("u_directionalLightFarPlane", UniformType.Vector4), new Vector4(ShadowMap.FAR_PLANES[2], 0.0f, 0.0f, 0.0f));

			graphics.setTexture(waterShader.getUniform("s_directionalLightShadowMap", UniformType.Sampler), 0, emptyShadowTexture);
			graphics.setUniform(waterShader.getUniform("u_directionalLightToLightSpace", UniformType.Matrix4), Matrix.Identity);
		}

		if (environmentMap != null)
			graphics.setTexture(waterShader.getUniform("s_environmentMap", UniformType.Sampler), 1, environmentMap);
		else
			graphics.setTexture(waterShader.getUniform("s_environmentMap", UniformType.Sampler), 1, emptyCubemap);

		graphics.draw(waterShader);
	}

	static void RenderWater()
	{
		graphics.resetState();
		graphics.setPass((int)RenderPass.Forward);

		graphics.setRenderTarget(forward);

		graphics.setViewTransform(projection, view);
		//graphics.setTransform(Matrix.CreateTranslation(0.0f, 3.0f, 0.0f));

		for (int i = 0; i < waterTiles.Count; i++)
		{
			if (waterTiles[i].model != null)
			{
				for (int j = 0; j < waterTiles[i].model.meshCount; j++)
				{
					MeshData mesh = waterTiles[i].model.getMeshData(j).Value;
					Matrix transform = Matrix.CreateTranslation(waterTiles[i].position) * GetNodeTransform(waterTiles[i].model.skeleton.getNode(mesh.nodeID));
					if (IntersectsFrustum(new Vector3(mesh.boundingSphere.xcenter, mesh.boundingSphere.ycenter, mesh.boundingSphere.zcenter), mesh.boundingSphere.radius, transform, pv))
					{
						SubmitWaterMesh(mesh.vertexBufferID, mesh.indexBufferID, transform);

						meshRenderCounter++;
					}
					else
					{
						// reset state so we can submit new draw call data
						//graphics.resetState();

						meshCulledCounter++;
					}
				}
			}
			else
			{
				Matrix transform = Matrix.CreateTranslation(waterTiles[i].position) * Matrix.CreateScale(waterTiles[i].size);
				SubmitWaterMesh(waterTileVertexBuffer.handle, waterTileIndexBuffer.handle, transform);
			}
		}
	}

	static void ForwardPass()
	{
		graphics.resetState();
		graphics.setPass((int)RenderPass.Forward);

		graphics.setRenderTarget(forward);


		RenderSky();
		RenderParticles();
		//RenderGrass();
		RenderWater();
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

	static void Composite()
	{
		graphics.resetState();
		graphics.setPass((int)RenderPass.Composite);

		graphics.setRenderTarget(compositeRenderTarget);

		graphics.setDepthTest(DepthTest.None);

		graphics.setTexture(compositeShader.getUniform("s_hdrBuffer", UniformType.Sampler), 0, forward.getAttachmentTexture(0));
		graphics.setTexture(compositeShader.getUniform("s_bloom", UniformType.Sampler), 1, bloomUpsampleChain[0].getAttachmentTexture(0));

		graphics.setUniform(compositeShader, "u_vignetteColor", new Vector4(vignetteColor, vignetteFalloff));

		graphics.setVertexBuffer(quad);
		graphics.draw(compositeShader);
	}

	static void PostProcessing()
	{
		BloomDownsample(0, forward.getAttachmentTexture(0), bloomDownsampleChain[0]);
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


		Composite();
	}

	static void TonemappingPass()
	{
		graphics.resetState();
		graphics.setPass((int)RenderPass.Tonemapping);

		graphics.setRenderTarget(null);

		graphics.setDepthTest(DepthTest.None);

		graphics.setVertexBuffer(quad);
		graphics.setTexture(tonemappingShader.getUniform("s_hdrBuffer", UniformType.Sampler), 0, compositeRenderTarget.getAttachmentTexture(0));

		graphics.draw(tonemappingShader);
	}

	static void UIPass()
	{
		graphics.resetState();
		graphics.setPass((int)RenderPass.UI);

		graphics.setRenderTarget(null);


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

		uiTextureBatch.end();

		for (int i = 0; i < uiTextureBatch.getNumDrawCalls(); i++)
		{
			graphics.setDepthTest(DepthTest.None);
			graphics.setBlendState(BlendState.Alpha);

			graphics.setViewTransform(Matrix.CreateOrthographic(0, Display.viewportSize.x, 0, Display.viewportSize.y, 1.0f, -1.0f), Matrix.Identity);

			uiTextureBatch.submitDrawCall(i, uiTextureShader);
		}


		int numCharacters = 0;
		foreach (TextDrawCommand text in texts)
			numCharacters += text.length;

		textBatch.begin(numCharacters);

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

		textBatch.end();

		for (int i = 0; i < textBatch.getNumDrawCalls(); i++)
		{
			graphics.setDepthTest(DepthTest.None);

			textBatch.submitDrawCall(i, textShader);
		}
	}

	public static void End()
	{
		meshRenderCounter = 0;
		meshCulledCounter = 0;

		GeometryPass();
		ShadowPass();
		ReflectionProbePass();
		AmbientOcclusionPass();
		DeferredPass();

		graphics.blit(forward.getAttachmentTexture(forward.attachmentCount - 1), gbuffer.getAttachmentTexture(gbuffer.attachmentCount - 1));

		ForwardPass();
		PostProcessing();
		TonemappingPass();
		UIPass();

		models.Clear();
		terrains.Clear();
		foliage.Clear();
		skies.Clear();
		waterTiles.Clear();
		lights.Clear();
		directionalLights.Clear();
		reflectionProbes.Clear();
		particles.Clear();
		particlesAdditive.Clear();
		grassPatches.Clear();
		uiTextures.Clear();
		texts.Clear();
	}
}
