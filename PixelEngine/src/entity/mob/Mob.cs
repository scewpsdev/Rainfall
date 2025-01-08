using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


struct StuckProjectile
{
	public string item;
	public Sprite sprite;
	public Vector2 relativePosition;
	public Vector2 direction;
	public float rotationOffset;
	public bool flipped;
}

public abstract class Mob : Entity, Hittable, StatusEffectReceiver
{
	static Sound[] mobHit = Resource.GetSounds("sounds/flesh", 2);
	static Sound[] mobDeath = Resource.GetSounds("sounds/death", 9);


	const float SPRINT_MULTIPLIER = 1.8f;
	const float STUN_DURATION = 0.4f;


	public float speed = 4;
	public float climbingSpeed = 4;
	public float jumpPower = 11;
	public float gravity = -30;

	public float itemDropChance = 0.1f;
	public float itemDropValueMultiplier = 1;
	public List<Item> itemDrops = new List<Item>();
	public float coinDropChance = 0.1f;
	public float spawnRate = 1;

	public float health = 1;
	public float damage = 1;
	public float poise = 1;
	public float awareness = 0.5f;

	protected float maxHealth;

	public bool isBoss = false;
	public bool canClimb = false;
	public bool canFly = false;
	public bool poisonResistant = false;

	public Sprite sprite;
	public Vector4 spriteColor = Vector4.One;
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
	public bool criticalStun = false;
	long stunTime = -1;
	public bool isVisible = true;

	Climbable currentLadder = null;

	public Item handItem = null;

	public Sound[] hitSound = mobHit;
	public Sound[] deathSound = mobDeath;

	public List<StatusEffect> statusEffects = new List<StatusEffect>();

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

	public virtual bool hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true, bool buffedHit = false)
	{
		health -= damage;

		if (damage >= 1)
			GameState.instance.level.addEntity(new DamageNumber((int)MathF.Floor(damage), new Vector2(MathHelper.RandomFloat(-1, 1), 1) * 3, buffedHit), new Vector2(MathHelper.RandomFloat(position.x + collider.min.x, position.x + collider.max.x), MathHelper.RandomFloat(position.y + collider.min.y, position.y + collider.max.y)));

		if (hitSound != null && (triggerInvincibility || health <= 0))
			Audio.PlayOrganic(hitSound, new Vector3(position, 0), 3);

		if (by != null)
		{
			Vector2 enemyPosition = by.position;
			GameState.instance.level.addEntity(ParticleEffects.CreateBloodEffect((position - enemyPosition).normalized), position + collider.center);

			if (item != null && item.projectileItem && item.projectileSticks && item.breakOnEnemyHit)
			{
				Vector2 relativePosition = new Vector2(MathHelper.Clamp(by.position.x - position.x, collider.min.x, collider.max.x), MathHelper.Clamp(by.position.y - position.y, collider.min.y, collider.max.y));
				Vector2 projectileDirection = (by.velocity.normalized + MathHelper.RandomVector2(-1, 1) * 0.1f).normalized;
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
				stuckProjectiles.Add(new StuckProjectile { item = item.name, sprite = item.sprite, relativePosition = relativePosition, direction = projectileDirection, rotationOffset = rotationOffset, flipped = flipped });
			}
		}

		if (health > 0)
		{
			if (damage >= poise)
			{
				stun();
			}
		}
		else
		{
			onDeath(by);
		}

		ai?.onHit(by);

		if ((Time.currentTime - lastHit) / 1e9f > 0.2f)
			lastHit = Time.currentTime;

		return true;
	}

	public virtual void onDeath(Entity by)
	{
		Player player = null;
		if (by is Player || by is ItemEntity && ((ItemEntity)by).thrower is Player || by is Projectile && ((Projectile)by).shooter is Player)
		{
			if (by is Player)
				player = by as Player;
			else if (by is ItemEntity)
				player = ((ItemEntity)by).thrower as Player;
			else if (by is Projectile)
				player = ((Projectile)by).shooter as Player;
			player.onKill(this);
		}

		if (ai != null)
			ai.onDeath();

		if (deathSound != null)
			Audio.PlayOrganic(deathSound, new Vector3(position, 0), 3);

		while (itemDropChance > 0 && Random.Shared.NextSingle() < itemDropChance)
		{
			Item[] items = Item.CreateRandom(Random.Shared, DropRates.mob, GameState.instance.level.avgLootValue * itemDropValueMultiplier);

			foreach (Item item in items)
			{
				Vector2 itemVelocity = new Vector2(MathHelper.RandomFloat(-0.2f, 0.2f), 0.5f) * 8;
				Vector2 throwOrigin = position + collider.center;
				ItemEntity obj = new ItemEntity(item, null, itemVelocity);
				GameState.instance.level.addEntity(obj, throwOrigin);
			}

			itemDropChance--;
		}
		for (int i = 0; i < itemDrops.Count; i++)
		{
			Vector2 itemVelocity = new Vector2(MathHelper.RandomFloat(-0.2f, 0.2f), 0.5f) * 8;
			Vector2 throwOrigin = position + collider.center;
			ItemEntity obj = new ItemEntity(itemDrops[i], null, itemVelocity);
			GameState.instance.level.addEntity(obj, throwOrigin);
		}
		if (Random.Shared.NextSingle() < coinDropChance)
		{
			int amount = MathHelper.RandomInt(1, Math.Max((int)MathF.Round(maxHealth / 2), 1));
			for (int i = 0; i < amount; i++)
			{
				Coin coin = new Coin();
				Vector2 spawnPosition = position + collider.center + Vector2.Rotate(Vector2.UnitX, i / (float)amount * 2 * MathF.PI) * 0.2f;
				coin.velocity = (spawnPosition - position - new Vector2(0, 0.5f)).normalized * 1;
				GameState.instance.level.addEntity(coin, spawnPosition);
			}
		}
		for (int i = 0; i < stuckProjectiles.Count; i++)
		{
			float dropChance = 0.5f;
			if (Random.Shared.NextSingle() < dropChance)
			{
				Vector2 itemVelocity = new Vector2(MathHelper.RandomFloat(-0.2f, 0.2f), 0.5f) * 8;
				Vector2 throwOrigin = position + collider.center;
				ItemEntity obj = new ItemEntity(Item.GetItemPrototype(stuckProjectiles[i].item).copy(), null, itemVelocity);
				GameState.instance.level.addEntity(obj, throwOrigin);
			}
		}

		if (player != null)
		{
			for (int i = 0; i < maxHealth; i++)
			{
				XPOrb orb = new XPOrb();
				Vector2 pos = position + collider.center + Vector2.Rotate(Vector2.Right, i / maxHealth * MathF.PI * 2) * (0.5f + i / maxHealth); // new Vector2(MathHelper.RandomFloat(collider.min.x, collider.max.x), MathHelper.RandomFloat(collider.min.y, collider.max.y));
				orb.velocity = (pos - (position + collider.center)).normalized * 3;
				GameState.instance.level.addEntity(orb, pos);
			}
		}

		for (int i = 0; i < statusEffects.Count; i++)
			statusEffects[i].destroy(this);
		statusEffects.Clear();

		GameState.instance.level.addEntity(new MobCorpse(sprite, spriteColor * new Vector4(0.5f, 0.5f, 0.5f, 0.5f), animator, rect, direction, Vector2.Zero, impulseVelocity, collider), position);

		remove();
	}

	public void stun(float stunDuration = 1, bool critical = false)
	{
		if (stunTime == -1 || (Time.currentTime - stunTime) / 1e9f > STUN_DURATION)
		{
			stunTime = Time.currentTime + (long)((stunDuration - 1) * STUN_DURATION * 1e9f);
			criticalStun = critical;
			isStunned = true;
		}
	}

	public StatusEffect addStatusEffect(StatusEffect effect)
	{
		statusEffects.Add(effect);
		effect.init(this);
		return effect;
	}

	public void heal(float amount)
	{
		health += amount;
	}

	public void setVisible(bool visible)
	{
		isVisible = visible;
	}

	public void addImpulse(Vector2 impulse)
	{
		impulseVelocity.x += impulse.x;
		velocity.y += impulse.y;
	}

	void updateMovement()
	{
		Vector2 delta = Vector2.Zero;

		if ((Time.currentTime - stunTime) / 1e9f > STUN_DURATION)
			isStunned = false;
		criticalStun = criticalStun && isStunned;

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
				if (Vector2.Rotate(delta, -rotation).x > 0)
					direction = 1;
				else if (Vector2.Rotate(delta, -rotation).x < 0)
					direction = -1;

				currentSpeed = isSprinting ? SPRINT_MULTIPLIER * speed : speed;
				velocity.x = delta.x * currentSpeed;

				if (canFly)
					velocity.y = MathHelper.Lerp(velocity.y, delta.y * currentSpeed, 5 * Time.deltaTime);
				else if (gravity == 0)
					velocity.y = delta.y * currentSpeed;
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
		// lol idk just found this in player too wtf
		//float rotationDst = direction == 1 ? 0 : MathF.PI;
		//rotation = MathHelper.Lerp(rotation, rotationDst, 5 * Time.deltaTime);
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

		maxHealth = MathF.Max(maxHealth, health);

		if (ai != null)
			ai.update();

		updateMovement();
		updateActions();
		updateAnimation();

		for (int i = 0; i < statusEffects.Count; i++)
		{
			if (!statusEffects[i].update(this) && isAlive)
			{
				statusEffects[i].destroy(this);
				statusEffects.RemoveAt(i--);
			}
		}
	}

	public override void render()
	{
		if (isVisible)
		{
			bool hitMarker = lastHit != -1 && (Time.currentTime - lastHit) / 1e9f < 0.1f;

			if (sprite != null)
			{
				if (hitMarker)
					Renderer.DrawSpriteSolid(position.x + rect.position.x, position.y + rect.position.y, LAYER_DEFAULT, rect.size.x, rect.size.y, rotation, sprite, direction == -1, 0xFFFFFFFF);
				else
					Renderer.DrawSprite(position.x + rect.position.x, position.y + rect.position.y, LAYER_DEFAULT, rect.size.x, rect.size.y, rotation, sprite, direction == -1, spriteColor);

				if (outline != 0)
					Renderer.DrawOutline(position.x + rect.position.x, position.y + rect.position.y, LAYER_BG, rect.size.x, rect.size.y, rotation, sprite, direction == -1, outline);

				for (int i = 0; i < statusEffects.Count; i++)
				{
					statusEffects[i].render(this);
				}

				for (int i = 0; i < stuckProjectiles.Count; i++)
				{
					StuckProjectile projectile = stuckProjectiles[i];
					Renderer.DrawSprite(position.x + projectile.relativePosition.x * direction - 0.5f, position.y + projectile.relativePosition.y - 0.5f, LAYER_BG, 1, 1, (projectile.direction.angle + projectile.rotationOffset) * direction, projectile.sprite, direction == -1);
				}
			}
		}
	}

	public bool isAlive
	{
		get => health > 0;
	}
}
