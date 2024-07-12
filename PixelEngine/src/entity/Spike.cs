using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Spike : Entity
{
	Sprite sprite;


	public Spike()
	{
		sprite = new Sprite(TileType.tileset, 0, 3);
	}

	public override void update()
	{
		Span<HitData> hits = new HitData[16];
		int numHits = GameState.instance.level.overlap(position, position + new Vector2(1, 0.5f), hits, FILTER_PLAYER | FILTER_MOB);
		for (int i = 0; i < numHits; i++)
		{
			if (hits[i].entity != null && hits[i].entity != this && hits[i].entity is Hittable)
			{
				Hittable hittable = hits[i].entity as Hittable;
				if (hits[i].entity.velocity.y < 0 && hits[i].entity.position.y - hits[i].entity.velocity.y * Time.deltaTime > position.y + 0.5f)
					hittable.hit(1000, this);
			}
		}
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x, position.y, LAYER_FG, 1, 1, 0, sprite, false, 0xFFFFFFFF);
	}
}
