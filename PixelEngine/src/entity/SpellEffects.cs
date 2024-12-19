using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
				Vector2 tileCenter = new Vector2(x, y) + 0.5f;
				HitData hit = GameState.instance.level.raycastTilesDestructible(position, (tileCenter - position).normalized, (tileCenter - position).length);
				if (hit == null)
				{
					float distance = (tileCenter - position).length - 0.5f;
					if (distance < radius * 0.75f && tile != null && tile.destructible)
						GameState.instance.level.setTile(x, y, null);
				}
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
				float distance = (center - pos).length - hits[i].entity.collider.size.length * 0.5f;
				if (distance < radius)
				{
					HitData hit = GameState.instance.level.raycastTilesDestructible(position, (center - position).normalized, (center - position).length);
					if (hit == null)
					{
						hits[i].entity.velocity += (center - pos).normalized * (1 - distance / radius) * 30;

						if (hits[i].entity is Hittable)
						{
							float dmg = (1 - distance / radius) * damage;

							Hittable hittable = hits[i].entity as Hittable;
							hittable.hit(dmg, fromEntity, fromItem, "Explosion");
						}
					}
				}
			}
		}

		// sound
		GameState.instance.level.addEntity(Effects.CreateExplosionEffect((int)MathF.Round(radius)), position);
		GameState.instance.camera.addScreenShake(position, 2.0f, 3.0f);

		Audio.PlayOrganic(Resource.GetSound("res/sounds/explosion.ogg"), new Vector3(position, 0));
	}

	public static void TeleportEntity(Entity entity, bool onGround, Vector2 center, float maxRange = 20)
	{
		for (int i = 0; i < 1000; i++)
		{
			int x0 = Math.Max(3, (int)(center.x - maxRange));
			int x1 = Math.Min(GameState.instance.level.width - 4, (int)(center.x + maxRange));
			int y0 = Math.Max(3, (int)(center.y - maxRange));
			int y1 = Math.Min(GameState.instance.level.height - 4, (int)(center.y + maxRange));
			int x = MathHelper.RandomInt(x0, x1);
			int y = MathHelper.RandomInt(y0, y1);
			TileType tile = GameState.instance.level.getTile(x, y);
			TileType down = GameState.instance.level.getTile(x, y - 1);
			if ((tile == null || !tile.isSolid) && (!onGround || down != null && down.isSolid))
			{
				entity.position = new Vector2(x + 0.5f, y);
				break;
			}
		}

		if (entity is Player)
		{
			Player player = entity as Player;
			player.hud.showMessage("Everything around you starts spinning.");
		}
	}

	public static void TeleportEntity(Entity entity, float maxRange = 20)
	{
		Vector2 center = entity.position + (entity.collider != null ? entity.collider.center : Vector2.Zero);
		TeleportEntity(entity, false, center, maxRange);
	}
}
