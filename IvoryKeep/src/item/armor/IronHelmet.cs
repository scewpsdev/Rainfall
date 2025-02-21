using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class IronHelmet : Item
{
	public IronHelmet()
		: base("iron_helmet", ItemType.Armor)
	{
		displayName = "Iron Helmet";

		baseArmor = 3;
		armorSlot = ArmorSlot.Helmet;
		baseWeight = 1.5f;

		value = 36;

		sprite = new Sprite(tileset, 12, 0);
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/armor/iron_helmet.png", false), 0, 0, 32, 32);
	}
}
