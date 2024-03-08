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

	public RigidBody body = null;

	public List<PointLight> lights = new List<PointLight>();

	public List<ParticleSystem> particles = new List<ParticleSystem>();
	//public Vector3 particleOffset = Vector3.Zero;


	public virtual void init()
	{
		if (body != null && !isStatic)
			body.setTransform(position, rotation);
	}

	public virtual void destroy()
	{
		model?.destroy();
		animator?.destroy();
	}

	public virtual void update()
	{
		if (animator != null)
		{
			animator.update();
			animator.applyAnimation();
		}
		if (body != null)
			body.getTransform(out position, out rotation);
		for (int i = 0; i < particles.Count; i++)
			particles[i].update(getModelMatrix());
	}

	public virtual void draw(GraphicsDevice graphics)
	{
		if (model != null)
			Renderer.DrawModel(model, getModelMatrix() * modelTransform, animator);
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

	public Matrix getModelMatrix(Vector3 offset)
	{
		return Matrix.CreateTranslation(position + offset) * Matrix.CreateRotation(rotation) * Matrix.CreateScale(scale);
	}

	public Matrix getModelMatrix()
	{
		return getModelMatrix(Vector3.Zero);
	}
}
