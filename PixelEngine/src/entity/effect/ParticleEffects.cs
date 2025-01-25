using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static unsafe class ParticleEffects
{
	public static ParticleEffect CreateBloodEffect(Vector2 direction)
	{
		ParticleEffect effect = new ParticleEffect(null, "effects/blood.rfs");
		effect.systems[0].handle->startVelocity = new Vector3(direction, 0);
		effect.collision = true;
		return effect;
	}

	public static ParticleEffect CreateFountainEffect()
	{
		ParticleEffect effect = new ParticleEffect(null, "effects/fountain2.rfs");
		effect.collision = true;
		effect.bounciness = 0.9f;
		return effect;
	}

	public static ParticleEffect CreateDeathEffect(Entity entity, int direction)
	{
		ParticleEffect effect = new ParticleEffect(entity, "effects/death.rfs");
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
		ParticleEffect effect = new ParticleEffect(null, "effects/impact.rfs");
		effect.systems[0].handle->startVelocity = new Vector3(normal * velocity, 0);
		effect.systems[0].handle->bursts[0].count = count;
		effect.systems[0].handle->colorAnim.value0.value.xyz = color;
		effect.systems[0].handle->colorAnim.value1.value.xyz = color;
		//effect.collision = true;
		effect.bounciness = 0.5f;
		return effect;
	}

	public static ParticleEffect CreateImpactEffect(Vector2 normal, float strength, Vector3 color)
	{
		ParticleEffect effect = new ParticleEffect(null, "effects/impact.rfs");
		effect.systems[0].handle->startVelocity = new Vector3(normal * strength * 0.15f, 0);
		effect.systems[0].handle->bursts[0].count = (int)MathF.Ceiling(strength * 0.5f);
		effect.systems[0].handle->colorAnim.value0.value.xyz = color;
		effect.systems[0].handle->colorAnim.value1.value.xyz = color;
		effect.systems[0].handle->lifetime *= 0.1f;
		effect.collision = true;
		//effect.bounce = true;
		return effect;
	}

	public static ParticleEffect CreateStepEffect(int count, Vector3 color)
	{
		ParticleEffect effect = new ParticleEffect(null, "effects/impact.rfs");
		effect.systems[0].handle->startVelocity = new Vector3(0, 2, 0);
		effect.systems[0].handle->bursts[0].count = count;
		effect.systems[0].handle->colorAnim.value0.value.xyz = color;
		effect.systems[0].handle->colorAnim.value1.value.xyz = color;
		effect.collision = true;
		return effect;
	}

	public static ParticleEffect CreateDestroyWoodEffect(uint color, int count = 10, Vector2 initialVelocity = default)
	{
		ParticleEffect effect = new ParticleEffect(null, "effects/destroy_wood.rfs");
		effect.systems[0].handle->color = MathHelper.ARGBToVector(color);
		effect.systems[0].handle->startVelocity += new Vector3(initialVelocity, 0);
		effect.systems[0].handle->bursts[0].count = count;
		effect.collision = true;
		effect.bounciness = 0.3f;
		return effect;
	}

	public static ParticleEffect CreateDestroyPotEffect(uint color, int count = 10, Vector2 initialVelocity = default)
	{
		ParticleEffect effect = new ParticleEffect(null, "effects/destroy_pot.rfs");
		effect.systems[0].handle->color = MathHelper.ARGBToVector(color);
		effect.systems[0].handle->startVelocity += new Vector3(initialVelocity, 0);
		effect.systems[0].handle->bursts[0].count = count;
		effect.collision = true;
		effect.bounciness = 0.3f;
		return effect;
	}

	public static ParticleEffect CreateSparkEffect()
	{
		ParticleEffect effect = new ParticleEffect(null, "effects/sparks.rfs");
		effect.collision = true;
		return effect;
	}

	public static ParticleEffect CreateSmithEffect()
	{
		ParticleEffect effect = new ParticleEffect(null, "effects/smith.rfs");
		effect.collision = true;
		return effect;
	}

	public static AnimatedEffect CreateCoinBlinkEffect()
	{
		return new AnimatedEffect(Resource.GetTexture("sprites/coin_collect.png", false), 0.2f);
	}

	public static ParticleEffect CreatePotionExplodeEffect(Vector3 color)
	{
		ParticleEffect effect = new ParticleEffect(null, "effects/potion_explode.rfs");
		effect.systems[0].handle->colorAnim.value0.value.xyz = color;
		effect.systems[0].handle->colorAnim.value1.value.xyz = color;
		effect.collision = true;
		return effect;
	}

	public static ParticleEffect CreateConsumableUseEffect(Entity entity, int direction, uint color)
	{
		ParticleEffect effect = new ParticleEffect(entity, "effects/consumable_use.rfs");
		effect.systems[0].handle->color = color;
		effect.systems[0].handle->startVelocity.x *= direction;
		effect.systems[0].handle->spawnOffset.x *= direction;
		return effect;
	}

	public static ParticleEffect CreateTeleportEffect(Entity entity, uint color)
	{
		ParticleEffect effect = new ParticleEffect(entity, "effects/teleport.rfs");
		effect.systems[0].handle->startVelocity = new Vector3(entity.velocity, 0);
		effect.systems[0].handle->colorAnim.value0.value.xyz = MathHelper.ARGBToVector(color).xyz;
		effect.systems[0].handle->colorAnim.value1.value.xyz = MathHelper.ARGBToVector(color).xyz;
		return effect;
	}

	public static ParticleEffect CreateTorchEffect(Entity entity)
	{
		ParticleEffect effect = new ParticleEffect(entity, "effects/torch.rfs");
		return effect;
	}

	public static ParticleEffect CreateCriticalEffect()
	{
		ParticleEffect effect = new ParticleEffect(null, "effects/critical.rfs") { layer = Entity.LAYER_FG };
		effect.systems[0].handle->color = 0xFFff3e24;
		return effect;
	}

	public static ParticleEffect CreateAirJumpEffect(Entity follow)
	{
		ParticleEffect effect = new ParticleEffect(follow, "effects/air_jump.rfs");
		return effect;
	}

	public static ParticleEffect CreateWallSlideEffect(Entity follow)
	{
		ParticleEffect effect = new ParticleEffect(follow, "effects/wall_slide.rfs");
		effect.collision = true;
		return effect;
	}

	public static ParticleEffect CreateScrollUseEffect(Entity follow)
	{
		ParticleEffect effect = new ParticleEffect(follow, "effects/scroll_use.rfs");
		return effect;
	}

	public static UIParticleEffect CreateRecordUIEffect(uint color)
	{
		UIParticleEffect effect = new UIParticleEffect(null, "effects/ui_record.rfs");
		effect.systems[0].handle->colorAnim.value0.value.xyz = MathHelper.ARGBToVector(color).xyz;
		effect.systems[0].handle->colorAnim.value1.value.xyz = MathHelper.ARGBToVector(color).xyz;
		return effect;
	}
}
