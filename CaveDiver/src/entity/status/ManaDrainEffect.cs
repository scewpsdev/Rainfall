using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ManaDrainEffect : StatusEffect
{
	float amount;
	float duration;

	long startTime;
	long lastUpdate;


	public ManaDrainEffect(float amount, float duration)
		: base("mana_drain", new Sprite(tileset, 2, 0))
	{
		this.amount = amount;
		this.duration = duration;

		positiveEffect = false;

		startTime = Time.currentTime;
		lastUpdate = Time.currentTime;
	}

	public override unsafe void init(Entity entity)
	{
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
			if (player.mana >= 0)
				player.mana -= charge;
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
