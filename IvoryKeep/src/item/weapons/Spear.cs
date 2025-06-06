using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Spear : Weapon
{
	public Spear()
		: base("spear")
	{
		displayName = "Spear";

		baseDamage = 1.1f;
		baseAttackRange = 1.5f;
		baseAttackRate = 2.0f;
		baseWeight = 1.5f;

		strengthScaling = 0.1f;
		dexterityScaling = 0.4f;

		anim = AttackAnim.Stab;

		projectileItem = true;
		projectileAims = true;
		projectileSticks = true;
		secondaryChargeTime = 0;

		value = 12;

		sprite = new Sprite(tileset, 6, 1, 2, 1);
		size = new Vector2(2, 1);
		renderOffset.x = 0.2f;

		useSound = weaponThrust;
	}

	public override bool useSecondary(Player player)
	{
		player.throwItem(this, player.lookDirection.normalized, 25);
		return true;
	}
}
