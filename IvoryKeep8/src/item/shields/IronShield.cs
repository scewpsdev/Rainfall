using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class IronShield : Shield
{
	public IronShield()
		: base("iron_shield")
	{
		displayName = "Iron Shield";

		baseArmor = 2;
		baseValue = 16;
		baseWeight = 1.5f;
		blockAbsorption = 0.9f;
		knockbackAbsorption = 0.5f;

		sprite = new Sprite(tileset, 3, 3);
	}
}
