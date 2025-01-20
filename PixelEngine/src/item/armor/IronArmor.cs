using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class IronArmor : Item
{
	public IronArmor()
		: base("iron_armor", ItemType.Armor)
	{
		displayName = "Iron Armor";

		baseArmor = 6;
		armorSlot = ArmorSlot.Body;
		baseWeight = 3;

		value = 25;

		sprite = new Sprite(tileset, 12, 7);
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/armor/iron_armor.png", false), 0, 0, 32, 32);
	}
}
