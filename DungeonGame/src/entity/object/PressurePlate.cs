using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class PressurePlate : Entity
{
	Model model;

	Sound sfxButton;

	RigidBody body;

	AudioSource audio;

	Activatable activatable;
	bool pressed = false;


	public PressurePlate(Activatable activatable)
	{
		this.activatable = activatable;

		model = Resource.GetModel("res/entity/object/pressure_plate/pressure_plate.gltf");
		model.configureLODs(LOD.DISTANCE_SMALL);

		sfxButton = Resource.GetSound("res/entity/object/elevator/sfx/button.ogg");
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Kinematic);
		body.addBoxTrigger(new Vector3(1.0f), Vector3.Zero, Quaternion.Identity);

		audio = new AudioSource(position);
	}

	public override void destroy()
	{
		body.destroy();
		audio.destroy();
	}

	public override void onContact(RigidBody other, CharacterController otherController, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType)
	{
		if (isTrigger && (otherController != null || other != null && other.entity is Creature))
		{
			if (contactType == ContactType.Found)
			{
				Entity entity = (Entity)(otherController != null ? otherController.entity : other != null ? other.entity : null);
				activatable.activate(entity);
			}

			pressed = contactType == ContactType.Found;
			if (!pressed)
			{
				audio.playSound(sfxButton, 0.2f);
			}
		}
	}

	public override void update()
	{
		body.setTransform(position, Quaternion.Identity);
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawModel(model, Matrix.CreateTranslation(0.0f, pressed ? -0.1f : 0.0f, 0.0f) * getModelMatrix());
	}
}
