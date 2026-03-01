using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AssassinsDagger : Weapon
{
	public AssassinsDagger()
		: base("assassins_dagger")
	{
		displayName = "Assassins Dagger";

		baseDamage = 0.6f;
		baseAttackRange = 1.0f;
		baseAttackRate = 3;
		//attackCooldown = 2.0f;
		//canDrop = false;
		//stab = false;
		baseWeight = 1;
		bleed = 0.8f;
		anim = AttackAnim.Stab;

		isSecondaryItem = true;
		//attackRotationOffset = MathF.PI * 0.25f;

		baseValue = 29;

		sprite = new Sprite(tileset, 5, 10);
		renderOffset.x = 0.1f;

		useSound = Resource.GetSounds("sounds/swing_dagger", 4);
	}
}
