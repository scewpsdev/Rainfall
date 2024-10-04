using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DarkHood : Item
{
	public DarkHood()
		: base("dark_hood", ItemType.Armor)
	{
		displayName = "Dark Hood";

		armor = 1;
		armorSlot = ArmorSlot.Helmet;

		value = 5;

		sprite = new Sprite(tileset, 6, 3);
		spriteColor = 0xFF17141d; // 0xFF2e2739;
		ingameSprite = new Sprite(Resource.GetTexture("res/sprites/hood.png", false), 0, 0, 16, 16);
		ingameSpriteColor = 0xFF17141d; // 0xFF2e2739;
	}
}
