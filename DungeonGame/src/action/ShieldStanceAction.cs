using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class ShieldStanceAction : Action
{
	public readonly int handID;
	public readonly Item item;


	public ShieldStanceAction(Item item, int handID, bool twoHanded)
		: base(ActionType.ShieldStance)
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

			parryFramesStartTime = item.parryFramesDelay / 24.0f;
			parryFramesEndTime = item.parryFramesDelay / 24.0f + item.parryFramesCount / 24.0f;
		}
		else if (item.category == ItemCategory.Shield)
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

			animationTransitionDuration = item.blockRaiseDuration;
			duration = 1000.0f;
		}
		else
		{
			Debug.Assert(false);
		}
	}

	public bool isBlocking
	{
		get => elapsedTime >= item.blockRaiseDuration;
	}
}
