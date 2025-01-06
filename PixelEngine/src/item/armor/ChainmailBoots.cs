using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ChainmailBoots : Item
{
	public ChainmailBoots()
		: base("chainmail_boots", ItemType.Armor)
	{
		displayName = "Chainmail Boots";

		baseArmor = 1.5f;
		armorSlot = ArmorSlot.Boots;
		baseWeight = 0.75f;

		value = 16;

		sprite = new Sprite(tileset, 2, 9);
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/chainmail_boots.png", false), 0, 0, 16, 16);
	}
}
