using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class GuardBreakAction : Action
{
	public readonly int handID;
	public readonly Item item;

	public readonly bool parry;


	public GuardBreakAction(Item item, int handID, bool twoHanded, bool parry = false)
		: base(ActionType.ShieldGuardBreak)
	{
		this.handID = handID;
		this.item = item;
		this.parry = parry;

		if (twoHanded)
		{
			animationName[0] = "guard_break";
			animationName[1] = "guard_break";
			animationSet[0] = item.moveset;
			animationSet[1] = item.moveset;
		}
		else
		{
			animationName[handID] = "guard_break";
			animationSet[handID] = item.moveset;
		}

		mirrorAnimation = handID == 1;

		movementSpeedMultiplier = 0.2f;

		staminaCost = item.shieldHitStaminaCost;
		staminaCostTime = 0.0f;
	}
}
