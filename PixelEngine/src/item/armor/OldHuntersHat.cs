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
		description = "Strangely familiar.";

		armor = 1;
		armorSlot = ArmorSlot.Helmet;
		baseWeight = 0.5f;

		value = 17;
		//canDrop = false;

		sprite = new Sprite(tileset, 3, 7);
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/old_hunters_hat.png", false), 0, 0, 32, 32);
		ingameSpriteSize = 2;
	}
}
