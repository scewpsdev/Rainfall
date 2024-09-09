using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Bomb : Item
{
	float blastRadius = 2.0f;
	float fuseTime = 1.5f;


	long useTime = -1;


	public Bomb()
		: base("bomb", ItemType.Utility)
	{
		displayName = "Bomb";
		stackable = true;

		value = 6;

		attackDamage = 8;

		sprite = new Sprite(tileset, 1, 0);

		//projectileItem = true;
	}

	public override bool use(Player player)
	{
		player.throwItem(this, player.lookDirection.normalized);
		ignite();
		return true;
	}

	public void ignite()
	{
		useTime = Time.currentTime;
	}

	public Bomb cook()
	{
		useTime = 1;
		return this;
	}

	void explode(Entity entity)
	{
		int x0 = (int)MathF.Floor(entity.position.x - blastRadius);
		int x1 = (int)MathF.Floor(entity.position.x + blastRadius);
		int y0 = (int)MathF.Floor(entity.position.y - blastRadius);
		int y1 = (int)MathF.Floor(entity.position.y + blastRadius);
		Vector2i tile = (Vector2i)Vector2.Round(entity.position);
		for (int y = y0; y <= y1; y++)
		{
			for (int x = x0; x <= x1; x++)
			{
				float distance = (new Vector2(x, y) + 0.5f - entity.position).length - 0.5f;
				if (distance < blastRadius)
					GameState.instance.level.setTile(x, y, null);
			}
		}
		GameState.instance.level.updateLightmap(x0, y0, x1 - x0 + 1, y1 - y0 + 1);

		Span<HitData> hits = new HitData[16];
		int numHits = GameState.instance.level.overlap(tile - blastRadius, tile + blastRadius, hits, Entity.FILTER_MOB | Entity.FILTER_PLAYER | Entity.FILTER_ITEM | Entity.FILTER_DEFAULT);
		for (int i = 0; i < numHits; i++)
		{
			if (hits[i].entity != null)
			{
				if (hits[i].entity is ItemEntity && ((ItemEntity)hits[i].entity).item == this)
					continue;

				Vector2 center = hits[i].entity.position + 0.5f * (hits[i].entity.collider.min + hits[i].entity.collider.max);
				float distance = (center - tile).length;
				if (distance < blastRadius)
				{
					hits[i].entity.velocity += (center - tile).normalized * (1 - distance / blastRadius) * 30;

					if (hits[i].entity is Hittable)
					{
						int damage = (int)MathF.Round((1 - distance / blastRadius) * attackDamage);

						Hittable hittable = hits[i].entity as Hittable;
						hittable.hit(damage, entity, this);
					}
					else if (hits[i].entity is Destructible && distance / blastRadius < 0.5f)
					{
						Destructible destructible = hits[i].entity as Destructible;
						destructible.onDestroyed(null, this);
						hits[i].entity.remove();
					}
				}
			}
		}

		// sound
		GameState.instance.level.addEntity(Effects.CreateExplosionEffect(), entity.position);
		GameState.instance.camera.addScreenShake(entity.position, 2.0f, 3.0f);
	}

	public override void update(Entity entity)
	{
		if (useTime == 1) // cooking
			useTime = Time.currentTime;

		if (useTime != -1 && entity is ItemEntity)
		{
			ItemEntity itemEntity = entity as ItemEntity;
			itemEntity.color = (int)((Time.currentTime - useTime) / 1e9f * 20) % 2 == 1 ? new Vector4(5, 1, 1, 1) : new Vector4(1);
		}

		if (useTime != -1 && (Time.currentTime - useTime) / 1e9f >= fuseTime)
		{
			explode(entity);
			entity.remove();
		}
	}
}
