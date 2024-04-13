using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class WeaponDrawAction : Action
{
	public WeaponDrawAction(Item item, int handID)
		: base(ActionType.WeaponDraw, "weapon_draw")
	{
		if (item.twoHanded)
		{
			animationName[0] = "draw";
			animationName[1] = "draw";
			animationSet[0] = item.moveset;
			animationSet[1] = item.moveset;
		}
		else
		{
			animationName[handID] = "draw";
			animationSet[handID] = item.moveset;
		}

		mirrorAnimation = handID == 1;

		animationTransitionDuration = 0.0f;

		addSoundEffect(item.sfxDraw, handID, 0.0f, false);
	}
}
