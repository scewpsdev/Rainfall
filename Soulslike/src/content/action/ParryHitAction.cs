using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ParryHitAction : PlayerAction
{
	public Weapon weapon;

	public ParryHitAction(Weapon weapon, int hand)
		: base("parry_hit", hand)
	{
		this.weapon = weapon;

		animationName[hand] = "parry_hit";
		animationSet[hand] = weapon.moveset;

		if (weapon.twoHanded)
		{
			animationName[hand ^ 1] = "parry_hit";
			animationSet[hand ^ 1] = weapon.moveset;
		}

		mirrorAnimation = hand == 1;

		animationTransitionDuration = 1 / 24.0f;
		followUpCancelTime = weapon.parryWindow * 2;

		viewmodelAim = 1;

		addSoundEffect(new ActionSfx(Resource.GetSound("sound/item/parry.ogg")));
	}
}
