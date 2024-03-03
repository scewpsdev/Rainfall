using Rainfall;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
		PointShadow,
		ReflectionProbe = PointShadow + MAX_POINT_SHADOWS * 6,
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

	struct ModelDrawCommand
	{
		internal Model model;
		internal int meshID;
		internal Animator animator;
		internal Matrix transform;
	}

	struct TerrainDrawCommand
	{
		internal Texture heightmap;
		internal Texture normalmap;
		internal Texture splatMap;
		internal Matrix transform;

		internal Texture diffuse0;
		internal Texture diffuse1;
		internal Texture diffuse2;
		internal Texture diffuse3;
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

	struct ReflectionProbeDrawCommand
	{
		internal ReflectionProbe reflectionProbe;
		internal Vector3 position;
		internal Vector3 size;
		internal Vector3 origin;
	}

	struct ParticleSystemDrawCommand
	{
		internal Particle[] particles;
		internal List<int> particleIndices;
		internal Matrix transform;
		internal Vector3 spawnOffset;
		internal bool follow;
		internal Texture textureAtlas;
		internal Vector2i atlasSize;
		internal bool linearFiltering;
		internal bool additive;
	}

	struct GrassDrawCommand
	{
		internal Vector2 position;
		internal Terrain terrain;
	}

	class ModelComparer : IComparer<Model>
	{
		public unsafe int Compare(Model x, Model y)
		{
			int xh = x.GetHashCode();
			int yh = y.GetHashCode();
			return xh < yh ? 1 : xh > yh ? -1 : 1;
		}
	}


	const int SSAO_KERNEL_SIZE = 64;
	const int MAX_LIGHTS_PER_PASS = 16;

	const int BLOOM_CHAIN_LENGTH = 6;
	const int MAX_REFLECTION_PROBES = 4;
	const int MAX_POINT_SHADOWS = 8;

	const int TERRAIN_RES = 128;
	const float TERRAIN_SIZE = 128.0f;

	const int NUM_GRASS_BLADES = 4096 * 16;
	public const float GRASS_PATCH_SIZE = 20.0f;


	public static GraphicsDevice graphics { get; private set; }

	static RenderTarget gbuffer;
	static RenderTarget forward;
	static RenderTarget postProcessing;
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

	//static Cubemap pointShadowCubemap;
	//static RenderTarget[] pointShadowRenderTargets = new RenderTarget[6];

	static Shader modelShader;
	static Shader modelDepthShader;
	static Shader modelSimpleShader;
	static Shader modelAnimShader;
	static Shader modelAnimDepthShader;
	static Shader meshInstancedShader;
	static Shader terrainShader;
	static Shader foliageShader;
	static Shader ssaoShader;
	static Shader ssaoBlurShader;
	static Shader deferredPointShader, deferredPointShadowShader, deferredPointSimpleShader, deferredPointShadowSimpleShader;
	static Shader deferredDirectionalShader;
	static Shader deferredEnvironmentShader, deferredEnvironmentSimpleShader;
	static Shader skyShader;
	static Shader waterShader;
	static Shader particleShader;
	static Shader particleAdditiveShader;
	static Shader grassShader;
	static Shader fogShader;
	static Shader bloomDownsampleShader;
	static Shader bloomUpsampleShader;
	static Shader compositeShader;
	static Shader tonemappingShader;

	static SpriteBatch particleBatch;

	//static FontData baskervilleFont;
	//public static Font promptFont, xpFont, notificationFont, stackSizeFont;
	//public static Font victoryFont;
	//public static Font uiFontMedium;

	public static Camera camera;
	public static Matrix projection, view, pv;

	static Cubemap environmentMap;
	static float environmentMapIntensity = 1.0f;

	static Matrix[] cubemapFaceRotations = new Matrix[6];

	public static Vector3 fogColor = new Vector3(1.0f);
	public static float fogIntensity = 0.0f;

	public static Vector3 vignetteColor = new Vector3(0.0f);
	public static float vignetteFalloff = 0.0f; // default value: 0.37f

	static Matrix[] instanceTransformBuffer = new Matrix[4096];

	static VertexBuffer particleVertexBuffer;
	static IndexBuffer particleIndexBuffer;

	static VertexBuffer[] terrainMeshes = new VertexBuffer[4];
	static IndexBuffer[] terrainMeshesIndices = new IndexBuffer[4];

	static GrassBladeData[] grassData = new GrassBladeData[NUM_GRASS_BLADES];

	static VertexBuffer grassBlade;
	static IndexBuffer grassIndices;
	static VertexBuffer waterTileVertexBuffer;
	static IndexBuffer waterTileIndexBuffer;

	static List<ModelDrawCommand> models = new List<ModelDrawCommand>();
	static SortedList<Model, ModelDrawCommand> modelsInstanced = new SortedList<Model, ModelDrawCommand>(new ModelComparer());
	static List<TerrainDrawCommand> terrains = new List<TerrainDrawCommand>();
	static List<LeaveDrawCommand> foliage = new List<LeaveDrawCommand>();
	static List<SkyDrawCommand> skies = new List<SkyDrawCommand>();
	static List<WaterDrawCommand> waterTiles = new List<WaterDrawCommand>();
	static List<LightDrawCommand> lights = new List<LightDrawCommand>();
	static List<PointLight> pointLights = new List<PointLight>();
	static List<DirectionalLight> directionalLights = new List<DirectionalLight>();
	static List<ReflectionProbeDrawCommand> reflectionProbes = new List<ReflectionProbeDrawCommand>();
	static List<ParticleSystemDrawCommand> particleSystems = new List<ParticleSystemDrawCommand>();
	static List<ParticleSystemDrawCommand> particleSystemsAdditive = new List<ParticleSystemDrawCommand>();
	static List<GrassDrawCommand> grassPatches = new List<GrassDrawCommand>();

	public static int meshRenderCounter = 0;
	public static int meshCulledCounter = 0;

	public static bool simplifiedLighting = false;
	public static bool ambientOcclusionEnabled = true;
	public static bool bloomEnabled = true;


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
			graphics.createVideoMemory(stackalloc float[] { -10, -10, 10, 10, -10, 10, 0, -10, -10, 0, 10, 0f }),
			stackalloc VertexElement[] { new VertexElement(VertexAttribute.Position, VertexAttributeType.Vector3, false) }
		);

		skydomeIdx = graphics.createIndexBuffer(graphics.createVideoMemory(stackalloc short[] { 0, 1, 2, 2, 1, 3, 1, 0, 3, 0, 2, 3 }));

		emptyShadowTexture = graphics.createTexture(1, 1, TextureFormat.D16F, graphics.createVideoMemory(stackalloc Half[] { (Half)1.0f }), (uint)SamplerFlags.CompareLEqual);
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

		Span<byte> ssaoNoiseData = stackalloc byte[4 * 4 * 2];
		Random.Shared.NextBytes(ssaoNoiseData);
		ssaoNoiseTexture = graphics.createTexture(4, 4, TextureFormat.RG8, graphics.createVideoMemory(ssaoNoiseData));

		ssaoBlurRenderTarget = graphics.createRenderTarget(new RenderTargetAttachment[]
		{
			new RenderTargetAttachment(BackbufferRatio.Equal, TextureFormat.R8, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp)
		});


		particleVertexBuffer = graphics.createVertexBuffer(graphics.createVideoMemory(stackalloc float[] { -0.5f, -0.5f, 0.0f, 1.0f, 0.5f, -0.5f, 1.0f, 1.0f, 0.5f, 0.5f, 1.0f, 0.0f, -0.5f, 0.5f, 0.0f, 0.0f }),
			stackalloc VertexElement[] { new VertexElement(VertexAttribute.Position, VertexAttributeType.Vector2, false), new VertexElement(VertexAttribute.TexCoord0, VertexAttributeType.Vector2, false) });
		particleIndexBuffer = graphics.createIndexBuffer(graphics.createVideoMemory(stackalloc short[] { 0, 1, 2, 2, 3, 0 }));


		{
			Span<VertexElement> vertexElements = stackalloc VertexElement[] { new VertexElement(VertexAttribute.Position, VertexAttributeType.Vector3, false) };

			int currentRes = TERRAIN_RES;
			for (int lod = 0; lod < 4; lod++)
			{
				Vector3[] positions = new Vector3[(currentRes + 1) * (currentRes + 1)];
				for (int z = 0; z < currentRes + 1; z++)
				{
					for (int x = 0; x < currentRes + 1; x++)
					{
						positions[x + z * (currentRes + 1)] = new Vector3(
							x / (float)(currentRes),
							0.0f,
							z / (float)(currentRes)
						);
					}
				}
				terrainMeshes[lod] = graphics.createVertexBuffer(
					graphics.createVideoMemory(positions),
					vertexElements
				);

				int[] indices = new int[currentRes * currentRes * 6];
				for (int z = 0; z < currentRes; z++)
				{
					for (int x = 0; x < currentRes; x++)
					{
						int i = x + z * currentRes;

						int i00 = x + z * (currentRes + 1);
						int i01 = x + (z + 1) * (currentRes + 1);
						int i11 = x + 1 + (z + 1) * (currentRes + 1);
						int i10 = x + 1 + z * (currentRes + 1);

						indices[i * 6 + 0] = i00;
						indices[i * 6 + 1] = i01;
						indices[i * 6 + 2] = i11;
						indices[i * 6 + 3] = i11;
						indices[i * 6 + 4] = i10;
						indices[i * 6 + 5] = i00;
					}
				}
				terrainMeshesIndices[lod] = graphics.createIndexBuffer(graphics.createVideoMemory(indices), BufferFlags.Index32);

				currentRes /= 2;
			}
		}

		for (int i = 0; i < NUM_GRASS_BLADES; i++)
		{
			//int xx = i % 256 % 16 * 4 + i / 256 % 4 % 2 * 2 + i / 1024 % 2;
			//int zz = i % 256 / 16 * 4 + i / 256 % 4 / 2 * 2 + i / 1024 / 2;
			int xx = i % 256;
			int zz = i / 256;
			float x = xx / 256.0f * GRASS_PATCH_SIZE + MathHelper.RandomFloat(-0.2f, 0.2f);
			float z = zz / 256.0f * GRASS_PATCH_SIZE + MathHelper.RandomFloat(-0.2f, 0.2f);
			float rotation = MathHelper.RandomFloat(0.0f, MathF.PI * 2.0f);

			grassData[i].positionRotation = new Vector4(x, 0.0f, z, rotation);
		}

		grassBlade = graphics.createVertexBuffer(
			graphics.createVideoMemory(new float[] {
				-0.05f, 0.0f, 0.0f,
				0.05f, 0.0f, 0.0f,
				0.0f, 1.0f, 0.0f,
				-0.5f, 1.0f, 0.0f,
			}),
			stackalloc VertexElement[] { new VertexElement(VertexAttribute.Position, VertexAttributeType.Vector3, false) }
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
			stackalloc VertexElement[] { new VertexElement(VertexAttribute.Position, VertexAttributeType.Vector3, false) }
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

		/*
		pointShadowCubemap = graphics.createCubemap(256, TextureFormat.D16F, (ulong)TextureFlags.RenderTarget);
		for (int i = 0; i < 6; i++)
		{
			pointShadowRenderTargets[i] = graphics.createRenderTarget(new RenderTargetAttachment[]
			{
				new RenderTargetAttachment(pointShadowCubemap, i, false)
			});
		}
		*/

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
		meshInstancedShader = Resource.GetShader("res/shaders/model/model_instanced.vs.shader", "res/shaders/model/model_instanced.fs.shader");
		terrainShader = Resource.GetShader("res/shaders/terrain/terrain.vs.shader", "res/shaders/terrain/terrain.fs.shader");
		foliageShader = Resource.GetShader("res/shaders/foliage/foliage.vs.shader", "res/shaders/foliage/foliage.fs.shader");
		ssaoShader = Resource.GetShader("res/shaders/ssao/ssao.vs.shader", "res/shaders/ssao/ssao.fs.shader");
		ssaoBlurShader = Resource.GetShader("res/shaders/ssao/ssao_blur.vs.shader", "res/shaders/ssao/ssao_blur.fs.shader");
		deferredPointShader = Resource.GetShader("res/shaders/deferred/deferred.vs.shader", "res/shaders/deferred/deferred_point.fs.shader");
		deferredPointShadowShader = Resource.GetShader("res/shaders/deferred/deferred.vs.shader", "res/shaders/deferred/deferred_point_shadow.fs.shader");
		deferredPointSimpleShader = Resource.GetShader("res/shaders/deferred/deferred.vs.shader", "res/shaders/deferred/deferred_point_simple.fs.shader");
		deferredPointShadowSimpleShader = Resource.GetShader("res/shaders/deferred/deferred.vs.shader", "res/shaders/deferred/deferred_point_shadow_simple.fs.shader");
		deferredDirectionalShader = Resource.GetShader("res/shaders/deferred/deferred.vs.shader", "res/shaders/deferred/deferred_directional.fs.shader");
		deferredEnvironmentShader = Resource.GetShader("res/shaders/deferred/deferred.vs.shader", "res/shaders/deferred/deferred_environment.fs.shader");
		deferredEnvironmentSimpleShader = Resource.GetShader("res/shaders/deferred/deferred.vs.shader", "res/shaders/deferred/deferred_environment_simple.fs.shader");
		skyShader = Resource.GetShader("res/shaders/sky/sky.vs.shader", "res/shaders/sky/sky.fs.shader");
		waterShader = Resource.GetShader("res/shaders/water/water.vs.shader", "res/shaders/water/water.fs.shader");
		particleShader = Resource.GetShader("res/shaders/particle/particle.vs.shader", "res/shaders/particle/particle.fs.shader");
		particleAdditiveShader = Resource.GetShader("res/shaders/particle/particle_additive.vs.shader", "res/shaders/particle/particle_additive.fs.shader");
		grassShader = Resource.GetShader("res/shaders/grass/grass.vs.shader", "res/shaders/grass/grass.fs.shader");
		fogShader = Resource.GetShader("res/shaders/fog/fog.vs.shader", "res/shaders/fog/fog.fs.shader");
		bloomDownsampleShader = Resource.GetShader("res/shaders/bloom/bloom.vs.shader", "res/shaders/bloom/bloom_downsample.fs.shader");
		bloomUpsampleShader = Resource.GetShader("res/shaders/bloom/bloom.vs.shader", "res/shaders/bloom/bloom_upsample.fs.shader");
		compositeShader = Resource.GetShader("res/shaders/composite/composite.vs.shader", "res/shaders/composite/composite.fs.shader");
		tonemappingShader = Resource.GetShader("res/shaders/tonemapping/tonemapping.vs.shader", "res/shaders/tonemapping/tonemapping.fs.shader");

		//promptFont = FontManager.GetFont("baskerville", 28.0f, true);
		//xpFont = FontManager.GetFont("baskerville", 20.0f, true);
		//notificationFont = FontManager.GetFont("baskerville", 18.0f, true);
		//stackSizeFont = FontManager.GetFont("baskerville", 20.0f, true);
		//victoryFont = FontManager.GetFont("baskerville", 80, true);
		//uiFontMedium = FontManager.GetFont("baskerville", 20, true);

		particleBatch = new SpriteBatch(graphics);

		GUI.Init(graphics);
	}

	public static void DrawModel(Model model, Matrix transform, Animator animator = null)
	{
		models.Add(new ModelDrawCommand { model = model, meshID = -1, transform = transform, animator = animator });
	}

	public static void DrawSubModel(Model model, int meshID, Matrix transform)
	{
		models.Add(new ModelDrawCommand { model = model, meshID = meshID, transform = transform });
	}

	public static void DrawModelStaticInstanced_(Model model, Matrix transform)
	{
		modelsInstanced.Add(model, new ModelDrawCommand { model = model, meshID = -1, transform = transform, animator = null });
		/*
		if (!modelsInstanced.TryGetValue(model, out List<ModelDrawCommand> drawList))
		{
			drawList = new List<ModelDrawCommand>();
			modelsInstanced.Add(model, drawList);
		}
		drawList.Add(new ModelDrawCommand { model = model, meshID = -1, transform = transform, animator = null });
		*/
	}

	public static void DrawSubModelStaticInstanced_(Model model, int meshID, Matrix transform)
	{
		modelsInstanced.Add(model, new ModelDrawCommand { model = model, meshID = meshID, transform = transform, animator = null });
		/*
		if (!modelsInstanced.TryGetValue(model, out List<ModelDrawCommand> drawList))
		{
			drawList = new List<ModelDrawCommand>();
			modelsInstanced.Add(model, drawList);
		}
		drawList.Add(new ModelDrawCommand { model = model, meshID = meshID, transform = transform, animator = null });
		*/
	}

	public static void DrawTerrain(Texture heightmap, Texture normalmap, Texture splatMap, Matrix transform, Texture diffuse0, Texture diffuse1, Texture diffuse2, Texture diffuse3)
	{
		terrains.Add(new TerrainDrawCommand { heightmap = heightmap, normalmap = normalmap, splatMap = splatMap, transform = transform, diffuse0 = diffuse0, diffuse1 = diffuse1, diffuse2 = diffuse2, diffuse3 = diffuse3 });
	}

	public static void DrawLeaves(Model model, int meshID, Matrix transform)
	{
		foliage.Add(new LeaveDrawCommand { model = model, meshID = meshID, transform = transform });
	}

	public static void DrawLight(Vector3 position, Vector3 color)
	{
		lights.Add(new LightDrawCommand { position = position, color = color });
	}

	public static void DrawPointLight(PointLight light)
	{
		pointLights.Add(light);
	}

	public static void DrawDirectionalLight(DirectionalLight light)
	{
		directionalLights.Add(light);
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

	public static void DrawParticleSystem(Particle[] particles, List<int> particleIndices, Matrix transform, Vector3 spawnOffset, bool follow, Texture textureAtlas, Vector2i atlasSize, bool linearFiltering, bool additive)
	{
		(additive ? particleSystemsAdditive : particleSystems).Add(new ParticleSystemDrawCommand { particles = particles, particleIndices = particleIndices, transform = transform, spawnOffset = spawnOffset, follow = follow, textureAtlas = textureAtlas, atlasSize = atlasSize, linearFiltering = linearFiltering, additive = additive });
	}

	public static void DrawGrassPatch(Terrain terrain, Vector2 position)
	{
		grassPatches.Add(new GrassDrawCommand { position = position, terrain = terrain });
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

	static void GetFrustumPlanes(Matrix matrix, Span<Vector4> planes)
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
			float sl = planes[i].x * planes[i].x + planes[i].y * planes[i].y + planes[i].z * planes[i].z;
			float ll = MathF.Sqrt(sl);
			float l = 1.0f / ll;
			planes[i].x *= l;
			planes[i].y *= l;
			planes[i].z *= l;
			planes[i].w *= l;
		}
	}

	static bool IsInFrustum(Vector3 p, float radius, Matrix transform, Matrix pv)
	{
		Span<Vector4> planes = stackalloc Vector4[16];
		GetFrustumPlanes(pv, planes);

		Vector4 boundingSpherePos = (transform * new Vector4(p, 1.0f));
		float boundingSphereRadius = MathF.Sqrt(transform.m00 * transform.m00 + transform.m01 * transform.m01 + transform.m02 * transform.m02) * radius;

		for (int i = 0; i < 6; i++)
		{
			float distance = boundingSpherePos.x * planes[i].x + boundingSpherePos.y * planes[i].y + boundingSpherePos.z * planes[i].z;
			float l = MathF.Sqrt(planes[i].x * planes[i].x + planes[i].y * planes[i].y + planes[i].z * planes[i].z);
			distance += planes[i].w / l;
			if (distance + boundingSphereRadius < 0.0f)
				return false;
		}
		return true;
	}

	static bool IsOccluded()
	{
		// TODO occlusion culling
		return false;
	}

	static bool IsInRange(Vector3 p, float radius, Matrix meshTransform, float maxDistance)
	{
		Vector3 d = p + meshTransform.translation - camera.position;
		float distanceSq = Vector3.Dot(d, d);
		return distanceSq < maxDistance * maxDistance;
	}

	static void SubmitMesh(Model model, int meshID, Animator animator, Shader shader, Shader animShader, Matrix transform, Matrix pv)
	{
		MeshData meshData = model.getMeshData(meshID).Value;
		Matrix meshTransform = transform;
		if (meshData.nodeID != -1)
			meshTransform *= GetNodeTransform(model.skeleton.getNode(meshData.nodeID));
		bool isAnimated = meshData.hasSkeleton && animator != null && animShader != null;

		if (IsInFrustum(meshData.boundingSphere.center, meshData.boundingSphere.radius * (isAnimated ? 3.0f : 1.0f), meshTransform, pv))
		{
			if (!IsOccluded())
			{
				if (IsInRange(meshData.boundingSphere.center, meshData.boundingSphere.radius * (isAnimated ? 3.0f : 1.0f), meshTransform, model.maxDistance))
				{
					if (isAnimated)
						model.drawMeshAnimated(graphics, meshID, animShader, animator, meshTransform);
					else
						model.drawMesh(graphics, meshID, shader, meshTransform);

					meshRenderCounter++;
				}
				else
				{
					graphics.resetState(); // reset state so we can submit new draw call data
					meshCulledCounter++;
				}
			}
			else
			{
				graphics.resetState(); // reset state so we can submit new draw call data
				meshCulledCounter++;
			}
		}
		else
		{
			graphics.resetState(); // reset state so we can submit new draw call data
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
			CullState cullState = CullState.ClockWise;

			if (meshID != -1)
			{
				graphics.setCullState(cullState);

				SubmitMesh(model, meshID, animator, modelShader, modelAnimShader, transform, pv);
			}
			else
			{
				for (int j = 0; j < model.meshCount; j++)
				{
					graphics.setCullState(cullState);

					SubmitMesh(model, j, animator, modelShader, modelAnimShader, transform, pv);
				}
			}
		}
	}

	static int CountInstances(SortedList<Model, ModelDrawCommand> draws, int offset)
	{
		Model firstModel = draws.GetKeyAtIndex(offset);
		for (int i = offset; i < draws.Count; i++)
		{
			if (draws.GetKeyAtIndex(i) != firstModel)
				return i - offset;
		}
		return draws.Count - offset;
	}

	static void RenderModelsInstanced()
	{
		graphics.resetState();
		graphics.setViewTransform(projection, view);

		for (int i = 0; i < modelsInstanced.Count; i++)
		{
			ModelDrawCommand draw = modelsInstanced.GetValueAtIndex(i);
			Model model = draw.model;

			int instanceCount = CountInstances(modelsInstanced, i);
			for (int j = 0; j < model.meshCount; j++)
			{
				graphics.createInstanceBuffer(instanceCount, 16 * sizeof(float), out InstanceBufferData instances);

				int numDrawnInstances = 0;
				for (int k = 0; k < instanceCount; k++)
				{
					draw = modelsInstanced.GetValueAtIndex(i + k);
					if (draw.meshID != -1 && draw.meshID != j)
						continue;

					BoundingSphere boundingSphere = model.boundingSphere.Value;
					if (numDrawnInstances < instanceTransformBuffer.Length)
					{
						Matrix transform = draw.transform;
						// TODO fix model duplication glitch with frustum culling
						//if (IntersectsFrustum(new Vector3(boundingSphere.xcenter, boundingSphere.ycenter, boundingSphere.zcenter), boundingSphere.radius, transform, pv))
						instanceTransformBuffer[numDrawnInstances++] = transform;
					}
					else
					{
						Console.WriteLine("Overflowing instance transform buffer of size " + instanceTransformBuffer.Length);
						break;
					}
				}

				instances.write(instanceTransformBuffer);


				graphics.setCullState(CullState.ClockWise);

				graphics.setInstanceBuffer(instances, 0, numDrawnInstances);

				model.drawMesh(graphics, j, meshInstancedShader, Matrix.Identity);
				meshRenderCounter += numDrawnInstances;
				meshCulledCounter += instanceCount - numDrawnInstances;
			}
			i += instanceCount - 1;
		}
	}

	static void RenderTerrains()
	{
		graphics.resetState();
		graphics.setViewTransform(projection, view);

		// TODO frustum culling
		for (int i = 0; i < terrains.Count; i++)
		{
			Matrix transform = terrains[i].transform;
			Vector3 center = transform.translation + 0.5f * new Vector3(TERRAIN_SIZE, 0.0f, TERRAIN_SIZE);
			float distance = Vector3.Distance(center, camera.position);
			int lod = distance < TERRAIN_SIZE ? 0 : distance < 2 * TERRAIN_SIZE ? 1 : distance < 4 * TERRAIN_SIZE ? 2 : 3;

			graphics.setCullState(CullState.ClockWise);

			graphics.setTexture(terrainShader.getUniform("s_heightMap", UniformType.Sampler), 0, terrains[i].heightmap);
			graphics.setTexture(terrainShader.getUniform("s_normalMap", UniformType.Sampler), 1, terrains[i].normalmap);
			graphics.setUniform(terrainShader.getUniform("u_terrainScale", UniformType.Vector4), new Vector4(TERRAIN_SIZE, 1.0f, TERRAIN_SIZE, 0.0f));

			graphics.setTexture(terrainShader.getUniform("s_splatMap", UniformType.Sampler), 2, terrains[i].splatMap);

			graphics.setTexture(terrainShader, "s_diffuse0", 3, terrains[i].diffuse0);
			graphics.setUniform(terrainShader, "u_materialInfo0", new Vector4(terrains[i].diffuse0 != null ? 1.0f : 0.0f, 0.0f, 0.0f, 0.25f));

			graphics.setTexture(terrainShader, "s_diffuse1", 6, terrains[i].diffuse1);
			graphics.setUniform(terrainShader, "u_materialInfo1", new Vector4(terrains[i].diffuse1 != null ? 1.0f : 0.0f, 0.0f, 0.0f, 0.25f));

			graphics.setTexture(terrainShader, "s_diffuse2", 9, terrains[i].diffuse2);
			graphics.setUniform(terrainShader, "u_materialInfo2", new Vector4(terrains[i].diffuse2 != null ? 1.0f : 0.0f, 0.0f, 0.0f, 0.25f));

			graphics.setTexture(terrainShader, "s_diffuse3", 12, terrains[i].diffuse3);
			graphics.setUniform(terrainShader, "u_materialInfo3", new Vector4(terrains[i].diffuse3 != null ? 1.0f : 0.0f, 0.0f, 0.0f, 0.25f));

			graphics.setVertexBuffer(terrainMeshes[lod]);
			graphics.setIndexBuffer(terrainMeshesIndices[lod]);

			graphics.setTransform(transform);

			graphics.draw(terrainShader);

			meshRenderCounter++;
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
			model.drawMesh(graphics, meshID, foliageShader, transform);
		}
	}

	static void RenderGrass()
	{
		if (grassPatches.Count > 0)
		{
			// TODO remove duplicate allocation
			graphics.createInstanceBuffer(NUM_GRASS_BLADES, 16, out InstanceBufferData grassInstances);
			grassInstances.write(grassData);

			for (int i = 0; i < grassPatches.Count; i++)
			{
				graphics.resetState();

				graphics.setBlendState(BlendState.Default);
				graphics.setCullState(CullState.None);

				graphics.setViewTransform(projection, view);

				graphics.setTransform(Matrix.CreateTranslation(grassPatches[i].position.x, 0.0f, grassPatches[i].position.y));

				graphics.setUniform(grassShader.getUniform("u_animationData", UniformType.Vector4), new Vector4(grassPatches[i].position - grassPatches[i].terrain.position.xz, grassPatches[i].terrain.size, Time.currentTime / 1e9f));

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

		models.Sort((ModelDrawCommand a, ModelDrawCommand b) =>
		{
			float d1 = (a.transform.translation - camera.position).lengthSquared;
			float d2 = (b.transform.translation - camera.position).lengthSquared;
			return d1 < d2 ? -1 : d1 > d2 ? 1 : 0;
		});

		RenderModels();
		RenderModelsInstanced();
		//RenderTerrains();
		//RenderFoliage();
		//RenderGrass();
	}

	static void UpdateDirectionalShadows()
	{
		if (directionalLights.Count > 0)
		{
			if (!directionalLights[0].shadowMap.needsUpdate && !Input.IsKeyPressed(KeyCode.F5))
				return;
			directionalLights[0].shadowMap.needsUpdate = false;

			DirectionalShadowMap shadowMap = directionalLights[0].shadowMap;

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
					if (!model.isStatic)
						continue;

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

					model.drawMesh(graphics, meshID, foliageShader, transform);
				}
			}
		}
	}

	static void UpdatePointShadows()
	{
		int numUpdatedPointShadows = 0;
		for (int h = 0; h < pointLights.Count && numUpdatedPointShadows < MAX_POINT_SHADOWS; h++)
		{
			if (!pointLights[h].shadowMap.needsUpdate && !Input.IsKeyPressed(KeyCode.F5))
				continue;
			pointLights[h].shadowMap.needsUpdate = false;

			PointShadowMap shadowMap = pointLights[h].shadowMap;

			for (int i = 0; i < shadowMap.renderTargets.Length; i++)
			{
				graphics.resetState();
				graphics.setPass((int)RenderPass.PointShadow + numUpdatedPointShadows * 6 + i);

				graphics.setRenderTarget(shadowMap.renderTargets[i]);

				Matrix shadowMapProjection = Matrix.CreatePerspective(MathF.PI * 0.5f, 1.0f, shadowMap.nearPlane, 30.0f);
				Matrix shadowMapView = cubemapFaceRotations[i] * Matrix.CreateTranslation(-pointLights[h].position);
				Matrix reflectionProbePV = shadowMapProjection * shadowMapView;
				graphics.setViewTransform(shadowMapProjection, shadowMapView);

				for (int j = 0; j < models.Count; j++)
				{
					Model model = models[j].model;
					if (!model.isStatic)
						continue;

					Animator animator = models[j].animator;
					Matrix transform = models[j].transform;
					//graphics.drawModel(model, modelDepthShader, modelAnimDepthShader, animator, transform);

					for (int k = 0; k < models[j].model.meshCount; k++)
					{
						graphics.setCullState(CullState.None);

						SubmitMesh(model, k, animator, modelDepthShader, modelAnimDepthShader, transform, reflectionProbePV);
					}
				}
			}

			numUpdatedPointShadows++;
		}
	}

	static void ShadowPass()
	{
		UpdateDirectionalShadows();
		UpdatePointShadows();
	}

	static void ReflectionProbePass()
	{
		Span<Vector4> lightPositionBuffer = stackalloc Vector4[MAX_LIGHTS_PER_PASS];
		Span<Vector4> lightColorBuffer = stackalloc Vector4[MAX_LIGHTS_PER_PASS];

		for (int i = 0; i < reflectionProbes.Count; i++)
		{
			if (!reflectionProbes[i].reflectionProbe.needsUpdate && !Input.IsKeyPressed(KeyCode.F5))
				continue;
			reflectionProbes[i].reflectionProbe.needsUpdate = false;

			for (int j = 0; j < 6; j++)
			{
				graphics.resetState();
				graphics.setPass((int)RenderPass.ReflectionProbe + i * 6 + j);

				graphics.setRenderTarget(reflectionProbes[i].reflectionProbe.renderTargets[j], 0xff00ffff);

				Matrix reflectionProbeProjection = Matrix.CreatePerspective(MathF.PI * 0.5f, 1.0f, 0.1f, 100.0f);
				Matrix reflectionProbeView = cubemapFaceRotations[j] * Matrix.CreateTranslation(-reflectionProbes[i].origin);
				Matrix reflectionProbePV = reflectionProbeProjection * reflectionProbeView;
				graphics.setViewTransform(reflectionProbeProjection, reflectionProbeView);

				{
					graphics.setUniform(modelSimpleShader, "u_cameraPosition", new Vector4(reflectionProbes[i].origin, 0.0f));

					for (int k = 0; k < models.Count; k++)
					{
						if (!models[k].model.isStatic)
							continue;

						for (int l = 0; l < models[k].model.meshCount; l++)
						{
							graphics.setCullState(CullState.CounterClockWise);

							if (directionalLights.Count > 0)
							{
								graphics.setUniform(modelSimpleShader.getUniform("u_directionalLightDirection", UniformType.Vector4), new Vector4(directionalLights[0].direction, 0.0f));
								graphics.setUniform(modelSimpleShader.getUniform("u_directionalLightColor", UniformType.Vector4), new Vector4(directionalLights[0].color, 0.0f));

								graphics.setUniform(modelSimpleShader.getUniform("u_directionalLightFarPlane", UniformType.Vector4), new Vector4(DirectionalShadowMap.FAR_PLANES[2], 0.0f, 0.0f, 0.0f));

								DirectionalShadowMap shadowMap = directionalLights[0].shadowMap;
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

								graphics.setUniform(modelSimpleShader.getUniform("u_directionalLightFarPlane", UniformType.Vector4), new Vector4(DirectionalShadowMap.FAR_PLANES[2], 0.0f, 0.0f, 0.0f));

								graphics.setTexture(modelSimpleShader.getUniform("s_directionalLightShadowMap", UniformType.Sampler), 5, emptyShadowTexture);
								graphics.setUniform(modelSimpleShader.getUniform("u_directionalLightToLightSpace", UniformType.Matrix4), Matrix.Identity);
							}


							for (int m = 0; m < MAX_LIGHTS_PER_PASS; m++)
							{
								int lightID = m;
								lightPositionBuffer[j] = lightID < lights.Count ? new Vector4(lights[lightID].position, 0.0f) : new Vector4(0.0f);
								lightColorBuffer[j] = lightID < lights.Count ? new Vector4(lights[lightID].color, 0.0f) : new Vector4(0.0f);
							}
							graphics.setUniform(modelSimpleShader.getUniform("u_lightPosition", UniformType.Vector4, MAX_LIGHTS_PER_PASS), lightPositionBuffer);
							graphics.setUniform(modelSimpleShader.getUniform("u_lightColor", UniformType.Vector4, MAX_LIGHTS_PER_PASS), lightColorBuffer);

							//graphics.drawSubModel(models[k].model, l, modelSimpleShader, models[k].transform);
							SubmitMesh(models[k].model, l, null, modelSimpleShader, null, models[k].transform, reflectionProbePV);
						}
					}
				}

				for (int k = 0; k < skies.Count; k++)
				{
					graphics.setCullState(CullState.None);

					graphics.setVertexBuffer(skydome);
					graphics.setIndexBuffer(skydomeIdx);

					graphics.setTransform(skies[k].transform);

					graphics.setViewTransform(reflectionProbeProjection, reflectionProbeView);

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
		if (!ambientOcclusionEnabled)
			return;

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

	static void RenderPointLights()
	{
		Span<Vector4> lightPositionBuffer = stackalloc Vector4[MAX_LIGHTS_PER_PASS];
		Span<Vector4> lightColorBuffer = stackalloc Vector4[MAX_LIGHTS_PER_PASS];

		Shader shader = simplifiedLighting ? deferredPointSimpleShader : deferredPointShader;

		int maxLights = 32;
		for (int i = 0; i < Math.Min(lights.Count, maxLights); i++)
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

		Span<Vector4> pointLightPositionBuffer = stackalloc Vector4[MAX_POINT_SHADOWS];
		Span<Vector4> pointLightColorBuffer = stackalloc Vector4[MAX_POINT_SHADOWS];
		Span<float> pointLightShadowNears = stackalloc float[MAX_POINT_SHADOWS];
		Span<byte> uniformName = stackalloc byte[32];
		if (pointLights.Count > 0)
		{
			shader = simplifiedLighting ? deferredPointShadowSimpleShader : deferredPointShadowShader;

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

			int numRemainingLights = Math.Min(pointLights.Count, MAX_POINT_SHADOWS);
			for (int j = 0; j < numRemainingLights; j++)
			{
				pointLightPositionBuffer[j] = new Vector4(pointLights[j].position, 0.0f);
				pointLightColorBuffer[j] = new Vector4(pointLights[j].color, 0.0f);
				pointLightShadowNears[j] = pointLights[j].shadowMap.nearPlane;

				StringUtils.WriteString(uniformName, "s_lightShadowMap");
				StringUtils.AppendInteger(uniformName, j);
				graphics.setTexture(shader, uniformName, 5 + j, pointLights[j].shadowMap.cubemap);
			}

			graphics.setUniform(shader.getUniform("u_lightPosition", UniformType.Vector4, MAX_POINT_SHADOWS), pointLightPositionBuffer);
			graphics.setUniform(shader.getUniform("u_lightColor", UniformType.Vector4, MAX_POINT_SHADOWS), pointLightColorBuffer);

			for (int i = 0; i < MAX_POINT_SHADOWS / 4; i++)
			{
				StringUtils.WriteString(uniformName, "u_lightShadowMapNear");
				StringUtils.AppendInteger(uniformName, i);
				graphics.setUniform(shader, uniformName, new Vector4(pointLightShadowNears[i * 4 + 0], pointLightShadowNears[i * 4 + 1], pointLightShadowNears[i * 4 + 2], pointLightShadowNears[i * 4 + 3]));
			}

			graphics.draw(shader);
		}
	}

	static void RenderDirectionalLights()
	{
		Span<byte> directionalLightShadowMapUniform = stackalloc byte[32];
		Span<byte> directionalLightToLightSpaceUniform = stackalloc byte[32];


		for (int i = 0; i < directionalLights.Count; i++)
		{
			graphics.resetState();

			graphics.setBlendState(BlendState.Additive);
			graphics.setDepthTest(DepthTest.None);
			graphics.setCullState(CullState.ClockWise);

			graphics.setVertexBuffer(quad);

			graphics.setTexture(deferredDirectionalShader.getUniform("s_gbuffer0", UniformType.Sampler), 0, gbuffer.getAttachmentTexture(0));
			graphics.setTexture(deferredDirectionalShader.getUniform("s_gbuffer1", UniformType.Sampler), 1, gbuffer.getAttachmentTexture(1));
			graphics.setTexture(deferredDirectionalShader.getUniform("s_gbuffer2", UniformType.Sampler), 2, gbuffer.getAttachmentTexture(2));
			graphics.setTexture(deferredDirectionalShader.getUniform("s_gbuffer3", UniformType.Sampler), 3, gbuffer.getAttachmentTexture(3));

			graphics.setTexture(deferredDirectionalShader.getUniform("s_ambientOcclusion", UniformType.Sampler), 4, ssaoBlurRenderTarget.getAttachmentTexture(0));

			graphics.setUniform(deferredDirectionalShader, "u_cameraPosition", new Vector4(camera.position, (Time.currentTime / 5 % 1000000000) / 1e9f));

			graphics.setUniform(deferredDirectionalShader, "u_directionalLightDirection", new Vector4(directionalLights[i].direction, 0.0f));
			graphics.setUniform(deferredDirectionalShader, "u_directionalLightColor", new Vector4(directionalLights[i].color, 0.0f));

			graphics.setUniform(deferredDirectionalShader, "u_directionalLightCascadeFarPlanes", new Vector4(DirectionalShadowMap.FAR_PLANES[0], DirectionalShadowMap.FAR_PLANES[1], DirectionalShadowMap.FAR_PLANES[2], 0.0f));

			DirectionalShadowMap shadowMap = directionalLights[i].shadowMap;
			for (int j = 0; j < shadowMap.renderTargets.Length; j++)
			{
				RenderTarget renderTarget = shadowMap.renderTargets[j];
				Matrix toLightSpace = shadowMap.cascadeProjections[j] * shadowMap.cascadeViews[j];

				StringUtils.WriteStringDigit(directionalLightShadowMapUniform, "s_directionalLightShadowMap", j);
				StringUtils.WriteStringDigit(directionalLightToLightSpaceUniform, "u_directionalLightToLightSpace", j);

				graphics.setTexture(deferredDirectionalShader, directionalLightShadowMapUniform, 10 + j, renderTarget.getAttachmentTexture(0));
				graphics.setUniform(deferredDirectionalShader, directionalLightToLightSpaceUniform, toLightSpace);
			}

			graphics.draw(deferredDirectionalShader);
		}
	}

	static void RenderEnvironmentLights()
	{
		Span<byte> reflectionProbeUniform = stackalloc byte[32];
		Span<byte> reflectionProbePositionUniform = stackalloc byte[32];
		Span<byte> reflectionProbeSizeUniform = stackalloc byte[32];
		Span<byte> reflectionProbeOriginUniform = stackalloc byte[32];


		Shader shader = simplifiedLighting ? deferredEnvironmentSimpleShader : deferredEnvironmentShader;

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


		Vector4 environmentMapIntensities = new Vector4(environmentMapIntensity, 0.0f, 0.0f, 0.0f);
		graphics.setUniform(shader.getUniform("u_environmentMapIntensities", UniformType.Vector4), environmentMapIntensities);
		if (environmentMap != null)
		{
			graphics.setTexture(shader.getUniform("s_environmentMap", UniformType.Sampler), 5, environmentMap);
		}
		else
		{
			graphics.setTexture(shader.getUniform("s_environmentMap", UniformType.Sampler), 5, emptyCubemap);
		}


		reflectionProbes.Sort((ReflectionProbeDrawCommand cmd0, ReflectionProbeDrawCommand cmd1) =>
		{
			float distance0 = (cmd0.position - camera.position).length;
			float distance1 = (cmd1.position - camera.position).length;
			return distance0 < distance1 ? -1 : distance0 > distance1 ? 1 : 0;
		});

		for (int j = 0; j < MAX_REFLECTION_PROBES; j++)
		{
			StringUtils.WriteStringDigit(reflectionProbeUniform, "s_reflectionProbe", j);
			StringUtils.WriteStringDigit(reflectionProbePositionUniform, "u_reflectionProbePosition", j);
			StringUtils.WriteStringDigit(reflectionProbeSizeUniform, "u_reflectionProbeSize", j);
			StringUtils.WriteStringDigit(reflectionProbeOriginUniform, "u_reflectionProbeOrigin", j);

			if (j < reflectionProbes.Count)
			{
				graphics.setTexture(shader, reflectionProbeUniform, 6 + j, reflectionProbes[j].reflectionProbe.cubemap);
				graphics.setUniform(shader, reflectionProbePositionUniform, new Vector4(reflectionProbes[j].position, 0.0f));
				graphics.setUniform(shader, reflectionProbeSizeUniform, new Vector4(reflectionProbes[j].size, 0.0f));
				graphics.setUniform(shader, reflectionProbeOriginUniform, new Vector4(reflectionProbes[j].origin, 0.0f));
			}
			else
			{
				graphics.setTexture(shader, reflectionProbeUniform, 6 + j, emptyCubemap);
				graphics.setUniform(shader, reflectionProbePositionUniform, new Vector4(0.0f, -100000.0f, 0.0f, 0.0f));
				graphics.setUniform(shader, reflectionProbeSizeUniform, Vector4.One);
				graphics.setUniform(shader, reflectionProbeOriginUniform, Vector4.Zero);
			}
		}

		graphics.draw(shader);
	}

	static void DeferredPass()
	{
		graphics.setPass((int)RenderPass.Deferred);
		graphics.setRenderTarget(forward);

		RenderPointLights();
		RenderDirectionalLights();
		RenderEnvironmentLights();
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

	[StructLayout(LayoutKind.Sequential)]
	struct ParticleInstanceData
	{
		public Vector4 positionRotation;
		public Vector4 color;
		public Vector4 sizeAnimation;
	}

	static int ParticleSystemDepthComparator(ParticleSystemDrawCommand particleSystem1, ParticleSystemDrawCommand particleSystem2)
	{
		Vector3 cameraPosition = camera.position;
		Vector3 cameraAxis = camera.rotation.forward;
		float d1 = Vector3.Dot(particleSystem1.transform.translation - cameraPosition, cameraAxis);
		float d2 = Vector3.Dot(particleSystem2.transform.translation - cameraPosition, cameraAxis);

		return d1 < d2 ? 1 : d1 > d2 ? -1 : 0;
	}

	static void RenderParticles()
	{
		/*
		{
			graphics.resetState();
			//particleSystems.Sort(systemDepthComparator);

			int totalParticleCount = 0;
			foreach (ParticleSystemDrawCommand draw in particleSystems)
				totalParticleCount += draw.numParticles;

			particleBatch.begin(totalParticleCount);
			foreach (ParticleSystemDrawCommand draw in particleSystems)
			{
				float scale = draw.transform.scale.x;
				Vector3 globalSpawnPos = (draw.transform * new Vector4(draw.spawnOffset, 1.0f)).xyz;

				for (int i = 0; i < draw.numParticles; i++)
				{
					Particle particle = draw.particles[i];
					if (particle.id != -1)
					{
						Vector3 position = particle.position;
						float size = scale * particle.size;

						if (draw.followMode == ParticleFollowMode.Follow)
							position = globalSpawnPos + particle.position * scale;

						float u0 = draw.textureAtlas != null ? particle.u0 / (float)draw.textureAtlas.info.width : 0.0f;
						float v0 = draw.textureAtlas != null ? particle.v0 / (float)draw.textureAtlas.info.height : 0.0f;
						float u1 = draw.textureAtlas != null ? particle.u1 / (float)draw.textureAtlas.info.width : 0.0f;
						float v1 = draw.textureAtlas != null ? particle.v1 / (float)draw.textureAtlas.info.height : 0.0f;
						particleBatch.drawBillboard(position.x, position.y, position.z,
							size, size,
							particle.rotation,
							draw.textureAtlas, uint.MaxValue,
							u0, v0, u1, v1,
							particle.color);
					}
				}
			}
			particleBatch.end();


			Span<Vector4> pointLightPositions = stackalloc Vector4[MAX_LIGHTS_PER_PASS];
			Span<Vector4> pointLightColors = stackalloc Vector4[MAX_LIGHTS_PER_PASS];

			for (int i = 0; i < particleBatch.getNumDrawCalls(); i++)
			{
				graphics.setBlendState(BlendState.Alpha);
				graphics.setViewTransform(projection, view);

				for (int j = 0; j < Math.Min(lights.Count, MAX_LIGHTS_PER_PASS); j++)
				{
					pointLightPositions[j] = new Vector4(lights[j].position, 1.0f);
					pointLightColors[j] = new Vector4(lights[j].color, 1.0f);
				}
				graphics.setUniform(particleShader.getUniform("u_pointLight_position", UniformType.Vector4, MAX_LIGHTS_PER_PASS), pointLightPositions);
				graphics.setUniform(particleShader.getUniform("u_pointLight_color", UniformType.Vector4, MAX_LIGHTS_PER_PASS), pointLightColors);
				graphics.setUniform(particleShader, "u_lightInfo", new Vector4(lights.Count + 0.5f, 0.0f, 0.0f, 0.0f));

				particleBatch.submitDrawCall(i, particleShader);
			}
		}
		*/


		{
			Span<Vector4> pointLightPositions = stackalloc Vector4[MAX_LIGHTS_PER_PASS];
			Span<Vector4> pointLightColors = stackalloc Vector4[MAX_LIGHTS_PER_PASS];

			graphics.resetState();

			particleSystems.Sort(ParticleSystemDepthComparator);

			int maxParticleSystems = 32;
			float maxParticleDistance = 20.0f;
			Vector3 cameraPosition = camera.position;
			Vector3 cameraAxis = camera.rotation.forward;
			for (int i = 0; i < particleSystems.Count; i++)
			{
				Vector3 toParticles = particleSystems[i].transform.translation - cameraPosition;
				float d = Vector3.Dot(toParticles, cameraAxis);
				float l2 = Vector3.Dot(toParticles, toParticles);
				if (d < 0.0f || l2 > maxParticleDistance * maxParticleDistance)
				{
					particleSystems.RemoveAt(i--);
				}
			}
			if (particleSystems.Count > maxParticleSystems)
				particleSystems.RemoveRange(0, particleSystems.Count - maxParticleSystems);

			graphics.setUniform(particleShader, "u_cameraAxisRight", new Vector4(camera.rotation.right, 1.0f));
			graphics.setUniform(particleShader, "u_cameraAxisUp", new Vector4(camera.rotation.up, 1.0f));

			foreach (ParticleSystemDrawCommand draw in particleSystems)
			{
				int numParticles = draw.particleIndices.Count;
				graphics.createInstanceBuffer(numParticles, 12 * sizeof(float), out InstanceBufferData particleInstanceBuffer);

				float scale = draw.transform.scale.x;
				Vector3 globalSpawnPos = (draw.transform * new Vector4(draw.spawnOffset, 1.0f)).xyz;

				if (draw.textureAtlas != null)
					graphics.setTexture(particleShader, "s_textureAtlas", 0, draw.textureAtlas, draw.linearFiltering ? 0 : (uint)SamplerFlags.Point);
				graphics.setUniform(particleShader, "u_atlasSize", new Vector4(draw.atlasSize.x, draw.atlasSize.y, draw.textureAtlas != null ? 1.0f : 0.0f, 0.0f));

				for (int i = 0; i < numParticles; i++)
				{
					int particleID = draw.particleIndices[i];
					Particle particle = draw.particles[particleID];
					Debug.Assert(particle.active);

					Vector3 position = particle.position;
					float size = scale * particle.size;

					if (draw.follow)
						position = globalSpawnPos + particle.position * scale;

					unsafe
					{
						ParticleInstanceData* particleData = particleInstanceBuffer.getData<ParticleInstanceData>();
						particleData[i].positionRotation.xyz = position;
						particleData[i].positionRotation.w = particle.rotation;
						particleData[i].color = particle.color;
						particleData[i].sizeAnimation.x = size;
						particleData[i].sizeAnimation.y = particle.animationFrame;
					}
				}

				graphics.setBlendState(BlendState.Alpha);
				graphics.setViewTransform(projection, view);

				graphics.setInstanceBuffer(particleInstanceBuffer, 0, numParticles);
				graphics.setVertexBuffer(particleVertexBuffer);
				graphics.setIndexBuffer(particleIndexBuffer);

				for (int j = 0; j < Math.Min(lights.Count, MAX_LIGHTS_PER_PASS); j++)
				{
					pointLightPositions[j] = new Vector4(lights[j].position, 1.0f);
					pointLightColors[j] = new Vector4(lights[j].color, 1.0f);
				}
				graphics.setUniform(particleShader.getUniform("u_pointLight_position", UniformType.Vector4, MAX_LIGHTS_PER_PASS), pointLightPositions);
				graphics.setUniform(particleShader.getUniform("u_pointLight_color", UniformType.Vector4, MAX_LIGHTS_PER_PASS), pointLightColors);
				graphics.setUniform(particleShader, "u_lightInfo", new Vector4(lights.Count + 0.5f, 0.0f, 0.0f, 0.0f));


				graphics.draw(particleShader);
			}
		}

		{
			graphics.resetState();

			particleSystemsAdditive.Sort(ParticleSystemDepthComparator);

			int maxParticleSystems = 32;
			float maxParticleDistance = 20.0f;
			Vector3 cameraPosition = camera.position;
			Vector3 cameraAxis = camera.rotation.forward;
			for (int i = 0; i < particleSystemsAdditive.Count; i++)
			{
				Vector3 toParticles = particleSystemsAdditive[i].transform.translation - cameraPosition;
				float d = Vector3.Dot(toParticles, cameraAxis);
				float l2 = Vector3.Dot(toParticles, toParticles);
				if (d < 0.0f || l2 > maxParticleDistance * maxParticleDistance)
				{
					particleSystemsAdditive.RemoveAt(i--);
				}
			}
			if (particleSystemsAdditive.Count > maxParticleSystems)
				particleSystemsAdditive.RemoveRange(0, particleSystemsAdditive.Count - maxParticleSystems);

			graphics.setUniform(particleAdditiveShader, "u_cameraAxisRight", new Vector4(camera.rotation.right, 1.0f));
			graphics.setUniform(particleAdditiveShader, "u_cameraAxisUp", new Vector4(camera.rotation.up, 1.0f));

			foreach (ParticleSystemDrawCommand draw in particleSystemsAdditive)
			{
				int numParticles = draw.particleIndices.Count;
				graphics.createInstanceBuffer(numParticles, 12 * sizeof(float), out InstanceBufferData particleInstanceBuffer);

				float scale = draw.transform.scale.x;
				Vector3 globalSpawnPos = (draw.transform * new Vector4(draw.spawnOffset, 1.0f)).xyz;

				if (draw.textureAtlas != null)
					graphics.setTexture(particleAdditiveShader, "s_textureAtlas", 0, draw.textureAtlas, draw.linearFiltering ? 0 : (uint)SamplerFlags.Point);
				graphics.setUniform(particleAdditiveShader, "u_atlasSize", new Vector4(draw.atlasSize.x, draw.atlasSize.y, draw.textureAtlas != null ? 1.0f : 0.0f, 0.0f));

				for (int i = 0; i < numParticles; i++)
				{
					int particleID = draw.particleIndices[i];
					Particle particle = draw.particles[particleID];
					Debug.Assert(particle.active);

					Vector3 position = particle.position;
					float size = scale * particle.size;

					if (draw.follow)
						position = globalSpawnPos + particle.position * scale;

					unsafe
					{
						ParticleInstanceData* particleData = particleInstanceBuffer.getData<ParticleInstanceData>();
						particleData[i].positionRotation.xyz = position;
						particleData[i].positionRotation.w = particle.rotation;
						particleData[i].color = particle.color;
						particleData[i].sizeAnimation.x = size;
						particleData[i].sizeAnimation.y = particle.animationFrame;
					}
				}

				graphics.setBlendState(BlendState.Additive);
				graphics.setViewTransform(projection, view);

				graphics.setInstanceBuffer(particleInstanceBuffer, 0, numParticles);
				graphics.setVertexBuffer(particleVertexBuffer);
				graphics.setIndexBuffer(particleIndexBuffer);

				graphics.draw(particleAdditiveShader);
			}
		}



		/*
		{
			graphics.resetState();
			//particleSystemsAdditive.Sort(systemDepthComparator);

			int totalParticleCount = 0;
			foreach (ParticleSystemDrawCommand draw in particleSystemsAdditive)
				totalParticleCount += draw.numParticles;

			particleBatch.begin(totalParticleCount);
			foreach (ParticleSystemDrawCommand draw in particleSystemsAdditive)
			{
				float scale = draw.transform.scale.x;
				Vector3 globalSpawnPos = (draw.transform * new Vector4(draw.spawnOffset, 1.0f)).xyz;

				for (int i = 0; i < draw.numParticles; i++)
				{
					Particle particle = draw.particles[i];
					if (particle.id != -1)
					{
						Vector3 position = particle.position;
						float size = scale * particle.size;

						if (draw.followMode == ParticleFollowMode.Follow)
							position = globalSpawnPos + particle.position * scale;

						float u0 = draw.textureAtlas != null ? particle.u0 / (float)draw.textureAtlas.info.width : 0.0f;
						float v0 = draw.textureAtlas != null ? particle.v0 / (float)draw.textureAtlas.info.height : 0.0f;
						float u1 = draw.textureAtlas != null ? particle.u1 / (float)draw.textureAtlas.info.width : 0.0f;
						float v1 = draw.textureAtlas != null ? particle.v1 / (float)draw.textureAtlas.info.height : 0.0f;
						particleBatch.drawBillboard(position.x, position.y, position.z,
							size, size,
							particle.rotation,
							draw.textureAtlas, uint.MaxValue,
							u0, v0, u1, v1,
							particle.color);
					}
				}
			}
			particleBatch.end();

			for (int i = 0; i < particleBatch.getNumDrawCalls(); i++)
			{
				graphics.setBlendState(BlendState.Additive);
				graphics.setViewTransform(projection, view);

				particleBatch.submitDrawCall(i, particleAdditiveShader);
			}
		}
		*/
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
			graphics.setUniform(waterShader.getUniform("u_directionalLightDirection", UniformType.Vector4), new Vector4(directionalLights[0].direction, 0.0f));
			graphics.setUniform(waterShader.getUniform("u_directionalLightColor", UniformType.Vector4), new Vector4(directionalLights[0].color, 0.0f));

			graphics.setUniform(waterShader.getUniform("u_directionalLightFarPlane", UniformType.Vector4), new Vector4(DirectionalShadowMap.FAR_PLANES[2], 0.0f, 0.0f, 0.0f));

			DirectionalShadowMap shadowMap = directionalLights[0].shadowMap;
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

			graphics.setUniform(waterShader.getUniform("u_directionalLightFarPlane", UniformType.Vector4), new Vector4(DirectionalShadowMap.FAR_PLANES[2], 0.0f, 0.0f, 0.0f));

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
					if (IsInFrustum(new Vector3(mesh.boundingSphere.xcenter, mesh.boundingSphere.ycenter, mesh.boundingSphere.zcenter), mesh.boundingSphere.radius, transform, pv))
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
		graphics.setPass((int)RenderPass.Forward);
		graphics.setRenderTarget(forward);


		RenderSky();
		RenderParticles();
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

		graphics.setUniform(fogShader.getUniform("u_fogData", UniformType.Vector4), new Vector4(fogColor, fogIntensity));
		graphics.setUniform(fogShader.getUniform("u_cameraFrustum", UniformType.Vector4), new Vector4(camera.near, camera.far, 0.0f, 0.0f));

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

	static Texture Bloom(Texture input)
	{
		BloomDownsample(0, input, bloomDownsampleChain[0]);
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

		return bloomUpsampleChain[0].getAttachmentTexture(0);
	}

	static void Composite(Texture bloom)
	{
		graphics.resetState();
		graphics.setPass((int)RenderPass.Composite);

		graphics.setRenderTarget(compositeRenderTarget);

		graphics.setDepthTest(DepthTest.None);
		graphics.setCullState(CullState.ClockWise);

		graphics.setTexture(compositeShader.getUniform("s_hdrBuffer", UniformType.Sampler), 0, postProcessing.getAttachmentTexture(0));
		if (bloom != null)
			graphics.setTexture(compositeShader.getUniform("s_bloom", UniformType.Sampler), 1, bloom);

		graphics.setUniform(compositeShader, "u_vignetteColor", new Vector4(vignetteColor, vignetteFalloff));

		graphics.setVertexBuffer(quad);
		graphics.draw(compositeShader);
	}

	static void PostProcessing()
	{
		DistanceFog();

		Texture bloom = null;
		if (bloomEnabled)
			bloom = Bloom(postProcessing.getAttachmentTexture(0));

		Composite(bloom);
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

	static int LightDistanceComparator(LightDrawCommand light1, LightDrawCommand light2)
	{
		Vector3 delta1 = light1.position - camera.position;
		Vector3 delta2 = light2.position - camera.position;
		float d1 = Vector3.Dot(delta1, delta1);
		float d2 = Vector3.Dot(delta2, delta2);
		return d1 < d2 ? -1 : d1 > d2 ? 1 : 0;
	}

	static int PointLightDistanceComparator(PointLight light1, PointLight light2)
	{
		Vector3 delta1 = light1.position - camera.position;
		Vector3 delta2 = light2.position - camera.position;
		float d1 = Vector3.Dot(delta1, delta1);
		float d2 = Vector3.Dot(delta2, delta2);
		return d1 < d2 ? -1 : d1 > d2 ? 1 : 0;
	}

	public static void End()
	{
		meshRenderCounter = 0;
		meshCulledCounter = 0;

		lights.Sort(LightDistanceComparator);
		pointLights.Sort(PointLightDistanceComparator);

		GeometryPass();
		ShadowPass();
		ReflectionProbePass();
		AmbientOcclusionPass();
		DeferredPass();

		graphics.blit(forward.getAttachmentTexture(forward.attachmentCount - 1), gbuffer.getAttachmentTexture(gbuffer.attachmentCount - 1));

		ForwardPass();
		PostProcessing();
		TonemappingPass();
		GUI.Draw((int)RenderPass.UI);

		models.Clear();
		modelsInstanced.Clear();
		terrains.Clear();
		foliage.Clear();
		skies.Clear();
		waterTiles.Clear();
		lights.Clear();
		pointLights.Clear();
		directionalLights.Clear();
		reflectionProbes.Clear();
		particleSystems.Clear();
		particleSystemsAdditive.Clear();
		grassPatches.Clear();
	}
}
