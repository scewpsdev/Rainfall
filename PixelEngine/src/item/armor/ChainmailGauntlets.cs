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

		armor = 1.5f;
		armorSlot = ArmorSlot.Gloves;
		baseWeight = 0.75f;

		value = 5;

		sprite = new Sprite(tileset, 15, 8);
	}
}
