using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Amogus : Item
{
	public Amogus()
		: base("amogus", ItemType.Armor)
	{
		displayName = "A m o g u s";

		description = "sus";

		baseArmor = 5;
		armorSlot = ArmorSlot.Body;
		//canDrop = false;
		rarity = 0.01f;

		baseValue = 25;

		sprite = new Sprite(tileset, 2, 5);
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/armor/amogus.png", false), 0, 0, 16, 16);
	}
}
