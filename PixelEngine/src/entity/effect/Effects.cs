using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static unsafe class Effects
{
	public static ParticleEffect CreateBloodEffect(Vector2 direction)
	{
		ParticleEffect effect = new ParticleEffect(null, "res/effects/blood.rfs");
		effect.systems[0].handle->startVelocity = new Vector3(direction, 0);
		effect.collision = true;
		return effect;
	}

	public static ParticleEffect CreateFountainEffect()
	{
		ParticleEffect effect = new ParticleEffect(null, "res/effects/fountain2.rfs");
		//effect.collision = true;
		return effect;
	}

	public static ParticleEffect CreateDeathEffect(Entity entity, int direction)
	{
		ParticleEffect effect = new ParticleEffect(entity, "res/effects/death.rfs");
		effect.collision = true;
		effect.systems[0].handle->startVelocity.x *= direction;
		return effect;
	}

	public static ParticleEffect CreateExplosionEffect(int size)
	{
		ExplosionEffect effect = new ExplosionEffect();
		effect.systems[0].handle->radialVelocity = size * 10;
		effect.systems[1].handle->radialVelocity = size * 10;
		effect.systems[0].handle->bursts[0].count = size * 6;
		return effect;
	}

	public static ParticleEffect CreateImpactEffect(Vector2 normal, float velocity, int count, Vector3 color)
	{
		ParticleEffect effect = new ParticleEffect(null, "res/effects/impact.rfs");
		effect.systems[0].handle->startVelocity = new Vector3(normal * velocity, 0);
		effect.systems[0].handle->bursts[0].count = count;
		effect.systems[0].handle->colorAnim.value0.value.xyz = color;
		effect.systems[0].handle->colorAnim.value1.value.xyz = color;
		effect.collision = true;
		return effect;
	}

	public static ParticleEffect CreateImpactEffect(Vector2 normal, float strength, Vector3 color)
	{
		ParticleEffect effect = new ParticleEffect(null, "res/effects/impact.rfs");
		effect.systems[0].handle->startVelocity = new Vector3(normal * strength * 0.15f, 0);
		effect.systems[0].handle->bursts[0].count = (int)MathF.Ceiling(strength * 0.5f);
		effect.systems[0].handle->colorAnim.value0.value.xyz = color;
		effect.systems[0].handle->colorAnim.value1.value.xyz = color;
		effect.collision = true;
		return effect;
	}

	public static ParticleEffect CreateStepEffect(int count, Vector3 color)
	{
		ParticleEffect effect = new ParticleEffect(null, "res/effects/impact.rfs");
		effect.systems[0].handle->startVelocity = new Vector3(0, 2, 0);
		effect.systems[0].handle->bursts[0].count = count;
		effect.systems[0].handle->colorAnim.value0.value.xyz = color;
		effect.systems[0].handle->colorAnim.value1.value.xyz = color;
		effect.collision = true;
		return effect;
	}

	public static ParticleEffect CreateDestroyWoodEffect(uint color)
	{
		ParticleEffect effect = new ParticleEffect(null, "res/effects/destroy_wood.rfs");
		effect.systems[0].handle->color = MathHelper.ARGBToVector(color);
		effect.collision = true;
		return effect;
	}

	public static ParticleEffect CreateSparkEffect()
	{
		ParticleEffect effect = new ParticleEffect(null, "res/effects/sparks.rfs");
		effect.collision = true;
		return effect;
	}

	public static AnimatedEffect CreateCoinBlinkEffect()
	{
		return new AnimatedEffect(Resource.GetTexture("res/sprites/coin_collect.png", false), 15);
	}
}
