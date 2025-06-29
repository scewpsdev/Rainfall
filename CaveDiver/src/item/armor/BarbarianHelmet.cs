﻿using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BarbarianHelmet : Item
{
	public BarbarianHelmet()
		: base("barbarian_helmet", ItemType.Armor)
	{
		displayName = "Barbarian Helmet";

		baseArmor = 5;
		armorSlot = ArmorSlot.Helmet;
		baseWeight = 2;

		value = 40;

		sprite = new Sprite(tileset, 11, 0);
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/armor/barbarian_helmet.png", false), 0, 0, 32, 32);
	}
}
