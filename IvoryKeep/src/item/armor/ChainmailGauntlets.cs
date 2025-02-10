using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ChainmailGauntlets : Item
{
	public ChainmailGauntlets()
		: base("chainmail_gauntlets", ItemType.Armor)
	{
		displayName = "Chainmail Gauntlets";

		baseArmor = 1.5f;
		armorSlot = ArmorSlot.Gloves;
		baseWeight = 0.75f;

		value = 16;

		sprite = new Sprite(tileset, 15, 8);
		ingameSprite = new Sprite("sprites/items/armor/chainmail_gauntlets.png", 0, 0, 32, 32);
		ingameSpriteLayer = Entity.LAYER_PLAYER_GLOVE;

		gloveColor = 0xFF8c8c8c;
	}
}
