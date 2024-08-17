using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract class Mob : Entity, Hittable
{
	const float SPRINT_MULTIPLIER = 1.8f;
	const float STUN_DURATION = 0.4f;


	public float speed = 4;
	public float climbingSpeed = 4;
	public float jumpPower = 12;
	public float gravity = -30;

	public float itemDropChance = 0.05f;

	public float health = 1;
	public int damage = 1;
	public bool canFly = false;

	protected Sprite sprite;
	public SpriteAnimator animator;
	protected FloatRect rect = new FloatRect(-0.5f, 0.0f, 1, 1);
	protected uint outline = 0;

	protected AI ai;

	public bool inputLeft, inputRight, inputUp, inputDown;
	public bool inputSprint, inputJump;

	public int direction = 1;
	float currentSpeed;
	Vector2 impulseVelocity;
	public bool isGrounded = false;
	bool isSprinting = false;
	bool isClimbing = false;
	float distanceWalked = 0;
	bool isStunned = false;
	long stunTime = -1;

	Climbable currentLadder = null;

	public Item handItem = null;

	long lastHit = -1;


	public Mob(string name)
	{
		this.name = name;

		filterGroup = FILTER_MOB;
	}

	public override void destroy()
	{
	}

	public void hit(float damage, Entity by, Item item)
	{
		health -= damage;

		if (health > 0)
		{
			stun();

			if (by != null)
			{
				Vector2 enemyPosition = by.position;
				if (by.collider != null)
					enemyPosition += 0.5f * (by.collider.max + by.collider.min);
				float knockbackStrength = item != null ? item.knockback : 8.0f;
				Vector2 knockback = (position - enemyPosition).normalized * knockbackStrength;
				addImpulse(knockback);
			}
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
			GameState.instance.run.kills++;

		if (Random.Shared.NextSingle() < itemDropChance)
		{
			Item item = Item.CreateRandom(Random.Shared);

			Vector2 itemVelocity = new Vector2(0, 1) * 8;
			Vector2 throwOrigin = position + new Vector2(0, 0.5f);
			ItemEntity obj = new ItemEntity(item, null, itemVelocity);
			GameState.instance.level.addEntity(obj, throwOrigin);
		}

		remove();
	}

	public void stun()
	{
		if (stunTime == -1 || (Time.currentTime - stunTime) / 1e9f > STUN_DURATION)
			stunTime = Time.currentTime;
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
			if (MathF.Sign(impulseVelocity.x) == MathF.Sign(velocity.x))
				impulseVelocity.x = 0;
			else if (velocity.x == 0)
				impulseVelocity.x = MathF.Sign(impulseVelocity.x) * MathF.Min(MathF.Abs(impulseVelocity.x), speed);
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

		float rotationDst = direction == 1 ? 0 : MathF.PI;
		rotation = MathHelper.Lerp(rotation, rotationDst, 5 * Time.deltaTime);
	}

	void updateActions()
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
		if (ai != null)
			ai.update(this);

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
				Renderer.DrawSpriteSolid(position.x + rect.position.x, position.y + rect.position.y, 0, rect.size.x, rect.size.y, 0, sprite, direction == -1, 0xFFFFFFFF);
			else
				Renderer.DrawSprite(position.x + rect.position.x, position.y + rect.position.y, 0, rect.size.x, rect.size.y, 0, sprite, direction == -1, 0xFFFFFFFF);

			if (outline != 0)
				Renderer.DrawOutline(position.x + rect.position.x, position.y + rect.position.y, 0, rect.size.x, rect.size.y, 0, sprite, direction == -1, outline);
		}
	}
}
