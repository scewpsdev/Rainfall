using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class Bob : Creature
{
	public Bob()
	{
		model = Resource.GetModel("res/entity/creature/bob/bob.gltf");
		rootNode = model.skeleton.getNode("Root");

		animator = new Animator(model);
		idleState = new AnimationState(model, "idle", true, 0.2f);
		runState = new AnimationState(model, "run", true, 0.2f);
		deadState = new AnimationState(model, "death", false, 0.2f);
		actionState1 = new AnimationState(model, "default", false, 0.2f);
		actionState2 = new AnimationState(model, "default", false, 0.2f);
		animator.setState(idleState);

		stats.maxHealth = 100;
		stats.health = 100;

		name = "Bob";

		//ragdollColliders.Add("Hand.L", Vector3.Zero);
		//ragdollColliders.Add("Hand.R", Vector3.Zero);
		hitboxData.Add("Hip", new BoneHitbox(new Vector2(0.1f, 0.06f)));
		hitboxData.Add("Chest", new BoneHitbox(new Vector2(0.12f, 0.07f)));
		hitboxData.Add("Neck", new BoneHitbox(0.1f, 0.0f, -0.2f));
		//hitboxData.Add("Shoulder.R", new BoneHitbox(0.05f));
		//hitboxData.Add("Shoulder.L", new BoneHitbox(0.05f));
		hitboxData.Add("Upper Arm.R", new BoneHitbox(0.05f, 0.1f));
		hitboxData.Add("Upper Arm.L", new BoneHitbox(0.05f, 0.1f));
		hitboxData.Add("Lower Arm.R", new BoneHitbox(0.04f));
		hitboxData.Add("Lower Arm.L", new BoneHitbox(0.04f));
		hitboxData.Add("Hand.R", new BoneHitbox(0.05f, 0.1f));
		hitboxData.Add("Hand.L", new BoneHitbox(0.05f, 0.1f));
		hitboxData.Add("Leg_Upper.R", new BoneHitbox(0.06f));
		hitboxData.Add("Leg_Upper.L", new BoneHitbox(0.06f));
		hitboxData.Add("Leg_Lower.R", new BoneHitbox(0.05f));
		hitboxData.Add("Leg_Lower.L", new BoneHitbox(0.05f));
	}

	public override void init()
	{
		base.init();

		movementBody.addCapsuleCollider(0.3f, 2.0f, new Vector3(0.0f, 1.0f, 0.0f), Quaternion.Identity, 0.0f);
	}

	public override void destroy()
	{
		base.destroy();
	}
}
