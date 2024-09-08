using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Spring : Entity
{
	const float STRENGTH = 14;
	const float ACTIVE_DURATION = 0.2f;


	Sprite sprite;
	Sprite activeSprite;

	long lastActive = -1;


	public Spring()
	{
		sprite = new Sprite(TileType.tileset, 0, 5);
		activeSprite = new Sprite(TileType.tileset, 1, 5);
	}

	public override void update()
	{
		Span<HitData> hits = new HitData[16];
		int numHits = GameState.instance.level.overlap(position + new Vector2(-0.5f, 0), position + new Vector2(0.5f, 0.25f), hits, FILTER_DEFAULT | FILTER_MOB | FILTER_PLAYER | FILTER_ITEM | FILTER_DECORATION);
		for (int i = 0; i < numHits; i++)
		{
			if (hits[i].entity != null && hits[i].entity != this)
			{
				//if (hits[i].entity.velocity.y < -0.1f)
				{
					hits[i].entity.velocity.y = MathF.Max(-hits[i].entity.velocity.y, STRENGTH);
					if (hits[i].entity is Mob)
						((Mob)hits[i].entity).isGrounded = true;
					if (hits[i].entity is Player)
						((Player)hits[i].entity).currentLadder = null;

					lastActive = Time.currentTime;
				}
			}
		}

		TileType tile = GameState.instance.level.getTile(position + new Vector2(0.0f, -0.5f));
		if (tile == null)
			remove();
	}

	public override void render()
	{
		bool active = lastActive != -1 && (Time.currentTime - lastActive) / 1e9f < ACTIVE_DURATION;

		Renderer.DrawSprite(position.x - 0.5f, position.y, 1, 1, active ? activeSprite : sprite, false);
	}
}
