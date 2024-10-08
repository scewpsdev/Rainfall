using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class OldHuntersHat : Item
{
	public OldHuntersHat()
		: base("old_hunters_hat", ItemType.Armor)
	{
		displayName = "Old Hunter's Hat";

		armor = 1;
		armorSlot = ArmorSlot.Helmet;

		value = 17;
		//canDrop = false;

		sprite = new Sprite(tileset, 3, 7);
		ingameSprite = new Sprite(Resource.GetTexture("res/sprites/old_hunters_hat.png", false), 0, 0, 16, 16);
	}
}
