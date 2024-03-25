#include "ModelReader.h"

#include "Geometry.h"
#include "Application.h"
#include "Resource.h"
#include "Console.h"

#include <bx/file.h>
#include <bx/readerwriter.h>

#include <string.h>


using namespace bx;


static void ReadAABB(FileReaderI* reader, AABB& aabb, Error* err)
{
	read(reader, aabb.x0, err);
	read(reader, aabb.y0, err);
	read(reader, aabb.z0, err);
	read(reader, aabb.x1, err);
	read(reader, aabb.y1, err);
	read(reader, aabb.z1, err);
}

static void ReadSphere(FileReaderI* reader, Sphere& sphere, Error* err)
{
	read(reader, sphere.xcenter, err);
	read(reader, sphere.ycenter, err);
	read(reader, sphere.zcenter, err);
	read(reader, sphere.radius, err);
}

static void ReadMesh(FileReaderI* reader, MeshData& mesh, Error* err)
{
	int hasPositions;
	int hasNormals;
	int hasTangents;
	int hasTexCoords;
	int hasVertexColors;
	int hasBones;

	read(reader, hasPositions, err);
	read(reader, hasNormals, err);
	read(reader, hasTangents, err);
	read(reader, hasTexCoords, err);
	read(reader, hasVertexColors, err);
	read(reader, hasBones, err);

	read(reader, mesh.vertexCount, err);

	if (hasPositions || hasNormals || hasTangents)
	{
		mesh.positionsNormalsTangents = (PositionNormalTangent*)BX_ALLOC(Application_GetAllocator(), sizeof(PositionNormalTangent) * mesh.vertexCount);
		memset(mesh.positionsNormalsTangents, 0, mesh.vertexCount * sizeof(PositionNormalTangent));

		if (hasPositions)
		{
			for (int i = 0; i < mesh.vertexCount; i++)
			{
				read(reader, mesh.positionsNormalsTangents[i].position, err);
			}
		}
		if (hasNormals)
		{
			for (int i = 0; i < mesh.vertexCount; i++)
			{
				read(reader, mesh.positionsNormalsTangents[i].normal, err);
			}
		}
		if (hasTangents)
		{
			for (int i = 0; i < mesh.vertexCount; i++)
			{
				read(reader, mesh.positionsNormalsTangents[i].tangent, err);
			}
		}
	}
	if (hasTexCoords)
	{
		mesh.texcoords = (Vector2*)BX_ALLOC(Application_GetAllocator(), sizeof(Vector2) * mesh.vertexCount);
		read(reader, mesh.texcoords, mesh.vertexCount * sizeof(Vector2), err);
	}
	if (hasVertexColors)
	{
		mesh.vertexColors = (uint32_t*)BX_ALLOC(Application_GetAllocator(), sizeof(uint32_t) * mesh.vertexCount);
		read(reader, mesh.vertexColors, mesh.vertexCount * sizeof(uint32_t), err);
	}
	if (hasBones)
	{
		mesh.boneWeights = (BoneWeights*)BX_ALLOC(Application_GetAllocator(), sizeof(BoneWeights) * mesh.vertexCount);
		read(reader, mesh.boneWeights, mesh.vertexCount * sizeof(BoneWeights), err);
	}

	read(reader, mesh.indexCount, err);
	mesh.indexData = (int*)BX_ALLOC(Application_GetAllocator(), sizeof(int) * mesh.indexCount);
	read(reader, mesh.indexData, mesh.indexCount * sizeof(int), err);

	read(reader, mesh.materialID, err);
	read(reader, mesh.skeletonID, err);

	ReadAABB(reader, mesh.boundingBox, err);
	ReadSphere(reader, mesh.boundingSphere, err);


	mesh.vertexNormalTangentBuffer = BGFX_INVALID_HANDLE;
	mesh.texcoordBuffer = BGFX_INVALID_HANDLE;
	mesh.vertexColorBuffer = BGFX_INVALID_HANDLE;
	mesh.boneWeightBuffer = BGFX_INVALID_HANDLE;
	mesh.indexBuffer = BGFX_INVALID_HANDLE;
}

static void ReadMeshes(FileReaderI* reader, SceneData& scene, Error* err)
{
	for (int i = 0; i < scene.numMeshes; i++)
	{
		ReadMesh(reader, scene.meshes[i], err);
	}
}

static void ReadTexture(FileReaderI* reader, TextureData& texture, Error* err)
{
	int isEmbedded;

	read(reader, texture.path, sizeof(texture.path), err);
	read(reader, isEmbedded, err);

	texture.isEmbedded = isEmbedded;

	if (texture.isEmbedded)
	{
		read(reader, texture.width, err);
		read(reader, texture.height, err);
		if (texture.height == 0)
		{
			texture.data = (uint32_t*)malloc(texture.width);
			read(reader, texture.data, texture.width, err);
		}
		else
		{
			texture.data = (uint32_t*)malloc(texture.width * texture.height * sizeof(uint32_t));
			read(reader, texture.data, texture.width * texture.height * sizeof(uint32_t), err);
		}
	}

	texture.handle = BGFX_INVALID_HANDLE;
}

static void ReadMaterial(FileReaderI* reader, MaterialData& material, Error* err)
{
	read(reader, material.color, err);
	read(reader, material.metallicFactor, err);
	read(reader, material.roughnessFactor, err);
	read(reader, material.emissiveColor, err);
	read(reader, material.emissiveStrength, err);

	int hasDiffuse;
	int hasNormal;
	int hasRoughness;
	int hasMetallic;
	int hasEmissive;

	read(reader, hasDiffuse, err);
	read(reader, hasNormal, err);
	read(reader, hasRoughness, err);
	read(reader, hasMetallic, err);
	read(reader, hasEmissive, err);

	material.diffuse = nullptr;
	material.normal = nullptr;
	material.roughness = nullptr;
	material.metallic = nullptr;
	material.emissive = nullptr;

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
}

static void ReadMaterials(FileReaderI* reader, SceneData& scene, Error* err)
{
	for (int i = 0; i < scene.numMaterials; i++)
	{
		ReadMaterial(reader, scene.materials[i], err);
	}
}

static void ReadSkeleton(FileReaderI* reader, SkeletonData& skeleton, Error* err)
{
	read(reader, skeleton.boneCount, err);
	skeleton.bones = (BoneData*)BX_ALLOC(Application_GetAllocator(), sizeof(BoneData) * skeleton.boneCount);
	for (int i = 0; i < skeleton.boneCount; i++)
	{
		BoneData& bone = skeleton.bones[i];
		read(reader, bone.name, sizeof(bone.name), err);
		read(reader, bone.offsetMatrix, err);
		read(reader, bone.nodeID, err);
	}

	skeleton.inverseBindPose = Matrix::Identity;
}

static void ReadSkeletons(FileReaderI* reader, SceneData& scene, Error* err)
{
	for (int i = 0; i < scene.numSkeletons; i++)
	{
		ReadSkeleton(reader, scene.skeletons[i], err);
	}
}

static void ReadAnimation(FileReaderI* reader, AnimationData& animation, Error* err)
{
	read(reader, animation.name, sizeof(animation.name), err);

	read(reader, animation.duration, err);

	read(reader, animation.numPositions, err);
	read(reader, animation.numRotations, err);
	read(reader, animation.numScales, err);
	read(reader, animation.numChannels, err);

	animation.positionKeyframes = (PositionKeyframe*)BX_ALLOC(Application_GetAllocator(), sizeof(PositionKeyframe) * animation.numPositions);
	animation.rotationKeyframes = (RotationKeyframe*)BX_ALLOC(Application_GetAllocator(), sizeof(RotationKeyframe) * animation.numRotations);
	animation.scaleKeyframes = (ScaleKeyframe*)BX_ALLOC(Application_GetAllocator(), sizeof(ScaleKeyframe) * animation.numScales);
	animation.channels = (AnimationChannel*)BX_ALLOC(Application_GetAllocator(), sizeof(AnimationChannel) * animation.numChannels);

	read(reader, animation.positionKeyframes, animation.numPositions * sizeof(PositionKeyframe), err);
	read(reader, animation.rotationKeyframes, animation.numRotations * sizeof(RotationKeyframe), err);
	read(reader, animation.scaleKeyframes, animation.numScales * sizeof(ScaleKeyframe), err);

	for (int i = 0; i < animation.numChannels; i++)
	{
		read(reader, animation.channels[i].nodeName, err);
		read(reader, animation.channels[i].positionsOffset, err);
		read(reader, animation.channels[i].positionsCount, err);
		read(reader, animation.channels[i].rotationsOffset, err);
		read(reader, animation.channels[i].rotationsCount, err);
		read(reader, animation.channels[i].scalesOffset, err);
		read(reader, animation.channels[i].scalesCount, err);
	}
}

static void ReadAnimations(FileReaderI* reader, SceneData& scene, Error* err)
{
	for (int i = 0; i < scene.numAnimations; i++)
	{
		ReadAnimation(reader, scene.animations[i], err);
	}
}

static void ReadNode(FileReaderI* reader, NodeData& node, Error* err)
{
	read(reader, node.id, err);
	read(reader, node.name, sizeof(node.name), err);
	read(reader, node.transform, err);

	read(reader, node.numChildren, err);
	read(reader, node.numMeshes, err);

	node.children = (int*)BX_ALLOC(Application_GetAllocator(), sizeof(int) * node.numChildren);
	node.meshes = (int*)BX_ALLOC(Application_GetAllocator(), sizeof(int) * node.numMeshes);

	read(reader, node.children, node.numChildren * sizeof(int), err);
	read(reader, node.meshes, node.numMeshes * sizeof(int), err);

	node.parent = nullptr;
}

static void ReadNodes(FileReaderI* reader, SceneData& scene, Error* err)
{
	for (int i = 0; i < scene.numNodes; i++)
	{
		ReadNode(reader, scene.nodes[i], err);
	}
}

static void ReadLight(FileReaderI* reader, LightData& light, Error* err)
{
	read(reader, light.name, sizeof(light.name), err);

	read(reader, light.type, err);
	read(reader, light.x, err);
	read(reader, light.y, err);
	read(reader, light.z, err);
	read(reader, light.xdir, err);
	read(reader, light.ydir, err);
	read(reader, light.zdir, err);
	read(reader, light.color, err);
}

static void ReadLights(FileReaderI* reader, SceneData& scene, Error* err)
{
	for (int i = 0; i < scene.numLights; i++)
	{
		ReadLight(reader, scene.lights[i], err);
	}
}

bool ReadSceneData(FileReaderI* reader, const char* path, SceneData& scene)
{
	Error err;

	if (open(reader, path))
	{
		read(reader, scene.numMeshes, &err);
		read(reader, scene.numMaterials, &err);
		read(reader, scene.numSkeletons, &err);
		read(reader, scene.numAnimations, &err);
		read(reader, scene.numNodes, &err);
		read(reader, scene.numLights, &err);

		if (scene.numMeshes > 0) scene.meshes = (MeshData*)BX_ALLOC(Application_GetAllocator(), sizeof(MeshData) * scene.numMeshes);
		if (scene.numMaterials > 0) scene.materials = (MaterialData*)BX_ALLOC(Application_GetAllocator(), sizeof(MaterialData) * scene.numMaterials);
		if (scene.numSkeletons > 0) scene.skeletons = (SkeletonData*)BX_ALLOC(Application_GetAllocator(), sizeof(SkeletonData) * scene.numSkeletons);
		if (scene.numAnimations > 0) scene.animations = (AnimationData*)BX_ALLOC(Application_GetAllocator(), sizeof(AnimationData) * scene.numAnimations);
		if (scene.numNodes > 0) scene.nodes = (NodeData*)BX_ALLOC(Application_GetAllocator(), sizeof(NodeData) * scene.numNodes);
		if (scene.numLights > 0) scene.lights = (LightData*)BX_ALLOC(Application_GetAllocator(), sizeof(LightData) * scene.numLights);

		ReadMeshes(reader, scene, &err);
		ReadMaterials(reader, scene, &err);
		ReadSkeletons(reader, scene, &err);
		ReadAnimations(reader, scene, &err);
		ReadNodes(reader, scene, &err);
		ReadLights(reader, scene, &err);

		read(reader, scene.boundingBox, &err);
		read(reader, scene.boundingSphere, &err);

		close(reader);

		return true;
	}

	return false;
}
