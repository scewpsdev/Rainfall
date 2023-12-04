using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;


public static class Renderer
{
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
		DistanceFog,
		BloomDownsample,
		BloomUpsample = BloomDownsample + BLOOM_CHAIN_LENGTH,
		Composite = BloomUpsample + BLOOM_CHAIN_LENGTH - 1,
		Tonemapping,
		UI,
	}

	struct SpriteDrawCommand
	{
		internal Vector3 position;
		internal Vector2 size;
		internal Vector4 color;
		internal Vector3 normal;
		internal float angle;
		internal Vector2 rotationCenter;
		internal bool horizontal;
		internal Sprite sprite;
		internal bool flippedX, flippedY;
	}

	struct LightDrawCommand
	{
		internal Vector3 position;
		internal Vector3 color;
	}

	struct ParticleDrawCommand
	{
		internal Vector3 position;
		internal Texture atlas;
		internal int u0, v0, u1, v1;
		internal float size;
		internal float rotation;
		internal Vector4 color;
		internal bool additive;
	}

	struct UITextureDrawCommand
	{
		internal int x, y;
		internal int width, height;
		internal Texture texture;
		internal uint textureFlags;
		internal int u0, v0, u1, v1;
		internal Vector4 color;
	}

	unsafe struct TextDrawCommand
	{
		internal int x, y;
		internal float scale;
		internal byte* text;
		internal string str;
		internal int length;
		internal Font font;
		internal uint color;
	}


	const int SSAO_KERNEL_SIZE = 64;
	const int MAX_LIGHTS_PER_PASS = 16;

	const int BLOOM_CHAIN_LENGTH = 6;
	const int MAX_REFLECTION_PROBES = 4;


	public static GraphicsDevice graphics { get; private set; }

	static RenderTarget gbuffer;
	static RenderTarget forward;
	static RenderTarget postProcessing;
	static VertexBuffer quad;
	static VertexBuffer skydome;
	static IndexBuffer skydomeIdx;

	static RenderTarget ssaoRenderTarget;
	//static Vector4[] ssaoKernel;
	static Texture ssaoNoiseTexture;
	static RenderTarget ssaoBlurRenderTarget;

	static RenderTarget[] bloomDownsampleChain;
	static RenderTarget[] bloomUpsampleChain;

	static RenderTarget compositeRenderTarget;

	static Shader spriteShader;
	static Shader spriteNormalmapShader;
	static Shader ssaoShader;
	static Shader ssaoBlurShader;
	static Shader deferredPointShader, deferredDirectionalShader, deferredEnvironmentShader;
	static Shader deferredPointSimpleShader, deferredEnvironmentSimpleShader;
	static Shader particleShader;
	static Shader particleAdditiveShader;
	static Shader fogShader;
	static Shader bloomDownsampleShader;
	static Shader bloomUpsampleShader;
	static Shader compositeShader;
	static Shader tonemappingShader;
	static Shader uiTextureShader;
	static Shader textShader;

	static SpriteBatch batch;
	static SpriteBatch batchNormalmapped;
	static SpriteBatch particleBatch;
	static SpriteBatch uiTextureBatch;
	static SpriteBatch textBatch;

	public static Camera camera;
	public static Matrix projection, view, pv;

	public static Vector3 ambientLight = new Vector3(1.0f);

	public static Vector3 fogColor = new Vector3(1.0f);
	public static float fogIntensity = 0.0f;

	public static Vector3 vignetteColor = new Vector3(0.0f);
	public static float vignetteFalloff = 0.0f; // default value: 0.37f

	static Matrix[] instanceTransformBuffer = new Matrix[4096];

	static List<SpriteDrawCommand> sprites = new List<SpriteDrawCommand>();
	static List<SpriteDrawCommand> spritesNormalmapped = new List<SpriteDrawCommand>();
	static List<LightDrawCommand> lights = new List<LightDrawCommand>();
	static List<ParticleDrawCommand> particles = new List<ParticleDrawCommand>();
	static List<ParticleDrawCommand> particlesAdditive = new List<ParticleDrawCommand>();
	static List<UITextureDrawCommand> uiTextures = new List<UITextureDrawCommand>();
	static List<TextDrawCommand> texts = new List<TextDrawCommand>();

	public static bool ambientOcclusionEnabled = true;


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
			new RenderTargetAttachment(BackbufferRatio.Equal, TextureFormat.D16F, (ulong)TextureFlags.RenderTarget | (ulong)TextureFlags.BlitDst | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp),
		});

		postProcessing = graphics.createRenderTarget(new RenderTargetAttachment[]
		{
			new RenderTargetAttachment(BackbufferRatio.Equal, TextureFormat.RG11B10F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp),
		});

		quad = graphics.createVertexBuffer(
			graphics.createVideoMemory(stackalloc float[] { -3.0f, -1.0f, 1.0f, 1.0f, -1.0f, 1.0f, 1.0f, 3.0f, 1.0f }),
			stackalloc VertexElement[] { new VertexElement(VertexAttribute.Position, VertexAttributeType.Vector3, false) }
		);

		skydome = graphics.createVertexBuffer(
			graphics.createVideoMemory(stackalloc float[] { -100.0f, -100.0f, 100.0f, 100.0f, -100.0f, 100.0f, 0.0f, -100.0f, -100.0f, 0.0f, 100.0f, 0.0f }),
			stackalloc VertexElement[] { new VertexElement(VertexAttribute.Position, VertexAttributeType.Vector3, false) }
		);

		skydomeIdx = graphics.createIndexBuffer(graphics.createVideoMemory(stackalloc short[] { 0, 1, 2, 2, 1, 3, 1, 0, 3, 0, 2, 3 }));

		ssaoRenderTarget = graphics.createRenderTarget(new RenderTargetAttachment[]
		{
			new RenderTargetAttachment(BackbufferRatio.Equal, TextureFormat.R8, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp)
		});

		Span<byte> ssaoNoiseData = stackalloc byte[4 * 4 * 2];
		Random.Shared.NextBytes(ssaoNoiseData);
		ssaoNoiseTexture = graphics.createTexture(4, 4, TextureFormat.RG8, graphics.createVideoMemory(ssaoNoiseData));

		ssaoBlurRenderTarget = graphics.createRenderTarget(new RenderTargetAttachment[]
		{
			new RenderTargetAttachment(BackbufferRatio.Equal, TextureFormat.R8, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp)
		});


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


		spriteShader = Resource.GetShader("res/shaders/sprite/sprite.vs.shader", "res/shaders/sprite/sprite.fs.shader");
		spriteNormalmapShader = Resource.GetShader("res/shaders/sprite/sprite.vs.shader", "res/shaders/sprite/sprite_normalmap.fs.shader");
		ssaoShader = Resource.GetShader("res/shaders/ssao/ssao.vs.shader", "res/shaders/ssao/ssao.fs.shader");
		ssaoBlurShader = Resource.GetShader("res/shaders/ssao/ssao_blur.vs.shader", "res/shaders/ssao/ssao_blur.fs.shader");
		deferredPointShader = Resource.GetShader("res/shaders/deferred/deferred.vs.shader", "res/shaders/deferred/deferred_point.fs.shader");
		deferredDirectionalShader = Resource.GetShader("res/shaders/deferred/deferred.vs.shader", "res/shaders/deferred/deferred_directional.fs.shader");
		deferredEnvironmentShader = Resource.GetShader("res/shaders/deferred/deferred.vs.shader", "res/shaders/deferred/deferred_environment.fs.shader");
		deferredPointSimpleShader = Resource.GetShader("res/shaders/deferred/deferred.vs.shader", "res/shaders/deferred/deferred_point_simple.fs.shader");
		deferredEnvironmentSimpleShader = Resource.GetShader("res/shaders/deferred/deferred.vs.shader", "res/shaders/deferred/deferred_environment_simple.fs.shader");
		particleShader = Resource.GetShader("res/shaders/particle/particle.vs.shader", "res/shaders/particle/particle.fs.shader");
		particleAdditiveShader = Resource.GetShader("res/shaders/particle_additive/particle_additive.vs.shader", "res/shaders/particle_additive/particle_additive.fs.shader");
		fogShader = Resource.GetShader("res/shaders/fog/fog.vs.shader", "res/shaders/fog/fog.fs.shader");
		bloomDownsampleShader = Resource.GetShader("res/shaders/bloom/bloom.vs.shader", "res/shaders/bloom/bloom_downsample.fs.shader");
		bloomUpsampleShader = Resource.GetShader("res/shaders/bloom/bloom.vs.shader", "res/shaders/bloom/bloom_upsample.fs.shader");
		compositeShader = Resource.GetShader("res/shaders/composite/composite.vs.shader", "res/shaders/composite/composite.fs.shader");
		tonemappingShader = Resource.GetShader("res/shaders/tonemapping/tonemapping.vs.shader", "res/shaders/tonemapping/tonemapping.fs.shader");
		uiTextureShader = Resource.GetShader("res/shaders/ui/ui.vs.shader", "res/shaders/ui/ui.fs.shader");
		textShader = Resource.GetShader("res/shaders/text/text.vs.shader", "res/shaders/text/text.fs.shader");

		batch = new SpriteBatch(graphics);
		batchNormalmapped = new SpriteBatch(graphics);
		particleBatch = new SpriteBatch(graphics);
		uiTextureBatch = new SpriteBatch(graphics);
		textBatch = new SpriteBatch(graphics);
	}

	public static void DrawHorizontalWall(Vector3 position, Vector2 size, Sprite sprite, Vector4 color)
	{
		spritesNormalmapped.Add(new SpriteDrawCommand { position = position, size = size, sprite = sprite, color = color, normal = Vector3.UnitY, angle = 0.0f, horizontal = true, flippedX = false, flippedY = false });
	}

	public static void DrawVerticalWall(Vector3 position, Vector2 size, Sprite sprite, Vector4 color)
	{
		spritesNormalmapped.Add(new SpriteDrawCommand { position = position, size = size, sprite = sprite, color = color, normal = Vector3.UnitZ, angle = 0.0f, horizontal = false, flippedX = false, flippedY = false });
	}

	public static void DrawEntitySprite(Vector3 position, Vector2 size, Sprite sprite, bool direction)
	{
		sprites.Add(new SpriteDrawCommand { position = position - new Vector3(0.5f * size.x, 0.0f, 0.0f), size = size, sprite = sprite, color = Vector4.One, normal = Vector3.UnitY, angle = 0.0f, horizontal = false, flippedX = !direction, flippedY = false });
	}

	public static void DrawProjectile(Vector3 position, Vector2 size, float angle, Sprite sprite, Vector4 color)
	{
		sprites.Add(new SpriteDrawCommand { position = position + new Vector3(-0.5f * size.x, -0.5f * size.y, 0.0f), size = size, sprite = sprite, color = color, normal = Vector3.UnitY, angle = angle, rotationCenter = size * 0.5f, horizontal = false, flippedX = false, flippedY = false });
	}

	public static void DrawLight(Vector3 position, Vector3 color)
	{
		lights.Add(new LightDrawCommand { position = position, color = color });
	}

	public static void DrawParticle(Vector3 position, Texture atlas, int u0, int v0, int u1, int v1, float size, float rotation, Vector4 color, bool additive)
	{
		if (additive)
			particlesAdditive.Add(new ParticleDrawCommand { position = position, atlas = atlas, u0 = u0, v0 = v0, u1 = u1, v1 = v1, size = size, rotation = rotation, color = color, additive = true });
		else
			particles.Add(new ParticleDrawCommand { position = position, atlas = atlas, u0 = u0, v0 = v0, u1 = u1, v1 = v1, size = size, rotation = rotation, color = color, additive = false });
	}

	public static void DrawUITexture(int x, int y, int width, int height, Texture texture, uint textureFlags, int u0, int v0, int u1, int v1, uint color)
	{
		uiTextures.Add(new UITextureDrawCommand { x = x, y = Display.viewportSize.y - y - height, width = width, height = height, texture = texture, textureFlags = textureFlags, u0 = u0, v0 = v0, u1 = u1, v1 = v1, color = MathHelper.ARGBToVector(color) });
	}

	public static void DrawUITexture(int x, int y, int width, int height, Texture texture)
	{
		uiTextures.Add(new UITextureDrawCommand { x = x, y = Display.viewportSize.y - y - height, width = width, height = height, texture = texture, u0 = 0, v0 = 0, u1 = texture.info.width, v1 = texture.info.height, color = Vector4.One });
	}

	public static void DrawUIRect(int x, int y, int width, int height, uint color)
	{
		uiTextures.Add(new UITextureDrawCommand { x = x, y = Display.viewportSize.y - y - height, width = width, height = height, texture = null, u0 = 0, v0 = 0, u1 = 0, v1 = 0, color = MathHelper.ARGBToVector(color) });
	}

	public static void DrawText(int x, int y, float scale, Span<byte> text, int length, Font font, uint color)
	{
		unsafe
		{
			fixed (byte* textPtr = text)
				texts.Add(new TextDrawCommand { x = x, y = y, scale = scale, text = textPtr, length = length, font = font, color = color });
		}
	}

	public static void DrawText(int x, int y, float scale, Span<byte> text, Font font, uint color)
	{
		DrawText(x, y, scale, text, StringUtils.StringLength(text), font, color);
	}

	public static void DrawText(int x, int y, float scale, string text, int length, Font font, uint color)
	{
		texts.Add(new TextDrawCommand { x = x, y = y, scale = scale, str = text, length = length, font = font, color = color });
	}

	public static void DrawText(int x, int y, float scale, string text, Font font, uint color)
	{
		DrawText(x, y, scale, text, text.Length, font, color);
	}

	public static void Begin(Camera camera)
	{
		Renderer.camera = camera;

		projection = camera.getProjectionMatrix();
		view = camera.getViewMatrix();
		pv = projection * view;
	}

	static void RenderSprites()
	{
		graphics.resetState();
		graphics.setViewTransform(projection, view);

		batch.begin(sprites.Count);
		for (int i = 0; i < sprites.Count; i++)
		{
			SpriteDrawCommand draw = sprites[i];
			Texture texture = null;
			float u0 = 0.0f, v0 = 0.0f, u1 = 0.0f, v1 = 0.0f;
			if (draw.sprite != null)
			{
				texture = draw.sprite.tileset.texture;
				draw.sprite.getUVs(out u0, out v0, out u1, out v1);
			}
			batch.draw(draw.position.x, draw.position.y, draw.position.z, draw.size.x, draw.size.y, draw.angle, draw.rotationCenter, draw.horizontal, false, texture, uint.MaxValue, u0, v0, u1, v1, draw.flippedX, draw.horizontal ^ draw.flippedY, draw.color, draw.normal);
		}
		batch.end();

		for (int i = 0; i < batch.getNumDrawCalls(); i++)
		{
			graphics.setCullState(CullState.None);
			graphics.setBlendState(BlendState.Alpha);

			batch.submitDrawCall(i, spriteShader);
		}


		batchNormalmapped.begin(spritesNormalmapped.Count);
		for (int i = 0; i < spritesNormalmapped.Count; i++)
		{
			SpriteDrawCommand draw = spritesNormalmapped[i];
			Texture texture = null;
			float u0 = 0.0f, v0 = 0.0f, u1 = 0.0f, v1 = 0.0f;
			if (draw.sprite != null)
			{
				texture = draw.sprite.tileset.texture;
				draw.sprite.getUVs(out u0, out v0, out u1, out v1);
			}
			batchNormalmapped.draw(draw.position.x, draw.position.y, draw.position.z, draw.size.x, draw.size.y, draw.angle, draw.rotationCenter, draw.horizontal, false, texture, uint.MaxValue, u0, v0, u1, v1, draw.flippedX, draw.horizontal ^ draw.flippedY, draw.color, draw.normal);
		}
		batchNormalmapped.end();

		for (int i = 0; i < batchNormalmapped.getNumDrawCalls(); i++)
		{
			graphics.setCullState(CullState.None);
			graphics.setBlendState(BlendState.Alpha);

			batchNormalmapped.submitDrawCall(i, spriteNormalmapShader);
		}
	}

	static void GeometryPass()
	{
		graphics.setPass((int)RenderPass.Geometry);
		graphics.setRenderTarget(gbuffer);

		RenderSprites();
		//RenderModelsInstanced();
		//RenderTerrains();
		//RenderFoliage();
		//RenderGrass();
	}

	static void AmbientOcclusionPass()
	{
		if (!ambientOcclusionEnabled)
			return;

		Vector4 cameraFrustum = new Vector4(Camera.near, Camera.far, 0.0f, 0.0f);

		// AO
		{
			graphics.resetState();
			graphics.setPass((int)RenderPass.AmbientOcclusion);

			graphics.setRenderTarget(ssaoRenderTarget);

			graphics.setTexture(ssaoShader.getUniform("s_depthBuffer", UniformType.Sampler), 0, gbuffer.getAttachmentTexture(4));
			graphics.setTexture(ssaoShader.getUniform("s_normalsBuffer", UniformType.Sampler), 1, gbuffer.getAttachmentTexture(1));

			//graphics.setUniform(ssaoShader.getUniform("u_ssaoKernel", UniformType.Vector4, SSAO_KERNEL_SIZE), ssaoKernel, SSAO_KERNEL_SIZE);
			graphics.setTexture(ssaoShader.getUniform("s_ssaoNoise", UniformType.Sampler), 2, ssaoNoiseTexture);

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

			graphics.setUniform(ssaoBlurShader.getUniform("u_cameraFrustum", UniformType.Vector4), cameraFrustum);

			graphics.setVertexBuffer(quad);

			graphics.draw(ssaoBlurShader);
		}
	}

	static void RenderPointLights()
	{
		Span<Vector4> lightPositionBuffer = stackalloc Vector4[MAX_LIGHTS_PER_PASS];
		Span<Vector4> lightColorBuffer = stackalloc Vector4[MAX_LIGHTS_PER_PASS];

		Shader shader = deferredPointShader;

		for (int i = 0; i < lights.Count; i++)
		{
			graphics.resetState();

			graphics.setBlendState(BlendState.Additive);
			graphics.setDepthTest(DepthTest.None);
			graphics.setCullState(CullState.ClockWise);

			graphics.setVertexBuffer(quad);

			graphics.setTexture(shader.getUniform("s_gbuffer0", UniformType.Sampler), 0, gbuffer.getAttachmentTexture(0));
			graphics.setTexture(shader.getUniform("s_gbuffer1", UniformType.Sampler), 1, gbuffer.getAttachmentTexture(1));
			graphics.setTexture(shader.getUniform("s_gbuffer2", UniformType.Sampler), 2, gbuffer.getAttachmentTexture(2));
			graphics.setTexture(shader.getUniform("s_gbuffer3", UniformType.Sampler), 3, gbuffer.getAttachmentTexture(3));

			graphics.setTexture(shader.getUniform("s_ambientOcclusion", UniformType.Sampler), 4, ssaoBlurRenderTarget.getAttachmentTexture(0));

			graphics.setUniform(shader, "u_cameraPosition", new Vector4(camera.position, 0.0f));

			int numRemainingLights = Math.Min(lights.Count - i, MAX_LIGHTS_PER_PASS);
			for (int j = 0; j < numRemainingLights; j++)
			{
				int lightID = i + j;
				lightPositionBuffer[j] = new Vector4(lights[lightID].position, 0.0f);
				lightColorBuffer[j] = new Vector4(lights[lightID].color, 0.0f);
			}
			i += numRemainingLights;

			graphics.setUniform(shader.getUniform("u_lightPosition", UniformType.Vector4, MAX_LIGHTS_PER_PASS), lightPositionBuffer);
			graphics.setUniform(shader.getUniform("u_lightColor", UniformType.Vector4, MAX_LIGHTS_PER_PASS), lightColorBuffer);

			graphics.draw(shader);
		}
	}

	static void RenderAmbientLights()
	{
		Span<byte> reflectionProbeUniform = stackalloc byte[32];
		Span<byte> reflectionProbePositionUniform = stackalloc byte[32];
		Span<byte> reflectionProbeSizeUniform = stackalloc byte[32];
		Span<byte> reflectionProbeOriginUniform = stackalloc byte[32];


		Shader shader = deferredEnvironmentShader;

		graphics.resetState();

		graphics.setBlendState(BlendState.Additive);
		graphics.setDepthTest(DepthTest.None);
		graphics.setCullState(CullState.ClockWise);

		graphics.setVertexBuffer(quad);

		graphics.setTexture(shader.getUniform("s_gbuffer0", UniformType.Sampler), 0, gbuffer.getAttachmentTexture(0));
		graphics.setTexture(shader.getUniform("s_gbuffer1", UniformType.Sampler), 1, gbuffer.getAttachmentTexture(1));
		graphics.setTexture(shader.getUniform("s_gbuffer2", UniformType.Sampler), 2, gbuffer.getAttachmentTexture(2));
		graphics.setTexture(shader.getUniform("s_gbuffer3", UniformType.Sampler), 3, gbuffer.getAttachmentTexture(3));

		graphics.setTexture(shader.getUniform("s_ambientOcclusion", UniformType.Sampler), 4, ssaoBlurRenderTarget.getAttachmentTexture(0));

		graphics.setUniform(shader, "u_cameraPosition", new Vector4(camera.position, 0.0f));

		graphics.setUniform(shader, "u_ambientColor", new Vector4(ambientLight, 1.0f));

		graphics.draw(shader);
	}

	static void DeferredPass()
	{
		graphics.setPass((int)RenderPass.Deferred);
		graphics.setRenderTarget(forward);

		RenderAmbientLights();
		RenderPointLights();
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
				particle.rotation, new Vector2(particle.size) * 0.5f,
				false, true,
				particle.atlas, 0,
				u0, v0, u1, v1, false, false,
				particle.color, Vector3.Zero);
		}
		particleBatch.end();

		Span<Vector4> pointLightPositions = stackalloc Vector4[MAX_LIGHTS_PER_PASS];
		Span<Vector4> pointLightColors = stackalloc Vector4[MAX_LIGHTS_PER_PASS];

		for (int i = 0; i < particleBatch.getNumDrawCalls(); i++)
		{
			graphics.setBlendState(BlendState.Alpha);

			graphics.setViewTransform(projection, view);

			for (int j = 0; j < lights.Count; j++)
			{
				pointLightPositions[j] = new Vector4(lights[j].position, 1.0f);
				pointLightColors[j] = new Vector4(lights[j].color, 1.0f);
			}
			graphics.setUniform(particleShader.getUniform("u_pointLight_position", UniformType.Vector4, MAX_LIGHTS_PER_PASS), pointLightPositions);
			graphics.setUniform(particleShader.getUniform("u_pointLight_color", UniformType.Vector4, MAX_LIGHTS_PER_PASS), pointLightColors);
			graphics.setUniform(particleShader, "u_lightInfo", new Vector4(lights.Count + 0.5f, 0.0f, 0.0f, 0.0f));

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
				particle.rotation, new Vector2(particle.size) * 0.5f,
				false, true,
				particle.atlas, 0,
				u0, v0, u1, v1, false, false,
				particle.color, Vector3.Zero);
		}
		particleBatch.end();

		for (int i = 0; i < particleBatch.getNumDrawCalls(); i++)
		{
			graphics.setBlendState(BlendState.Additive);

			graphics.setViewTransform(projection, view);

			particleBatch.submitDrawCall(i, particleAdditiveShader);
		}
	}

	static void ForwardPass()
	{
		graphics.setPass((int)RenderPass.Forward);
		graphics.setRenderTarget(forward);


		//RenderSky();
		//RenderParticles();
		//RenderGrass();
		//RenderWater();
	}

	static void DistanceFog()
	{
		graphics.setPass((int)RenderPass.DistanceFog);
		graphics.setRenderTarget(postProcessing);

		graphics.resetState();
		graphics.setCullState(CullState.ClockWise);

		graphics.setTexture(fogShader, "s_frame", 0, forward.getAttachmentTexture(0));
		graphics.setTexture(fogShader, "s_depth", 1, forward.getAttachmentTexture(1));

		Vector4 cameraFrustum = new Vector4(Camera.near, Camera.far, 0.0f, 0.0f);

		graphics.setUniform(fogShader.getUniform("u_fogData", UniformType.Vector4), new Vector4(fogColor, fogIntensity));
		graphics.setUniform(fogShader.getUniform("u_cameraFrustum", UniformType.Vector4), cameraFrustum);

		graphics.setVertexBuffer(quad);

		graphics.draw(fogShader);
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
		graphics.setCullState(CullState.ClockWise);

		graphics.setTexture(compositeShader.getUniform("s_hdrBuffer", UniformType.Sampler), 0, forward.getAttachmentTexture(0));
		graphics.setTexture(compositeShader.getUniform("s_bloom", UniformType.Sampler), 1, bloomUpsampleChain[0].getAttachmentTexture(0));

		graphics.setUniform(compositeShader, "u_vignetteColor", new Vector4(vignetteColor, vignetteFalloff));

		graphics.setVertexBuffer(quad);
		graphics.draw(compositeShader);
	}

	static void PostProcessing()
	{
		//DistanceFog();


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
		graphics.setCullState(CullState.ClockWise);

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
				uiTextures[i].texture, uiTextures[i].textureFlags,
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

			unsafe
			{
				byte* text = texts[i].text;
				string str = texts[i].str;
				int length = texts[i].length;

				Font font = texts[i].font;
				uint color = texts[i].color;

				if (text != null)
				{
					graphics.drawText(x, y, scale, text, length, font, color, textBatch);
				}
				else if (str != null)
				{
					graphics.drawText(x, y, scale, str, length, font, color, textBatch);
				}
			}
		}

		textBatch.end();

		for (int i = 0; i < textBatch.getNumDrawCalls(); i++)
		{
			graphics.setDepthTest(DepthTest.None);
			graphics.setBlendState(BlendState.Alpha);

			graphics.setViewTransform(Matrix.CreateOrthographic(0, Display.viewportSize.x, 0, Display.viewportSize.y, 1.0f, -1.0f), Matrix.Identity);

			textBatch.submitDrawCall(i, textShader);
		}
	}

	public static void End()
	{
		GeometryPass();
		AmbientOcclusionPass();
		DeferredPass();

		//graphics.blit(forward.getAttachmentTexture(forward.attachmentCount - 1), gbuffer.getAttachmentTexture(gbuffer.attachmentCount - 1));

		//ForwardPass();
		PostProcessing();
		TonemappingPass();
		UIPass();

		sprites.Clear();
		spritesNormalmapped.Clear();
		lights.Clear();
		particles.Clear();
		particlesAdditive.Clear();
		uiTextures.Clear();
		texts.Clear();
	}
}
