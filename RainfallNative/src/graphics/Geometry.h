#pragma once

#include "vector/Vector.h"
#include "vector/Quaternion.h"
#include "vector/Matrix.h"

#include <stdint.h>

#include <bgfx/bgfx.h>


struct PositionNormalTangent
{
	Vector3 position;
	Vector3 normal;
	Vector3 tangent;
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
	PositionNormalTangent* positionsNormalsTangents = nullptr;
	Vector2* texcoords = nullptr;
	uint32_t* vertexColors = nullptr;
	BoneWeights* boneWeights = nullptr;

	int vertexCount;

	int* indexData;
	int indexCount;

	int materialID;
	int skeletonID;

	AABB boundingBox;
	Sphere boundingSphere;


	struct NodeData* node = nullptr;

	bgfx::VertexBufferHandle vertexNormalTangentBuffer = BGFX_INVALID_HANDLE;
	bgfx::VertexBufferHandle texcoordBuffer = BGFX_INVALID_HANDLE;
	bgfx::VertexBufferHandle vertexColorBuffer = BGFX_INVALID_HANDLE;
	bgfx::VertexBufferHandle boneWeightBuffer = BGFX_INVALID_HANDLE;

	bgfx::IndexBufferHandle indexBuffer = BGFX_INVALID_HANDLE;
};

struct TextureData
{
	char path[256];
	bool isEmbedded;
	int width, height;
	uint32_t* data;

	bgfx::TextureHandle handle;
};

struct MaterialData
{
	uint32_t color;
	float metallicFactor;
	float roughnessFactor;
	Vector3 emissiveColor;
	float emissiveStrength;

	TextureData* diffuse;
	TextureData* normal;
	TextureData* roughness;
	TextureData* metallic;
	TextureData* emissive;
};

struct BoneData
{
	char name[32];
	Matrix offsetMatrix;

	int nodeID;


	struct NodeData* node = nullptr;
};

struct SkeletonData
{
	int boneCount;
	BoneData* bones;
	//float(*boneTransforms)[16];
	//char** boneNames;

	Matrix inverseBindPose = Matrix::Identity;
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


	PositionKeyframe* positions = nullptr;
	RotationKeyframe* rotations = nullptr;
	ScaleKeyframe* scales = nullptr;

	struct NodeData* node = nullptr;
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
	Matrix transform;

	int numChildren;
	int* children;

	int numMeshes;
	int* meshes;


	NodeData* parent = nullptr;
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
	uint32_t color;
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
};
