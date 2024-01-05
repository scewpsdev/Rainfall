using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Bullet : Entity
{
	const float SPEED = 15;
	const float MAX_LIFETIME = 5.0f;


	public readonly Entity shooter;
	Vector2 direction;
	float size = 0.1f;

	long birthTime;


	public Bullet(Entity shooter, Vector2 position, Vector2 direction)
	{
		this.shooter = shooter;
		base.position = position;
		this.direction = direction;

		collider = new FloatRect(-0.5f * size, -0.5f * size, size, size);

		birthTime = Time.currentTime;
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
			((Enemy)entity).hit(this);
		}
	}

	public override void draw()
	{
		Renderer.DrawSprite(position.x - 0.5f * size, position.y - 0.5f * size, size, size, null, 0xFFFFFF77);
	}
}
