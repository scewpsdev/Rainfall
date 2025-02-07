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


static void SubmitMesh(bgfx::ViewId view, SceneData* scene, int id, Shader* shader, const Matrix& transform)
{
	__debugbreak(); // deprecated

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


	Vector4 materialInfo = Vector4(0.0f, 0.0f, 0.0f, 0.0f);
	Vector4 materialInfo2 = Vector4(1.0f, 1.0f, 1.0f, 0.0f);
	Vector4 materialInfo3 = Vector4(0.0f, 1.0f, 0.0f, 0.0f);
	Vector4 materialInfo4 = Vector4(0.0f, 0.0f, 0.0f, 0.0f);

	if (mesh.materialID != -1)
	{
		MaterialData& material = scene->materials[mesh.materialID];

		bool hasDiffuse = material.diffuse && material.diffuse->handle.idx != bgfx::kInvalidHandle;
		bool hasNormal = material.normal && material.normal->handle.idx != bgfx::kInvalidHandle;
		bool hasRoughness = material.roughness && material.roughness->handle.idx != bgfx::kInvalidHandle;
		bool hasMetallic = material.metallic && material.metallic->handle.idx != bgfx::kInvalidHandle;
		bool hasEmissive = material.emissive && material.emissive->handle.idx != bgfx::kInvalidHandle;

		materialInfo[0] = hasDiffuse ? 1.0f : 0.0f;
		materialInfo[1] = hasNormal ? 1.0f : 0.0f;
		materialInfo[2] = hasRoughness ? 1.0f : 0.0f;
		materialInfo[3] = hasMetallic ? 1.0f : 0.0f;

		materialInfo2[0] = ((material.color & 0x000000FF) >> 0) / 255.0f;
		materialInfo2[1] = ((material.color & 0x0000FF00) >> 8) / 255.0f;
		materialInfo2[2] = ((material.color & 0x00FF0000) >> 16) / 255.0f;
		materialInfo2[3] = hasEmissive ? 1.0f : 0.0f;

		materialInfo3[0] = material.metallicFactor;
		materialInfo3[1] = material.roughnessFactor;
		materialInfo3[2] = 0.0f;
		materialInfo3[3] = 0.0f;

		materialInfo4[0] = material.emissiveColor.x;
		materialInfo4[1] = material.emissiveColor.y;
		materialInfo4[2] = material.emissiveColor.z;
		materialInfo4[3] = material.emissiveStrength;

		if (hasDiffuse)
			bgfx::setTexture(0, shader->getUniform("s_diffuse", bgfx::UniformType::Sampler), material.diffuse->handle, UINT32_MAX);
		if (hasNormal)
			bgfx::setTexture(1, shader->getUniform("s_normal", bgfx::UniformType::Sampler), material.normal->handle, UINT32_MAX);
		if (hasRoughness)
			bgfx::setTexture(2, shader->getUniform("s_roughness", bgfx::UniformType::Sampler), material.roughness->handle, UINT32_MAX);
		if (hasMetallic)
			bgfx::setTexture(3, shader->getUniform("s_metallic", bgfx::UniformType::Sampler), material.metallic->handle, UINT32_MAX);
		if (hasEmissive)
			bgfx::setTexture(4, shader->getUniform("s_emissive", bgfx::UniformType::Sampler), material.emissive->handle, UINT32_MAX);
	}

	bgfx::setUniform(shader->getUniform("u_materialInfo", bgfx::UniformType::Vec4), &materialInfo);
	bgfx::setUniform(shader->getUniform("u_materialInfo2", bgfx::UniformType::Vec4), &materialInfo2);
	bgfx::setUniform(shader->getUniform("u_materialInfo3", bgfx::UniformType::Vec4), &materialInfo3);
	bgfx::setUniform(shader->getUniform("u_materialInfo4", bgfx::UniformType::Vec4), &materialInfo4);


	bgfx::submit(view, shader->program, 0, BGFX_DISCARD_ALL);
}

RFAPI void Model_DrawMesh(int pass, SceneData* scene, int meshID, Shader* shader, const Matrix& transform)
{
	SubmitMesh((bgfx::ViewId)pass, scene, meshID, shader, transform);
}

RFAPI void Model_DrawMeshAnimated(int pass, SceneData* scene, int meshID, Shader* shader, AnimationState* animationState, const Matrix& transform)
{
	//SkeletonState* skeleton = animationState->skeletons[meshID];
	//bgfx::setUniform(shader->getUniform("u_boneTransforms", bgfx::UniformType::Mat4, MAX_BONES), skeleton->boneTransforms, skeleton->numBones);

	SubmitMesh((bgfx::ViewId)pass, scene, meshID, shader, transform);
}

RFAPI void Model_Draw(int pass, SceneData* scene, Shader* shader, Shader* animatedShader, AnimationState* animationState, const Matrix& transform)
{
	for (int i = 0; i < scene->numMeshes; i++)
	{
		bool isAnimated = animationState && animationState->skeletons[i] && animatedShader;

		if (isAnimated)
			Model_DrawMeshAnimated(pass, scene, i, animatedShader, animationState, transform);
		else
			Model_DrawMesh(pass, scene, i, shader, transform);
	}
}


AnimationData* Model_GetAnimation(SceneData* scene, const char* name)
{
	for (int i = 0; i < scene->numAnimations; i++)
	{
		if (strcmp(scene->animations[i].name, name) == 0)
			return &scene->animations[i];
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
	for (int i = 0; i < node.numChildren; i++)
	{
		int childID = node.children[i];
		NodeData& child = scene.nodes[childID];
		child.parent = &node;
	}
}

static void OnMemoryRelease(void* ptr, void* userData)
{
	//BX_FREE(Application_GetAllocator(), ptr);
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
		static bgfx::VertexLayout layout;
		if (layout.m_hash == 0)
		{
			layout.begin()
				.add(bgfx::Attrib::Position, 3, bgfx::AttribType::Float)
				.add(bgfx::Attrib::Normal, 3, bgfx::AttribType::Float)
				.add(bgfx::Attrib::Tangent, 3, bgfx::AttribType::Float)
				.end();
		}

		const bgfx::Memory* memory = bgfx::makeRef(mesh.positionsNormalsTangents, mesh.vertexCount * sizeof(PositionNormalTangent), OnMemoryRelease);
		mesh.vertexNormalTangentBuffer = bgfx::createVertexBuffer(memory, layout);
	}
	if (mesh.texcoords)
	{
		static bgfx::VertexLayout layout;
		if (layout.m_hash == 0)
		{
			layout.begin().add(bgfx::Attrib::TexCoord0, 2, bgfx::AttribType::Float).end();
		}

		const bgfx::Memory* memory = bgfx::makeRef(mesh.texcoords, mesh.vertexCount * sizeof(Vector2), OnMemoryRelease);
		mesh.texcoordBuffer = bgfx::createVertexBuffer(memory, layout);
	}
	if (mesh.vertexColors)
	{
		static bgfx::VertexLayout layout;
		if (layout.m_hash == 0)
		{
			layout.begin().add(bgfx::Attrib::Color0, 4, bgfx::AttribType::Uint8, true).end();
		}

		const bgfx::Memory* memory = bgfx::makeRef(mesh.vertexColors, mesh.vertexCount * sizeof(uint32_t), OnMemoryRelease);
		mesh.vertexColorBuffer = bgfx::createVertexBuffer(memory, layout);
	}
	if (mesh.boneWeights)
	{
		static bgfx::VertexLayout layout;
		if (layout.m_hash == 0)
		{
			layout.begin().add(bgfx::Attrib::Weight, 4, bgfx::AttribType::Float).add(bgfx::Attrib::Indices, 4, bgfx::AttribType::Float).end();
		}

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
			char fullPath[256];
			//sprintf(compiledPath, "%s/%s.bin", scenePath, texture.path);
			if (const char* folder = strrchr(scenePath, '/'))
			{
				int pathLength = (int)(folder - scenePath);
				sprintf(fullPath, "%.*s/%s", pathLength, scenePath, texture.path);
			}
			else if (const char* folder = strrchr(scenePath, '\\'))
			{
				int pathLength = (int)(folder - scenePath);
				sprintf(fullPath, "%.*s/%s", pathLength, scenePath, texture.path);
			}
			else
			{
				sprintf(fullPath, "%s.bin", texture.path);
			}

			if (TextureResource* tex = Resource_GetTexture(fullPath, flags, false))
			{
				texture.handle = tex->handle;
			}
			else
			{
				Console_Error("Failed to read model texture file '%s'", fullPath);
				texture.handle = BGFX_INVALID_HANDLE;
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
		if (material.height)
			InitializeTexture(*material.height, scenePath, textureFlags);
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

RFAPI SceneData* Model_Create(int numVertices, PositionNormalTangent* vertices, Vector2* uvs, int numIndices, int* indices)
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
	meshData->vertexNormalTangentBuffer = BGFX_INVALID_HANDLE;
	meshData->texcoordBuffer = BGFX_INVALID_HANDLE;
	meshData->vertexColorBuffer = BGFX_INVALID_HANDLE;
	meshData->boneWeightBuffer = BGFX_INVALID_HANDLE;
	meshData->indexBuffer = BGFX_INVALID_HANDLE;

	sceneData->numMeshes = 1;
	sceneData->numMaterials = 0;
	sceneData->numSkeletons = 0;
	sceneData->numAnimations = 0;
	sceneData->numNodes = 0;
	sceneData->numLights = 0;

	sceneData->meshes = meshData;
	sceneData->materials = nullptr;
	sceneData->skeletons = nullptr;
	sceneData->animations = nullptr;
	sceneData->nodes = nullptr;
	sceneData->lights = nullptr;

	sceneData->boundingBox = meshData->boundingBox;
	sceneData->boundingSphere = meshData->boundingSphere;

	InitializeScene(*sceneData, nullptr, 0); // textureFlags == 0 since textures will not be reinitialized here

	return sceneData;
}

RFAPI void Model_Destroy(SceneData* scene)
{
	SceneData* sceneData = scene;

	if (sceneData->meshes)
	{
		for (int i = 0; i < sceneData->numMeshes; i++)
		{
			if (sceneData->meshes[i].positionsNormalsTangents)
				BX_FREE(Application_GetAllocator(), sceneData->meshes[i].positionsNormalsTangents);
			if (sceneData->meshes[i].texcoords)
				BX_FREE(Application_GetAllocator(), sceneData->meshes[i].texcoords);
			if (sceneData->meshes[i].vertexColors)
				BX_FREE(Application_GetAllocator(), sceneData->meshes[i].vertexColors);
			if (sceneData->meshes[i].boneWeights)
				BX_FREE(Application_GetAllocator(), sceneData->meshes[i].boneWeights);
			if (sceneData->meshes[i].indexData)
				BX_FREE(Application_GetAllocator(), sceneData->meshes[i].indexData);

			if (sceneData->meshes[i].vertexNormalTangentBuffer.idx != bgfx::kInvalidHandle)
				bgfx::destroy(sceneData->meshes[i].vertexNormalTangentBuffer);
			if (sceneData->meshes[i].texcoordBuffer.idx != bgfx::kInvalidHandle)
				bgfx::destroy(sceneData->meshes[i].texcoordBuffer);
			if (sceneData->meshes[i].vertexColorBuffer.idx != bgfx::kInvalidHandle)
				bgfx::destroy(sceneData->meshes[i].vertexColorBuffer);
			if (sceneData->meshes[i].boneWeightBuffer.idx != bgfx::kInvalidHandle)
				bgfx::destroy(sceneData->meshes[i].boneWeightBuffer);

			if (sceneData->meshes[i].indexBuffer.idx != bgfx::kInvalidHandle)
				bgfx::destroy(sceneData->meshes[i].indexBuffer);
		}
		BX_FREE(Application_GetAllocator(), sceneData->meshes);
	}
	if (sceneData->materials)
	{
		BX_FREE(Application_GetAllocator(), sceneData->materials);
	}
	if (sceneData->skeletons)
	{
		for (int i = 0; i < sceneData->numSkeletons; i++)
		{
			if (sceneData->skeletons[i].bones)
				BX_FREE(Application_GetAllocator(), sceneData->skeletons[i].bones);
		}

		BX_FREE(Application_GetAllocator(), sceneData->skeletons);
	}
	if (sceneData->animations)
	{
		for (int i = 0; i < sceneData->numAnimations; i++)
		{
			if (sceneData->animations[i].positionKeyframes)
				BX_FREE(Application_GetAllocator(), sceneData->animations[i].positionKeyframes);
			if (sceneData->animations[i].rotationKeyframes)
				BX_FREE(Application_GetAllocator(), sceneData->animations[i].rotationKeyframes);
			if (sceneData->animations[i].scaleKeyframes)
				BX_FREE(Application_GetAllocator(), sceneData->animations[i].scaleKeyframes);
			if (sceneData->animations[i].channels)
				BX_FREE(Application_GetAllocator(), sceneData->animations[i].channels);
		}

		BX_FREE(Application_GetAllocator(), sceneData->animations);
	}
	if (sceneData->nodes)
	{
		for (int i = 0; i < sceneData->numNodes; i++)
		{
			if (sceneData->nodes[i].children)
				BX_FREE(Application_GetAllocator(), sceneData->nodes[i].children);
			if (sceneData->nodes[i].meshes)
				BX_FREE(Application_GetAllocator(), sceneData->nodes[i].meshes);
		}

		BX_FREE(Application_GetAllocator(), sceneData->nodes);
	}
	if (sceneData->lights)
	{
		BX_FREE(Application_GetAllocator(), sceneData->lights);
	}

	BX_FREE(Application_GetAllocator(), sceneData);
}
