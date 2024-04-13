using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class ShieldRaiseAction : Action
{
	public readonly int handID;
	public readonly Item shield;


	public ShieldRaiseAction(Item shield, int handID)
		: base(ActionType.ShieldRaise, "shield_raise")
	{
		this.handID = handID;
		this.shield = shield;

		animationName[handID] = "shield_raise";
		animationSet[handID] = shield.moveset;

		mirrorAnimation = handID == 1;

		duration = 1000.0f;
	}
}
