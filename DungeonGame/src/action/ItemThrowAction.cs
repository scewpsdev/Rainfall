using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class ItemThrowAction : Action
{
	public ItemThrowAction(int handID)
		: base(ActionType.ItemThrow)
	{
		animationName[handID] = "item_throw";
		animationTransitionDuration = 0.0f;
		mirrorAnimation = handID == 1;

		addSoundEffect(Resource.GetSound("res/entity/player/sfx/throw.ogg"), handID, 0.0f, true);
	}
}
