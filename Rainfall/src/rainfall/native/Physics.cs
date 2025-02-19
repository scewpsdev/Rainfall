using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using static Rainfall.Native.Physics;
using System.Runtime.CompilerServices;

namespace Rainfall.Native
{
	internal enum JointType
	{
		Fix = 0,                   //!< All joint axes, i.e. degrees of freedom (DOFs) locked
		Prismatic = 1,             //!< Single linear DOF, e.g. cart on a rail
		Revolute = 2,              //!< Single rotational DOF, e.g. an elbow joint or a rotational motor, position wrapped at 2pi radians
		RevoluteUnwrapped = 3,    //!< Single rotational DOF, e.g. an elbow joint or a rotational motor, position not wrapped
		Spherical = 4,             //!< Ball and socket joint with two or three DOFs
		Undefined = 5
	}

	internal static class Physics
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void RigidBodySetTransformCallback_t(Vector3 position, Quaternion rotation, IntPtr userPtr);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void RigidBodyGetTransformCallback_t(ref Vector3 position, ref Quaternion rotation, IntPtr userPtr);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void RigidBodyContactCallback_t(IntPtr body, IntPtr other, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType, IntPtr otherController);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void CharacterControllerOnHitCallback_t(ControllerHit hit, IntPtr userPtr);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void CharacterControllerSetPositionCallback_t(Vector3 position, IntPtr userPtr);


		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_Init(RigidBodySetTransformCallback_t setTransform, RigidBodyGetTransformCallback_t getTransform, RigidBodyContactCallback_t contactCallback, CharacterControllerSetPositionCallback_t setPosition, CharacterControllerOnHitCallback_t onHit);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_Shutdown();

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_Update();

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern long Physics_GetSimulationDelta();

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr Physics_CreateRigidBody(RigidBodyType type, float density, Vector3 centerOfMass, Vector3 position, Quaternion rotation);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_DestroyRigidBody(IntPtr body);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe IntPtr Physics_CreateMeshCollider(void* vertices, int numVertices, int vertexStride, int* indices, int numIndices);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_DestroyMeshCollider(IntPtr mesh);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe IntPtr Physics_CreateConvexMeshCollider(void* vertices, int numVertices, int vertexStride, int* indices, int numIndices);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_DestroyConvexMeshCollider(IntPtr mesh);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr Physics_CreateHeightField(int width, int height, ref HeightFieldSample data);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_DestroyHeightField(IntPtr heightField);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddSphereCollider(IntPtr body, float radius, Vector3 position, uint filterGroup, uint filterMask, float staticFriction, float dynamicFriction, float restitution);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddBoxCollider(IntPtr body, Vector3 halfExtents, Vector3 position, Quaternion rotation, uint filterGroup, uint filterMask, float staticFriction, float dynamicFriction, float restitution);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddCapsuleCollider(IntPtr body, float radius, float height, Vector3 position, Quaternion rotation, uint filterGroup, uint filterMask, float staticFriction, float dynamicFriction, float restitution);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddMeshCollider(IntPtr body, IntPtr mesh, Matrix transform, uint filterGroup, uint filterMask, float staticFriction, float dynamicFriction, float restitution);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe void Physics_RigidBodyAddConvexMeshCollider(IntPtr body, IntPtr mesh, Matrix transform, uint filterGroup, uint filterMask, float staticFriction, float dynamicFriction, float restitution);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddHeightFieldCollider(IntPtr body, IntPtr heightField, Vector3 scale, Matrix transform, uint filterGroup, uint filterMask, float staticFriction, float dynamicFriction, float restitution);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddSphereTrigger(IntPtr body, float radius, Vector3 position, uint filterGroup, uint filterMask);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddBoxTrigger(IntPtr body, Vector3 halfExtents, Vector3 position, Quaternion rotation, uint filterGroup, uint filterMask);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddCapsuleTrigger(IntPtr body, float radius, float height, Vector3 position, Quaternion rotation, uint filterGroup, uint filterMask);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddMeshTrigger(IntPtr body, IntPtr mesh, Matrix transform, uint filterGroup, uint filterMask);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddConvexMeshTrigger(IntPtr body, IntPtr mesh, Matrix transform, uint filterGroup, uint filterMask);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyClearColliders(IntPtr body);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodySetSimulationEnabled(IntPtr body, byte enabled);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodySetGravityEnabled(IntPtr body, byte enabled);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodySetTransform(IntPtr body, Vector3 position, Quaternion rotation);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodySetRotation(IntPtr body, Quaternion rotation);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodySetVelocity(IntPtr body, Vector3 velocity);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodySetRotationVelocity(IntPtr body, Vector3 rotvelocity);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodySetCenterOfMass(IntPtr body, Vector3 centerOfMass);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyLockAxis(IntPtr body, byte x, byte y, byte z);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyLockRotationAxis(IntPtr body, byte x, byte y, byte z);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddForce(IntPtr body, Vector3 force);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddTorque(IntPtr body, Vector3 torque);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddAcceleration(IntPtr body, Vector3 acceleration);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddImpulse(IntPtr body, Vector3 impulse);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyGetTransform(IntPtr body, out Vector3 position, out Quaternion rotation);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyGetVelocity(IntPtr body, out Vector3 position);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyGetAngularVelocity(IntPtr body, out Vector3 angularVelocity);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyGetCenterOfMass(IntPtr body, out Vector3 centerOfMass);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr Physics_CreateCharacterController(float radius, float height, Vector3 offset, float stepOffset, Vector3 position);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_DestroyCharacterController(IntPtr controller);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_ResizeCharacterController(IntPtr controller, float height);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_CharacterControllerSetRadius(IntPtr controller, float radius);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_CharacterControllerSetOffset(IntPtr controller, Vector3 offset);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_CharacterControllerSetHeight(IntPtr controller, float height);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_CharacterControllerSetPosition(IntPtr controller, Vector3 position);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern ControllerCollisionFlag Physics_MoveCharacterController(IntPtr controller, Vector3 delta, uint filterMask);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr Physics_CreateRagdoll();

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_SpawnRagdoll(IntPtr ragdoll);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_DestroyRagdoll(IntPtr ragdoll);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr Physics_RagdollCreateLink(IntPtr ragdoll, IntPtr parentLink, JointType jointType, Matrix linkTransform, Vector3 jointPosition, Vector3 jointRotation, Vector3 velocity, Vector3 rotationVelocity);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RagdollLinkAddBoxCollider(IntPtr link, Vector3 halfExtents, Vector3 position, Quaternion rotation, uint filterGroup, uint filterMask);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RagdollLinkAddSphereCollider(IntPtr link, float radius, Vector3 colliderPosition, uint filterGroup, uint filterMask);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RagdollLinkAddCapsuleCollider(IntPtr link, float radius, float height, Vector3 colliderPosition, Quaternion colliderRotation, uint filterGroup, uint filterMask);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RagdollLinkGetGlobalTransform(IntPtr link, out Vector3 position, out Quaternion rotation);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RagdollLinkSetSwingXLimit(IntPtr link, float min, float max);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RagdollLinkSetSwingZLimit(IntPtr link, float min, float max);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RagdollLinkSetTwistLimit(IntPtr link, float min, float max);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe int Physics_Raycast(Vector3 origin, Vector3 direction, float maxDistance, HitData* hits, int maxHits, QueryFilterFlags filterData, uint filterMask);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe byte Physics_RaycastCheck(Vector3 origin, Vector3 direction, float maxDistance, out HitData hit, QueryFilterFlags filterData, uint filterMask);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe int Physics_SweepBox(Vector3 halfExtents, Vector3 position, Quaternion rotation, Vector3 direction, float maxDistance, HitData* hits, int maxHits, QueryFilterFlags filterData, uint filterMask);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe int Physics_SweepSphere(float radius, Vector3 position, Quaternion rotation, Vector3 direction, float maxDistance, HitData* hits, int maxHits, QueryFilterFlags filterData, uint filterMask);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe int Physics_SweepCapsule(float radius, float height, Vector3 position, Quaternion rotation, Vector3 direction, float maxDistance, HitData* hits, int maxHits, QueryFilterFlags filterData, uint filterMask);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe int Physics_OverlapBox(Vector3 halfExtents, Vector3 position, Quaternion rotation, HitData* hits, int maxHits, QueryFilterFlags filterData, uint filterMask);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe int Physics_OverlapSphere(float radius, Vector3 position, HitData* hits, int maxHits, QueryFilterFlags filterData, uint filterMask);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe int Physics_OverlapCapsule(float radius, float height, Vector3 position, Quaternion rotation, HitData* hits, int maxHits, QueryFilterFlags filterData, uint filterMask);
	}
}
