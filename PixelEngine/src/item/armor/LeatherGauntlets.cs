using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LeatherGauntlets : Item
{
	public LeatherGauntlets()
		: base("leather_gauntlets", ItemType.Armor)
	{
		displayName = "Leather Gauntlets";

		armor = 0.5f;
		armorSlot = ArmorSlot.Gloves;
		baseWeight = 0.2f;

		value = 8;

		sprite = new Sprite(tileset, 14, 8);
	}
}
