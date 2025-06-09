using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum CoinType
{
	Bronze,
	Silver,
	Gold,
	Ivory,
}

public class Coin : Entity
{
	readonly static uint[] TYPE_COLORS = [0xFF926c5c, 0xFFbab6ae, 0xFFCCAA66, 0xFFffc6ae];
	readonly static Sprite[] TYPE_SPRITES = [new Sprite(tileset, 4, 3, 0.5f, 0.5f), new Sprite(tileset, 4.5f, 3, 0.5f, 0.5f), new Sprite(tileset, 4, 3.5f, 0.5f, 0.5f), new Sprite(tileset, 4.5f, 3.5f, 0.5f, 0.5f)];
	readonly static int[] TYPE_VALUES = [1, 5, 20, 50];

	public static CoinType SubtractCoinFromValue(ref int value)
	{
		for (int i = TYPE_VALUES.Length - 1; i >= 0; i--)
		{
			if (value >= TYPE_VALUES[i] * 3 / 2)
			{
				value -= TYPE_VALUES[i];
				return (CoinType)i;
			}
		}

		value--;
		return CoinType.Bronze;
	}


	const float COLLECT_DELAY = 0.1f;

	CoinType type;

	long spawnTime;

	float rotationSpeed;
	public Entity target;

	Sound[] collectSound;


	public Coin(CoinType type)
	{
		this.type = type;

		collider = new FloatRect(-1 / 16.0f, -1 / 16.0f, 2.0f / 16, 2.0f / 16);

		collectSound = Resource.GetSounds("sounds/coin", 6);
	}

	public override void init(Level level)
	{
		spawnTime = Time.currentTime;
		target = GameState.instance.player;

		rotationSpeed = MathHelper.RandomFloat(-1, 1);
	}

	public override void update()
	{
		float followDistance = GameState.instance.player.coinCollectDistance;
		if (target != null && target is Player)
			followDistance = (target as Player).coinCollectDistance;

		Vector2 displacement = velocity * Time.deltaTime;

		if (target != null)
		{
			Vector2 toTarget = target.position + target.collider.center - position;
			float distance = toTarget.length;
			if (distance < followDistance && !target.removed)
			{
				//float speed = (1 - distance / followDistance * 0.5f) * 1;
				//velocity += speed * toTarget / distance * 0.3f;
				//displacement += toTarget.normalized * 7 * Time.deltaTime;

				float maxSpeed = 10;
				float currentSpeed = velocity.length;
				velocity = (velocity + toTarget / distance * Time.deltaTime * 150);
				if (velocity.length >= maxSpeed)
					velocity = velocity.normalized * maxSpeed;

				if ((Time.currentTime - spawnTime) / 1e9f > COLLECT_DELAY)
				{
					HitData[] hits = new HitData[16];
					int numHits = GameState.instance.level.overlap(position - 0.25f, position + 0.25f, hits, FILTER_PLAYER | FILTER_MOB);
					for (int i = 0; i < numHits; i++)
					{
						HitData hit = hits[i];
						if (hit != null && hit.entity == target)
						{
							int value = TYPE_VALUES[(int)type];

							if (hit.entity is Player)
								(target as Player).money += value;
							else if (hit.entity is Leprechaun)
								(target as Leprechaun).money += value;

							if (Random.Shared.NextSingle() < 0.4f)
								GameState.instance.level.addEntity(ParticleEffects.CreateCoinBlinkEffect(), position + MathHelper.RandomVector2(-0.5f, 0.5f));

							Audio.Play(collectSound, new Vector3(position, 0));

							remove();
							break;
						}
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
			velocity.y += -20 * Time.deltaTime;

			HitData[] hits = new HitData[32];
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

		int collisionFlags = GameState.instance.level.doCollision(ref position, collider, ref displacement, true);
		if ((collisionFlags & Level.COLLISION_X) != 0)
			velocity.x *= -0.5f;
		if ((collisionFlags & Level.COLLISION_Y) != 0)
		{
			if (MathF.Abs(velocity.y) > 4)
				velocity.x += MathHelper.RandomFloat(-2, 2);
			velocity.y *= -0.5f;
		}
		if (collisionFlags != 0)
			rotationSpeed = MathHelper.RandomFloat(-2, 2);

		position += displacement;

		rotation += rotationSpeed * Time.deltaTime;
	}

	public override void render()
	{
		//Renderer.DrawSprite(position.x - 1.0f / 16, position.y - 1.0f / 16, 2 / 16.0f, 2 / 16.0f, null, false, 0xFFFFCC77);
		float brightness = 1 + MathF.Sin(Time.currentTime / 1e9f * 80 + position.x + position.y) * 0.3f;
		Renderer.DrawSprite(position.x - 0.25f, position.y - 0.25f, LAYER_FG, 0.5f, 0.5f, rotation, TYPE_SPRITES[(int)type], false, /*TYPE_COLORS[(int)type] **/ new Vector4(brightness, brightness, brightness, 1));
	}
}
