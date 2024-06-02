#include "GeometryCompiler.h"

#include "Geometry.h"
#include "ModelWriter.h"

#include <bx/allocator.h>
#include <bx/file.h>
#include <bx/math.h>

#include <assimp/Importer.hpp>
#include <assimp/scene.h>
#include <assimp/postprocess.h>
#include <assimp/material.h>
#include <assimp/GltfMaterial.h>

#include <unordered_map>


static void ConstructBoundingInfo(MeshData& mesh, aiMesh* aimesh)
{
	mesh.boundingBox.x0 = INFINITY;
	mesh.boundingBox.y0 = INFINITY;
	mesh.boundingBox.z0 = INFINITY;
	mesh.boundingBox.x1 = -INFINITY;
	mesh.boundingBox.y1 = -INFINITY;
	mesh.boundingBox.z1 = -INFINITY;

	for (int i = 0; i < (int)aimesh->mNumVertices; i++)
	{
		mesh.boundingBox.x0 = fminf(mesh.boundingBox.x0, aimesh->mVertices[i].x);
		mesh.boundingBox.y0 = fminf(mesh.boundingBox.y0, aimesh->mVertices[i].y);
		mesh.boundingBox.z0 = fminf(mesh.boundingBox.z0, aimesh->mVertices[i].z);
		mesh.boundingBox.x1 = fmaxf(mesh.boundingBox.x1, aimesh->mVertices[i].x);
		mesh.boundingBox.y1 = fmaxf(mesh.boundingBox.y1, aimesh->mVertices[i].y);
		mesh.boundingBox.z1 = fmaxf(mesh.boundingBox.z1, aimesh->mVertices[i].z);
	}

	mesh.boundingSphere.xcenter = 0.5f * (mesh.boundingBox.x0 + mesh.boundingBox.x1);
	mesh.boundingSphere.ycenter = 0.5f * (mesh.boundingBox.y0 + mesh.boundingBox.y1);
	mesh.boundingSphere.zcenter = 0.5f * (mesh.boundingBox.z0 + mesh.boundingBox.z1);

	float radiusSq = 0.0f;
	for (int i = 0; i < (int)aimesh->mNumVertices; i++)
	{
		float dx = aimesh->mVertices[i].x - mesh.boundingSphere.xcenter;
		float dy = aimesh->mVertices[i].y - mesh.boundingSphere.ycenter;
		float dz = aimesh->mVertices[i].z - mesh.boundingSphere.zcenter;
		radiusSq = fmaxf(dx * dx + dy * dy + dz * dz, radiusSq);
	}

	mesh.boundingSphere.radius = sqrtf(radiusSq);
}

static aiNode* FindMeshNode(aiNode* ainode, int meshID)
{
	for (int i = 0; i < (int)ainode->mNumMeshes; i++)
	{
		if (ainode->mMeshes[i] == meshID)
			return ainode;
	}
	for (int i = 0; i < (int)ainode->mNumChildren; i++)
	{
		if (aiNode* child = FindMeshNode(ainode->mChildren[i], meshID))
			return child;
	}
	return nullptr;
}

static aiMatrix4x4 GetNodeTransform(aiNode* ainode)
{
	aiMatrix4x4 transform = ainode->mTransformation;
	aiNode* parent = ainode->mParent;
	while (parent)
	{
		transform = parent->mTransformation * transform;
		parent = parent->mParent;
	}
	return transform;
}

static aiMatrix4x4 GetMeshTransform(SceneData& scene, const aiScene* aiscene, int meshID)
{
	if (aiNode* meshNode = FindMeshNode(aiscene->mRootNode, meshID))
		return GetNodeTransform(meshNode);
	return aiMatrix4x4();
}

static void ConstructSceneBoundingInfo(SceneData& scene, const aiScene* aiscene)
{
	scene.boundingBox.x0 = INFINITY;
	scene.boundingBox.y0 = INFINITY;
	scene.boundingBox.z0 = INFINITY;
	scene.boundingBox.x1 = -INFINITY;
	scene.boundingBox.y1 = -INFINITY;
	scene.boundingBox.z1 = -INFINITY;

	for (int i = 0; i < scene.numMeshes; i++)
	{
		aiMatrix4x4 transform = GetMeshTransform(scene, aiscene, i);
		for (int j = 0; j < scene.meshes[i].vertexCount; j++)
		{
			Vector3 vertex = scene.meshes[i].positions[j];

			aiVector3D aiPos;
			aiQuaternion aiRot;
			(transform * aiMatrix4x4(aiVector3D(1), aiQuaternion(1, 0, 0, 0), aiVector3D(vertex.x, vertex.y, vertex.z))).DecomposeNoScaling(aiRot, aiPos);
			vertex = Vector3{ aiPos.x, aiPos.y, aiPos.z };

			scene.boundingBox.x0 = fminf(scene.boundingBox.x0, vertex.x);
			scene.boundingBox.y0 = fminf(scene.boundingBox.y0, vertex.y);
			scene.boundingBox.z0 = fminf(scene.boundingBox.z0, vertex.z);
			scene.boundingBox.x1 = fmaxf(scene.boundingBox.x1, vertex.x);
			scene.boundingBox.y1 = fmaxf(scene.boundingBox.y1, vertex.y);
			scene.boundingBox.z1 = fmaxf(scene.boundingBox.z1, vertex.z);
		}
	}

	scene.boundingSphere.xcenter = 0.5f * (scene.boundingBox.x0 + scene.boundingBox.x1);
	scene.boundingSphere.ycenter = 0.5f * (scene.boundingBox.y0 + scene.boundingBox.y1);
	scene.boundingSphere.zcenter = 0.5f * (scene.boundingBox.z0 + scene.boundingBox.z1);

	float radiusSq = 0.0f;
	for (int i = 0; i < scene.numMeshes; i++)
	{
		for (int j = 0; j < scene.meshes[i].vertexCount; j++)
		{
			float dx = scene.meshes[i].positions[j].x - scene.boundingSphere.xcenter;
			float dy = scene.meshes[i].positions[j].y - scene.boundingSphere.ycenter;
			float dz = scene.meshes[i].positions[j].z - scene.boundingSphere.zcenter;
			radiusSq = fmaxf(dx * dx + dy * dy + dz * dz, radiusSq);
		}
	}

	scene.boundingSphere.radius = sqrtf(radiusSq);
}

static void ProcessMesh(MeshData& mesh, aiMesh* aimesh, int skeletonID)
{
	mesh.positions = nullptr;
	mesh.normals = nullptr;
	mesh.tangents = nullptr;
	mesh.texcoords = nullptr;
	mesh.vertexColors = nullptr;
	mesh.boneWeights = nullptr;

	mesh.vertexCount = aimesh->mNumVertices;

	if (aimesh->HasPositions())
		mesh.positions = new Vector3[mesh.vertexCount];
	if (aimesh->HasNormals())
		mesh.normals = new Vector3[mesh.vertexCount];
	if (aimesh->HasTangentsAndBitangents())
		mesh.tangents = new Vector3[mesh.vertexCount];
	if (aimesh->HasTextureCoords(0))
		mesh.texcoords = new Vector2[mesh.vertexCount];
	if (aimesh->HasVertexColors(0))
		mesh.vertexColors = new uint32_t[mesh.vertexCount];
	if (aimesh->HasBones())
		mesh.boneWeights = new BoneWeights[mesh.vertexCount];

	mesh.indexCount = aimesh->mNumFaces * 3;
	mesh.indexData = new int[mesh.indexCount];


	struct BoneData
	{
		float weight;
		int boneID;
	};

	BoneData* boneData = new BoneData[mesh.vertexCount * 4];
	memset(boneData, 0, mesh.vertexCount * 4 * sizeof(BoneData));

	for (int i = 0; i < (int)aimesh->mNumBones; i++)
	{
		for (int j = 0; j < (int)aimesh->mBones[i]->mNumWeights; j++)
		{
			float weight = aimesh->mBones[i]->mWeights[j].mWeight;
			int vertexID = aimesh->mBones[i]->mWeights[j].mVertexId;

			for (int k = 0; k < 4; k++)
			{
				if (boneData[vertexID * 4 + k].weight == 0.0f)
				{
					boneData[vertexID * 4 + k] = { weight, i };
					break;
				}
			}
		}
	}


	if (mesh.positions)
	{
		for (int i = 0; i < mesh.vertexCount; i++)
		{
			mesh.positions[i].x = aimesh->mVertices[i].x;
			mesh.positions[i].y = aimesh->mVertices[i].y;
			mesh.positions[i].z = aimesh->mVertices[i].z;
		}
	}
	if (mesh.normals)
	{
		for (int i = 0; i < mesh.vertexCount; i++)
		{
			mesh.normals[i].x = aimesh->mNormals[i].x;
			mesh.normals[i].y = aimesh->mNormals[i].y;
			mesh.normals[i].z = aimesh->mNormals[i].z;
		}
	}
	if (mesh.tangents)
	{
		for (int i = 0; i < mesh.vertexCount; i++)
		{
			mesh.tangents[i].x = aimesh->mTangents[i].x;
			mesh.tangents[i].y = aimesh->mTangents[i].y;
			mesh.tangents[i].z = aimesh->mTangents[i].z;
		}
	}
	if (mesh.texcoords)
	{
		for (int i = 0; i < mesh.vertexCount; i++)
		{
			mesh.texcoords[i].x = aimesh->mTextureCoords[0][i].x;
			mesh.texcoords[i].y = aimesh->mTextureCoords[0][i].y;
		}
	}
	if (mesh.vertexColors)
	{
		for (int i = 0; i < mesh.vertexCount; i++)
		{
			((unsigned char*)mesh.vertexColors)[i * 4 + 0] = (unsigned char)(aimesh->mColors[0][i].r * 255 + 0.5f);
			((unsigned char*)mesh.vertexColors)[i * 4 + 1] = (unsigned char)(aimesh->mColors[0][i].g * 255 + 0.5f);
			((unsigned char*)mesh.vertexColors)[i * 4 + 2] = (unsigned char)(aimesh->mColors[0][i].b * 255 + 0.5f);
			((unsigned char*)mesh.vertexColors)[i * 4 + 3] = (unsigned char)(aimesh->mColors[0][i].a * 255 + 0.5f);
		}
	}
	if (mesh.boneWeights)
	{
		for (int i = 0; i < mesh.vertexCount; i++)
		{
			mesh.boneWeights[i].weights.x = boneData[i * 4 + 0].weight;
			mesh.boneWeights[i].weights.y = boneData[i * 4 + 1].weight;
			mesh.boneWeights[i].weights.z = boneData[i * 4 + 2].weight;
			mesh.boneWeights[i].weights.w = boneData[i * 4 + 3].weight;

			mesh.boneWeights[i].boneIDs.x = (float)boneData[i * 4 + 0].boneID;
			mesh.boneWeights[i].boneIDs.y = (float)boneData[i * 4 + 1].boneID;
			mesh.boneWeights[i].boneIDs.z = (float)boneData[i * 4 + 2].boneID;
			mesh.boneWeights[i].boneIDs.w = (float)boneData[i * 4 + 3].boneID;
		}
	}


	delete[] boneData;


	for (int i = 0; i < (int)aimesh->mNumFaces; i++)
	{
		mesh.indexData[i * 3 + 0] = aimesh->mFaces[i].mIndices[0];
		mesh.indexData[i * 3 + 1] = aimesh->mFaces[i].mIndices[1];
		mesh.indexData[i * 3 + 2] = aimesh->mFaces[i].mIndices[2];
	}


	mesh.materialID = aimesh->mMaterialIndex;
	mesh.skeletonID = skeletonID;


	ConstructBoundingInfo(mesh, aimesh);
}

static void CopyMatrixTransposed(float matrix[16], aiMatrix4x4& aimatrix)
{
	matrix[0] = aimatrix.a1;
	matrix[1] = aimatrix.b1;
	matrix[2] = aimatrix.c1;
	matrix[3] = aimatrix.d1;
	matrix[4] = aimatrix.a2;
	matrix[5] = aimatrix.b2;
	matrix[6] = aimatrix.c2;
	matrix[7] = aimatrix.d2;
	matrix[8] = aimatrix.a3;
	matrix[9] = aimatrix.b3;
	matrix[10] = aimatrix.c3;
	matrix[11] = aimatrix.d3;
	matrix[12] = aimatrix.a4;
	matrix[13] = aimatrix.b4;
	matrix[14] = aimatrix.c4;
	matrix[15] = aimatrix.d4;
}

static int FindNodeWithName(const char* name, SceneData& scene)
{
	for (int i = 0; i < scene.numNodes; i++)
	{
		if (strcmp(scene.nodes[i].name, name) == 0)
			return i;
	}
	printf("Could not find corresponding node for bone '%s'\n", name);
	return -1;
}

static void ProcessNode(NodeData& node, SceneData& scene, aiNode* ainode, int& idCounter, std::unordered_map<aiNode*, int>& nodeMap)
{
	node.id = idCounter;
	strcpy(node.name, ainode->mName.C_Str());
	//node.armatureID = -1;
	nodeMap.emplace(ainode, node.id);

	CopyMatrixTransposed(node.transform, ainode->mTransformation);

	node.numChildren = ainode->mNumChildren;
	node.numMeshes = ainode->mNumMeshes;

	node.children = new int[node.numChildren];
	node.meshes = new int[node.numMeshes];

	for (int i = 0; i < node.numChildren; i++)
	{
		int childID = ++idCounter;
		node.children[i] = childID;

		ProcessNode(scene.nodes[childID], scene, ainode->mChildren[i], idCounter, nodeMap);
	}

	for (int i = 0; i < node.numMeshes; i++)
	{
		node.meshes[i] = ainode->mMeshes[i];
	}
}

static std::unordered_map<aiNode*, int> nodeMap;

static void ProcessNodes(SceneData& scene, const aiScene* aiscene)
{
	nodeMap.clear();

	int idCounter = 0;
	ProcessNode(scene.nodes[0], scene, aiscene->mRootNode, idCounter, nodeMap);
}

static void ProcessSkeleton(SkeletonData& skeleton, aiMesh* aimesh, SceneData& scene)
{
	skeleton.boneCount = aimesh->mNumBones;
	skeleton.bones = new BoneData[skeleton.boneCount];
	//skeleton.boneTransforms = new float[skeleton.boneCount][16];
	//skeleton.boneNames = new char* [skeleton.boneCount];

	for (int i = 0; i < skeleton.boneCount; i++)
	{
		BoneData& bone = skeleton.bones[i];
		strcpy(bone.name, aimesh->mBones[i]->mName.C_Str());
		CopyMatrixTransposed(bone.offsetMatrix, aimesh->mBones[i]->mOffsetMatrix);
		bone.nodeID = nodeMap[aimesh->mBones[i]->mNode];
		//skeleton.boneNames[i] = _strdup(aimesh->mBones[i]->mName.C_Str());
	}
}

static void ProcessMeshesAndSkeletons(SceneData& scene, const aiScene* aiscene)
{
	int numSkeletons = 0;
	for (int i = 0; i < scene.numMeshes; i++)
	{
		aiMesh* aimesh = aiscene->mMeshes[i];

		int skeletonID = -1;
		if (aimesh->HasBones())
		{
			skeletonID = numSkeletons++;
			ProcessSkeleton(scene.skeletons[skeletonID], aimesh, scene);
		}

		ProcessMesh(scene.meshes[i], aimesh, skeletonID);
	}
}

static uint32_t AiColorToUInt(aiColor4D color)
{
	uint8_t r = (uint8_t)(color.r * 255 + 0.5f);
	uint8_t g = (uint8_t)(color.g * 255 + 0.5f);
	uint8_t b = (uint8_t)(color.b * 255 + 0.5f);
	uint8_t a = (uint8_t)(color.a * 255 + 0.5f);
	return (a << 24) | (b << 16) | (g << 8) | r;
}

static uint32_t AiColorToUInt(aiColor3D color)
{
	uint8_t r = (uint8_t)(color.r * 255 + 0.5f);
	uint8_t g = (uint8_t)(color.g * 255 + 0.5f);
	uint8_t b = (uint8_t)(color.b * 255 + 0.5f);
	uint8_t a = 255;
	return (a << 24) | (b << 16) | (g << 8) | r;
}

static void ProcessTexture(TextureData& texture, aiString texturePath, const aiScene* aiscene, const char* scenePath)
{
	if (const aiTexture* aitexture = aiscene->GetEmbeddedTexture(texturePath.C_Str()))
	{
		texture.isEmbedded = true;

		texture.width = aitexture->mWidth;
		texture.height = aitexture->mHeight;
		texture.data = (uint32_t*)aitexture->pcData;
		strcpy(texture.path, aitexture->mFilename.C_Str());
	}
	else
	{
		texture.isEmbedded = false;

		/*
		if (const char* folder = strrchr(scenePath, '/'))
		{
			int pathLength = (int)(folder - scenePath);
			sprintf(texture.path, "%.*s/%s", pathLength, scenePath, texturePath.C_Str());
		}
		else if (const char* folder = strrchr(scenePath, '\\'))
		{
			int pathLength = (int)(folder - scenePath);
			sprintf(texture.path, "%.*s\\%s", pathLength, scenePath, texturePath.C_Str());
		}
		else
		*/
		{
			strcpy(texture.path, texturePath.C_Str());
		}
	}
}

static void ProcessMaterial(MaterialData& material, aiMaterial* aimaterial, const aiScene* aiscene, const char* scenePath)
{
	material.color = 0xFFB4B4B4;
	material.diffuse = nullptr;
	material.roughness = nullptr;
	material.metallic = nullptr;
	material.normal = nullptr;
	material.emissive = nullptr;
	material.height = nullptr;


	aiColor4D color;
	if (aimaterial->Get(AI_MATKEY_COLOR_DIFFUSE, color) == aiReturn_SUCCESS)
		material.color = AiColorToUInt(color);
	ai_real metallicFactor;
	if (aimaterial->Get(AI_MATKEY_METALLIC_FACTOR, metallicFactor) == aiReturn_SUCCESS)
		material.metallicFactor = metallicFactor;
	ai_real roughnessFactor;
	if (aimaterial->Get(AI_MATKEY_ROUGHNESS_FACTOR, roughnessFactor) == aiReturn_SUCCESS)
		material.roughnessFactor = roughnessFactor;
	aiColor4D emissiveColor;
	if (aimaterial->Get(AI_MATKEY_COLOR_EMISSIVE, emissiveColor) == aiReturn_SUCCESS)
		material.emissiveColor = { emissiveColor.r, emissiveColor.g, emissiveColor.b };
	ai_real emissiveStrength;
	if (aimaterial->Get(AI_MATKEY_EMISSIVE_INTENSITY, emissiveStrength) == aiReturn_SUCCESS)
		material.emissiveStrength = emissiveStrength;


	aiString texturePath;

	if (aimaterial->GetTexture(AI_MATKEY_BASE_COLOR_TEXTURE, &texturePath) == aiReturn_SUCCESS)
	{
		material.diffuse = new TextureData();
		ProcessTexture(*material.diffuse, texturePath, aiscene, scenePath);
	}
	if (aimaterial->GetTexture(AI_MATKEY_GLTF_PBRMETALLICROUGHNESS_METALLICROUGHNESS_TEXTURE, &texturePath) == aiReturn_SUCCESS)
	{
		material.roughness = new TextureData();
		ProcessTexture(*material.roughness, texturePath, aiscene, scenePath);
		material.metallic = new TextureData();
		ProcessTexture(*material.metallic, texturePath, aiscene, scenePath);
	}
	else
	{
		if (aimaterial->GetTexture(AI_MATKEY_ROUGHNESS_TEXTURE, &texturePath) == aiReturn_SUCCESS)
		{
			material.roughness = new TextureData();
			ProcessTexture(*material.roughness, texturePath, aiscene, scenePath);
		}
		if (aimaterial->GetTexture(AI_MATKEY_METALLIC_TEXTURE, &texturePath) == aiReturn_SUCCESS)
		{
			material.metallic = new TextureData();
			ProcessTexture(*material.metallic, texturePath, aiscene, scenePath);
		}
	}
	if (aimaterial->GetTexture(aiTextureType_NORMALS, 0, &texturePath) == aiReturn_SUCCESS)
	{
		material.normal = new TextureData();
		ProcessTexture(*material.normal, texturePath, aiscene, scenePath);
	}
	if (aimaterial->GetTexture(aiTextureType_EMISSIVE, 0, &texturePath) == aiReturn_SUCCESS)
	{
		material.emissive = new TextureData();
		ProcessTexture(*material.emissive, texturePath, aiscene, scenePath);
	}
	if (aimaterial->GetTexture(aiTextureType_HEIGHT, 0, &texturePath) == aiReturn_SUCCESS)
	{
		material.height = new TextureData();
		ProcessTexture(*material.height, texturePath, aiscene, scenePath);
	}
}

static void ProcessMaterials(SceneData& scene, const aiScene* aiscene, const char* scenePath)
{
	for (int i = 0; i < scene.numMaterials; i++)
	{
		ProcessMaterial(scene.materials[i], aiscene->mMaterials[i], aiscene, scenePath);
	}
}

static void CountKeyframes(AnimationData& animation, aiAnimation* aianimation)
{
	animation.numPositions = 0;
	animation.numRotations = 0;
	animation.numScales = 0;

	for (int i = 0; i < (int)aianimation->mNumChannels; i++)
	{
		animation.numPositions += aianimation->mChannels[i]->mNumPositionKeys;
		animation.numRotations += aianimation->mChannels[i]->mNumRotationKeys;
		animation.numScales += aianimation->mChannels[i]->mNumScalingKeys;
	}
}

static void ProcessAnimationChannel(AnimationChannel& channel, AnimationData& animation, aiNodeAnim* aichannel, int& positionOffset, int& rotationOffset, int& scaleOffset)
{
	strcpy(channel.nodeName, aichannel->mNodeName.C_Str());

	channel.positionsCount = aichannel->mNumPositionKeys;
	channel.rotationsCount = aichannel->mNumRotationKeys;
	channel.scalesCount = aichannel->mNumScalingKeys;

	channel.positionsOffset = positionOffset;
	channel.rotationsOffset = rotationOffset;
	channel.scalesOffset = scaleOffset;

	positionOffset += channel.positionsCount;
	rotationOffset += channel.rotationsCount;
	scaleOffset += channel.scalesCount;

	for (int i = 0; i < channel.positionsCount; i++)
	{
		PositionKeyframe& keyframe = animation.positionKeyframes[channel.positionsOffset + i];
		keyframe.value.x = aichannel->mPositionKeys[i].mValue.x;
		keyframe.value.y = aichannel->mPositionKeys[i].mValue.y;
		keyframe.value.z = aichannel->mPositionKeys[i].mValue.z;
		keyframe.time = (float)(aichannel->mPositionKeys[i].mTime / 1000);
	}
	for (int i = 0; i < channel.rotationsCount; i++)
	{
		RotationKeyframe& keyframe = animation.rotationKeyframes[channel.rotationsOffset + i];
		keyframe.value.x = aichannel->mRotationKeys[i].mValue.x;
		keyframe.value.y = aichannel->mRotationKeys[i].mValue.y;
		keyframe.value.z = aichannel->mRotationKeys[i].mValue.z;
		keyframe.value.w = aichannel->mRotationKeys[i].mValue.w;
		keyframe.time = (float)(aichannel->mRotationKeys[i].mTime / 1000);
	}
	for (int i = 0; i < channel.scalesCount; i++)
	{
		ScaleKeyframe& keyframe = animation.scaleKeyframes[channel.scalesOffset + i];
		keyframe.value.x = aichannel->mScalingKeys[i].mValue.x;
		keyframe.value.y = aichannel->mScalingKeys[i].mValue.y;
		keyframe.value.z = aichannel->mScalingKeys[i].mValue.z;
		keyframe.time = (float)(aichannel->mScalingKeys[i].mTime / 1000);
	}
}

static void ProcessAnimation(AnimationData& animation, aiAnimation* aianimation)
{
	strcpy(animation.name, aianimation->mName.C_Str());

	animation.duration = (float)(aianimation->mDuration / aianimation->mTicksPerSecond);

	CountKeyframes(animation, aianimation);
	animation.numChannels = aianimation->mNumChannels;

	animation.positionKeyframes = new PositionKeyframe[animation.numPositions];
	animation.rotationKeyframes = new RotationKeyframe[animation.numRotations];
	animation.scaleKeyframes = new ScaleKeyframe[animation.numScales];
	animation.channels = new AnimationChannel[animation.numChannels];

	int positionOffset = 0;
	int rotationOffset = 0;
	int scaleOffset = 0;

	for (int i = 0; i < animation.numChannels; i++)
	{
		ProcessAnimationChannel(animation.channels[i], animation, aianimation->mChannels[i], positionOffset, rotationOffset, scaleOffset);
	}
}

static void ProcessAnimations(SceneData& scene, const aiScene* aiscene)
{
	for (int i = 0; i < scene.numAnimations; i++)
	{
		ProcessAnimation(scene.animations[i], aiscene->mAnimations[i]);
	}
}

static void ProcessLight(LightData& light, aiLight* ailight)
{
	strcpy(light.name, ailight->mName.C_Str());
	light.type = (LightType)ailight->mType;
	light.x = ailight->mPosition.x;
	light.y = ailight->mPosition.y;
	light.z = ailight->mPosition.z;
	light.xdir = ailight->mDirection.x;
	light.ydir = ailight->mDirection.y;
	light.zdir = ailight->mDirection.z;
	light.color = Vector3{ ailight->mColorDiffuse.r, ailight->mColorDiffuse.g, ailight->mColorDiffuse.b };
}

static void ProcessLights(SceneData& scene, const aiScene* aiscene)
{
	for (int i = 0; i < scene.numLights; i++)
	{
		ProcessLight(scene.lights[i], aiscene->mLights[i]);
	}
}


static int CountSkeletons(const aiScene* aiscene)
{
	int result = 0;
	for (int i = 0; i < (int)aiscene->mNumMeshes; i++)
	{
		if (aiscene->mMeshes[i]->HasBones())
			result++;
	}
	return result;
}

static int CountNodes(const aiNode* ainode)
{
	int result = 1;
	for (int i = 0; i < (int)ainode->mNumChildren; i++)
	{
		result += CountNodes(ainode->mChildren[i]);
	}
	return result;
}

bool CompileGeometry(const char* path, const char* out)
{
	unsigned int flags = 0
		| aiProcess_ValidateDataStructure
		| aiProcess_Triangulate
		| aiProcess_CalcTangentSpace
		| aiProcess_JoinIdenticalVertices
		//| aiProcess_SplitLargeMeshes
		| aiProcess_ImproveCacheLocality
		| aiProcess_OptimizeMeshes
		| aiProcess_FlipUVs
		| aiProcess_OptimizeGraph
		| aiProcess_PopulateArmatureData
		//| aiProcess_RemoveRedundantMaterials
		;

	Assimp::Importer importer;
	//importer.SetPropertyString(AI_CONFIG_PP_OG_EXCLUDE_LIST,
	//	"_tag_node_00 _tag_node_01 _tag_node_02 _tag_node_03 _tag_node_04 _tag_node_05 _tag_node_06 _tag_node_07 _tag_node_08 _tag_node_09 "
	//	"_tag_node_10 _tag_node_11 _tag_node_12 _tag_node_13 _tag_node_14 _tag_node_15 _tag_node_16 _tag_node_17 _tag_node_18 _tag_node_19 "
	//);

	if (const aiScene* aiscene = importer.ReadFile(path, flags))
	{
		SceneData scene;

		scene.numMeshes = (int)aiscene->mNumMeshes;
		scene.numMaterials = (int)aiscene->mNumMaterials;
		scene.numSkeletons = CountSkeletons(aiscene);
		scene.numAnimations = (int)aiscene->mNumAnimations;
		scene.numNodes = CountNodes(aiscene->mRootNode);
		scene.numLights = (int)aiscene->mNumLights;

		scene.meshes = new MeshData[scene.numMeshes];
		scene.materials = new MaterialData[scene.numMaterials];
		scene.skeletons = new SkeletonData[scene.numSkeletons];
		scene.animations = new AnimationData[scene.numAnimations];
		scene.nodes = new NodeData[scene.numNodes];
		scene.lights = new LightData[scene.numLights];

		ProcessNodes(scene, aiscene);
		ProcessMeshesAndSkeletons(scene, aiscene);
		ProcessMaterials(scene, aiscene, path);
		ProcessAnimations(scene, aiscene);
		ProcessLights(scene, aiscene);

		ConstructSceneBoundingInfo(scene, aiscene);

		bx::FileWriter* writer = new bx::FileWriter();
		WriteSceneData(writer, scene, out);
		delete writer;

		delete[] scene.meshes;
		delete[] scene.materials;
		delete[] scene.skeletons;
		delete[] scene.animations;
		delete[] scene.nodes;
		delete[] scene.lights;

		return true;
	}
	else
	{
		const char* error = importer.GetErrorString();
		fprintf(stderr, "\t%s\n", error);
		return false;
	}
}
