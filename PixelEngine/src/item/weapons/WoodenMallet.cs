using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WoodenMallet : Weapon
{
	public WoodenMallet()
		: base("wooden_mallet")
	{
		displayName = "Wooden Mallet";

		baseDamage = 2.25f;
		baseAttackRange = 1.2f;
		baseAttackRate = 1.1f;
		anim = AttackAnim.SwingOverhead;
		attackAcceleration = 1;
		twoHanded = true;
		attackCooldown = 2.5f;
		attackStartAngle = MathF.PI;
		attackEndAngle = 0;
		doubleBladed = false;
		baseWeight = 2;

		value = 20;

		sprite = new Sprite(tileset, 1, 4, 2, 1);
		icon = new Sprite(tileset.texture, 1 * 16 + 16, 4 * 16, 16, 16);
		size = new Vector2(2, 1);
		renderOffset.x = 0.0f;

		hitSound = woodHit;
	}
}
