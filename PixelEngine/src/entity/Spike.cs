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
		HitData hit = GameState.instance.level.overlap(position, position + new Vector2(1, 0.5f), FILTER_PLAYER | FILTER_MOB);
		if (hit != null)
		{
			if (hit.entity != null && hit.entity != this && hit.entity is Hittable)
			{
				Hittable hittable = hit.entity as Hittable;
				if (hit.entity.velocity.y < 0 && hit.entity.position.y - hit.entity.velocity.y * Time.deltaTime > position.y + 0.5f)
					hittable.hit(1000, this);
			}
		}
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x, position.y, LAYER_FG, 1, 1, 0, sprite, false, 0xFFFFFFFF);
	}
}
