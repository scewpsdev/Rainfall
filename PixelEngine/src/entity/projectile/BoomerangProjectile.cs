using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BoomerangProjectile : Entity
{
	float speed = 12;
	float currentRange;

	Entity shooter;
	Boomerang item;

	Sprite sprite;
	Vector2 direction;
	Vector2 startPosition;

	List<Entity> hitEntities = new List<Entity>();
	long throwTime;


	public BoomerangProjectile(Vector2 direction, Vector2 startVelocity, Entity shooter, Boomerang item)
	{
		this.direction = direction;
		this.shooter = shooter;
		this.item = item;

		collider = new FloatRect(-0.2f, -0.2f, 0.4f, 0.4f);
		filterGroup = FILTER_PROJECTILE;

		currentRange = item.attackRange;

		velocity = direction * speed + 0.5f * MathF.Max(Vector2.Dot(direction, startVelocity.normalized), 0) * startVelocity;

		sprite = new Sprite(Item.tileset, 3, 1);
	}

	public override void init(Level level)
	{
		startPosition = position - velocity.normalized * 0.001f;
		throwTime = Time.currentTime;
	}

	void drop()
	{
		GameState.instance.level.addEntity(new ItemEntity(new Boomerang(), null, velocity) { rotation = rotation }, position - velocity.normalized * 0.25f);
		remove();
	}

	public override void update()
	{
		Vector2 accSign = (startPosition - position).normalized;
		Vector2 acc = speed * speed / (2 * currentRange) * accSign;
		Vector2 lastVelocityX = velocity;
		velocity += acc * Time.deltaTime;
		if (Vector2.Dot(velocity, lastVelocityX) < 0)
			hitEntities.Clear();

		Vector2 displacement = velocity * Time.deltaTime;
		position += displacement;

		Vector2 newAccSign = (startPosition - position).normalized;
		if (Vector2.Dot(newAccSign, accSign) < 0)
		{
			velocity /= 1.5f;
			speed /= 1.5f;
			currentRange /= 1.5f;
			if (currentRange < 2)
				drop();
		}

		rotation += 5 * Time.deltaTime;
		rotation = (rotation + MathF.PI) % (MathF.PI * 2) - MathF.PI;

		HitData tileHit = GameState.instance.level.sampleTiles(position);
		if (tileHit != null)
		{
			velocity *= -1;
			hitEntities.Clear();
			//drop();
		}

		HitData[] hits = new HitData[16];
		int numHits = GameState.instance.level.overlap(position - 0.2f, position + 0.2f, hits, FILTER_MOB | FILTER_PLAYER | FILTER_DEFAULT);
		for (int i = 0; i < numHits; i++)
		{
			HitData hit = hits[i];
			if (hit.entity != null)
			{
				Player player = shooter as Player;
				if (hit.entity == shooter)
				{
					if (player != null && (Time.currentTime - throwTime) / 1e9f > 0.1f)
					{
						player.handItem = new Boomerang();
						remove();
					}
				}
				else if (hit.entity is Hittable && !hitEntities.Contains(hit.entity))
				{
					Hittable hittable = hit.entity as Hittable;
					float dmg = item.attackDamage;
					if (player != null)
						dmg *= player.attack;
					hittable.hit(dmg, shooter, item);
					hitEntities.Add(hit.entity);
				}
			}
			else
			{
				Debug.Assert(false);
			}
		}
	}

	public override void render()
	{
		bool flipped = direction.x < 0;
		Renderer.DrawSprite(position.x - 0.5f, position.y - 0.5f, 0, 1, 1, rotation, sprite, flipped);
	}
}

