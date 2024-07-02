using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Spring : Entity
{
	const float STRENGTH = 12;


	Sprite sprite;


	public Spring()
	{
		sprite = new Sprite(TileType.tileset, 0, 1);
	}

	public override void update()
	{
		HitData hit = GameState.instance.level.overlap(position + new Vector2(-0.5f, 0), position + new Vector2(0.5f, 0.5f), FILTER_DEFAULT | FILTER_MOB | FILTER_PLAYER);
		if (hit != null && hit.entity != null && hit.entity != this)
		{
			if (hit.entity.velocity.y < -0.1f)
			{
				hit.entity.velocity.y = MathF.Max(-hit.entity.velocity.y, STRENGTH);
			}
		}
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y, 1, 1, sprite, false);
	}
}
