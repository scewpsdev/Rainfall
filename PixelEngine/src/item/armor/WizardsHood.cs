using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WizardsHood : Item
{
	public WizardsHood()
		: base("wizards_hood", ItemType.Armor)
	{
		displayName = "Wizard's Hood";

		armor = 1;
		armorSlot = ArmorSlot.Helmet;
		baseWeight = 0.2f;

		value = 5;

		sprite = new Sprite(tileset, 6, 3);
		spriteColor = 0xFF4c358f; // 0xFF2e2739;
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/hood.png", false), 0, 0, 16, 16);
		ingameSpriteColor = 0xFF4c358f; // 0xFF2e2739;
	}
}
