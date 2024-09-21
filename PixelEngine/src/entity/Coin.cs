using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Coin : Entity
{
	const float COLLECT_DELAY = 0.2f;

	uint color;

	long spawnTime;

	public Entity target;


	public Coin()
	{
		//color = MathHelper.VectorToARGB(new Vector4(MathHelper.ARGBToVector(0xFF66AAAA).xyz * MathHelper.RandomVector3(0.8f, 1.5f), 1.0f));
		color = MathHelper.VectorToARGB(new Vector4(MathHelper.ARGBToVector(0xFFCCAA66).xyz * MathHelper.RandomVector3(0.8f, 1.5f), 1.0f));
	}

	public override void init(Level level)
	{
		spawnTime = Time.currentTime;
		target = GameState.instance.player;
	}

	public override void update()
	{
		float followDistance = 3.0f;

		Vector2 displacement = velocity * Time.deltaTime;

		if (target != null)
		{
			Vector2 toTarget = target.position + target.collider.center - position;
			float distance = toTarget.length;
			if (distance < followDistance && !target.removed)
			{
				float speed = (1 - distance / followDistance * 0.5f) * 1;
				velocity += speed * toTarget / distance * 0.3f;
				displacement += toTarget.normalized * 7 * Time.deltaTime;

				if ((Time.currentTime - spawnTime) / 1e9f > COLLECT_DELAY)
				{
					HitData hit = GameState.instance.level.sample(position, FILTER_PLAYER | FILTER_MOB);
					if (hit != null && hit.entity == target)
					{
						if (hit.entity is Player)
							(target as Player).money++;
						else if (hit.entity is Leprechaun)
							(target as Leprechaun).money++;

						if (Random.Shared.NextSingle() < 0.4f)
							GameState.instance.level.addEntity(Effects.CreateCoinBlinkEffect(), position + MathHelper.RandomVector2(-0.5f, 0.5f));

						remove();
					}
				}
			}
			else
			{
				target = null;
			}
		}
		if (target == null)
		{
			velocity.x = MathHelper.Lerp(velocity.x, 0, 5 * Time.deltaTime);
			velocity.y += -10 * Time.deltaTime;

			HitData[] hits = new HitData[4];
			int numHits = GameState.instance.level.overlap(position - followDistance, position + followDistance, hits, FILTER_PLAYER | FILTER_MOB);
			for (int i = 0; i < numHits; i++)
			{
				if (hits[i].entity is Player || hits[i].entity is Leprechaun)
				{
					target = hits[i].entity;
					break;
				}
			}

			HitData hit = GameState.instance.level.raycastTiles(position, velocity.normalized, velocity.length * Time.deltaTime);
			if (hit != null)
			{
				velocity = Vector2.Zero;
				//displacement = MathF.Min(displacement.length, hit.distance) * displacement.normalized;
				displacement.y = MathF.Sign(displacement.y) * MathF.Min(MathF.Abs(displacement.y), hit.distance);
			}
		}

		position += displacement;
	}

	public override void render()
	{
		//Renderer.DrawSprite(position.x - 1.0f / 16, position.y - 1.0f / 16, 2 / 16.0f, 2 / 16.0f, null, false, 0xFFFFCC77);
		Renderer.DrawSprite(position.x, position.y, LAYER_FG, 1 / 16.0f, 1 / 16.0f, 0, null, false, color);
	}
}
