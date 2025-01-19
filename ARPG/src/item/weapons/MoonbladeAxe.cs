using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MoonbladeAxe : Weapon
{
	public MoonbladeAxe()
		: base("moonblade_axe")
	{
		displayName = "Moonblade Axe";

		baseDamage = 1.6f;
		baseAttackRange = 1.9f;
		baseAttackRate = 1.2f;
		stab = false;
		doubleBladed = false;

		value = 27;

		sprite = new Sprite(tileset, 10, 7, 2, 1);
		size = new Vector2(2, 1);
		renderOffset.x = 0.4f;
	}
}
