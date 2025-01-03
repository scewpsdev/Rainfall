using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LeatherBoots : Item
{
	public LeatherBoots()
		: base("leather_boots", ItemType.Armor)
	{
		displayName = "Leather Boots";

		armor = 0.5f;
		armorSlot = ArmorSlot.Boots;
		baseWeight = 0.2f;

		value = 8;

		sprite = new Sprite(tileset, 1, 9);
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/leather_boots.png", false), 0, 0, 16, 16);
	}
}
