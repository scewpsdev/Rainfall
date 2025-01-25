using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Greathammer : Weapon
{
	public Greathammer()
		: base("greathammer")
	{
		displayName = "Greathammer";

		baseDamage = 3.0f;
		baseAttackRange = 1.8f;
		baseAttackRate = 0.7f;
		knockback = 12;
		anim = AttackAnim.SwingOverhead;
		attackAcceleration = 1;
		twoHanded = true;
		attackCooldown = 2.5f;
		attackStartAngle = MathF.PI;
		attackEndAngle = 0;
		doubleBladed = false;
		baseWeight = 5;

		value = 45;

		sprite = new Sprite(tileset, 10, 4, 2, 1);
		size = new Vector2(2, 1);
		renderOffset.x = 0.7f;
	}
}
