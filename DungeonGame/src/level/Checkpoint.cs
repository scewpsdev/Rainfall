using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class Checkpoint : Entity
{
	Vector3 halfExtents;
	RigidBody body;


	public Checkpoint(Vector3 halfExtents)
	{
		this.halfExtents = halfExtents;
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Static);
		body.addBoxTrigger(halfExtents);
	}

	public override void onContact(RigidBody other, CharacterController otherController, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType)
	{
		if (otherController != null && otherController.entity is Player)
		{
			Player player = otherController.entity as Player;
			if (contactType == ContactType.Found || contactType == ContactType.Lost)
			{
				player.resetPoint = player.position;
				Console.WriteLine("checkpoint " + player.resetPoint);
			}
		}
	}
}
