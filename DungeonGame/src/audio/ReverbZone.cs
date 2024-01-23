using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class ReverbZone : Entity
{
	Vector3 halfExtents;
	bool value;

	RigidBody body;


	public ReverbZone(Vector3 size, bool value)
	{
		halfExtents = 0.5f * size;
		this.value = value;
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Static);
		body.addBoxTrigger(halfExtents, halfExtents, Quaternion.Identity);
	}

	public override void onContact(RigidBody other, CharacterController otherController, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType)
	{
		if (otherController != null && otherController.entity is Player)
		{
			if (contactType == ContactType.Found)
			{
				AudioManager.SetReverb(value);
			}
			else if (contactType == ContactType.Lost)
			{
				AudioManager.SetReverb(!value);
			}
		}
	}
}
