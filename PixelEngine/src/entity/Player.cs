using Rainfall;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Player : Entity, Hittable
{
	const float JUMP_BUFFER = 0.3f;
	const float COYOTE_TIME = 0.2f;
	const float SPRINT_MULTIPLIER = 1.8f;
	const float DUCKED_MULTIPLIER = 0.8f;
	const float MAX_FALL_SPEED = -15;
	const float HIT_COOLDOWN = 1.0f;
	const float STUN_DURATION = 1.0f;
	const float FALL_DAMAGE_DISTANCE = 8;


	float speed = 7;
	float climbingSpeed = 4;
	float jumpPower = 10;
	float gravity = -22;

	public int direction = 1;
	float currentSpeed;
	Vector2 impulseVelocity;
	public bool isGrounded = false;
	bool isMoving = false;
	bool isSprinting = false;
	public bool isDucked = false;
	bool isClimbing = false;
	float fallDistance = 0;

	// Status effects
	bool isStunned = false;

	Sprite stunnedIcon;

	Sprite sprite;
	SpriteAnimator animator;

	long lastJumpInput = -10000000000;
	long lastGrounded = -10000000000;

	public int maxHealth = 3;
	public int health = 3;

	public int money = 0;

	long lastHit = 0;
	public long deathTime = -1;
	long stunTime = -1;

	public ActionQueue actions;

	Interactable interactableInFocus = null;
	Climbable currentLadder = null;
	Climbable lastLadderJumpedFrom = null;

	public Item handItem = null;
	public Item[] quickItems = new Item[4];
	public int currentQuickItem = 0;
	public Item[] passiveItems = new Item[4];

	HUD hud;
	InventoryUI inventoryUI;
	public bool inventoryOpen = false;


	public Player()
	{
		actions = new ActionQueue(this);

		collider = new FloatRect(-0.2f, 0, 0.4f, 0.75f);
		filterGroup = FILTER_PLAYER;

		sprite = new Sprite(Resource.GetTexture("res/sprites/player.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();

		animator.addAnimation("idle", 0, 0, 16, 0, 4, 5, true);
		animator.addAnimation("run", 4 * 16, 0, 16, 0, 8, 10, true);
		animator.addAnimation("jump", 12 * 16, 0, 16, 0, 1, 12, true);
		animator.addAnimation("fall", 13 * 16, 0, 16, 0, 1, 12, true);
		animator.addAnimation("climb", 14 * 16, 0, 16, 0, 2, 4, true);
		animator.addAnimation("dead", 16 * 16, 0, 16, 0, 1, 12, true);
		animator.addAnimation("stun", 17 * 16, 0, 16, 0, 1, 1, true);

		stunnedIcon = new Sprite(Resource.GetTexture("res/sprites/status_stun.png", false));

		hud = new HUD(this);
		inventoryUI = new InventoryUI(this);

		handItem = new Boomerang();
		quickItems[0] = new HealthPotion();
	}

	public override void destroy()
	{
	}

	public bool pickupObject(ItemEntity obj)
	{
		if (obj.item.type == ItemType.Tool)
		{
			if (handItem != null)
				throwItem(handItem, true);
			handItem = obj.item;
			return true;
		}
		else if (obj.item.type == ItemType.Active)
		{
			if (obj.item.stackable)
			{
				for (int i = 0; i < quickItems.Length; i++)
				{
					if (quickItems[i] != null && quickItems[i].id == obj.item.id)
					{
						quickItems[i].stackSize += obj.item.stackSize;
						return true;
					}
				}
			}
			for (int i = 0; i < quickItems.Length; i++)
			{
				if (quickItems[i] == null)
				{
					quickItems[i] = obj.item;
					return true;
				}
			}
			return false;
		}
		else
		{
			for (int i = 0; i < passiveItems.Length; i++)
			{
				if (passiveItems[i] == null)
				{
					passiveItems[i] = obj.item;
					return true;
				}
			}
			return false;
		}
	}

	public void throwItem(Item item, bool shortThrow = false)
	{
		Vector2 itemVelocity = velocity;
		if (Input.IsKeyDown(KeyCode.Up))
		{
			itemVelocity += new Vector2(direction * 0.05f, 1.0f) * 14;
		}
		else if (Input.IsKeyDown(KeyCode.Down))
		{
			itemVelocity += new Vector2(direction * 0.05f, -1.0f) * 14;
			if (!isGrounded)
				velocity.y = MathF.Max(velocity.y, 0) + 5.0f;
		}
		else
		{
			itemVelocity += new Vector2(direction, 1) * (shortThrow ? new Vector2(0.4f, 0.1f) : new Vector2(1, 0.1f)) * 14;
		}
		Vector2 throwOrigin = position + new Vector2(0, shortThrow ? 0.25f : 0.5f);
		ItemEntity obj = new ItemEntity(item, this, itemVelocity);
		GameState.instance.level.addEntity(obj, throwOrigin);

		if (item == handItem)
			handItem = null;
	}

	public void hit(int damage, Entity by)
	{
		bool invincible = (Time.currentTime - lastHit) / 1e9f < HIT_COOLDOWN;
		if (!invincible)
		{
			health -= damage;

			Vector2 enemyPosition = by.position;
			if (by.collider != null)
				enemyPosition += 0.5f * (by.collider.max + by.collider.min);
			Vector2 knockback = (position - enemyPosition).normalized * 4.0f;
			addImpulse(knockback);

			if (health <= 0)
			{
				onDeath(by);
				deathTime = Time.currentTime;
			}

			lastHit = Time.currentTime;
		}
	}

	void stun()
	{
		isStunned = true;
		stunTime = Time.currentTime;
	}

	public void addImpulse(Vector2 impulse)
	{
		impulseVelocity.x += impulse.x;
		velocity.y += impulse.y;
	}

	void onDeath(Entity by)
	{
		if (handItem != null)
			throwItem(handItem);

		GameState.instance.run.active = false;
		GameState.instance.run.killedBy = by;
	}

	void updateMovement()
	{
		Vector2 delta = Vector2.Zero;

		if (isStunned)
		{
			if ((Time.currentTime - stunTime) / 1e9f > STUN_DURATION)
			{
				isStunned = false;
				stunTime = -1;
			}
		}

		if (isAlive && !isStunned)
		{
			if (Input.IsKeyDown(KeyCode.Left))
				delta.x--;
			if (Input.IsKeyDown(KeyCode.Right))
				delta.x++;
			if (isClimbing)
			{
				if (Input.IsKeyDown(KeyCode.Up))
				{
					if (GameState.instance.level.getClimbable(position + new Vector2(0, 0.2f)) != null)
						delta.y++;
				}
				if (Input.IsKeyDown(KeyCode.Down))
					delta.y--;
			}

			isSprinting = Input.IsKeyDown(KeyCode.Shift);

			isDucked = Input.IsKeyDown(KeyCode.Down);
			collider.size.y = isDucked ? 0.4f : 0.8f;

			if (isGrounded)
				lastGrounded = Time.currentTime;
			if (Input.IsKeyPressed(KeyCode.C))
			{
				if (isClimbing)
				{
					velocity.y = jumpPower;
					lastJumpInput = 0;
					lastGrounded = 0;
					lastLadderJumpedFrom = currentLadder;
					currentLadder = null;
					isClimbing = false;
				}
				else
				{
					lastJumpInput = Time.currentTime;
					if (isGrounded || (Time.currentTime - lastGrounded) / 1e9f < COYOTE_TIME)
					{
						velocity.y = jumpPower;
						lastJumpInput = 0;
						lastGrounded = 0;
					}
				}
			}
			else if ((Time.currentTime - lastJumpInput) / 1e9f < JUMP_BUFFER)
			{
				if (isGrounded || (Time.currentTime - lastGrounded) / 1e9f < COYOTE_TIME)
				{
					velocity.y = jumpPower;
					lastJumpInput = 0;
					lastGrounded = 0;
				}
			}
		}
		else
		{
			isSprinting = false;
			isDucked = false;
			currentLadder = null;
			isClimbing = false;
		}

		if (delta.x != 0)
		{
			//if (isGrounded)
			{
				if (delta.x > 0)
					direction = 1;
				else if (delta.x < 0)
					direction = -1;

				currentSpeed = isSprinting ? SPRINT_MULTIPLIER * speed : isDucked ? DUCKED_MULTIPLIER * speed : speed;
				velocity.x = delta.x * currentSpeed;
			}

			isMoving = true;
		}
		else
		{
			//if (isGrounded)
			velocity.x = 0.0f;

			isMoving = false;
		}

		if (!isClimbing)
		{
			float gravityMultiplier = 1;
			if (!isAlive || !Input.IsKeyDown(KeyCode.C))
			{
				gravityMultiplier = 1.5f;
				if (Input.IsKeyReleased(KeyCode.C))
					velocity.y = MathF.Min(velocity.y, 0);
			}
			velocity.y += gravityMultiplier * gravity * Time.deltaTime;
			velocity.y = MathF.Max(velocity.y, MAX_FALL_SPEED);

			velocity += impulseVelocity;
			impulseVelocity.x = MathHelper.Lerp(impulseVelocity.x, 0, 10 * Time.deltaTime);

			if (velocity.y < 0 && lastLadderJumpedFrom != null)
				lastLadderJumpedFrom = null;
		}
		else
		{
			velocity.y = delta.y * climbingSpeed;
		}

		Vector2 displacement = velocity * Time.deltaTime;

		if (!isGrounded && displacement.y < 0)
			fallDistance += -displacement.y;
		else
			fallDistance = 0;

		int collisionFlags = GameState.instance.level.doCollision(ref position, collider, ref displacement, Input.IsKeyDown(KeyCode.Down));

		isGrounded = false;
		if ((collisionFlags & Level.COLLISION_Y) != 0)
		{
			if (fallDistance >= FALL_DAMAGE_DISTANCE)
				stun();

			if (velocity.y < 0)
				isGrounded = true;

			velocity.y = 0;
			impulseVelocity.y = 0;
			impulseVelocity.x *= 0.5f;
		}
		if ((collisionFlags & Level.COLLISION_X) != 0)
		{
			impulseVelocity.x = 0;
			impulseVelocity.y *= 0.5f;
		}

		position += displacement;

		//TileType below = TileType.Get(GameState.instance.level.getTile((Vector2i)Vector2.Floor(position - new Vector2(0, 0.1f))));
		//isGrounded = below != null && below.isSolid;

		float rotationDst = direction == 1 ? 0 : MathF.PI;
		rotation = MathHelper.Lerp(rotation, rotationDst, 5 * Time.deltaTime);
	}

	void updateActions()
	{
		if (isAlive)
		{
			if (Input.IsKeyPressed(KeyCode.V))
			{
				bool switched = false;
				for (int i = 0; i < quickItems.Length; i++)
				{
					if (quickItems[(currentQuickItem + 1 + i) % quickItems.Length] != null)
					{
						currentQuickItem = (currentQuickItem + 1 + i) % quickItems.Length;
						switched = true;
						break;
					}
				}
				if (!switched)
					currentQuickItem = (currentQuickItem + 1) % quickItems.Length;
			}
			if (Input.IsKeyPressed(KeyCode.F))
			{
				Item item = quickItems[currentQuickItem];
				if (item != null)
				{
					item.use(this);
					if (item.stackable && item.stackSize > 1)
						item.stackSize--;
					else
						quickItems[currentQuickItem] = null;
				}
			}

			Span<HitData> hits = new HitData[16];
			int numHits = GameState.instance.level.overlap(position + collider.min, position + collider.max, hits, FILTER_MOB);
			for (int i = 0; i < numHits; i++)
			{
				if (hits[i].entity != null && hits[i].entity is Mob)
				{
					Mob mob = hits[i].entity as Mob;
					hit(mob.damage, mob);
				}
			}

			if (!isStunned)
			{
				interactableInFocus = GameState.instance.level.getInteractable(position + new Vector2(0, 0.5f));
				if (interactableInFocus != null && interactableInFocus.canInteract(this))
				{
					if (Input.IsKeyPressed(interactableInFocus.getInput()))
					{
						Input.ConsumeKeyEvent(interactableInFocus.getInput());
						interactableInFocus.interact(this);
					}
				}

				Climbable hoveredLadder = GameState.instance.level.getClimbable(position + new Vector2(0, 0.1f));
				if (currentLadder == null)
				{
					if (hoveredLadder != null && (Input.IsKeyDown(KeyCode.Up) || Input.IsKeyDown(KeyCode.Down)) && lastLadderJumpedFrom == null)
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
					if (Input.IsKeyPressed(KeyCode.X))
					{
						Input.ConsumeKeyEvent(KeyCode.X);
						if (isDucked)
							throwItem(handItem, true);
						else
							handItem.use(this);
					}
				}
				else
				{
					if (Input.IsKeyPressed(KeyCode.X))
					{
						Input.ConsumeKeyEvent(KeyCode.X);
						actions.queueAction(new AttackAction(DefaultWeapon.instance));
					}
				}
			}

			actions.update();
		}
	}

	void updateAnimation()
	{
		if (isAlive)
		{
			if (isStunned)
			{
				animator.setAnimation("stun");
			}
			else
			{
				if (isGrounded)
				{
					if (isMoving)
					{
						animator.setAnimation("run");
					}
					else
					{
						animator.setAnimation("idle");
					}
				}
				else
				{
					if (isClimbing)
					{
						animator.setAnimation("climb");
					}
					else
					{
						if (velocity.y > 0)
						{
							animator.setAnimation("jump");
						}
						else
						{
							animator.setAnimation("fall");
						}
					}
				}
			}
		}
		else
		{
			animator.setAnimation("dead");
		}

		animator.update(sprite);

		for (int i = 0; i < passiveItems.Length; i++)
		{
			if (passiveItems[i] != null && passiveItems[i].ingameSprite != null)
				animator.update(passiveItems[i].ingameSprite);
		}
	}

	public override void update()
	{
		updateMovement();
		updateActions();
		updateAnimation();
	}

	public bool isAlive
	{
		get => health > 0;
	}

	public override void render()
	{
		bool invincible = isAlive && (Time.currentTime - lastHit) / 1e9f < HIT_COOLDOWN;
		bool show = !invincible || ((int)(Time.currentTime / 1e9f * 20) % 2 == 1);

		if (show)
		{
			Renderer.DrawSprite(position.x - 0.5f, position.y, 1, isDucked ? 0.5f : 1, sprite, direction == -1, 0xFFFFFFFF);

			for (int i = 0; i < passiveItems.Length; i++)
			{
				if (passiveItems[i] != null && passiveItems[i].ingameSprite != null)
					Renderer.DrawSprite(position.x - 0.5f, position.y, LAYER_PLAYER_ARMOR, 1, isDucked ? 0.5f : 1, 0, passiveItems[i].ingameSprite, direction == -1, 0xFFFFFFFF);
			}

			if (handItem != null)
			{
				if (handItem.sprite != null)
				{
					if (actions.currentAction is AttackAction)
					{
						AttackAction action = actions.currentAction as AttackAction;
						float xoffset = (action.currentRange - 0.5f) * action.direction;
						Renderer.DrawSprite(position.x - 0.5f + xoffset, position.y - 0.2f, LAYER_PLAYER_ITEM, 1, 1, 0, handItem.sprite, action.direction == -1);
					}
					else
					{
						Renderer.DrawSprite(position.x - 0.5f + itemRenderOffset.x, position.y - 0.5f + itemRenderOffset.y, LAYER_PLAYER_ITEM, 1, 1, 0, handItem.sprite, direction == -1);
					}
				}

				/*
				Renderer.DrawSprite(position.x - 0.25f, position.y + (isDucked ? 0.5f : 1) + 0.5f - 0.25f, 0, 0.5f, 0.5f, null, 0, 0, 0, 0, 0xFF444444);
				*/
			}

			if (actions.currentAction is AttackAction)
			{
				AttackAction action = actions.currentAction as AttackAction;
				if (handItem != null)
				{
					//Renderer.DrawLine(new Vector3(position.x, position.y + 0.5f, 0), new Vector3(position.x + direction * handItem.attackRange, position.y + 0.5f, 0), new Vector4(1));
				}
				else
				{
					Renderer.DrawLine(new Vector3(position.x, position.y + 0.5f, 0), new Vector3(position.x + action.direction * action.currentRange, position.y + 0.5f, 0), new Vector4(1));
				}
			}

			if (isStunned)
			{
				Renderer.DrawSprite(position.x - 0.5f, position.y + 1.0f, 1, 1, stunnedIcon, false);
			}
		}

		if (GameState.instance.run.active)
		{
			hud.render();
			inventoryUI.render();
		}
	}

	public Vector2 itemRenderOffset
	{
		get => new Vector2(direction * 0.2f, 0.5f - 0.2f);
	}
}
