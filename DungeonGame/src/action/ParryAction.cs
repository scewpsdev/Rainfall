using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class ParryAction : Action
{
	public readonly int handID;
	public readonly Item item;


	public ParryAction(Item item, int handID, bool twoHanded)
		: base(ActionType.Parry)
	{
		this.handID = handID;
		this.item = item;

		if (item.category == ItemCategory.Weapon)
		{
			if (twoHanded)
			{
				animationName[0] = "parry_stance";
				animationName[1] = "parry_stance";
				animationSet[0] = item.moveset;
				animationSet[1] = item.moveset;
			}
			else
			{
				animationName[handID] = "parry_stance";
				animationSet[handID] = item.moveset;
			}

			mirrorAnimation = handID == 1;

			animationTransitionDuration = item.parryFramesDelay / 24.0f;
			duration = (item.parryFramesDelay + item.parryFramesCount) / 24.0f;
		}
		else if (item.category == ItemCategory.Shield)
		{
			animationName[handID] = "parry";
			animationSet[handID] = item.moveset;

			mirrorAnimation = handID == 1;
		}
		else
		{
			Debug.Assert(false);
		}

		parryFramesStartTime = item.parryFramesDelay / 24.0f;
		parryFramesEndTime = (item.parryFramesDelay + item.parryFramesCount) / 24.0f;
	}
}
