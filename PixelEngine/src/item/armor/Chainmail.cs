using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Chainmail : Item
{
	public Chainmail()
		: base("chainmail", ItemType.Armor)
	{
		displayName = "Chainmail";

		armor = 3;
		canEquipMultiple = false;

		value = 8;

		sprite = new Sprite(tileset, 3, 2);
		ingameSprite = new Sprite(Resource.GetTexture("res/sprites/chainmail.png", false), 0, 0, 16, 16);
	}
}
