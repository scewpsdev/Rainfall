#ifndef RAINFALL_EXCLUDE_PHYSICS

#include "Physics.h"

#include "Cloth.h"

#include "Application.h"
#include "Console.h"
#include "Hash.h"
#include "Resource.h"

#include "FilterShader.h"

#include "graphics/Model.h"

#include "vector/Vector.h"
#include "vector/Quaternion.h"
#include "vector/Math.h"

#include <PxPhysicsAPI.h>
#include <extensions/PxParticleClothCooker.h>

#include <bx/allocator.h>

#include <math.h>

#include <vector>


#define PVD_HOST "localhost"

#define NUM_THREADS 1

#define UPDATES_PER_SECOND 60


using namespace physx;
using namespace physx::ExtGpu;


struct AllocatorCallback : PxAllocatorCallback
{
	void* allocate(size_t size, const char* typeName, const char* filename, int line) override
	{
		return bx::alloc(Application_GetAllocator(), size, 16, bx::Location(filename, line));
	}

	void deallocate(void* ptr) override
	{
		bx::free(Application_GetAllocator(), ptr, 16);
	}
};

struct ErrorCallback : PxErrorCallback
{
	void reportError(physx::PxErrorCode::Enum code, const char* message, const char* file, int line) override
	{
		Console_Error("PhysX Error %d at %s:%d: %s", (int)code, file, line, message);
	}
};

enum class ActorType
{
	RigidBody,
	CharacterController
};

enum class RigidBodyType
{
	Null = 0,
	Static,
	Kinematic,
	Dynamic,
};

enum class ContactType
{
	Found,
	Lost,
};

typedef void (*RigidBodySetTransformCallback_t)(Vector3 position, Quaternion rotation, struct RigidBody* userPtr);
typedef bool (*RigidBodyGetTransformCallback_t)(Vector3* outPosition, Quaternion* outRotation, struct RigidBody* userPtr);
typedef void (*RigidBodyContactCallback_t)(struct RigidBody* body, struct RigidBody* other, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType, struct CharacterController* otherController);

struct RigidBody
{
	ActorType actorType = ActorType::RigidBody;

	RigidBodyType type;
	float density;
	Vector3 centerOfMass;

	physx::PxRigidActor* actor;

	Vector3 position0, position1;
	Quaternion rotation0, rotation1;
};


struct ControllerHit
{
	Vector3 position;
	Vector3 normal;
	float length;
	Vector3 direction;
};

typedef void (*CharacterControllerSetPositionCallback_t)(Vector3 position, CharacterController* userPtr);

typedef void (*CharacterControllerOnHitCallback_t)(ControllerHit hit, CharacterController* userPtr);

struct CharacterController
{
	ActorType actorType = ActorType::CharacterController;

	PxCapsuleController* controller;
	Vector3 offset;

	PxUserControllerHitReport* hitCallback;

	int64_t lastMove = 0;
};


struct HitData
{
	float distance;
	Vector3 position;
	Vector3 normal;
	int isTrigger;

	void* userData;
};

static bool operator==(const PxContactPair& pair0, const PxContactPair& pair1)
{
	return pair0.shapes[0] == pair1.shapes[0] && pair0.shapes[1] == pair1.shapes[1];
}

static bool operator==(const PxTriggerPair& pair0, const PxTriggerPair& pair1)
{
	return pair0.triggerActor == pair1.triggerActor && pair0.otherActor == pair1.otherActor && pair0.triggerShape == pair1.triggerShape && pair0.otherShape == pair1.otherShape;
}

struct SimulationEventCallback : PxSimulationEventCallback
{
	struct ContactEvent
	{
		RigidBody* body;
		RigidBody* otherBody;
		int shapeID;
		int otherShapeID;
		bool isTrigger;
		bool otherTrigger;
		ContactType contactType;
		CharacterController* otherController;
	};

	std::vector<ContactEvent> contactEvents;


	virtual void onConstraintBreak(PxConstraintInfo* constraints, PxU32 count) {}

	virtual void onWake(PxActor** actors, PxU32 count) {}

	virtual void onSleep(PxActor** actors, PxU32 count) {}

	virtual void onContact(const PxContactPairHeader& pairHeader, const PxContactPair* pairs, PxU32 nbPairs)
	{
		for (uint32_t i = 0; i < nbPairs; i++)
		{
			const PxContactPair& pair = pairs[i];

			//if (pair.flags & PxContactPairFlag::eREMOVED_SHAPE_0 || pair.flags & PxContactPairFlag::eREMOVED_SHAPE_1)
			//	continue;

			RigidBody* body = (RigidBody*)pair.shapes[0]->getActor()->userData;
			RigidBody* other = (RigidBody*)pair.shapes[1]->getActor()->userData;

			int shapeID = getShapeID(pair.shapes[0]->getActor(), pair.shapes[0]);
			int otherShapeID = getShapeID(pair.shapes[1]->getActor(), pair.shapes[1]);

			CharacterController* otherController = nullptr;
			bool isController = other->actorType == ActorType::CharacterController;

			if (isController)
			{
				otherController = (CharacterController*)other;
				other = nullptr;
			}

			if (pair.events & PxPairFlag::eNOTIFY_TOUCH_FOUND)
			{
				contactEvents.push_back({ body, other, shapeID, otherShapeID, false, false, ContactType::Found, otherController });

				//contactEvents.push_back(pair);
			}
			else if (pair.events & PxPairFlag::eNOTIFY_TOUCH_LOST)
			{
				contactEvents.push_back({ body, other, shapeID, otherShapeID, false, false, ContactType::Lost, otherController });

				// Reimplementation of contactPairs.remove() because C++ templates suck ass
				/*
				for (size_t i = 0; i < contactPairs.size(); i++)
				{
					if (contactPairs[i] == pair)
					{
						contactPairs.erase(contactPairs.begin() + i);
						break;
					}
				}
				*/
			}
		}
	}

	int getShapeID(PxRigidActor* actor, PxShape* shape)
	{
		int n = actor->getNbShapes();
		PxShape* shapes[1];
		int numShapes = actor->getShapes(shapes, 1);
		for (int i = 0; i < numShapes; i++)
		{
			if (shapes[i] == shape)
				return i;
		}
		return -1;
	}

	virtual void onTrigger(PxTriggerPair* pairs, PxU32 count)
	{
		for (uint32_t i = 0; i < count; i++)
		{
			PxTriggerPair& pair = pairs[i];

			if (pair.flags & PxTriggerPairFlag::eREMOVED_SHAPE_TRIGGER || pair.flags & PxTriggerPairFlag::eREMOVED_SHAPE_OTHER)
				continue;

			if (!pair.otherActor->userData)
				continue;

			RigidBody* body = (RigidBody*)pair.triggerActor->userData;
			RigidBody* other = (RigidBody*)pair.otherActor->userData;

			int shapeID = getShapeID((PxRigidActor*)pair.triggerActor, pair.triggerShape);
			int otherShapeID = getShapeID((PxRigidActor*)pair.otherActor, pair.otherShape);

			CharacterController* otherController = nullptr;
			bool isController = other->actorType == ActorType::CharacterController;

			if (isController)
			{
				otherController = (CharacterController*)other;
				other = nullptr;
			}

			if (pair.status & PxPairFlag::eNOTIFY_TOUCH_FOUND)
			{
				contactEvents.push_back({ body, other, shapeID, otherShapeID, true, pair.otherShape->getFlags() & PxShapeFlag::eTRIGGER_SHAPE, ContactType::Found, otherController });

				//triggerPairs.push_back(pair);
			}
			else if (pair.status & PxPairFlag::eNOTIFY_TOUCH_LOST)
			{
				contactEvents.push_back({ body, other, shapeID, otherShapeID, true, pair.otherShape->getFlags() & PxShapeFlag::eTRIGGER_SHAPE, ContactType::Lost, otherController });

				// Reimplementation of triggerPairs.remove() because C++ templates suck ass
				/*
				for (size_t i = 0; i < triggerPairs.size(); i++)
				{
					if (triggerPairs[i] == pair)
					{
						triggerPairs.erase(triggerPairs.begin() + i);
						break;
					}
				}
				*/
			}
		}
	}

	virtual void onAdvance(const PxRigidBody* const* bodyBuffer, const PxTransform* poseBuffer, const PxU32 count) {}
};


static PxFoundation* foundation;
static PxPhysics* physics;
static PxPvd* pvd;
static PxScene* scene;
static PxControllerManager* controllerManager;

static ErrorCallback errorHandler;
static AllocatorCallback allocator;
static SimulationEventCallback simulationEventCallback;
static PxTolerancesScale tolerancesScale;
static PxCookingParams cookingParams(tolerancesScale);

static RigidBodySetTransformCallback_t rigidBodySetTransform;
static RigidBodyGetTransformCallback_t rigidBodyGetTransform;
static RigidBodyContactCallback_t rigidBodyOnContact;

static CharacterControllerSetPositionCallback_t controllerSetPosition;
static CharacterControllerOnHitCallback_t controllerOnHit;

//static int64_t lastFrameTime;
static float timeAccumulator;
static int64_t simulationDelta;

static std::vector<RigidBody*> rigidBodies;
static std::vector<CharacterController*> controllers;

static std::unordered_map<uint32_t, PxMaterial*> materialCache;

void(*fixedUpdateCallback)(float delta);


static PxTransform MatrixToPxTransform(Matrix matrix)
{
	Vector3 position = matrix.translation();
	Quaternion rotation = matrix.rotation();
	PxTransform transform(PxVec3(position.x, position.y, position.z), PxQuat(rotation.x, rotation.y, rotation.z, rotation.w));
	return transform;
}

RFAPI void Physics_Init(RigidBodySetTransformCallback_t rigidBodySetTransform, RigidBodyGetTransformCallback_t rigidBodyGetTransform, RigidBodyContactCallback_t rigidBodyOnContact, CharacterControllerSetPositionCallback_t controllerSetPosition, CharacterControllerOnHitCallback_t controllerOnHit)
{
	foundation = PxCreateFoundation(PX_PHYSICS_VERSION, allocator, errorHandler);
	if (!foundation)
	{
		printf("Failed to initialize PhysX foundation\n");
		return;
	}

	pvd = PxCreatePvd(*foundation);
	physx::PxPvdTransport* pvdTransport = physx::PxDefaultPvdSocketTransportCreate(PVD_HOST, 5425, 10);
	pvd->connect(*pvdTransport, physx::PxPvdInstrumentationFlag::eALL);

	physics = PxCreatePhysics(PX_PHYSICS_VERSION, *foundation, tolerancesScale, true, pvd);
	if (!physics)
	{
		printf("Failed to initialize PhysX module\n");
		return;
	}

	PxSceneDesc sceneDesc = PxSceneDesc(tolerancesScale);
	sceneDesc.gravity = PxVec3(0.0f, -9.81f, 0.0f);
	sceneDesc.simulationEventCallback = &simulationEventCallback;
	sceneDesc.filterShader = FilterShader;
	sceneDesc.cpuDispatcher = PxDefaultCpuDispatcherCreate(NUM_THREADS);
	if (!sceneDesc.cpuDispatcher)
	{
		printf("Failed to initialize PhysX cpu dispatcher\n");
		return;
	}

	scene = physics->createScene(sceneDesc);
	if (!scene)
	{
		printf("Failed to initialize PhysX scene\n");
		return;
	}

	controllerManager = PxCreateControllerManager(*scene);


	Physics_ClothInit(&allocator, &errorHandler);


	::rigidBodySetTransform = rigidBodySetTransform;
	::rigidBodyGetTransform = rigidBodyGetTransform;
	::rigidBodyOnContact = rigidBodyOnContact;

	::controllerSetPosition = controllerSetPosition;
	::controllerOnHit = controllerOnHit;


	//lastFrameTime = Application_GetCurrentTime();
	timeAccumulator = 0.0f;
}

RFAPI void Physics_Shutdown()
{
	Physics_ClothTerminate();

	controllerManager->release();
	scene->release();
	physics->release();
	foundation->release();
}

RFAPI void Physics_Update(float delta)
{
	float timeStep = 1.0f / UPDATES_PER_SECOND;

	//int64_t currentFrameTime = Application_GetCurrentTime();
	//int64_t delta = currentFrameTime - lastFrameTime;
	//lastFrameTime = currentFrameTime;
	timeAccumulator += delta;

	int64_t beforeUpdate = Application_GetTimestamp();

	int numSteps = 0;
	int maxStepsPerFrame = 10;
	while (timeAccumulator >= timeStep)
	{
		if (numSteps >= maxStepsPerFrame)
		{
			timeAccumulator = 0.0f;
			break;
		}

		for (size_t i = 0; i < rigidBodies.size(); i++)
		{
			RigidBody* body = rigidBodies[i];
			switch (body->type)
			{
			case RigidBodyType::Dynamic:
			{
				PxTransform transform = body->actor->getGlobalPose();
				body->position0 = Vector3(transform.p.x, transform.p.y, transform.p.z);
				body->rotation0 = Quaternion(transform.q.x, transform.q.y, transform.q.z, transform.q.w);
				break;
			}
			case RigidBodyType::Kinematic:
			{
				Vector3 position;
				Quaternion rotation;
				if (rigidBodyGetTransform(&position, &rotation, body))
				{
					PxRigidDynamic* dynamic = body->actor->is<PxRigidDynamic>();
					PxTransform transform(PxVec3(position.x, position.y, position.z), PxQuat(rotation.x, rotation.y, rotation.z, rotation.w));
					dynamic->setKinematicTarget(transform);
				}


				/*
				if (!(body->actor->getActorFlags() & PxActorFlag::eDISABLE_SIMULATION))
				{
					Vector3 position;
					Quaternion rotation;
					body->getTransform(&position, &rotation, body->userPtr);

					PxRigidDynamic* dynamic = body->actor->is<PxRigidDynamic>();
					PxTransform transform(PxVec3(position.x, position.y, position.z), PxQuat(rotation.x, rotation.y, rotation.z, rotation.w));
					dynamic->setKinematicTarget(transform);
				}
				*/
				break;
			}
			case RigidBodyType::Static:
			{
				break;
			}
			}
		}

		scene->simulate(timeStep);
		scene->fetchResults(true);

		timeAccumulator -= timeStep;

		for (size_t i = 0; i < rigidBodies.size(); i++)
		{
			RigidBody* body = rigidBodies[i];
			if (body->type == RigidBodyType::Dynamic)
			{
				PxRigidDynamic* dynamic = body->actor->is<PxRigidDynamic>();
				PxTransform transform = dynamic->getGlobalPose();
				body->position1 = Vector3(transform.p.x, transform.p.y, transform.p.z);
				body->rotation1 = Quaternion(transform.q.x, transform.q.y, transform.q.z, transform.q.w);
			}
		}

		for (size_t i = 0; i < simulationEventCallback.contactEvents.size(); i++)
		{
			RigidBody* body = simulationEventCallback.contactEvents[i].body;
			int shapeID = simulationEventCallback.contactEvents[i].shapeID;
			RigidBody* other = simulationEventCallback.contactEvents[i].otherBody;
			int otherShapeID = simulationEventCallback.contactEvents[i].otherShapeID;
			CharacterController* otherController = simulationEventCallback.contactEvents[i].otherController;
			ContactType contactType = simulationEventCallback.contactEvents[i].contactType;
			bool isTrigger = simulationEventCallback.contactEvents[i].isTrigger;
			bool otherTrigger = simulationEventCallback.contactEvents[i].otherTrigger;

			rigidBodyOnContact(body, other, shapeID, otherShapeID, isTrigger, otherTrigger, contactType, otherController);
		}
		simulationEventCallback.contactEvents.clear();

		Physics_ClothUpdate(timeStep);

		fixedUpdateCallback(timeStep);

		numSteps++;
	}

	float interpFactor = timeAccumulator / timeStep;

	for (size_t i = 0; i < rigidBodies.size(); i++)
	{
		RigidBody* body = rigidBodies[i];
		if (body->type == RigidBodyType::Dynamic)
		{
			PxRigidDynamic* dynamic = body->actor->is<PxRigidDynamic>();
			Vector3 position = mix(body->position0, body->position1, interpFactor);
			Quaternion rotation = slerp(body->rotation0, body->rotation1, interpFactor);
			rigidBodySetTransform(position, rotation, body);
			//body->setTransform(position, rotation, body->userPtr);
		}
	}

	int64_t afterUpdate = Application_GetTimestamp();
	if (numSteps > 0)
		simulationDelta = (afterUpdate - beforeUpdate) / numSteps;
}

RFAPI int64_t Physics_GetSimulationDelta()
{
	return simulationDelta;
}

static PxRigidActor* CreateRigidBody(const Vector3& position, const Quaternion& rotation, RigidBodyType type)
{
	PxTransform transform(PxVec3(position.x, position.y, position.z), PxQuat(rotation.x, rotation.y, rotation.z, rotation.w));

	PxRigidActor* actor = nullptr;
	switch (type)
	{
	case RigidBodyType::Dynamic:
	case RigidBodyType::Kinematic:
	{
		PxRigidDynamic* dynamic = physics->createRigidDynamic(transform);
		actor = dynamic;

		if (type == RigidBodyType::Kinematic)
			dynamic->setRigidBodyFlag(PxRigidBodyFlag::eKINEMATIC, true);
		break;
	}
	case RigidBodyType::Static:
	{
		actor = physics->createRigidStatic(transform);

		break;
	}
	}

	scene->addActor(*actor);

	return actor;
}

RFAPI RigidBody* Physics_CreateRigidBody(RigidBodyType type, float density, Vector3 centerOfMass, Vector3 position, Quaternion rotation)
{
	RigidBody* body = BX_NEW(Application_GetAllocator(), RigidBody);
	body->type = type;
	body->density = density;
	body->centerOfMass = centerOfMass;
	body->actor = CreateRigidBody(position, rotation, type);
	body->actor->userData = body;

	rigidBodies.push_back(body);

	return body;
}

RFAPI void Physics_DestroyRigidBody(RigidBody* body)
{
	if (body->actor)
	{
		scene->removeActor(*body->actor);
		body->actor->release();
		body->actor = nullptr;
	}

	rigidBodies.erase(std::find(rigidBodies.begin(), rigidBodies.end(), body));
}

RFAPI PxTriangleMesh* Physics_CreateMeshCollider(const void* vertices, int numVertices, int vertexStride, const int* indices, int numIndices)
{
	PxTriangleMeshDesc meshDesc;
	meshDesc.points.count = numVertices;
	meshDesc.points.stride = vertexStride;
	meshDesc.points.data = vertices;

	meshDesc.triangles.count = numIndices / 3;
	meshDesc.triangles.stride = 3 * sizeof(int);
	meshDesc.triangles.data = indices;

	PxDefaultMemoryOutputStream writeBuffer;
	PxTriangleMeshCookingResult::Enum result;
	bool status = PxCookTriangleMesh(cookingParams, meshDesc, writeBuffer, &result);
	if (!status)
	{
		printf("Failed to cook triangle mesh\n");
		return nullptr;
	}

	PxDefaultMemoryInputData readBuffer(writeBuffer.getData(), writeBuffer.getSize());
	PxTriangleMesh* mesh = physics->createTriangleMesh(readBuffer);

	return mesh;
}

RFAPI void Physics_DestroyMeshCollider(PxTriangleMesh* mesh)
{
	mesh->release();
}

RFAPI PxConvexMesh* Physics_CreateConvexMeshCollider(const void* vertices, int numVertices, int vertexStride, const int* indices, int numIndices)
{
	/*
	Vector3* vertexPositions = (Vector3*)BX_ALLOC(Application_GetAllocator(), numVertices * sizeof(Vector3));
	for (int i = 0; i < numVertices; i++)
		vertexPositions[i] = vertices[i].position;
		*/

	PxConvexMeshDesc meshDesc;
	meshDesc.points.count = numVertices;
	meshDesc.points.stride = vertexStride;
	meshDesc.points.data = vertices;

	meshDesc.flags = PxConvexFlag::eCOMPUTE_CONVEX | PxConvexFlag::ePLANE_SHIFTING | PxConvexFlag::eFAST_INERTIA_COMPUTATION | PxConvexFlag::eSHIFT_VERTICES;

	bool valid = meshDesc.isValid();

	PxDefaultMemoryOutputStream writeBuffer;
	PxConvexMeshCookingResult::Enum result;
	bool status = PxCookConvexMesh(cookingParams, meshDesc, writeBuffer, &result);
	if (!status)
	{
		printf("Failed to cook convex mesh\n");
		//BX_FREE(Application_GetAllocator(), vertexPositions);
		return nullptr;
	}

	PxDefaultMemoryInputData readBuffer(writeBuffer.getData(), writeBuffer.getSize());
	PxConvexMesh* mesh = physics->createConvexMesh(readBuffer);

	//BX_FREE(Application_GetAllocator(), vertexPositions);

	return mesh;
}

RFAPI void Physics_DestroyConvexMeshCollider(PxConvexMesh* mesh)
{
	mesh->release();
}

RFAPI PxHeightField* Physics_CreateHeightField(int width, int height, PxHeightFieldSample* data)
{
	PxHeightFieldDesc desc;
	//desc.flags = PxHeightFieldFlag::eNO_BOUNDARY_EDGES;
	desc.samples.stride = sizeof(PxHeightFieldSample);
	desc.samples.data = data;
	desc.nbRows = width;
	desc.nbColumns = height;

	PxDefaultMemoryOutputStream writeBuffer;
	bool status = PxCookHeightField(desc, writeBuffer);
	if (!status)
		printf("Failed to cook triangle mesh\n");

	PxDefaultMemoryInputData readBuffer(writeBuffer.getData(), writeBuffer.getSize());
	PxHeightField* heightField = physics->createHeightField(readBuffer);

	return heightField;
}

RFAPI void Physics_DestroyHeightField(PxHeightField* heightField)
{
	heightField->release();
}

static PxMaterial* GetMaterial(float staticFriction, float dynamicFriction, float restitution)
{
	static PxMaterial* mat;
	if (!mat)
		mat = physics->createMaterial(0.5f, 0.5f, 0.0f);
	return mat;

	uint32_t h = hashCombine(hashCombine(hash(staticFriction), hash(dynamicFriction)), hash(restitution));

	auto it = materialCache.find(h);
	if (it != materialCache.end())
		return it->second;

	PxMaterial* material = physics->createMaterial(staticFriction, dynamicFriction, restitution);
	materialCache.emplace(h, material);
	return material;
}

static void AddCollider(PxRigidActor* actor, const PxGeometry& geometry, uint32_t filterGroup, uint32_t filterMask, const Vector3& position, const Quaternion& rotation, float staticFriction, float dynamicFriction, float restitution, bool isDynamic, float density, Vector3 centerOfMass)
{
	PxMaterial* material = GetMaterial(staticFriction, dynamicFriction, restitution);
	PxShape* shape = physics->createShape(geometry, *material, true);

	shape->setContactOffset(0.01f);
	shape->setFlag(PxShapeFlag::eSIMULATION_SHAPE, true);
	shape->setFlag(PxShapeFlag::eTRIGGER_SHAPE, false);
	shape->setFlag(PxShapeFlag::eSCENE_QUERY_SHAPE, true);

	PxFilterData filterData;
	filterData.word0 = filterGroup;
	filterData.word1 = filterMask;
	shape->setSimulationFilterData(filterData);
	shape->setQueryFilterData(filterData);

	PxTransform relativeTransform(PxVec3(position.x, position.y, position.z), PxQuat(rotation.x, rotation.y, rotation.z, rotation.w));
	if (geometry.getType() == PxGeometryType::eCAPSULE)
		relativeTransform.q = relativeTransform.q * PxQuat(PxHalfPi, PxVec3(0, 0, 1));
	shape->setLocalPose(relativeTransform);

	actor->attachShape(*shape);

	if (isDynamic)
	{
		PxRigidBodyExt::updateMassAndInertia(*(PxRigidBody*)actor, density);
	}
}

static void AddTrigger(PxRigidActor* actor, const PxGeometry& geometry, uint32_t filterGroup, uint32_t filterMask, const Vector3& position, const Quaternion& rotation)
{
	PxMaterial* material = GetMaterial(0.0f, 0.0f, 0.0f);
	PxShape* shape = physics->createShape(geometry, *material, true);

	shape->setFlag(PxShapeFlag::eSIMULATION_SHAPE, false);
	shape->setFlag(PxShapeFlag::eTRIGGER_SHAPE, true);
	shape->setFlag(PxShapeFlag::eSCENE_QUERY_SHAPE, true);

	PxFilterData filterData;
	filterData.word0 = filterGroup;
	filterData.word1 = filterMask;
	shape->setSimulationFilterData(filterData);
	shape->setQueryFilterData(filterData);

	PxTransform relativeTransform(PxVec3(position.x, position.y, position.z), PxQuat(rotation.x, rotation.y, rotation.z, rotation.w));
	if (geometry.getType() == PxGeometryType::eCAPSULE)
		relativeTransform.q = relativeTransform.q * PxQuat(PxHalfPi, PxVec3(0, 0, 1));
	shape->setLocalPose(relativeTransform);

	actor->attachShape(*shape);
}

RFAPI void Physics_RigidBodyAddSphereCollider(RigidBody* body, float radius, const Vector3& position, uint32_t filterGroup, uint32_t filterMask, float staticFriction, float dynamicFriction, float restitution)
{
	AddCollider(body->actor, PxSphereGeometry(radius), filterGroup, filterMask, position, Quaternion::Identity, staticFriction, dynamicFriction, restitution, body->type == RigidBodyType::Dynamic, body->density, body->centerOfMass);
}

RFAPI void Physics_RigidBodyAddBoxCollider(RigidBody* body, const Vector3& halfExtents, const Vector3& position, const Quaternion& rotation, uint32_t filterGroup, uint32_t filterMask, float staticFriction, float dynamicFriction, float restitution)
{
	AddCollider(body->actor, PxBoxGeometry(halfExtents.x, halfExtents.y, halfExtents.z), filterGroup, filterMask, position, rotation, staticFriction, dynamicFriction, restitution, body->type == RigidBodyType::Dynamic, body->density, body->centerOfMass);
}

RFAPI void Physics_RigidBodyAddCapsuleCollider(RigidBody* body, float radius, float height, const Vector3& position, const Quaternion& rotation, uint32_t filterGroup, uint32_t filterMask, float staticFriction, float dynamicFriction, float restitution)
{
	AddCollider(body->actor, PxCapsuleGeometry(radius, 0.5f * height - radius), filterGroup, filterMask, position, rotation, staticFriction, dynamicFriction, restitution, body->type == RigidBodyType::Dynamic, body->density, body->centerOfMass);
}

RFAPI void Physics_RigidBodyAddMeshCollider(RigidBody* body, PxTriangleMesh* mesh, const Matrix& transform, uint32_t filterGroup, uint32_t filterMask, float staticFriction, float dynamicFriction, float restitution)
{
	Vector3 position = transform.translation();
	Quaternion rotation = transform.rotation();
	Vector3 scale = transform.scale();
	AddCollider(body->actor, PxTriangleMeshGeometry(mesh, PxMeshScale(PxVec3(scale.x, scale.y, scale.z))), filterGroup, filterMask, position, rotation, staticFriction, dynamicFriction, restitution, body->type == RigidBodyType::Dynamic, body->density, body->centerOfMass);
}

RFAPI void Physics_RigidBodyAddConvexMeshCollider(RigidBody* body, PxConvexMesh* mesh, const Matrix& transform, uint32_t filterGroup, uint32_t filterMask, float staticFriction, float dynamicFriction, float restitution)
{
	Vector3 position = transform.translation();
	Quaternion rotation = transform.rotation();
	Vector3 scale = transform.scale();
	AddCollider(body->actor, PxConvexMeshGeometry(mesh, PxMeshScale(PxVec3(scale.x, scale.y, scale.z))), filterGroup, filterMask, position, rotation, staticFriction, dynamicFriction, restitution, body->type == RigidBodyType::Dynamic, body->density, body->centerOfMass);
}

RFAPI void Physics_RigidBodyAddHeightFieldCollider(RigidBody* body, PxHeightField* heightField, const Vector3& scale, const Matrix& transform, uint32_t filterGroup, uint32_t filterMask, float staticFriction, float dynamicFriction, float restitution)
{
	AddCollider(body->actor, PxHeightFieldGeometry(heightField, (PxMeshGeometryFlags)0, scale.y, scale.x, scale.z), filterGroup, filterMask, transform.translation(), transform.rotation(), staticFriction, dynamicFriction, restitution, body->type == RigidBodyType::Dynamic, body->density, body->centerOfMass);
}

RFAPI void Physics_RigidBodyAddSphereTrigger(RigidBody* body, float radius, const Vector3& position, uint32_t filterGroup, uint32_t filterMask)
{
	AddTrigger(body->actor, PxSphereGeometry(radius), filterGroup, filterMask, position, Quaternion::Identity);
}

RFAPI void Physics_RigidBodyAddBoxTrigger(RigidBody* body, const Vector3& halfExtents, const Vector3& position, const Quaternion& rotation, uint32_t filterGroup, uint32_t filterMask)
{
	AddTrigger(body->actor, PxBoxGeometry(halfExtents.x, halfExtents.y, halfExtents.z), filterGroup, filterMask, position, rotation);
}

RFAPI void Physics_RigidBodyAddCapsuleTrigger(RigidBody* body, float radius, float height, const Vector3& position, const Quaternion& rotation, uint32_t filterGroup, uint32_t filterMask)
{
	AddTrigger(body->actor, PxCapsuleGeometry(radius, 0.5f * height - radius), filterGroup, filterMask, position, rotation);
}

RFAPI void Physics_RigidBodyAddMeshTrigger(RigidBody* body, PxTriangleMesh* mesh, const Matrix& transform, uint32_t filterGroup, uint32_t filterMask)
{
	Vector3 position = transform.translation();
	Quaternion rotation = transform.rotation();
	Vector3 scale = transform.scale();
	AddTrigger(body->actor, PxTriangleMeshGeometry(mesh, PxMeshScale(PxVec3(scale.x, scale.y, scale.z))), filterGroup, filterMask, position, rotation);
}

RFAPI void Physics_RigidBodyAddConvexMeshTrigger(RigidBody* body, PxConvexMesh* mesh, const Matrix& transform, uint32_t filterGroup, uint32_t filterMask)
{
	Vector3 position = transform.translation();
	Quaternion rotation = transform.rotation();
	Vector3 scale = transform.scale();
	AddTrigger(body->actor, PxConvexMeshGeometry(mesh, PxMeshScale(PxVec3(scale.x, scale.y, scale.z))), filterGroup, filterMask, position, rotation);
}

RFAPI void Physics_RigidBodyClearColliders(RigidBody* body)
{
	uint32_t numShapes = body->actor->getNbShapes();
	PxShape** shapeBuffer = (PxShape**)BX_ALLOC(Application_GetAllocator(), numShapes * sizeof(PxShape*));
	body->actor->getShapes(shapeBuffer, numShapes);
	for (uint32_t i = 0; i < numShapes; i++)
	{
		body->actor->detachShape(*shapeBuffer[i]);
		//shapeBuffer[i]->release();
	}
	BX_FREE(Application_GetAllocator(), shapeBuffer);
}

RFAPI void Physics_RigidBodySetSimulationEnabled(RigidBody* body, bool enabled)
{
	body->actor->setActorFlag(PxActorFlag::eDISABLE_SIMULATION, !enabled);
}

RFAPI void Physics_RigidBodySetGravityEnabled(RigidBody* body, bool enabled)
{
	body->actor->setActorFlag(PxActorFlag::eDISABLE_GRAVITY, !enabled);
}

RFAPI void Physics_RigidBodySetTransform(RigidBody* body, const Vector3& position, const Quaternion& rotation)
{
	PxTransform transform(PxVec3(position.x, position.y, position.z), PxQuat(rotation.x, rotation.y, rotation.z, rotation.w));
	if (body->type == RigidBodyType::Kinematic)
	{
		if (PxRigidDynamic* dynamic = body->actor->is<PxRigidDynamic>())
		{
			dynamic->setKinematicTarget(transform);
			//rigidBodySetTransform(position, rotation, body);
		}
		else
		{
			__debugbreak();
		}
	}
	else if (body->type == RigidBodyType::Dynamic)
	{
		if (PxRigidBody* dynamic = body->actor->is<PxRigidBody>())
		{
			dynamic->setGlobalPose(transform);
			body->position0 = body->position1 = position;
			body->rotation0 = body->rotation1 = rotation;
		}
		else
		{
			__debugbreak();
		}
	}
	else if (body->type == RigidBodyType::Static)
	{
		if (PxRigidStatic* staticBody = body->actor->is<PxRigidStatic>())
		{
			staticBody->setGlobalPose(transform);
		}
		else
		{
			__debugbreak();
		}
	}
}

RFAPI void Physics_RigidBodySetRotation(RigidBody* body, const Quaternion& rotation)
{
	PxTransform transform = body->actor->getGlobalPose();
	transform.q = PxQuat(rotation.x, rotation.y, rotation.z, rotation.w);

	if (body->type == RigidBodyType::Kinematic)
	{
		if (PxRigidDynamic* dynamic = body->actor->is<PxRigidDynamic>())
		{
			dynamic->setKinematicTarget(transform);
		}
		else
		{
			__debugbreak();
		}
	}
	else if (body->type == RigidBodyType::Dynamic)
	{
		if (PxRigidBody* dynamic = body->actor->is<PxRigidBody>())
		{
			dynamic->setGlobalPose(transform);
			body->rotation0 = body->rotation1 = rotation;
		}
		else
		{
			__debugbreak();
		}
	}
	else if (body->type == RigidBodyType::Static)
	{
		if (PxRigidStatic* staticBody = body->actor->is<PxRigidStatic>())
		{
			staticBody->setGlobalPose(transform);
		}
		else
		{
			__debugbreak();
		}
	}
}

RFAPI void Physics_RigidBodySetVelocity(RigidBody* body, const Vector3& velocity)
{
	if (PxRigidDynamic* dynamic = body->actor->is<PxRigidDynamic>())
	{
		//dynamic->addForce(PxVec3(velocity.x, velocity.y, velocity.z) - dynamic->getLinearVelocity(), PxForceMode::eVELOCITY_CHANGE);
		dynamic->setLinearVelocity(PxVec3(velocity.x, velocity.y, velocity.z));
	}
}

RFAPI void Physics_RigidBodySetRotationVelocity(RigidBody* body, const Vector3& rotvelocity)
{
	if (PxRigidDynamic* dynamic = body->actor->is<PxRigidDynamic>())
	{
		//dynamic->addTorque(PxVec3(rotvelocity.x, rotvelocity.y, rotvelocity.z) - dynamic->getAngularVelocity(), PxForceMode::eVELOCITY_CHANGE);
		dynamic->setAngularVelocity(PxVec3(rotvelocity.x, rotvelocity.y, rotvelocity.z));
	}
}

RFAPI void Physics_RigidBodySetCenterOfMass(RigidBody* body, Vector3 centerOfMass)
{
	if (PxRigidDynamic* dynamic = body->actor->is<PxRigidDynamic>())
	{
		PxTransform pose(PxVec3(centerOfMass.x, centerOfMass.y, centerOfMass.z));
		dynamic->setCMassLocalPose(pose);
	}
}

RFAPI void Physics_RigidBodyLockAxis(RigidBody* body, bool x, bool y, bool z)
{
	if (PxRigidDynamic* dynamic = body->actor->is<PxRigidDynamic>())
	{
		dynamic->setRigidDynamicLockFlag(PxRigidDynamicLockFlag::eLOCK_LINEAR_X, x);
		dynamic->setRigidDynamicLockFlag(PxRigidDynamicLockFlag::eLOCK_LINEAR_Y, y);
		dynamic->setRigidDynamicLockFlag(PxRigidDynamicLockFlag::eLOCK_LINEAR_Z, z);
	}
	else
	{
		__debugbreak();
	}
}

RFAPI void Physics_RigidBodyLockRotationAxis(RigidBody* body, bool x, bool y, bool z)
{
	if (PxRigidDynamic* dynamic = body->actor->is<PxRigidDynamic>())
	{
		dynamic->setRigidDynamicLockFlag(PxRigidDynamicLockFlag::eLOCK_ANGULAR_X, x);
		dynamic->setRigidDynamicLockFlag(PxRigidDynamicLockFlag::eLOCK_ANGULAR_Y, y);
		dynamic->setRigidDynamicLockFlag(PxRigidDynamicLockFlag::eLOCK_ANGULAR_Z, z);
	}
	else
	{
		__debugbreak();
	}
}

RFAPI void Physics_RigidBodyAddForce(RigidBody* body, const Vector3& force)
{
	if (body->type == RigidBodyType::Dynamic)
	{
		PxRigidBody* dynamic = body->actor->is<PxRigidBody>();
		dynamic->addForce(PxVec3(force.x, force.y, force.z), PxForceMode::eFORCE);
	}
}

RFAPI void Physics_RigidBodyAddTorque(RigidBody* body, const Vector3& torque)
{
	if (body->type == RigidBodyType::Dynamic)
	{
		PxRigidBody* dynamic = body->actor->is<PxRigidBody>();
		dynamic->addTorque(PxVec3(torque.x, torque.y, torque.z), PxForceMode::eFORCE);
	}
}

RFAPI void Physics_RigidBodyAddAcceleration(RigidBody* body, Vector3 acceleration)
{
	if (body->type == RigidBodyType::Dynamic)
	{
		PxRigidBody* dynamic = body->actor->is<PxRigidBody>();
		dynamic->addForce(PxVec3(acceleration.x, acceleration.y, acceleration.z), PxForceMode::eACCELERATION);
	}
}

RFAPI void Physics_RigidBodyAddImpulse(RigidBody* body, Vector3 impulse)
{
	if (body->type == RigidBodyType::Dynamic)
	{
		PxRigidBody* dynamic = body->actor->is<PxRigidBody>();
		dynamic->addForce(PxVec3(impulse.x, impulse.y, impulse.z), PxForceMode::eIMPULSE);
	}
}

RFAPI void Physics_RigidBodyGetTransform(RigidBody* body, Vector3* outPosition, Quaternion* outRotation)
{
	if (PxRigidBody* dynamic = body->actor->is<PxRigidBody>())
	{
		if (body->type == RigidBodyType::Kinematic)
		{
			PxTransform transform = dynamic->getGlobalPose();
			*outPosition = Vector3(transform.p.x, transform.p.y, transform.p.z);
			*outRotation = Quaternion(transform.q.x, transform.q.y, transform.q.z, transform.q.w);
		}
		else
		{
			// TODO simplify
			if (dynamic->is<PxArticulationLink>())
			{
				PxTransform transform = dynamic->getGlobalPose();
				*outPosition = Vector3(transform.p.x, transform.p.y, transform.p.z);
				*outRotation = Quaternion(transform.q.x, transform.q.y, transform.q.z, transform.q.w);
			}
			else
			{
				*outPosition = body->position1;
				*outRotation = body->rotation1;
			}
		}
	}
}

RFAPI void Physics_RigidBodyGetVelocity(RigidBody* body, Vector3* outVelocity)
{
	if (PxRigidBody* dynamic = body->actor->is<PxRigidBody>())
	{
		PxVec3 velocity = dynamic->getLinearVelocity();
		*outVelocity = Vector3(velocity.x, velocity.y, velocity.z);
	}
}

RFAPI void Physics_RigidBodyGetAngularVelocity(RigidBody* body, Vector3* outAngularVelocity)
{
	if (PxRigidBody* dynamic = body->actor->is<PxRigidBody>())
	{
		PxVec3 velocity = dynamic->getAngularVelocity();
		*outAngularVelocity = Vector3(velocity.x, velocity.y, velocity.z);
	}
}

RFAPI void Physics_RigidBodyGetCenterOfMass(RigidBody* body, Vector3* outPosition)
{
	if (PxRigidBody* dynamic = body->actor->is<PxRigidBody>())
	{
		PxTransform centerOfMass = dynamic->getCMassLocalPose();
		*outPosition = Vector3(centerOfMass.p.x, centerOfMass.p.y, centerOfMass.p.z);
	}
}

RFAPI CharacterController* Physics_CreateCharacterController(float radius, float height, Vector3 offset, float stepOffset, Vector3 position)
{
	PxMaterial* material = GetMaterial(0.5f, 0.5f, 0.1f);

	class ControllerHitCallback : public PxUserControllerHitReport
	{
		CharacterController* controller;

	public:
		ControllerHitCallback(CharacterController* controller)
			: controller(controller)
		{
		}

		virtual void onShapeHit(const PxControllerShapeHit& hit) override
		{
			ControllerHit controllerHit;
			controllerHit.position = Vector3((float)hit.worldPos.x, (float)hit.worldPos.y, (float)hit.worldPos.z);
			controllerHit.normal = Vector3(hit.worldNormal.x, hit.worldNormal.y, hit.worldNormal.z);
			controllerHit.length = hit.length;
			controllerHit.direction = Vector3(hit.dir.x, hit.dir.y, hit.dir.z);
			controllerOnHit(controllerHit, controller);
		}

		virtual void onControllerHit(const PxControllersHit& hit) override
		{
		}

		virtual void onObstacleHit(const PxControllerObstacleHit& hit) override
		{
		}
	};

	CharacterController* controller = BX_NEW(Application_GetAllocator(), CharacterController);

	PxUserControllerHitReport* hitCallback = BX_NEW(Application_GetAllocator(), ControllerHitCallback)(controller);

	PxCapsuleControllerDesc desc;
	desc.material = material;
	desc.radius = radius; // -desc.contactOffset;
	desc.height = height - 2 * radius; // -2.0f * desc.contactOffset;
	desc.contactOffset = 0.01f;
	desc.stepOffset = stepOffset;
	desc.climbingMode = PxCapsuleClimbingMode::eEASY;
	desc.slopeLimit = 0.5f;
	desc.reportCallback = hitCallback;

	PxCapsuleController* capsule = (PxCapsuleController*)controllerManager->createController(desc);
	float x = position.x + offset.x;
	float y = position.y + offset.y;
	float z = position.z + offset.z;
	capsule->setFootPosition(PxExtendedVec3((PxExtended)x, (PxExtended)y, (PxExtended)z));

	//controller->setUserData(character);
	capsule->getActor()->userData = controller;


	controller->controller = capsule;
	controller->offset = offset;
	controller->hitCallback = hitCallback;
	controllers.push_back(controller);

	return controller;
}

RFAPI void Physics_DestroyCharacterController(CharacterController* controller)
{
	if (controller->controller)
	{
		controller->controller->release();
		controller->controller = nullptr;
		//controllers.remove(character);
	}
}

RFAPI void Physics_ResizeCharacterController(CharacterController* controller, float height)
{
	controller->controller->resize(height - 2 * controller->controller->getRadius());
}

RFAPI void Physics_CharacterControllerSetHeight(CharacterController* controller, float height)
{
	controller->controller->setHeight(height - 2 * controller->controller->getRadius());
}

RFAPI void Physics_CharacterControllerSetRadius(CharacterController* controller, float radius)
{
	controller->controller->setRadius(radius);
}

RFAPI void Physics_CharacterControllerSetOffset(CharacterController* controller, const Vector3& offset)
{
	physx::PxExtendedVec3 footPosition = controller->controller->getFootPosition();
	float dx = offset.x - controller->offset.x;
	float dy = offset.y - controller->offset.y;
	float dz = offset.z - controller->offset.z;
	//controller->controller->move(physx::PxVec3(dx, dy, dz), 0.0f, );
	controller->controller->setFootPosition(physx::PxExtendedVec3(footPosition.x + dx, footPosition.y + dy, footPosition.z + dz));
	controller->offset = offset;
}

RFAPI void Physics_CharacterControllerSetPosition(CharacterController* controller, const Vector3& position)
{
	int64_t now = Application_GetCurrentTime();

	controller->controller->setFootPosition(PxExtendedVec3(position.x, position.y, position.z));

	controllerSetPosition(position, controller);

	controller->lastMove = now;
}

RFAPI uint8_t Physics_MoveCharacterController(CharacterController* controller, const Vector3& delta, uint32_t filterMask)
{
	if (!(controller->controller->getActor()->getActorFlags() & PxActorFlag::eDISABLE_SIMULATION))
	{
		int64_t now = Application_GetCurrentTime();

		if (controller->lastMove == 0)
			controller->lastMove = now;

		float timeStep = (now - controller->lastMove) / 1e9f;

		PxFilterData filterData;
		filterData.word0 = filterMask;

		PxControllerFilters filters;
		filters.mFilterData = &filterData;

		PxControllerCollisionFlags collisionFlags = controller->controller->move(PxVec3(delta.x, delta.y, delta.z), 0.0f, timeStep, filters);
		PxExtendedVec3 newFootPosition = controller->controller->getFootPosition();

		controllerSetPosition(Vector3((float)newFootPosition.x - controller->offset.x, (float)newFootPosition.y - controller->offset.y, (float)newFootPosition.z - controller->offset.z), controller);

		controller->lastMove = now;

		return collisionFlags;
	}
	return 0;
}

RFAPI PxArticulationReducedCoordinate* Physics_CreateRagdoll()
{
	return physics->createArticulationReducedCoordinate();
}

RFAPI void Physics_SpawnRagdoll(PxArticulationReducedCoordinate* articulation)
{
	scene->addArticulation(*articulation);
}

RFAPI void Physics_DestroyRagdoll(PxArticulationReducedCoordinate* articulation)
{
	scene->removeArticulation(*articulation);
	articulation->release();
}

RFAPI RigidBody* Physics_RagdollCreateLink(PxArticulationReducedCoordinate* articulation, RigidBody* parentLink, PxArticulationJointType::Enum jointType, Matrix linkTransform, Vector3 jointPosition, Vector3 jointRotation, Vector3 velocity, Vector3 rotationVelocity)
{
	PxTransform transform = MatrixToPxTransform(linkTransform);

	PxArticulationLink* link = articulation->createLink(parentLink ? parentLink->actor->is<PxArticulationLink>() : nullptr, transform);
	//link->userData = articulation;
	//link->setActorFlag(PxActorFlag::eVISUALIZATION, true);

	//link->addForce(PxVec3(velocity.x, velocity.y, velocity.z), PxForceMode::eVELOCITY_CHANGE);
	//link->addTorque(PxVec3(rotationVelocity.x, rotationVelocity.y, rotationVelocity.z), PxForceMode::eVELOCITY_CHANGE);

	if (parentLink)
	{
		PxArticulationJointReducedCoordinate* joint = link->getInboundJoint();

		joint->setParentPose(joint->getParentArticulationLink().getGlobalPose().getInverse() * transform);
		//joint->setChildPose(link->getGlobalPose().getInverse() * transform); // will be identity anyways

		// Twist is actually rotation around the X axis, Swing1 is Y, Swing2 is Z

		joint->setJointPosition(PxArticulationAxis::eX, jointPosition.x);
		joint->setJointPosition(PxArticulationAxis::eY, jointPosition.y);
		joint->setJointPosition(PxArticulationAxis::eZ, jointPosition.z);
		joint->setJointPosition(PxArticulationAxis::eTWIST, jointRotation.x);
		joint->setJointPosition(PxArticulationAxis::eSWING1, jointRotation.y);
		joint->setJointPosition(PxArticulationAxis::eSWING2, jointRotation.z);

		joint->setJointVelocity(PxArticulationAxis::eX, velocity.x);
		joint->setJointVelocity(PxArticulationAxis::eY, velocity.y);
		joint->setJointVelocity(PxArticulationAxis::eZ, velocity.z);
		joint->setJointVelocity(PxArticulationAxis::eTWIST, rotationVelocity.x);
		joint->setJointVelocity(PxArticulationAxis::eSWING1, rotationVelocity.y);
		joint->setJointVelocity(PxArticulationAxis::eSWING2, rotationVelocity.z);

		joint->setJointType(jointType);

		//if (jointType == PxArticulationJointType::eREVOLUTE_UNWRAPPED)
		//{
		//	joint->setMotion(PxArticulationAxis::eTWIST, PxArticulationMotion::eLIMITED);
		//	//joint->setMotion(PxArticulationAxis::eSWING1, PxArticulationMotion::eLIMITED);
		//	//joint->setMotion(PxArticulationAxis::eSWING2, PxArticulationMotion::eLIMITED);
		//	joint->setLimitParams(PxArticulationAxis::eTWIST, PxArticulationLimit(0.0f, 0.9f * 3.1415f));
		//	//joint->setLimitParams(PxArticulationAxis::eSWING1, PxArticulationLimit(-0.2f * 3.1415f, 0.2f * 3.1415f));
		//	//joint->setLimitParams(PxArticulationAxis::eSWING2, PxArticulationLimit(-0.5f * 3.1415f, 0.5f * 3.1415f));
		//}
		//else if (jointType == PxArticulationJointType::eSPHERICAL)
		//{
		//	joint->setMotion(PxArticulationAxis::eTWIST, PxArticulationMotion::eLIMITED);
		//	joint->setMotion(PxArticulationAxis::eSWING1, PxArticulationMotion::eLIMITED);
		//	joint->setMotion(PxArticulationAxis::eSWING2, PxArticulationMotion::eLIMITED);
		//	joint->setLimitParams(PxArticulationAxis::eTWIST, PxArticulationLimit(-0.3f * 3.1415f, 0.3f * 3.1415f));
		//	joint->setLimitParams(PxArticulationAxis::eSWING1, PxArticulationLimit(-0.1f * 3.1415f, 0.1f * 3.1415f));
		//	joint->setLimitParams(PxArticulationAxis::eSWING2, PxArticulationLimit(-0.3f * 3.1415f, 0.3f * 3.1415f));
		//}

		//float friction = 0.05f;
		//joint->setFrictionCoefficient(friction);

		//float stiffness = 0.0f;
		//float damping = 0.05f;
		//joint->setDriveParams(PxArticulationAxis::eSWING1, PxArticulationDrive(stiffness, damping, 10));
		//joint->setDriveVelocity(PxArticulationAxis::eSWING1, 0.0f);
		//joint->setDriveParams(PxArticulationAxis::eSWING2, PxArticulationDrive(stiffness, damping, 10));
		//joint->setDriveVelocity(PxArticulationAxis::eSWING2, 0.0f);
		//joint->setDriveParams(PxArticulationAxis::eTWIST, PxArticulationDrive(stiffness, damping, 10));
		//joint->setDriveVelocity(PxArticulationAxis::eTWIST, 0.0f);
		//joint->setDriveTarget(PxArticulationAxis::eTWIST, 0.0f);
	}
	else
	{
		articulation->setRootLinearVelocity(PxVec3(velocity.x, velocity.y, velocity.z));
		articulation->setRootAngularVelocity(PxVec3(rotationVelocity.x, rotationVelocity.y, rotationVelocity.z));
	}

	RigidBody* body = BX_NEW(Application_GetAllocator(), RigidBody);
	body->type = RigidBodyType::Dynamic;
	body->density = 1.0f;
	body->centerOfMass = Vector3::Zero;
	body->actor = link;
	body->actor->userData = body;

	return body;
}

RFAPI void Physics_RagdollLinkAddBoxCollider(RigidBody* link, const Vector3& halfExtents, const Vector3& colliderPosition, const Quaternion& colliderRotation, uint32_t filterGroup, uint32_t filterMask)
{
	AddCollider(link->actor, PxBoxGeometry(PxVec3(halfExtents.x, halfExtents.y, halfExtents.z)), filterGroup, filterMask, colliderPosition, colliderRotation, 0.6f, 0.6f, 0.0f, true, 1.0f, Vector3::Zero);
}

RFAPI void Physics_RagdollLinkAddSphereCollider(RigidBody* link, float radius, const Vector3& colliderPosition, uint32_t filterGroup, uint32_t filterMask)
{
	AddCollider(link->actor, PxSphereGeometry(radius), filterGroup, filterMask, colliderPosition, Quaternion::Identity, 0.6f, 0.6f, 0.0f, true, 1.0f, Vector3::Zero);
}

RFAPI void Physics_RagdollLinkAddCapsuleCollider(RigidBody* link, float radius, float height, const Vector3& colliderPosition, const Quaternion& colliderRotation, uint32_t filterGroup, uint32_t filterMask)
{
	AddCollider(link->actor, PxCapsuleGeometry(radius, height * 0.5f - radius), filterGroup, filterMask, colliderPosition, colliderRotation, 0.6f, 0.6f, 0.0f, true, 1.0f, Vector3::Zero);
}

RFAPI void Physics_RagdollLinkGetGlobalTransform(RigidBody* body, Vector3* outPosition, Quaternion* outRotation)
{
	PxTransform transform = body->actor->getGlobalPose();
	*outPosition = Vector3(transform.p.x, transform.p.y, transform.p.z);
	*outRotation = Quaternion(transform.q.x, transform.q.y, transform.q.z, transform.q.w);
}

RFAPI void Physics_RagdollLinkSetSwingXLimit(RigidBody* body, float min, float max)
{
	PxArticulationJointReducedCoordinate* joint = ((PxArticulationLink*)body->actor)->getInboundJoint();
	joint->setMotion(PxArticulationAxis::eTWIST, PxArticulationMotion::eLIMITED);
	joint->setLimitParams(PxArticulationAxis::eTWIST, PxArticulationLimit(min, max));
}

RFAPI void Physics_RagdollLinkSetSwingZLimit(RigidBody* body, float min, float max)
{
	PxArticulationJointReducedCoordinate* joint = ((PxArticulationLink*)body->actor)->getInboundJoint();
	joint->setMotion(PxArticulationAxis::eSWING2, PxArticulationMotion::eLIMITED);
	joint->setLimitParams(PxArticulationAxis::eSWING2, PxArticulationLimit(min, max));
}

RFAPI void Physics_RagdollLinkSetTwistLimit(RigidBody* body, float min, float max)
{
	PxArticulationJointReducedCoordinate* joint = ((PxArticulationLink*)body->actor)->getInboundJoint();
	joint->setMotion(PxArticulationAxis::eSWING1, PxArticulationMotion::eLIMITED);
	joint->setLimitParams(PxArticulationAxis::eSWING1, PxArticulationLimit(min, max));
}

RFAPI int Physics_Raycast(const Vector3& origin, const Vector3& direction, float maxDistance, HitData* hits, int maxHits, PxQueryFlag::Enum flags, uint32_t filterMask)
{
	if (direction.lengthSquared() == 0.0f)
		return 0;

	PxHitFlags hitFlags = PxHitFlags(PxHitFlag::eDEFAULT);
	PxQueryFilterData queryFilterData = PxQueryFilterData(flags);
	queryFilterData.data.word0 = filterMask;

	static PxRaycastHit hitData[256];
	PxRaycastBuffer hitBuffer(hitData, maxHits);

	if (scene->raycast(PxVec3(origin.x, origin.y, origin.z), PxVec3(direction.x, direction.y, direction.z), maxDistance, hitBuffer, hitFlags, queryFilterData))
	{
		for (uint32_t i = 0; i < hitBuffer.getNbAnyHits(); i++)
		{
			const PxRaycastHit* hit = &hitBuffer.getAnyHit(i);

			hits[i].distance = hit->distance;
			hits[i].position = Vector3(hit->position.x, hit->position.y, hit->position.z);
			hits[i].normal = Vector3(hit->normal.x, hit->normal.y, hit->normal.z);
			hits[i].isTrigger = hit->shape->getFlags() & PxShapeFlag::eTRIGGER_SHAPE ? 1 : 0;
			hits[i].userData = hit->actor->userData;
		}
	}

	return (int)hitBuffer.getNbTouches();
}

RFAPI bool Physics_RaycastCheck(const Vector3& origin, const Vector3& direction, float maxDistance, HitData* hit, PxQueryFlag::Enum flags, uint32_t filterMask)
{
	if (direction.lengthSquared() == 0.0f)
		return 0;

	PxHitFlags hitFlags = PxHitFlags(PxHitFlag::eDEFAULT);
	PxQueryFilterData queryFilterData = PxQueryFilterData(flags);
	queryFilterData.data.word0 = filterMask;

	PxRaycastBuffer hitBuffer;

	if (scene->raycast(PxVec3(origin.x, origin.y, origin.z), PxVec3(direction.x, direction.y, direction.z), maxDistance, hitBuffer, hitFlags, queryFilterData))
	{
		hit->distance = hitBuffer.block.distance;
		hit->position = Vector3(hitBuffer.block.position.x, hitBuffer.block.position.y, hitBuffer.block.position.z);
		hit->normal = Vector3(hitBuffer.block.normal.x, hitBuffer.block.normal.y, hitBuffer.block.normal.z);
		hit->isTrigger = hitBuffer.block.shape->getFlags() & PxShapeFlag::eTRIGGER_SHAPE ? 1 : 0;
		hit->userData = hitBuffer.block.actor->userData;

		return true;
	}

	return false;
}

static int Sweep(const PxGeometry& geometry, const Vector3& position, const Quaternion& rotation, const Vector3& direction, float maxDistance, HitData* hits, int maxHits, PxQueryFlag::Enum flags, uint32_t filterMask)
{
	if (direction.lengthSquared() == 0.0f)
		return 0;

	PxHitFlags hitFlags = PxHitFlags(PxHitFlag::eDEFAULT | PxHitFlag::eMTD);
	PxQueryFilterData queryFilterData = PxQueryFilterData(flags);
	queryFilterData.data.word0 = filterMask;

	static PxSweepHit hitData[256];
	PxSweepBuffer hitBuffer(hitData, maxHits);

	PxTransform transform(PxVec3(position.x, position.y, position.z), PxQuat(rotation.x, rotation.y, rotation.z, rotation.w));
	if (geometry.getType() == PxGeometryType::eCAPSULE)
		transform.q = transform.q * PxQuat(PxHalfPi, PxVec3(0, 0, 1));

	if (scene->sweep(geometry, transform, PxVec3(direction.x, direction.y, direction.z), maxDistance, hitBuffer, hitFlags, queryFilterData))
	{
		for (uint32_t i = 0; i < hitBuffer.getNbAnyHits(); i++)
		{
			const PxSweepHit* hit = &hitBuffer.getAnyHit(i);

			hits[i].distance = hit->distance;
			hits[i].position = Vector3(hit->position.x, hit->position.y, hit->position.z);
			hits[i].normal = Vector3(hit->normal.x, hit->normal.y, hit->normal.z);
			hits[i].isTrigger = hit->shape->getFlags() & PxShapeFlag::eTRIGGER_SHAPE ? 1 : 0;
			hits[i].userData = hit->actor->userData;
		}
	}

	return hitBuffer.getNbAnyHits();
}

RFAPI int Physics_SweepBox(const Vector3& halfExtents, const Vector3& position, const Quaternion& rotation, const Vector3& direction, float maxDistance, HitData* hits, int maxHits, PxQueryFlag::Enum flags, uint32_t filterMask)
{
	return Sweep(PxBoxGeometry(halfExtents.x, halfExtents.y, halfExtents.z), position, rotation, direction, maxDistance, hits, maxHits, flags, filterMask);
}

RFAPI int Physics_SweepSphere(float radius, const Vector3& position, const Quaternion& rotation, const Vector3& direction, float maxDistance, HitData* hits, int maxHits, PxQueryFlag::Enum flags, uint32_t filterMask)
{
	return Sweep(PxSphereGeometry(radius), position, rotation, direction, maxDistance, hits, maxHits, flags, filterMask);
}

RFAPI int Physics_SweepCapsule(float radius, float height, const Vector3& position, const Quaternion& rotation, const Vector3& direction, float maxDistance, HitData* hits, int maxHits, PxQueryFlag::Enum flags, uint32_t filterMask)
{
	return Sweep(PxCapsuleGeometry(radius, 0.5f * height), position, rotation, direction, maxDistance, hits, maxHits, flags, filterMask);
}

static int Overlap(const PxGeometry& geometry, PxTransform transform, HitData* hits, int maxHits, PxQueryFlag::Enum flags, uint32_t filterMask)
{
	PxQueryFilterData queryFilterData = PxQueryFilterData(flags);
	queryFilterData.data.word0 = filterMask;

	static PxOverlapHit hitData[256];
	PxOverlapBuffer hitBuffer(hitData, maxHits);

	if (geometry.getType() == PxGeometryType::eCAPSULE)
		transform.q = transform.q * PxQuat(PxHalfPi, PxVec3(0, 0, 1));

	if (scene->overlap(geometry, transform, hitBuffer, queryFilterData))
	{
		for (uint32_t i = 0; i < hitBuffer.getNbTouches(); i++)
		{
			const PxOverlapHit* hit = &hitBuffer.getTouches()[i];

			hits[i].isTrigger = hit->shape->getFlags() & PxShapeFlag::eTRIGGER_SHAPE ? 1 : 0;
			hits[i].userData = hit->actor->userData;
		}
	}

	return hitBuffer.getNbTouches();
}

RFAPI int Physics_OverlapBox(const Vector3& halfExtents, const Vector3& position, const Quaternion& rotation, HitData* hits, int maxHits, PxQueryFlag::Enum flags, uint32_t filterMask)
{
	return Overlap(PxBoxGeometry(halfExtents.x, halfExtents.y, halfExtents.z), PxTransform(PxVec3(position.x, position.y, position.z), PxQuat(rotation.x, rotation.y, rotation.z, rotation.w)), hits, maxHits, flags, filterMask);
}

RFAPI int Physics_OverlapSphere(float radius, const Vector3& position, HitData* hits, int maxHits, PxQueryFlag::Enum flags, uint32_t filterMask)
{
	return Overlap(PxSphereGeometry(radius), PxTransform(PxVec3(position.x, position.y, position.z)), hits, maxHits, flags, filterMask);
}

RFAPI int Physics_OverlapCapsule(float radius, float height, const Vector3& position, const Quaternion& rotation, HitData* hits, int maxHits, PxQueryFlag::Enum flags, uint32_t filterMask)
{
	return Overlap(PxCapsuleGeometry(radius, 0.5f * height - radius), PxTransform(PxVec3(position.x, position.y, position.z), PxQuat(rotation.x, rotation.y, rotation.z, rotation.w)), hits, maxHits, flags, filterMask);
}

#endif