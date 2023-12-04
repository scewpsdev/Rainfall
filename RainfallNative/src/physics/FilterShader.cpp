#include "FilterShader.h"

#include <PxRigidActor.h>
#include <PxShape.h>
#include <PxFiltering.h>

#include <foundation/PxIntrinsics.h>
#include <foundation/PxSimpleTypes.h>


namespace Physics
{
	using namespace physx;

#define GROUP_SIZE	32

	class PxGroupsMask
	{
	public:
		PX_INLINE	PxGroupsMask() :bits0(0), bits1(0), bits2(0), bits3(0) {}
		PX_INLINE	~PxGroupsMask() {}

		PxU16		bits0, bits1, bits2, bits3;
	};

	struct PxFilterOp
	{
		enum Enum
		{
			PX_FILTEROP_AND,
			PX_FILTEROP_OR,
			PX_FILTEROP_XOR,
			PX_FILTEROP_NAND,
			PX_FILTEROP_NOR,
			PX_FILTEROP_NXOR,
			PX_FILTEROP_SWAP_AND
		};
	};

	struct PxCollisionBitMap
	{
		PX_INLINE PxCollisionBitMap() : enable(true) {}

		bool operator()() const { return enable; }
		bool& operator= (const bool& v) { enable = v; return enable; }

	private:
		bool enable;
	};

	PxCollisionBitMap gCollisionTable[GROUP_SIZE][GROUP_SIZE];

	PxFilterOp::Enum gFilterOps[3] = { PxFilterOp::PX_FILTEROP_AND, PxFilterOp::PX_FILTEROP_AND, PxFilterOp::PX_FILTEROP_AND };

	PxGroupsMask gFilterConstants[2];

	bool gFilterBool = false;

	static void gAND(PxGroupsMask& results, const PxGroupsMask& mask0, const PxGroupsMask& mask1)
	{
		results.bits0 = PxU16(mask0.bits0 & mask1.bits0);
		results.bits1 = PxU16(mask0.bits1 & mask1.bits1);
		results.bits2 = PxU16(mask0.bits2 & mask1.bits2);
		results.bits3 = PxU16(mask0.bits3 & mask1.bits3);
	}
	static void gOR(PxGroupsMask& results, const PxGroupsMask& mask0, const PxGroupsMask& mask1)
	{
		results.bits0 = PxU16(mask0.bits0 | mask1.bits0);
		results.bits1 = PxU16(mask0.bits1 | mask1.bits1);
		results.bits2 = PxU16(mask0.bits2 | mask1.bits2);
		results.bits3 = PxU16(mask0.bits3 | mask1.bits3);
	}
	static void gXOR(PxGroupsMask& results, const PxGroupsMask& mask0, const PxGroupsMask& mask1)
	{
		results.bits0 = PxU16(mask0.bits0 ^ mask1.bits0);
		results.bits1 = PxU16(mask0.bits1 ^ mask1.bits1);
		results.bits2 = PxU16(mask0.bits2 ^ mask1.bits2);
		results.bits3 = PxU16(mask0.bits3 ^ mask1.bits3);
	}
	static void gNAND(PxGroupsMask& results, const PxGroupsMask& mask0, const PxGroupsMask& mask1)
	{
		results.bits0 = PxU16(~(mask0.bits0 & mask1.bits0));
		results.bits1 = PxU16(~(mask0.bits1 & mask1.bits1));
		results.bits2 = PxU16(~(mask0.bits2 & mask1.bits2));
		results.bits3 = PxU16(~(mask0.bits3 & mask1.bits3));
	}
	static void gNOR(PxGroupsMask& results, const PxGroupsMask& mask0, const PxGroupsMask& mask1)
	{
		results.bits0 = PxU16(~(mask0.bits0 | mask1.bits0));
		results.bits1 = PxU16(~(mask0.bits1 | mask1.bits1));
		results.bits2 = PxU16(~(mask0.bits2 | mask1.bits2));
		results.bits3 = PxU16(~(mask0.bits3 | mask1.bits3));
	}
	static void gNXOR(PxGroupsMask& results, const PxGroupsMask& mask0, const PxGroupsMask& mask1)
	{
		results.bits0 = PxU16(~(mask0.bits0 ^ mask1.bits0));
		results.bits1 = PxU16(~(mask0.bits1 ^ mask1.bits1));
		results.bits2 = PxU16(~(mask0.bits2 ^ mask1.bits2));
		results.bits3 = PxU16(~(mask0.bits3 ^ mask1.bits3));
	}

	static void gSWAP_AND(PxGroupsMask& results, const PxGroupsMask& mask0, const PxGroupsMask& mask1)
	{
		results.bits0 = PxU16(mask0.bits0 & mask1.bits2);
		results.bits1 = PxU16(mask0.bits1 & mask1.bits3);
		results.bits2 = PxU16(mask0.bits2 & mask1.bits0);
		results.bits3 = PxU16(mask0.bits3 & mask1.bits1);
	}

	typedef void	(*FilterFunction)	(PxGroupsMask& results, const PxGroupsMask& mask0, const PxGroupsMask& mask1);

	FilterFunction const gTable[] = { gAND, gOR, gXOR, gNAND, gNOR, gNXOR, gSWAP_AND };

	static PxFilterData convert(const PxGroupsMask& mask)
	{
		PxFilterData fd;

		fd.word2 = PxU32(mask.bits0 | (mask.bits1 << 16));
		fd.word3 = PxU32(mask.bits2 | (mask.bits3 << 16));

		return fd;
	}

	static PxGroupsMask convert(const PxFilterData& fd)
	{
		PxGroupsMask mask;

		mask.bits0 = PxU16((fd.word2 & 0xffff));
		mask.bits1 = PxU16((fd.word2 >> 16));
		mask.bits2 = PxU16((fd.word3 & 0xffff));
		mask.bits3 = PxU16((fd.word3 >> 16));

		return mask;
	}

	static bool getFilterData(const PxActor& actor, PxFilterData& fd)
	{
		PxActorType::Enum aType = actor.getType();
		switch (aType)
		{
		case PxActorType::eRIGID_DYNAMIC:
		case PxActorType::eRIGID_STATIC:
		case PxActorType::eARTICULATION_LINK:
		{
			const PxRigidActor& rActor = static_cast<const PxRigidActor&>(actor);
			//AssertMsg(rActor.getNbShapes() >= 1, "There must be a shape in actor");

			PxShape* shape = NULL;
			rActor.getShapes(&shape, 1);

			fd = shape->getSimulationFilterData();
		}
		break;

		case PxActorType::eACTOR_COUNT:
		case PxActorType::eACTOR_FORCE_DWORD:
			break;
		}

		return true;
	}

	PX_FORCE_INLINE static void adjustFilterData(bool groupsMask, const PxFilterData& src, PxFilterData& dst)
	{
		if (groupsMask)
		{
			dst.word2 = src.word2;
			dst.word3 = src.word3;
		}
		else
			dst.word0 = src.word0;
	}

	template<bool TGroupsMask>
	static void setFilterData(PxActor& actor, const PxFilterData& fd)
	{
		PxActorType::Enum aType = actor.getType();
		switch (aType)
		{
		case PxActorType::eRIGID_DYNAMIC:
		case PxActorType::eRIGID_STATIC:
		case PxActorType::eARTICULATION_LINK:
		{
			const PxRigidActor& rActor = static_cast<const PxRigidActor&>(actor);

			PxShape* shape;
			for (PxU32 i = 0; i < rActor.getNbShapes(); i++)
			{
				rActor.getShapes(&shape, 1, i);

				// retrieve current group mask
				PxFilterData resultFd = shape->getSimulationFilterData();

				adjustFilterData(TGroupsMask, fd, resultFd);

				// set new filter data
				shape->setSimulationFilterData(resultFd);
			}
		}
		break;
		case PxActorType::eACTOR_COUNT:
		case PxActorType::eACTOR_FORCE_DWORD:
			break;
		}
	}

	PxFilterFlags FilterShader_(
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

		// let triggers through
		if (PxFilterObjectIsTrigger(attributes0) || PxFilterObjectIsTrigger(attributes1))
		{
			pairFlags = PxPairFlag::eTRIGGER_DEFAULT;
			return PxFilterFlag::eDEFAULT;
		}

		// Collision Group
		if (!gCollisionTable[filterData0.word0][filterData1.word0]())
		{
			return PxFilterFlag::eSUPPRESS;
		}

		// Filter function
		PxGroupsMask g0 = convert(filterData0);
		PxGroupsMask g1 = convert(filterData1);

		PxGroupsMask g0k0;	gTable[gFilterOps[0]](g0k0, g0, gFilterConstants[0]);
		PxGroupsMask g1k1;	gTable[gFilterOps[1]](g1k1, g1, gFilterConstants[1]);
		PxGroupsMask final;	gTable[gFilterOps[2]](final, g0k0, g1k1);

		bool r = final.bits0 || final.bits1 || final.bits2 || final.bits3;
		if (r != gFilterBool)
		{
			return PxFilterFlag::eSUPPRESS;
		}

		pairFlags = PxPairFlag::eCONTACT_DEFAULT | PxPairFlag::eNOTIFY_CONTACT_POINTS;

		return PxFilterFlag::eDEFAULT;
	}

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

		if (filterData0.word0 == 2 || filterData1.word0 == 2)
		{
			int a = 5;
		}

		if (!(filterData0.word0 & filterData1.word1) || !(filterData1.word0 & filterData0.word1))
		{
			return PxFilterFlag::eKILL;
		}

		// let triggers through
		if (PxFilterObjectIsTrigger(attributes0) || PxFilterObjectIsTrigger(attributes1))
		{
			pairFlags = PxPairFlag::eTRIGGER_DEFAULT;
			return PxFilterFlag::eDEFAULT;
		}

		pairFlags = PxPairFlag::eCONTACT_DEFAULT | PxPairFlag::eNOTIFY_CONTACT_POINTS;
		return PxFilterFlag::eDEFAULT;
	}
}
