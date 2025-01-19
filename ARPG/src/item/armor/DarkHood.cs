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

		baseArmor = 0.5f;
		armorSlot = ArmorSlot.Helmet;
		baseWeight = 0.2f;

		value = 2;

		sprite = new Sprite(tileset, 6, 3);
		spriteColor = 0xFF443c56; // 0xFF2e2739;
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/hood.png", false), 0, 0, 16, 16);
		ingameSpriteColor = 0xFF443c56; // 0xFF2e2739;
	}
}
