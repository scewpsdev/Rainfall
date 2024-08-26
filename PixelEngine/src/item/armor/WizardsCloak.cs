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

		armor = 2;
		canEquipMultiple = false;

		value = 5;

		sprite = new Sprite(tileset, 5, 0);
		ingameSprite = new Sprite(Resource.GetTexture("res/sprites/cloak.png", false), 0, 0, 16, 16);
	}
}
