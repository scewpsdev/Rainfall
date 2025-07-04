﻿using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ManaRechargeEffect : StatusEffect
{
	float amount;
	float duration;

	long startTime;
	long lastUpdate;


	public ManaRechargeEffect(float amount, float duration)
		: base("mana_recharge", new Sprite(tileset, 2, 0))
	{
		this.amount = amount;
		this.duration = duration;

		startTime = Time.currentTime;
		lastUpdate = Time.currentTime;
	}

	public override unsafe void init(Entity entity)
	{
		//GameState.instance.level.addEntity(new ParticleEffect(player, (int)(amount * 8), duration, 5.0f, 0.25f, ), player.position + new Vector2(0, 0.5f));

		ParticleEffect effect = new ParticleEffect(entity, "effects/regenerate.rfs");
		effect.systems[0].handle->colorAnim.value0.value.xyz = MathHelper.ARGBToVector(0xFF758FFF).xyz;
		effect.systems[0].handle->colorAnim.value1.value.xyz = MathHelper.ARGBToVector(0xFF758FFF).xyz;
		effect.systems[0].handle->bursts[0].duration = duration;
		effect.systems[0].handle->bursts[0].count = (int)(amount * 8);

		GameState.instance.level.addEntity(effect, entity.position + new Vector2(0, 0.5f));
	}

	public override bool update(Entity entity)
	{
		float elapsed = (Time.currentTime - startTime) / 1e9f;
		float sinceLastFrame = (Time.currentTime - lastUpdate) / 1e9f;
		if (elapsed >= duration)
			sinceLastFrame -= elapsed - duration;
		float charge = amount * sinceLastFrame / duration;

		if (entity is Player)
		{
			Player player = entity as Player;
			if (player.mana < player.maxMana)
				player.mana += charge;
		}
		lastUpdate = Time.currentTime;
		return elapsed < duration;
	}

	public override float getProgress()
	{
		float elapsed = (Time.currentTime - startTime) / 1e9f;
		return elapsed / duration;
	}
}
