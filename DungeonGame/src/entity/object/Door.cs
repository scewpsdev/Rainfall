﻿using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract class Door : Entity, Interactable
{
	protected Model model;
	protected Model frame;
	protected float doorHingeOffset = 0.5f;
	protected float swingSpeed = MathF.PI * 0.6f * 3;

	protected RigidBody doorBody;
	protected RigidBody frameBody;

	AudioSource audio;
	protected Sound sfxOpen, sfxClose, sfxLocked;

	public Item requiredKey = null;
	public int lockedSide = 0;

	bool isOpen = false;
	float doorAngle;
	float doorTargetAngle;


	public override void init()
	{
		doorBody = new RigidBody(this, RigidBodyType.Kinematic, (uint)PhysicsFilterGroup.Default | (uint)PhysicsFilterGroup.Interactable);
		frameBody = new RigidBody(this, RigidBodyType.Static);

		audio = new AudioSource(position + new Vector3(0.0f, 1.0f, 0.0f));

		sfxLocked = Resource.GetSound("res/entity/object/door_iron/sfx/locked.ogg");
	}

	public override void destroy()
	{
		model?.destroy();
		frame?.destroy();

		doorBody.destroy();
		frameBody.destroy();

		audio.destroy();
	}

	public void open()
	{
		if (sfxOpen != null)
		{
			audio.playSoundOrganic(sfxOpen, 0.5f);
			AIManager.NotifySound(position, 5.0f);
		}
		isOpen = true;
	}

	public void close()
	{
		isOpen = false;
	}

	public bool canInteract(Entity by)
	{
		return true;
	}

	public void interact(Entity by)
	{
		if (by is Player)
		{
			Player player = by as Player;
			if (!isOpen)
			{
				bool canOpen = true;
				if (requiredKey != null)
				{
					if (player.inventory.findSlot(requiredKey) == null)
						canOpen = false;
				}
				else if (lockedSide != 0)
				{
					Vector3 toPlayer = player.position - position;
					int playerDirection = Math.Sign(Vector3.Dot(rotation.forward, toPlayer));
					if (lockedSide != playerDirection)
						canOpen = false;
				}

				if (canOpen)
				{
					open();
					float direction = MathF.Sign(Vector3.Dot(by.position - position, rotation.forward));
					doorTargetAngle = direction * MathF.PI * 0.5f;
					if (lockedSide != 0)
						lockedSide = 0;
				}
				else
				{
					audio.playSoundOrganic(sfxLocked, 2.0f);
					if (lockedSide != 0)
						player.hud.showMessage("Does not open from this side");
					else if (requiredKey != null)
						player.hud.showMessage("Locked");
				}
			}
			else
			{
				close();
				doorTargetAngle = 0.0f;
			}
		}
	}

	void onClose()
	{
		if (sfxClose != null)
		{
			audio.playSoundOrganic(sfxClose, 0.5f);
			AIManager.NotifySound(position, 6.0f);
		}
	}

	public override void update()
	{
		if (doorAngle < doorTargetAngle)
		{
			doorAngle = MathF.Min(MathHelper.Lerp(doorAngle, doorTargetAngle + 0.1f, swingSpeed * Time.deltaTime), doorTargetAngle);
			if (doorAngle == 0.0f)
				onClose();
		}
		else if (doorAngle > doorTargetAngle)
		{
			doorAngle = MathF.Max(MathHelper.Lerp(doorAngle, doorTargetAngle - 0.1f, swingSpeed * Time.deltaTime), doorTargetAngle);
			if (doorAngle == 0.0f)
				onClose();
		}

		Matrix transform = getModelMatrix();
		Matrix lidTransform = transform * Matrix.CreateTranslation(doorHingeOffset, 0.0f, 0.0f) * Matrix.CreateRotation(Vector3.Up, doorAngle);
		doorBody.setTransform(lidTransform.translation, lidTransform.rotation);

		audio.updateTransform(position);
	}

	public override void draw(GraphicsDevice graphics)
	{
		Matrix transform = getModelMatrix();
		Matrix lidTransform = transform * Matrix.CreateTranslation(doorHingeOffset, 0.0f, 0.0f) * Matrix.CreateRotation(Vector3.Up, doorAngle) * Matrix.CreateTranslation(-doorHingeOffset, 0.0f, 0.0f);

		Renderer.DrawModel(model, lidTransform);
		if (frame != null)
			Renderer.DrawModel(frame, transform);
	}
}
