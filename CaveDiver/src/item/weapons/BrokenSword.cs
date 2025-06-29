﻿using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BrokenSword : Weapon
{
	public BrokenSword()
		: base("broken_sword")
	{
		displayName = "Broken Sword";

		baseDamage = 0.8f;
		baseAttackRange = 1;
		baseAttackRate = 2;
		baseWeight = 1;
		canBlock = true;
		parryWeaponRotation = -0.3f * MathF.PI;

		value = 2;

		strengthScaling = 0.2f;
		dexterityScaling = 0.2f;

		sprite = new Sprite(tileset, 15, 3);
		renderOffset.x = 0.2f;

		//ingameSprite = new Sprite("sprites/items/weapon/broken_sword.png", 0, 0, 32, 32);
		//ingameSpriteSize = 2;
	}
}
