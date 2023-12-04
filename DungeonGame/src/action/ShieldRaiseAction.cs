using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class ShieldRaiseAction : Action
{
	public readonly int handID;
	public readonly Item item;


	public ShieldRaiseAction(Item item, int handID, bool twoHanded)
		: base(ActionType.ShieldRaise)
	{
		this.handID = handID;
		this.item = item;

		if (item.category == ItemCategory.Weapon)
		{
			if (twoHanded)
			{
				animationName[0] = "block_stance";
				animationName[1] = "block_stance";
				animationSet[0] = item.moveset;
				animationSet[1] = item.moveset;
			}
			else
			{
				animationName[handID] = "block_stance";
				animationSet[handID] = item.moveset;
			}

			mirrorAnimation = handID == 1;

			animationTransitionDuration = item.parryFramesDelay / 24.0f;
			duration = 1000.0f;

			parryFramesStartTime = (item.parryFramesDelay) / 24.0f;
			parryFramesEndTime = (item.parryFramesDelay + item.parryFramesCount) / 24.0f;
		}
		else if (item.category == ItemCategory.Shield)
		{
			if (twoHanded)
			{
				animationName[0] = "block_raise";
				animationName[1] = "block_raise";
				animationSet[0] = item.moveset;
				animationSet[1] = item.moveset;
			}
			else
			{
				animationName[handID] = "block_raise";
				animationSet[handID] = item.moveset;
			}

			mirrorAnimation = handID == 1;

			duration = 1000.0f;
		}
		else
		{
			Debug.Assert(false);
		}
	}
}
