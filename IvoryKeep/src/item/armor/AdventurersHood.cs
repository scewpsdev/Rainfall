using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AdventurersHood : Item
{
	public AdventurersHood()
		: base("adventurers_hood", ItemType.Armor)
	{
		displayName = "Adventurer's Hood";

		baseArmor = 0.5f;
		armorSlot = ArmorSlot.Helmet;
		baseWeight = 0.2f;

		value = 2;

		sprite = new Sprite(tileset, 6, 3);
		spriteColor = 0xFF944046; // 0xFF2e2739;
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/armor/hood.png", false), 0, 0, 32, 32);
		ingameSpriteColor = 0xFF944046; // 0xFF2e2739;
	}
}
