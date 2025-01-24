using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Greataxe : Weapon
{
	public Greataxe()
		: base("greataxe")
	{
		displayName = "Greataxe";

		baseDamage = 2.8f;
		baseAttackRange = 1.8f;
		baseAttackRate = 0.8f;
		knockback = 8;
		anim = AttackAnim.SwingOverhead;
		attackAcceleration = 1;
		twoHanded = true;
		doubleBladed = true;
		attackCooldown = 1.0f;
		baseWeight = 5;

		value = 47;

		sprite = new Sprite(tileset, 0, 5, 2, 1);
		size = new Vector2(2, 1);
		renderOffset.x = 0.5f;
	}
}
