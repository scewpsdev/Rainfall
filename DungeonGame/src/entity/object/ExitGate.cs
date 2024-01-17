using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class ExitGate : Entity, Activatable, Interactable
{
	const float RAISE_SPEED = 0.4f;
	const float MAX_OPEN_TIME = 15.0f;


	Model model;
	RigidBody barsBody;
	RigidBody exitBody;

	AudioSource audioMechanism;
	AudioSource audioBars;
	Sound sfxRun, sfxStop, sfxFall;

	bool open = false;
	bool openedOnce = false;
	float barsHeight = 0.0f;
	float targetHeight = 0.0f;
	float barsVerticalSpeed = 0.0f;
	float openTimer = 0.0f;

	Lever lever;


	public ExitGate()
	{
		model = Resource.GetModel("res/entity/object/exit/exit.gltf");

		sfxRun = Resource.GetSound("res/entity/object/exit/sfx/run.ogg");
		sfxStop = Resource.GetSound("res/entity/object/exit/sfx/stop.ogg");
		sfxFall = Resource.GetSound("res/entity/object/exit/sfx/fall.ogg");
	}

	public override void init()
	{
		barsBody = new RigidBody(this, RigidBodyType.Kinematic);
		barsBody.addBoxCollider(new Vector3(1.0f, 1.5f, 0.1f), new Vector3(0.0f, 1.5f, -0.1f), Quaternion.Identity);

		exitBody = new RigidBody(this, RigidBodyType.Static, (uint)PhysicsFilterGroup.Default | (uint)PhysicsFilterGroup.Interactable);
		exitBody.addBoxCollider(new Vector3(1.0f, 1.5f, 0.1f), new Vector3(0.0f, 1.5f, -1.9f), Quaternion.Identity);

		audioMechanism = new AudioSource(position + new Vector3(0.0f, 5.0f, 0.0f) + rotation.forward);
		audioBars = new AudioSource(position + rotation.forward);
	}

	public override void destroy()
	{
		audioMechanism.destroy();
		audioBars.destroy();
	}

	public void activate(Entity from)
	{
		open = true;
		targetHeight = 2.2f;

		Debug.Assert(from is Lever);
		lever = from as Lever;

		audioMechanism.playSound(sfxRun);
		audioMechanism.isLooping = true;
	}

	public bool canInteract(Entity by)
	{
		return openedOnce;
	}

	public void interact(Entity by)
	{
		if (by is Player)
		{
			Player player = by as Player;
			player.hasWon = true;
		}
	}

	public override void update()
	{
		if (open)
		{
			openTimer += Time.deltaTime;
			if (openTimer > MAX_OPEN_TIME)
			{
				open = false;
				targetHeight = 0.0f;
				openTimer = 0.0f;
				lever.reset();

				audioBars.playSound(sfxFall);
			}
		}

		if (barsHeight < targetHeight)
		{
			barsHeight += RAISE_SPEED * Time.deltaTime;

			if (barsHeight >= targetHeight)
			{
				barsHeight = targetHeight;
				openedOnce = true;
				audioMechanism.stop();
			}
		}
		else if (barsHeight > targetHeight)
		{
			barsVerticalSpeed += 0.5f * -1.8f * Time.deltaTime;
			barsHeight += barsVerticalSpeed * Time.deltaTime;
			barsVerticalSpeed += 0.5f * -1.8f * Time.deltaTime;

			if (barsHeight <= targetHeight)
			{
				barsHeight = targetHeight;
				barsVerticalSpeed = 0.0f;
				//audioBars.playSound(sfxStop);
			}
		}

		Matrix barsTransform = getModelMatrix() * Matrix.CreateTranslation(0.0f, barsHeight, 0.0f);
		barsBody.setTransform(barsTransform.translation, barsTransform.rotation);

		audioBars.updateTransform(barsTransform.translation + rotation.forward);
	}

	public override void draw(GraphicsDevice graphics)
	{
		Matrix transform = getModelMatrix();
		Renderer.DrawSubModel(model, 0, transform * Matrix.CreateTranslation(0.0f, barsHeight, 0.0f));
		for (int i = 1; i < model.meshCount; i++)
			Renderer.DrawSubModel(model, i, transform);
	}
}
