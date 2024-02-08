#include "Model.h"

#include "Application.h"
#include "Animation.h"
#include "Resource.h"
#include "Console.h"

#include "Hash.h"

#include <bx/allocator.h>

#include <bimg/bimg.h>
#include <bimg/decode.h>

#include <string.h>
#include <stdio.h>
#include <math.h>

#include <map>


#define MAX_BONES 128


std::map<uint32_t, bgfx::TextureHandle> loadedTextures;


Model::Model(SceneData* scene)
	: lod0(scene)
{
}

static void SubmitMesh(SceneData* scene, int id, bgfx::ViewId view, Shader* shader, const Matrix& transform)
{
	MeshData& mesh = scene->meshes[id];

	bgfx::setTransform(&transform.m00);

	if (mesh.vertexNormalTangentBuffer.idx != bgfx::kInvalidHandle)
		bgfx::setVertexBuffer(0, mesh.vertexNormalTangentBuffer);
	if (mesh.texcoordBuffer.idx != bgfx::kInvalidHandle)
		bgfx::setVertexBuffer(1, mesh.texcoordBuffer);
	if (mesh.vertexColorBuffer.idx != bgfx::kInvalidHandle)
		bgfx::setVertexBuffer(2, mesh.vertexColorBuffer);
	if (mesh.boneWeightBuffer.idx != bgfx::kInvalidHandle)
		bgfx::setVertexBuffer(3, mesh.boneWeightBuffer);

	bgfx::setIndexBuffer(mesh.indexBuffer);

	Vector4 attributeInfo;
	attributeInfo[0] = mesh.texcoordBuffer.idx != bgfx::kInvalidHandle ? 1.0f : 0.0f;
	attributeInfo[1] = 0.0f;
	attributeInfo[2] = 0.0f;
	attributeInfo[3] = 0.0f;

	bgfx::setUniform(shader->getUniform("u_attributeInfo", bgfx::UniformType::Vec4), &attributeInfo);


	MaterialData& material = scene->materials[mesh.materialID];

	if (material.diffuse)
		bgfx::setTexture(0, shader->getUniform("s_diffuse", bgfx::UniformType::Sampler), material.diffuse->handle, UINT32_MAX);
	if (material.normal)
		bgfx::setTexture(1, shader->getUniform("s_normal", bgfx::UniformType::Sampler), material.normal->handle, UINT32_MAX);
	if (material.roughness)
		bgfx::setTexture(2, shader->getUniform("s_roughness", bgfx::UniformType::Sampler), material.roughness->handle, UINT32_MAX);
	if (material.metallic)
		bgfx::setTexture(3, shader->getUniform("s_metallic", bgfx::UniformType::Sampler), material.metallic->handle, UINT32_MAX);
	if (material.emissive)
		bgfx::setTexture(4, shader->getUniform("s_emissive", bgfx::UniformType::Sampler), material.emissive->handle, UINT32_MAX);

	Vector4 materialInfo;
	materialInfo[0] = material.diffuse ? 1.0f : 0.0f;
	materialInfo[1] = material.normal ? 1.0f : 0.0f;
	materialInfo[2] = material.roughness ? 1.0f : 0.0f;
	materialInfo[3] = material.metallic ? 1.0f : 0.0f;

	Vector4 materialInfo2;
	materialInfo2[0] = ((material.color & 0x000000FF) >> 0) / 255.0f;
	materialInfo2[1] = ((material.color & 0x0000FF00) >> 8) / 255.0f;
	materialInfo2[2] = ((material.color & 0x00FF0000) >> 16) / 255.0f;
	materialInfo2[3] = material.emissive ? 1.0f : 0.0f;

	Vector4 materialInfo3;
	materialInfo3[0] = material.metallicFactor;
	materialInfo3[1] = material.roughnessFactor;
	materialInfo3[2] = 0.0f;
	materialInfo3[3] = 0.0f;

	Vector4 materialInfo4;
	materialInfo4[0] = material.emissiveColor.x;
	materialInfo4[1] = material.emissiveColor.y;
	materialInfo4[2] = material.emissiveColor.z;
	materialInfo4[3] = material.emissiveStrength;

	bgfx::setUniform(shader->getUniform("u_materialInfo", bgfx::UniformType::Vec4), &materialInfo);
	bgfx::setUniform(shader->getUniform("u_materialInfo2", bgfx::UniformType::Vec4), &materialInfo2);
	bgfx::setUniform(shader->getUniform("u_materialInfo3", bgfx::UniformType::Vec4), &materialInfo3);
	bgfx::setUniform(shader->getUniform("u_materialInfo4", bgfx::UniformType::Vec4), &materialInfo4);


	bgfx::submit(view, shader->program, 0, BGFX_DISCARD_ALL);
}

void Model::drawMesh(int id, bgfx::ViewId view, Shader* shader, const Matrix& transform)
{
	SubmitMesh(lod0, id, view, shader, transform);
}

void Model::drawMeshAnimated(int id, bgfx::ViewId view, Shader* shader, AnimationState* animationState, const Matrix& transform)
{
	SkeletonState* skeleton = animationState->skeletons[id];
	bgfx::setUniform(shader->getUniform("u_boneTransforms", bgfx::UniformType::Mat4, MAX_BONES), skeleton->boneTransforms, skeleton->numBones);

	SubmitMesh(lod0, id, view, shader, transform);
}

void Model::draw(bgfx::ViewId view, Shader* staticShader, Shader* animatedShader, AnimationState* animationState, const Matrix& transform)
{
	for (int i = 0; i < lod0->numMeshes; i++)
	{
		bool isAnimated = animationState && animationState->skeletons[i] && animatedShader;

		if (isAnimated)
			drawMeshAnimated(i, view, animatedShader, animationState, transform);
		else
			drawMesh(i, view, staticShader, transform);
	}
}

AnimationData* Model::getAnimation(const char* name) const
{
	for (int i = 0; i < lod0->numAnimations; i++)
	{
		if (strcmp(lod0->animations[i].name, name) == 0)
			return &lod0->animations[i];
	}
	return nullptr;
}

static void InitializeNode(NodeData& node, SceneData& scene)
{
	for (int i = 0; i < node.numMeshes; i++)
	{
		MeshData& mesh = scene.meshes[node.meshes[i]];
		mesh.node = &node;
	}
}

static void OnMemoryRelease(void* ptr, void* userData)
{
	delete[] ptr;
}

static Matrix GetGlobalTransform(NodeData& node)
{
	if (node.parent)
		return GetGlobalTransform(*node.parent) * node.transform;
	return node.transform;
}

static void InitializeMesh(MeshData& mesh, SceneData& scene)
{
	if (mesh.skeletonID != -1)
	{
		scene.skeletons[mesh.skeletonID].inverseBindPose = GetGlobalTransform(*mesh.node);
	}

	if (mesh.positionsNormalsTangents)
	{
		bgfx::VertexLayout layout;
		layout.begin()
			.add(bgfx::Attrib::Position, 3, bgfx::AttribType::Float)
			.add(bgfx::Attrib::Normal, 3, bgfx::AttribType::Float)
			.add(bgfx::Attrib::Tangent, 3, bgfx::AttribType::Float)
			.end();
		const bgfx::Memory* memory = bgfx::makeRef(mesh.positionsNormalsTangents, mesh.vertexCount * sizeof(PositionNormalTangent));
		mesh.vertexNormalTangentBuffer = bgfx::createVertexBuffer(memory, layout);
	}
	if (mesh.texcoords)
	{
		bgfx::VertexLayout layout;
		layout.begin().add(bgfx::Attrib::TexCoord0, 2, bgfx::AttribType::Float).end();
		const bgfx::Memory* memory = bgfx::makeRef(mesh.texcoords, mesh.vertexCount * sizeof(Vector2), OnMemoryRelease);
		mesh.texcoordBuffer = bgfx::createVertexBuffer(memory, layout);
	}
	if (mesh.vertexColors)
	{
		bgfx::VertexLayout layout;
		layout.begin().add(bgfx::Attrib::Color0, 4, bgfx::AttribType::Uint8, true).end();
		const bgfx::Memory* memory = bgfx::makeRef(mesh.vertexColors, mesh.vertexCount * sizeof(uint32_t), OnMemoryRelease);
		mesh.vertexColorBuffer = bgfx::createVertexBuffer(memory, layout);
	}
	if (mesh.boneWeights)
	{
		bgfx::VertexLayout layout;
		layout.begin().add(bgfx::Attrib::Weight, 4, bgfx::AttribType::Float).add(bgfx::Attrib::Indices, 4, bgfx::AttribType::Float).end();
		const bgfx::Memory* memory = bgfx::makeRef(mesh.boneWeights, mesh.vertexCount * sizeof(BoneWeights), OnMemoryRelease);
		mesh.boneWeightBuffer = bgfx::createVertexBuffer(memory, layout);
	}

	{
		const bgfx::Memory* memory = bgfx::makeRef(mesh.indexData, mesh.indexCount * sizeof(int), OnMemoryRelease);
		mesh.indexBuffer = bgfx::createIndexBuffer(memory, BGFX_BUFFER_INDEX32);
	}
}

static void InitializeTexture(TextureData& texture, const char* scenePath, uint64_t flags)
{
	if (texture.width * texture.height != 0)
		return;

	if (texture.path[0])
	{
		if (texture.isEmbedded)
		{
			if (texture.height == 0)
			{
				if (bimg::ImageContainer* image = bimg::imageParse(Application_GetAllocator(), texture.data, texture.width, bimg::TextureFormat::BGRA8))
				{
					texture.handle = bgfx::createTexture2D(image->m_width, image->m_height, false, 1, bgfx::TextureFormat::BGRA8, flags);
					bgfx::updateTexture2D(texture.handle, 0, 0, 0, 0, image->m_width, image->m_height, bgfx::makeRef(image->m_data, image->m_size));
				}
			}
			else
			{
				texture.handle = bgfx::createTexture2D(texture.width, texture.height, false, 1, bgfx::TextureFormat::BGRA8, flags);
				bgfx::updateTexture2D(texture.handle, 0, 0, 0, 0, texture.width, texture.height, bgfx::makeRef(texture.data, texture.width * texture.height * sizeof(uint32_t)));
			}
		}
		else
		{
			char compiledPath[256] = "";
			//sprintf(compiledPath, "%s/%s.bin", scenePath, texture.path);
			if (const char* folder = strrchr(scenePath, '/'))
			{
				int pathLength = (int)(folder - scenePath);
				sprintf(compiledPath, "%.*s/%s.bin", pathLength, scenePath, texture.path);
			}
			else if (const char* folder = strrchr(scenePath, '\\'))
			{
				int pathLength = (int)(folder - scenePath);
				sprintf(compiledPath, "%.*s\\%s.bin", pathLength, scenePath, texture.path);
			}
			else
			{
				sprintf(compiledPath, "%s.bin", texture.path);
			}

			uint32_t pathHash = hash(compiledPath);
			auto it = loadedTextures.find(pathHash);
			if (it != loadedTextures.end())
			{
				texture.handle = it->second;
			}
			else
			{
				if (const bgfx::Memory* memory = ReadFileBinary(Application_GetFileReader(), compiledPath))
				{
					texture.handle = bgfx::createTexture(memory, flags);
					loadedTextures.emplace(pathHash, texture.handle);
				}
				else
				{
					Console_Error("Failed to read model texture file '%s'", texture.path);
					texture.handle = BGFX_INVALID_HANDLE;
				}
			}
		}
	}
}

static void InitializeMaterial(MaterialData& material, const char* scenePath, uint64_t textureFlags)
{
	if (scenePath)
	{
		if (material.diffuse)
			InitializeTexture(*material.diffuse, scenePath, textureFlags);
		if (material.normal)
			InitializeTexture(*material.normal, scenePath, textureFlags);
		if (material.roughness)
			InitializeTexture(*material.roughness, scenePath, textureFlags);
		if (material.metallic)
			InitializeTexture(*material.metallic, scenePath, textureFlags);
		if (material.emissive)
			InitializeTexture(*material.emissive, scenePath, textureFlags);
	}
}

static void InitializeSkeleton(SkeletonData& skeleton, SceneData& scene)
{
	for (int i = 0; i < skeleton.boneCount; i++)
	{
		if (skeleton.bones[i].nodeID != -1)
			skeleton.bones[i].node = &scene.nodes[skeleton.bones[i].nodeID];
	}
}

static void InitializeAnimationChannel(AnimationChannel& channel, AnimationData& animation, SceneData& scene)
{
	channel.positions = &animation.positionKeyframes[channel.positionsOffset];
	channel.rotations = &animation.rotationKeyframes[channel.rotationsOffset];
	channel.scales = &animation.scaleKeyframes[channel.scalesOffset];

	for (int i = 0; i < scene.numNodes; i++)
	{
		if (strcmp(scene.nodes[i].name, channel.nodeName) == 0)
		{
			channel.node = &scene.nodes[i];
			break;
		}
	}
}

static void InitializeAnimation(AnimationData& animation, SceneData& scene)
{
	for (int i = 0; i < animation.numChannels; i++)
	{
		InitializeAnimationChannel(animation.channels[i], animation, scene);
	}
}

void InitializeScene(SceneData& scene, const char* scenePath, uint64_t textureFlags)
{
	for (int i = 0; i < scene.numNodes; i++)
	{
		InitializeNode(scene.nodes[i], scene);
	}
	for (int i = 0; i < scene.numMeshes; i++)
	{
		InitializeMesh(scene.meshes[i], scene);
	}
	for (int i = 0; i < scene.numMaterials; i++)
	{
		InitializeMaterial(scene.materials[i], scenePath, textureFlags);
	}
	for (int i = 0; i < scene.numSkeletons; i++)
	{
		InitializeSkeleton(scene.skeletons[i], scene);
	}
	for (int i = 0; i < scene.numAnimations; i++)
	{
		InitializeAnimation(scene.animations[i], scene);
	}
}

template<typename T>
T* CopyData(T* data, int length)
{
	T* mem = (T*)BX_ALLOC(Application_GetAllocator(), length * sizeof(T));
	memcpy(mem, data, length * sizeof(T));
	return mem;
}

static AABB CalculateBoundingBox(int numVertices, PositionNormalTangent* vertices)
{
	float x0 = INFINITY;
	float y0 = INFINITY;
	float z0 = INFINITY;
	float x1 = -INFINITY;
	float y1 = -INFINITY;
	float z1 = -INFINITY;

	for (int i = 0; i < numVertices; i++)
	{
		x0 = fminf(x0, vertices[i].position.x);
		y0 = fminf(y0, vertices[i].position.y);
		z0 = fminf(z0, vertices[i].position.z);
		x1 = fmaxf(x1, vertices[i].position.x);
		y1 = fmaxf(y1, vertices[i].position.y);
		z1 = fmaxf(z1, vertices[i].position.z);
	}

	AABB boundingBox;
	boundingBox.x0 = x0;
	boundingBox.y0 = y0;
	boundingBox.z0 = z0;
	boundingBox.x1 = x1;
	boundingBox.y1 = y1;
	boundingBox.z1 = z1;

	return boundingBox;
}

static Sphere CalculateBoundingSphere(int numVertices, PositionNormalTangent* vertices, const AABB& boundingBox)
{
	float xcenter = 0.5f * (boundingBox.x0 + boundingBox.x1);
	float ycenter = 0.5f * (boundingBox.y0 + boundingBox.y1);
	float zcenter = 0.5f * (boundingBox.z0 + boundingBox.z1);

	float radiusSq = 0.0f;
	for (int i = 0; i < numVertices; i++)
	{
		float dx = vertices[i].position.x - xcenter;
		float dy = vertices[i].position.y - ycenter;
		float dz = vertices[i].position.z - zcenter;
		radiusSq = fmaxf(dx * dx + dy * dy + dz * dz, radiusSq);
	}

	float radius = sqrtf(radiusSq);

	Sphere boundingSphere;
	boundingSphere.xcenter = xcenter;
	boundingSphere.ycenter = ycenter;
	boundingSphere.zcenter = zcenter;
	boundingSphere.radius = radius;

	return boundingSphere;
}

RFAPI Model* Model_Create(int numVertices, PositionNormalTangent* vertices, Vector2* uvs, int numIndices, int* indices, MaterialData* material)
{
	vertices = CopyData(vertices, numVertices);
	uvs = CopyData(uvs, numVertices);
	indices = CopyData(indices, numIndices);

	SceneData* sceneData = BX_NEW(Application_GetAllocator(), SceneData)();

	MeshData* meshData = BX_NEW(Application_GetAllocator(), MeshData)();
	meshData->positionsNormalsTangents = vertices;
	meshData->texcoords = uvs;
	meshData->vertexColors = nullptr;
	meshData->boneWeights = nullptr;
	meshData->vertexCount = numVertices;
	meshData->indexData = indices;
	meshData->indexCount = numIndices;
	meshData->materialID = 0;
	meshData->skeletonID = -1;
	meshData->boundingBox = CalculateBoundingBox(numVertices, vertices);
	meshData->boundingSphere = CalculateBoundingSphere(numVertices, vertices, meshData->boundingBox);

	MaterialData* materialData = BX_NEW(Application_GetAllocator(), MaterialData)();
	*materialData = *material;
	/*materialData->color = 0xFFFFFFFF;
	materialData->metallicFactor = 0.0f;
	materialData->roughnessFactor = 1.0f;
	materialData->emissiveColor = Vector3(0.0f);
	materialData->emissiveStrength = 0.0f;
	materialData->diffuse = diffuse;
	materialData->normal = normal;
	materialData->roughness = roughness;
	materialData->metallic = metallic;
	materialData->emissive = emissive;*/

	sceneData->numMeshes = 1;
	sceneData->numMaterials = 1;
	sceneData->numSkeletons = 0;
	sceneData->numAnimations = 0;
	sceneData->numNodes = 0;
	sceneData->numLights = 0;

	sceneData->meshes = meshData;
	sceneData->materials = materialData;
	sceneData->skeletons = nullptr;
	sceneData->animations = nullptr;
	sceneData->nodes = nullptr;
	sceneData->lights = nullptr;

	InitializeScene(*sceneData, nullptr, 0); // textureFlags == 0 since textures will not be reinitialized here

	Model* model = BX_NEW(Application_GetAllocator(), Model)(sceneData);
	return model;
}

RFAPI void Model_Destroy(Model* model)
{
	SceneData* sceneData = model->lod0;

	if (sceneData->meshes)
		BX_FREE(Application_GetAllocator(), sceneData->meshes);
	if (sceneData->materials)
		BX_FREE(Application_GetAllocator(), sceneData->materials);
	if (sceneData->skeletons)
		BX_FREE(Application_GetAllocator(), sceneData->skeletons);
	if (sceneData->animations)
		BX_FREE(Application_GetAllocator(), sceneData->animations);
	if (sceneData->nodes)
		BX_FREE(Application_GetAllocator(), sceneData->nodes);
	if (sceneData->lights)
		BX_FREE(Application_GetAllocator(), sceneData->lights);

	BX_FREE(Application_GetAllocator(), sceneData);

	BX_FREE(Application_GetAllocator(), model);
}

RFAPI void Model_ConfigureLODs(Model* model, float maxDistance)
{
	model->maxDistance = maxDistance;
}

RFAPI float Model_GetMaxDistance(Model* model)
{
	return model->maxDistance;
}
