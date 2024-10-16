using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DarkCloak : Item
{
	public DarkCloak()
		: base("dark_cloak", ItemType.Armor)
	{
		displayName = "Dark Cloak";

		armor = 1;
		armorSlot = ArmorSlot.Body;
		weight = 0.5f;

		value = 5;

		sprite = new Sprite(tileset, 5, 0);
		spriteColor = 0xFF17141d; // 0xFF2e2739;
		ingameSprite = new Sprite(Resource.GetTexture("res/sprites/cloak.png", false), 0, 0, 16, 16);
		ingameSpriteColor = 0xFF17141d; // 0xFF2e2739;
	}
}
