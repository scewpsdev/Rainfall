﻿using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WizardsHood : Item
{
	public WizardsHood()
		: base("wizards_hood", ItemType.Armor)
	{
		displayName = "Wizard's Hood";

		baseArmor = 0.5f;
		armorSlot = ArmorSlot.Helmet;
		baseWeight = 0.2f;

		value = 2;

		sprite = new Sprite(tileset, 6, 3);
		spriteColor = 0xFF4c358f; // 0xFF2e2739;
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/armor/hood.png", false), 0, 0, 32, 32);
		ingameSpriteColor = 0xFF4c358f; // 0xFF2e2739;

		buff = new ItemBuff(this) { manaRecoveryModifier = 1.1f };
	}
}
