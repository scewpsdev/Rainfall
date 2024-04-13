using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class ReverbZone : Entity
{
	Vector3 halfExtents;
	RigidBody body;


	public ReverbZone(Vector3 halfExtents)
	{
		this.halfExtents = halfExtents;
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Static);
		body.addBoxTrigger(halfExtents);
	}

	public override void onContact(RigidBody other, CharacterController otherController, ContactType contactType, bool trigger)
	{
		if (otherController != null && otherController.entity is Player)
		{
			if (contactType == ContactType.Found)
			{
				AudioManager.SetReverb(true);
			}
			else if (contactType == ContactType.Lost)
			{
				AudioManager.SetReverb(false);
			}
		}
	}
}
