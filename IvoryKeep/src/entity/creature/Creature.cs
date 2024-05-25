using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract class Creature : Entity, Hittable
{
	public readonly EntityType type;

	public float pitch, yaw;

	public Node rootMotionNode;

	public Sound[] stepSound;
	public Sound[] landSound;

	public Sound[] hitSound;


	public Creature(EntityType type)
	{
		this.type = type;

		if (type.entityData != null)
		{
			bodyFilterGroup = PhysicsFiltering.CREATURE;
			bodyFilterMask = PhysicsFiltering.DEFAULT | PhysicsFiltering.CREATURE;
			bodyFriction = 0.0f;
			hitboxFilterGroup = PhysicsFiltering.CREATURE_HITBOX;
			hitboxFilterMask = 0;
			EntityLoader.CreateEntityFromData(type.entityData.Value, type.entityDataPath, this);
		}

		stepSound = type.stepSound;
		landSound = type.landSound;

		hitSound = type.hitSound;
	}

	public override void init()
	{
		base.init();

		if (model != null)
			rootMotionNode = model.skeleton.getNode("Root");
	}

	public abstract bool hit(float damage, float poiseDamage, Entity from, Item item, Vector3 hitPosition, Vector3 hitDirection, RigidBody body);

	public void hit(float damage, Entity from = null, RigidBody body = null)
	{
		hit(damage, damage, from, null, position + Vector3.Up, Vector3.Zero, body);
	}

	public abstract bool isAlive();

	protected void onDeath()
	{
		if (body != null)
		{
			body.destroy();
			body = null;
		}
		if (hitboxes != null)
		{
			foreach (string nodeName in hitboxes.Keys)
			{
				hitboxes[nodeName].destroy();
				hitboxes.Remove(nodeName);
			}
			hitboxes = null;
		}
	}
}
