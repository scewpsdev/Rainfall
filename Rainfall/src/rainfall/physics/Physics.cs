using Rainfall;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Rainfall
{
	[StructLayout(LayoutKind.Sequential)]
	public struct HeightFieldSample
	{
		public short height;
		byte materialIndex0;
		byte materialIndex1;

		public bool tesselationBit
		{
			get { return (materialIndex0 & 0b10000000) != 0; }
			set { materialIndex0 |= (byte)(value ? 0b10000000 : 0); }
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct HitData
	{
		public float distance;
		public Vector3 position;
		public Vector3 normal;
		[MarshalAs(UnmanagedType.Bool)] public bool isTrigger;

		public IntPtr userData;


		public RigidBody body
		{
			get => RigidBody.GetBodyFromHandle((RigidBodyData*)userData);
		}

		public CharacterController controller
		{
			get => CharacterController.GetControllerFromHandle(userData);
		}

		public Ragdoll ragdoll
		{
			get => Ragdoll.GetRagdollFromHandle(userData);
		}
	}

	public enum QueryFilterFlags : int
	{
		Static = 1 << 0,  //!< Traverse static shapes
		Dynamic = 1 << 1, //!< Traverse dynamic shapes
		AnyHit = 1 << 4,  //!< Abort traversal as soon as any hit is found and return it via callback.block.
		NoBlock = 1 << 5, //!< All hits are reported as touching. Overrides eBLOCK returned from user filters with eTOUCH.

		Default = Static | Dynamic,
	}

	internal enum JointType
	{
		Fix = 0,                   //!< All joint axes, i.e. degrees of freedom (DOFs) locked
		Prismatic = 1,             //!< Single linear DOF, e.g. cart on a rail
		Revolute = 2,              //!< Single rotational DOF, e.g. an elbow joint or a rotational motor, position wrapped at 2pi radians
		RevoluteUnwrapped = 3,    //!< Single rotational DOF, e.g. an elbow joint or a rotational motor, position not wrapped
		Spherical = 4,             //!< Ball and socket joint with two or three DOFs
		Undefined = 5
	}

	public static unsafe class Physics
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void RigidBodySetTransformCallback_t(Vector3 position, Quaternion rotation, IntPtr userPtr);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void RigidBodyGetTransformCallback_t(ref Vector3 position, ref Quaternion rotation, IntPtr userPtr);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void RigidBodyContactCallback_t(RigidBodyData* body, RigidBodyData* other, int shapeID, int otherShapeID, byte isTrigger, byte otherTrigger, ContactType contactType, IntPtr otherController);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void CharacterControllerOnHitCallback_t(ControllerHit hit, IntPtr userPtr);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void CharacterControllerSetPositionCallback_t(Vector3 position, IntPtr userPtr);

		static readonly RigidBodySetTransformCallback_t rigidBodySetTransformCallback = RigidBodySetTransform;
		static readonly RigidBodyGetTransformCallback_t rigidBodyGetTransformCallback = RigidBodyGetTransform;
		static readonly RigidBodyContactCallback_t rigidBodyContactCallback = RigidBodyContactCallback;

		static readonly CharacterControllerSetPositionCallback_t characterControllerSetPositionCallback = SetCharacterControllerPosition;
		static readonly CharacterControllerOnHitCallback_t characterControllerOnHitCallback = CharacterControllerOnHit;


		public static void Init()
		{
			Physics_Init(rigidBodySetTransformCallback, rigidBodyGetTransformCallback, rigidBodyContactCallback, characterControllerSetPositionCallback, characterControllerOnHitCallback);
		}

		public static void Shutdown()
		{
			Physics_Shutdown();
		}

		public static void Update()
		{
			Physics_Update(Time.deltaTime);
		}

		public static float SimulationDelta
		{
			get => Physics_GetSimulationDelta() / 1e9f;
		}

		static unsafe Matrix GetNodeTransform(Model model, int idx)
		{
			MeshData mesh = model.scene->meshes[idx];
			NodeData* node = mesh.node;
			Matrix transform = node->transform;
			NodeData* parent = node->parent;
			while (parent != null)
			{
				transform = parent->transform * transform;
				parent = parent->parent;
			}
			return transform;
		}

		public static unsafe MeshCollider CreateMeshCollider(Model model, int meshIdx)
		{
			MeshData mesh = model.scene->meshes[meshIdx];
			IntPtr handle = Physics_CreateMeshCollider(mesh.vertices, mesh.vertexCount, sizeof(PositionNormalTangent), mesh.indices, mesh.indexCount);
			Matrix transform = GetNodeTransform(model, meshIdx);
			return new MeshCollider(handle, transform);
		}

		public static unsafe MeshCollider CreateMeshCollider(Model model)
		{
			int numVertices = 0;
			int numIndices = 0;
			for (int i = 0; i < model.scene->numMeshes; i++)
			{
				numVertices += model.scene->meshes[i].vertexCount;
				numIndices += model.scene->meshes[i].indexCount;
			}
			Span<Vector3> vertices = stackalloc Vector3[numVertices];
			Span<int> indices = stackalloc int[numIndices];

			int currentVertex = 0;
			int currentIndex = 0;
			for (int i = 0; i < model.scene->numMeshes; i++)
			{
				Matrix transform = GetNodeTransform(model, i);
				int startIndex = currentVertex;
				for (int j = 0; j < model.scene->meshes[i].vertexCount; j++)
				{
					PositionNormalTangent* vertex = &model.scene->meshes[i].vertices[j];
					vertices[currentVertex++] = transform * vertex->position;
				}
				for (int j = 0; j < model.scene->meshes[i].indexCount; j++)
				{
					int index = startIndex + model.scene->meshes[i].indices[j];
					indices[currentIndex++] = index;
				}
			}

			fixed (Vector3* verticesPtr = vertices)
			fixed (int* indicesPtr = indices)
			{
				IntPtr handle = Physics_CreateMeshCollider(verticesPtr, numVertices, sizeof(Vector3), indicesPtr, numIndices);
				return new MeshCollider(handle, Matrix.Identity);
			}
		}

		public static unsafe ConvexMeshCollider CreateConvexMeshCollider(Model model, int meshIdx)
		{
			MeshData mesh = model.scene->meshes[meshIdx];
			IntPtr handle = Physics_CreateConvexMeshCollider(mesh.vertices, mesh.vertexCount, sizeof(PositionNormalTangent), mesh.indices, mesh.indexCount);
			Matrix transform = GetNodeTransform(model, meshIdx);
			return new ConvexMeshCollider(handle, transform);
		}

		public static unsafe ConvexMeshCollider CreateConvexMeshCollider(Model model)
		{
			int numVertices = 0;
			int numIndices = 0;
			for (int i = 0; i < model.scene->numMeshes; i++)
			{
				numVertices += model.scene->meshes[i].vertexCount;
				numIndices += model.scene->meshes[i].indexCount;
			}
			Span<Vector3> vertices = stackalloc Vector3[numVertices];
			Span<int> indices = stackalloc int[numIndices];

			int currentVertex = 0;
			int currentIndex = 0;
			for (int i = 0; i < model.scene->numMeshes; i++)
			{
				Matrix transform = GetNodeTransform(model, i);
				int startIndex = currentVertex;
				for (int j = 0; j < model.scene->meshes[i].vertexCount; j++)
				{
					PositionNormalTangent* vertex = &model.scene->meshes[i].vertices[j];
					vertices[currentVertex++] = transform * vertex->position;
				}
				for (int j = 0; j < model.scene->meshes[i].indexCount; j++)
				{
					int index = startIndex + model.scene->meshes[i].indices[j];
					indices[currentIndex++] = index;
				}
			}

			fixed (Vector3* verticesPtr = vertices)
			fixed (int* indicesPtr = indices)
			{
				IntPtr handle = Physics_CreateConvexMeshCollider(verticesPtr, numVertices, sizeof(Vector3), indicesPtr, numIndices);
				return new ConvexMeshCollider(handle, Matrix.Identity);
			}
		}

		public static IntPtr CreateHeightField(int width, int height, HeightFieldSample[] data)
		{
			return Physics_CreateHeightField(width, height, ref data[0]);
		}

		public static void DestroyHeightField(IntPtr heightField)
		{
			Physics_DestroyHeightField(heightField);
		}

		public static int Raycast(Vector3 origin, Vector3 direction, float distance, Span<HitData> hits, QueryFilterFlags filterData = QueryFilterFlags.Default, uint filterMask = 1)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Physics_Raycast(origin, direction, distance, data, hits.Length, filterData, filterMask);
			}
		}

		public static int Raycast(Vector3 origin, Vector3 direction, float distance, HitData[] hits, QueryFilterFlags filterData = QueryFilterFlags.Default, uint filterMask = 1)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Physics_Raycast(origin, direction, distance, data, hits.Length, filterData, filterMask);
			}
		}

		public static HitData? Raycast(Vector3 origin, Vector3 direction, float distance, QueryFilterFlags filterData = QueryFilterFlags.Default, uint filterMask = 1)
		{
			if (Physics_RaycastCheck(origin, direction, distance, out HitData hit, filterData, filterMask) != 0)
				return hit;
			return null;
		}

		public static int SweepBox(Vector3 halfExtents, Vector3 position, Quaternion rotation, Vector3 direction, float distance, Span<HitData> hits, QueryFilterFlags filterData = QueryFilterFlags.Default, uint filterMask = 1)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Physics_SweepBox(halfExtents, position, rotation, direction, distance, data, hits.Length, filterData, filterMask);
			}
		}

		public static int SweepBox(Vector3 halfExtents, Vector3 position, Quaternion rotation, Vector3 direction, float distance, HitData[] hits, QueryFilterFlags filterData = QueryFilterFlags.Default, uint filterMask = 1)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Physics_SweepBox(halfExtents, position, rotation, direction, distance, data, hits.Length, filterData, filterMask);
			}
		}

		public static int SweepSphere(float radius, Vector3 position, Vector3 direction, float distance, Span<HitData> hits, QueryFilterFlags filterData = QueryFilterFlags.Default, uint filterMask = 1)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Physics_SweepSphere(radius, position, Quaternion.Identity, direction, distance, data, hits.Length, filterData, filterMask);
			}
		}

		public static int SweepSphere(float radius, Vector3 position, Vector3 direction, float distance, HitData[] hits, QueryFilterFlags filterData = QueryFilterFlags.Default, uint filterMask = 1)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Physics_SweepSphere(radius, position, Quaternion.Identity, direction, distance, data, hits.Length, filterData, filterMask);
			}
		}

		public static HitData? SweepSphere(float radius, Vector3 position, Vector3 direction, float distance, QueryFilterFlags filterData = QueryFilterFlags.Default, uint filterMask = 1)
		{
			Span<HitData> hits = stackalloc HitData[16];
			int numHits = SweepSphere(radius, position, direction, distance, hits, filterData | QueryFilterFlags.AnyHit, filterMask);

			if (numHits > 0)
				return hits[0];

			return null;
		}

		public static int SweepCapsule(float radius, float height, Vector3 position, Quaternion rotation, Vector3 direction, float distance, Span<HitData> hits, QueryFilterFlags filterData = QueryFilterFlags.Default, uint filterMask = 1)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Physics_SweepCapsule(radius, height, position, rotation, direction, distance, data, hits.Length, filterData, filterMask);
			}
		}

		public static int SweepCapsule(float radius, float height, Vector3 position, Quaternion rotation, Vector3 direction, float distance, HitData[] hits, QueryFilterFlags filterData = QueryFilterFlags.Default, uint filterMask = 1)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Physics_SweepCapsule(radius, height, position, rotation, direction, distance, data, hits.Length, filterData, filterMask);
			}
		}

		public static int OverlapBox(Vector3 halfExtents, Vector3 position, Quaternion rotation, Span<HitData> hits, QueryFilterFlags filterData = QueryFilterFlags.Default, uint filterMask = 1)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Physics_OverlapBox(halfExtents, position, rotation, data, hits.Length, filterData, filterMask);
			}
		}

		public static int OverlapBox(Vector3 halfExtents, Vector3 position, Quaternion rotation, HitData[] hits, QueryFilterFlags filterData = QueryFilterFlags.Default, uint filterMask = 1)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Physics_OverlapBox(halfExtents, position, rotation, data, hits.Length, filterData, filterMask);
			}
		}

		public static int OverlapSphere(float radius, Vector3 position, Span<HitData> hits, QueryFilterFlags filterData = QueryFilterFlags.Default, uint filterMask = 1)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Physics_OverlapSphere(radius, position, data, hits.Length, filterData, filterMask);
			}
		}

		public static int OverlapSphere(float radius, Vector3 position, HitData[] hits, QueryFilterFlags filterData = QueryFilterFlags.Default, uint filterMask = 1)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Physics_OverlapSphere(radius, position, data, hits.Length, filterData, filterMask);
			}
		}

		public static int OverlapCapsule(float radius, float height, Vector3 position, Quaternion rotation, Span<HitData> hits, QueryFilterFlags filterData = QueryFilterFlags.Default, uint filterMask = 1)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Physics_OverlapCapsule(radius, height, position, rotation, data, hits.Length, filterData, filterMask);
			}
		}

		public static int OverlapCapsule(float radius, float height, Vector3 position, Quaternion rotation, HitData[] hits, QueryFilterFlags filterData = QueryFilterFlags.Default, uint filterMask = 1)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Physics_OverlapCapsule(radius, height, position, rotation, data, hits.Length, filterData, filterMask);
			}
		}


		static void RigidBodySetTransform(Vector3 position, Quaternion rotation, IntPtr userPtr)
		{
			RigidBody body = RigidBody.GetBodyFromHandle((RigidBodyData*)userPtr);
			body.entity.setPosition(position);
			body.entity.setRotation(rotation);
		}

		static void RigidBodyGetTransform(ref Vector3 position, ref Quaternion rotation, IntPtr userPtr)
		{
			RigidBody body = RigidBody.GetBodyFromHandle((RigidBodyData*)userPtr);
			position = body.entity.getPosition();
			rotation = body.entity.getRotation();
		}

		static void RigidBodyContactCallback(RigidBodyData* bodyHandle, RigidBodyData* otherHandle, int shapeID, int otherShapeID, byte isTrigger, byte otherTrigger, ContactType contactType, IntPtr otherControllerHandle)
		{
			RigidBody body = RigidBody.GetBodyFromHandle(bodyHandle);
			RigidBody other = otherHandle != null ? RigidBody.GetBodyFromHandle(otherHandle) : null;
			CharacterController otherController = otherControllerHandle != IntPtr.Zero ? CharacterController.GetControllerFromHandle(otherControllerHandle) : null;

			if (body != null)
			{
				body.entity?.onContact(other, otherController, shapeID, otherShapeID, isTrigger != 0, otherTrigger != 0, contactType);

				if (other != null)
				{
					Debug.Assert(otherController == null);
					other.entity?.onContact(body, null, otherShapeID, shapeID, otherTrigger != 0, isTrigger != 0, contactType);
				}
				if (otherController != null)
				{
					Debug.Assert(other == null);
					otherController.entity?.onContact(null, otherController, otherShapeID, shapeID, otherTrigger != 0, isTrigger != 0, contactType);
				}
			}
		}

		static void SetCharacterControllerPosition(Vector3 position, IntPtr userPtr)
		{
			CharacterController controller = CharacterController.GetControllerFromHandle(userPtr);
			controller.entity.setPosition(position);
		}

		static void CharacterControllerOnHit(ControllerHit hit, IntPtr userPtr)
		{
			CharacterController controller = CharacterController.GetControllerFromHandle(userPtr);
			if (controller.hitCallback != null)
				controller.hitCallback.onShapeHit(hit);
		}


		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_Init(RigidBodySetTransformCallback_t setTransform, RigidBodyGetTransformCallback_t getTransform, RigidBodyContactCallback_t contactCallback, CharacterControllerSetPositionCallback_t setPosition, CharacterControllerOnHitCallback_t onHit);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_Shutdown();

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_Update(float delta);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern long Physics_GetSimulationDelta();

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe RigidBodyData* Physics_CreateRigidBody(RigidBodyType type, float density, Vector3 centerOfMass, Vector3 position, Quaternion rotation);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_DestroyRigidBody(RigidBodyData* body);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe IntPtr Physics_CreateMeshCollider(void* vertices, int numVertices, int vertexStride, int* indices, int numIndices);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_DestroyMeshCollider(IntPtr mesh);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe IntPtr Physics_CreateConvexMeshCollider(void* vertices, int numVertices, int vertexStride, int* indices, int numIndices);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_DestroyConvexMeshCollider(IntPtr mesh);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr Physics_CreateHeightField(int width, int height, ref HeightFieldSample data);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_DestroyHeightField(IntPtr heightField);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddSphereCollider(RigidBodyData* body, float radius, Vector3 position, uint filterGroup, uint filterMask, float staticFriction, float dynamicFriction, float restitution);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddBoxCollider(RigidBodyData* body, Vector3 halfExtents, Vector3 position, Quaternion rotation, uint filterGroup, uint filterMask, float staticFriction, float dynamicFriction, float restitution);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddCapsuleCollider(RigidBodyData* body, float radius, float height, Vector3 position, Quaternion rotation, uint filterGroup, uint filterMask, float staticFriction, float dynamicFriction, float restitution);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddMeshCollider(RigidBodyData* body, IntPtr mesh, Matrix transform, uint filterGroup, uint filterMask, float staticFriction, float dynamicFriction, float restitution);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe void Physics_RigidBodyAddConvexMeshCollider(RigidBodyData* body, IntPtr mesh, Matrix transform, uint filterGroup, uint filterMask, float staticFriction, float dynamicFriction, float restitution);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddHeightFieldCollider(RigidBodyData* body, IntPtr heightField, Vector3 scale, Matrix transform, uint filterGroup, uint filterMask, float staticFriction, float dynamicFriction, float restitution);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddSphereTrigger(RigidBodyData* body, float radius, Vector3 position, uint filterGroup, uint filterMask);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddBoxTrigger(RigidBodyData* body, Vector3 halfExtents, Vector3 position, Quaternion rotation, uint filterGroup, uint filterMask);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddCapsuleTrigger(RigidBodyData* body, float radius, float height, Vector3 position, Quaternion rotation, uint filterGroup, uint filterMask);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddMeshTrigger(RigidBodyData* body, IntPtr mesh, Matrix transform, uint filterGroup, uint filterMask);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddConvexMeshTrigger(RigidBodyData* body, IntPtr mesh, Matrix transform, uint filterGroup, uint filterMask);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyClearColliders(RigidBodyData* body);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodySetSimulationEnabled(RigidBodyData* body, byte enabled);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodySetGravityEnabled(RigidBodyData* body, byte enabled);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodySetTransform(RigidBodyData* body, Vector3 position, Quaternion rotation);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodySetRotation(RigidBodyData* body, Quaternion rotation);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodySetVelocity(RigidBodyData* body, Vector3 velocity);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodySetRotationVelocity(RigidBodyData* body, Vector3 rotvelocity);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodySetCenterOfMass(RigidBodyData* body, Vector3 centerOfMass);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyLockAxis(RigidBodyData* body, byte x, byte y, byte z);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyLockRotationAxis(RigidBodyData* body, byte x, byte y, byte z);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddForce(RigidBodyData* body, Vector3 force);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddTorque(RigidBodyData* body, Vector3 torque);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddAcceleration(RigidBodyData* body, Vector3 acceleration);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyAddImpulse(RigidBodyData* body, Vector3 impulse);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyGetTransform(RigidBodyData* body, out Vector3 position, out Quaternion rotation);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyGetVelocity(RigidBodyData* body, out Vector3 position);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyGetAngularVelocity(RigidBodyData* body, out Vector3 angularVelocity);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RigidBodyGetCenterOfMass(RigidBodyData* body, out Vector3 centerOfMass);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr Physics_CreateCharacterController(float radius, float height, Vector3 offset, float stepOffset, Vector3 position);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_DestroyCharacterController(IntPtr controller);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_ResizeCharacterController(IntPtr controller, float height);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_CharacterControllerSetRadius(IntPtr controller, float radius);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_CharacterControllerSetOffset(IntPtr controller, Vector3 offset);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_CharacterControllerSetHeight(IntPtr controller, float height);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_CharacterControllerSetPosition(IntPtr controller, Vector3 position);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern ControllerCollisionFlag Physics_MoveCharacterController(IntPtr controller, Vector3 delta, uint filterMask);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr Physics_CreateRagdoll();

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_SpawnRagdoll(IntPtr ragdoll);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_DestroyRagdoll(IntPtr ragdoll);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr Physics_RagdollCreateLink(IntPtr ragdoll, IntPtr parentLink, JointType jointType, Matrix linkTransform, Vector3 jointPosition, Vector3 jointRotation, Vector3 velocity, Vector3 rotationVelocity);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RagdollLinkAddBoxCollider(IntPtr link, Vector3 halfExtents, Vector3 position, Quaternion rotation, uint filterGroup, uint filterMask);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RagdollLinkAddSphereCollider(IntPtr link, float radius, Vector3 colliderPosition, uint filterGroup, uint filterMask);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RagdollLinkAddCapsuleCollider(IntPtr link, float radius, float height, Vector3 colliderPosition, Quaternion colliderRotation, uint filterGroup, uint filterMask);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RagdollLinkGetGlobalTransform(IntPtr link, out Vector3 position, out Quaternion rotation);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RagdollLinkSetSwingXLimit(IntPtr link, float min, float max);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RagdollLinkSetSwingZLimit(IntPtr link, float min, float max);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Physics_RagdollLinkSetTwistLimit(IntPtr link, float min, float max);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe int Physics_Raycast(Vector3 origin, Vector3 direction, float maxDistance, HitData* hits, int maxHits, QueryFilterFlags filterData, uint filterMask);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe byte Physics_RaycastCheck(Vector3 origin, Vector3 direction, float maxDistance, out HitData hit, QueryFilterFlags filterData, uint filterMask);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe int Physics_SweepBox(Vector3 halfExtents, Vector3 position, Quaternion rotation, Vector3 direction, float maxDistance, HitData* hits, int maxHits, QueryFilterFlags filterData, uint filterMask);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe int Physics_SweepSphere(float radius, Vector3 position, Quaternion rotation, Vector3 direction, float maxDistance, HitData* hits, int maxHits, QueryFilterFlags filterData, uint filterMask);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe int Physics_SweepCapsule(float radius, float height, Vector3 position, Quaternion rotation, Vector3 direction, float maxDistance, HitData* hits, int maxHits, QueryFilterFlags filterData, uint filterMask);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe int Physics_OverlapBox(Vector3 halfExtents, Vector3 position, Quaternion rotation, HitData* hits, int maxHits, QueryFilterFlags filterData, uint filterMask);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe int Physics_OverlapSphere(float radius, Vector3 position, HitData* hits, int maxHits, QueryFilterFlags filterData, uint filterMask);

		[DllImport(Native.Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern unsafe int Physics_OverlapCapsule(float radius, float height, Vector3 position, Quaternion rotation, HitData* hits, int maxHits, QueryFilterFlags filterData, uint filterMask);
	}
}
