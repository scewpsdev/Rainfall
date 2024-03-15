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
	public Animator animator = null;
	public Matrix modelTransform = Matrix.Identity;

	public RigidBodyType bodyType;
	public RigidBody body = null;
	public Dictionary<string, SceneFormat.ColliderData> hitboxData;
	public Dictionary<string, RigidBody> hitboxes;
	public uint hitboxFilterGroup = 1, hitboxFilterMask = 1;

	public List<PointLight> lights = new List<PointLight>();

	public List<ParticleSystem> particles = new List<ParticleSystem>();
	//public Vector3 particleOffset = Vector3.Zero;


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
		animator?.destroy();
	}

	void updateBoneHitbox(Node node, Matrix nodeTransform)
	{
		if (hitboxes.ContainsKey(node.name))
		{
			RigidBody hitbox = hitboxes[node.name];
			hitbox.setTransform(nodeTransform.translation, nodeTransform.rotation);
		}

		for (int i = 0; i < node.children.Length; i++)
		{
			Matrix childTransform = nodeTransform * animator.getNodeLocalTransform(node.children[i]);
			updateBoneHitbox(node.children[i], childTransform);
		}
	}

	public virtual void update()
	{
		if (animator != null)
		{
			animator.update();
			animator.applyAnimation();
		}

		if (body != null)
		{
			if (bodyType == RigidBodyType.Dynamic)
				body.getTransform(out position, out rotation);
			else if (bodyType == RigidBodyType.Kinematic)
				body.setTransform(position, rotation);
		}

		Matrix transform = getModelMatrix();

		if (hitboxes != null && model != null && animator != null)
			updateBoneHitbox(model.skeleton.rootNode, transform * animator.getNodeLocalTransform(model.skeleton.rootNode));

		for (int i = 0; i < particles.Count; i++)
			particles[i].update(transform);
	}

	public virtual void draw(GraphicsDevice graphics)
	{
		Matrix transform = getModelMatrix();
		if (model != null)
			Renderer.DrawModel(model, transform * modelTransform, animator);
		for (int i = 0; i < lights.Count; i++)
		{
			if (isStatic)
				Renderer.DrawPointLight(lights[i], position);
			else
				Renderer.DrawLight(position + lights[i].offset, lights[i].color);
		}
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
		position = transform.translation;
		rotation = transform.rotation;
		if (body != null && bodyType == RigidBodyType.Kinematic)
			body.setTransform(position, rotation);
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
