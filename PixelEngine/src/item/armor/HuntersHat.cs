using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class HuntersHat : Item
{
	public HuntersHat()
		: base("hunters_hat", ItemType.Armor)
	{
		displayName = "Hunter's Hat";

		baseArmor = 1;
		armorSlot = ArmorSlot.Helmet;
		baseWeight = 0.5f;

		value = 6;
		canDrop = false;

		sprite = new Sprite(tileset, 2, 7);
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/armor/hunters_hat.png", false), 0, 0, 32, 32);
	}
}
