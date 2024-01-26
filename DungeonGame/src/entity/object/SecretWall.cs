using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class SecretWall : Entity, Interactable
{
	const float SLIDE_SPEED = 1.6f;


	protected Model model = null;

	float doorOffset;
	float doorTargetOffset;

	RigidBody doorBody;

	AudioSource audio;
	Sound sfxOpen, sfxClose;

	bool isOpen = false;


	public SecretWall()
	{
		model = Resource.GetModel("res/entity/object/secret_wall/secret_wall.gltf");
		model.configureLODs(LOD.DISTANCE_MEDIUM);

		sfxOpen = Resource.GetSound("res/entity/object/secret_wall/sfx/open.ogg");
		sfxClose = Resource.GetSound("res/entity/object/secret_wall/sfx/close.ogg");
	}

	public override void init()
	{
		doorBody = new RigidBody(this, RigidBodyType.Kinematic, (uint)PhysicsFilterGroup.Default | (uint)PhysicsFilterGroup.Interactable);
		doorBody.addBoxCollider(new Vector3(1.5f, 1.5f, 0.5f), new Vector3(0.0f, 1.5f, 0.0f), Quaternion.Identity, 0.0f);
		doorBody.setTransform(position, rotation);

		audio = new AudioSource(position);
	}

	public override void destroy()
	{
		doorBody.destroy();
		audio.destroy();
	}

	public void open()
	{
		audio.playSoundOrganic(sfxOpen, 1.0f, 1.0f, 0.2f, 0.15f);
		isOpen = true;
	}

	public void close()
	{
		audio.playSoundOrganic(sfxOpen, 1.0f, 1.0f, 0.2f, 0.15f);
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
				doorTargetOffset = 2.5f;
			}
			else
			{
				close();
				doorTargetOffset = 0.0f;
			}
		}
	}

	void onClose()
	{
		audio.playSoundOrganic(sfxClose);
	}

	public override void update()
	{
		if (doorOffset < doorTargetOffset)
		{
			doorOffset = MathF.Min(MathHelper.Lerp(doorOffset, doorTargetOffset + 0.1f, SLIDE_SPEED * Time.deltaTime), doorTargetOffset);
			if (doorOffset == 0.0f)
				onClose();
		}
		else if (doorOffset > doorTargetOffset)
		{
			doorOffset = MathF.Max(MathHelper.Lerp(doorOffset, doorTargetOffset - 0.1f, SLIDE_SPEED * Time.deltaTime), doorTargetOffset);
			if (doorOffset == 0.0f)
				onClose();
		}

		Matrix transform = getModelMatrix();
		Matrix lidTransform = transform * Matrix.CreateTranslation(doorOffset, 0.0f, 0.0f);
		doorBody.setTransform(lidTransform.translation, lidTransform.rotation);

		audio.updateTransform((lidTransform * Matrix.CreateTranslation(-1.0f, 1.0f, 0.0f)).translation);
	}

	public override void draw(GraphicsDevice graphics)
	{
		Matrix transform = getModelMatrix();
		Matrix lidTransform = transform * Matrix.CreateTranslation(doorOffset, 0.0f, 0.0f);

		Renderer.DrawModel(model, lidTransform);
	}
}
