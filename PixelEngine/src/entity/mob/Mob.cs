using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


struct StuckProjectile
{
	public Sprite sprite;
	public Vector2 relativePosition;
	public Vector2 direction;
	public float rotationOffset;
	public bool flipped;
}

public abstract class Mob : Entity, Hittable
{
	const float SPRINT_MULTIPLIER = 1.8f;
	const float STUN_DURATION = 0.4f;


	public float speed = 4;
	public float climbingSpeed = 4;
	public float jumpPower = 12;
	public float gravity = -30;

	public float itemDropChance = 0.1f;
	public float coinDropChance = 0.2f;

	public float health = 1;
	public float damage = 1;
	public bool canClimb = false;
	public bool canFly = false;

	public Sprite sprite;
	public SpriteAnimator animator;
	public FloatRect rect = new FloatRect(-0.5f, 0.0f, 1, 1);
	protected uint outline = 0;

	public AI ai;

	public bool inputLeft, inputRight, inputUp, inputDown;
	public bool inputSprint, inputJump;

	public int direction = 1;
	float currentSpeed;
	public Vector2 impulseVelocity;
	public bool isGrounded = false;
	bool isSprinting = false;
	bool isClimbing = false;
	float distanceWalked = 0;
	public bool isStunned = false;
	long stunTime = -1;

	Climbable currentLadder = null;

	public Item handItem = null;

	long lastHit = -1;

	List<StuckProjectile> stuckProjectiles = new List<StuckProjectile>();


	public Mob(string name)
	{
		this.name = name;

		filterGroup = FILTER_MOB;
	}

	public override void destroy()
	{
	}

	public void hit(float damage, Entity by, Item item, string byName, bool triggerInvincibility)
	{
		health -= damage;

		if (by != null)
		{
			Vector2 enemyPosition = by.position;
			if (by.collider != null)
				enemyPosition += 0.5f * (by.collider.max + by.collider.min);
			float knockbackStrength = item != null ? item.knockback : 8.0f;
			Vector2 knockback = (position - enemyPosition).normalized * knockbackStrength;
			addImpulse(knockback);

			GameState.instance.level.addEntity(Effects.CreateBloodEffect((position - enemyPosition).normalized), position + collider.center);

			if (item != null && item.projectileItem && item.breakOnEnemyHit)
			{
				Vector2 relativePosition = new Vector2(MathHelper.Clamp(by.position.x - position.x, collider.min.x, collider.max.x), MathHelper.Clamp(by.position.y - position.y, collider.min.y, collider.max.y));
				Vector2 projectileDirection = by.velocity.normalized;
				float rotationOffset = item.projectileRotationOffset;
				bool flipped = false;
				if (direction == -1)
				{
					relativePosition.x *= -1;
					projectileDirection.x *= -1;
					rotationOffset *= -1;
					if (item.projectileSpins && by.velocity.x < 0)
						flipped = !flipped;
				}
				stuckProjectiles.Add(new StuckProjectile { sprite = item.sprite, relativePosition = relativePosition, direction = projectileDirection, rotationOffset = rotationOffset, flipped = flipped });
			}
		}

		if (health > 0)
		{
			stun();
		}
		else
		{
			onDeath(by);
		}

		ai?.onHit(by);

		lastHit = Time.currentTime;
	}

	void onDeath(Entity by)
	{
		if (by is Player || by is ItemEntity && ((ItemEntity)by).thrower is Player)
		{
			Player player = null;
			if (by is Player)
				player = by as Player;
			else if (by is ItemEntity)
				player = ((ItemEntity)by).thrower as Player;
			player.onKill(this);
		}

		if (Random.Shared.NextSingle() < itemDropChance)
		{
			Item[] items = Item.CreateRandom(Random.Shared, DropRates.mob);

			foreach (Item item in items)
			{
				Vector2 itemVelocity = new Vector2(MathHelper.RandomFloat(-0.2f, 0.2f), 0.5f) * 8;
				Vector2 throwOrigin = position + new Vector2(0, 0.5f);
				ItemEntity obj = new ItemEntity(item, null, itemVelocity);
				GameState.instance.level.addEntity(obj, throwOrigin);
			}
		}
		if (Random.Shared.NextSingle() < coinDropChance)
		{
			int amount = MathHelper.RandomInt(1, 8);
			for (int i = 0; i < amount; i++)
			{
				Coin coin = new Coin();
				Vector2 spawnPosition = position + collider.center + Vector2.Rotate(Vector2.UnitX, i / (float)amount * 2 * MathF.PI) * 0.2f;
				coin.velocity = (spawnPosition - position - new Vector2(0, 0.5f)).normalized * 4;
				GameState.instance.level.addEntity(coin, spawnPosition);
			}
		}

		GameState.instance.level.addEntity(new MobCorpse(sprite, animator, rect, direction, velocity, impulseVelocity, collider, 0xFF7F7F7F), position);

		remove();
	}

	public void stun(float stunDuration = 1)
	{
		if (stunTime == -1 || (Time.currentTime - stunTime) / 1e9f > STUN_DURATION)
			stunTime = Time.currentTime + (long)((stunDuration - 1) * STUN_DURATION * 1e9f);
	}

	public void addImpulse(Vector2 impulse)
	{
		impulseVelocity.x += impulse.x;
		velocity.y += impulse.y;
	}

	void updateMovement()
	{
		Vector2 delta = Vector2.Zero;

		isStunned = stunTime != -1 && (Time.currentTime - stunTime) / 1e9f < STUN_DURATION;

		if (!isStunned)
		{
			if (inputLeft)
				delta.x--;
			if (inputRight)
				delta.x++;
			if (inputUp)
				delta.y++;
			if (inputDown)
				delta.y--;
			if (isClimbing)
			{
				if (inputUp)
				{
					if (GameState.instance.level.getClimbable(position + new Vector2(0, 0.2f)) != null)
						delta.y++;
				}
				if (inputDown)
					delta.y--;
			}

			isSprinting = inputSprint;

			if (inputJump)
			{
				if (isGrounded)
				{
					velocity.y = jumpPower;
				}
				else if (isClimbing)
				{
					velocity.y = jumpPower;
					currentLadder = null;
					isClimbing = false;
				}
			}
		}

		if (delta.lengthSquared > 0)
		{
			//if (isGrounded)
			{
				if (delta.x > 0)
					direction = 1;
				else if (delta.x < 0)
					direction = -1;

				currentSpeed = isSprinting ? SPRINT_MULTIPLIER * speed : speed;
				velocity.x = delta.x * currentSpeed;

				if (canFly)
					velocity.y = MathHelper.Lerp(velocity.y, delta.y * currentSpeed, 5 * Time.deltaTime);
			}
		}
		else
		{
			//if (isGrounded)
			velocity.x = 0.0f;
			if (canFly)
				velocity.y = 0.0f;
		}

		if (!isClimbing)
		{
			velocity.y += gravity * Time.deltaTime;

			impulseVelocity.x = MathHelper.Lerp(impulseVelocity.x, 0, 8 * Time.deltaTime);
			if (MathF.Abs(impulseVelocity.x) < 0.01f)
				impulseVelocity.x = 0;
			if (MathF.Sign(impulseVelocity.x) == MathF.Sign(velocity.x))
				velocity.x = 0;
			//else if (velocity.x == 0)
			//	impulseVelocity.x = MathF.Sign(impulseVelocity.x) * MathF.Min(MathF.Abs(impulseVelocity.x), speed);
			//impulseVelocity.x = impulseVelocity.x - velocity.x;
			velocity += impulseVelocity;
		}
		else
		{
			velocity.y = delta.y * climbingSpeed;
		}

		Vector2 displacement = velocity * Time.deltaTime;
		int collisionFlags = GameState.instance.level.doCollision(ref position, collider, ref displacement, inputDown);

		isGrounded = false;
		if ((collisionFlags & Level.COLLISION_Y) != 0)
		{
			if (velocity.y < 0)
				isGrounded = true;

			velocity.y = 0;
			impulseVelocity.y = 0;
			//impulseVelocity.x *= 0.5f;
		}
		if ((collisionFlags & Level.COLLISION_X) != 0)
		{
			impulseVelocity.x = 0;
			impulseVelocity.y *= 0.5f;
		}

		position += displacement;
		distanceWalked += MathF.Abs(displacement.x);

		// why is this here?
		float rotationDst = direction == 1 ? 0 : MathF.PI;
		rotation = MathHelper.Lerp(rotation, rotationDst, 5 * Time.deltaTime);
	}

	void updateActions()
	{
		if (canClimb)
		{
			Climbable hoveredLadder = GameState.instance.level.getClimbable(position + new Vector2(0, 0.1f));
			if (currentLadder == null)
			{
				if (hoveredLadder != null && inputUp)
				{
					currentLadder = hoveredLadder;
					isClimbing = true;
					velocity = Vector2.Zero;
				}
			}
			else
			{
				if (hoveredLadder == null)
				{
					currentLadder = null;
					isClimbing = false;
				}
			}
		}

		if (handItem != null)
		{
			/*
			if (Input.IsKeyPressed(KeyCode.X))
			{
				Input.ConsumeKeyEvent(KeyCode.X);
				handItem.type.use(handItem, this);
			}
			*/
		}

		//actions.update();
	}

	void updateAnimation()
	{
		animator?.update(sprite);
	}

	public override void update()
	{
		if (!isAlive)
			return;

		if (ai != null)
			ai.update();

		updateMovement();
		updateActions();
		updateAnimation();
	}

	public override void render()
	{
		bool hitMarker = lastHit != -1 && (Time.currentTime - lastHit) / 1e9f < 0.1f;

		if (sprite != null)
		{
			if (hitMarker)
				Renderer.DrawSpriteSolid(position.x + rect.position.x, position.y + rect.position.y, LAYER_DEFAULT, rect.size.x, rect.size.y, 0, sprite, direction == -1, 0xFFFFFFFF);
			else
				Renderer.DrawSprite(position.x + rect.position.x, position.y + rect.position.y, LAYER_DEFAULT, rect.size.x, rect.size.y, 0, sprite, direction == -1, 0xFFFFFFFF);

			if (outline != 0)
				Renderer.DrawOutline(position.x + rect.position.x, position.y + rect.position.y, LAYER_BGBG, rect.size.x, rect.size.y, 0, sprite, direction == -1, outline);

			for (int i = 0; i < stuckProjectiles.Count; i++)
			{
				StuckProjectile projectile = stuckProjectiles[i];
				Renderer.DrawSprite(position.x + projectile.relativePosition.x * direction - 0.5f, position.y + projectile.relativePosition.y - 0.5f, LAYER_BG, 1, 1, (projectile.direction.angle + projectile.rotationOffset) * direction, projectile.sprite, direction == -1);
			}
		}
	}

	public bool isAlive
	{
		get => health > 0;
	}
}
