using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


public class LadderRegion : Entity
{
	public Vector3 halfExtents;
	public Vector3 offset;

	public readonly Sound[] sfxStep = new Sound[5];


	public LadderRegion(Vector3 halfExtents, Vector3 offset)
	{
		this.halfExtents = halfExtents;
		this.offset = offset;

		sfxStep[0] = Resource.GetSound("entity/object/ladder/sfx/climb1.ogg");
		sfxStep[1] = Resource.GetSound("entity/object/ladder/sfx/climb2.ogg");
		sfxStep[2] = Resource.GetSound("entity/object/ladder/sfx/climb3.ogg");
		sfxStep[3] = Resource.GetSound("entity/object/ladder/sfx/climb4.ogg");
		sfxStep[4] = Resource.GetSound("entity/object/ladder/sfx/climb5.ogg");
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Kinematic, 0, PhysicsFiltering.PLAYER);
		body.addBoxTrigger(halfExtents, offset, Quaternion.Identity);
	}

	public override void destroy()
	{
		body.destroy();
	}

	public override void onContact(RigidBody other, CharacterController otherController, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType)
	{
		if (other != null && other.entity is Player)
		{
			Player player = other.entity as Player;

			if (contactType == ContactType.Found)
			{
				player.controller.initLadder(this);
			}
			else if (contactType == ContactType.Lost)
			{
				player.controller.endLadder();
			}
		}
		if (body != null && body.entity is Player)
		{
			Debug.Assert(false);
		}
	}

	public Vector3 normal
	{
		get { return rotation.back; }
	}
}
