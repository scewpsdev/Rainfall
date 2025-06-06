using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ParryAction : PlayerAction
{
	public Weapon weapon;

	public ParryAction(Weapon weapon, int hand)
		: base("parry", hand)
	{
		this.weapon = weapon;

		animationName[hand] = "parry";
		animationSet[hand] = weapon.moveset;

		if (weapon.twoHanded)
		{
			animationName[hand ^ 1] = "parry";
			animationSet[hand ^ 1] = weapon.moveset;
		}

		mirrorAnimation = hand == 1;

		followUpCancelTime = weapon.parryWindow;

		//lockYaw = true;

		viewmodelAim[hand] = 1;
	}

	public bool inParryWindow => elapsedTime < weapon.parryWindow;
}
