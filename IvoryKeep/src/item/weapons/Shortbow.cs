using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


public class Shortbow : Bow
{
	public Shortbow()
		: base("shortbow")
	{
		displayName = "Shortbow";

		baseDamage = 1;
		baseAttackRate = 2.5f;
		baseAttackRange = 30; // arrow speed
		knockback = 2.0f;
		trigger = false;
		baseWeight = 1;

		dexterityScaling = 0.6f;

		value = 16;

		sprite = new Sprite(tileset, 9, 3);
	}
}
