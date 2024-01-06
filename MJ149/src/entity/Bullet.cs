using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Bullet : Entity
{
	const float SPEED = 25;
	const float MAX_LIFETIME = 5.0f;


	public readonly Entity shooter;
	int damage;

	Vector2 direction;

	long birthTime;


	public Bullet(Entity shooter, int damage, Vector2 position, Vector2 direction)
	{
		this.shooter = shooter;
		this.damage = damage;
		base.position = position;
		this.direction = direction;

		size = new Vector2(0.1f);

		collider = new FloatRect(-0.5f * size.x, -0.5f * size.y, size.x, size.y);

		birthTime = Time.currentTime;
	}

	public override void reset()
	{
		removed = true;
	}

	public override void update()
	{
		Vector2 velocity = direction * SPEED;
		Vector2 delta = velocity * Time.deltaTime;

		CollisionDetection.DoWallCollision(position, collider, ref delta, level, out bool collidesX, out bool collidesY);
		if (collidesX || collidesY)
		{
			onWallHit();
			removed = true;
		}

		List<Entity> hitEntities = CollisionDetection.OverlapEntities(position, collider, level);
		if (hitEntities.Count > 0)
		{
			foreach (Entity hitEntity in hitEntities)
			{
				if (hitEntity != this && hitEntity != shooter)
				{
					onEnemyHit(hitEntity);
					removed = true;
					break;
				}
			}
		}

		if ((Time.currentTime - birthTime) / 1e9f >= MAX_LIFETIME)
		{
			removed = true;
		}

		position += delta;
	}

	void onWallHit()
	{
		// particles
		// sound effect
	}

	void onEnemyHit(Entity entity)
	{
		// particles
		// sound effect
		if (entity is Player)
		{
			((Player)entity).hit(this);
		}
		else if (entity is Enemy)
		{
			((Enemy)entity).hit(damage, this);
		}
	}

	public override void draw()
	{
		Renderer.DrawSprite(position.x - 0.5f * size.x, position.y - 0.5f * size.y, size.x, size.y, null, 0xFFFFFF77);
	}
}
