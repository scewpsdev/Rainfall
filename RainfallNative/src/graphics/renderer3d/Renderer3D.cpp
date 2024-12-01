#include "Renderer3D.h"

#include "Rainfall.h"
#include "Resource.h"

#include "graphics/Graphics.h"
#include "graphics/Model.h"
#include "graphics/Material.h"
#include "graphics/ParticleSystem.h"
#include "graphics/LineRenderer.h"

#include "physics/Physics.h"
#include "physics/Cloth.h"

#include "vector/Math.h"

#include "utils/List.h"

#include <bgfx/bgfx.h>


#define MAX_REFLECTION_PROBES 4
#define MAX_POINT_SHADOWS 8
#define BLOOM_STEP_COUNT 6
#define MAX_INDIRECT_DRAWS 1024
#define MAX_POINT_LIGHTS 1024
#define MAX_PARTICLE_SYSTEMS 1024
#define MAX_BONES 128


enum RenderPass
{
	DepthPrepass,
	HZB,
	MeshCulling,
	LightCulling,
	StreamCompaction,
	ParticleCulling,
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
	BloomDownsample_,
	BloomUpsample_ = BloomDownsample_ + BLOOM_STEP_COUNT,
	Composite = BloomUpsample_ + BLOOM_STEP_COUNT - 1,
	Tonemapping,
	UI,
	Debug,

	Count
};

struct Renderer3DSettings
{
	bool showFrame = true;

	bool ssaoEnabled = true;

	bool bloomEnabled = true;
	float bloomStrength = 0.15f;
	float bloomFalloff = 5.0f;

	float exposure = 1.0f;

	Vector3 fogColor = Vector3::One;
	float fogStrength = 0.0f;

	bool physicsDebugDraw = false;
};

struct MeshDraw
{
	MeshData* mesh;
	Matrix transform;
	AABB boundingBox;
	Material* material;
	AnimationState* animation = nullptr;

	bool culled = false;
};

struct ClothDraw
{
	Cloth* cloth;
	Material* material;
	Vector3 position;
	Quaternion rotation;

	bool culled = false;
};

struct PointLightDraw
{
	Vector3 position;
	Vector3 color;
	float radius;

	bool culled = false;
};

struct ParticleSystemDraw
{
	ParticleSystem* system;

	bool culled = false;
};

struct EnvironmentMapMask
{
	Vector3 position;
	Vector3 size;
	float falloff;
};

struct ReflectionProbeDraw
{
	Vector3 position;
	Vector3 size;
};

struct DebugLineDraw
{
	Vector3 position0;
	Vector3 position1;
	uint32_t color;
};


static Renderer3DSettings settings;

static int width, height;

static uint16_t gbuffer = bgfx::kInvalidHandle;
static bgfx::TextureHandle gbufferTextures[5];
static bgfx::TextureInfo gbufferTextureInfos[5];

static uint16_t hzb = bgfx::kInvalidHandle;
static bgfx::TextureInfo hzbTextureInfo;
static int hzbWidth, hzbHeight;

static uint16_t deferredRT = bgfx::kInvalidHandle;
static bgfx::TextureHandle deferredRTTextures[2];
static bgfx::TextureInfo deferredRTTextureInfos[2];

static uint16_t forwardRT = bgfx::kInvalidHandle;
static bgfx::TextureHandle forwardRTTextures[2];
static bgfx::TextureInfo forwardRTTextureInfos[2];

static uint16_t compositeRT = bgfx::kInvalidHandle;
static bgfx::TextureHandle compositeRTTexture;
static bgfx::TextureInfo compositeRTTextureInfo;

static uint16_t tonemappingRT = bgfx::kInvalidHandle;
static bgfx::TextureHandle tonemappingRTTextures[2];
static bgfx::TextureInfo tonemappingRTTextureInfos[2];

static uint16_t ssaoRT = bgfx::kInvalidHandle;
static bgfx::TextureHandle ssaoRTTexture;
static bgfx::TextureInfo ssaoRTTextureInfo;
static Shader* ssaoShader;
static uint16_t ssaoBlurRT = bgfx::kInvalidHandle;
static bgfx::TextureHandle ssaoBlurRTTexture;
static bgfx::TextureInfo ssaoBlurRTTextureInfo;
static Shader* ssaoBlurShader;

static bgfx::UniformHandle s_depth;
static bgfx::UniformHandle s_normals;
static bgfx::UniformHandle s_ao;
static bgfx::UniformHandle s_noise;
static bgfx::UniformHandle u_cameraFrustum;
static bgfx::UniformHandle u_viewMatrix;
static bgfx::UniformHandle u_viewInv;
static bgfx::UniformHandle u_projectionView;
static bgfx::UniformHandle u_projectionInv;
static bgfx::UniformHandle u_projectionViewInv;

static uint16_t bloomDownsampleRTs[BLOOM_STEP_COUNT];
static bgfx::TextureHandle bloomDownsampleRTTextures[BLOOM_STEP_COUNT];
static bgfx::TextureInfo bloomDownsampleRTTextureInfos[BLOOM_STEP_COUNT];
static Shader* bloomDownsampleShader;
static uint16_t bloomUpsampleRTs[BLOOM_STEP_COUNT - 1];
static bgfx::TextureHandle bloomUpsampleRTTextures[BLOOM_STEP_COUNT - 1];
static bgfx::TextureInfo bloomUpsampleRTTextureInfos[BLOOM_STEP_COUNT - 1];
static Shader* bloomUpsampleShader;

Shader* defaultShader;
Shader* defaultAnimatedShader;
static Shader* deferredPointShader;
static Shader* deferredDirectionalShader;
static Shader* deferredEmissiveShader;
static Shader* deferredEnvironmentShader;
static Shader* deferredReflectionProbeShader;
static Shader* tonemappingShader;
static Shader* particleShader;
static Shader* skyShader;
static Shader* clothShader;
static Shader* debugShader;

static const float quadVertices[] = { -3.0f, -1.0f, 1.0f, 1.0f, -1.0f, 1.0f, 1.0f, 3.0f, 1.0f };
static uint16_t quad = bgfx::kInvalidHandle;

static const float boxVertices[] = { -1, -1, -1, 1, -1, -1, -1, -1, 1, 1, -1, 1, -1, 1, -1, 1, 1, -1, -1, 1, 1, 1, 1, 1 };
static const short boxIndices[] = { 0, 1, 2, 3, 2, 1, 0, 4, 5, 0, 5, 1, 0, 2, 6, 0, 6, 4, 1, 5, 7, 1, 7, 3, 4, 6, 7, 4, 7, 5, 2, 3, 7, 2, 7, 6 };
static uint16_t box = bgfx::kInvalidHandle;
static uint16_t boxIBO = bgfx::kInvalidHandle;

static SceneData* sphereData;
static uint16_t sphere = bgfx::kInvalidHandle;
static uint16_t sphereIBO = bgfx::kInvalidHandle;

static const float particleVertices[] = { -0.5f, -0.5f, 0.5f, -0.5f, 0.5f, 0.5f, 0.5f, 0.5f, -0.5f, 0.5f, -0.5f, -0.5f };
static uint16_t particle = bgfx::kInvalidHandle;

static const float skydomeVertices[] = { -10, -10, 10, 10, -10, 10, 0, -10, -10, 0, 10, 0 };
static const short skydomeIndices[] = { 0, 1, 2, 2, 1, 3, 1, 0, 3, 0, 2, 3 };
static uint16_t skydome = bgfx::kInvalidHandle;
static uint16_t skydomeIBO = bgfx::kInvalidHandle;

static uint16_t emptyCubemap = bgfx::kInvalidHandle;

static bgfx::TextureHandle blueNoise64;

static bgfx::UniformHandle s_hzb;
static bgfx::UniformHandle u_params;
static bgfx::UniformHandle u_pv;

static Vector3 cameraPosition;
static Quaternion cameraRotation;
static float cameraFOV, cameraAspect;
static float cameraNear, cameraFar;
static Vector3 cameraRight, cameraUp, cameraForward;
static Matrix projection, view, pv;
static Vector4 frustumPlanes[6];

static List<MeshDraw> meshDraws;
static List<MeshDraw> occluderMeshes;
static int numVisibleMeshes;
static bgfx::IndirectBufferHandle indirectBuffer;
static bgfx::DynamicVertexBufferHandle aabbBuffer;
static Shader* hzbDownsampleShader;
static Shader* meshIndirectShader;

static List<MeshDraw> forwardDraws;

static List<PointLightDraw> pointLightDraws;
static int numVisibleLights;
static bgfx::IndirectBufferHandle lightIndirectBuffer;
static bgfx::DynamicVertexBufferHandle lightBuffer;
static bgfx::DynamicIndexBufferHandle lightInstanceCount;
static bgfx::DynamicIndexBufferHandle lightInstancePredicates;
static bgfx::DynamicVertexBufferHandle lightCulledInstanceBuffer;
static Shader* lightIndirectShader;
static Shader* streamCompactionShader;

static List<ParticleSystemDraw> particleSystemDraws;
static int numVisibleParticleSystems;
static bgfx::IndirectBufferHandle particleIndirectBuffer;
static bgfx::DynamicVertexBufferHandle particleAabbBuffer;
static Shader* particleIndirectShader;

static List<ClothDraw> clothDraws;

static bool renderDirectionalLight;
static Vector3 directionalLightDirection;
static Vector3 directionalLightColor;
static bool directionalLightShadowsNeedUpdate;
static int directionalLightShadowMapRes;
static bgfx::FrameBufferHandle directionalLightShadowMapRTs[3];
static float directionalLightShadowMapFar[3];
static Matrix directionalLightShadowMapMatrix[3];

static uint16_t environmentMap = bgfx::kInvalidHandle;
float environmentIntensity;
static List<EnvironmentMapMask> environmentMapMasks;

static List<ReflectionProbeDraw> reflectionProbeDraws;

static uint16_t skyTexture = bgfx::kInvalidHandle;
static float skyIntensity;
static Matrix skyTransform;

static List<DebugLineDraw> debugLineDraws;
static LineRenderer debugLineRenderer;


RFAPI void Renderer3D_Resize(int width, int height);

RFAPI void Renderer3D_Init(int width, int height)
{
	for (int i = 0; i < BLOOM_STEP_COUNT; i++)
	{
		bloomDownsampleRTs[i] = bgfx::kInvalidHandle;
		if (i < BLOOM_STEP_COUNT - 1)
			bloomUpsampleRTs[i] = bgfx::kInvalidHandle;
	}

	Renderer3D_Resize(width, height);

	defaultShader = Shader_Create("res/rainfall/shaders/default/default.vsh.bin", "res/rainfall/shaders/default/default.fsh.bin");
	defaultAnimatedShader = Shader_Create("res/rainfall/shaders/default/default_animated.vsh.bin", "res/rainfall/shaders/default/default.fsh.bin");
	deferredPointShader = Shader_Create("res/rainfall/shaders/deferred/deferred_point.vsh.bin", "res/rainfall/shaders/deferred/deferred_point.fsh.bin");
	deferredDirectionalShader = Shader_Create("res/rainfall/shaders/deferred/deferred.vsh.bin", "res/rainfall/shaders/deferred/deferred_directional.fsh.bin");
	deferredEmissiveShader = Shader_Create("res/rainfall/shaders/deferred/deferred.vsh.bin", "res/rainfall/shaders/deferred/deferred_emissive.fsh.bin");
	deferredEnvironmentShader = Shader_Create("res/rainfall/shaders/deferred/deferred.vsh.bin", "res/rainfall/shaders/deferred/deferred_environment.fsh.bin");
	deferredReflectionProbeShader = Shader_Create("res/rainfall/shaders/deferred/deferred_reflection_probe.vsh.bin", "res/rainfall/shaders/deferred/deferred_reflection_probe.fsh.bin");
	hzbDownsampleShader = Shader_CreateCompute("res/rainfall/shaders/hzb/hzb_downsample.csh.bin");
	meshIndirectShader = Shader_CreateCompute("res/rainfall/shaders/occlusion_culling/mesh_indirect.csh.bin");
	lightIndirectShader = Shader_CreateCompute("res/rainfall/shaders/occlusion_culling/light_indirect.csh.bin");
	streamCompactionShader = Shader_CreateCompute("res/rainfall/shaders/occlusion_culling/stream_compaction.csh.bin");
	particleIndirectShader = Shader_CreateCompute("res/rainfall/shaders/occlusion_culling/particle_indirect.csh.bin");
	tonemappingShader = Shader_Create("res/rainfall/shaders/tonemapping/tonemapping.vsh.bin", "res/rainfall/shaders/tonemapping/tonemapping.fsh.bin");
	particleShader = Shader_Create("res/rainfall/shaders/particle/particle.vsh.bin", "res/rainfall/shaders/particle/particle.fsh.bin");
	skyShader = Shader_Create("res/rainfall/shaders/sky/sky.vsh.bin", "res/rainfall/shaders/sky/sky.fsh.bin");
	clothShader = Shader_Create("res/rainfall/shaders/cloth/cloth.vsh.bin", "res/rainfall/shaders/cloth/cloth.fsh.bin");
	ssaoShader = Shader_Create("res/rainfall/shaders/ao/ssao.vsh.bin", "res/rainfall/shaders/ao/ssao.fsh.bin");
	ssaoBlurShader = Shader_Create("res/rainfall/shaders/ao/ssao_blur.vsh.bin", "res/rainfall/shaders/ao/ssao_blur.fsh.bin");
	bloomDownsampleShader = Shader_Create("res/rainfall/shaders/bloom/bloom.vsh.bin", "res/rainfall/shaders/bloom/bloom_downsample.fsh.bin");
	bloomUpsampleShader = Shader_Create("res/rainfall/shaders/bloom/bloom.vsh.bin", "res/rainfall/shaders/bloom/bloom_upsample.fsh.bin");
	debugShader = Shader_Create("res/rainfall/shaders/debug/debug.vsh.bin", "res/rainfall/shaders/debug/debug.fsh.bin");

	VertexElement quadLayout(VertexAttribute::Position, VertexAttributeType::Vector3);
	const bgfx::Memory* quadMemory = Graphics_CreateVideoMemoryRef(sizeof(quadVertices), quadVertices, nullptr);
	quad = Graphics_CreateVertexBuffer(quadMemory, &quadLayout, 1, BufferFlags::None);

	VertexElement boxLayout(VertexAttribute::Position, VertexAttributeType::Vector3);
	const bgfx::Memory* boxMemory = Graphics_CreateVideoMemoryRef(sizeof(boxVertices), boxVertices, nullptr);
	box = Graphics_CreateVertexBuffer(boxMemory, &boxLayout, 1, BufferFlags::None);

	const bgfx::Memory* boxIndicesMemory = Graphics_CreateVideoMemoryRef(sizeof(boxIndices), boxIndices, nullptr);
	boxIBO = Graphics_CreateIndexBuffer(boxIndicesMemory, BufferFlags::None);

	sphereData = Resource_CreateSceneDataFromFile("res/rainfall/sphere.gltf.bin", 0);
	sphere = sphereData->meshes[0].vertexNormalTangentBuffer.idx;
	sphereIBO = sphereData->meshes[0].indexBuffer.idx;

	VertexElement particleLayout(VertexAttribute::Position, VertexAttributeType::Vector2);
	const bgfx::Memory* particleMemory = Graphics_CreateVideoMemoryRef(sizeof(particleVertices), particleVertices, nullptr);
	particle = Graphics_CreateVertexBuffer(particleMemory, &particleLayout, 1, BufferFlags::None);

	VertexElement skydomeLayout(VertexAttribute::Position, VertexAttributeType::Vector3);
	const bgfx::Memory* skydomeMemory = Graphics_CreateVideoMemoryRef(sizeof(skydomeVertices), skydomeVertices, nullptr);
	skydome = Graphics_CreateVertexBuffer(skydomeMemory, &skydomeLayout, 1, BufferFlags::None);

	const bgfx::Memory* skydomeIndicesMemory = Graphics_CreateVideoMemoryRef(sizeof(skydomeIndices), skydomeIndices, nullptr);
	skydomeIBO = Graphics_CreateIndexBuffer(skydomeIndicesMemory, BufferFlags::None);

	bgfx::TextureInfo cubemapInfo;
	emptyCubemap = Graphics_CreateCubemap(250, bgfx::TextureFormat::RG11B10F, 0, &cubemapInfo);

	blueNoise64 = bgfx::TextureHandle{ Resource_CreateTexture2DFromFile("res/rainfall/LDR_LLL1_0.png.bin", BGFX_SAMPLER_POINT, nullptr) };

	s_hzb = bgfx::createUniform("s_hzb", bgfx::UniformType::Sampler);
	u_params = bgfx::createUniform("u_params", bgfx::UniformType::Vec4);
	u_pv = bgfx::createUniform("u_pv", bgfx::UniformType::Mat4);

	indirectBuffer = bgfx::IndirectBufferHandle{ Graphics_CreateIndirectBuffer(MAX_INDIRECT_DRAWS) };

	VertexElement aabbLayout(VertexAttribute::TexCoord0, VertexAttributeType::Vector4);
	aabbBuffer = bgfx::DynamicVertexBufferHandle{ Graphics_CreateDynamicVertexBuffer(&aabbLayout, 1, MAX_INDIRECT_DRAWS * 2, BGFX_BUFFER_COMPUTE_READ) };

	lightIndirectBuffer = bgfx::IndirectBufferHandle{ Graphics_CreateIndirectBuffer(MAX_POINT_LIGHTS) };

	VertexElement lightLayout[] = {
		VertexElement(VertexAttribute::TexCoord7, VertexAttributeType::Vector4),
		VertexElement(VertexAttribute::TexCoord6, VertexAttributeType::Vector4),
	};
	lightBuffer = bgfx::DynamicVertexBufferHandle{ Graphics_CreateDynamicVertexBuffer(lightLayout, 2, MAX_POINT_LIGHTS, BGFX_BUFFER_COMPUTE_READ) };

	lightInstanceCount = bgfx::DynamicIndexBufferHandle{ Graphics_CreateDynamicIndexBuffer(1, BGFX_BUFFER_INDEX32 | BGFX_BUFFER_COMPUTE_READ_WRITE) };

	lightInstancePredicates = bgfx::DynamicIndexBufferHandle{ Graphics_CreateDynamicIndexBuffer(MAX_POINT_LIGHTS, BGFX_BUFFER_COMPUTE_READ_WRITE) };

	lightCulledInstanceBuffer = bgfx::DynamicVertexBufferHandle{ Graphics_CreateDynamicVertexBuffer(lightLayout, 2, MAX_POINT_LIGHTS, BGFX_BUFFER_COMPUTE_WRITE) };

	particleIndirectBuffer = bgfx::IndirectBufferHandle{ Graphics_CreateIndirectBuffer(MAX_PARTICLE_SYSTEMS) };

	particleAabbBuffer = bgfx::DynamicVertexBufferHandle{ Graphics_CreateDynamicVertexBuffer(lightLayout, 2, MAX_PARTICLE_SYSTEMS, BGFX_BUFFER_COMPUTE_READ) };

	s_depth = bgfx::createUniform("s_depth", bgfx::UniformType::Sampler);
	s_normals = bgfx::createUniform("s_normals", bgfx::UniformType::Sampler);
	s_ao = bgfx::createUniform("s_ao", bgfx::UniformType::Sampler);
	s_noise = bgfx::createUniform("s_noise", bgfx::UniformType::Sampler);
	u_cameraFrustum = bgfx::createUniform("u_cameraFrustum", bgfx::UniformType::Vec4);
	u_viewMatrix = bgfx::createUniform("u_viewMatrix", bgfx::UniformType::Mat4);
	u_viewInv = bgfx::createUniform("u_viewInv", bgfx::UniformType::Mat4);
	u_projectionView = bgfx::createUniform("u_projectionView", bgfx::UniformType::Mat4);
	u_projectionInv = bgfx::createUniform("u_projectionInv", bgfx::UniformType::Mat4);
	u_projectionViewInv = bgfx::createUniform("u_projectionViewInv", bgfx::UniformType::Mat4);
}

RFAPI void Renderer3D_Resize(int width, int height)
{
	::width = width;
	::height = height;

	if (gbuffer != bgfx::kInvalidHandle)
		bgfx::destroy(bgfx::FrameBufferHandle{ gbuffer });
	if (hzb != bgfx::kInvalidHandle)
		bgfx::destroy(bgfx::TextureHandle{ hzb });
	if (deferredRT != bgfx::kInvalidHandle)
		bgfx::destroy(bgfx::FrameBufferHandle{ deferredRT });
	if (forwardRT != bgfx::kInvalidHandle)
		bgfx::destroy(bgfx::FrameBufferHandle{ forwardRT });
	if (compositeRT != bgfx::kInvalidHandle)
		bgfx::destroy(bgfx::FrameBufferHandle{ compositeRT });
	if (tonemappingRT != bgfx::kInvalidHandle)
		bgfx::destroy(bgfx::FrameBufferHandle{ tonemappingRT });
	if (ssaoRT != bgfx::kInvalidHandle)
		bgfx::destroy(bgfx::FrameBufferHandle{ ssaoRT });
	if (ssaoBlurRT != bgfx::kInvalidHandle)
		bgfx::destroy(bgfx::FrameBufferHandle{ ssaoBlurRT });
	for (int i = 0; i < BLOOM_STEP_COUNT; i++)
	{
		if (bloomDownsampleRTs[i] != bgfx::kInvalidHandle)
			Graphics_DestroyRenderTarget(bloomDownsampleRTs[i]);
		if (i < BLOOM_STEP_COUNT - 1 && bloomUpsampleRTs[i] != bgfx::kInvalidHandle)
			Graphics_DestroyRenderTarget(bloomUpsampleRTs[i]);
	}

	RenderTargetAttachment gbufferAttachments[5] =
	{
		RenderTargetAttachment(width, height, bgfx::TextureFormat::RGBA32F, BGFX_TEXTURE_RT | BGFX_SAMPLER_U_CLAMP | BGFX_SAMPLER_V_CLAMP),
		RenderTargetAttachment(width, height, bgfx::TextureFormat::RGBA16F, BGFX_TEXTURE_RT | BGFX_SAMPLER_U_CLAMP | BGFX_SAMPLER_V_CLAMP),
		RenderTargetAttachment(width, height, bgfx::TextureFormat::RGBA8, BGFX_TEXTURE_RT | BGFX_SAMPLER_U_CLAMP | BGFX_SAMPLER_V_CLAMP),
		RenderTargetAttachment(width, height, bgfx::TextureFormat::RGBA8, BGFX_TEXTURE_RT | BGFX_SAMPLER_U_CLAMP | BGFX_SAMPLER_V_CLAMP),
		RenderTargetAttachment(width, height, bgfx::TextureFormat::D32F, BGFX_TEXTURE_RT | BGFX_SAMPLER_U_CLAMP | BGFX_SAMPLER_V_CLAMP, true)
	};
	gbuffer = Graphics_CreateRenderTarget(5, gbufferAttachments, gbufferTextures, gbufferTextureInfos);

	hzbWidth = ipow(2, (int)floor(log2((double)width + 0.5)));
	hzbHeight = ipow(2, (int)floor(log2((double)height + 0.5)));
	hzb = Graphics_CreateTextureMutableEx(hzbWidth, hzbHeight, bgfx::TextureFormat::R32F, true, 1, BGFX_SAMPLER_POINT | BGFX_SAMPLER_UVW_CLAMP | BGFX_TEXTURE_COMPUTE_WRITE, &hzbTextureInfo);

	RenderTargetAttachment deferredRTAttachments[2] =
	{
		RenderTargetAttachment(width, height, bgfx::TextureFormat::RG11B10F, BGFX_TEXTURE_RT | BGFX_SAMPLER_U_CLAMP | BGFX_SAMPLER_V_CLAMP),
		RenderTargetAttachment(width, height, bgfx::TextureFormat::D32F, BGFX_TEXTURE_RT | BGFX_SAMPLER_U_CLAMP | BGFX_SAMPLER_V_CLAMP)
	};
	deferredRT = Graphics_CreateRenderTarget(2, deferredRTAttachments, deferredRTTextures, deferredRTTextureInfos);

	RenderTargetAttachment forwardRTAttachments[2] =
	{
		RenderTargetAttachment(width, height, bgfx::TextureFormat::RG11B10F, BGFX_TEXTURE_RT | BGFX_TEXTURE_BLIT_DST | BGFX_SAMPLER_U_CLAMP | BGFX_SAMPLER_V_CLAMP),
		RenderTargetAttachment(width, height, bgfx::TextureFormat::D32F, BGFX_TEXTURE_RT | BGFX_TEXTURE_BLIT_DST | BGFX_SAMPLER_U_CLAMP | BGFX_SAMPLER_V_CLAMP)
	};
	forwardRT = Graphics_CreateRenderTarget(2, forwardRTAttachments, forwardRTTextures, forwardRTTextureInfos);

	RenderTargetAttachment compositeRTAttachment(width, height, bgfx::TextureFormat::RG11B10F, BGFX_TEXTURE_RT | BGFX_SAMPLER_U_CLAMP | BGFX_SAMPLER_V_CLAMP);
	compositeRT = Graphics_CreateRenderTarget(1, &compositeRTAttachment, &compositeRTTexture, &compositeRTTextureInfo);

	RenderTargetAttachment tonemappingRTAttachments[2] = {
		RenderTargetAttachment(width, height, bgfx::TextureFormat::RGBA8, BGFX_TEXTURE_RT | BGFX_SAMPLER_UVW_CLAMP),
		RenderTargetAttachment(width, height, bgfx::TextureFormat::D32F, BGFX_TEXTURE_RT | BGFX_TEXTURE_BLIT_DST | BGFX_SAMPLER_UVW_CLAMP),
	};
	tonemappingRT = Graphics_CreateRenderTarget(2, tonemappingRTAttachments, tonemappingRTTextures, tonemappingRTTextureInfos);

	RenderTargetAttachment ssaoRTAttachment(width / 2, height / 2, bgfx::TextureFormat::R8, BGFX_TEXTURE_RT | BGFX_SAMPLER_UVW_CLAMP);
	ssaoRT = Graphics_CreateRenderTarget(1, &ssaoRTAttachment, &ssaoRTTexture, &ssaoRTTextureInfo);

	RenderTargetAttachment ssaoBlurRTAttachment(width, height, bgfx::TextureFormat::R8, BGFX_TEXTURE_RT | BGFX_SAMPLER_UVW_CLAMP);
	ssaoBlurRT = Graphics_CreateRenderTarget(1, &ssaoBlurRTAttachment, &ssaoBlurRTTexture, &ssaoBlurRTTextureInfo);

	int bloomRTWidth = width / 2;
	int bloomRTHeight = height / 2;
	for (int i = 0; i < BLOOM_STEP_COUNT; i++)
	{
		RenderTargetAttachment bloomRTAttachment(bloomRTWidth, bloomRTHeight, bgfx::TextureFormat::RG11B10F, BGFX_TEXTURE_RT | BGFX_SAMPLER_UVW_CLAMP);
		bloomDownsampleRTs[i] = Graphics_CreateRenderTarget(1, &bloomRTAttachment, &bloomDownsampleRTTextures[i], &bloomDownsampleRTTextureInfos[i]);
		if (i < BLOOM_STEP_COUNT - 1)
			bloomUpsampleRTs[i] = Graphics_CreateRenderTarget(1, &bloomRTAttachment, &bloomUpsampleRTTextures[i], &bloomUpsampleRTTextureInfos[i]);

		bloomRTWidth /= 2;
		bloomRTHeight /= 2;
	}
}

RFAPI void Renderer3D_Terminate()
{
	if (gbuffer != bgfx::kInvalidHandle)
		bgfx::destroy(bgfx::FrameBufferHandle{ gbuffer });
	if (hzb != bgfx::kInvalidHandle)
		bgfx::destroy(bgfx::TextureHandle{ hzb });
	if (forwardRT != bgfx::kInvalidHandle)
		bgfx::destroy(bgfx::FrameBufferHandle{ forwardRT });
	if (compositeRT != bgfx::kInvalidHandle)
		bgfx::destroy(bgfx::FrameBufferHandle{ compositeRT });
	if (tonemappingRT != bgfx::kInvalidHandle)
		bgfx::destroy(bgfx::FrameBufferHandle{ tonemappingRT });
	if (ssaoRT != bgfx::kInvalidHandle)
		bgfx::destroy(bgfx::FrameBufferHandle{ ssaoRT });
	if (ssaoBlurRT != bgfx::kInvalidHandle)
		bgfx::destroy(bgfx::FrameBufferHandle{ ssaoBlurRT });
	for (int i = 0; i < BLOOM_STEP_COUNT; i++)
	{
		if (bloomDownsampleRTs[i] != bgfx::kInvalidHandle)
			Graphics_DestroyRenderTarget(bloomDownsampleRTs[i]);
		if (i < BLOOM_STEP_COUNT - 1 && bloomUpsampleRTs[i] != bgfx::kInvalidHandle)
			Graphics_DestroyRenderTarget(bloomUpsampleRTs[i]);
	}

	Graphics_DestroyShader(defaultShader);
	Graphics_DestroyShader(defaultAnimatedShader);
	Graphics_DestroyShader(deferredPointShader);
	Graphics_DestroyShader(deferredDirectionalShader);
	Graphics_DestroyShader(deferredEmissiveShader);
	Graphics_DestroyShader(deferredEnvironmentShader);
	Graphics_DestroyShader(hzbDownsampleShader);
	Graphics_DestroyShader(meshIndirectShader);
	Graphics_DestroyShader(lightIndirectShader);
	Graphics_DestroyShader(tonemappingShader);
	Graphics_DestroyShader(particleShader);
	Graphics_DestroyShader(skyShader);
	Graphics_DestroyShader(streamCompactionShader);

	Graphics_DestroyVertexBuffer(quad);
	Graphics_DestroyVertexBuffer(box);
	Graphics_DestroyIndexBuffer(boxIBO);
	Graphics_DestroyVertexBuffer(particle);
	Graphics_DestroyVertexBuffer(skydome);
	Graphics_DestroyIndexBuffer(skydomeIBO);
	Graphics_DestroyTexture(emptyCubemap);
	Graphics_DestroyIndirectBuffer(indirectBuffer.idx);

	Graphics_DestroyIndirectBuffer(indirectBuffer.idx);
	Graphics_DestroyDynamicVertexBuffer(aabbBuffer.idx);

	Graphics_DestroyIndirectBuffer(lightIndirectBuffer.idx);
	Graphics_DestroyDynamicVertexBuffer(lightBuffer.idx);
	Graphics_DestroyDynamicIndexBuffer(lightInstanceCount.idx);
	Graphics_DestroyDynamicIndexBuffer(lightInstancePredicates.idx);
	Graphics_DestroyDynamicVertexBuffer(lightCulledInstanceBuffer.idx);

	Graphics_DestroyIndirectBuffer(particleIndirectBuffer.idx);
	Graphics_DestroyDynamicVertexBuffer(particleAabbBuffer.idx);
}

RFAPI void Renderer3D_SetSettings(Renderer3DSettings settings)
{
	::settings = settings;
}

RFAPI void Renderer3D_SetCamera(Vector3 position, Quaternion rotation, Matrix proj, float fov, float aspect, float near, float far)
{
	cameraFOV = fov;
	cameraAspect = aspect;
	cameraNear = near;
	cameraFar = far;
	cameraPosition = position;
	cameraRotation = rotation;
	cameraRight = rotation.right();
	cameraUp = rotation.up();
	cameraForward = rotation.forward();
	projection = proj;
	view = (Matrix::Translate(position) * Matrix::Rotate(rotation)).inverted();
	pv = projection * view;
	GetFrustumPlanes(pv, frustumPlanes);
}

RFAPI void Renderer3D_DrawMesh(MeshData* mesh, Matrix transform, Material* material, AnimationState* animation, bool isOccluder)
{
	//if (mesh->node)
	//	transform = transform * mesh->node->transform;

	AABB worldSpaceBoundingBox = TransformBoundingBox(mesh->boundingBox, transform);

	if (isOccluder)
		occluderMeshes.add({ mesh, transform, worldSpaceBoundingBox, material, animation });
	else if (material->isForward)
		forwardDraws.add({ mesh, transform, worldSpaceBoundingBox, material, animation });
	else
		meshDraws.add({ mesh, transform, worldSpaceBoundingBox, material, animation });
}

RFAPI void Renderer3D_DrawScene(SceneData* scene, Matrix transform, AnimationState* animation, bool isOccluder)
{
	for (int i = 0; i < scene->numMeshes; i++)
	{
		MeshData* mesh = &scene->meshes[i];
		Material* material = Material_GetDefault();
		if (mesh->materialID != -1)
			material = Material_GetForData(&scene->materials[mesh->materialID]);
		Renderer3D_DrawMesh(mesh, transform, material, animation, isOccluder);
	}
}

static float CalculateLightRadius(Vector3 color)
{
	/// 
	/// I * attenuation = 0.01
	/// attenuation = 0.01 / I
	/// 1 / attenuation = I / 0.01
	/// 1 + 4 * d2 = I / 0.01
	/// 4 * d2 = I / 0.01 - 1
	/// 2 * d = sqrt(I / 0.01 - 1)
	/// 

	float maxComponent = fmaxf(color.x, fmaxf(color.y, color.z));
	float brightnessCap = 0.01f;
	float radius = sqrtf(maxComponent / brightnessCap - 1) / sqrtf(2);
	//radius *= 0.1f;
	return radius;
}

RFAPI void Renderer3D_DrawPointLight(Vector3 position, Vector3 color)
{
	float radius = CalculateLightRadius(color);
	pointLightDraws.add({ position, color, radius });
}

RFAPI void Renderer3D_DrawDirectionalLight(Vector3 direction, Vector3 color, bool shadowsNeedUpdate, int shadowMapRes, RenderTarget cascade0, RenderTarget cascade1, RenderTarget cascade2)
{
	renderDirectionalLight = true;
	directionalLightDirection = direction;
	directionalLightColor = color;
	directionalLightShadowsNeedUpdate = shadowsNeedUpdate;
	directionalLightShadowMapRes = shadowMapRes;
	directionalLightShadowMapRTs[0] = { cascade0 };
	directionalLightShadowMapRTs[1] = { cascade1 };
	directionalLightShadowMapRTs[2] = { cascade2 };
}

RFAPI void Renderer3D_DrawParticleSystem(ParticleSystem* particleSystem)
{
	particleSystemDraws.add({ particleSystem });
}

RFAPI void Renderer3D_DrawSky(uint16_t sky, float intensity, Quaternion rotation)
{
	skyTexture = sky;
	skyIntensity = intensity;
	skyTransform = Matrix::Rotate(rotation);
}

RFAPI void Renderer3D_DrawCloth(Cloth* cloth, Material* material, Vector3 position, Quaternion rotation)
{
	clothDraws.add({ cloth, material, position, rotation });
}

RFAPI void Renderer3D_DrawEnvironmentMap(uint16_t envir, float intensity)
{
	environmentMap = envir;
	environmentIntensity = intensity;
}

RFAPI void Renderer3D_DrawEnvironmentMapMask(Vector3 position, Vector3 size, float falloff)
{
	environmentMapMasks.add({ position, size, falloff });
}

RFAPI void Renderer3D_DrawReflectionProbe(Vector3 position, Vector3 size)
{
	reflectionProbeDraws.add({ position, size });
}

RFAPI void Renderer3D_DrawDebugLine(Vector3 position0, Vector3 position1, uint32_t color)
{
	debugLineDraws.add({ position0, position1, color });
}

RFAPI void Renderer3D_Begin()
{
}

static int PointLightComparator(const PointLightDraw* a, const PointLightDraw* b)
{
	Vector3 delta1 = a->position - cameraPosition;
	Vector3 delta2 = b->position - cameraPosition;
	float d1 = dot(delta1, delta1);
	float d2 = dot(delta2, delta2);
	return d1 < d2 ? -1 : d1 > d2 ? 1 : 0;
}

static bool FrustumCulling(const Sphere& boundingSphere, Vector4 planes[6])
{
	Vector3 boundingSpherePos = boundingSphere.center;
	float boundingSphereRadius = boundingSphere.radius;

	for (int i = 0; i < 6; i++)
	{
		float distance = boundingSpherePos.x * planes[i].x + boundingSpherePos.y * planes[i].y + boundingSpherePos.z * planes[i].z;
		float l = sqrtf(planes[i].x * planes[i].x + planes[i].y * planes[i].y + planes[i].z * planes[i].z);
		distance += planes[i].w / l;
		if (distance + boundingSphereRadius < 0.0f)
			return false;
	}
	return true;
}

static bool FrustumCulling(const Sphere& boundingSphere, Matrix transform, Vector4 planes[6])
{
	Vector4 boundingSpherePos = (transform * Vector4(boundingSphere.center, 1.0f));
	float boundingSphereRadius = sqrtf(transform.m00 * transform.m00 + transform.m01 * transform.m01 + transform.m02 * transform.m02) * boundingSphere.radius;

	for (int i = 0; i < 6; i++)
	{
		float distance = boundingSpherePos.x * planes[i].x + boundingSpherePos.y * planes[i].y + boundingSpherePos.z * planes[i].z;
		float l = sqrtf(planes[i].x * planes[i].x + planes[i].y * planes[i].y + planes[i].z * planes[i].z);
		distance += planes[i].w / l;
		if (distance + boundingSphereRadius < 0.0f)
			return false;
	}
	return true;
}

static int MeshDrawComparator(const MeshDraw* a, const MeshDraw* b)
{
	int ia = a->culled * 1;
	int ib = b->culled * 1;
	return ia < ib ? -1 : ia > ib ? 1 : 0;
}

static int PointLightDrawComparator(const PointLightDraw* a, const PointLightDraw* b)
{
	int ia = a->culled * 1;
	int ib = b->culled * 1;
	return ia < ib ? -1 : ia > ib ? 1 : 0;
}

static int ParticleSystemDrawComparator(const ParticleSystemDraw* a, const ParticleSystemDraw* b)
{
	int ia = a->culled * 1;
	int ib = b->culled * 1;
	return ia < ib ? -1 : ia > ib ? 1 : 0;
}

static void FrustumCullObjects()
{
	numVisibleMeshes = 0;
	for (int i = 0; i < meshDraws.size; i++)
	{
		meshDraws[i].culled = !FrustumCulling(meshDraws[i].mesh->boundingSphere, meshDraws[i].transform, frustumPlanes);
		if (!meshDraws[i].culled)
			numVisibleMeshes++;
	}
	for (int i = 0; i < occluderMeshes.size; i++)
	{
		occluderMeshes[i].culled = !FrustumCulling(occluderMeshes[i].mesh->boundingSphere, occluderMeshes[i].transform, frustumPlanes);
	}

	qsort(meshDraws.buffer, meshDraws.size, sizeof(MeshDraw), (_CoreCrtNonSecureSearchSortCompareFunction)MeshDrawComparator);
	qsort(occluderMeshes.buffer, occluderMeshes.size, sizeof(MeshDraw), (_CoreCrtNonSecureSearchSortCompareFunction)MeshDrawComparator);

	numVisibleLights = 0;
	for (int i = 0; i < pointLightDraws.size; i++)
	{
		Sphere boundingSphere(pointLightDraws[i].position, pointLightDraws[i].radius);
		pointLightDraws[i].culled = !FrustumCulling(boundingSphere, frustumPlanes);
		if (!pointLightDraws[i].culled)
			numVisibleLights++;
	}

	qsort(pointLightDraws.buffer, pointLightDraws.size, sizeof(PointLightDraw), (_CoreCrtNonSecureSearchSortCompareFunction)PointLightDrawComparator);

	numVisibleParticleSystems = 0;
	for (int i = 0; i < particleSystemDraws.size; i++)
	{
		Sphere boundingSphere = particleSystemDraws[i].system->boundingSphere;
		particleSystemDraws[i].culled = !FrustumCulling(boundingSphere, frustumPlanes);
		if (!particleSystemDraws[i].culled)
			numVisibleParticleSystems++;
	}

	qsort(particleSystemDraws.buffer, particleSystemDraws.size, sizeof(ParticleSystemDraw), (_CoreCrtNonSecureSearchSortCompareFunction)ParticleSystemDrawComparator);
}

static void DrawMesh(MeshData* mesh, Matrix transform, Material* material, SkeletonState* skeleton, bgfx::ViewId view, bgfx::IndirectBufferHandle indirect = BGFX_INVALID_HANDLE, int indirectID = 0)
{
	Graphics_SetCullState(CullState::ClockWise);

	bgfx::setTransform(&transform.m00);

	if (mesh->vertexNormalTangentBuffer.idx != bgfx::kInvalidHandle)
		bgfx::setVertexBuffer(0, mesh->vertexNormalTangentBuffer);
	if (mesh->texcoordBuffer.idx != bgfx::kInvalidHandle)
		bgfx::setVertexBuffer(1, mesh->texcoordBuffer);
	if (mesh->vertexColorBuffer.idx != bgfx::kInvalidHandle)
		bgfx::setVertexBuffer(2, mesh->vertexColorBuffer);
	if (mesh->boneWeightBuffer.idx != bgfx::kInvalidHandle)
		bgfx::setVertexBuffer(3, mesh->boneWeightBuffer);

	bgfx::setIndexBuffer(mesh->indexBuffer);


	bool hasDiffuse = material->textures[0].idx != bgfx::kInvalidHandle;
	bool hasNormal = material->textures[1].idx != bgfx::kInvalidHandle;
	bool hasRoughness = material->textures[2].idx != bgfx::kInvalidHandle;
	bool hasMetallic = material->textures[3].idx != bgfx::kInvalidHandle;
	bool hasEmissive = material->textures[4].idx != bgfx::kInvalidHandle;
	bool hasHeight = material->textures[5].idx != bgfx::kInvalidHandle;

	Vector4 attributeInfo0(hasDiffuse, hasNormal, hasRoughness, hasMetallic);
	Vector4 attributeInfo1(
		hasEmissive,
		hasHeight,
		0,
		mesh->texcoordBuffer.idx != bgfx::kInvalidHandle
	);

	bgfx::setUniform(material->shader->getUniform("u_attributeInfo0", bgfx::UniformType::Vec4), &attributeInfo0);
	bgfx::setUniform(material->shader->getUniform("u_attributeInfo1", bgfx::UniformType::Vec4), &attributeInfo1);

	bgfx::setUniform(material->shader->getUniform("u_materialData0", bgfx::UniformType::Vec4), &material->materialData[0]);
	bgfx::setUniform(material->shader->getUniform("u_materialData1", bgfx::UniformType::Vec4), &material->materialData[1]);
	bgfx::setUniform(material->shader->getUniform("u_materialData2", bgfx::UniformType::Vec4), &material->materialData[2]);
	bgfx::setUniform(material->shader->getUniform("u_materialData3", bgfx::UniformType::Vec4), &material->materialData[3]);

	if (hasDiffuse)
		bgfx::setTexture(0, material->shader->getUniform("s_diffuse", bgfx::UniformType::Sampler), material->textures[0], UINT32_MAX);
	if (hasNormal)
		bgfx::setTexture(1, material->shader->getUniform("s_normal", bgfx::UniformType::Sampler), material->textures[1], UINT32_MAX);
	if (hasRoughness)
		bgfx::setTexture(2, material->shader->getUniform("s_roughness", bgfx::UniformType::Sampler), material->textures[2], UINT32_MAX);
	if (hasMetallic)
		bgfx::setTexture(3, material->shader->getUniform("s_metallic", bgfx::UniformType::Sampler), material->textures[3], UINT32_MAX);
	if (hasEmissive)
		bgfx::setTexture(4, material->shader->getUniform("s_emissive", bgfx::UniformType::Sampler), material->textures[4], UINT32_MAX);
	if (hasHeight)
		bgfx::setTexture(5, material->shader->getUniform("s_height", bgfx::UniformType::Sampler), material->textures[5], UINT32_MAX);

	if (skeleton)
		bgfx::setUniform(material->shader->getUniform("u_boneTransforms", bgfx::UniformType::Mat4, MAX_BONES), skeleton->boneTransforms, skeleton->numBones);

	Vector4 u_cameraPosition(cameraPosition, 0);
	Graphics_SetUniform(material->shader->getUniform("u_cameraPosition", bgfx::UniformType::Vec4), &u_cameraPosition);

	if (indirect.idx != bgfx::kInvalidHandle)
		bgfx::submit(view, material->shader->program, indirect, indirectID, 1);
	else
		bgfx::submit(view, material->shader->program);
}

static void DoDepthPrepass()
{
	Graphics_ResetState();
	Graphics_SetRenderTarget(RenderPass::DepthPrepass, gbuffer, width, height);
	Graphics_ClearRenderTarget(RenderPass::DepthPrepass, gbuffer, true, true, 0, 1);
	Graphics_SetViewTransform(RenderPass::DepthPrepass, projection, view);

	for (int i = 0; i < occluderMeshes.size; i++)
	{
		if (occluderMeshes[i].culled)
			break;

		MeshData* mesh = occluderMeshes[i].mesh;
		Material* material = occluderMeshes[i].material;
		AnimationState* animation = occluderMeshes[i].animation;

		Matrix transform = occluderMeshes[i].transform;

		SkeletonState* skeleton = nullptr;
		if (animation && mesh->skeletonID != -1)
			skeleton = animation->skeletons[mesh->skeletonID];

		bgfx::setMarker("Depth Prepass");
		DrawMesh(mesh, transform, material, skeleton, RenderPass::DepthPrepass);
	}
}

static void BuildHZB()
{
	int numDownsamples = (int)floorf(log2f((float)max(hzbWidth, hzbHeight))) + 1;
	int w = hzbWidth;
	int h = hzbHeight;
	for (int i = 0; i < numDownsamples; i++)
	{
		Graphics_ResetState();

		uint16_t src = i == 0 ? gbufferTextures[4].idx : hzb;
		uint16_t dst = hzb;

		Graphics_SetComputeTexture(0, src, max(i - 1, 0), bgfx::Access::Read);
		Graphics_SetComputeTexture(1, dst, i, bgfx::Access::Write);

		bgfx::setMarker("HZB Downsample");
		Graphics_ComputeDispatch(RenderPass::HZB, hzbDownsampleShader, w / 16 + 1, h / 16 + 1, 1);

		w /= 2;
		h /= 2;
	}
}

static void CullMeshes()
{
	if (numVisibleMeshes == 0)
		return;

	if (numVisibleMeshes > MAX_INDIRECT_DRAWS)
		__debugbreak();

	const bgfx::Memory* aabbBufferMem = bgfx::alloc(numVisibleMeshes * 2 * sizeof(Vector4));
	Vector4* aabbBufferData = (Vector4*)aabbBufferMem->data;
	for (int i = 0; i < numVisibleMeshes; i++)
	{
		aabbBufferData[i * 2 + 0].xyz = meshDraws[i].boundingBox.min;
		aabbBufferData[i * 2 + 0].w = (float)meshDraws[i].mesh->indexCount;
		aabbBufferData[i * 2 + 1].xyz = meshDraws[i].boundingBox.max;
		aabbBufferData[i * 2 + 1].w = (float)numVisibleMeshes;
	}

	bgfx::update(aabbBuffer, 0, aabbBufferMem);

	Graphics_ResetState();

	bgfx::setBuffer(0, aabbBuffer, bgfx::Access::Read);
	bgfx::setBuffer(1, indirectBuffer, bgfx::Access::Write);

	Graphics_SetTexture(meshIndirectShader->getUniform("s_hzb", bgfx::UniformType::Sampler), 2, bgfx::TextureHandle{ hzb });

	Vector4 params((float)numVisibleMeshes, 0, 0, 0);
	Graphics_SetUniform(meshIndirectShader->getUniform("u_meshParams", bgfx::UniformType::Vec4), &params);

	Graphics_SetUniform(meshIndirectShader->getUniform("u_pv", bgfx::UniformType::Mat4), &pv);

	bgfx::setMarker("Mesh Culling");
	Graphics_ComputeDispatch(RenderPass::MeshCulling, meshIndirectShader, numVisibleMeshes / 64 + 1, 1, 1);
}

static void CullLights()
{
	if (numVisibleLights == 0)
		return;

	if (numVisibleLights > MAX_POINT_LIGHTS)
		__debugbreak();

	const bgfx::Memory* lightBufferMem = bgfx::alloc(numVisibleLights * 2 * sizeof(Vector4));
	Vector4* lightBufferData = (Vector4*)lightBufferMem->data;
	for (int i = 0; i < numVisibleLights; i++)
	{
		lightBufferData[i * 2 + 0].xyz = pointLightDraws[i].position;
		lightBufferData[i * 2 + 0].w = pointLightDraws[i].radius;
		lightBufferData[i * 2 + 1].xyz = pointLightDraws[i].color;
		lightBufferData[i * 2 + 1].w = (float)numVisibleLights;
	}

	bgfx::update(lightBuffer, 0, lightBufferMem);

	Graphics_ResetState();

	bgfx::setBuffer(0, lightBuffer, bgfx::Access::Read);
	bgfx::setBuffer(1, lightInstanceCount, bgfx::Access::ReadWrite);
	bgfx::setBuffer(2, lightInstancePredicates, bgfx::Access::Write);

	bgfx::setTexture(3, s_hzb, bgfx::TextureHandle{ hzb });

	int instancesPowOf2 = ipow(2, (int)ceil(log2((double)numVisibleLights)));
	Vector4 params((float)numVisibleLights, (float)instancesPowOf2, 0, 1);
	bgfx::setUniform(u_params, &params);

	bgfx::setUniform(u_pv, &pv);

	bgfx::setMarker("Light Culling");
	bgfx::dispatch((bgfx::ViewId)RenderPass::LightCulling, lightIndirectShader->program, MAX_POINT_LIGHTS / 64 + 1, 1, 1);

	// Stream compaction

	bgfx::setBuffer(1, lightBuffer, bgfx::Access::Read);
	bgfx::setBuffer(2, lightInstancePredicates, bgfx::Access::Read);

	bgfx::setBuffer(3, lightInstanceCount, bgfx::Access::ReadWrite);
	bgfx::setBuffer(4, lightIndirectBuffer, bgfx::Access::ReadWrite);
	bgfx::setBuffer(5, lightCulledInstanceBuffer, bgfx::Access::Write);

	Graphics_SetUniform(streamCompactionShader->getUniform("u_params", bgfx::UniformType::Vec4), &params);

	bgfx::setMarker("Stream Compaction");
	Graphics_ComputeDispatch(RenderPass::StreamCompaction, streamCompactionShader, 1, 1, 1);
}

static void CullParticles()
{
	if (numVisibleParticleSystems == 0)
		return;

	if (numVisibleParticleSystems > MAX_PARTICLE_SYSTEMS)
		__debugbreak();

	const bgfx::Memory* aabbBufferMem = bgfx::alloc(numVisibleParticleSystems * 2 * sizeof(Vector4));
	Vector4* aabbBufferData = (Vector4*)aabbBufferMem->data;
	for (int i = 0; i < numVisibleParticleSystems; i++)
	{
		aabbBufferData[i * 2 + 0].xyz = particleSystemDraws[i].system->boundingBox.min;
		aabbBufferData[i * 2 + 0].w = (float)particleSystemDraws[i].system->numParticles;
		aabbBufferData[i * 2 + 1].xyz = particleSystemDraws[i].system->boundingBox.max;
		aabbBufferData[i * 2 + 1].w = (float)numVisibleParticleSystems;
	}

	bgfx::update(particleAabbBuffer, 0, aabbBufferMem);

	Graphics_ResetState();

	bgfx::setBuffer(0, particleAabbBuffer, bgfx::Access::Read);
	bgfx::setBuffer(1, particleIndirectBuffer, bgfx::Access::Write);

	Graphics_SetTexture(particleIndirectShader->getUniform("s_hzb", bgfx::UniformType::Sampler), 2, bgfx::TextureHandle{ hzb });

	Vector4 params((float)numVisibleParticleSystems, 0, 0, 0);
	Graphics_SetUniform(particleIndirectShader->getUniform("u_params", bgfx::UniformType::Vec4), &params);

	Graphics_SetUniform(particleIndirectShader->getUniform("u_pv", bgfx::UniformType::Mat4), &pv);

	bgfx::setMarker("Particle Culling");
	Graphics_ComputeDispatch(RenderPass::ParticleCulling, particleIndirectShader, numVisibleParticleSystems / 64 + 1, 1, 1);
}

static void DrawCloth(Cloth* cloth, const Matrix& transform, Material* material, bgfx::ViewId view)
{
	Shader* shader = clothShader;

	Graphics_SetCullState(CullState::None);

	bgfx::setTransform(&transform.m00);

	if (cloth->mesh->vertexNormalTangentBuffer.idx != bgfx::kInvalidHandle)
		bgfx::setVertexBuffer(0, cloth->mesh->vertexNormalTangentBuffer);
	if (cloth->mesh->texcoordBuffer.idx != bgfx::kInvalidHandle)
		bgfx::setVertexBuffer(1, cloth->mesh->texcoordBuffer);
	bgfx::setVertexBuffer(2, cloth->animatedPosition);
	bgfx::setVertexBuffer(3, cloth->animatedNormalTangent);

	bgfx::setIndexBuffer(cloth->mesh->indexBuffer);


	bool hasDiffuse = material->textures[0].idx != bgfx::kInvalidHandle;
	bool hasNormal = material->textures[1].idx != bgfx::kInvalidHandle;
	bool hasRoughness = material->textures[2].idx != bgfx::kInvalidHandle;
	bool hasMetallic = material->textures[3].idx != bgfx::kInvalidHandle;
	bool hasEmissive = material->textures[4].idx != bgfx::kInvalidHandle;
	bool hasHeight = material->textures[5].idx != bgfx::kInvalidHandle;

	Vector4 attributeInfo0(hasDiffuse, hasNormal, hasRoughness, hasMetallic);
	Vector4 attributeInfo1(
		hasEmissive,
		hasHeight,
		0,
		cloth->mesh->texcoordBuffer.idx != bgfx::kInvalidHandle
	);

	bgfx::setUniform(shader->getUniform("u_attributeInfo0", bgfx::UniformType::Vec4), &attributeInfo0);
	bgfx::setUniform(shader->getUniform("u_attributeInfo1", bgfx::UniformType::Vec4), &attributeInfo1);

	bgfx::setUniform(shader->getUniform("u_materialData0", bgfx::UniformType::Vec4), &material->materialData[0]);
	bgfx::setUniform(shader->getUniform("u_materialData1", bgfx::UniformType::Vec4), &material->materialData[1]);
	bgfx::setUniform(shader->getUniform("u_materialData2", bgfx::UniformType::Vec4), &material->materialData[2]);
	bgfx::setUniform(shader->getUniform("u_materialData3", bgfx::UniformType::Vec4), &material->materialData[3]);

	if (hasDiffuse)
		bgfx::setTexture(0, shader->getUniform("s_diffuse", bgfx::UniformType::Sampler), material->textures[0], UINT32_MAX);
	if (hasNormal)
		bgfx::setTexture(1, shader->getUniform("s_normal", bgfx::UniformType::Sampler), material->textures[1], UINT32_MAX);
	if (hasRoughness)
		bgfx::setTexture(2, shader->getUniform("s_roughness", bgfx::UniformType::Sampler), material->textures[2], UINT32_MAX);
	if (hasMetallic)
		bgfx::setTexture(3, shader->getUniform("s_metallic", bgfx::UniformType::Sampler), material->textures[3], UINT32_MAX);
	if (hasEmissive)
		bgfx::setTexture(4, shader->getUniform("s_emissive", bgfx::UniformType::Sampler), material->textures[4], UINT32_MAX);
	if (hasHeight)
		bgfx::setTexture(5, shader->getUniform("s_height", bgfx::UniformType::Sampler), material->textures[5], UINT32_MAX);

	Vector4 u_cameraPosition(cameraPosition, 0);
	Graphics_SetUniform(shader->getUniform("u_cameraPosition", bgfx::UniformType::Vec4), &u_cameraPosition);

	bgfx::submit(view, shader->program);
}

static void GeometryPass()
{
	Graphics_ResetState();

	Graphics_SetRenderTarget(RenderPass::Geometry, gbuffer, width, height);
	Graphics_SetViewTransform(RenderPass::Geometry, projection, view);

	bgfx::setMarker("Geometry Pass");

	for (int i = 0; i < numVisibleMeshes; i++)
	{
		MeshData* mesh = meshDraws[i].mesh;
		Material* material = meshDraws[i].material;
		AnimationState* animation = meshDraws[i].animation;

		Matrix transform = meshDraws[i].transform;

		SkeletonState* skeleton = nullptr;
		if (animation && mesh->skeletonID != -1)
			skeleton = animation->skeletons[mesh->skeletonID];

		DrawMesh(mesh, transform, material, skeleton, RenderPass::Geometry, indirectBuffer, i);
	}

	for (int i = 0; i < clothDraws.size; i++)
	{
		Cloth* cloth = clothDraws[i].cloth;
		Material* material = clothDraws[i].material;

		Matrix transform = Matrix::Transform(clothDraws[i].position, clothDraws[i].rotation, Vector3::One);

		DrawCloth(cloth, transform, material, RenderPass::Geometry);
	}
}

void CalculateCascade(float near, float far, Matrix* lightProjection, Matrix* lightView)
{
	Vector3 forward = cameraRotation.forward();
	Vector3 up = cameraRotation.up();
	Vector3 right = cameraRotation.right();

	float halfHeight = tanf(cameraFOV * 0.5f / 180.0f * PI);
	float farHalfHeight = far * halfHeight;
	float nearHalfHeight = near * halfHeight;

	float farHalfWidth = farHalfHeight * cameraAspect;
	float nearHalfWidth = nearHalfHeight * cameraAspect;

	Vector3 centerFar = cameraPosition + forward * far;
	Vector3 centerNear = cameraPosition + forward * near;

	Vector3 corners[8];
	corners[0] = centerFar + up * farHalfHeight + right * farHalfWidth;
	corners[1] = centerFar + up * farHalfHeight - right * farHalfWidth;
	corners[2] = centerFar - up * farHalfHeight + right * farHalfWidth;
	corners[3] = centerFar - up * farHalfHeight - right * farHalfWidth;
	corners[4] = centerNear + up * nearHalfHeight + right * nearHalfWidth;
	corners[5] = centerNear + up * nearHalfHeight - right * nearHalfWidth;
	corners[6] = centerNear - up * nearHalfHeight + right * nearHalfWidth;
	corners[7] = centerNear - up * nearHalfHeight - right * nearHalfWidth;

	Quaternion lightRotation = Quaternion::LookAt(Vector3::Zero, directionalLightDirection);
	Quaternion lightRotationInv = lightRotation.conjugated();

	for (int i = 0; i < 8; i++)
		corners[i] = lightRotationInv * corners[i];

	Vector3 vmin = corners[0];
	Vector3 vmax = corners[0];
	for (int i = 0; i < 8; i++)
	{
		vmin = min(vmin, corners[i]);
		vmax = max(vmax, corners[i]);
	}

	Vector3 center = 0.5f * (vmin + vmax);

	Vector3 localMin = vmin - center;
	Vector3 localMax = vmax - center;

	Vector3 size = vmax - vmin;
	Vector3 unitsPerTexel = size / (float)directionalLightShadowMapRes;
	//localMin = Vector3.Floor(localMin / unitsPerTexel) * unitsPerTexel;
	//localMax = Vector3.Floor(localMax / unitsPerTexel) * unitsPerTexel;

	Vector3 boxPosition = lightRotation * center;

	*lightProjection = Matrix::Orthographic(localMin.x, localMax.x, localMin.y, localMax.y, localMin.z, localMax.z);
	*lightView = (Matrix::Translate(boxPosition) * Matrix::Rotate(lightRotation)).inverted();
}

static void CalculateCascadeMatrices(Vector3 position, Quaternion rotation, float fov, float aspect, Matrix projections[3], Matrix views[3])
{
	static float NEAR_PLANES[3] = { -40.0f, 30.0f, 80.0f };
	static float FAR_PLANES[3] = { 40.0f, 100.0f, 200.0f };

	for (int i = 0; i < 3; i++)
	{
		CalculateCascade(NEAR_PLANES[i], FAR_PLANES[i], &projections[i], &views[i]);
		directionalLightShadowMapFar[i] = FAR_PLANES[i];
	}
}

static void UpdateDirectionalShadows()
{
	if (renderDirectionalLight)
	{
		if (!directionalLightShadowsNeedUpdate)
			return;

		Matrix cascadeProjections[3];
		Matrix cascadeViews[3];

		CalculateCascadeMatrices(cameraPosition, cameraRotation, cameraFOV, cameraAspect, cascadeProjections, cascadeViews);

		for (int i = 0; i < 3; i++)
		{
			Graphics_ResetState();

			Graphics_SetRenderTarget(RenderPass::Shadow0 + i, directionalLightShadowMapRTs[i], directionalLightShadowMapRes, directionalLightShadowMapRes);
			Graphics_ClearRenderTarget(RenderPass::Shadow0 + i, directionalLightShadowMapRTs[i].idx, false, true, 0, 1);

			Matrix cascadePV = cascadeProjections[i] * cascadeViews[i];
			Graphics_SetViewTransform(RenderPass::Shadow0 + i, cascadeProjections[i], cascadeViews[i]);
			directionalLightShadowMapMatrix[i] = cascadePV;

			for (int j = 0; j < meshDraws.size; j++)
			{
				MeshData* mesh = meshDraws[j].mesh;
				Material* material = meshDraws[j].material;
				AnimationState* animation = meshDraws[j].animation;

				Matrix transform = meshDraws[j].transform;

				SkeletonState* skeleton = nullptr;
				if (animation && mesh->skeletonID != -1)
					skeleton = animation->skeletons[mesh->skeletonID];

				DrawMesh(mesh, transform, material, skeleton, RenderPass::Shadow0 + i);
			}
		}
	}
}

static void ShadowPass()
{
	UpdateDirectionalShadows();
}

static void AmbientOcclusionPass()
{
	if (!settings.ssaoEnabled)
	{
		Graphics_ResetState();

		Graphics_SetRenderTarget(RenderPass::AmbientOcclusionBlur, ssaoBlurRT, ssaoBlurRTTextureInfo.width, ssaoBlurRTTextureInfo.height);
		Graphics_ClearRenderTarget(RenderPass::AmbientOcclusionBlur, ssaoBlurRT, true, false, 0xFFFFFFFF, 1);

		bgfx::touch(RenderPass::AmbientOcclusionBlur);

		return;
	}


	Graphics_ResetState();

	Graphics_SetRenderTarget(RenderPass::AmbientOcclusion, ssaoRT, ssaoRTTextureInfo.width, ssaoRTTextureInfo.height);

	bgfx::setTexture(0, s_depth, gbufferTextures[4]);
	bgfx::setTexture(1, s_normals, gbufferTextures[1]);

	bgfx::setTexture(2, s_noise, blueNoise64, BGFX_SAMPLER_POINT);

	Vector4 cameraFrustum(cameraNear, cameraFar, 0.0f, 0.0f);
	Matrix pvInv = pv.inverted();
	Matrix viewInv = view.inverted();
	Matrix projectionInv = projection.inverted();

	bgfx::setUniform(u_cameraFrustum, &cameraFrustum);
	bgfx::setUniform(u_viewMatrix, &view);
	bgfx::setUniform(u_viewInv, &viewInv);
	bgfx::setUniform(u_projectionView, &pv);
	bgfx::setUniform(u_projectionInv, &projectionInv);
	bgfx::setUniform(u_projectionViewInv, &pvInv);

	bgfx::setVertexBuffer(0, bgfx::VertexBufferHandle{ quad });

	bgfx::submit((bgfx::ViewId)RenderPass::AmbientOcclusion, ssaoShader->program);


	Graphics_ResetState();

	Graphics_SetRenderTarget(RenderPass::AmbientOcclusionBlur, ssaoBlurRT, ssaoBlurRTTextureInfo.width, ssaoBlurRTTextureInfo.height);

	bgfx::setTexture(0, s_depth, gbufferTextures[4]);
	bgfx::setTexture(1, s_ao, ssaoRTTexture);

	bgfx::setUniform(u_cameraFrustum, &cameraFrustum);

	bgfx::setVertexBuffer(0, bgfx::VertexBufferHandle{ quad });

	bgfx::submit((bgfx::ViewId)RenderPass::AmbientOcclusionBlur, ssaoBlurShader->program);
}

static void RenderPointLights()
{
	if (numVisibleLights == 0)
		return;

	Shader* shader = deferredPointShader;

	Graphics_ResetState();

	Graphics_SetBlendState(BlendState::Additive);
	Graphics_SetDepthTest(DepthTest::None);
	Graphics_SetCullState(CullState::CounterClockWise);

	Graphics_SetVertexBuffer(sphere);
	Graphics_SetIndexBuffer(sphereIBO);

	Graphics_SetViewTransform(RenderPass::Deferred, projection, view);

	Graphics_SetTexture(shader->getUniform("s_gbuffer0", bgfx::UniformType::Sampler), 0, gbufferTextures[0]);
	Graphics_SetTexture(shader->getUniform("s_gbuffer1", bgfx::UniformType::Sampler), 1, gbufferTextures[1]);
	Graphics_SetTexture(shader->getUniform("s_gbuffer2", bgfx::UniformType::Sampler), 2, gbufferTextures[2]);
	Graphics_SetTexture(shader->getUniform("s_gbuffer3", bgfx::UniformType::Sampler), 3, gbufferTextures[3]);

	Graphics_SetTexture(shader->getUniform("s_ao", bgfx::UniformType::Sampler), 4, ssaoBlurRTTexture);

	Vector4 u_cameraPosition(cameraPosition, (float)numVisibleLights);
	Graphics_SetUniform(shader->getUniform("u_cameraPosition", bgfx::UniformType::Vec4), &u_cameraPosition);

	bgfx::setInstanceDataBuffer(lightCulledInstanceBuffer, 0, numVisibleLights);

	Graphics_DrawIndirect(RenderPass::Deferred, shader, lightIndirectBuffer.idx, 0, 1);
}

static void RenderDirectionalLights()
{
	if (renderDirectionalLight)
	{
		Shader* shader = deferredDirectionalShader;

		Graphics_ResetState();

		Graphics_SetBlendState(BlendState::Additive);
		Graphics_SetDepthTest(DepthTest::None);
		Graphics_SetCullState(CullState::ClockWise);

		Graphics_SetVertexBuffer(quad);

		Graphics_SetTexture(shader->getUniform("s_gbuffer0", bgfx::UniformType::Sampler), 0, gbufferTextures[0]);
		Graphics_SetTexture(shader->getUniform("s_gbuffer1", bgfx::UniformType::Sampler), 1, gbufferTextures[1]);
		Graphics_SetTexture(shader->getUniform("s_gbuffer2", bgfx::UniformType::Sampler), 2, gbufferTextures[2]);
		Graphics_SetTexture(shader->getUniform("s_gbuffer3", bgfx::UniformType::Sampler), 3, gbufferTextures[3]);

		Vector4 lightDirection(directionalLightDirection, 0);
		Vector4 lightColor(directionalLightColor, 0);
		Graphics_SetUniform(shader->getUniform("u_lightDirection", bgfx::UniformType::Vec4), &lightDirection);
		Graphics_SetUniform(shader->getUniform("u_lightColor", bgfx::UniformType::Vec4), &lightColor);

		Vector4 farPlanes;
		for (int i = 0; i < 3; i++)
		{
			char s_shadowMap[32];
			sprintf(s_shadowMap, "s_shadowMap%d", i);
			Graphics_SetTexture(shader->getUniform(s_shadowMap, bgfx::UniformType::Sampler), 4 + i, bgfx::getTexture(directionalLightShadowMapRTs[i]));

			char u_toLightSpace[32];
			sprintf(u_toLightSpace, "u_toLightSpace%d", i);
			Graphics_SetUniform(shader->getUniform(u_toLightSpace, bgfx::UniformType::Mat4), &directionalLightShadowMapMatrix[i]);

			farPlanes[i] = directionalLightShadowMapFar[i];
		}

		Graphics_SetUniform(shader->getUniform("u_params", bgfx::UniformType::Vec4), &farPlanes);

		Graphics_SetTexture(shader->getUniform("s_ao", bgfx::UniformType::Sampler), 7, ssaoBlurRTTexture);

		Vector4 u_cameraPosition(cameraPosition, 0);
		Graphics_SetUniform(shader->getUniform("u_cameraPosition", bgfx::UniformType::Vec4), &u_cameraPosition);

		Graphics_Draw(RenderPass::Deferred, shader);
	}
}

static void RenderEnvironmentMaps()
{
	if (environmentMap == bgfx::kInvalidHandle)
		return;

	Shader* shader = deferredEnvironmentShader;

	Graphics_ResetState();

	Graphics_SetBlendState(BlendState::Additive);
	Graphics_SetDepthTest(DepthTest::None);
	Graphics_SetCullState(CullState::ClockWise);

	Graphics_SetVertexBuffer(quad);

	Graphics_SetTexture(shader->getUniform("s_gbuffer0", bgfx::UniformType::Sampler), 0, gbufferTextures[0]);
	Graphics_SetTexture(shader->getUniform("s_gbuffer1", bgfx::UniformType::Sampler), 1, gbufferTextures[1]);
	Graphics_SetTexture(shader->getUniform("s_gbuffer2", bgfx::UniformType::Sampler), 2, gbufferTextures[2]);
	Graphics_SetTexture(shader->getUniform("s_gbuffer3", bgfx::UniformType::Sampler), 3, gbufferTextures[3]);

	Graphics_SetTexture(shader->getUniform("s_environmentMap", bgfx::UniformType::Sampler), 4, bgfx::TextureHandle{ environmentMap });

	int numEnvironmentMasks = min(environmentMapMasks.size, 4);

	Vector4 environmentData(environmentIntensity, (float)numEnvironmentMasks, 0, 0);
	Graphics_SetUniform(shader->getUniform("u_environmentData", bgfx::UniformType::Vec4), &environmentData);

	Vector4 maskPositions[4];
	Vector4 maskSizes[4];
	for (int i = 0; i < numEnvironmentMasks; i++)
	{
		maskPositions[i] = Vector4(environmentMapMasks[i].position, 0);
		maskSizes[i] = Vector4(environmentMapMasks[i].size, environmentMapMasks[i].falloff);
	}
	Graphics_SetUniform(shader->getUniform("u_maskPosition", bgfx::UniformType::Vec4, 4), maskPositions, numEnvironmentMasks);
	Graphics_SetUniform(shader->getUniform("u_maskSize", bgfx::UniformType::Vec4, 4), maskSizes, numEnvironmentMasks);

	Graphics_SetTexture(shader->getUniform("s_ao", bgfx::UniformType::Sampler), 5, ssaoBlurRTTexture);

	Vector4 u_cameraPosition(cameraPosition, 0);
	Graphics_SetUniform(shader->getUniform("u_cameraPosition", bgfx::UniformType::Vec4), &u_cameraPosition);

	Graphics_Draw(RenderPass::Deferred, shader);
}

static void RenderReflectionProbes()
{
	return;
	// TODO implement

	Shader* shader = deferredReflectionProbeShader;

	Graphics_SetViewTransform(RenderPass::Deferred, projection, view);

	for (int i = 0; i < reflectionProbeDraws.size; i++)
	{
		Graphics_ResetState();

		Graphics_SetBlendState(BlendState::Additive);
		Graphics_SetDepthTest(DepthTest::None);
		Graphics_SetCullState(CullState::CounterClockWise);

		Graphics_SetVertexBuffer(box);
		Graphics_SetIndexBuffer(boxIBO);

		Matrix boxTransform = Matrix::Translate(reflectionProbeDraws[i].position + reflectionProbeDraws[i].size * 0.5f) * Matrix::Scale(reflectionProbeDraws[i].size * 0.5f);
		Graphics_SetTransform(RenderPass::Deferred, boxTransform);

		Graphics_SetTexture(shader->getUniform("s_gbuffer0", bgfx::UniformType::Sampler), 0, gbufferTextures[0]);
		Graphics_SetTexture(shader->getUniform("s_gbuffer1", bgfx::UniformType::Sampler), 1, gbufferTextures[1]);
		Graphics_SetTexture(shader->getUniform("s_gbuffer2", bgfx::UniformType::Sampler), 2, gbufferTextures[2]);
		Graphics_SetTexture(shader->getUniform("s_gbuffer3", bgfx::UniformType::Sampler), 3, gbufferTextures[3]);

		Graphics_SetTexture(shader->getUniform("s_environmentMap", bgfx::UniformType::Sampler).idx, 4, environmentMap);

		Vector4 cubemapPosition(reflectionProbeDraws[i].position, 0);
		Vector4 cubemapSize(reflectionProbeDraws[i].size, 0);
		Vector4 cubemapOrigin(reflectionProbeDraws[i].position, 0);
		Graphics_SetUniform(shader->getUniform("u_cubemapPosition", bgfx::UniformType::Vec4), &cubemapPosition);
		Graphics_SetUniform(shader->getUniform("u_cubemapSize", bgfx::UniformType::Vec4), &cubemapSize);
		Graphics_SetUniform(shader->getUniform("u_cubemapOrigin", bgfx::UniformType::Vec4), &cubemapOrigin);

		Graphics_SetTexture(shader->getUniform("s_ao", bgfx::UniformType::Sampler), 5, ssaoBlurRTTexture);

		Vector4 u_cameraPosition(cameraPosition, 0);
		Graphics_SetUniform(shader->getUniform("u_cameraPosition", bgfx::UniformType::Vec4), &u_cameraPosition);

		Graphics_Draw(RenderPass::Deferred, shader);
	}
}

static void RenderEmissive()
{
	Shader* shader = deferredEmissiveShader;

	Graphics_ResetState();

	Graphics_SetBlendState(BlendState::Additive);
	Graphics_SetDepthTest(DepthTest::None);
	Graphics_SetCullState(CullState::ClockWise);

	Graphics_SetVertexBuffer(quad);

	Graphics_SetTexture(shader->getUniform("s_gbuffer1", bgfx::UniformType::Sampler), 1, gbufferTextures[1]);
	Graphics_SetTexture(shader->getUniform("s_gbuffer3", bgfx::UniformType::Sampler), 3, gbufferTextures[3]);

	Graphics_Draw(RenderPass::Deferred, shader);
}

static void DeferredPass()
{
	Graphics_SetRenderTarget(RenderPass::Deferred, deferredRT, width, height);
	Graphics_ClearRenderTarget(RenderPass::Deferred, deferredRT, true, true, 0, 1);

	RenderPointLights();
	RenderDirectionalLights();
	RenderEnvironmentMaps();
	RenderReflectionProbes();
	RenderEmissive();
}

static void RenderForwardMeshes()
{
	for (int i = 0; i < forwardDraws.size; i++)
	{
		MeshData* mesh = forwardDraws[i].mesh;
		Material* material = forwardDraws[i].material;
		AnimationState* animation = forwardDraws[i].animation;

		Matrix transform = forwardDraws[i].transform;

		SkeletonState* skeleton = nullptr;
		if (animation && mesh->skeletonID != -1)
			skeleton = animation->skeletons[mesh->skeletonID];

		Graphics_SetTexture(material->shader->getUniform("s_deferredFrame", bgfx::UniformType::Sampler), 0, deferredRTTextures[0]);
		Graphics_SetTexture(material->shader->getUniform("s_deferredDepth", bgfx::UniformType::Sampler), 1, gbufferTextures[4]);

		DrawMesh(mesh, transform, material, skeleton, RenderPass::Forward);
	}
}

struct ParticleInstanceData
{
	Vector3 position;
	float rotation;
	Vector4 color;
	Vector4 sizeAnimation;
};

static int ParticleComparator(const ParticleInstanceData* a, const ParticleInstanceData* b)
{
	float d1 = dot(a->position, cameraForward);
	float d2 = dot(b->position, cameraForward);

	return d1 < d2 ? 1 : d1 > d2 ? -1 : 0;
}

static void RenderParticles()
{
	Shader* shader = particleShader;

	for (int i = 0; i < particleSystemDraws.size; i++)
	{
		if (particleSystemDraws[i].culled)
			break;
		if (particleSystemDraws[i].system->numParticles == 0)
			continue;

		ParticleSystem* system = particleSystemDraws[i].system;

		bgfx::InstanceDataBuffer instanceBuffer;
		if (Graphics_CreateInstanceBuffer(system->numParticles, sizeof(ParticleInstanceData), &instanceBuffer))
		{
			if (instanceBuffer.num != system->numParticles)
				__debugbreak();

			ParticleInstanceData* instanceData = (ParticleInstanceData*)instanceBuffer.data;
			memset(instanceBuffer.data, 0, instanceBuffer.size);

			int particleCount = 0;
			for (int i = 0; i < system->maxParticles; i++)
			{
				Particle* particle = &system->particles[i];
				if (particle->active)
				{
					Vector3 position = particle->position;
					if (system->follow)
						position = system->transform * (position + system->spawnOffset);

					ParticleInstanceData* particleData = &instanceData[particleCount++];
					particleData->position = position;
					particleData->rotation = particle->rotation;
					particleData->color = particle->color;
					particleData->sizeAnimation.x = particle->size;
					particleData->sizeAnimation.y = particle->animationFrame;
				}
			}

			//if (particleCount != system->numParticles)
			//	__debugbreak();

			qsort(instanceData, particleCount, sizeof(ParticleInstanceData), (_CoreCrtNonSecureSearchSortCompareFunction)ParticleComparator);

			if (system->textureAtlas != bgfx::kInvalidHandle)
				Graphics_SetTexture(shader->getUniform("s_textureAtlas", bgfx::UniformType::Sampler), 0, bgfx::TextureHandle{ system->textureAtlas }, system->linearFiltering ? 0 : BGFX_SAMPLER_POINT);
			Vector4 atlasSize((float)system->atlasSize.x, (float)system->atlasSize.y, system->textureAtlas != bgfx::kInvalidHandle ? 1.0f : 0.0f, 0);
			Graphics_SetUniform(shader->getUniform("u_atlasSize", bgfx::UniformType::Vec4), &atlasSize);

			int numLights = min(pointLightDraws.size, 16);

			Vector4 lightInfo((float)numLights, system->emissiveIntensity, system->lightInfluence, system->additive);
			Graphics_SetUniform(shader->getUniform("u_lightInfo", bgfx::UniformType::Vec4), &lightInfo);

			Vector4 lightPositions[16];
			Vector4 lightColors[16];
			for (int j = 0; j < numLights; j++)
			{
				lightPositions[j] = Vector4(pointLightDraws[j].position, 1);
				lightColors[j] = Vector4(pointLightDraws[j].color, 1);
			}
			Graphics_SetUniform(shader->getUniform("u_lightPosition", bgfx::UniformType::Vec4, 16), lightPositions, numLights);
			Graphics_SetUniform(shader->getUniform("u_lightColor", bgfx::UniformType::Vec4, 16), lightColors, numLights);

			Vector4 cameraAxisRight(cameraRight, 1);
			Vector4 cameraAxisUp(cameraUp, 1);
			Graphics_SetUniform(shader->getUniform("u_cameraAxisRight", bgfx::UniformType::Vec4), &cameraAxisRight);
			Graphics_SetUniform(shader->getUniform("u_cameraAxisUp", bgfx::UniformType::Vec4), &cameraAxisUp);

			Graphics_SetBlendState(system->additive ? BlendState::Additive : BlendState::Alpha);

			Graphics_SetVertexBuffer(particle);
			Graphics_SetInstanceBuffer(&instanceBuffer);

			Graphics_DrawIndirect(RenderPass::Forward, shader, particleIndirectBuffer.idx, i, 1);
		}
	}
}

static void RenderSky()
{
	if (skyTexture == bgfx::kInvalidHandle)
		return;

	Shader* shader = skyShader;

	Graphics_ResetState();

	Graphics_SetBlendState(BlendState::Alpha);

	Graphics_SetVertexBuffer(skydome);
	Graphics_SetIndexBuffer(skydomeIBO);

	Graphics_SetTransform(RenderPass::Forward, skyTransform);

	Vector4 skyData(skyIntensity, 0, 0, 0);
	Graphics_SetUniform(shader->getUniform("u_skyData", bgfx::UniformType::Vec4), &skyData);

	Graphics_SetTexture(shader->getUniform("s_skyTexture", bgfx::UniformType::Sampler), 0, bgfx::TextureHandle{ skyTexture });

	Graphics_Draw(RenderPass::Forward, shader);
}

static void ForwardPass()
{
	Graphics_ResetState();
	Graphics_SetRenderTarget(RenderPass::Forward, forwardRT, width, height);
	Graphics_SetViewTransform(RenderPass::Forward, projection, view);

	RenderForwardMeshes();
	RenderParticles();
	RenderSky();
}

static void BloomDownsample(int idx, bgfx::TextureHandle texture, RenderTarget target, int width, int height)
{
	Graphics_ResetState();
	Graphics_SetRenderTarget(RenderPass::BloomDownsample_ + idx, target, width, height);
	Graphics_ClearRenderTarget(RenderPass::BloomDownsample_ + idx, target, true, false, 0, 1);

	Graphics_SetDepthTest(DepthTest::None);
	Graphics_SetCullState(CullState::ClockWise);

	Graphics_SetTexture(bloomDownsampleShader->getUniform("s_input", bgfx::UniformType::Sampler), 0, texture);

	Graphics_SetVertexBuffer(quad);

	Graphics_Draw(RenderPass::BloomDownsample_ + idx, bloomDownsampleShader);
}

static void BloomUpsample(int idx, bgfx::TextureHandle texture0, bgfx::TextureHandle texture1, RenderTarget target, int width, int height)
{
	Graphics_ResetState();
	Graphics_SetRenderTarget(RenderPass::BloomUpsample_ + idx, target, width, height);
	Graphics_ClearRenderTarget(RenderPass::BloomUpsample_ + idx, target, true, false, 0, 1);

	Graphics_SetDepthTest(DepthTest::None);
	Graphics_SetCullState(CullState::ClockWise);

	Graphics_SetTexture(bloomUpsampleShader->getUniform("s_input0", bgfx::UniformType::Sampler), 0, texture0);
	Graphics_SetTexture(bloomUpsampleShader->getUniform("s_input1", bgfx::UniformType::Sampler), 1, texture1);

	Graphics_SetVertexBuffer(quad);

	Graphics_Draw(RenderPass::BloomUpsample_ + idx, bloomUpsampleShader);
}

static void BloomPass()
{
	if (!settings.bloomEnabled)
		return;

	bgfx::TextureHandle input = forwardRTTextures[0];

	BloomDownsample(0, input, bloomDownsampleRTs[0], bloomDownsampleRTTextureInfos[0].width, bloomDownsampleRTTextureInfos[0].height);
	BloomDownsample(1, bloomDownsampleRTTextures[0], bloomDownsampleRTs[1], bloomDownsampleRTTextureInfos[1].width, bloomDownsampleRTTextureInfos[1].height);
	BloomDownsample(2, bloomDownsampleRTTextures[1], bloomDownsampleRTs[2], bloomDownsampleRTTextureInfos[2].width, bloomDownsampleRTTextureInfos[2].height);
	BloomDownsample(3, bloomDownsampleRTTextures[2], bloomDownsampleRTs[3], bloomDownsampleRTTextureInfos[3].width, bloomDownsampleRTTextureInfos[3].height);
	BloomDownsample(4, bloomDownsampleRTTextures[3], bloomDownsampleRTs[4], bloomDownsampleRTTextureInfos[4].width, bloomDownsampleRTTextureInfos[4].height);
	BloomDownsample(5, bloomDownsampleRTTextures[4], bloomDownsampleRTs[5], bloomDownsampleRTTextureInfos[5].width, bloomDownsampleRTTextureInfos[5].height);

	BloomUpsample(0, bloomDownsampleRTTextures[5], bloomDownsampleRTTextures[4], bloomUpsampleRTs[4], bloomUpsampleRTTextureInfos[4].width, bloomUpsampleRTTextureInfos[4].height);
	BloomUpsample(1, bloomUpsampleRTTextures[4], bloomDownsampleRTTextures[3], bloomUpsampleRTs[3], bloomUpsampleRTTextureInfos[3].width, bloomUpsampleRTTextureInfos[3].height);
	BloomUpsample(2, bloomUpsampleRTTextures[3], bloomDownsampleRTTextures[2], bloomUpsampleRTs[2], bloomUpsampleRTTextureInfos[2].width, bloomUpsampleRTTextureInfos[2].height);
	BloomUpsample(3, bloomUpsampleRTTextures[2], bloomDownsampleRTTextures[1], bloomUpsampleRTs[1], bloomUpsampleRTTextureInfos[1].width, bloomUpsampleRTTextureInfos[1].height);
	BloomUpsample(4, bloomUpsampleRTTextures[1], bloomDownsampleRTTextures[0], bloomUpsampleRTs[0], bloomUpsampleRTTextureInfos[0].width, bloomUpsampleRTTextureInfos[0].height);
}

static void TonemappingPass(uint16_t target)
{
	Graphics_ResetState();
	Graphics_SetRenderTarget(RenderPass::Tonemapping, target, width, height);
	Graphics_ClearRenderTarget(RenderPass::Tonemapping, target, true, true, 0x0, 1);

	Graphics_SetDepthTest(DepthTest::None);
	Graphics_SetCullState(CullState::ClockWise);

	Shader* shader = tonemappingShader;

	Graphics_SetVertexBuffer(quad);
	Graphics_SetTexture(shader->getUniform("s_hdrBuffer", bgfx::UniformType::Sampler), 0, forwardRTTextures[0]);
	Graphics_SetTexture(shader->getUniform("s_depth", bgfx::UniformType::Sampler), 1, forwardRTTextures[1]);
	Graphics_SetTexture(shader->getUniform("s_bloom", bgfx::UniformType::Sampler), 2, bloomUpsampleRTTextures[0]);

	Vector4 params(settings.exposure, settings.bloomStrength, settings.bloomFalloff, 0);
	Graphics_SetUniform(u_params, &params);

	Vector4 fogData(settings.fogColor, settings.fogStrength);
	Graphics_SetUniform(shader->getUniform("u_fogData", bgfx::UniformType::Vec4), &fogData);

	Vector4 cameraFrustum(cameraNear, cameraFar, 0, 0);
	Graphics_SetUniform(shader->getUniform("u_cameraFrustum", bgfx::UniformType::Vec4), &cameraFrustum);

	Graphics_Draw(RenderPass::Tonemapping, shader);
}

static void DebugDrawPass(uint16_t target)
{
	Graphics_ResetState();
	Graphics_SetRenderTarget(RenderPass::Debug, target, width, height);

	Graphics_SetViewTransform(RenderPass::Debug, projection, view);

	debugLineRenderer.begin(debugLineDraws.size);
	for (int i = 0; i < debugLineDraws.size; i++)
	{
		LineDrawCommand cmd;
		cmd.vertex0 = debugLineDraws[i].position0;
		cmd.vertex1 = debugLineDraws[i].position1;
		cmd.color = ARGBToVector(debugLineDraws[i].color);
		debugLineRenderer.processDrawCommand(cmd);
	}
	debugLineRenderer.end(RenderPass::Debug, debugShader->program);
}

RFAPI uint16_t Renderer3D_End()
{
	FrustumCullObjects();
	DoDepthPrepass(); // render occluder objects
	BuildHZB();
	CullMeshes();
	CullLights();
	CullParticles();

	GeometryPass(); // render visible models here
	ShadowPass();
	AmbientOcclusionPass();
	DeferredPass(); // render visible lights here

	Graphics_Blit(RenderPass::Forward, forwardRTTextures[0], deferredRTTextures[0]);
	Graphics_Blit(RenderPass::Forward, forwardRTTextures[1], gbufferTextures[4]);

	ForwardPass(); // render visible particles here
	BloomPass();
	TonemappingPass(settings.showFrame ? bgfx::kInvalidHandle : tonemappingRT);

	Graphics_Blit(RenderPass::Tonemapping, tonemappingRTTextures[1], forwardRTTextures[1]);

	DebugDrawPass(settings.showFrame ? bgfx::kInvalidHandle : tonemappingRT);

	meshDraws.clear();
	occluderMeshes.clear();
	forwardDraws.clear();
	pointLightDraws.clear();
	clothDraws.clear();
	renderDirectionalLight = false;
	particleSystemDraws.clear();
	skyTexture = bgfx::kInvalidHandle;
	environmentMap = bgfx::kInvalidHandle;
	environmentMapMasks.clear();
	reflectionProbeDraws.clear();
	debugLineDraws.clear();

	return settings.showFrame ? bgfx::kInvalidHandle : tonemappingRTTextures[0].idx;
}

static float GetCPUTime(RenderPass pass)
{
	const bgfx::Stats* stats = bgfx::getStats();
	bgfx::ViewStats* viewStats = bgfx::getStats()->viewStats;
	for (int i = 0; i < stats->numViews; i++)
	{
		if (viewStats[i].view == pass)
			return (viewStats[i].cpuTimeEnd - viewStats[i].cpuTimeBegin) / (float)stats->cpuTimerFreq;
	}
	return 0;
}

static float GetGPUTime(RenderPass pass)
{
	const bgfx::Stats* stats = bgfx::getStats();
	bgfx::ViewStats* viewStats = bgfx::getStats()->viewStats;
	for (int i = 0; i < stats->numViews; i++)
	{
		if (viewStats[i].view == pass)
			return (viewStats[i].gpuTimeEnd - viewStats[i].gpuTimeBegin) / (float)stats->gpuTimerFreq;
	}
	return 0;
}

float GetCumulativeGPUTime(RenderPass pass, int count)
{
	float result = 0.0f;
	for (uint16_t v = pass; v < pass + count; v++)
		result += GetGPUTime((RenderPass)v);
	return result;
}

RFAPI int Renderer3D_DrawDebugStats(int x, int y, uint8_t color)
{
	char str[64] = "";

	sprintf(str, "Depth Prepass: %.2f ms", GetGPUTime(RenderPass::DepthPrepass) * 1000);
	Graphics_DrawDebugText(x, y++, color, str);

	sprintf(str, "HZB: %.2f ms", GetGPUTime(RenderPass::HZB) * 1000);
	Graphics_DrawDebugText(x, y++, color, str);

	sprintf(str, "Occlusion Culling: %.2f ms", GetGPUTime(RenderPass::LightCulling) * 1000);
	Graphics_DrawDebugText(x, y++, color, str);

	sprintf(str, "Geometry Pass: %.2f ms", GetGPUTime(RenderPass::Geometry) * 1000);
	Graphics_DrawDebugText(x, y++, color, str);

	sprintf(str, "AO Pass: %.2f ms", GetCumulativeGPUTime(RenderPass::AmbientOcclusion, 2) * 1000);
	Graphics_DrawDebugText(x, y++, color, str);

	sprintf(str, "Deferred Pass: %.2f ms", GetGPUTime(RenderPass::Deferred) * 1000);
	Graphics_DrawDebugText(x, y++, color, str);

	sprintf(str, "Forward Pass: %.2f ms", GetGPUTime(RenderPass::Forward) * 1000);
	Graphics_DrawDebugText(x, y++, color, str);

	sprintf(str, "Bloom Pass: %.2f ms", GetCumulativeGPUTime(RenderPass::BloomDownsample_, RenderPass::Composite - RenderPass::BloomDownsample_) * 1000);
	Graphics_DrawDebugText(x, y++, color, str);

	sprintf(str, "Tonemapping Pass: %.2f ms", GetGPUTime(RenderPass::Tonemapping) * 1000);
	Graphics_DrawDebugText(x, y++, color, str);

	sprintf(str, "UI Pass: %.2f ms", GetGPUTime(RenderPass::AmbientOcclusion) * 1000);
	Graphics_DrawDebugText(x, y++, color, str);

	return y;
}
