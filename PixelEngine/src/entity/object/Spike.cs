using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Spike : Entity, Hittable
{
	Sprite sprite;


	public Spike()
	{
		displayName = "Spikes";

		sprite = new Sprite(tileset, 0, 3);
		collider = new FloatRect(0, 0, 1, 0.5f);
	}

	public override void update()
	{
		Span<HitData> hits = new HitData[16];
		int numHits = GameState.instance.level.overlap(position + collider.min, position + collider.max, hits, FILTER_PLAYER | FILTER_MOB);
		for (int i = 0; i < numHits; i++)
		{
			if (hits[i].entity != null && hits[i].entity != this && hits[i].entity is Hittable)
			{
				Hittable hittable = hits[i].entity as Hittable;
				if (hits[i].entity.velocity.y < -3.5f && hits[i].entity.position.y - hits[i].entity.velocity.y * Time.deltaTime > position.y + 0.5f)
				{
					float damage = (-3.5f - hits[i].entity.velocity.y) * 0.4f;
					hittable.hit(damage, this, null);
				}
			}
		}

		TileType tile = GameState.instance.level.getTile(position + new Vector2(0.5f, -0.5f));
		if (tile == null)
			remove();
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x, position.y, LAYER_FG, 1, 1, 0, sprite, false, 0xFFFFFFFF);
	}

	public bool hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true, bool buffedHit = false)
	{
		if (item != null && item.type == ItemType.Weapon)
			return true;
		return false;
	}
}
