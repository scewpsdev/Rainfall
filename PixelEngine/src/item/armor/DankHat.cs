using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DankHat : Item
{
	public DankHat()
		: base("dank_hat", ItemType.Armor)
	{
		displayName = "Dank Hat";

		baseArmor = 0.5f;
		armorSlot = ArmorSlot.Helmet;
		baseWeight = 0.2f;

		value = 5;
		rarity = GetRarity(50);

		sprite = new Sprite(tileset, 0, 8);
		spriteColor = 0xFF86a331;
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/pointy_hat.png", false), 0, 0, 32, 32);
		ingameSpriteSize = 2;
		ingameSpriteColor = 0xFF86a331;
	}
}
