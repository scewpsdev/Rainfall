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


	public float speed = 6;
	public float climbingSpeed = 4;
	public float jumpPower = 10.5f;
	public float gravity = -22;
	public float wallJumpPower = 10;

	public int direction = 1;
	float currentSpeed;
	Vector2 impulseVelocity;
	float wallJumpVelocity;
	float wallJumpFactor;
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
	long lastWallTouchRight = -10000000000;
	long lastWallTouchLeft = -10000000000;
	long lastItemUseDown = -1;

	public int maxHealth = 3;
	public float health = 3;

	public int money = 0;

	long lastHit = -10000000000;
	public long deathTime = -1;
	long stunTime = -1;

	public ActionQueue actions;

	Interactable interactableInFocus = null;
	public Climbable currentLadder = null;
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
		animator.addAnimation("run", 4 * 16, 0, 16, 0, 8, 12, true);
		animator.addAnimation("jump", 12 * 16, 0, 16, 0, 1, 12, true);
		animator.addAnimation("fall", 13 * 16, 0, 16, 0, 1, 12, true);
		animator.addAnimation("climb", 14 * 16, 0, 16, 0, 2, 4, true);
		animator.addAnimation("dead", 16 * 16, 0, 16, 0, 1, 12, true);
		animator.addAnimation("stun", 17 * 16, 0, 16, 0, 1, 1, true);

		animator.addAnimationEvent("run", 3, onStep);
		animator.addAnimationEvent("run", 7, onStep);

		stunnedIcon = new Sprite(Resource.GetTexture("res/sprites/status_stun.png", false));

		hud = new HUD(this);
		inventoryUI = new InventoryUI(this);
	}

	public override void destroy()
	{
	}

	public bool pickupObject(ItemEntity obj)
	{
		if (obj.item.type == ItemType.Tool)
		{
			if (handItem != null)
			{
				handItem.onUnequip(this);
				throwItem(handItem, true);
			}
			handItem = obj.item;
			handItem.onEquip(this);
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
						quickItems[i].onEquip(this);
						return true;
					}
				}
			}
			for (int i = 0; i < quickItems.Length; i++)
			{
				if (quickItems[i] == null)
				{
					quickItems[i] = obj.item;
					quickItems[i].onEquip(this);
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
					passiveItems[i].onEquip(this);
					return true;
				}
			}
			return false;
		}
	}

	public void throwItem(Item item, bool shortThrow = false)
	{
		Vector2 itemVelocity = velocity;
		if (InputManager.IsDown("Up"))
		{
			itemVelocity += new Vector2(direction * 0.05f, 1.0f) * 14;
		}
		else if (InputManager.IsDown("Down"))
		{
			itemVelocity += new Vector2(direction * 0.05f, -1.0f) * 14;
			if (!isGrounded)
				velocity.y = MathF.Max(velocity.y, 0) + 5.0f;
		}
		else
		{
			itemVelocity += new Vector2(direction, 1) * (shortThrow ? new Vector2(0.4f, 0.1f) : new Vector2(1, 0.15f)) * 14;
		}
		Vector2 throwOrigin = position + new Vector2(0, shortThrow ? 0.25f : 0.5f);
		ItemEntity obj = new ItemEntity(item, this, itemVelocity);
		GameState.instance.level.addEntity(obj, throwOrigin);

		if (item == handItem)
			handItem = null;
	}

	public int getTotalArmor()
	{
		int totalArmor = 0;
		for (int i = 0; i < passiveItems.Length; i++)
		{
			if (passiveItems[i] != null)
				totalArmor += passiveItems[i].armor;
		}
		return totalArmor;
	}

	public void hit(float damage, Entity by)
	{
		bool invincible = (Time.currentTime - lastHit) / 1e9f < HIT_COOLDOWN;
		if (!invincible)
		{
			int totalArmor = getTotalArmor();
			float armorAbsorption = totalArmor / (10.0f + totalArmor);
			damage *= 1 - armorAbsorption;

			health -= damage;

			if (by != null)
			{
				Vector2 enemyPosition = by.position;
				if (by.collider != null)
					enemyPosition += 0.5f * (by.collider.max + by.collider.min);
				Vector2 knockback = (position - enemyPosition).normalized * 4.0f;
				addImpulse(knockback);
			}

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
		{
			Item handItemCopy = handItem.createNew();
			throwItem(handItemCopy);
		}

		actions.cancelAllActions();

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
			if (InputManager.IsDown("Left"))
				delta.x--;
			if (InputManager.IsDown("Right"))
				delta.x++;
			if (isClimbing)
			{
				if (InputManager.IsDown("Up"))
				{
					if (GameState.instance.level.getClimbable(position + new Vector2(0, 0.2f)) != null)
						delta.y++;
					else
					{
						TileType tile = TileType.Get(GameState.instance.level.getTile(position));
						TileType up = TileType.Get(GameState.instance.level.getTile(position + new Vector2(0, 0.2f)));
						if (tile != null && tile.isPlatform && up == null)
						{
							lastLadderJumpedFrom = currentLadder;
							currentLadder = null;
							isClimbing = false;
							position.y = MathF.Floor(position.y + 0.2f);
						}
					}
				}
				if (InputManager.IsDown("Down"))
					delta.y--;
			}

			isSprinting = InputManager.IsDown("Sprint");

			isDucked = InputManager.IsDown("Down");
			collider.size.y = isDucked ? 0.4f : 0.8f;

			if (isGrounded)
				lastGrounded = Time.currentTime;

			if (InputManager.IsDown("Right") && GameState.instance.level.overlapTiles(position + new Vector2(0, 0.2f), position + new Vector2(collider.max.x + 0.1f, 0.8f)))
				lastWallTouchRight = Time.currentTime;
			if (InputManager.IsDown("Left") && GameState.instance.level.overlapTiles(position + new Vector2(collider.min.x - 0.1f, 0.2f), position + new Vector2(0.0f, 0.8f)))
				lastWallTouchLeft = Time.currentTime;

			if (InputManager.IsPressed("Jump"))
			{
				if (isClimbing)
				{
					velocity.y = InputManager.IsDown("Down") ? -0.5f * jumpPower : jumpPower;
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
					else if (!isGrounded)
					{
						if ((Time.currentTime - lastWallTouchRight) / 1e9f < COYOTE_TIME)
						{
							velocity.y = jumpPower * 0.7f;
							wallJumpVelocity = -wallJumpPower;
							wallJumpFactor = 1.0f;
							lastWallTouchRight = 0;
						}

						if ((Time.currentTime - lastWallTouchLeft) / 1e9f < COYOTE_TIME)
						{
							velocity.y = jumpPower * 0.7f;
							wallJumpVelocity = wallJumpPower;
							wallJumpFactor = 1.0f;
							lastWallTouchLeft = 0;
						}
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
			if (!isAlive || !InputManager.IsDown("Jump"))
			{
				gravityMultiplier = 1.5f;
				if (InputManager.IsReleased("Jump"))
					velocity.y = MathF.Min(velocity.y, 0);
			}
			velocity.y += gravityMultiplier * gravity * Time.deltaTime;
			velocity.y = MathF.Max(velocity.y, MAX_FALL_SPEED);

			wallJumpFactor = MathHelper.Linear(wallJumpFactor, 0, 2 * Time.deltaTime);
			velocity.x = MathHelper.Lerp(velocity.x, wallJumpVelocity, wallJumpFactor);

			impulseVelocity.x = MathHelper.Lerp(impulseVelocity.x, 0, 3 * Time.deltaTime);
			if (MathF.Sign(impulseVelocity.x) == MathF.Sign(velocity.x))
				impulseVelocity.x = 0;
			else if (velocity.x == 0)
				impulseVelocity.x = MathF.Sign(impulseVelocity.x) * MathF.Min(MathF.Abs(impulseVelocity.x), speed);
			//impulseVelocity.x = impulseVelocity.x - velocity.x;
			velocity += impulseVelocity;

			if (isGrounded && lastLadderJumpedFrom != null)
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

		int collisionFlags = GameState.instance.level.doCollision(ref position, collider, ref displacement, InputManager.IsDown("Down"));

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
			wallJumpFactor = 0;
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
			if (InputManager.IsPressed("SwitchItem"))
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
			if (InputManager.IsPressed("UseItem"))
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
					if (InputManager.IsPressed("Interact"))
					{
						InputManager.ConsumeEvent("Interact");
						interactableInFocus.interact(this);
					}
				}

				Climbable hoveredLadder = GameState.instance.level.getClimbable(position + new Vector2(0, 0.1f));
				if (currentLadder == null)
				{
					if (hoveredLadder != null && (InputManager.IsDown("Up") || InputManager.IsDown("Down")) && lastLadderJumpedFrom == null)
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
					if (InputManager.IsDown("Attack"))
					{
						if (lastItemUseDown == -1)
							lastItemUseDown = Time.currentTime;
					}
					if (InputManager.IsReleased("Attack"))
						lastItemUseDown = -1;

					if (InputManager.IsPressed("Attack"))
					{
						InputManager.ConsumeEvent("Attack");
						handItem.use(this);
					}
					else if (InputManager.IsDown("Attack") && lastItemUseDown != -1 && (Time.currentTime - lastItemUseDown) / 1e9f > handItem.chargeTime)
					{
						handItem.useSecondary(this);
						lastItemUseDown = -1;
					}

					if (InputManager.IsPressed("Interact"))
					{
						InputManager.ConsumeEvent("Interact");
						if (isDucked)
							throwItem(handItem, true);
					}
				}
				else
				{
					if (InputManager.IsPressed("Attack"))
					{
						InputManager.ConsumeEvent("Attack");
						actions.queueAction(new AttackAction(DefaultWeapon.instance));
					}
				}
			}

			handItem?.update(this);
			for (int i = 0; i < quickItems.Length; i++)
			{
				if (quickItems[i] != null)
					quickItems[i].update(this);
			}
			for (int i = 0; i < passiveItems.Length; i++)
			{
				if (passiveItems[i] != null)
					passiveItems[i].update(this);
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

	void onStep()
	{
		GameState.instance.run.stepsTaken++;
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

			if (isAlive && handItem != null)
			{
				if (handItem.sprite != null)
				{
					if (actions.currentAction is AttackAction)
					{
						AttackAction action = actions.currentAction as AttackAction;
						if (handItem.stab)
						{
							float xoffset = action.currentRange * action.direction;
							Renderer.DrawSprite(position.x - ((action.direction + 1) / 2 * handItem.size.x) + xoffset, position.y - 0.2f, LAYER_PLAYER_ITEM, handItem.size.x, handItem.size.y, 0, handItem.sprite, action.direction == -1);
						}
						else
						{
							float rotation = action.currentDirection * action.direction;
							Vector2 offset = new Vector2(MathF.Cos(rotation), MathF.Sin(rotation)) * 0.5f * action.direction;
							Renderer.DrawSprite(position.x - (handItem.size.x - 0.5f) + itemRenderOffset.x + offset.x, position.y - 0.5f + itemRenderOffset.y + offset.y, LAYER_PLAYER_ITEM, handItem.size.x, handItem.size.y, rotation, handItem.sprite, action.direction == -1);
						}
					}
					else
					{
						Renderer.DrawSprite(position.x - 0.5f * handItem.size.x + itemRenderOffset.x, position.y - 0.5f + itemRenderOffset.y, LAYER_PLAYER_ITEM, handItem.size.x, handItem.size.y, 0, handItem.sprite, direction == -1);
					}
				}

				/*
				Renderer.DrawSprite(position.x - 0.25f, position.y + (isDucked ? 0.5f : 1) + 0.5f - 0.25f, 0, 0.5f, 0.5f, null, 0, 0, 0, 0, 0xFF444444);
				*/
			}
			else
			{
				if (actions.currentAction is AttackAction)
				{
					AttackAction action = actions.currentAction as AttackAction;
					float xoffset = (action.currentRange - 0.5f) * action.direction;
					Renderer.DrawSprite(position.x - 0.5f + xoffset, position.y - 0.2f, LAYER_PLAYER_ITEM, 1, 1, 0, DefaultWeapon.instance.sprite, action.direction == -1);
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
