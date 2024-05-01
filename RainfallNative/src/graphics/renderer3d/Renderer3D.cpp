#include "Renderer3D.h"

#include "Rainfall.h"
#include "Resource.h"

#include "graphics/Graphics.h"
#include "graphics/Model.h"
#include "graphics/Material.h"

#include "vector/Math.h"

#include "utils/List.h"

#include <bgfx/bgfx.h>


#define MAX_REFLECTION_PROBES 4
#define MAX_POINT_SHADOWS 8
#define MAX_LIGHTS_PER_PASS 16
#define BLOOM_STEP_COUNT 6
#define MAX_BONES 128


enum RenderPass
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
	BloomUpsample = BloomDownsample + BLOOM_STEP_COUNT,
	Composite = BloomUpsample + BLOOM_STEP_COUNT - 1,
	Tonemapping,
	UI,

	Count
};

struct Renderer3DSettings
{

};

struct MeshDraw
{
	MeshData* mesh;
	Matrix transform;
	Material* material;
	AnimationState* animation = nullptr;
};

struct SceneDraw
{
	SceneData* scene;
	Matrix transform;
	AnimationState* animation = nullptr;
};

struct PointLightDraw
{
	Vector3 position;
	Vector3 color;
};

struct ParticleSystemDraw
{
	
};


static Renderer3DSettings settings;

static int width, height;

static uint16_t gbuffer = bgfx::kInvalidHandle;
static bgfx::TextureHandle gbufferTextures[5];
static bgfx::TextureInfo gbufferTextureInfos[5];

static uint16_t forwardRT = bgfx::kInvalidHandle;
static bgfx::TextureHandle forwardRTTextures[2];
static bgfx::TextureInfo forwardRTTextureInfos[2];

static uint16_t compositeRT = bgfx::kInvalidHandle;
static bgfx::TextureHandle compositeRTTexture;
static bgfx::TextureInfo compositeRTTextureInfo;

Shader* defaultShader;
Shader* defaultAnimatedShader;
static Shader* deferredPointShader;
static Shader* tonemappingShader;
static Shader* particleShader;

static const float quadVertices[] = { -3.0f, -1.0f, 1.0f, 1.0f, -1.0f, 1.0f, 1.0f, 3.0f, 1.0f };
static uint16_t quad;

static const float boxVertices[] = { -1, -1, -1, 1, -1, -1, -1, -1, 1, 1, -1, 1, -1, 1, -1, 1, 1, -1, -1, 1, 1, 1, 1, 1 };
static const int boxIndices[] = { 0, 1, 2, 3, 2, 1, 0, 4, 5, 0, 5, 1, 0, 2, 6, 0, 6, 4, 1, 5, 7, 1, 7, 3, 4, 6, 7, 4, 7, 5, 2, 3, 7, 2, 7, 6 };
static uint16_t box;

static Vector3 cameraPosition;
static Quaternion cameraRotation;
static Matrix projection, view, pv;
static Vector4 frustumPlanes[6];

static List<MeshDraw> meshDraws;
static List<SceneDraw> sceneDraws;
static List<PointLightDraw> pointLightDraws;


RFAPI void Renderer3D_Init(int width, int height)
{
	Renderer3D_Resize(width, height);

	defaultShader = Shader_Create("res/rainfall/shaders/default/default.vs.bin", "res/rainfall/shaders/default/default.fs.bin");
	defaultAnimatedShader = Shader_Create("res/rainfall/shaders/default/default_animated.vs.bin", "res/rainfall/shaders/default/default.fs.bin");
	deferredPointShader = Shader_Create("res/rainfall/shaders/deferred/deferred.vs.bin", "res/rainfall/shaders/deferred/deferred_point.fs.bin");
	tonemappingShader = Shader_Create("res/rainfall/shaders/tonemapping/tonemapping.vs.bin", "res/rainfall/shaders/tonemapping/tonemapping.fs.bin");
	particleShader = Shader_Create("res/rainfall/shaders/particle/particle.vs", "res/rainfall/shaders/particle/particle.fs");

	VertexElement quadLayout(VertexAttribute::Position, VertexAttributeType::Vector3);
	const bgfx::Memory* quadMemory = Graphics_CreateVideoMemoryRef(sizeof(quadVertices), quadVertices, nullptr);
	quad = Graphics_CreateVertexBuffer(quadMemory, &quadLayout, 1, BufferFlags::None);

	VertexElement boxLayout(VertexAttribute::Position, VertexAttributeType::Vector3);
	const bgfx::Memory* boxMemory = Graphics_CreateVideoMemoryRef(sizeof(boxVertices), boxVertices, nullptr);
	box = Graphics_CreateVertexBuffer(boxMemory, &boxLayout, 1, BufferFlags::None);
}

RFAPI void Renderer3D_Resize(int width, int height)
{
	::width = width;
	::height = height;

	if (gbuffer != bgfx::kInvalidHandle)
		bgfx::destroy(bgfx::FrameBufferHandle{ gbuffer });
	if (forwardRT != bgfx::kInvalidHandle)
		bgfx::destroy(bgfx::FrameBufferHandle{ forwardRT });
	if (compositeRT != bgfx::kInvalidHandle)
		bgfx::destroy(bgfx::FrameBufferHandle{ compositeRT });

	RenderTargetAttachment gbufferAttachments[5] =
	{
		RenderTargetAttachment(width, height, bgfx::TextureFormat::RGBA32F, BGFX_TEXTURE_RT | BGFX_SAMPLER_U_CLAMP | BGFX_SAMPLER_V_CLAMP),
		RenderTargetAttachment(width, height, bgfx::TextureFormat::RGBA16F, BGFX_TEXTURE_RT | BGFX_SAMPLER_U_CLAMP | BGFX_SAMPLER_V_CLAMP),
		RenderTargetAttachment(width, height, bgfx::TextureFormat::RGBA8, BGFX_TEXTURE_RT | BGFX_SAMPLER_U_CLAMP | BGFX_SAMPLER_V_CLAMP),
		RenderTargetAttachment(width, height, bgfx::TextureFormat::RGBA8, BGFX_TEXTURE_RT | BGFX_SAMPLER_U_CLAMP | BGFX_SAMPLER_V_CLAMP),
		RenderTargetAttachment(width, height, bgfx::TextureFormat::D16F, BGFX_TEXTURE_RT | BGFX_SAMPLER_U_CLAMP | BGFX_SAMPLER_V_CLAMP, true) // TODO do i need mipmaps?
	};
	gbuffer = Graphics_CreateRenderTarget(5, gbufferAttachments, gbufferTextures, gbufferTextureInfos);

	RenderTargetAttachment forwardRTAttachments[2] =
	{
		RenderTargetAttachment(width, height, bgfx::TextureFormat::RG11B10F, BGFX_TEXTURE_RT | BGFX_SAMPLER_U_CLAMP | BGFX_SAMPLER_V_CLAMP),
		RenderTargetAttachment(width, height, bgfx::TextureFormat::D16F, BGFX_TEXTURE_RT | BGFX_TEXTURE_BLIT_DST | BGFX_SAMPLER_U_CLAMP | BGFX_SAMPLER_V_CLAMP)
	};
	forwardRT = Graphics_CreateRenderTarget(2, forwardRTAttachments, forwardRTTextures, forwardRTTextureInfos);

	RenderTargetAttachment compositeRTAttachment(width, height, bgfx::TextureFormat::RG11B10F, BGFX_TEXTURE_RT | BGFX_SAMPLER_U_CLAMP | BGFX_SAMPLER_V_CLAMP);
	compositeRT = Graphics_CreateRenderTarget(1, &compositeRTAttachment, &compositeRTTexture, &compositeRTTextureInfo);
}

RFAPI void Renderer3D_Terminate()
{
	if (gbuffer != bgfx::kInvalidHandle)
		bgfx::destroy(bgfx::FrameBufferHandle{ gbuffer });
	if (forwardRT != bgfx::kInvalidHandle)
		bgfx::destroy(bgfx::FrameBufferHandle{ forwardRT });
	if (compositeRT != bgfx::kInvalidHandle)
		bgfx::destroy(bgfx::FrameBufferHandle{ compositeRT });

	Graphics_DestroyShader(defaultShader);
	Graphics_DestroyShader(deferredPointShader);
	Graphics_DestroyShader(tonemappingShader);
}

RFAPI void Renderer3D_SetCamera(Vector3 position, Quaternion rotation, Matrix proj)
{
	cameraPosition = position;
	cameraRotation = rotation;
	projection = proj;
	view = (Matrix::Translate(position) * Matrix::Rotate(rotation)).inverted();
	pv = projection * view;
	GetFrustumPlanes(pv, frustumPlanes);
}

RFAPI void Renderer3D_DrawMesh(MeshData* mesh, Matrix transform, Material* material, AnimationState* animation)
{
	meshDraws.add({ mesh, transform, material, animation });
}

RFAPI void Renderer3D_DrawScene(SceneData* scene, Matrix transform, AnimationState* animation)
{
	sceneDraws.add({ scene, transform, animation });
}

RFAPI void Renderer3D_DrawPointLight(Vector3 position, Vector3 color)
{
	pointLightDraws.add({ position, color });
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

static void DrawMesh(MeshData* mesh, Matrix transform, Material* material, SkeletonState* skeleton, bgfx::ViewId view)
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

	Vector4 attributeInfo0(hasDiffuse, hasNormal, hasRoughness, hasMetallic);
	Vector4 attributeInfo1(
		hasEmissive,
		0, 0,
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

	if (skeleton)
		bgfx::setUniform(material->shader->getUniform("u_boneTransforms", bgfx::UniformType::Mat4, MAX_BONES), skeleton->boneTransforms, skeleton->numBones);


	bgfx::submit(view, material->shader->program, 0, BGFX_DISCARD_ALL);
}

static void GeometryPass()
{
	Graphics_ResetState();
	Graphics_SetRenderTarget(RenderPass::Geometry, gbuffer, width, height, true, true, 0, 1);
	Graphics_SetViewTransform(RenderPass::Geometry, projection, view);

	for (int i = 0; i < meshDraws.size; i++)
	{
		MeshData* mesh = meshDraws[i].mesh;
		Material* material = meshDraws[i].material;
		AnimationState* animation = meshDraws[i].animation;

		Matrix transform = meshDraws[i].transform;
		if (mesh->node)
			transform = transform * mesh->node->transform;
		if (!FrustumCulling(mesh->boundingSphere, transform, frustumPlanes))
			continue;

		SkeletonState* skeleton = nullptr;
		if (animation && mesh->skeletonID != -1)
			skeleton = animation->skeletons[mesh->skeletonID];

		DrawMesh(mesh, transform, material, skeleton, RenderPass::Geometry);
	}

	for (int i = 0; i < sceneDraws.size; i++)
	{
		SceneData* scene = sceneDraws[i].scene;
		Matrix transform = sceneDraws[i].transform;
		if (!FrustumCulling(scene->boundingSphere, transform, frustumPlanes))
			continue;

		AnimationState* animation = sceneDraws[i].animation;

		for (int j = 0; j < scene->numMeshes; j++)
		{
			MeshData* mesh = &scene->meshes[j];
			Matrix meshTransform = transform;
			if (mesh->node)
				meshTransform = meshTransform * mesh->node->transform;

			if (!FrustumCulling(mesh->boundingSphere, meshTransform, frustumPlanes))
				continue;

			Material* material = Material_GetDefault();
			if (mesh->materialID != -1)
				material = Material_GetForData(&scene->materials[mesh->materialID]);

			SkeletonState* skeleton = nullptr;
			if (animation && mesh->skeletonID != -1)
				skeleton = animation->skeletons[mesh->skeletonID];

			DrawMesh(&scene->meshes[j], meshTransform, material, skeleton, RenderPass::Geometry);
		}
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
	float radius = sqrtf(maxComponent / brightnessCap - 1) / 2;
	return radius;
}

static void RenderPointLights()
{
	Shader* shader = deferredPointShader;

	Graphics_SetViewTransform(RenderPass::Deferred, projection, view);

	Vector4 lightPositions[MAX_LIGHTS_PER_PASS];
	Vector4 lightColors[MAX_LIGHTS_PER_PASS];

	int numDrawnLights = 0;
	for (int i = 0; i < pointLightDraws.size; i++)
	{
		Vector3 lightPosition = pointLightDraws[i].position;
		Vector3 lightColor = pointLightDraws[i].color;
		float radius = CalculateLightRadius(lightColor);
		Sphere boundingSphere(lightPosition, radius);

		bool flush = false;
		if (!FrustumCulling(boundingSphere, frustumPlanes))
		{
			if (i < pointLightDraws.size - 1)
				continue;
			else
				// if this is the last iteration, flush the rendering even if this one was culled
				flush = true;
		}

		int index = numDrawnLights % MAX_LIGHTS_PER_PASS;
		lightPositions[index] = Vector4(lightPosition, 0);
		lightColors[index] = Vector4(lightColor, 0);

		if (index == MAX_LIGHTS_PER_PASS - 1 || i == pointLightDraws.size - 1 || flush)
		{
			int count = flush ? index : index + 1;

			Graphics_ResetState();

			Graphics_SetBlendState(BlendState::Additive);
			Graphics_SetDepthTest(DepthTest::None);
			Graphics_SetCullState(CullState::ClockWise);

			Graphics_SetVertexBuffer(quad);

			Graphics_SetTexture(shader->getUniform("s_gbuffer0", bgfx::UniformType::Sampler), 0, gbufferTextures[0]);
			Graphics_SetTexture(shader->getUniform("s_gbuffer1", bgfx::UniformType::Sampler), 1, gbufferTextures[1]);
			Graphics_SetTexture(shader->getUniform("s_gbuffer2", bgfx::UniformType::Sampler), 2, gbufferTextures[2]);
			Graphics_SetTexture(shader->getUniform("s_gbuffer3", bgfx::UniformType::Sampler), 3, gbufferTextures[3]);

			Graphics_SetUniform(shader->getUniform("u_lightPosition", bgfx::UniformType::Vec4, MAX_LIGHTS_PER_PASS), lightPositions, count);
			Graphics_SetUniform(shader->getUniform("u_lightColor", bgfx::UniformType::Vec4, MAX_LIGHTS_PER_PASS), lightColors, count);

			Vector4 u_cameraPosition(cameraPosition, (float)count);
			Graphics_SetUniform(shader->getUniform("u_cameraPosition", bgfx::UniformType::Vec4), &u_cameraPosition);

			Graphics_Draw(RenderPass::Deferred, shader);
		}

		numDrawnLights++;
	}
}

static bgfx::TextureHandle DeferredPass()
{
	Graphics_SetRenderTarget(RenderPass::Deferred, forwardRT, width, height, true, true, 0, 1);

	RenderPointLights();

	return forwardRTTextures[0];
}

static void RenderParticles()
{

}

static void ForwardPass()
{
	Graphics_ResetState();
	Graphics_SetRenderTarget(RenderPass::Forward, forwardRT, width, height, true, true, 0, 1);
	
	RenderParticles();
}

static void TonemappingPass(bgfx::TextureHandle input)
{
	Graphics_ResetState();
	Graphics_SetRenderTarget(RenderPass::Tonemapping, BGFX_INVALID_HANDLE, width, height, true, true, 0, 1);
	Graphics_SetDepthTest(DepthTest::None);
	Graphics_SetCullState(CullState::ClockWise);

	Shader* shader = tonemappingShader;

	Graphics_SetVertexBuffer(quad);
	Graphics_SetTexture(shader->getUniform("s_hdrBuffer", bgfx::UniformType::Sampler), 0, input);

	Graphics_Draw(RenderPass::Tonemapping, shader);
}

RFAPI void Renderer3D_End()
{
	pointLightDraws.sort(PointLightComparator);

	GeometryPass();
	bgfx::TextureHandle hdr = DeferredPass();

	Graphics_Blit(RenderPass::Forward, forwardRTTextures[1], gbufferTextures[4]);

	ForwardPass();
	TonemappingPass(hdr);

	meshDraws.clear();
	sceneDraws.clear();
	pointLightDraws.clear();
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

	sprintf(str, "Geometry Pass: %.2f ms", GetGPUTime(RenderPass::Geometry) * 1000);
	Graphics_DrawDebugText(x, y++, color, str);

	sprintf(str, "AO Pass: %.2f ms", GetCumulativeGPUTime(RenderPass::AmbientOcclusion, 2) * 1000);
	Graphics_DrawDebugText(x, y++, color, str);

	sprintf(str, "Deferred Pass: %.2f ms", GetGPUTime(RenderPass::Deferred) * 1000);
	Graphics_DrawDebugText(x, y++, color, str);

	sprintf(str, "Forward Pass: %.2f ms", GetGPUTime(RenderPass::Forward) * 1000);
	Graphics_DrawDebugText(x, y++, color, str);

	sprintf(str, "Bloom Pass: %.2f ms", GetCumulativeGPUTime(RenderPass::BloomDownsample, RenderPass::Composite - RenderPass::BloomDownsample) * 1000);
	Graphics_DrawDebugText(x, y++, color, str);

	sprintf(str, "Tonemapping Pass: %.2f ms", GetGPUTime(RenderPass::Tonemapping) * 1000);
	Graphics_DrawDebugText(x, y++, color, str);

	sprintf(str, "UI Pass: %.2f ms", GetGPUTime(RenderPass::AmbientOcclusion) * 1000);
	Graphics_DrawDebugText(x, y++, color, str);

	return y;
}
