using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum DoorType
{
	Normal,
	Windowed,
}

internal class Door : Entity, Interactable
{
	Model model;
	Animator animator;
	AnimationState openAnimation;
	AnimationState closeAnimation;
	Node doorNode;

	RigidBody body;

	AudioSource audio;
	Sound sfxOpen, sfxClose;
	bool soundPlayed = false;

	bool isOpen = false;


	public Door(DoorType type)
	{
		if (type == DoorType.Normal)
		{
			model = Resource.GetModel("res/entity/object/door/door.gltf");
		}
		else
		{
			model = Resource.GetModel("res/entity/object/door_windowed/door_windowed.gltf");
		}

		animator = new Animator(model);
		doorNode = model.skeleton.getNode("root");

		animator.setState(new AnimationState(model, "default"));
		openAnimation = new AnimationState(model, "open");
		closeAnimation = new AnimationState(model, "close");

		sfxOpen = Resource.GetSound("res/entity/object/door/sfx/open.ogg");
		sfxClose = Resource.GetSound("res/entity/object/door/sfx/close.ogg");
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Kinematic);
		body.addBoxCollider(new Vector3(0.6f, 1.0f, 0.42f), new Vector3(-0.6f, 1.0f, 0.0f), Quaternion.Identity, 0.0f);
		body.setTransform(position, rotation);

		audio = Audio.CreateSource(position);
	}

	public void open()
	{
		animator.setStateIfNot(openAnimation);
		audio.playSound(sfxOpen);
		isOpen = true;
	}

	public void close()
	{
		animator.setStateIfNot(closeAnimation);
		soundPlayed = false;
		isOpen = false;
	}

	public bool canInteract()
	{
		return true;
	}

	public void interact(Entity by)
	{
		if (by is Player)
		{
			Player player = (Player)by;
			if (player.currentAction == null)
			{
				if (!isOpen)
				{
					open();
					//player.queueAction(new DoorOpenAction(this));
				}
				else
				{
					close();
					//player.queueAction(new DoorCloseAction(this));
				}
			}
		}
	}

	public override void update()
	{
		animator.update();
		animator.applyAnimation();
		//model.applyAnimation(animator.nodeLocalTransforms);

		Matrix lidTransform = getModelMatrix() * Matrix.CreateTranslation(0.6f, 0.0f, 0.0f) * Matrix.CreateRotation(Vector3.Up, animator.getNodeTransform(doorNode, 0).rotation.eulers.y - MathHelper.PiOver2);
		body.setTransform(lidTransform.translation, lidTransform.rotation);

		audio.updateTransform(position);

		if (animator.getState() == closeAnimation && animator.timer >= 18 / 24.0f && !soundPlayed)
		{
			audio.playSound(sfxClose);
			soundPlayed = true;
		}
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawModel(model, getModelMatrix(), animator);
	}
}
