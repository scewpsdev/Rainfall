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

		baseArmor = 2;
		armorSlot = ArmorSlot.Body;
		baseWeight = 1;

		value = 12;

		sprite = new Sprite(tileset, 2, 2);
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/armor/leather_armor.png", false), 0, 0, 32, 32);
	}
}
