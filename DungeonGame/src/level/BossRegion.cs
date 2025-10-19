using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BossRegion : Entity
{
	Vector3 halfExtents;
	RigidBody body;

	Creature boss;


	public BossRegion(Vector3 size, Creature boss)
	{
		halfExtents = 0.5f * size;
		this.boss = boss;
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Static, (uint)PhysicsFilterGroup.EventTriggers, (uint)PhysicsFilterGroup.PlayerControllerKinematicBody);
		body.addBoxTrigger(halfExtents, halfExtents, Quaternion.Identity);
	}

	public override void destroy()
	{
		body.destroy();
	}

	public override void onContact(RigidBody other, CharacterController otherController, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType)
	{
		if (otherController != null && otherController.entity is Player || other != null && other.entity is Player)
		{
			if (contactType == ContactType.Found)
			{
				DungeonGame.instance.gameManager.initiateBoss(boss);
			}
			else if (contactType == ContactType.Lost)
			{
				//DungeonGame.instance.gameManager.terminateBoss();
			}
		}
	}
}
