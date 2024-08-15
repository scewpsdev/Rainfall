using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Coin : Entity
{
	const float COLLECT_DELAY = 0.5f;

	uint color;

	long spawnTime;


	public Coin()
	{
		color = MathHelper.VectorToARGB(new Vector4(MathHelper.ARGBToVector(0xFF66AAAA).xyz * MathHelper.RandomVector3(0.8f, 1.5f), 1.0f));
	}

	public override void init()
	{
		spawnTime = Time.currentTime;
	}

	public override void update()
	{
		Vector2 toPlayer = GameState.instance.player.position - position;
		float distance = toPlayer.length;
		if (distance < 4.0f)
		{
			float speed = (1 - distance / 4.0f) * 1;
			velocity += speed * toPlayer / distance * 0.3f;
		}
		else
		{
			velocity = Vector2.Lerp(velocity, Vector2.Zero, 3.0f * Time.deltaTime);
		}

		Vector2 displacement = velocity * Time.deltaTime;
		if (distance < 4.0f)
			displacement += toPlayer.normalized * 3 * Time.deltaTime;
		position += displacement;

		if ((Time.currentTime - spawnTime) / 1e9f > COLLECT_DELAY)
		{
			HitData[] hits = new HitData[16];
			int numHits = GameState.instance.level.overlap(position - 0.25f, position + 0.25f, hits, FILTER_PLAYER);
			for (int i = 0; i < numHits; i++)
			{
				if (hits[i].entity != null && hits[i].entity is Player)
				{
					Player player = hits[i].entity as Player;
					player.money++;
					remove();
				}
			}
		}
	}

	public override void render()
	{
		//Renderer.DrawSprite(position.x - 1.0f / 16, position.y - 1.0f / 16, 2 / 16.0f, 2 / 16.0f, null, false, 0xFFFFCC77);
		Renderer.DrawSprite(position.x - 1.0f / 16, position.y - 1.0f / 16, 1 / 16.0f, 1 / 16.0f, null, false, color);
	}
}
