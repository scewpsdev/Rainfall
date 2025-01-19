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
		stab = false;
		sidewaySwing = false;
		twoHanded = true;
		attackCooldown = 2.5f;
		attackAngleOffset = 0;
		attackAngle = MathF.PI;
		doubleBladed = false;
		baseWeight = 2;

		value = 20;

		sprite = new Sprite(tileset, 1, 4, 2, 1);
		icon = new Sprite(tileset.texture, 1 * 16 + 12, 4 * 16, 16, 16);
		size = new Vector2(2, 1);
		renderOffset.x = 0.2f;

		hitSound = woodHit;
	}
}
