using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class EventTrigger : Entity
{
	Vector3 size, offset;
	Action<RigidBody> callback;

	public EventTrigger(Vector3 size, Vector3 offset, Action<RigidBody> callback)
	{
		this.size = size;
		this.offset = offset;
		this.callback = callback;
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Kinematic);
		body.addBoxTrigger(0.5f * size, offset, Quaternion.Identity);
	}

	public override void onContact(RigidBody other, CharacterController otherController, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType)
	{
		if (other != null)
			callback(other);
	}
}
