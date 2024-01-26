using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DoubleDoor : Entity, Interactable
{
	Model model;
	Animator animator;
	AnimationState defaultState, openState;

	RigidBody bodyLeft, bodyRight;

	Activatable activatable;
	bool open = false;


	public DoubleDoor(Activatable activatable)
	{
		this.activatable = activatable;

		model = Resource.GetModel("res/entity/object/door/double_door.gltf");
		model.configureLODs(LOD.DISTANCE_MEDIUM);

		animator = new Animator(model);

		defaultState = new AnimationState(model, "default", true);
		openState = new AnimationState(model, "open");
		animator.setState(defaultState);

		bodyLeft = new RigidBody(this, RigidBodyType.Kinematic, (uint)PhysicsFilterGroup.Default | (uint)PhysicsFilterGroup.Interactable);
		bodyLeft.addBoxCollider(new Vector3(0.75f, 1.5f, 0.1f), new Vector3(0.75f, 1.5f, 0.0f), Quaternion.Identity);

		bodyRight = new RigidBody(this, RigidBodyType.Kinematic, (uint)PhysicsFilterGroup.Default | (uint)PhysicsFilterGroup.Interactable);
		bodyRight.addBoxCollider(new Vector3(0.75f, 1.5f, 0.1f), new Vector3(-0.75f, 1.5f, 0.0f), Quaternion.Identity);

		//exitBody = new RigidBody(this, RigidBodyType.Static, (uint)PhysicsFilterGroup.Default | (uint)PhysicsFilterGroup.Interactable);
		//exitBody.addBoxCollider(new Vector3(1.0f, 1.5f, 0.1f), new Vector3(0.0f, 1.5f, -1.9f), Quaternion.Identity);
	}

	public bool canInteract(Entity by)
	{
		return !open;
	}

	public void interact(Entity by)
	{
		open = true;
		if (activatable != null)
		{
			if (activatable is ExitGate)
				((ExitGate)activatable).interact(by);
			else
				activatable.activate(this);
		}
	}

	public override void update()
	{
		if (open)
			animator.setStateIfNot(openState);
		else
			animator.setStateIfNot(defaultState);

		animator.update();
		animator.applyAnimation();

		Matrix transform = getModelMatrix();

		Matrix leftTransform = transform * animator.getNodeLocalTransform(model.skeleton.getNode("hingeLeft"));
		bodyLeft.setTransform(leftTransform.translation, leftTransform.rotation);

		Matrix rightTransform = transform * animator.getNodeLocalTransform(model.skeleton.getNode("hingeRight"));
		bodyRight.setTransform(rightTransform.translation, rightTransform.rotation);
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawModel(model, getModelMatrix(), animator);
	}
}
