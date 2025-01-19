using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Trampoline : Entity
{
	const float STRENGTH = 14;
	const float ACTIVE_DURATION = 0.2f;


	Sprite sprite;
	Sprite activeSprite;

	Sound useSound;

	long lastActive = -1;


	public Trampoline()
	{
		sprite = new Sprite(tileset, 0, 5);
		activeSprite = new Sprite(tileset, 1, 5);

		useSound = Resource.GetSound("sounds/spring.ogg");
	}

	public override void update()
	{
		Span<HitData> hits = new HitData[16];
		int numHits = GameState.instance.level.overlap(position + new Vector2(-0.5f, 0), position + new Vector2(0.5f, 0.25f), hits, FILTER_PROJECTILE | FILTER_MOB | FILTER_PLAYER | FILTER_ITEM | FILTER_DECORATION | FILTER_OBJECT);
		for (int i = 0; i < numHits; i++)
		{
			if (hits[i].entity != null && hits[i].entity != this)
			{
				//if (hits[i].entity.velocity.y < -0.1f)
				{
					if (hits[i].entity is Player)
					{
						if ((hits[i].entity as Player).isDucked)
							continue;

						hits[i].entity.velocity.y = MathF.Max(-hits[i].entity.velocity.y, STRENGTH);
						((Player)hits[i].entity).currentLadder = null;
					}
					else
					{
						hits[i].entity.velocity.y = -hits[i].entity.velocity.y;

						if (hits[i].entity is Mob)
							((Mob)hits[i].entity).isGrounded = true;
					}

					if (!isActive)
						Audio.Play(useSound, new Vector3(position, 0));

					lastActive = Time.currentTime;
				}
			}
		}

		TileType tile = GameState.instance.level.getTile(position + new Vector2(0.0f, -0.5f));
		if (tile == null)
			remove();
	}

	public bool isActive => lastActive != -1 && (Time.currentTime - lastActive) / 1e9f < ACTIVE_DURATION;

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y, 1, 1, isActive ? activeSprite : sprite, false);
	}
}
