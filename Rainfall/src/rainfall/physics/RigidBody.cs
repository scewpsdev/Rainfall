using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Rainfall;


namespace Rainfall
{
	enum ActorType
	{
		RigidBody,
		CharacterController,
	}

	public enum RigidBodyType
	{
		Null = 0,
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

	[StructLayout(LayoutKind.Sequential)]
	struct RigidBodyData
	{
		public ActorType actorType;

		public RigidBodyType type;
		public float density;
		public Vector3 centerOfMass;

		public IntPtr actor;

		public Vector3 position0, position1;
		public Quaternion rotation0, rotation1;
	}

	public unsafe class RigidBody
	{
		internal static Dictionary<IntPtr, RigidBody> bodies = new Dictionary<IntPtr, RigidBody>();
		public static int numBodies { get => bodies.Count; }


		RigidBodyData* body;

		public RigidBodyType type => body->type;

		public PhysicsEntity entity;
		public readonly uint filterGroup, filterMask;

		public readonly Ragdoll ragdoll = null;


		public RigidBody(PhysicsEntity entity, RigidBodyType type, Vector3 position, Quaternion rotation, float density, Vector3 centerOfMass, uint filterGroup = 1, uint filterMask = 1)
		{
			this.entity = entity;
			this.filterGroup = filterGroup;
			this.filterMask = filterMask;

			body = Physics.Physics_CreateRigidBody(type, density, centerOfMass, position, rotation);
			bodies.Add((IntPtr)body, this);
		}

		public RigidBody(PhysicsEntity entity, RigidBodyType type, Vector3 position, Quaternion rotation)
			: this(entity, type, position, rotation, 1.0f, Vector3.Zero)
		{
		}

		public RigidBody(PhysicsEntity entity, RigidBodyType type, float density, Vector3 centerOfMass, uint filterGroup = 1, uint filterMask = 1)
			: this(entity, type, entity != null ? entity.getPosition() : Vector3.Zero, entity != null ? entity.getRotation() : Quaternion.Identity, density, centerOfMass, filterGroup, filterMask)
		{
		}

		public RigidBody(PhysicsEntity entity, RigidBodyType type = RigidBodyType.Dynamic, uint filterGroup = 1, uint filterMask = 1)
			: this(entity, type, 1.0f, Vector3.Zero, filterGroup, filterMask)
		{
		}

		public RigidBody(PhysicsEntity entity, IntPtr body, Ragdoll ragdoll, uint filterGroup, uint filterMask)
		{
			this.entity = entity;
			this.body = (RigidBodyData*)body;
			this.ragdoll = ragdoll;
			this.filterGroup = filterGroup;
			this.filterMask = filterMask;
		}

		public void destroy()
		{
			bodies.Remove((IntPtr)body);
			Physics.Physics_DestroyRigidBody(body);
			body = null;
		}

		public bool isValid
		{
			get => body != null;
		}

		public void addSphereCollider(float radius, Vector3 position, float friction = 0.5f, float restitution = 0.1f)
		{
			Physics.Physics_RigidBodyAddSphereCollider(body, radius, position, filterGroup, filterMask, friction, friction, restitution);
		}

		public void addBoxCollider(Vector3 halfExtents, Vector3 position, Quaternion rotation, float friction = 0.5f, float restitution = 0.1f)
		{
			Physics.Physics_RigidBodyAddBoxCollider(body, halfExtents, position, rotation, filterGroup, filterMask, friction, friction, restitution);
		}

		public void addBoxCollider(Vector3 position, Vector3 size)
		{
			addBoxCollider(size * 0.5f, position + 0.5f * size, Quaternion.Identity);
		}

		public void addCapsuleCollider(float radius, float height, Vector3 position, Quaternion rotation, float friction = 0.5f, float restitution = 0.1f)
		{
			Physics.Physics_RigidBodyAddCapsuleCollider(body, radius, height, position, rotation, filterGroup, filterMask, friction, friction, restitution);
		}

		public void addMeshCollider(MeshCollider mesh, Matrix transform, float friction = 0.5f, float restitution = 0.1f)
		{
			Physics.Physics_RigidBodyAddMeshCollider(body, mesh.handle, transform * mesh.transform, filterGroup, filterMask, friction, friction, restitution);
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
			Physics.Physics_RigidBodyAddConvexMeshCollider(body, mesh.handle, transform, filterGroup, filterMask, friction, friction, restitution);
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
			Physics.Physics_RigidBodyAddHeightFieldCollider(body, heightField, scale, transform, filterGroup, filterMask, friction, friction, restitution);
		}

		public void addSphereTrigger(float radius, Vector3 position)
		{
			Physics.Physics_RigidBodyAddSphereTrigger(body, radius, position, filterGroup, filterMask);
		}

		public void addBoxTrigger(Vector3 halfExtents, Vector3 position, Quaternion rotation)
		{
			Physics.Physics_RigidBodyAddBoxTrigger(body, halfExtents, position, rotation, filterGroup, filterMask);
		}

		public void addBoxTrigger(Vector3 halfExtents)
		{
			Physics.Physics_RigidBodyAddBoxTrigger(body, halfExtents, Vector3.Zero, Quaternion.Identity, filterGroup, filterMask);
		}

		public void addCapsuleTrigger(float radius, float height, Vector3 position, Quaternion rotation)
		{
			Physics.Physics_RigidBodyAddCapsuleTrigger(body, radius, height, position, rotation, filterGroup, filterMask);
		}

		public void addMeshTrigger(MeshCollider mesh, Matrix transform)
		{
			Physics.Physics_RigidBodyAddMeshTrigger(body, mesh.handle, transform * mesh.transform, filterGroup, filterMask);
		}

		public MeshCollider addMeshTrigger(Model model, int meshIdx, Matrix transform)
		{
			MeshCollider mesh = Physics.CreateMeshCollider(model, meshIdx);
			addMeshTrigger(mesh, transform);
			return mesh;
		}

		public void addMeshTriggers(Model model, Matrix transform)
		{
			for (int i = 0; i < model.meshCount; i++)
				addMeshTrigger(model, i, transform);
		}

		public void addConvexMeshTrigger(ConvexMeshCollider mesh, Matrix transform)
		{
			Physics.Physics_RigidBodyAddConvexMeshTrigger(body, mesh.handle, transform * mesh.transform, filterGroup, filterMask);
		}

		public ConvexMeshCollider addConvexMeshTrigger(Model model, int meshIdx, Matrix transform)
		{
			ConvexMeshCollider mesh = Physics.CreateConvexMeshCollider(model, meshIdx);
			addConvexMeshTrigger(mesh, transform);
			return mesh;
		}

		public void addConvexMeshTriggers(Model model, Matrix transform)
		{
			for (int i = 0; i < model.meshCount; i++)
				addConvexMeshTrigger(model, i, transform);
		}

		public void clearColliders()
		{
			Physics.Physics_RigidBodyClearColliders(body);
		}

		public void setSimulationEnabled(bool enabled)
		{
			Physics.Physics_RigidBodySetSimulationEnabled(body, (byte)(enabled ? 1 : 0));
		}

		public void setGravityEnabled(bool enabled)
		{
			Physics.Physics_RigidBodySetGravityEnabled(body, (byte)(enabled ? 1 : 0));
		}

		public void setTransform(Vector3 position, Quaternion rotation)
		{
			Physics.Physics_RigidBodySetTransform(body, position, rotation);
		}

		public void setRotation(Quaternion rotation)
		{
			Physics.Physics_RigidBodySetRotation(body, rotation);
		}

		public void setVelocity(Vector3 velocity)
		{
			Physics.Physics_RigidBodySetVelocity(body, velocity);
		}

		public void setVelocityX(float x)
		{
			Vector3 velocity = getVelocity();
			Physics.Physics_RigidBodySetVelocity(body, new Vector3(x, velocity.y, velocity.z));
		}

		public void setVelocityY(float y)
		{
			Vector3 velocity = getVelocity();
			Physics.Physics_RigidBodySetVelocity(body, new Vector3(velocity.x, y, velocity.z));
		}

		public void setVelocityZ(float z)
		{
			Vector3 velocity = getVelocity();
			Physics.Physics_RigidBodySetVelocity(body, new Vector3(velocity.x, velocity.y, z));
		}

		public void setRotationVelocity(Vector3 rotationVelocity)
		{
			Physics.Physics_RigidBodySetRotationVelocity(body, rotationVelocity);
		}

		public void setCenterOfMass(Vector3 centerOfMass)
		{
			Physics.Physics_RigidBodySetCenterOfMass(body, centerOfMass);
		}

		public void addForce(Vector3 force)
		{
			Physics.Physics_RigidBodyAddForce(body, force);
		}

		public void addTorque(Vector3 torque)
		{
			Physics.Physics_RigidBodyAddTorque(body, torque);
		}

		public void addForceAtPosition(Vector3 force, Vector3 position)
		{
			addForce(force);
			addTorque(Vector3.Cross(position - getCenterOfMass(), force));
		}

		public void addAcceleration(Vector3 acceleration)
		{
			Physics.Physics_RigidBodyAddAcceleration(body, acceleration);
		}

		public void addImpulse(Vector3 impulse)
		{
			Physics.Physics_RigidBodyAddAcceleration(body, impulse);
		}

		public void lockAxis(bool x, bool y, bool z)
		{
			Physics.Physics_RigidBodyLockAxis(body, x ? (byte)1 : (byte)0, y ? (byte)1 : (byte)0, z ? (byte)1 : (byte)0);
		}

		public void lockRotationAxis(bool x, bool y, bool z)
		{
			Physics.Physics_RigidBodyLockRotationAxis(body, x ? (byte)1 : (byte)0, y ? (byte)1 : (byte)0, z ? (byte)1 : (byte)0);
		}

		public void getTransform(out Vector3 position, out Quaternion rotation)
		{
			Physics.Physics_RigidBodyGetTransform(body, out position, out rotation);
		}

		public Vector3 getVelocity()
		{
			Physics.Physics_RigidBodyGetVelocity(body, out Vector3 velocity);
			return velocity;
		}

		public Vector3 getAngularVelocity()
		{
			Physics.Physics_RigidBodyGetAngularVelocity(body, out Vector3 angularVelocity);
			return angularVelocity;
		}

		public Vector3 getCenterOfMass()
		{
			Physics.Physics_RigidBodyGetCenterOfMass(body, out Vector3 centerOfMass);
			return entity.getPosition() + entity.getRotation() * centerOfMass;
		}

		public Vector3 getPointVelocity(Vector3 point)
		{
			return getVelocity() + Vector3.Cross(getAngularVelocity(), point - getCenterOfMass());
		}

		public Vector3 getPosition()
		{
			getTransform(out Vector3 position, out Quaternion _);
			return position;
		}

		internal static RigidBody GetBodyFromHandle(RigidBodyData* handle)
		{
			if (bodies.ContainsKey((IntPtr)handle))
				return bodies[(IntPtr)handle];
			return null;
		}
	}
}
