using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class XPOrb : Entity
{
	const float MAGNET_DELAY = 0.1f;
	const float COLLECT_DELAY = 0.5f;

	Vector4 color;

	long spawnTime;

	Sound[] collectSound;


	public XPOrb()
	{
		//color = MathHelper.VectorToARGB(new Vector4(MathHelper.ARGBToVector(0xFF66AAAA).xyz * MathHelper.RandomVector3(0.8f, 1.5f), 1.0f));
		color = new Vector4(MathHelper.RandomVector3(0.5f, 1.5f), 1.0f);
		collider = new FloatRect(-1 / 16.0f, -1 / 16.0f, 2.0f / 16, 2.0f / 16);

		collectSound = Resource.GetSounds("res/sounds/coin", 6);
	}

	public override void init(Level level)
	{
		spawnTime = Time.currentTime;
	}

	public override void update()
	{
		Player target = GameState.instance.player;

		Vector2 displacement = velocity * Time.deltaTime;

		if (target != null && (Time.currentTime - spawnTime) / 1e9f > MAGNET_DELAY)
		{
			Vector2 toTarget = target.position + target.collider.center - position;
			float speed = MathF.Min(toTarget.length * 0.05f, 3);
			velocity += toTarget.normalized * speed;
			displacement += toTarget.normalized * 10 * Time.deltaTime;

			if ((Time.currentTime - spawnTime) / 1e9f > COLLECT_DELAY)
			{
				HitData hit = GameState.instance.level.sample(position, FILTER_PLAYER | FILTER_MOB);
				if (hit != null && hit.entity == target)
				{
					target.awardXP(1);

					//Audio.Play(collectSound, new Vector3(position, 0));

					remove();
				}
			}
		}

		position += displacement;
	}

	public override void render()
	{
		float brightness = 1 + MathF.Sin(Time.currentTime / 1e9f * 80 + position.x + position.y) * 0.5f;
		Renderer.DrawSprite(position.x - 0.5f / 16.0f, position.y - 0.5f / 16.0f, LAYER_FG, 1 / 16.0f, 1 / 16.0f, 0, null, false, color * new Vector4(brightness, brightness, brightness, 1));
	}
}
