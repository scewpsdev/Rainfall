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
		value = 8;
		baseWeight = 2;
		blockAbsorption = 1;

		sprite = new Sprite(tileset, 3, 3);
	}
}
