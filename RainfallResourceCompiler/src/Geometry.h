#pragma once

#include <stdint.h>


struct Vector2
{
	float x, y;
};

struct Vector3
{
	float x, y, z;
};

struct Vector4
{
	float x, y, z, w;
};

struct Vector4i
{
	int x, y, z, w;
};

struct Quaternion
{
	float x, y, z, w;
};

struct BoneWeights
{
	Vector4 weights;
	Vector4 boneIDs;
};

struct AABB
{
	float x0, y0, z0;
	float x1, y1, z1;
};

struct Sphere
{
	float xcenter, ycenter, zcenter;
	float radius;
};

struct MeshData
{
	Vector3* positions;
	Vector3* normals;
	Vector3* tangents;
	Vector2* texcoords;
	uint32_t* vertexColors;
	BoneWeights* boneWeights;

	int vertexCount;

	int* indexData;
	int indexCount;

	int materialID;
	int skeletonID;

	AABB boundingBox;
	Sphere boundingSphere;

	~MeshData()
	{
		if (positions)
			delete[] positions;
		if (normals)
			delete[] normals;
		if (tangents)
			delete[] tangents;
		if (texcoords)
			delete[] texcoords;
		if (vertexColors)
			delete[] vertexColors;
		if (boneWeights)
			delete[] boneWeights;
		if (indexData)
			delete[] indexData;
	}
};

struct TextureData
{
	char path[256];
	bool isEmbedded;
	int width, height;
	uint32_t* data;

	~TextureData()
	{
		if (data)
			delete[] data;
	}
};

struct MaterialData
{
	uint32_t color;
	float metallicFactor = 0.0f;
	float roughnessFactor = 1.0f;
	Vector3 emissiveColor = {};
	float emissiveStrength = 1.0f;

	TextureData* diffuse;
	TextureData* normal;
	TextureData* roughness;
	TextureData* metallic;
	TextureData* emissive;
	TextureData* height;
};

struct BoneData
{
	char name[32];
	float offsetMatrix[16];

	int nodeID;
};

struct SkeletonData
{
	int boneCount;
	BoneData* bones;
	//float(*boneTransforms)[16];
	//char** boneNames;

	//float invBindPose[16];
};

struct PositionKeyframe
{
	Vector3 value;
	float time;
};

struct RotationKeyframe
{
	Quaternion value;
	float time;
};

struct ScaleKeyframe
{
	Vector3 value;
	float time;
};

struct AnimationChannel
{
	char nodeName[32];

	int positionsOffset, positionsCount;
	int rotationsOffset, rotationsCount;
	int scalesOffset, scalesCount;
};

struct AnimationData
{
	char name[32];

	float duration;

	int numPositions;
	int numRotations;
	int numScales;
	int numChannels;

	PositionKeyframe* positionKeyframes;
	RotationKeyframe* rotationKeyframes;
	ScaleKeyframe* scaleKeyframes;
	AnimationChannel* channels;
};

struct NodeData
{
	int id;
	char name[32];
	float transform[16];

	int numChildren;
	int* children;

	int numMeshes;
	int* meshes;
};

enum class LightType
{
	Undefined,
	Directional,
	Point,
	Spot,
	Ambient,
	Area,
};

struct LightData
{
	char name[32];

	LightType type;
	float x, y, z;
	float xdir, ydir, zdir;
	Vector3 color;
};

struct SceneData
{
	int numMeshes;
	int numMaterials;
	int numSkeletons;
	int numAnimations;
	int numNodes;
	int numLights;

	MeshData* meshes;
	MaterialData* materials;
	SkeletonData* skeletons;
	AnimationData* animations;
	NodeData* nodes;
	LightData* lights;

	AABB boundingBox;
	Sphere boundingSphere;
};
