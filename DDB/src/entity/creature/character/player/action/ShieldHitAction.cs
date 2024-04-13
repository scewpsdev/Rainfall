using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class ShieldHitAction : Action
{
	public readonly int handID;
	public readonly Item shield;


	public ShieldHitAction(Item shield, int handID)
		: base(ActionType.ShieldHit, "shield_hit")
	{
		this.handID = handID;
		this.shield = shield;

		animationName[handID] = "shield_hit";
		animationSet[handID] = shield.moveset;

		mirrorAnimation = handID == 1;

		movementSpeedMultiplier = 0.5f;

		staminaCost = shield.shieldHitStaminaCost;
		staminaCostTime = 0.0f;
	}
}
