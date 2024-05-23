using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class BlockHitAction : Action
{
	public readonly int handID;
	public readonly Item item;

	public readonly bool wasParry;


	public BlockHitAction(Item item, int handID, float staminaCost, bool wasParry = false)
		: base("block_hit")
	{
		this.handID = handID;
		this.item = item;
		this.wasParry = wasParry;

		if (item.twoHanded)
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

		animationTransitionDuration = 0.0f;

		mirrorAnimation = handID == 1;

		movementSpeedMultiplier = 0.4f;

		this.staminaCost = staminaCost;
		staminaCostTime = 0.0f;

		if (wasParry && item.parrySound != null)
			addSoundEffect(new ActionSfx(item.parrySound, 0.4f, 0.0f, false));
		else if (item.blockSound != null)
			addSoundEffect(new ActionSfx(item.blockSound, 1.0f, 0.0f, true));
	}
}
