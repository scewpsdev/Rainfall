using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PoisonStatusEffect : StatusEffect
{
	float amount;
	float duration;

	long startTime;
	long lastUpdate;


	public PoisonStatusEffect(float amount, float duration)
		: base("poison", new Sprite(tileset, 1, 0))
	{
		this.amount = amount;
		this.duration = duration;

		positiveEffect = false;

		startTime = Time.currentTime;
		lastUpdate = Time.currentTime;
	}

	public override unsafe void init(Entity entity)
	{
		//GameState.instance.level.addEntity(new ParticleEffect(player, (int)(amount * 16), duration, 5.0f, 0.25f, 0xFFAFAF2A), player.position + new Vector2(0, 0.5f));

		ParticleEffect effect = new ParticleEffect(entity, "effects/regenerate.rfs");
		effect.systems[0].handle->colorAnim.value0.value.xyz = MathHelper.ARGBToVector(0xFFAFAF2A).xyz;
		effect.systems[0].handle->colorAnim.value1.value.xyz = MathHelper.ARGBToVector(0xFFAFAF2A).xyz;
		effect.systems[0].handle->bursts[0].duration = duration;
		effect.systems[0].handle->bursts[0].count = (int)(amount * 8);

		GameState.instance.level.addEntity(effect, entity.position + new Vector2(0, 0.5f));
	}

	public override bool update(Entity entity)
	{
		float elapsed = (Time.currentTime - startTime) / 1e9f;
		float sinceLastFrame = (Time.currentTime - lastUpdate) / 1e9f;
		float dmg = amount * sinceLastFrame / duration;

		Hittable hittable = entity as Hittable;
		if (hittable is not Player)
			dmg *= 10;
		if (hittable is Mob && (hittable as Mob).poisonResistant)
			dmg = 0;
		if (dmg > 0)
			hittable.hit(dmg, null, null, "Poison", false);

		lastUpdate = Time.currentTime;
		return elapsed < duration;
	}

	public override float getProgress()
	{
		float elapsed = (Time.currentTime - startTime) / 1e9f;
		return elapsed / duration;
	}
}
