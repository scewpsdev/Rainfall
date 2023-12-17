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
	const float SWING_SPEED = MathF.PI * 0.6f * 3;
	const float DOOR_HINGE_OFFSET = 0.75f;


	DoorType type;
	Model model;
	int doorMeshIdx;

	float doorAngle;
	float doorTargetAngle;

	RigidBody frameBody;
	RigidBody doorBody;

	AudioSource audio;
	Sound sfxOpen, sfxClose;

	bool isOpen = false;


	public Door(DoorType type)
	{
		this.type = type;

		if (type == DoorType.Normal)
		{
			model = Resource.GetModel("res/entity/object/door/door.gltf");
			doorMeshIdx = 0;
		}
		else
		{
			model = Resource.GetModel("res/entity/object/door/door_windowed.gltf");
			doorMeshIdx = 0;
		}

		sfxOpen = Resource.GetSound("res/entity/object/door/sfx/open.ogg");
		sfxClose = Resource.GetSound("res/entity/object/door/sfx/close.ogg");
	}

	public override void init()
	{
		frameBody = new RigidBody(this, RigidBodyType.Static);
		frameBody.addBoxCollider(new Vector3(0.4f, 1.5f, 0.2f), new Vector3(-1.1f, 1.5f, 0.0f), Quaternion.Identity);
		frameBody.addBoxCollider(new Vector3(0.4f, 1.5f, 0.2f), new Vector3(1.1f, 1.5f, 0.0f), Quaternion.Identity);
		frameBody.addBoxCollider(new Vector3(0.7f, 0.35f, 0.2f), new Vector3(0.0f, 2.65f, 0.0f), Quaternion.Identity);

		if (type == DoorType.Normal)
		{
			doorBody = new RigidBody(this, RigidBodyType.Kinematic, (uint)PhysicsFilterGroup.Default | (uint)PhysicsFilterGroup.Interactable);
			doorBody.addBoxCollider(new Vector3(0.7f, 1.15f, 0.1f), new Vector3(-DOOR_HINGE_OFFSET, 1.15f, 0.0f), Quaternion.Identity, 0.0f);
			doorBody.setTransform(position, rotation);
		}
		else if (type == DoorType.Windowed)
		{
			doorBody = new RigidBody(this, RigidBodyType.Kinematic, (uint)PhysicsFilterGroup.Default | (uint)PhysicsFilterGroup.Interactable);
			doorBody.addBoxCollider(new Vector3(0.7f, 0.7f, 0.1f), new Vector3(-DOOR_HINGE_OFFSET, 0.7f, 0.0f), Quaternion.Identity, 0.0f);
			doorBody.addBoxCollider(new Vector3(0.7f, 0.25f, 0.1f), new Vector3(-DOOR_HINGE_OFFSET, 2.05f, 0.0f), Quaternion.Identity, 0.0f);
			doorBody.addBoxCollider(new Vector3(0.2f, 0.2f, 0.1f), new Vector3(-DOOR_HINGE_OFFSET - 0.5f, 1.6f, 0.0f), Quaternion.Identity, 0.0f);
			doorBody.addBoxCollider(new Vector3(0.2f, 0.2f, 0.1f), new Vector3(-DOOR_HINGE_OFFSET + 0.5f, 1.6f, 0.0f), Quaternion.Identity, 0.0f);
			doorBody.setTransform(position, rotation);
		}
		else
		{
			Debug.Assert(false);
		}

		audio = Audio.CreateSource(position);
	}

	public void open()
	{
		audio.playSoundOrganic(sfxOpen);
		AIManager.NotifySound(position, 5.0f);
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
			if (!isOpen)
			{
				open();
				float direction = MathF.Sign(Vector3.Dot(by.position - position, rotation.forward));
				doorTargetAngle = direction * MathF.PI * 0.5f;
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
		audio.playSoundOrganic(sfxClose);
		AIManager.NotifySound(position, 6.0f);
	}

	public override void update()
	{
		if (doorAngle < doorTargetAngle)
		{
			doorAngle = MathF.Min(MathHelper.Lerp(doorAngle, doorTargetAngle + 0.1f, SWING_SPEED * Time.deltaTime), doorTargetAngle);
			if (doorAngle == 0.0f)
				onClose();
		}
		else if (doorAngle > doorTargetAngle)
		{
			doorAngle = MathF.Max(MathHelper.Lerp(doorAngle, doorTargetAngle - 0.1f, SWING_SPEED * Time.deltaTime), doorTargetAngle);
			if (doorAngle == 0.0f)
				onClose();
		}

		Matrix transform = getModelMatrix();
		Matrix lidTransform = transform * Matrix.CreateTranslation(DOOR_HINGE_OFFSET, 0.0f, 0.0f) * Matrix.CreateRotation(Vector3.Up, doorAngle);
		doorBody.setTransform(lidTransform.translation, lidTransform.rotation);

		audio.updateTransform(position);
	}

	public override void draw(GraphicsDevice graphics)
	{
		Matrix transform = getModelMatrix();
		Matrix lidTransform = transform * Matrix.CreateTranslation(DOOR_HINGE_OFFSET, 0.0f, 0.0f) * Matrix.CreateRotation(Vector3.Up, doorAngle) * Matrix.CreateTranslation(-DOOR_HINGE_OFFSET, 0.0f, 0.0f);

		Renderer.DrawSubModel(model, doorMeshIdx ^ 1, transform);
		Renderer.DrawSubModel(model, doorMeshIdx, lidTransform);
	}
}
