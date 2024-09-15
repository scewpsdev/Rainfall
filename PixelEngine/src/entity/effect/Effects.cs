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

	public static void Explode(Vector2 position, float radius, float damage, Entity fromEntity, Item fromItem)
	{
		int x0 = (int)MathF.Floor(position.x - radius);
		int x1 = (int)MathF.Floor(position.x + radius);
		int y0 = (int)MathF.Floor(position.y - radius);
		int y1 = (int)MathF.Floor(position.y + radius);
		Vector2i pos = (Vector2i)Vector2.Round(position);
		for (int y = y0; y <= y1; y++)
		{
			for (int x = x0; x <= x1; x++)
			{
				TileType tile = GameState.instance.level.getTile(x, y);
				float distance = (new Vector2(x, y) + 0.5f - position).length - 0.5f;
				if (distance < radius && tile != null && tile.destructible)
					GameState.instance.level.setTile(x, y, null);
			}
		}
		GameState.instance.level.updateLightmap(x0, y0, x1 - x0 + 1, y1 - y0 + 1);

		Span<HitData> hits = new HitData[16];
		int numHits = GameState.instance.level.overlap(pos - radius, pos + radius, hits, Entity.FILTER_MOB | Entity.FILTER_PLAYER | Entity.FILTER_ITEM | Entity.FILTER_DEFAULT);
		for (int i = 0; i < numHits; i++)
		{
			if (hits[i].entity != null)
			{
				if (hits[i].entity == fromEntity)
					continue;

				Vector2 center = hits[i].entity.position + 0.5f * (hits[i].entity.collider.min + hits[i].entity.collider.max);
				float distance = (center - pos).length;
				if (distance < radius)
				{
					hits[i].entity.velocity += (center - pos).normalized * (1 - distance / radius) * 30;

					if (hits[i].entity is Hittable)
					{
						float dmg = (1 - distance / radius) * damage;

						Hittable hittable = hits[i].entity as Hittable;
						hittable.hit(dmg, fromEntity, fromItem, "Explosion");
					}
					else if (hits[i].entity is Destructible && distance / radius < 0.5f)
					{
						Destructible destructible = hits[i].entity as Destructible;
						destructible.onDestroyed(fromEntity, fromItem);
						hits[i].entity.remove();
					}
				}
			}
		}

		// sound
		GameState.instance.level.addEntity(CreateExplosionEffect((int)MathF.Round(radius)), position);
		GameState.instance.camera.addScreenShake(position, 2.0f, 3.0f);
	}
}
