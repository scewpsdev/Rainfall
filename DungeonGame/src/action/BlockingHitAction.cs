using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class BlockingHitAction : Action
{
	public readonly int handID;
	public readonly Item item;

	public readonly bool parry;


	public BlockingHitAction(Item item, int handID, bool twoHanded, bool parry = false)
		: base(ActionType.BlockingHit)
	{
		this.handID = handID;
		this.item = item;
		this.parry = parry;

		if (twoHanded)
		{
			animationName[0] = "block_hit";
			animationName[1] = "block_hit";
			animationSet[0] = item.moveset;
			animationSet[1] = item.moveset;
		}
		else
		{
			animationName[handID] = "block_hit";
			animationSet[handID] = item.moveset;
		}

		mirrorAnimation = handID == 1;

		movementSpeedMultiplier = 0.4f;

		staminaCost = item.shieldHitStaminaCost;
		staminaCostTime = 0.0f;
	}
}
