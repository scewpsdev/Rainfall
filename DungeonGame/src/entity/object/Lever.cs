using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;


internal class Lever : Entity, Interactable
{
	const float PULL_SPEED = 2.0f;
	const float RESET_SPEED = 4.0f;


	Model model;
	RigidBody body;

	AudioSource audio;
	Sound sfxPull;

	float angle;

	Activatable activatable;
	bool activated = false;
	bool canBeActivated = true;

	Entity pulledBy;


	public Lever(Activatable activatable)
	{
		this.activatable = activatable;

		model = Resource.GetModel("res/entity/object/lever/lever.gltf");
		model.configureLODs(LOD.DISTANCE_SMALL);

		sfxPull = Resource.GetSound("res/entity/object/lever/sfx/pull.ogg");
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Static, (uint)PhysicsFilterGroup.Default | (uint)PhysicsFilterGroup.Interactable);
		body.addBoxCollider(new Vector3(0.15f, 0.25f, 0.075f), new Vector3(0.0f, 0.0f, 0.075f), Quaternion.Identity);

		audio = new AudioSource(position);

		angle = MathF.PI * -0.25f;
	}

	public override void destroy()
	{
		body.destroy();
		audio.destroy();
	}

	public bool canInteract(Entity by)
	{
		return !activated && canBeActivated;
	}

	public void interact(Entity by)
	{
		audio.playSound(sfxPull, 0.2f);
		activated = true;
		pulledBy = by;
	}

	public void reset()
	{
		activated = false;
		pulledBy = null;
		canBeActivated = false;
	}

	public override void update()
	{
		if (activated)
		{
			if (angle < MathF.PI * 0.25f)
			{
				angle += PULL_SPEED * Time.deltaTime;
				if (angle >= MathF.PI * 0.25f)
				{
					angle = MathF.PI * 0.25f;
					activatable.activate(this);
				}
			}
		}
		else
		{
			if (angle > MathF.PI * -0.25f)
			{
				angle -= RESET_SPEED * Time.deltaTime;
				if (angle <= MathF.PI * -0.25f)
				{
					angle = MathF.PI * -0.25f;
					canBeActivated = true;
				}
			}
		}
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawSubModel(model, 0, getModelMatrix());
		Renderer.DrawSubModel(model, 1, getModelMatrix() * Matrix.CreateRotation(Vector3.Right, angle));
	}
}
