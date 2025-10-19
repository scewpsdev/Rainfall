using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Handaxe : Weapon
{
	public Handaxe()
		: base("handaxe")
	{
		displayName = "Handaxe";

		baseDamage = 1.4f;
		baseAttackRange = 1.0f;
		baseAttackRate = 2.0f;

		projectileItem = true;
		projectileSpins = true;
		projectileSticks = true;
		doubleBladed = false;
		secondaryChargeTime = 0;
		anim = AttackAnim.SwingSideways;

		strengthScaling = 0.3f;
		dexterityScaling = 0.3f;

		baseValue = 5;

		sprite = new Sprite(tileset, 15, 4);
		renderOffset.x = 0.2f;
	}

	public override bool useSecondary(Player player)
	{
		throwWeapon(player);
		return true;
	}
}
