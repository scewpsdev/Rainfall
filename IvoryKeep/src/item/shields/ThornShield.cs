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
		description = "A cruelly barbed shield that returns a portion of blocked damage to it's attacker.";

		baseArmor = 3;
		damageReflect = 1.0f;
		blockAbsorption = 0.9f;
		baseValue = 14;
		rarity *= 0.5f;

		sprite = new Sprite(tileset, 4, 3);
	}
}
