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
		stab = false;
		sidewaySwing = false;
		twoHanded = true;
		attackCooldown = 2.5f;
		attackAngleOffset = 0;
		attackAngle = MathF.PI;
		doubleBladed = false;
		baseWeight = 5;

		value = 35;

		sprite = new Sprite(tileset, 10, 4, 2, 1);
		size = new Vector2(2, 1);
		renderOffset.x = 0.7f;
	}
}
