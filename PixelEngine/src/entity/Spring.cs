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
		Span<HitData> hits = new HitData[16];
		int numHits = GameState.instance.level.overlap(position + new Vector2(-0.5f, 0), position + new Vector2(0.5f, 0.5f), hits, FILTER_DEFAULT | FILTER_MOB | FILTER_PLAYER);
		for (int i = 0; i < numHits; i++)
		{
			if (hits[i].entity != null && hits[i].entity != this)
			{
				if (hits[i].entity.velocity.y < -0.1f)
				{
					hits[i].entity.velocity.y = MathF.Max(-hits[i].entity.velocity.y, STRENGTH);
					if (hits[i].entity is Mob)
						((Mob)hits[i].entity).isGrounded = true;
				}
			}
		}
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y, 1, 1, sprite, false);
	}
}
