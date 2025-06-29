﻿using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AssassinsDagger : Weapon
{
	public AssassinsDagger()
		: base("assassins_dagger")
	{
		displayName = "Assassins Dagger";

		baseDamage = 1.5f;
		baseAttackRange = 1.0f;
		baseAttackRate = 2;
		attackCooldown = 2.0f;
		canDrop = false;
		//stab = false;
		baseWeight = 1;

		isSecondaryItem = true;
		//attackRotationOffset = MathF.PI * 0.25f;

		value = 11;

		sprite = new Sprite(tileset, 15, 5);
		renderOffset.x = 0.0f;
		renderOffset.y = -0.25f;

		useSound = Resource.GetSounds("sounds/swing_dagger", 6);
	}
}
