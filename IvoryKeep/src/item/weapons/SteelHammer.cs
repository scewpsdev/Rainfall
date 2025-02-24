using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SteelHammer : Weapon
{
	public SteelHammer()
		: base("steel_hammer")
	{
		displayName = "Steel Hammer";

		baseDamage = 1.6f;
		baseAttackRange = 1.0f;
		baseAttackRate = 1.2f;
		anim = AttackAnim.SwingOverhead;
		attackAcceleration = 1;
		attackCooldown = 2.5f;
		attackStartAngle = MathF.PI;
		attackEndAngle = 0;
		doubleBladed = false;
		baseWeight = 2;

		strengthScaling = 0.8f;

		value = 28;

		sprite = new Sprite(tileset, 0, 11);
		renderOffset.x = 0.2f;
	}
}
