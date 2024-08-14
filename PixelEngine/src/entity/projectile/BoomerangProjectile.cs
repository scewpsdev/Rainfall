using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BoomerangProjectile : Entity
{
	int damage = 2;
	float speed = 12;
	float range = 4;

	Entity shooter;
	Sprite sprite;
	int direction;
	Vector2 startPosition;

	List<Entity> hitEntities = new List<Entity>();


	public BoomerangProjectile(int direction, Entity shooter)
	{
		this.direction = direction;
		this.shooter = shooter;

		velocity = new Vector2(direction * speed, 0);

		sprite = new Sprite(Item.tileset, 3, 1);
	}

	public override void init()
	{
		startPosition = position - velocity.normalized * 0.001f;
	}

	void drop()
	{
		GameState.instance.level.addEntity(new ItemEntity(new Boomerang(), null, velocity) { rotation = rotation }, position - velocity.normalized * 0.25f);
		remove();
	}

	public override void update()
	{
		float accSign = MathF.Sign(startPosition.x - position.x);
		float acc = speed * speed / (2 * range) * accSign;
		float lastVelocityX = velocity.x;
		velocity.x += acc * Time.deltaTime;
		if (MathF.Sign(velocity.x) != MathF.Sign(lastVelocityX))
			hitEntities.Clear();

		Vector2 displacement = velocity * Time.deltaTime;
		position += displacement;

		float newAccSign = MathF.Sign(startPosition.x - position.x);
		if (newAccSign != accSign)
		{
			velocity.x /= 1.5f;
			speed /= 1.5f;
			range /= 1.5f;
			if (range < 2)
				drop();
		}

		rotation += 3 * Time.deltaTime;
		rotation = (rotation + MathF.PI) % (MathF.PI * 2) - MathF.PI;

		HitData hit = GameState.instance.level.sample(position, FILTER_MOB | FILTER_PLAYER | FILTER_DEFAULT);
		if (hit != null)
		{
			if (hit.entity != null)
			{
				if (hit.entity == shooter)
				{
					Player player = hit.entity as Player;
					player.handItem = new Boomerang();
					remove();
				}
				else if (hit.entity is Hittable && !hitEntities.Contains(hit.entity))
				{
					Hittable hittable = hit.entity as Hittable;
					hittable.hit(damage, shooter);
					hitEntities.Add(hit.entity);
				}
			}
			else
			{
				drop();
			}
		}
	}

	public override void render()
	{
		bool flipped = direction == -1;
		Renderer.DrawSprite(position.x - 0.5f, position.y - 0.5f, 0, 1, 1, rotation, sprite, flipped);
	}
}
