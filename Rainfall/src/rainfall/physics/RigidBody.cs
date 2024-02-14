using System;
using System.Xml.Linq;
using Rainfall;


namespace Rainfall
{
	public enum RigidBodyType
	{
		Static,
		Kinematic,
		Dynamic,
	}

	public enum ContactType
	{
		Found,
		Lost,
		Persists,
	}

	public class RigidBody
	{
		internal static Dictionary<IntPtr, RigidBody> bodies = new Dictionary<IntPtr, RigidBody>();
		public static int numBodies { get => bodies.Count; }


		public readonly PhysicsEntity entity;
		public readonly Ragdoll ragdoll = null;

		IntPtr body;
		public readonly uint filterGroup, filterMask;


		public RigidBody(PhysicsEntity entity, RigidBodyType type, Vector3 position, Quaternion rotation, float density, Vector3 centerOfMass, uint filterGroup = 1, uint filterMask = 0x0000FFFF)
		{
			this.entity = entity;
			this.filterGroup = filterGroup;
			this.filterMask = filterMask;

			body = Native.Physics.Physics_CreateRigidBody(type, density, centerOfMass, position, rotation);
			bodies.Add(body, this);
		}

		public RigidBody(PhysicsEntity entity, RigidBodyType type, Vector3 position, Quaternion rotation)
			: this(entity, type, position, rotation, 1.0f, Vector3.Zero)
		{
		}

		public RigidBody(PhysicsEntity entity, RigidBodyType type, float density, Vector3 centerOfMass, uint filterGroup = 1, uint filterMask = 0x0000FFFF)
			: this(entity, type, entity != null ? entity.getPosition() : Vector3.Zero, entity != null ? entity.getRotation() : Quaternion.Identity, density, centerOfMass, filterGroup, filterMask)
		{
		}

		public RigidBody(PhysicsEntity entity, RigidBodyType type = RigidBodyType.Dynamic, uint filterGroup = 1, uint filterMask = 0x0000FFFF)
			: this(entity, type, 1.0f, Vector3.Zero, filterGroup, filterMask)
		{
		}

		public RigidBody(PhysicsEntity entity, IntPtr body, Ragdoll ragdoll, uint filterGroup, uint filterMask)
		{
			this.entity = entity;
			this.body = body;
			this.ragdoll = ragdoll;
			this.filterGroup = filterGroup;
			this.filterMask = filterMask;
		}

		public void destroy()
		{
			bodies.Remove(body);
			Native.Physics.Physics_DestroyRigidBody(body);
			body = IntPtr.Zero;
		}

		public bool isValid
		{
			get => body != IntPtr.Zero;
		}

		public void addSphereCollider(float radius, Vector3 position)
		{
			Native.Physics.Physics_RigidBodyAddSphereCollider(body, radius, position, filterGroup, filterMask, 0.5f, 0.5f, 0.1f);
		}

		public void addBoxCollider(Vector3 halfExtents, Vector3 position, Quaternion rotation, float friction = 0.5f, float restitution = 0.1f)
		{
			Native.Physics.Physics_RigidBodyAddBoxCollider(body, halfExtents, position, rotation, filterGroup, filterMask, friction, friction, restitution);
		}

		public void addBoxCollider(Vector3 position, Vector3 size)
		{
			addBoxCollider(size * 0.5f, position + 0.5f * size, Quaternion.Identity);
		}

		public void addCapsuleCollider(float radius, float height, Vector3 position, Quaternion rotation, float friction = 0.0f, float restitution = 0.1f)
		{
			Native.Physics.Physics_RigidBodyAddCapsuleCollider(body, radius, height, position, rotation, filterGroup, filterMask, friction, friction, restitution);
		}

		public void addCapsuleCollider(float radius, float height, Vector3 position, Quaternion rotation, uint filterGroup, uint filterMask, float friction = 0.0f, float restitution = 0.1f)
		{
			Native.Physics.Physics_RigidBodyAddCapsuleCollider(body, radius, height, position, rotation, filterGroup, filterMask, friction, friction, restitution);
		}

		public void addMeshCollider(MeshCollider mesh, Matrix transform, float friction = 0.5f, float restitution = 0.1f)
		{
			Native.Physics.Physics_RigidBodyAddMeshCollider(body, mesh.handle, transform * mesh.transform, filterGroup, filterMask, friction, friction, restitution);
		}

		public MeshCollider addMeshCollider(Model model, int meshIdx, Matrix transform, float friction = 0.5f, float restitution = 0.1f)
		{
			MeshCollider mesh = Physics.CreateMeshCollider(model, meshIdx);
			addMeshCollider(mesh, transform, friction, restitution);
			return mesh;
		}

		public void addMeshColliders(Model model, Matrix transform, float friction = 0.5f, float restitution = 0.1f)
		{
			for (int i = 0; i < model.meshCount; i++)
				addMeshCollider(model, i, transform, friction, restitution);
		}

		public void addConvexMeshCollider(ConvexMeshCollider mesh, Matrix transform, float friction = 0.5f, float restitution = 0.1f)
		{
			Native.Physics.Physics_RigidBodyAddConvexMeshCollider(body, mesh.handle, transform, filterGroup, filterMask, friction, friction, restitution);
		}

		public ConvexMeshCollider addConvexMeshCollider(Model model, int meshIdx, Matrix transform, float friction = 0.5f, float restitution = 0.1f)
		{
			ConvexMeshCollider mesh = Physics.CreateConvexMeshCollider(model, meshIdx);
			addConvexMeshCollider(mesh, transform, friction, restitution);
			return mesh;
		}

		public void addConvexMeshColliders(Model model, Matrix transform, float friction = 0.5f, float restitution = 0.1f)
		{
			for (int i = 0; i < model.meshCount; i++)
				addConvexMeshCollider(model, i, transform, friction, restitution);
		}

		public void addHeightFieldCollider(IntPtr heightField, Vector3 scale, Matrix transform, float friction = 0.5f, float restitution = 0.1f)
		{
			Native.Physics.Physics_RigidBodyAddHeightFieldCollider(body, heightField, scale, transform, filterGroup, filterMask, friction, friction, restitution);
		}

		public void addSphereTrigger(float radius, Vector3 position)
		{
			Native.Physics.Physics_RigidBodyAddSphereTrigger(body, radius, position, filterGroup, filterMask);
		}

		public void addBoxTrigger(Vector3 halfExtents, Vector3 position, Quaternion rotation)
		{
			Native.Physics.Physics_RigidBodyAddBoxTrigger(body, halfExtents, position, rotation, filterGroup, filterMask);
		}

		public void addBoxTrigger(Vector3 halfExtents)
		{
			Native.Physics.Physics_RigidBodyAddBoxTrigger(body, halfExtents, Vector3.Zero, Quaternion.Identity, filterGroup, filterMask);
		}

		public void addCapsuleTrigger(float radius, float height, Vector3 position, Quaternion rotation)
		{
			Native.Physics.Physics_RigidBodyAddCapsuleTrigger(body, radius, height, position, rotation, filterGroup, filterMask);
		}

		public void addMeshTrigger(IntPtr mesh, Vector3 position, Quaternion rotation)
		{
			Native.Physics.Physics_RigidBodyAddMeshTrigger(body, mesh, position, rotation, filterGroup, filterMask);
		}

		public void clearColliders()
		{
			Native.Physics.Physics_RigidBodyClearColliders(body);
		}

		public void setTransform(Vector3 position, Quaternion rotation)
		{
			Native.Physics.Physics_RigidBodySetTransform(body, position, rotation);
		}

		public void setVelocity(Vector3 velocity)
		{
			Native.Physics.Physics_RigidBodySetVelocity(body, velocity);
		}

		public void setVelocityX(float x)
		{
			Vector3 velocity = getVelocity();
			Native.Physics.Physics_RigidBodySetVelocity(body, new Vector3(x, velocity.y, velocity.z));
		}

		public void setVelocityY(float y)
		{
			Vector3 velocity = getVelocity();
			Native.Physics.Physics_RigidBodySetVelocity(body, new Vector3(velocity.x, y, velocity.z));
		}

		public void setVelocityZ(float z)
		{
			Vector3 velocity = getVelocity();
			Native.Physics.Physics_RigidBodySetVelocity(body, new Vector3(velocity.x, velocity.y, z));
		}

		public void setRotationVelocity(Vector3 rotationVelocity)
		{
			Native.Physics.Physics_RigidBodySetRotationVelocity(body, rotationVelocity);
		}

		public void addForce(Vector3 force)
		{
			Native.Physics.Physics_RigidBodyAddForce(body, force);
		}

		public void lockAxis(bool x, bool y, bool z)
		{
			Native.Physics.Physics_RigidBodyLockAxis(body, x ? (byte)1 : (byte)0, y ? (byte)1 : (byte)0, z ? (byte)1 : (byte)0);
		}

		public void lockRotationAxis(bool x, bool y, bool z)
		{
			Native.Physics.Physics_RigidBodyLockRotationAxis(body, x ? (byte)1 : (byte)0, y ? (byte)1 : (byte)0, z ? (byte)1 : (byte)0);
		}

		public void getTransform(out Vector3 position, out Quaternion rotation)
		{
			Native.Physics.Physics_RigidBodyGetTransform(body, out position, out rotation);
		}

		public Vector3 getPosition()
		{
			getTransform(out Vector3 position, out Quaternion _);
			return position;
		}

		public Vector3 getVelocity()
		{
			Native.Physics.Physics_RigidBodyGetVelocity(body, out Vector3 velocity);
			return velocity;
		}

		internal static RigidBody GetBodyFromHandle(IntPtr handle)
		{
			if (bodies.ContainsKey(handle))
				return bodies[handle];
			return null;
		}
	}
}
