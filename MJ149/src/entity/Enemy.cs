using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Enemy : Entity, Toucheable
{
	const int AI_TICKS = 10;


	float speed;
	float shootCooldown = 0.5f;

	Entity target;
	Vector2 targetPosition;
	List<Vector2i> currentPath = new List<Vector2i>();

	public int health = 10;

	long lastAITick = 0;
	long lastShootTime = 0;


	public Enemy(Vector2 position)
	{
		this.position = position;
		size = new Vector2(2, 2);
		collider = new FloatRect(-0.4f, -0.4f, 0.8f, 0.8f);
		hitbox = new FloatRect(-1, 0, 2, 2);

		speed = 4;
	}

	public override void reset()
	{
		removed = true;
	}

	public override void destroy()
	{
	}

	public void touch(Entity entity)
	{
		if (entity is Player)
		{
			Player player = entity as Player;
			player.hit(this);
		}
	}

	public void hit(int damage, Entity from)
	{
		health -= damage;
		if (health <= 0)
		{
			health = 0;
			onDeath(from);
			removed = true;
		}
	}

	void onDeath(Entity from)
	{
		if (from is Player)
		{
			Player player = from as Player;
			player.points += 100;
			Gaem.instance.manager.pointsEarned += 100;
			Gaem.instance.manager.enemiesKilled++;
		}
		else if (from is Bullet)
		{
			Player player = (Player)((Bullet)from).shooter;
			player.points += 100;
			Gaem.instance.manager.pointsEarned += 100;
			Gaem.instance.manager.enemiesKilled++;
		}
		Gaem.instance.manager.enemiesRemaining--;
	}

	void updateAI()
	{
		if (target == null)
		{
			target = level.findEntity<Player>();
		}
		if (target != null)
		{
			Vector2i currentTile = (Vector2i)position;
			Vector2i targetTile = (Vector2i)target.position;

			if (currentTile != targetTile)
			{
				if (!CollisionDetection.Linecast(currentTile + 0.5f, targetTile + 0.5f, level))
					targetPosition = targetTile + 0.5f;
				else if (level.astar.run(currentTile, targetTile, currentPath) && currentPath.Count > 1)
				{
					Vector2i nextTile = currentPath[1];
					Vector2 nextTileCenter = nextTile + 0.5f;
					targetPosition = nextTileCenter;
				}
			}
		}
	}

	void updateMovement()
	{
		Vector2 velocity = Vector2.Zero;

		if (target != null)
		{
			Vector2 input = targetPosition - position;

			if (input.lengthSquared > 0.0f)
			{
				velocity += input.normalized * speed;
			}
		}

		/*
		if (Input.IsKeyPressed(KeyCode.Space))
			lastJumpInput = Time.currentTime;
		if (grounded)
			lastGroundedTime = Time.currentTime;
		if ((Time.currentTime - lastGroundedTime) / 1e9f < COYOTE_TIME)
		{
			if ((Time.currentTime - lastJumpInput) / 1e9f < JUMP_BUFFER_TIME)
			{
				verticalVelocity = JUMP_POWER;
			}
		}
		*/

		Vector2 delta = velocity * Time.deltaTime;

		CollisionDetection.DoWallCollision(position, collider, ref delta, level, out bool collidesX, out bool collidesY);

		position += delta;
	}

	void updateActions()
	{
		bool shouldShoot = false;
		if (shouldShoot)
		{
			if ((Time.currentTime - lastShootTime) / 1e9f >= shootCooldown)
			{
				shoot();
				lastShootTime = Time.currentTime;
			}
		}
	}

	void shoot()
	{
		Vector2 target = targetPosition;
		Vector2 shootOrigin = position;
		Vector2 direction = (target - shootOrigin).normalized;
		level.addEntity(new Bullet(this, 1, shootOrigin, direction));
	}

	void updateAnimations()
	{
		//animator.update(sprite);
	}

	public override void update()
	{
		if ((Time.currentTime - lastAITick) >= 1.0f / AI_TICKS)
		{
			updateAI();
			lastAITick = Time.currentTime;
		}

		updateMovement();
		updateActions();
		updateAnimations();
	}

	public override void draw()
	{
		Renderer.DrawSprite(position.x - 0.5f * size.x, position.y, size.x, size.y, null, 0xFFFF7777);
	}
}
