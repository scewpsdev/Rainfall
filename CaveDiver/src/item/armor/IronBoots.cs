using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class IronSabatons : Item
{
	public IronSabatons()
		: base("iron_sabatons", ItemType.Armor)
	{
		displayName = "Iron Sabatons";

		baseArmor = 2.5f;
		armorSlot = ArmorSlot.Boots;
		baseWeight = 1.25f;

		value = 32;

		sprite = new Sprite(tileset, 3, 9);
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/armor/iron_sabatons.png", false), 0, 0, 32, 32);
	}
}
