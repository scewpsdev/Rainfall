using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class IronGauntlets : Item
{
	public IronGauntlets()
		: base("iron_gauntlets", ItemType.Armor)
	{
		displayName = "Iron Gauntlets";

		baseArmor = 2.5f;
		armorSlot = ArmorSlot.Gloves;
		baseWeight = 1.25f;

		value = 32;

		sprite = new Sprite(tileset, 0, 9);
		ingameSprite = new Sprite("sprites/items/armor/iron_gauntlets.png", 0, 0, 32, 32);
		ingameSpriteLayer = Entity.LAYER_PLAYER_GLOVE;

		gloveColor = 0xFF969696;
	}
}
