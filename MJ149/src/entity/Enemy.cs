using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Enemy : Entity
{
	const int AI_TICKS = 10;


	float speed;
	float shootCooldown = 0.5f;

	Entity target;
	Vector2 targetPosition;

	public int health = 5;

	long lastAITick = 0;
	long lastShootTime = 0;


	public Enemy(Vector2 position)
	{
		this.position = position;
		collider = new FloatRect(-0.5f, -0.5f, 1, 1);

		speed = 5;
	}

	public override void destroy()
	{
	}

	public void hit(Entity from)
	{
		health--;
		if (health == 0)
		{
			onDeath(from);
			removed = true;
		}
	}

	void onDeath(Entity from)
	{
		if (from is Player)
		{
			((Player)from).points += 100;
		}
		else if (from is Bullet)
		{
			((Player)((Bullet)from).shooter).points += 100;
		}
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
			List<Vector2i> path = AStar.Run(currentTile, targetTile, level.width, level.height, level.walkable);
			if (path != null && path.Count > 1)
			{
				Vector2i nextTile = path[1];
				Vector2 nextTileCenter = nextTile + 0.5f;
				targetPosition = nextTileCenter;
			}
		}
	}

	void updateMovement()
	{
		Vector2 velocity = Vector2.Zero;

		if (target != null)
		{
			Vector2i input = Vector2i.Zero;
			if (targetPosition.x - position.x > 0.01f)
				input.x++;
			if (targetPosition.x - position.x < -0.01f)
				input.x--;
			if (targetPosition.y - position.y > 0.01f)
				input.y++;
			if (targetPosition.y - position.y < -0.01f)
				input.y--;

			//if (Input.IsKeyDown(KeyCode.KeyA))
			//	input.x--;
			//if (Input.IsKeyDown(KeyCode.KeyD))
			//	input.x++;
			//if (Input.IsKeyDown(KeyCode.KeyS))
			//	input.y--;
			//if (Input.IsKeyDown(KeyCode.KeyW))
			//	input.y++;

			if (input.x != 0 || input.y != 0)
			{
				velocity += ((Vector2)input).normalized * speed;
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
		level.addEntity(new Bullet(this, shootOrigin, direction));
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
		Renderer.DrawSprite(position.x - 1, position.y - 1, 2, 2, null, 0xFFFF7777);
	}
}
