#include "FilterShader.h"

#include <PxRigidActor.h>
#include <PxShape.h>
#include <PxFiltering.h>

#include <foundation/PxIntrinsics.h>
#include <foundation/PxSimpleTypes.h>


using namespace physx;

PxFilterFlags FilterShader(
	PxFilterObjectAttributes attributes0,
	PxFilterData filterData0,
	PxFilterObjectAttributes attributes1,
	PxFilterData filterData1,
	PxPairFlags& pairFlags,
	const void* constantBlock,
	PxU32 constantBlockSize)
{
	PX_UNUSED(constantBlock);
	PX_UNUSED(constantBlockSize);

	if ((filterData0.word0 & filterData1.word1) || (filterData1.word0 & filterData0.word1))
	{
		// let triggers through
		if (PxFilterObjectIsTrigger(attributes0) || PxFilterObjectIsTrigger(attributes1))
		{
			pairFlags = PxPairFlag::eTRIGGER_DEFAULT;
			return PxFilterFlag::eDEFAULT;
		}

		pairFlags = PxPairFlag::eCONTACT_DEFAULT | PxPairFlag::eNOTIFY_TOUCH_FOUND;
		return PxFilterFlag::eDEFAULT;
	}
	return PxFilterFlag::eKILL;
}
