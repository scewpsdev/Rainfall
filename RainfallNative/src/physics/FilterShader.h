#pragma once

#include "PxFiltering.h"


namespace Physics
{
	using namespace physx;

	PxFilterFlags FilterShader(
		PxFilterObjectAttributes attributes0,
		PxFilterData filterData0,
		PxFilterObjectAttributes attributes1,
		PxFilterData filterData1,
		PxPairFlags& pairFlags,
		const void* constantBlock,
		PxU32 constantBlockSize);
}
