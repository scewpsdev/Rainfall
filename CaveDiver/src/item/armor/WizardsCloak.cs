using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WizardsCloak : Item
{
	public WizardsCloak()
		: base("wizards_cloak", ItemType.Armor)
	{
		displayName = "Wizard's Cloak";

		baseArmor = 1;
		armorSlot = ArmorSlot.Body;
		baseWeight = 0.5f;

		value = 5;

		sprite = new Sprite(tileset, 5, 0);
		spriteColor = 0xFF4c358f; // 0xFF874774;
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/armor/cloak.png", false), 0, 0, 32, 32);
		ingameSpriteColor = 0xFF4c358f; // 0xFF874774;

		//ingameSpriteColor = 0xFF676898;
		ingameSpriteCoversArms = true;
	}
}
