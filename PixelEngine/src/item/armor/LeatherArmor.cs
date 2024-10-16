using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LeatherArmor : Item
{
	public LeatherArmor()
		: base("leather_armor", ItemType.Armor)
	{
		displayName = "Leather Armor";

		armor = 4;
		armorSlot = ArmorSlot.Body;
		baseWeight = 1;

		value = 8;

		sprite = new Sprite(tileset, 2, 2);
		ingameSprite = new Sprite(Resource.GetTexture("res/sprites/leather_armor.png", false), 0, 0, 16, 16);
	}
}
