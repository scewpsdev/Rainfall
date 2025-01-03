using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ChainmailArmor : Item
{
	public ChainmailArmor()
		: base("chainmail_armor", ItemType.Armor)
	{
		displayName = "Chainmail Armor";

		armor = 4;
		armorSlot = ArmorSlot.Body;
		baseWeight = 2;

		value = 18;

		sprite = new Sprite(tileset, 8, 8);
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/chainmail_armor.png", false), 0, 0, 16, 16);
	}
}
