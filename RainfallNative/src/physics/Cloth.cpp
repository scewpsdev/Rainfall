#ifndef RAINFALL_EXCLUDE_PHYSICS

#include "Cloth.h"

#include "Rainfall.h"
#include "Application.h"
#include "Console.h"

#include "graphics/Geometry.h"

#include "vector/Matrix.h"

#include "utils/List.h"

#include <NvCloth/Factory.h>
#include <NvCloth/Cloth.h>
#include <NvCloth/Fabric.h>
#include <NvCloth/Solver.h>

#include <NvClothExt/ClothFabricCooker.h>
#include <NvClothExt/ClothMeshDesc.h>

#include <foundation/PxErrorCallback.h>
#include <foundation/PxVec3.h>
#include <foundation/PxQuat.h>

#include <bx/allocator.h>


using namespace physx;


struct AssertHandler : nv::cloth::PxAssertHandler
{
	void operator()(const char* exp, const char* file, int line, bool& ignore) override
	{
		__debugbreak();
	}
};

struct ProfilerCallback : PxProfilerCallback
{
	void* zoneStart(const char* eventName, bool detached, uint64_t contextId) override
	{
		return nullptr;
	}

	void zoneEnd(void* profilerData, const char* eventName, bool detached, uint64_t contextId) override
	{
	}
};


static AssertHandler assertHandler;
static ProfilerCallback profilerCallback;

static nv::cloth::Factory* factory;

static nv::cloth::Solver* solver;

static List<Cloth*> cloths;


void Physics_ClothInit(PxAllocatorCallback* allocator, PxErrorCallback* errorHandler)
{
	nv::cloth::InitializeNvCloth(allocator, errorHandler, &assertHandler, &profilerCallback);
	factory = NvClothCreateFactoryCPU();
	if (!factory)
	{
		Console_Error("Failed to create NvCloth factory!");
		return;
	}

	solver = factory->createSolver();
}

void Physics_ClothTerminate()
{
	NvClothDestroyFactory(factory);
}

void Physics_ClothUpdate(float delta)
{
	solver->beginSimulation(delta);

	for (int i = 0; i < solver->getSimulationChunkCount(); i++)
		solver->simulateChunk(i);

	solver->endSimulation();

	for (int i = 0; i < cloths.size; i++)
	{
		Cloth* cloth = cloths[i];
		nv::cloth::MappedRange<PxVec4> particles = cloth->cloth->getCurrentParticles();

		bgfx::update(cloth->animatedPosition, 0, bgfx::copy(particles.begin(), particles.size() * sizeof(PxVec4)));

		// Update normals
		Vector4* normalTangents = (Vector4*)BX_ALLOC(Application_GetAllocator(), particles.size() * 2 * sizeof(Vector4));
		memset(normalTangents, 0, particles.size() * 2 * sizeof(Vector4));
		for (int i = 0; i < cloth->mesh->indexCount / 3; i++)
		{
			int i0 = cloth->mesh->indexData[i * 3 + 0];
			int i1 = cloth->mesh->indexData[i * 3 + 1];
			int i2 = cloth->mesh->indexData[i * 3 + 2];
			Vector3 position0 = *(Vector3*)&particles[i0];
			Vector3 position1 = *(Vector3*)&particles[i1];
			Vector3 position2 = *(Vector3*)&particles[i2];
			Vector3 normal = cross(position1 - position0, position2 - position0);
			Vector3 tangent = cloth->mesh->positionsNormalsTangents[i * 3 + 0].tangent;
			normalTangents[i0 * 2 + 0] += Vector4(normal, 1);
			normalTangents[i0 * 2 + 1] += Vector4(tangent, 1);
			normalTangents[i1 * 2 + 0] += Vector4(normal, 1);
			normalTangents[i1 * 2 + 1] += Vector4(tangent, 1);
			normalTangents[i2 * 2 + 0] += Vector4(normal, 1);
			normalTangents[i2 * 2 + 1] += Vector4(tangent, 1);
		}

		for (int i = 0; i < (int)particles.size(); i++)
		{
			Vector4& normal = normalTangents[i * 2 + 0];
			normal.xyz /= normal.w;

			Vector4& tangent = normalTangents[i * 2 + 1];
			tangent.xyz /= tangent.w;
		}

		bgfx::update(cloth->animatedNormalTangent, 0, bgfx::copy(normalTangents, particles.size() * 2 * sizeof(Vector4)));

		BX_FREE(Application_GetAllocator(), normalTangents);
	}
}

RFAPI void Physics_ClothsSetWind(Vector3 wind)
{
	for (int i = 0; i < cloths.size; i++)
		cloths[i]->cloth->setWindVelocity(PxVec3(wind.x, wind.y, wind.z));
}

RFAPI Cloth* Physics_CreateCloth(MeshData* mesh, float* invMasses, Vector3 position, Quaternion rotation)
{
	nv::cloth::ClothMeshDesc meshDesc;

	PxVec4* particles = (PxVec4*)BX_ALLOC(Application_GetAllocator(), mesh->vertexCount * sizeof(PxVec4));
	for (int i = 0; i < mesh->vertexCount; i++)
	{
		float invMass = invMasses[i];
		particles[i] = PxVec4(mesh->positionsNormalsTangents[i].position.x, mesh->positionsNormalsTangents[i].position.y, mesh->positionsNormalsTangents[i].position.z, invMass);
	}

	PxVec4* normalTangents = (PxVec4*)BX_ALLOC(Application_GetAllocator(), mesh->vertexCount * 2 * sizeof(PxVec4));
	for (int i = 0; i < mesh->vertexCount; i++)
	{
		normalTangents[i * 2 + 0] = PxVec4(mesh->positionsNormalsTangents[i].normal.x, mesh->positionsNormalsTangents[i].normal.y, mesh->positionsNormalsTangents[i].normal.z, 0);
		normalTangents[i * 2 + 1] = PxVec4(mesh->positionsNormalsTangents[i].tangent.x, mesh->positionsNormalsTangents[i].tangent.y, mesh->positionsNormalsTangents[i].tangent.z, 0);
	}

	//Fill meshDesc with data
	meshDesc.setToDefault();
	meshDesc.points.data = particles;
	meshDesc.points.stride = sizeof(particles[0]);
	meshDesc.points.count = mesh->vertexCount;
	meshDesc.invMasses.data = &particles[0].w;
	meshDesc.invMasses.stride = sizeof(particles[0]);
	meshDesc.invMasses.count = mesh->vertexCount;
	meshDesc.triangles.data = mesh->indexData;
	meshDesc.triangles.stride = 3 * sizeof(int);
	meshDesc.triangles.count = mesh->indexCount / 3;
	BX_ASSERT(meshDesc.isValid(), "");

	PxVec3 gravity(0.0f, -9.8f, 0.0f);
	nv::cloth::Vector<int32_t>::Type phaseTypeInfo;
	nv::cloth::Fabric* fabric = NvClothCookFabricFromMesh(factory, meshDesc, gravity, &phaseTypeInfo);

	nv::cloth::Cloth* cloth = factory->createCloth(nv::cloth::Range<PxVec4>(particles, particles + mesh->vertexCount), *fabric);

	nv::cloth::PhaseConfig* phases = (nv::cloth::PhaseConfig*)BX_ALLOC(Application_GetAllocator(), fabric->getNumPhases() * sizeof(nv::cloth::PhaseConfig));
	for (int i = 0; i < (int)fabric->getNumPhases(); i++)
	{
		phases[i].mPhaseIndex = i; // Set index to the corresponding set (constraint group)

		//Give phases different configs depending on type
		switch (phaseTypeInfo[i])
		{
		case nv::cloth::ClothFabricPhaseType::eINVALID:
			//ERROR
			break;
		case nv::cloth::ClothFabricPhaseType::eVERTICAL:
			break;
		case nv::cloth::ClothFabricPhaseType::eHORIZONTAL:
			break;
		case nv::cloth::ClothFabricPhaseType::eBENDING:
			break;
		case nv::cloth::ClothFabricPhaseType::eSHEARING:
			break;
		}

		//For this example we give every phase the same config
		phases[i].mStiffness = 10.0f;
		phases[i].mStiffnessMultiplier = 1.0f;
		phases[i].mCompressionLimit = 1.0f;
		phases[i].mStretchLimit = 1.0f;
	}
	cloth->setPhaseConfig(nv::cloth::Range<nv::cloth::PhaseConfig>(phases, phases + fabric->getNumPhases()));

	cloth->setDragCoefficient(0.5f);
	cloth->setLiftCoefficient(0.6f);

	cloth->setGravity(PxVec3(0.0f, -9.8f, 0.0f));
	cloth->teleportToLocation(PxVec3(position.x, position.y, position.z), PxQuat(rotation.x, rotation.y, rotation.z, rotation.w));

	solver->addCloth(cloth);

	Cloth* c = BX_NEW(Application_GetAllocator(), Cloth);
	c->cloth = cloth;
	c->fabric = fabric;
	c->mesh = mesh;
	c->position = position;
	c->rotation = rotation;

	bgfx::VertexLayout positionLayout;
	positionLayout.begin().add(bgfx::Attrib::TexCoord1, 4, bgfx::AttribType::Float).end();
	c->animatedPosition = bgfx::createDynamicVertexBuffer(bgfx::copy(particles, mesh->vertexCount * sizeof(PxVec4)), positionLayout);

	bgfx::VertexLayout normalTangentLayout;
	normalTangentLayout.begin().add(bgfx::Attrib::TexCoord2, 4, bgfx::AttribType::Float).add(bgfx::Attrib::Weight, 4, bgfx::AttribType::Float).end();
	c->animatedNormalTangent = bgfx::createDynamicVertexBuffer(bgfx::copy(normalTangents, mesh->vertexCount * 2 * sizeof(PxVec4)), normalTangentLayout);

	cloth->setUserData(c);

	cloths.add(c);

	BX_FREE(Application_GetAllocator(), phases);
	BX_FREE(Application_GetAllocator(), particles);
	BX_FREE(Application_GetAllocator(), normalTangents);
	fabric->decRefCount();

	return c;
}

RFAPI void Physics_DestroyCloth(Cloth* cloth)
{
	cloths.remove(cloth);

	solver->removeCloth(cloth->cloth);

	bgfx::destroy(cloth->animatedPosition);
	bgfx::destroy(cloth->animatedNormalTangent);

	BX_DELETE(Application_GetAllocator(), cloth);
}

RFAPI void Physics_ClothSetTransform(Cloth* cloth, Vector3 position, Quaternion rotation)
{
	cloth->cloth->teleportToLocation(PxVec3(position.x, position.y, position.z), PxQuat(rotation.x, rotation.y, rotation.z, rotation.w));
	cloth->position = position;
	cloth->rotation = rotation;
}

RFAPI void Physics_ClothMoveTo(Cloth* cloth, Vector3 position, Quaternion rotation)
{
	cloth->cloth->setTranslation(PxVec3(position.x, position.y, position.z));
	cloth->cloth->setRotation(PxQuat(rotation.x, rotation.y, rotation.z, rotation.w));
	cloth->position = position;
	cloth->rotation = rotation;
}

RFAPI void Physics_ClothSetSpheres(Cloth* cloth, Vector4* spheres, int numSpheres, int first, int last)
{
	cloth->cloth->setSpheres(nv::cloth::Range<PxVec4>((PxVec4*)spheres, (PxVec4*)spheres + numSpheres), first, last);
}

RFAPI int Physics_ClothGetNumSpheres(Cloth* cloth)
{
	return (int)cloth->cloth->getNumSpheres();
}

RFAPI void Physics_ClothSetCapsules(Cloth* cloth, Vector2i* capsules, int numCapsules, int first, int last)
{
	cloth->cloth->setCapsules(nv::cloth::Range<uint32_t>((uint32_t*)capsules, (uint32_t*)(capsules + numCapsules)), first, last);
}

RFAPI int Physics_ClothGetNumCapsules(Cloth* cloth)
{
	return (int)cloth->cloth->getNumCapsules();
}

#endif