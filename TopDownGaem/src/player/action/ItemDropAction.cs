using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class ItemDropAction : Action
{
	public ItemDropAction(int handID)
		: base("item_drop")
	{
		animationName[handID] = "item_drop";
		animationTransitionDuration = 0.0f;
		mirrorAnimation = handID == 1;

		addSoundEffect(new ActionSfx(Resource.GetSound("res/item/sfx/throw.ogg"), 1.0f, 0.0f, true));
	}
}
