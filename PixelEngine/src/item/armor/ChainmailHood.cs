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

		baseArmor = 2;
		armorSlot = ArmorSlot.Helmet;
		baseWeight = 1;

		value = 6;

		sprite = new Sprite(tileset, 3, 2);
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/armor/chainmail_hood.png", false), 0, 0, 32, 32);

		equipSound = Resource.GetSounds("sounds/equip_chainmail", 2);
	}
}
