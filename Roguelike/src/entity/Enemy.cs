using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


public class Enemy : Entity, Toucheable
{
	const int AI_TICKS = 2;
	const float DESPAWN_DELAY = 10.0f;


	static Texture shadow;

	static Enemy()
	{
		shadow = Resource.GetTexture("res/sprites/shadow.png", true);
	}


	SpriteSheet sheet;
	Sprite sprite;
	SpriteAnimator animator;
	bool direction = true;
	bool running = false;
	int lastStep = 0;

	float speed;
	float shootCooldown = 0.5f;
	Vector2 knockback;

	Entity target;
	Vector2 targetPosition;
	List<Vector2i> currentPath = new List<Vector2i>();

	AudioSource audio;
	Sound[] sfxStep;
	Sound[] sfxHit;

	public int health;

	long lastAITick = 0;
	long lastShootTime = 0;
	long deathTime = 0;


	public Enemy(Vector2 position)
	{
		this.position = position;
		size = new Vector2(4);
		collider = new FloatRect(-0.4f, -0.4f, 0.8f, 0.8f);
		hitbox = new FloatRect(-1, 0, 2, 2);

		sheet = new SpriteSheet(Resource.GetTexture("res/sprites/enemy.png", false), 32, 32);
		sprite = new Sprite(sheet, 0, 0);

		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 1, 0, 2, 1, true);
		animator.addAnimation("run", 2, 0, 1, 0, 8, 12, true);
		animator.addAnimation("death", 13, 0, 1, 0, 7, 12, false);
		animator.setAnimation("idle");

		audio = new AudioSource(new Vector3(position, 0.0f));

		sfxStep = [
			Resource.GetSound("res/sounds/step1.ogg"),
			Resource.GetSound("res/sounds/step2.ogg"),
		];
		sfxHit = [
			Resource.GetSound("res/sounds/hit1.ogg"),
			Resource.GetSound("res/sounds/hit2.ogg"),
		];

		speed = Gaem.instance.manager.enemySpeed;
		health = Gaem.instance.manager.enemyHealth;
	}

	public override void reset()
	{
		removed = true;
	}

	public override void destroy()
	{
		audio.destroy();
	}

	public void touch(Entity entity)
	{
		if (health == 0)
			return;

		if (entity is Player)
		{
			Player player = entity as Player;
			player.hit(this);
		}
	}

	public void hit(int damage, Entity from)
	{
		if (health == 0)
			return;

		health -= damage;

		if (from is Bullet)
			from = ((Bullet)from).shooter;

		Vector2 knockbackDirection = (from.position - position).normalized;

		if (from is Player)
		{
			Player player = from as Player;
			knockback = -knockbackDirection * player.knockback;
		}

		level.addEntity(new Effect("bleed", (-knockbackDirection).angle, 0xFF000000, position));

		audio.playSoundOrganic(sfxHit);

		if (health <= 0)
		{
			health = 0;
			onDeath(from);
		}
	}

	void onDeath(Entity from)
	{
		if (from is Bullet)
			from = ((Bullet)from).shooter;

		if (from != null)
		{
			int numXP = (Gaem.instance.player.luck + MathHelper.RandomInt(-Gaem.instance.player.luck / 2, Gaem.instance.player.luck / 2)) * 10;
			while (numXP > 0)
			{
				Vector2 offset = MathHelper.RandomVector2(-0.5f, 0.5f);
				int amount = numXP < 300 ? 10 : numXP < 800 ? (Random.Shared.Next() % 4 == 0 ? 100 : 10) : numXP < 5000 ? (Random.Shared.Next() % 4 == 0 ? 1000 : Random.Shared.Next() % 4 == 0 ? 100 : 10) : 1000;
				level.addEntity(new XPOrb(position + offset, from, amount));
				numXP -= amount;
			}

			float dropChance = 0.0001f * Gaem.instance.player.luck;
			if (Random.Shared.NextSingle() < dropChance)
			{
				DropType type = (DropType)(Random.Shared.Next() % (int)DropType.Count);
				level.addEntity(new Drop(type, position));
			}
		}

		Gaem.instance.manager.enemiesKilled++;
		Gaem.instance.manager.enemiesRemaining--;

		colliderEnabled = false;
		deathTime = Time.currentTime;

		animator.setAnimation("death");
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

		if (health > 0 && target != null)
		{
			Vector2 input = targetPosition - position;

			if (input.lengthSquared > 0.0f)
			{
				if (input.x > 0)
					direction = true;
				else if (input.x < 0)
					direction = false;
				velocity += input.normalized * speed;

				running = true;
			}
			else
			{
				running = false;
			}
		}

		knockback = Vector2.Lerp(knockback, Vector2.Zero, 5.0f * Time.deltaTime);
		velocity += knockback;

		List<Entity> nearbyEntities = CollisionDetection.OverlapEntities(position, collider, level);
		if (nearbyEntities.Count > 1)
		{
			Entity closestEntity = nearbyEntities[1];
			if (closestEntity != this && closestEntity is Enemy)
			{
				Vector2 toEntity = closestEntity.position - position;
				float distance = toEntity.length;
				if (distance < 0.0001f)
					velocity -= Vector2.UnitX * 0.3f / Time.deltaTime;
				else if (distance < 0.5f)
					velocity -= toEntity.normalized / Time.deltaTime;
			}
		}

		Vector2i tilePos = (Vector2i)position;
		if (Gaem.instance.level.getTile(tilePos.x, tilePos.y) != 0)
			hit(10000, null);

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
		if (health > 0)
		{
			if (running)
			{
				animator.setAnimation("run");

				if (animator.currentFrame == 3 || animator.currentFrame == 7)
				{
					if (lastStep != animator.currentFrame)
					{
						audio.playSoundOrganic(sfxStep, 1, 1, 0.2f, 0.25f, 5.0f);
						lastStep = animator.currentFrame;
					}
				}
			}
			else
				animator.setAnimation("idle");
		}

		animator.update(sprite);
	}

	public override void update()
	{
		if (health == 0)
		{
			if ((Time.currentTime - deathTime) / 1e9f >= DESPAWN_DELAY)
				removed = true;

			updateMovement();
			updateAnimations();
			return;
		}

		if ((Time.currentTime - lastAITick) >= 1.0f / AI_TICKS)
		{
			updateAI();
			lastAITick = Time.currentTime;
		}

		updateMovement();
		updateActions();
		updateAnimations();

		audio.updateTransform(new Vector3(position, 0.0f));
	}

	public override void draw()
	{
		Renderer.DrawVerticalSprite(position.x - 0.5f * size.x, position.y, size.x, size.y, sprite, !direction);
		Renderer.DrawSprite(position.x - 0.5f, position.y - 0.5f, 0.01f, 1, 1, shadow, 0, 0, 8, 8, 0xFFFFFFFF);
	}
}
