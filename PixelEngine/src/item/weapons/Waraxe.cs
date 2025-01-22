using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Waraxe : Weapon
{
	public Waraxe()
		: base("waraxe")
	{
		displayName = "Waraxe";

		baseDamage = 1.8f;
		baseAttackRange = 1.5f;
		baseAttackRate = 1.0f;
		anim = AttackAnim.SwingOverhead;
		attackAcceleration = 1;
		baseWeight = 2.5f;
		doubleBladed = false;

		value = 19;

		sprite = new Sprite(tileset, 8, 7, 2, 1);
		size = new Vector2(2, 1);
		renderOffset.x = 0.4f;
	}
}
