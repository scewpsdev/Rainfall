﻿using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WizardHat : Item
{
	public WizardHat()
		: base("wizard_hat", ItemType.Armor)
	{
		displayName = "Wizard Hat";

		baseArmor = 0.5f;
		armorSlot = ArmorSlot.Helmet;
		baseWeight = 0.2f;

		value = 5;

		sprite = new Sprite(tileset, 0, 8);
		spriteColor = 0xFF4c358f; // 0xFF874774;
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/armor/pointy_hat.png", false), 0, 0, 32, 32);
		ingameSpriteColor = 0xFF4c358f; // 0xFF874774;
										//ingameSpriteColor = 0xFF676898;
	}
}
