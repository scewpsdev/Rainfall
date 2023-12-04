using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	public interface PhysicsEntity
	{
		void setPosition(Vector3 position);
		Vector3 getPosition();

		void setRotation(Quaternion rotation);
		Quaternion getRotation();

		void onContact(RigidBody other, CharacterController otherController, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType);
	}
}
