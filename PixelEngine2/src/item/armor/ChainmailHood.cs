using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ChainmailHood : Item
{
	public ChainmailHood()
		: base("chainmail_hood", ItemType.Armor)
	{
		displayName = "Chainmail Hood";

		armor = 3;
		armorSlot = ArmorSlot.Helmet;
		weight = 1;

		value = 8;

		sprite = new Sprite(tileset, 3, 2);
		ingameSprite = new Sprite(Resource.GetTexture("res/sprites/chainmail_hood.png", false), 0, 0, 16, 16);

		equipSound = Resource.GetSounds("res/sounds/equip_chainmail", 2);
	}
}
