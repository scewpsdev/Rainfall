using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CreatureRagdoll : Entity
{
	Ragdoll ragdoll;


	public CreatureRagdoll(Creature creature, Vector3 knockback, RigidBody hitbox)
	{
		setTransform(creature.getModelMatrix());

		model = creature.model;
		animator = creature.animator;

		RigidBody getRagdollBodyForHitbox(RigidBody hitbox)
		{
			foreach (string nodeName in creature.hitboxes.Keys)
			{
				if (creature.hitboxes[nodeName] == hitbox)
				{
					return ragdoll.getHitboxForNode(model.skeleton.getNode(nodeName));
				}
			}
			return null;
		}

		if (creature.hitboxes != null)
		{
			if (model != null && model.isAnimated && animator != null && creature.hitboxData != null)
			{
				//Vector3 localVelocity = Quaternion.FromAxisAngle(Vector3.Up, creature.yaw).conjugated * creature.velocity;
				ragdoll = new Ragdoll(this, model.skeleton.getNode("spine"), animator, getModelMatrix(), creature.velocity, creature.hitboxData, PhysicsFilter.Ragdoll, PhysicsFilter.Default);
				RigidBody hitBody = getRagdollBodyForHitbox(hitbox);
				if (hitBody != null)
				{
					hitBody.addForce(knockback);
				}
			}
		}
	}

	public override void update()
	{
		ragdoll.update();
		ragdoll.getTransform().decompose(out position, out rotation);
	}
}
