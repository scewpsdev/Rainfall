using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class EnvirTrigger : Entity
{
	Cubemap cubemap;

	public EnvirTrigger(Cubemap cubemap)
	{
		this.cubemap = cubemap;
	}

	public override void onContact(RigidBody other, CharacterController otherController, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType)
	{
		if (other != null && other.entity is Player)
		{
			Player player = other.entity as Player;
			if (contactType == ContactType.Found)
				onActivate();
			else
				onDeactivate();
		}
	}

	void onActivate()
	{
		GameState.instance.world.pushSkybox(cubemap);
	}

	void onDeactivate()
	{
		GameState.instance.world.popSkybox(cubemap);
	}
}
