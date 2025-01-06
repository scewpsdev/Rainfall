using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class IronGauntlets : Item
{
	public IronGauntlets()
		: base("iron_gauntlets", ItemType.Armor)
	{
		displayName = "Iron Gauntlets";

		baseArmor = 2.5f;
		armorSlot = ArmorSlot.Gloves;
		baseWeight = 1.25f;

		value = 20;

		sprite = new Sprite(tileset, 0, 9);
	}
}
