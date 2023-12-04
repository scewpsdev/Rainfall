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
