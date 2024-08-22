using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PoisonEffect : StatusEffect
{
	float amount;
	float duration;

	long startTime;
	long lastUpdate;


	public PoisonEffect(float amount, float duration)
	{
		this.amount = amount;
		this.duration = duration;

		startTime = Time.currentTime;
		lastUpdate = Time.currentTime;
	}

	public override void init(Player player)
	{
		GameState.instance.level.addEntity(new ParticleEffect(player, (int)(amount * 16), duration, 5.0f, 0.25f, 0xFFAFAF2A), player.position + new Vector2(0, 0.5f));
	}

	public override bool update(Player player)
	{
		float elapsed = (Time.currentTime - startTime) / 1e9f;
		float sinceLastFrame = (Time.currentTime - lastUpdate) / 1e9f;
		float heal = amount * sinceLastFrame / duration;
		player.hit(heal, null, null, "Poison", false);
		lastUpdate = Time.currentTime;
		return elapsed < duration;
	}
}
