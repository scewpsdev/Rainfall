using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LeatherCap : Item
{
	public LeatherCap()
		: base("leather_cap", ItemType.Armor)
	{
		displayName = "Leather Cap";

		armor = 1f;
		armorSlot = ArmorSlot.Helmet;
		baseWeight = 0.5f;

		value = 10;

		sprite = new Sprite(tileset, 2, 8);
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/leather_cap.png", false), 0, 0, 16, 16);
	}
}
