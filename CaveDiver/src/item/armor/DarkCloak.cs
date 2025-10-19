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

		baseArmor = 1;
		armorSlot = ArmorSlot.Body;
		baseWeight = 0.5f;

		value = 5;

		sprite = new Sprite(tileset, 5, 0);
		spriteColor = 0xFF443c56; // 0xFF2e2739;
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/armor/cloak.png", false), 0, 0, 32, 32);
		ingameSpriteColor = 0xFF443c56; // 0xFF2e2739;
		ingameSpriteCoversArms = true;
	}
}
