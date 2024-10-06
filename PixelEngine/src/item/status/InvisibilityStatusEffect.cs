using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class InvisibilityStatusEffect : StatusEffect
{
	float duration;

	long startTime;
	long lastUpdate;


	public InvisibilityStatusEffect(float duration)
		: base("heal", new Sprite(tileset, 0, 0))
	{
		this.duration = duration;

		startTime = Time.currentTime;
		lastUpdate = Time.currentTime;
	}

	public override unsafe void init(Entity entity)
	{
		//GameState.instance.level.addEntity(new ParticleEffect(player, (int)(amount * 8), duration, 5.0f, 0.25f, 0xFFFF4D40), player.position + new Vector2(0, 0.5f));

		ParticleEffect effect = new ParticleEffect(entity, "res/effects/regenerate.rfs");
		effect.systems[0].handle->colorAnim.value0.value.xyz = MathHelper.ARGBToVector(0x7Fabb6bd).xyz;
		effect.systems[0].handle->colorAnim.value1.value.xyz = MathHelper.ARGBToVector(0x7Fabb6bd).xyz;
		effect.systems[0].handle->bursts[0].duration = duration;
		effect.systems[0].handle->bursts[0].count = (int)(duration * 20);

		GameState.instance.level.addEntity(effect, entity.position + new Vector2(0, 0.5f));
	}

	public override bool update(Entity entity)
	{
		float elapsed = (Time.currentTime - startTime) / 1e9f;
		if (elapsed < duration)
		{
			if (entity is StatusEffectReceiver)
			{
				StatusEffectReceiver receiver = entity as StatusEffectReceiver;
				receiver.setVisible(false);
			}
		}
		else
		{
			if (entity is StatusEffectReceiver)
			{
				StatusEffectReceiver receiver = entity as StatusEffectReceiver;
				receiver.setVisible(true);
			}
		}
		return elapsed < duration;
	}

	public override float getProgress()
	{
		float elapsed = (Time.currentTime - startTime) / 1e9f;
		return elapsed / duration;
	}
}
