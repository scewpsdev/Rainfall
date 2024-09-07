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


	public Coin()
	{
		//color = MathHelper.VectorToARGB(new Vector4(MathHelper.ARGBToVector(0xFF66AAAA).xyz * MathHelper.RandomVector3(0.8f, 1.5f), 1.0f));
		color = MathHelper.VectorToARGB(new Vector4(MathHelper.ARGBToVector(0xFFCCAA66).xyz * MathHelper.RandomVector3(0.8f, 1.5f), 1.0f));
	}

	public override void init(Level level)
	{
		spawnTime = Time.currentTime;
	}

	public override void update()
	{
		float followDistance = 3.0f;

		Vector2 toPlayer = GameState.instance.player.position + GameState.instance.player.collider.center - position;
		float distance = toPlayer.length;
		if (distance < followDistance)
		{
			float speed = (1 - distance / followDistance * 0.5f) * 1;
			velocity += speed * toPlayer / distance * 0.3f;
		}
		else
		{
			velocity.x = MathHelper.Lerp(velocity.x, 0, 5 * Time.deltaTime);
			velocity.y += -10 * Time.deltaTime;
		}

		Vector2 displacement = velocity * Time.deltaTime;
		if (distance < followDistance)
			displacement += toPlayer.normalized * 7 * Time.deltaTime;
		else
		{
			HitData hit = GameState.instance.level.raycastTiles(position, velocity.normalized, velocity.length * Time.deltaTime);
			if (hit != null)
			{
				velocity = Vector2.Zero;
				displacement.y = MathF.Sign(displacement.y) * MathF.Min(MathF.Abs(displacement.y), hit.distance);
			}
		}
		position += displacement;

		if ((Time.currentTime - spawnTime) / 1e9f > COLLECT_DELAY)
		{
			HitData hit = GameState.instance.level.sample(position, FILTER_PLAYER);
			if (hit != null && hit.entity != null && hit.entity is Player)
			{
				Player player = hit.entity as Player;
				player.money++;

				if (Random.Shared.NextSingle() < 0.4f)
					GameState.instance.level.addEntity(Effects.CreateCoinBlinkEffect(), position + MathHelper.RandomVector2(-0.5f, 0.5f));

				remove();
			}
		}
	}

	public override void render()
	{
		//Renderer.DrawSprite(position.x - 1.0f / 16, position.y - 1.0f / 16, 2 / 16.0f, 2 / 16.0f, null, false, 0xFFFFCC77);
		Renderer.DrawSprite(position.x, position.y, LAYER_FG, 1 / 16.0f, 1 / 16.0f, 0, null, false, color);
	}
}
