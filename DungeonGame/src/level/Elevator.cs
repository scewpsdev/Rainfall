using Rainfall;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class Elevator : Entity, Activatable
{
	const float ELEVATOR_SPEED = 3.0f;


	Model model;
	Model buttonModel;

	AudioSource audio;
	AudioSource effectAudio;
	Sound sfxMove, sfxStart, sfxStop, sfxButton;

	RigidBody body;

	Vector3 startPoint;
	Vector3 destination;

	bool direction;
	bool moving = false;
	float progress = 1.0f;

	bool buttonDown = false;


	public Elevator(Vector3 startPoint, Vector3 destination, bool startDirection = false)
	{
		this.startPoint = startPoint;
		this.destination = destination;
		direction = startDirection;
		progress = direction ? 0.0f : 1.0f;

		model = Resource.GetModel("res/entity/object/elevator/elevator.gltf");
		buttonModel = Resource.GetModel("res/entity/object/elevator/elevator_button.gltf");

		sfxMove = Resource.GetSound("res/entity/object/elevator/sfx/run.ogg");
		sfxStart = Resource.GetSound("res/entity/object/elevator/sfx/start.ogg");
		sfxStop = Resource.GetSound("res/entity/object/elevator/sfx/stop.ogg");
		sfxButton = Resource.GetSound("res/entity/object/elevator/sfx/button.ogg");
	}

	public override void init()
	{
		if (!direction)
			position = destination;

		body = new RigidBody(this, RigidBodyType.Kinematic);
		body.addMeshColliders(model, Matrix.Identity);

		body.addBoxTrigger(new Vector3(1.0f), Vector3.Zero, Quaternion.Identity);

		audio = Audio.CreateSource(position);
		effectAudio = Audio.CreateSource(position);
	}

	void onStart()
	{
		audio.playSound(sfxMove);
		audio.isLooping = true;

		effectAudio.playSound(sfxStart);
	}

	void onStop()
	{
		audio.stop();

		effectAudio.playSound(sfxStop);
	}

	public void activate(Entity from)
	{
		if (!moving)
		{
			moving = true;
			onStart();
		}
	}

	public override void onContact(RigidBody other, CharacterController otherController, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType)
	{
		if (isTrigger && otherController != null)
		{
			if (contactType == ContactType.Found)
			{
				activate((Entity)otherController.entity);
			}

			buttonDown = contactType == ContactType.Found;
			if (!buttonDown)
			{
				effectAudio.playSound(sfxButton, 0.2f);
			}
		}
	}

	public override void update()
	{
		if (moving)
		{
			float distance = (destination - startPoint).length;

			if (direction)
			{
				float moveSpeedMultiplier = (progress < 0.1f ? MathHelper.Remap(progress, 0.0f, 0.1f, 0.2f, 1.0f) : progress > 0.9f ? MathF.Exp(-(progress - 0.9f) * 10) : 1.0f);
				float moveSpeed = moveSpeedMultiplier / distance * ELEVATOR_SPEED;

				progress += moveSpeed * Time.deltaTime;
				audio.pitch = moveSpeedMultiplier;

				if (progress >= 1.0f)
				{
					progress = 1.0f;
					direction = false;
					moving = false;
					onStop();
				}
			}
			else
			{
				progress = 1.0f - progress;
				float moveSpeedMultiplier = (progress < 0.1f ? MathHelper.Remap(progress, 0.0f, 0.1f, 0.2f, 1.0f) : progress > 0.9f ? MathF.Exp(-(progress - 0.9f) * 10) : 1.0f);
				float moveSpeed = moveSpeedMultiplier / distance * ELEVATOR_SPEED;
				progress = 1.0f - progress;

				progress -= moveSpeed * Time.deltaTime;
				audio.pitch = moveSpeedMultiplier;

				if (progress <= 0.0f)
				{
					progress = 0.0f;
					direction = true;
					moving = false;
					onStop();
				}
			}

			position = Vector3.Lerp(startPoint, destination, progress);
			body.setTransform(position, Quaternion.Identity);

			audio.updateTransform(position - new Vector3(0.0f, 5.0f, 0.0f));
			effectAudio.updateTransform(position - new Vector3(0.0f, 5.0f, 0.0f));
		}
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawModel(model, getModelMatrix());
		Renderer.DrawModel(buttonModel, Matrix.CreateTranslation(0.0f, buttonDown ? -0.1f : 0.0f, 0.0f) * getModelMatrix());
	}
}
