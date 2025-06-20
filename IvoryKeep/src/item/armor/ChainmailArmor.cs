﻿using Rainfall;
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

		baseArmor = 4;
		armorSlot = ArmorSlot.Body;
		baseWeight = 2;

		value = 24;

		sprite = new Sprite(tileset, 8, 8);
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/armor/chainmail_armor.png", false), 0, 0, 32, 32);
	}
}
