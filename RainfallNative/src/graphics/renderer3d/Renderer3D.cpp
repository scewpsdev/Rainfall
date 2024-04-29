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

struct ModelDraw
{
	SceneData* scene;
	Matrix transform;
	Material* material;
};

struct PointLightDraw
{
	Vector3 position;
	float radius;
	Vector3 color;
};


static Renderer3DSettings settings;

static int width, height;

static uint16_t gbuffer;
static bgfx::TextureHandle gbufferTextures[5];
static bgfx::TextureInfo gbufferTextureInfos[5];

static uint16_t forwardRT;
static bgfx::TextureHandle forwardRTTextures[2];
static bgfx::TextureInfo forwardRTTextureInfos[2];

static uint16_t compositeRT;
static bgfx::TextureHandle compositeRTTexture;
static bgfx::TextureInfo compositeRTTextureInfo;

Shader* defaultShader;
static Shader* deferredPointShader;
static Shader* compositeShader;
static Shader* tonemappingShader;

static const float quadVertices[] = { -3.0f, -1.0f, 1.0f, 1.0f, -1.0f, 1.0f, 1.0f, 3.0f, 1.0f };
static uint16_t quad;

static Vector3 cameraPosition;
static Quaternion cameraRotation;
static Matrix projection, view, pv;

static List<ModelDraw> modelDraws;
static List<PointLightDraw> pointLightDraws;


static void MemoryReleaseCallback(void* ptr, void* userData)
{

}

RFAPI void Renderer3D_Init(int width, int height)
{
	Renderer3D_Resize(width, height);

	defaultShader = Shader_Create("res/shaders/rainfall/default/default.vs", "res/shaders/rainfall/default/default.fs");
	deferredPointShader = Shader_Create("res/shaders/rainfall/deferred/deferred.vs", "res/shaders/rainfall/deferred/deferred_point.fs");
	compositeShader = Shader_Create("res/shaders/rainfall/composite/composite.vs", "res/shaders/rainfall/composite/composite.fs");
	tonemappingShader = Shader_Create("res/shaders/rainfall/tonemapping/tonemapping.vs", "res/shaders/rainfall/tonemapping/tonemapping.fs");

	VertexElement quadLayout(VertexAttribute::Position, VertexAttributeType::Vector3);
	const bgfx::Memory* quadMemory = Graphics_CreateVideoMemoryRef(sizeof(quadVertices), quadVertices, nullptr);
	quad = Graphics_CreateVertexBuffer(quadMemory, &quadLayout, 1, BufferFlags::None);

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
		RenderTargetAttachment(width, height, bgfx::TextureFormat::D16F, BGFX_TEXTURE_RT | BGFX_SAMPLER_U_CLAMP | BGFX_SAMPLER_V_CLAMP)
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
	Graphics_DestroyShader(compositeShader);
	Graphics_DestroyShader(tonemappingShader);
}

RFAPI void Renderer3D_SetCamera(Vector3 position, Quaternion rotation, Matrix proj)
{
	cameraPosition = position;
	cameraRotation = rotation;
	projection = proj;
	view = (Matrix::Translate(position) * Matrix::Rotate(rotation)).inverted();
	pv = projection * view;
}

RFAPI void Renderer3D_DrawModel(SceneData* scene, Matrix transform, Material* material)
{
	modelDraws.add({ scene, transform, material });
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

static bool FrustumCulling(const AABB& boundingBox, const Sphere& boundingSphere, Matrix transform, Matrix pv)
{
	return true;
}

static void DrawMesh(MeshData* mesh, Matrix transform, Material* material, bgfx::ViewId view)
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


	bgfx::submit(view, material->shader->program, 0, BGFX_DISCARD_ALL);
}

static void GeometryPass()
{
	Graphics_ResetState();
	Graphics_SetRenderTarget(RenderPass::Geometry, gbuffer, width, height, true, true, 0, 1);
	Graphics_SetViewTransform(RenderPass::Geometry, projection, view);

	for (int i = 0; i < modelDraws.size; i++)
	{
		SceneData* scene = modelDraws[i].scene;
		Matrix transform = modelDraws[i].transform;
		if (!FrustumCulling(scene->boundingBox, scene->boundingSphere, transform, pv))
			continue;

		for (int j = 0; j < scene->numMeshes; j++)
		{
			MeshData* mesh = &scene->meshes[j];
			Matrix meshTransform = transform;
			if (mesh->node)
				meshTransform = meshTransform * mesh->node->transform;

			if (!FrustumCulling(mesh->boundingBox, mesh->boundingSphere, meshTransform, pv))
				continue;

			DrawMesh(&scene->meshes[j], meshTransform, modelDraws[i].material, RenderPass::Geometry);
		}
	}
}

static void RenderPointLights()
{
	Shader* shader = deferredPointShader;

	for (int i = 0; i < pointLightDraws.size; i++)
	{
		Graphics_ResetState();

		Graphics_SetBlendState(BlendState::Additive);
		Graphics_SetDepthTest(DepthTest::None);
		Graphics_SetCullState(CullState::ClockWise);

		Graphics_SetVertexBuffer(quad);

		Graphics_SetTexture(shader->getUniform("s_gbuffer0", bgfx::UniformType::Sampler), 0, gbufferTextures[0]);
		Graphics_SetTexture(shader->getUniform("s_gbuffer1", bgfx::UniformType::Sampler), 1, gbufferTextures[1]);
		Graphics_SetTexture(shader->getUniform("s_gbuffer2", bgfx::UniformType::Sampler), 2, gbufferTextures[2]);
		Graphics_SetTexture(shader->getUniform("s_gbuffer3", bgfx::UniformType::Sampler), 3, gbufferTextures[3]);

		Vector4 lightPositions[MAX_LIGHTS_PER_PASS];
		Vector4 lightColors[MAX_LIGHTS_PER_PASS];

		int numRemainingLights = min(pointLightDraws.size - i, MAX_LIGHTS_PER_PASS);
		for (int j = 0; j < numRemainingLights; j++)
		{
			int lightID = i + j;
			lightPositions[j] = Vector4(pointLightDraws[lightID].position, pointLightDraws[lightID].radius);
			lightColors[j] = Vector4(pointLightDraws[lightID].color, 0.0f);
		}
		i += numRemainingLights;

		Graphics_SetUniform(shader->getUniform("u_lightPosition", bgfx::UniformType::Vec4, MAX_LIGHTS_PER_PASS), lightPositions, MAX_LIGHTS_PER_PASS);
		Graphics_SetUniform(shader->getUniform("u_lightColor", bgfx::UniformType::Vec4, MAX_LIGHTS_PER_PASS), lightColors, MAX_LIGHTS_PER_PASS);

		Vector4 u_cameraPosition(cameraPosition, (float)numRemainingLights);
		Graphics_SetUniform(shader->getUniform("u_cameraPosition", bgfx::UniformType::Vec4), &u_cameraPosition, MAX_LIGHTS_PER_PASS);

		Graphics_Draw(RenderPass::Deferred, shader);
	}
}

static bgfx::TextureHandle DeferredPass()
{
	Graphics_SetRenderTarget(RenderPass::Deferred, forwardRT, width, height, true, true, 0, 1);

	RenderPointLights();

	return forwardRTTextures[0];
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

	TonemappingPass(hdr);

	modelDraws.clear();
	pointLightDraws.clear();
}
