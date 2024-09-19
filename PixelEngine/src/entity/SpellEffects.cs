using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class SpellEffects
{
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

				Vector2 center = hits[i].entity.position + hits[i].entity.collider.center;
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
		GameState.instance.level.addEntity(Effects.CreateExplosionEffect((int)MathF.Round(radius)), position);
		GameState.instance.camera.addScreenShake(position, 2.0f, 3.0f);
	}

	public static void TeleportEntity(Entity entity)
	{
		for (int i = 0; i < 1000; i++)
		{
			int x = MathHelper.RandomInt(3, GameState.instance.level.width - 4);
			int y = MathHelper.RandomInt(3, GameState.instance.level.height - 4);
			TileType tile = GameState.instance.level.getTile(x, y);
			if (tile == null || !tile.isSolid)
			{
				entity.position = new Vector2(x + 0.5f, y + 0.5f) - entity.collider.center;
				break;
			}
		}

		if (entity is Player)
		{
			Player player = entity as Player;
			player.hud.showMessage("Everything around you starts spinning.");
		}
	}
}
