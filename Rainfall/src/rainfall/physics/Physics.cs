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
	public struct HitData
	{
		public float distance;
		public Vector3 position;
		public Vector3 normal;
		[MarshalAs(UnmanagedType.Bool)] public bool isTrigger;

		public IntPtr userData;


		public RigidBody body
		{
			get => RigidBody.GetBodyFromHandle(userData);
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

	public static class Physics
	{
		static readonly Native.Physics.RigidBodySetTransformCallback_t rigidBodySetTransformCallback = RigidBodySetTransform;
		static readonly Native.Physics.RigidBodyGetTransformCallback_t rigidBodyGetTransformCallback = RigidBodyGetTransform;
		static readonly Native.Physics.RigidBodyContactCallback_t rigidBodyContactCallback = RigidBodyContactCallback;

		static readonly Native.Physics.CharacterControllerSetPositionCallback_t characterControllerSetPositionCallback = SetCharacterControllerPosition;
		static readonly Native.Physics.CharacterControllerOnHitCallback_t characterControllerOnHitCallback = CharacterControllerOnHit;


		public static void Init()
		{
			Native.Physics.Physics_Init(rigidBodySetTransformCallback, rigidBodyGetTransformCallback, rigidBodyContactCallback, characterControllerSetPositionCallback, characterControllerOnHitCallback);
		}

		public static void Shutdown()
		{
			Native.Physics.Physics_Shutdown();
		}

		public static void Update()
		{
			Native.Physics.Physics_Update();
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
			IntPtr handle = Native.Physics.Physics_CreateMeshCollider(mesh.vertices, mesh.vertexCount, mesh.indices, mesh.indexCount);
			Matrix transform = GetNodeTransform(model, meshIdx);
			return new MeshCollider(handle, transform);
		}

		public static unsafe ConvexMeshCollider CreateConvexMeshCollider(Model model, int meshIdx)
		{
			MeshData mesh = model.scene->meshes[meshIdx];
			IntPtr handle = Native.Physics.Physics_CreateConvexMeshCollider(mesh.vertices, mesh.vertexCount, mesh.indices, mesh.indexCount);
			Matrix transform = GetNodeTransform(model, meshIdx);
			return new ConvexMeshCollider(handle, transform);
		}

		public static IntPtr CreateHeightField(int width, int height, HeightFieldSample[] data)
		{
			return Native.Physics.Physics_CreateHeightField(width, height, ref data[0]);
		}

		public static void DestroyHeightField(IntPtr heightField)
		{
			Native.Physics.Physics_DestroyHeightField(heightField);
		}

		public static int Raycast(Vector3 origin, Vector3 direction, float distance, Span<HitData> hits, QueryFilterFlags filterData = QueryFilterFlags.Default)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Native.Physics.Physics_Raycast(origin, direction, distance, data, hits.Length, filterData);
			}
		}

		public static int Raycast(Vector3 origin, Vector3 direction, float distance, HitData[] hits, QueryFilterFlags filterData = QueryFilterFlags.Default)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Native.Physics.Physics_Raycast(origin, direction, distance, data, hits.Length, filterData);
			}
		}

		public static HitData? Raycast(Vector3 origin, Vector3 direction, float distance, uint filterMask = 1, QueryFilterFlags filterData = QueryFilterFlags.Default)
		{
			Span<HitData> hits = stackalloc HitData[16];
			int numHits = Raycast(origin, direction, distance, hits, filterData);

			if (numHits > 0)
			{
				float shortestDistance = float.MaxValue;
				int closestHit = -1;
				for (int i = 0; i < numHits; i++)
				{
					if (hits[i].body != null && (hits[i].body.filterMask & filterMask) != 0 && hits[i].distance < shortestDistance)
					{
						shortestDistance = hits[i].distance;
						closestHit = i;
					}
				}
				if (closestHit != -1)
					return hits[closestHit];
			}

			return null;
		}

		public static int SweepBox(Vector3 halfExtents, Vector3 position, Quaternion rotation, Vector3 direction, float distance, Span<HitData> hits, QueryFilterFlags filterData = QueryFilterFlags.Default)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Native.Physics.Physics_SweepBox(halfExtents, position, rotation, direction, distance, data, hits.Length, filterData);
			}
		}

		public static int SweepBox(Vector3 halfExtents, Vector3 position, Quaternion rotation, Vector3 direction, float distance, HitData[] hits, QueryFilterFlags filterData = QueryFilterFlags.Default)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Native.Physics.Physics_SweepBox(halfExtents, position, rotation, direction, distance, data, hits.Length, filterData);
			}
		}

		public static int SweepSphere(float radius, Vector3 position, Vector3 direction, float distance, Span<HitData> hits, QueryFilterFlags filterData = QueryFilterFlags.Default)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Native.Physics.Physics_SweepSphere(radius, position, Quaternion.Identity, direction, distance, data, hits.Length, filterData);
			}
		}

		public static int SweepSphere(float radius, Vector3 position, Vector3 direction, float distance, HitData[] hits, QueryFilterFlags filterData = QueryFilterFlags.Default)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Native.Physics.Physics_SweepSphere(radius, position, Quaternion.Identity, direction, distance, data, hits.Length, filterData);
			}
		}

		public static int SweepCapsule(float radius, float height, Vector3 position, Quaternion rotation, Vector3 direction, float distance, Span<HitData> hits, QueryFilterFlags filterData = QueryFilterFlags.Default)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Native.Physics.Physics_SweepCapsule(radius, height, position, rotation, direction, distance, data, hits.Length, filterData);
			}
		}

		public static int SweepCapsule(float radius, float height, Vector3 position, Quaternion rotation, Vector3 direction, float distance, HitData[] hits, QueryFilterFlags filterData = QueryFilterFlags.Default)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Native.Physics.Physics_SweepCapsule(radius, height, position, rotation, direction, distance, data, hits.Length, filterData);
			}
		}

		public static int OverlapBox(Vector3 halfExtents, Vector3 position, Quaternion rotation, Span<HitData> hits, QueryFilterFlags filterData = QueryFilterFlags.Default)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Native.Physics.Physics_OverlapBox(halfExtents, position, rotation, data, hits.Length, filterData);
			}
		}

		public static int OverlapBox(Vector3 halfExtents, Vector3 position, Quaternion rotation, HitData[] hits, QueryFilterFlags filterData = QueryFilterFlags.Default)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Native.Physics.Physics_OverlapBox(halfExtents, position, rotation, data, hits.Length, filterData);
			}
		}

		public static int OverlapSphere(float radius, Vector3 position, Span<HitData> hits, QueryFilterFlags filterData = QueryFilterFlags.Default)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Native.Physics.Physics_OverlapSphere(radius, position, data, hits.Length, filterData);
			}
		}

		public static int OverlapSphere(float radius, Vector3 position, HitData[] hits, QueryFilterFlags filterData = QueryFilterFlags.Default)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Native.Physics.Physics_OverlapSphere(radius, position, data, hits.Length, filterData);
			}
		}

		public static int OverlapCapsule(float radius, float height, Vector3 position, Quaternion rotation, Span<HitData> hits, QueryFilterFlags filterData = QueryFilterFlags.Default)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Native.Physics.Physics_OverlapCapsule(radius, height, position, rotation, data, hits.Length, filterData);
			}
		}

		public static int OverlapCapsule(float radius, float height, Vector3 position, Quaternion rotation, HitData[] hits, QueryFilterFlags filterData = QueryFilterFlags.Default)
		{
			unsafe
			{
				fixed (HitData* data = hits)
					return Native.Physics.Physics_OverlapCapsule(radius, height, position, rotation, data, hits.Length, filterData);
			}
		}


		static void RigidBodySetTransform(Vector3 position, Quaternion rotation, IntPtr userPtr)
		{
			RigidBody body = RigidBody.GetBodyFromHandle(userPtr);
			body.entity.setPosition(position);
			body.entity.setRotation(rotation);
		}

		static void RigidBodyGetTransform(ref Vector3 position, ref Quaternion rotation, IntPtr userPtr)
		{
			RigidBody body = RigidBody.GetBodyFromHandle(userPtr);
			position = body.entity.getPosition();
			rotation = body.entity.getRotation();
		}

		static void RigidBodyContactCallback(IntPtr bodyHandle, IntPtr otherHandle, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType, IntPtr otherControllerHandle)
		{
			RigidBody body = RigidBody.GetBodyFromHandle(bodyHandle);
			RigidBody other = otherHandle != IntPtr.Zero ? RigidBody.GetBodyFromHandle(otherHandle) : null;
			CharacterController otherController = otherControllerHandle != IntPtr.Zero ? CharacterController.GetControllerFromHandle(otherControllerHandle) : null;

			if (body != null)
			{
				body.entity?.onContact(other, otherController, shapeID, otherShapeID, isTrigger, otherTrigger, contactType);

				if (other != null)
				{
					Debug.Assert(otherController == null);
					other.entity?.onContact(body, null, otherShapeID, shapeID, otherTrigger, isTrigger, contactType);
				}
				if (otherController != null)
				{
					Debug.Assert(other == null);
					otherController.entity?.onContact(null, otherController, otherShapeID, shapeID, otherTrigger, isTrigger, contactType);
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
	}
}
