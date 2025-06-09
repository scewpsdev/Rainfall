using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Rapier : Weapon
{
	public Rapier()
		: base("rapier")
	{
		displayName = "Rapier";

		baseDamage = 0.9f;
		baseAttackRange = 1.0f;
		baseAttackRate = 2;
		attackCooldown = 2;
		baseWeight = 1.5f;
		secondaryChargeTime = 0.3f;

		dexterityScaling = 0.9f;

		anim = AttackAnim.Stab;
		canParry = true;
		parryWeaponRotation = -0.3f * MathF.PI;
		blockCharge = 0;

		value = 12;

		sprite = new Sprite(tileset, 14, 4);
		renderOffset.x = 0.4f;

		useSound = weaponThrust;
	}
}
