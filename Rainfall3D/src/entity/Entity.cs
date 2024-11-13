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
	public bool isOccluder;

	public Vector3 position = Vector3.Zero;
	public Quaternion rotation = Quaternion.Identity;
	public Vector3 scale = Vector3.One;

	public bool removed { get; private set; } = false;
	public List<Action> removeCallbacks = new List<Action>();

	public Model model = null;
	public Animator animator = null;
	public Matrix modelTransform = Matrix.Identity;

	public RigidBody body = null;
	public RigidBodyType bodyType = RigidBodyType.Null;
	public uint bodyFilterGroup = 1, bodyFilterMask = 1;
	public float bodyFriction = 0.5f;
	public Dictionary<string, SceneFormat.ColliderData> hitboxData;
	public Dictionary<string, RigidBody> hitboxes;
	public uint hitboxFilterGroup = 1, hitboxFilterMask = 1;

	public List<PointLight> lights = new List<PointLight>();

	public List<ParticleSystem> particles = new List<ParticleSystem>();
	//public Vector3 particleOffset = Vector3.Zero;


	public void load(SceneFormat.EntityData entity, uint filterGroup = 1)
	{
		model = entity.model;

		if (entity.rigidBodyType != RigidBodyType.Null)
		{
			body = new RigidBody(this, entity.rigidBodyType, filterGroup);
			for (int i = 0; i < entity.colliders.Count; i++)
			{
				SceneFormat.ColliderData collider = entity.colliders[i];
				if (collider.type == SceneFormat.ColliderType.Box)
					body.addBoxCollider(collider.size * 0.5f, collider.offset, Quaternion.Identity);
				else if (collider.type == SceneFormat.ColliderType.Sphere)
					body.addSphereCollider(collider.radius, collider.offset);
				else if (collider.type == SceneFormat.ColliderType.Capsule)
					body.addCapsuleCollider(collider.radius, collider.size.y, collider.offset, Quaternion.Identity);
				else if (collider.type == SceneFormat.ColliderType.Mesh)
					body.addMeshColliders(collider.meshCollider, Matrix.Identity);
				else if (collider.type == SceneFormat.ColliderType.ConvexMesh)
					body.addConvexMeshColliders(collider.meshCollider, Matrix.Identity);
				else
					Debug.Assert(false);
			}
		}

		for (int i = 0; i < entity.lights.Count; i++)
		{
			PointLight light = new PointLight(entity.lights[i].offset, entity.lights[i].color);
			lights.Add(light);
		}

		for (int i = 0; i < entity.particles.Length; i++)
		{
			ParticleSystem system = ParticleSystem.Create(getModelMatrix());
			system.setData(entity.particles[i]);
			particles.Add(system);
		}
	}

	protected void load(string path, uint filterGroup = 1)
	{
		if (SceneFormat.Read(path, out List<SceneFormat.EntityData> entities, out _))
		{
			load(entities[0], filterGroup);
		}
	}

	public virtual void init()
	{
		if (body != null)
			body.setTransform(position, rotation);
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
			}
		}
	}

	public virtual void destroy()
	{
		model?.destroy();
		if (animator != null)
		{
			Animator.Destroy(animator);
			animator = null;
		}
		body?.destroy();
		if (hitboxes != null)
		{
			foreach (var hitbox in hitboxes.Values)
				hitbox.destroy();
		}
		if (lights != null)
		{
			foreach (PointLight light in lights)
				light.destroy(Renderer.graphics);
		}
		if (particles != null)
		{
			foreach (ParticleSystem particleSystem in particles)
				ParticleSystem.Destroy(particleSystem);
		}
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

	public virtual void update()
	{
		Matrix transform = getModelMatrix();

		if (body != null)
		{
			if (body.type == RigidBodyType.Dynamic)
				body.getTransform(out position, out rotation);
			else if (body.type == RigidBodyType.Kinematic)
				body.setTransform(position, rotation);
		}
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
			particles[i].setTransform(transform);
		}
	}

	public virtual void draw(GraphicsDevice graphics)
	{
		Matrix transform = getModelMatrix();
		if (model != null)
			Renderer.DrawModel(model, transform * modelTransform, animator);
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
