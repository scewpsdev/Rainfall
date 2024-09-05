using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class HealEffect : StatusEffect
{
	float amount;
	float duration;

	long startTime;
	long lastUpdate;


	public HealEffect(float amount, float duration)
	{
		this.amount = amount;
		this.duration = duration;

		startTime = Time.currentTime;
		lastUpdate = Time.currentTime;
	}

	public override unsafe void init(Player player)
	{
		//GameState.instance.level.addEntity(new ParticleEffect(player, (int)(amount * 8), duration, 5.0f, 0.25f, 0xFFFF4D40), player.position + new Vector2(0, 0.5f));

		ParticleEffect effect = new ParticleEffect(player, "res/effects/regenerate.rfs");
		effect.system.handle->color = MathHelper.ARGBToVector(0xFFFF4D40);
		effect.system.handle->bursts[0].duration = duration;
		effect.system.handle->bursts[0].count = (int)(amount * 8);

		GameState.instance.level.addEntity(effect, player.position + new Vector2(0, 0.5f));
	}

	public override bool update(Player player)
	{
		float elapsed = (Time.currentTime - startTime) / 1e9f;
		float sinceLastFrame = (Time.currentTime - lastUpdate) / 1e9f;
		if (elapsed >= duration)
			sinceLastFrame -= elapsed - duration;
		float heal = amount * sinceLastFrame / duration;
		if (player.health < player.maxHealth)
			player.health += heal;
		lastUpdate = Time.currentTime;
		return elapsed < duration;
	}
}
