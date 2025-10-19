using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


public class Longbow : Bow
{
	public Longbow()
		: base("longbow")
	{
		displayName = "Longbow";

		baseDamage = 2.5f;
		baseAttackRate = 2;
		baseAttackRange = 40; // arrow speed
		knockback = 8.0f;
		trigger = false;
		twoHanded = true;
		accuracy = 3.0f;

		strengthScaling = 0.2f;
		dexterityScaling = 0.5f;

		value = 25;

		sprite = new Sprite(tileset, 10, 3, 2, 1);
		icon = new Sprite(tileset.texture, 10 * 16 + 8, 3 * 16, 16, 16);
		size = new Vector2(2, 1);
		renderOffset.x = 0.2f;
	}
}
