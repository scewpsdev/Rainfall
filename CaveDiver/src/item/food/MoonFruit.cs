﻿using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MoonFruit : Item
{
	public MoonFruit()
		: base("moon_fruit", ItemType.Food)
	{
		displayName = "Moon Fruit";

		description = "+1 MP";

		value = 40;

		sprite = new Sprite(tileset, 13, 4);

		useSound = [Resource.GetSound("sounds/eat.ogg")];
	}

	public override bool use(Player player)
	{
		base.use(player);
		player.maxMana++;
		player.addStatusEffect(new ManaRechargeEffect(player.maxMana - player.mana, 3.0f));
		GameState.instance.level.addEntity(ParticleEffects.CreateConsumableUseEffect(player, player.direction, 0xFFa6f1cc), player.position);
		return true;
	}
}
