﻿using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MagicArrowSpell : Spell
{
	public MagicArrowSpell()
		: base("magic_arrow_spell")
	{
		displayName = "Magic Arrow";

		value = 14;

		baseDamage = 1;
		baseAttackRate = 3;
		manaCost = 0.1f;
		knockback = 1.0f;
		trigger = false;
		canCastWithoutMana = true;

		spellIcon = new Sprite(tileset, 0, 6);

		castSound = Resource.GetSounds("sounds/shoot", 11);
	}

	public override bool cast(Player player, Item staff, float manaCost, float duration)
	{
		Vector2 position = player.position + new Vector2(player.direction * 0.3f, 0.5f);
		Vector2 offset = new Vector2(player.direction * 0.3f, -0.1f);

		Vector2 direction = (player.lookDirection.normalized + Vector2.Sign(player.velocity) * 0.1f).normalized;
		Vector2 inaccuracy = MathHelper.RandomPointOnCircle(Random.Shared) * 0.05f;
		direction = (direction + inaccuracy / (staff.accuracy * player.getAccuracyModifier())).normalized;

		GameState.instance.level.addEntity(new MagicProjectile(direction, player.velocity, offset, player, this, staff), position);
		GameState.instance.level.addEntity(new MagicProjectileCastEffect(player), position + offset);

		return true;
	}
}
