using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TravellingCloak : Item
{
	public TravellingCloak()
		: base("travelling_cloak", ItemType.Armor)
	{
		displayName = "Travelling Cloak";

		armor = 1;
		armorSlot = ArmorSlot.Body;
		baseWeight = 0.5f;

		value = 3;

		sprite = new Sprite(tileset, 5, 0);
		spriteColor = 0xFF5a6557;
		ingameSprite = new Sprite(Resource.GetTexture("res/sprites/items/cloak.png", false), 0, 0, 16, 16);
		ingameSpriteColor = 0xFF5a6557;
	}
}
