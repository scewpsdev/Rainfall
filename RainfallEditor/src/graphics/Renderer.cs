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

	struct DebugLineDrawCommand
	{
		internal Vector3 vertex0;
		internal Vector3 vertex1;
		internal Vector4 color;
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

	static int width, height;

	static RenderTarget gbuffer;
	static RenderTarget forward;
	static VertexBuffer quad;
	static VertexBuffer skydome;
	static IndexBuffer skydomeIdx;

	static Texture emptyShadowTexture;
	static Cubemap emptyCubemap;

	static RenderTarget finalTarget;

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
	static Shader deferredPointShader, deferredPointShadowShader, deferredPointSimpleShader;
	static Shader deferredDirectionalShader;
	static Shader deferredEnvironmentShader, deferredEnvironmentSimpleShader;
	static Shader skyShader;
	static Shader waterShader;
	static Shader particleShader;
	static Shader particleAdditiveShader;
	static Shader lineShader;
	static Shader grassShader;
	static Shader fogShader;
	static Shader bloomDownsampleShader;
	static Shader bloomUpsampleShader;
	static Shader compositeShader;
	static Shader tonemappingShader;

	static SpriteBatch particleBatch;

	static LineRenderer debugLineRenderer;

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

	static List<ModelDrawCommand> models = new List<ModelDrawCommand>();
	static SortedList<Model, ModelDrawCommand> modelsInstanced = new SortedList<Model, ModelDrawCommand>(new ModelComparer());
	static List<SkyDrawCommand> skies = new List<SkyDrawCommand>();
	static List<WaterDrawCommand> waterTiles = new List<WaterDrawCommand>();
	static List<LightDrawCommand> lights = new List<LightDrawCommand>();
	static List<PointLight> pointLights = new List<PointLight>();
	static List<DirectionalLight> directionalLights = new List<DirectionalLight>();
	static List<ReflectionProbeDrawCommand> reflectionProbes = new List<ReflectionProbeDrawCommand>();
	static List<ParticleSystemDrawCommand> particleSystems = new List<ParticleSystemDrawCommand>();
	static List<ParticleSystemDrawCommand> particleSystemsAdditive = new List<ParticleSystemDrawCommand>();
	static List<DebugLineDrawCommand> debugLines = new List<DebugLineDrawCommand>();

	public static int meshRenderCounter = 0;
	public static int meshCulledCounter = 0;


	public static void Init(GraphicsDevice graphics)
	{
		Renderer.graphics = graphics;

		Resize(Display.width, Display.height);

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


		particleVertexBuffer = graphics.createVertexBuffer(graphics.createVideoMemory(stackalloc float[] { -0.5f, -0.5f, 0.0f, 1.0f, 0.5f, -0.5f, 1.0f, 1.0f, 0.5f, 0.5f, 1.0f, 0.0f, -0.5f, 0.5f, 0.0f, 0.0f }),
			stackalloc VertexElement[] { new VertexElement(VertexAttribute.Position, VertexAttributeType.Vector2, false), new VertexElement(VertexAttribute.TexCoord0, VertexAttributeType.Vector2, false) });
		particleIndexBuffer = graphics.createIndexBuffer(graphics.createVideoMemory(stackalloc short[] { 0, 1, 2, 2, 3, 0 }));


		finalTarget = graphics.createRenderTarget(new RenderTargetAttachment[]
		{
			new RenderTargetAttachment(BackbufferRatio.Equal, TextureFormat.BGRA8, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp)
		});


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
		deferredDirectionalShader = Resource.GetShader("res/shaders/deferred/deferred.vs.shader", "res/shaders/deferred/deferred_directional.fs.shader");
		deferredEnvironmentShader = Resource.GetShader("res/shaders/deferred/deferred.vs.shader", "res/shaders/deferred/deferred_environment.fs.shader");
		deferredEnvironmentSimpleShader = Resource.GetShader("res/shaders/deferred/deferred.vs.shader", "res/shaders/deferred/deferred_environment_simple.fs.shader");
		skyShader = Resource.GetShader("res/shaders/sky/sky.vs.shader", "res/shaders/sky/sky.fs.shader");
		waterShader = Resource.GetShader("res/shaders/water/water.vs.shader", "res/shaders/water/water.fs.shader");
		particleShader = Resource.GetShader("res/shaders/particle/particle.vs.shader", "res/shaders/particle/particle.fs.shader");
		particleAdditiveShader = Resource.GetShader("res/shaders/particle/particle_additive.vs.shader", "res/shaders/particle/particle_additive.fs.shader");
		lineShader = Resource.GetShader("res/shaders/line/line.vs.shader", "res/shaders/line/line.fs.shader");
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

		debugLineRenderer = new LineRenderer();

		GUI.Init(graphics);
	}

	public static void Resize(int width, int height)
	{
		if (gbuffer != null)
			graphics.destroyRenderTarget(gbuffer);
		if (forward != null)
			graphics.destroyRenderTarget(forward);

		gbuffer = graphics.createRenderTarget(new RenderTargetAttachment[] {
			new RenderTargetAttachment(width, height, TextureFormat.RGBA32F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp),
			new RenderTargetAttachment(width, height, TextureFormat.RGBA16F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp),
			new RenderTargetAttachment(width, height, TextureFormat.RGBA8, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp),
			new RenderTargetAttachment(width, height, TextureFormat.RGBA8, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp),
			new RenderTargetAttachment(width, height, TextureFormat.D16F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp, true)
		});

		forward = graphics.createRenderTarget(new RenderTargetAttachment[]
		{
			new RenderTargetAttachment(width, height, TextureFormat.RGBA16F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp),
			new RenderTargetAttachment(width, height, TextureFormat.D16F, (ulong)TextureFlags.RenderTarget | (ulong)TextureFlags.BlitDst | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp),
		});

		Renderer.width = width;
		Renderer.height = height;
	}

	public static void DrawModel(Model model, Matrix transform, Animator animator = null)
	{
		models.Add(new ModelDrawCommand { model = model, meshID = -1, transform = transform, animator = animator });
	}

	public static void DrawSubModel(Model model, int meshID, Matrix transform)
	{
		models.Add(new ModelDrawCommand { model = model, meshID = meshID, transform = transform });
	}

	public static void DrawDebugLine(Vector3 vertex0, Vector3 vertex1, Vector4 color)
	{
		debugLines.Add(new DebugLineDrawCommand { vertex0 = vertex0, vertex1 = vertex1, color = color });
	}

	public static void DrawDebugBox(Vector3 size, Matrix transform, Vector4 color)
	{
		Vector3 vertex0 = transform * (0.5f * new Vector3(-size.x, -size.y, -size.z));
		Vector3 vertex1 = transform * (0.5f * new Vector3(size.x, -size.y, -size.z));
		Vector3 vertex2 = transform * (0.5f * new Vector3(size.x, -size.y, size.z));
		Vector3 vertex3 = transform * (0.5f * new Vector3(-size.x, -size.y, size.z));
		Vector3 vertex4 = transform * (0.5f * new Vector3(-size.x, size.y, -size.z));
		Vector3 vertex5 = transform * (0.5f * new Vector3(size.x, size.y, -size.z));
		Vector3 vertex6 = transform * (0.5f * new Vector3(size.x, size.y, size.z));
		Vector3 vertex7 = transform * (0.5f * new Vector3(-size.x, size.y, size.z));

		DrawDebugLine(vertex0, vertex1, color);
		DrawDebugLine(vertex1, vertex2, color);
		DrawDebugLine(vertex2, vertex3, color);
		DrawDebugLine(vertex3, vertex0, color);

		DrawDebugLine(vertex4, vertex5, color);
		DrawDebugLine(vertex5, vertex6, color);
		DrawDebugLine(vertex6, vertex7, color);
		DrawDebugLine(vertex7, vertex4, color);

		DrawDebugLine(vertex0, vertex4, color);
		DrawDebugLine(vertex1, vertex5, color);
		DrawDebugLine(vertex2, vertex6, color);
		DrawDebugLine(vertex3, vertex7, color);
	}

	public static void DrawDebugSphere(float radius, Matrix transform, Vector4 color)
	{
		int segmentCount = 32;
		for (int j = 0; j < 3; j++)
		{
			Quaternion ringRot = j == 0 ? Quaternion.Identity : j == 1 ? Quaternion.FromAxisAngle(Vector3.UnitX, MathF.PI * 0.5f) : Quaternion.FromAxisAngle(Vector3.UnitZ, MathF.PI * 0.5f);

			for (int k = 0; k < segmentCount; k++)
			{
				Vector3 vertex0 = transform * (ringRot * Quaternion.FromAxisAngle(Vector3.Up, k / (float)segmentCount * 2 * MathF.PI) * new Vector3(0.0f, 0.0f, radius));
				Vector3 vertex1 = transform * (ringRot * Quaternion.FromAxisAngle(Vector3.Up, (k + 1) / (float)segmentCount * 2 * MathF.PI) * new Vector3(0.0f, 0.0f, radius));

				DrawDebugLine(vertex0, vertex1, color);
			}
		}
	}

	public static void DrawDebugCapsule(float radius, float height, Matrix transform, Vector4 color)
	{
		int segmentCount = 32;

		// top ring
		for (int k = 0; k < segmentCount; k++)
		{
			Vector3 vertex0 = transform * (new Vector3(0.0f, height * 0.5f - radius, 0.0f) + Quaternion.FromAxisAngle(Vector3.Up, k / (float)segmentCount * 2 * MathF.PI) * new Vector3(0.0f, 0.0f, radius));
			Vector3 vertex1 = transform * (new Vector3(0.0f, height * 0.5f - radius, 0.0f) + Quaternion.FromAxisAngle(Vector3.Up, (k + 1) / (float)segmentCount * 2 * MathF.PI) * new Vector3(0.0f, 0.0f, radius));

			DrawDebugLine(vertex0, vertex1, color);
		}

		// bottom ring
		for (int k = 0; k < segmentCount; k++)
		{
			Vector3 vertex0 = transform * (new Vector3(0.0f, height * 0.5f - radius, 0.0f) + Quaternion.FromAxisAngle(Vector3.Up, k / (float)segmentCount * 2 * MathF.PI) * new Vector3(0.0f, 0.0f, radius));
			Vector3 vertex1 = transform * (new Vector3(0.0f, height * 0.5f - radius, 0.0f) + Quaternion.FromAxisAngle(Vector3.Up, (k + 1) / (float)segmentCount * 2 * MathF.PI) * new Vector3(0.0f, 0.0f, radius));

			DrawDebugLine(vertex0, vertex1, color);
		}

		// vertical ring 1
		for (int k = 0; k < segmentCount; k++)
		{
			Quaternion ringRot = Quaternion.FromAxisAngle(Vector3.UnitZ, MathF.PI * 0.5f);

			Vector3 vertex0 = transform * ((k < segmentCount / 2 ? 1 : -1) * new Vector3(0.0f, height * 0.5f - radius, 0.0f) + ringRot * Quaternion.FromAxisAngle(Vector3.Up, k / (float)segmentCount * 2 * MathF.PI) * new Vector3(0.0f, 0.0f, radius));
			Vector3 vertex1 = transform * ((k < segmentCount / 2 ? 1 : -1) * new Vector3(0.0f, height * 0.5f - radius, 0.0f) + ringRot * Quaternion.FromAxisAngle(Vector3.Up, (k + 1) / (float)segmentCount * 2 * MathF.PI) * new Vector3(0.0f, 0.0f, radius));

			DrawDebugLine(vertex0, vertex1, color);
		}

		// vertical ring 2
		for (int k = 0; k < segmentCount; k++)
		{
			Quaternion ringRot = Quaternion.FromAxisAngle(Vector3.UnitY, MathF.PI * 0.5f) * Quaternion.FromAxisAngle(Vector3.UnitZ, MathF.PI * 0.5f);

			Vector3 vertex0 = transform * ((k < segmentCount / 2 ? 1 : -1) * new Vector3(0.0f, height * 0.5f - radius, 0.0f) + ringRot * Quaternion.FromAxisAngle(Vector3.Up, k / (float)segmentCount * 2 * MathF.PI) * new Vector3(0.0f, 0.0f, radius));
			Vector3 vertex1 = transform * ((k < segmentCount / 2 ? 1 : -1) * new Vector3(0.0f, height * 0.5f - radius, 0.0f) + ringRot * Quaternion.FromAxisAngle(Vector3.Up, (k + 1) / (float)segmentCount * 2 * MathF.PI) * new Vector3(0.0f, 0.0f, radius));

			DrawDebugLine(vertex0, vertex1, color);
		}

		// vertical lines
		for (int k = 0; k < 4; k++)
		{
			Vector3 vertex0 = transform * (new Vector3(0.0f, -height * 0.5f + radius, 0.0f) + Quaternion.FromAxisAngle(Vector3.Up, k / 4.0f * MathF.PI * 2) * new Vector3(0.0f, 0.0f, radius));
			Vector3 vertex1 = transform * (new Vector3(0.0f, height * 0.5f - radius, 0.0f) + Quaternion.FromAxisAngle(Vector3.Up, k / 4.0f * MathF.PI * 2) * new Vector3(0.0f, 0.0f, radius));

			DrawDebugLine(vertex0, vertex1, color);
		}
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

		Renderer.projection = camera.getProjectionMatrix(width, height);
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

	static void GeometryPass()
	{
		graphics.setPass((int)RenderPass.Geometry);
		graphics.setRenderTarget(gbuffer, 0x000000FF);

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

			shadowMap.calculateCascadeTransforms(camera.position, camera.rotation, Camera.FOV, Display.aspectRatio);

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

	static void RenderPointLights()
	{
		Span<Vector4> lightPositionBuffer = stackalloc Vector4[MAX_LIGHTS_PER_PASS];
		Span<Vector4> lightColorBuffer = stackalloc Vector4[MAX_LIGHTS_PER_PASS];

		Shader shader = deferredPointShader;

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
			shader = deferredPointShadowShader;

			graphics.resetState();

			graphics.setBlendState(BlendState.Additive);
			graphics.setDepthTest(DepthTest.None);
			graphics.setCullState(CullState.ClockWise);

			graphics.setVertexBuffer(quad);

			graphics.setTexture(shader.getUniform("s_gbuffer0", UniformType.Sampler), 0, gbuffer.getAttachmentTexture(0));
			graphics.setTexture(shader.getUniform("s_gbuffer1", UniformType.Sampler), 1, gbuffer.getAttachmentTexture(1));
			graphics.setTexture(shader.getUniform("s_gbuffer2", UniformType.Sampler), 2, gbuffer.getAttachmentTexture(2));
			graphics.setTexture(shader.getUniform("s_gbuffer3", UniformType.Sampler), 3, gbuffer.getAttachmentTexture(3));

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
	}

	static void RenderDebugLines()
	{
		graphics.resetState();

		graphics.setViewTransform(projection, view);

		debugLineRenderer.begin(debugLines.Count);
		for (int i = 0; i < debugLines.Count; i++)
		{
			debugLineRenderer.draw(debugLines[i].vertex0, debugLines[i].vertex1, debugLines[i].color);
		}
		debugLineRenderer.end((int)RenderPass.Forward, lineShader, graphics);
	}

	static void ForwardPass()
	{
		graphics.setPass((int)RenderPass.Forward);
		graphics.setRenderTarget(forward);


		//RenderSky();
		//RenderParticles();
		RenderDebugLines();
	}

	static void TonemappingPass()
	{
		graphics.resetState();
		graphics.setPass((int)RenderPass.Tonemapping);

		graphics.setRenderTarget(finalTarget);

		graphics.setDepthTest(DepthTest.None);
		graphics.setCullState(CullState.ClockWise);

		graphics.setVertexBuffer(quad);
		graphics.setTexture(tonemappingShader.getUniform("s_hdrBuffer", UniformType.Sampler), 0, forward.getAttachmentTexture(0));

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

	public static Texture End()
	{
		meshRenderCounter = 0;
		meshCulledCounter = 0;

		lights.Sort(LightDistanceComparator);
		pointLights.Sort(PointLightDistanceComparator);

		GeometryPass();
		ShadowPass();
		ReflectionProbePass();
		DeferredPass();

		graphics.blit(forward.getAttachmentTexture(forward.attachmentCount - 1), gbuffer.getAttachmentTexture(gbuffer.attachmentCount - 1));

		ForwardPass();
		TonemappingPass();
		GUI.Draw((int)RenderPass.UI, finalTarget);

		models.Clear();
		modelsInstanced.Clear();
		skies.Clear();
		waterTiles.Clear();
		lights.Clear();
		pointLights.Clear();
		directionalLights.Clear();
		reflectionProbes.Clear();
		particleSystems.Clear();
		particleSystemsAdditive.Clear();
		debugLines.Clear();

		return finalTarget.getAttachmentTexture(0);
	}
}
