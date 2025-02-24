#include "ModelReader.h"

#include "Geometry.h"
#include "Application.h"
#include "Resource.h"
#include "Console.h"

#include <bx/file.h>
#include <bx/bx.h>
#include <bx/readerwriter.h>

#include <string.h>
#include <stdio.h>


static void ReadAABB(bx::ReaderI* reader, AABB& aabb, bx::Error* err)
{
	bx::read(reader, aabb.x0, err);
	bx::read(reader, aabb.y0, err);
	bx::read(reader, aabb.z0, err);
	bx::read(reader, aabb.x1, err);
	bx::read(reader, aabb.y1, err);
	bx::read(reader, aabb.z1, err);
}

static void ReadSphere(bx::ReaderI* reader, Sphere& sphere, bx::Error* err)
{
	bx::read(reader, sphere.xcenter, err);
	bx::read(reader, sphere.ycenter, err);
	bx::read(reader, sphere.zcenter, err);
	bx::read(reader, sphere.radius, err);
}

static void ReadMesh(bx::ReaderI* reader, MeshData& mesh, bx::Error* err)
{
	mesh = {};

	int hasPositions;
	int hasNormals;
	int hasTangents;
	int hasTexCoords;
	int hasVertexColors;
	int hasBones;

	bx::read(reader, hasPositions, err);
	bx::read(reader, hasNormals, err);
	bx::read(reader, hasTangents, err);
	bx::read(reader, hasTexCoords, err);
	bx::read(reader, hasVertexColors, err);
	bx::read(reader, hasBones, err);

	bx::read(reader, mesh.vertexCount, err);

	if (hasPositions || hasNormals || hasTangents)
	{
		mesh.positionsNormalsTangents = (PositionNormalTangent*)BX_ALLOC(Application_GetAllocator(), sizeof(PositionNormalTangent) * mesh.vertexCount);
		memset(mesh.positionsNormalsTangents, 0, mesh.vertexCount * sizeof(PositionNormalTangent));

		if (hasPositions)
		{
			for (int i = 0; i < mesh.vertexCount; i++)
			{
				bx::read(reader, mesh.positionsNormalsTangents[i].position, err);
			}
		}
		if (hasNormals)
		{
			for (int i = 0; i < mesh.vertexCount; i++)
			{
				bx::read(reader, mesh.positionsNormalsTangents[i].normal, err);
			}
		}
		if (hasTangents)
		{
			for (int i = 0; i < mesh.vertexCount; i++)
			{
				bx::read(reader, mesh.positionsNormalsTangents[i].tangent, err);
			}
		}
	}
	if (hasTexCoords)
	{
		mesh.texcoords = (Vector2*)BX_ALLOC(Application_GetAllocator(), sizeof(Vector2) * mesh.vertexCount);
		bx::read(reader, mesh.texcoords, mesh.vertexCount * sizeof(Vector2), err);
	}
	if (hasVertexColors)
	{
		mesh.vertexColors = (uint32_t*)BX_ALLOC(Application_GetAllocator(), sizeof(uint32_t) * mesh.vertexCount);
		bx::read(reader, mesh.vertexColors, mesh.vertexCount * sizeof(uint32_t), err);
	}
	if (hasBones)
	{
		mesh.boneWeights = (BoneWeights*)BX_ALLOC(Application_GetAllocator(), sizeof(BoneWeights) * mesh.vertexCount);
		bx::read(reader, mesh.boneWeights, mesh.vertexCount * sizeof(BoneWeights), err);
	}

	bx::read(reader, mesh.indexCount, err);
	mesh.indexData = (int*)BX_ALLOC(Application_GetAllocator(), sizeof(int) * mesh.indexCount);
	bx::read(reader, mesh.indexData, mesh.indexCount * sizeof(int), err);

	bx::read(reader, mesh.materialID, err);
	bx::read(reader, mesh.skeletonID, err);

	ReadAABB(reader, mesh.boundingBox, err);
	ReadSphere(reader, mesh.boundingSphere, err);


	mesh.vertexNormalTangentBuffer = BGFX_INVALID_HANDLE;
	mesh.texcoordBuffer = BGFX_INVALID_HANDLE;
	mesh.vertexColorBuffer = BGFX_INVALID_HANDLE;
	mesh.boneWeightBuffer = BGFX_INVALID_HANDLE;
	mesh.indexBuffer = BGFX_INVALID_HANDLE;
}

static void ReadMeshes(bx::ReaderI* reader, SceneData& scene, bx::Error* err)
{
	for (int i = 0; i < scene.numMeshes; i++)
	{
		ReadMesh(reader, scene.meshes[i], err);
	}
}

static void ReadTexture(bx::ReaderI* reader, TextureData& texture, bx::Error* err)
{
	texture = {};

	int isEmbedded;

	bx::read(reader, texture.path, sizeof(texture.path), err);
	bx::read(reader, isEmbedded, err);

	texture.isEmbedded = isEmbedded;

	if (texture.isEmbedded)
	{
		bx::read(reader, texture.width, err);
		bx::read(reader, texture.height, err);
		if (texture.height == 0)
		{
			texture.data = (uint32_t*)malloc(texture.width);
			bx::read(reader, texture.data, texture.width, err);
		}
		else
		{
			texture.data = (uint32_t*)malloc(texture.width * texture.height * sizeof(uint32_t));
			bx::read(reader, texture.data, texture.width * texture.height * sizeof(uint32_t), err);
		}
	}

	texture.handle = BGFX_INVALID_HANDLE;
}

static void ReadMaterial(bx::ReaderI* reader, MaterialData& material, bx::Error* err)
{
	material = {};

	bx::read(reader, material.color, err);
	bx::read(reader, material.metallicFactor, err);
	bx::read(reader, material.roughnessFactor, err);
	bx::read(reader, material.emissiveColor, err);
	bx::read(reader, material.emissiveStrength, err);

	int hasDiffuse;
	int hasNormal;
	int hasRoughness;
	int hasMetallic;
	int hasEmissive;
	int hasHeight;

	bx::read(reader, hasDiffuse, err);
	bx::read(reader, hasNormal, err);
	bx::read(reader, hasRoughness, err);
	bx::read(reader, hasMetallic, err);
	bx::read(reader, hasEmissive, err);
	bx::read(reader, hasHeight, err);

	material.diffuse = nullptr;
	material.normal = nullptr;
	material.roughness = nullptr;
	material.metallic = nullptr;
	material.emissive = nullptr;
	material.height = nullptr;

	if (hasDiffuse)
	{
		material.diffuse = BX_NEW(Application_GetAllocator(), TextureData);
		ReadTexture(reader, *material.diffuse, err);
	}
	if (hasNormal)
	{
		material.normal = BX_NEW(Application_GetAllocator(), TextureData);
		ReadTexture(reader, *material.normal, err);
	}
	if (hasRoughness)
	{
		material.roughness = BX_NEW(Application_GetAllocator(), TextureData);
		ReadTexture(reader, *material.roughness, err);
	}
	if (hasMetallic)
	{
		material.metallic = BX_NEW(Application_GetAllocator(), TextureData);
		ReadTexture(reader, *material.metallic, err);
	}
	if (hasEmissive)
	{
		material.emissive = BX_NEW(Application_GetAllocator(), TextureData);
		ReadTexture(reader, *material.emissive, err);
	}
	if (hasHeight)
	{
		material.height = BX_NEW(Application_GetAllocator(), TextureData);
		ReadTexture(reader, *material.height, err);
	}
}

static void ReadMaterials(bx::ReaderI* reader, SceneData& scene, bx::Error* err)
{
	for (int i = 0; i < scene.numMaterials; i++)
	{
		ReadMaterial(reader, scene.materials[i], err);
	}
}

static void ReadSkeleton(bx::ReaderI* reader, SkeletonData& skeleton, bx::Error* err)
{
	bx::read(reader, skeleton.boneCount, err);
	skeleton.bones = (BoneData*)BX_ALLOC(Application_GetAllocator(), sizeof(BoneData) * skeleton.boneCount);
	for (int i = 0; i < skeleton.boneCount; i++)
	{
		BoneData& bone = skeleton.bones[i];
		bx::read(reader, bone.name, sizeof(bone.name), err);
		bx::read(reader, bone.offsetMatrix, err);
		bx::read(reader, bone.nodeID, err);
	}

	skeleton.inverseBindPose = Matrix::Identity;
}

static void ReadSkeletons(bx::ReaderI* reader, SceneData& scene, bx::Error* err)
{
	for (int i = 0; i < scene.numSkeletons; i++)
	{
		ReadSkeleton(reader, scene.skeletons[i], err);
	}
}

static void ReadAnimation(bx::ReaderI* reader, AnimationData& animation, bx::Error* err)
{
	bx::read(reader, animation.name, sizeof(animation.name), err);

	bx::read(reader, animation.duration, err);

	bx::read(reader, animation.numPositions, err);
	bx::read(reader, animation.numRotations, err);
	bx::read(reader, animation.numScales, err);
	bx::read(reader, animation.numChannels, err);

	animation.positionKeyframes = (PositionKeyframe*)BX_ALLOC(Application_GetAllocator(), sizeof(PositionKeyframe) * animation.numPositions);
	animation.rotationKeyframes = (RotationKeyframe*)BX_ALLOC(Application_GetAllocator(), sizeof(RotationKeyframe) * animation.numRotations);
	animation.scaleKeyframes = (ScaleKeyframe*)BX_ALLOC(Application_GetAllocator(), sizeof(ScaleKeyframe) * animation.numScales);
	animation.channels = (AnimationChannel*)BX_ALLOC(Application_GetAllocator(), sizeof(AnimationChannel) * animation.numChannels);

	bx::read(reader, animation.positionKeyframes, animation.numPositions * sizeof(PositionKeyframe), err);
	bx::read(reader, animation.rotationKeyframes, animation.numRotations * sizeof(RotationKeyframe), err);
	bx::read(reader, animation.scaleKeyframes, animation.numScales * sizeof(ScaleKeyframe), err);

	for (int i = 0; i < animation.numChannels; i++)
	{
		bx::read(reader, animation.channels[i].nodeName, err);
		bx::read(reader, animation.channels[i].positionsOffset, err);
		bx::read(reader, animation.channels[i].positionsCount, err);
		bx::read(reader, animation.channels[i].rotationsOffset, err);
		bx::read(reader, animation.channels[i].rotationsCount, err);
		bx::read(reader, animation.channels[i].scalesOffset, err);
		bx::read(reader, animation.channels[i].scalesCount, err);
	}
}

static void ReadAnimations(bx::ReaderI* reader, SceneData& scene, bx::Error* err)
{
	for (int i = 0; i < scene.numAnimations; i++)
	{
		ReadAnimation(reader, scene.animations[i], err);
	}
}

static void ReadNode(bx::ReaderI* reader, NodeData& node, bx::Error* err)
{
	bx::read(reader, node.id, err);
	bx::read(reader, node.name, sizeof(node.name), err);
	bx::read(reader, node.armatureID, err);
	bx::read(reader, node.transform, err);

	bx::read(reader, node.numChildren, err);
	bx::read(reader, node.numMeshes, err);

	node.children = (int*)BX_ALLOC(Application_GetAllocator(), sizeof(int) * node.numChildren);
	node.meshes = (int*)BX_ALLOC(Application_GetAllocator(), sizeof(int) * node.numMeshes);

	bx::read(reader, node.children, node.numChildren * sizeof(int), err);
	bx::read(reader, node.meshes, node.numMeshes * sizeof(int), err);

	node.parent = nullptr;
}

static void ReadNodes(bx::ReaderI* reader, SceneData& scene, bx::Error* err)
{
	for (int i = 0; i < scene.numNodes; i++)
	{
		ReadNode(reader, scene.nodes[i], err);
	}
}

static void ReadLight(bx::ReaderI* reader, LightData& light, bx::Error* err)
{
	bx::read(reader, light.name, sizeof(light.name), err);

	bx::read(reader, light.type, err);
	bx::read(reader, light.x, err);
	bx::read(reader, light.y, err);
	bx::read(reader, light.z, err);
	bx::read(reader, light.xdir, err);
	bx::read(reader, light.ydir, err);
	bx::read(reader, light.zdir, err);
	bx::read(reader, light.color, err);
}

static void ReadLights(bx::ReaderI* reader, SceneData& scene, bx::Error* err)
{
	for (int i = 0; i < scene.numLights; i++)
	{
		ReadLight(reader, scene.lights[i], err);
	}
}

void ReadSceneData(bx::ReaderI* reader, SceneData& scene)
{
	bx::Error err;

	bx::read(reader, scene.numMeshes, &err);
	bx::read(reader, scene.numMaterials, &err);
	bx::read(reader, scene.numSkeletons, &err);
	bx::read(reader, scene.numAnimations, &err);
	bx::read(reader, scene.numNodes, &err);
	bx::read(reader, scene.numLights, &err);

	scene.meshes = scene.numMeshes > 0 ? (MeshData*)BX_ALLOC(Application_GetAllocator(), sizeof(MeshData) * scene.numMeshes) : nullptr;
	scene.materials = scene.numMaterials > 0 ? (MaterialData*)BX_ALLOC(Application_GetAllocator(), sizeof(MaterialData) * scene.numMaterials) : nullptr;
	scene.skeletons = scene.numSkeletons > 0 ? (SkeletonData*)BX_ALLOC(Application_GetAllocator(), sizeof(SkeletonData) * scene.numSkeletons) : nullptr;
	scene.animations = scene.numAnimations > 0 ? (AnimationData*)BX_ALLOC(Application_GetAllocator(), sizeof(AnimationData) * scene.numAnimations) : nullptr;
	scene.nodes = scene.numNodes > 0 ? (NodeData*)BX_ALLOC(Application_GetAllocator(), sizeof(NodeData) * scene.numNodes) : nullptr;
	scene.lights = scene.numLights > 0 ? (LightData*)BX_ALLOC(Application_GetAllocator(), sizeof(LightData) * scene.numLights) : nullptr;

	ReadMeshes(reader, scene, &err);
	ReadMaterials(reader, scene, &err);
	ReadSkeletons(reader, scene, &err);
	ReadAnimations(reader, scene, &err);
	ReadNodes(reader, scene, &err);
	ReadLights(reader, scene, &err);

	bx::read(reader, scene.boundingBox, &err);
	bx::read(reader, scene.boundingSphere, &err);
}
