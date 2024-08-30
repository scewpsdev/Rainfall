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
		displayName = "Chainmail";

		armor = 2;
		canEquipMultiple = false;

		value = 8;

		sprite = new Sprite(tileset, 2, 2);
		ingameSprite = new Sprite(Resource.GetTexture("res/sprites/chainmail.png", false), 0, 0, 16, 16);
	}
}
