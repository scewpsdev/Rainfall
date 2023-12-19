using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class IronDoor : Entity, Interactable
{
	Model model;
	RigidBody body;

	bool open = false;
	float doorRotation = 0.0f;


	public IronDoor()
	{
		model = Resource.GetModel("res/entity/object/door_iron/door_iron.gltf");
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Kinematic, (uint)PhysicsFilterGroup.Default | (uint)PhysicsFilterGroup.Interactable);
		body.addBoxCollider(new Vector3(0.5f, 1.0f, 0.05f), new Vector3(0.0f, 1.0f, 0.0f), Quaternion.Identity);
	}

	public bool canInteract(Entity by)
	{
		return true;
	}

	public void interact(Entity by)
	{
		open = !open;
	}

	public override void update()
	{
		float dstRotation = open ? MathF.PI * -0.5f : 0.0f;
		doorRotation = MathHelper.Lerp(doorRotation, dstRotation, 3 * Time.deltaTime);
		Matrix doorTransform = getModelMatrix() * Matrix.CreateTranslation(0.5f, 0.0f, 0.0f) * Matrix.CreateRotation(Vector3.Up, doorRotation) * Matrix.CreateTranslation(-0.5f, 0.0f, 0.0f);
		body.setTransform(doorTransform.translation, doorTransform.rotation);
	}

	public override void draw(GraphicsDevice graphics)
	{
		Matrix doorTransform = getModelMatrix() * Matrix.CreateTranslation(0.5f, 0.0f, 0.0f) * Matrix.CreateRotation(Vector3.Up, doorRotation) * Matrix.CreateTranslation(-0.5f, 0.0f, 0.0f);
		Renderer.DrawModelStaticInstanced(model, doorTransform);
	}
}
