using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class ItemPickUpAction : Action
{
	public ItemPickUpAction()
		: base(ActionType.ItemPickUp, "pick_up")
	{
		animationName[0] = "pick_up";

		overrideHandModels[0] = true;

		movementSpeedMultiplier = 0.7f;

		addSoundEffect(Resource.GetSound("res/entity/player/sfx/take.ogg"), -1, 0.0f, true);
	}
}
