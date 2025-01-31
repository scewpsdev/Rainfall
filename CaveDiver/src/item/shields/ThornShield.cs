using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ThornShield : Shield
{
	public ThornShield()
		: base("thorn_shield")
	{
		displayName = "Thorn Shield";

		baseArmor = 3;
		damageReflect = 1.0f;
		blockAbsorption = 0.9f;
		value = 14;
		rarity *= 0.5f;

		sprite = new Sprite(tileset, 4, 3);
	}
}
