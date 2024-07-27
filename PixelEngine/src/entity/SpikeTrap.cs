using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class SpikeTrap : Entity
{
	int damage = 5;

	Sprite sprite;

	bool falling = false;
	bool hit = false;
	List<Entity> hitEntities = new List<Entity>();


	public SpikeTrap()
	{
		displayName = "Spike Trap";

		sprite = new Sprite(TileType.tileset, 0, 4);
	}

	public override void update()
	{
		if (hit)
		{
			velocity.y = 0;
		}
		else if (!falling)
		{
			HitData hit = GameState.instance.level.raycast(position, new Vector2(0, -1), 10, FILTER_PLAYER | FILTER_MOB | FILTER_ITEM);
			if (hit != null && hit.entity != null)
			{
				falling = true;
			}
		}
		else if (falling)
		{
			velocity.y += -20 * Time.deltaTime;
			position.y += velocity.y * Time.deltaTime;

			HitData[] hits = new HitData[16];
			int numHits = GameState.instance.level.overlap(position + new Vector2(-0.25f, -0.5f), position + new Vector2(0.25f, 0.0f), hits, FILTER_PLAYER | FILTER_MOB);
			for (int i = 0; i < numHits; i++)
			{
				HitData hit = hits[i];
				if (hit.entity != null)
				{
					if (hit.entity is Hittable && !hitEntities.Contains(hit.entity))
					{
						Hittable hittable = hit.entity as Hittable;
						hittable.hit(damage, this);
						hitEntities.Add(hit.entity);
					}
				}
				else
				{
					this.hit = true;
				}
			}
		}
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y - 0.5f, 1, 1, sprite, false);
	}
}
