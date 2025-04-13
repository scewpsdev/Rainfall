using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Entity : PhysicsEntity
{
	public string name;
	public bool isStatic;

	public Vector3 position = Vector3.Zero;
	public Quaternion rotation = Quaternion.Identity;
	public Vector3 scale = Vector3.One;

	public bool removed { get; private set; } = false;
	public List<Action> removeCallbacks = new List<Action>();

	public Model model = null;
	public int meshIdx = -1;
	public Animator animator = null;
	public Matrix modelTransform = Matrix.Identity;

	public RigidBody body = null;
	public RigidBodyType bodyType = RigidBodyType.Null;
	public float bodyDensity = 1;
	public uint bodyFilterGroup = 1, bodyFilterMask = 1;
	public float bodyFriction = 0.5f;
	public float bodyRestitution = 0.1f;
	public Dictionary<string, SceneFormat.ColliderData> hitboxData;
	public Dictionary<string, RigidBody> hitboxes;
	public uint hitboxFilterGroup = 1, hitboxFilterMask = 1;

	public List<PointLight> lights = new List<PointLight>();

	public List<ParticleSystem> particles = new List<ParticleSystem>();
	//public Vector3 particleOffset = Vector3.Zero;


	public Entity load(SceneFormat.EntityData entity, uint filterGroup = 1, uint filterMask = 1)
	{
		name = entity.name;
		isStatic = entity.isStatic;
		model = entity.model;

		if (entity.rigidBodyType != RigidBodyType.Null)
		{
			Vector3 centerOfMass = Vector3.Zero;
			if (entity.model != null)
				centerOfMass = entity.model.boundingSphere.center;
			body = new RigidBody(this, entity.rigidBodyType, bodyDensity, centerOfMass, filterGroup, filterMask);
			for (int i = 0; i < entity.colliders.Count; i++)
			{
				SceneFormat.ColliderData collider = entity.colliders[i];
				Quaternion rotation = Quaternion.FromEulerAngles(collider.eulers);
				if (collider.trigger)
				{
					if (collider.type == SceneFormat.ColliderType.Box)
						body.addBoxTrigger(collider.size * 0.5f, collider.offset, rotation);
					else if (collider.type == SceneFormat.ColliderType.Sphere)
						body.addSphereTrigger(collider.radius, collider.offset);
					else if (collider.type == SceneFormat.ColliderType.Capsule)
						body.addCapsuleTrigger(collider.radius, collider.size.y, collider.offset, rotation);
					else if (collider.type == SceneFormat.ColliderType.Mesh)
					{
						if (collider.meshCollider != null)
							body.addMeshTriggers(collider.meshCollider, Matrix.CreateTranslation(collider.offset) * Matrix.CreateRotation(rotation));
					}
					else if (collider.type == SceneFormat.ColliderType.ConvexMesh)
					{
						if (collider.meshCollider != null)
							body.addConvexMeshTriggers(collider.meshCollider, Matrix.CreateTranslation(collider.offset) * Matrix.CreateRotation(rotation));
					}
					else
						Debug.Assert(false);
				}
				else
				{
					if (collider.type == SceneFormat.ColliderType.Box)
						body.addBoxCollider(collider.size * 0.5f, collider.offset, rotation, bodyFriction, bodyRestitution);
					else if (collider.type == SceneFormat.ColliderType.Sphere)
						body.addSphereCollider(collider.radius, collider.offset, bodyFriction, bodyRestitution);
					else if (collider.type == SceneFormat.ColliderType.Capsule)
						body.addCapsuleCollider(collider.radius, collider.size.y, collider.offset, rotation, bodyFriction, bodyRestitution);
					else if (collider.type == SceneFormat.ColliderType.Mesh)
					{
						if (collider.meshCollider != null)
							body.addMeshColliders(collider.meshCollider, Matrix.CreateTranslation(collider.offset) * Matrix.CreateRotation(rotation), bodyFriction, bodyRestitution);
					}
					else if (collider.type == SceneFormat.ColliderType.ConvexMesh)
					{
						if (collider.meshCollider != null)
							body.addConvexMeshColliders(collider.meshCollider, Matrix.CreateTranslation(collider.offset) * Matrix.CreateRotation(rotation), bodyFriction, bodyRestitution);
					}
					else
						Debug.Assert(false);
				}
			}
		}

		if (entity.boneColliders != null)
		{
			hitboxData = new Dictionary<string, SceneFormat.ColliderData>();
			hitboxes = new Dictionary<string, RigidBody>();

			foreach (string nodeName in entity.boneColliders.Keys)
			{
				RigidBody boneCollider = new RigidBody(this, RigidBodyType.Kinematic, hitboxFilterGroup, hitboxFilterMask);
				boneCollider.entityMovesBody = false;
				hitboxData.Add(nodeName, entity.boneColliders[nodeName]);
				hitboxes.Add(nodeName, boneCollider);

				SceneFormat.ColliderData colliderData = entity.boneColliders[nodeName];
				if (colliderData.trigger)
				{
					switch (colliderData.type)
					{
						case SceneFormat.ColliderType.Box:
							boneCollider.addBoxTrigger(colliderData.size * 0.5f, colliderData.offset, Quaternion.FromEulerAngles(colliderData.eulers));
							break;
						case SceneFormat.ColliderType.Sphere:
							boneCollider.addSphereTrigger(colliderData.radius, colliderData.offset);
							break;
						case SceneFormat.ColliderType.Capsule:
							boneCollider.addCapsuleTrigger(colliderData.radius, colliderData.height, colliderData.offset, Quaternion.FromEulerAngles(colliderData.eulers));
							break;
						default:
							Debug.Assert(false);
							break;
					}
				}
				else
				{
					switch (colliderData.type)
					{
						case SceneFormat.ColliderType.Box:
							boneCollider.addBoxCollider(colliderData.size * 0.5f, colliderData.offset, Quaternion.FromEulerAngles(colliderData.eulers));
							break;
						case SceneFormat.ColliderType.Sphere:
							boneCollider.addSphereCollider(colliderData.radius, colliderData.offset);
							break;
						case SceneFormat.ColliderType.Capsule:
							boneCollider.addCapsuleCollider(colliderData.radius, colliderData.height, colliderData.offset, Quaternion.FromEulerAngles(colliderData.eulers));
							break;
						default:
							Debug.Assert(false);
							break;
					}
				}
			}
		}

		for (int i = 0; i < entity.lights.Count; i++)
		{
			PointLight light = new PointLight(entity.lights[i].offset, entity.lights[i].color * entity.lights[i].intensity);
			lights.Add(light);
		}

		for (int i = 0; i < entity.particles.Length; i++)
		{
			ParticleSystem system = ParticleSystem.Create(getModelMatrix());
			system.setData(entity.particles[i]);
			particles.Add(system);
		}

		return this;
	}

	public Entity load(string path, uint filterGroup = 1, uint filterMask = 1)
	{
		if (SceneFormat.Read(path, out List<SceneFormat.EntityData> entities, out _))
		{
			load(entities[0], filterGroup, filterMask);
		}
		return this;
	}

	public void unload()
	{
		if (body != null)
		{
			body.destroy();
			body = null;
		}
		if (hitboxes != null)
		{
			foreach (RigidBody hitbox in hitboxes.Values)
				hitbox.destroy();
			hitboxes.Clear();
			hitboxes = null;
		}
		if (lights != null)
		{
			foreach (PointLight light in lights)
				light.destroy(Renderer.graphics);
			lights.Clear();
			lights = null;
		}
		if (particles != null)
		{
			foreach (ParticleSystem particle in particles)
				ParticleSystem.Destroy(particle);
			particles.Clear();
			particles = null;
		}
	}

	public virtual void init()
	{
		if (body != null)
		{
			body.setTransform(position, rotation);
		}
		if (hitboxes != null)
		{
			Matrix entityTransform = getModelMatrix();
			foreach (string nodeName in hitboxes.Keys)
			{
				Node node = model.skeleton.getNode(nodeName);
				if (node != null)
				{
					Matrix nodeTransform = entityTransform * model.skeleton.getNodeTransform(node);
					hitboxes[nodeName].setTransform(nodeTransform.translation, nodeTransform.rotation);
				}
				else
				{
					Debug.Assert(false);
				}
			}
		}
	}

	public virtual void destroy()
	{
		unload();
	}

	protected void updateBoneHitbox(Node node, Matrix nodeTransform)
	{
		if (hitboxes.ContainsKey(node.name))
		{
			RigidBody hitbox = hitboxes[node.name];
			hitbox.setTransform(nodeTransform.translation, nodeTransform.rotation);
		}

		if (node.children != null)
		{
			for (int i = 0; i < node.children.Length; i++)
			{
				Matrix childTransform = nodeTransform * animator.getNodeLocalTransform(node.children[i]);
				updateBoneHitbox(node.children[i], childTransform);
			}
		}
	}

	public virtual unsafe void update()
	{
		Matrix transform = getModelMatrix();

		if (hitboxes != null && model != null && animator != null)
			updateBoneHitbox(model.skeleton.rootNode, transform * animator.getNodeLocalTransform(model.skeleton.rootNode));

		if (animator != null)
		{
			//animator.update();
			animator.applyAnimation();
		}

		for (int i = 0; i < particles.Count; i++)
		{
			//if (Renderer.IsInFrustum(particles[i].boundingSphere.center, particles[i].boundingSphere.radius, transform, Renderer.pv))
			particles[i].setTransform(transform, particles[i].handle->applyEntityVelocity);
		}
	}

	public virtual void fixedUpdate(float delta)
	{
	}

	public virtual void draw(GraphicsDevice graphics)
	{
		Matrix transform = getModelMatrix();
		if (model != null)
		{
			if (meshIdx != -1)
				Renderer.DrawMesh(model, meshIdx, transform * modelTransform, animator, isStatic);
			else
				Renderer.DrawModel(model, transform * modelTransform, animator, isStatic);
		}
		for (int i = 0; i < lights.Count; i++)
			Renderer.DrawPointLight(lights[i], transform);
		for (int i = 0; i < particles.Count; i++)
			Renderer.DrawParticleSystem(particles[i]);
	}

	public void remove()
	{
		removed = true;
	}

	public void addRemoveCallback(Action action)
	{
		removeCallbacks.Add(action);
	}

	public void setTransform(Matrix transform)
	{
		if (body != null)
		{
			if (body.type == RigidBodyType.Dynamic || body.type == RigidBodyType.Kinematic)
				body.setTransform(transform.translation, transform.rotation);
		}
		else
		{
			position = transform.translation;
			rotation = transform.rotation;
		}
	}

	public void setPosition(Vector3 position)
	{
		this.position = position;
	}

	public Vector3 getPosition()
	{
		return position;
	}

	public void setRotation(Quaternion rotation)
	{
		this.rotation = rotation;
	}

	public Quaternion getRotation()
	{
		return rotation;
	}

	public virtual void onContact(RigidBody other, CharacterController otherController, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType)
	{
	}

	public Node getHitboxNode(RigidBody body)
	{
		if (hitboxes != null)
		{
			foreach (string nodeName in hitboxes.Keys)
			{
				if (hitboxes[nodeName] == body)
					return model.skeleton.getNode(nodeName);
			}
		}
		return null;
	}

	public Matrix getModelMatrix(Vector3 offset)
	{
		return Matrix.CreateTranslation(position + offset) * Matrix.CreateRotation(rotation) * Matrix.CreateScale(scale);
	}

	public Matrix getModelMatrix()
	{
		return getModelMatrix(Vector3.Zero);
	}
}
