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

		value = 5;

		sprite = new Sprite(tileset, 6, 3);
		ingameSprite = new Sprite(Resource.GetTexture("res/sprites/wizards_hood.png", false), 0, 0, 16, 16);
	}
}
