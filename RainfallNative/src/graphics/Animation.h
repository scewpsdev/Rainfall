#pragma once

#include "vector/Matrix.h"


struct SkeletonState
{
	int numBones;
	Matrix* boneTransforms;
};

struct AnimationState
{
	int numSkeletons;
	SkeletonState** skeletons;
};


Vector3 SkinVertex(Vector3 position, Vector4 weights, Vector4 indices, SkeletonState* skeleton);
