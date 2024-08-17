using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Bomb : Item
{
	int blastRadius = 3;
	float fuseTime = 2.0f;


	long useTime = -1;


	public Bomb()
		: base("bomb")
	{
		displayName = "Bomb";
		type = ItemType.Active;
		stackable = true;

		value = 9;

		attackDamage = 8;

		sprite = new Sprite(tileset, 1, 0);

		//projectileItem = true;
	}

	public override bool use(Player player)
	{
		player.throwItem(this);
		ignite();
		return true;
	}

	public void ignite()
	{
		useTime = Time.currentTime;
	}

	public override void update(Entity entity)
	{
		if (useTime != -1 && entity is ItemEntity)
		{
			ItemEntity itemEntity = entity as ItemEntity;
			itemEntity.color = 0xFFFFBB00;
		}

		if (useTime != -1 && (Time.currentTime - useTime) / 1e9f >= fuseTime)
		{
			Vector2i tile = (Vector2i)Vector2.Round(entity.position);
			for (int y = tile.y - blastRadius; y < tile.y + blastRadius; y++)
			{
				for (int x = tile.x - blastRadius; x < tile.x + blastRadius; x++)
				{
					float distance = (new Vector2(x, y) + 0.5f - tile).length;
					if (distance < blastRadius)
						GameState.instance.level.setTile(x, y, null);
				}
			}

			Span<HitData> hits = new HitData[16];
			int numHits = GameState.instance.level.overlap(tile - (float)blastRadius, tile + (float)blastRadius, hits, Entity.FILTER_MOB | Entity.FILTER_PLAYER | Entity.FILTER_ITEM | Entity.FILTER_DEFAULT);
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

						hits[i].entity.velocity += (center - tile).normalized * (1 - distance / blastRadius) * 30;
					}
				}
			}

			entity.remove();
		}
	}
}
