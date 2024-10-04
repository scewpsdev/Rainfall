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

		armor = 1;
		armorSlot = ArmorSlot.Body;

		value = 5;

		sprite = new Sprite(tileset, 5, 0);
		spriteColor = 0xFF4c358f; // 0xFF874774;
		ingameSprite = new Sprite(Resource.GetTexture("res/sprites/cloak.png", false), 0, 0, 16, 16);
		ingameSpriteColor = 0xFF4c358f; // 0xFF874774;
		//ingameSpriteColor = 0xFF676898;
	}
}
