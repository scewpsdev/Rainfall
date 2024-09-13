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
	const float DUCKED_MULTIPLIER = 0.6f;
	const float MAX_FALL_SPEED = -15;
	const float HIT_COOLDOWN = 1.0f;
	const float STUN_DURATION = 1.0f;
	const float FALL_DAMAGE_DISTANCE = 8;
	const float MANA_RECHARGE_RATE = 0.02f;
	const float MANA_KILL_REWARD = 0.4f;


	public float speed = 6;
	public float climbingSpeed = 4;
	public float jumpPower = 10.5f;
	public float gravity = -22;
	public float wallJumpPower = 10;
	public float attack = 1.0f;

	public float maxHealth = 3;
	public float health = 3;

	public float maxMana = 2;
	public float mana = 2;

	public int money = 0;

	public int direction = 1;
	public Vector2i aimPosition;
	public Vector2 lookDirection = Vector2.Zero;
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

	public Sprite sprite;
	public SpriteAnimator animator;

	long lastJumpInput = -10000000000;
	long lastGrounded = -10000000000;
	long lastWallTouchRight = -10000000000;
	long lastWallTouchLeft = -10000000000;
	long lastItemUseDown = -1;

	List<StatusEffect> statusEffects = new List<StatusEffect>();

	long lastHit = -10000000000;
	long stunTime = -1;

	public ActionQueue actions;

	Interactable interactableInFocus = null;
	public Climbable currentLadder = null;
	Climbable lastLadderJumpedFrom = null;

	public TupleList<ItemType, Item> items = new TupleList<ItemType, Item>();
	public Item handItem = null;
	public Item offhandItem = null;
	public Item[] activeItems = new Item[4];
	public int selectedActiveItem = 0;
	public Item[] passiveItems = new Item[(int)ArmorSlot.Count];

	public Item blockingItem = null;

	public HUD hud;
	InventoryUI inventoryUI;
	public int numOverlaysOpen = 0;
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
		animator.addAnimation("dead_falling", 17 * 16, 0, 16, 0, 1, 12, true);
		animator.addAnimation("stun", 18 * 16, 0, 16, 0, 1, 1, true);

		animator.addAnimationEvent("run", 3, onStep);
		animator.addAnimationEvent("run", 7, onStep);

		stunnedIcon = new Sprite(Resource.GetTexture("res/sprites/status_stun.png", false));

		hud = new HUD(this);
		inventoryUI = new InventoryUI(this);

#if DEBUG
		giveItem(new Backpack());
		giveItem(new StaffOfIllumination());
#endif
	}

	public override void destroy()
	{
	}

	public bool equipHandItem(Item item)
	{
		if (handItem != null)
		{
			handItem.onUnequip(this);
			handItem = null;
		}
		handItem = item;
		handItem.onEquip(this);

		if (item.twoHanded && offhandItem != null)
			unequipItem(offhandItem);

		return true;
	}

	public bool equipOffhandItem(Item item)
	{
		if (offhandItem != null)
		{
			offhandItem.onUnequip(this);
			offhandItem = null;
		}
		offhandItem = item;
		offhandItem.onEquip(this);

		if (handItem != null && handItem.twoHanded)
			unequipItem(handItem);

		return true;
	}

	public bool equipItem(Item item)
	{
		if (item.isHandItem)
			return equipHandItem(item);
		if (item.isActiveItem)
		{
			for (int i = 0; i < activeItems.Length; i++)
			{
				if (activeItems[i] == null)
				{
					activeItems[i] = item;
					activeItems[i].onEquip(this);
					return true;
				}
			}

			unequipItem(activeItems[activeItems.Length - 1]);
			activeItems[activeItems.Length - 1] = item;
			activeItems[activeItems.Length - 1].onEquip(this);
			return true;
		}
		if (item.isPassiveItem)
		{
			if (item.type == ItemType.Armor)
			{
				int slotIdx = (int)item.armorSlot;
				if (passiveItems[slotIdx] != null)
					unequipItem(passiveItems[slotIdx]);

				passiveItems[slotIdx] = item;
				passiveItems[slotIdx].onEquip(this);
				return true;
			}
			else if (item.type == ItemType.Ring)
			{
				for (int i = (int)ArmorSlot.Ring1; i <= (int)ArmorSlot.Ring2; i++)
				{
					if (passiveItems[i] == null)
					{
						passiveItems[i] = item;
						passiveItems[i].onEquip(this);
						return true;
					}
				}

				unequipItem(passiveItems[(int)ArmorSlot.Ring2]);
				passiveItems[(int)ArmorSlot.Ring2] = item;
				passiveItems[(int)ArmorSlot.Ring2].onEquip(this);
				return true;
			}
			return false;
		}

		Debug.Assert(false);
		return false;
	}

	public bool unequipItem(Item item)
	{
		if (handItem == item)
		{
			handItem.onUnequip(this);
			handItem = null;
			return true;
		}
		if (offhandItem == item)
		{
			offhandItem.onUnequip(this);
			offhandItem = null;
			return true;
		}
		for (int i = 0; i < activeItems.Length; i++)
		{
			if (activeItems[i] == item)
			{
				activeItems[i].onUnequip(this);
				activeItems[i] = null;
				if (selectedActiveItem == i)
				{
					for (int j = 0; j < activeItems.Length; j++)
					{
						if (activeItems[(selectedActiveItem + 1 + j) % activeItems.Length] != null)
						{
							selectedActiveItem = (selectedActiveItem + 1 + j) % activeItems.Length;
							hud.onItemSwitch();
							break;
						}
					}
				}
				return true;
			}
		}
		for (int i = 0; i < passiveItems.Length; i++)
		{
			if (passiveItems[i] == item)
			{
				passiveItems[i].onUnequip(this);
				passiveItems[i] = null;
				return true;
			}
		}
		return false;
	}

	public void giveItem(Item item)
	{
		if (item.stackable)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i].Item2.id == item.id)
				{
					items[i].Item2.stackSize += item.stackSize;
					return;
				}
			}
		}

		items.Add(item.type, item);
		items.Sort();

		if (item.isHandItem && handItem == null && (offhandItem == null || !item.twoHanded))
			equipHandItem(item);
		else if (item.isSecondaryItem && offhandItem == null && (handItem == null || !handItem.twoHanded))
			equipOffhandItem(item);
		else if (item.isActiveItem && numActiveItems < activeItems.Length)
			equipItem(item);
		else if (item.isPassiveItem && canEquipPassiveItem(item))
			equipItem(item);
	}

	public void removeItem(Item item)
	{
		unequipItem(item);
		items.Remove(item.type, item);
	}

	public Item removeItemSingle(Item item)
	{
		if (item.stackable && item.stackSize > 1)
		{
			Item copy = item.copy();
			copy.stackSize = 1;
			item.stackSize--;
			return copy;
		}
		else
		{
			removeItem(item);
			return item;
		}
	}

	public Item getItem(string name)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].Item2.name == name)
				return items[i].Item2;
		}
		return null;
	}

	public bool isEquipped(Item item)
	{
		if (handItem == item)
			return true;
		if (offhandItem == item)
			return true;
		if (isActiveItem(item, out _))
			return true;
		if (isPassiveItem(item, out _))
			return true;
		return false;
	}

	public bool isActiveItem(Item item, out int slot)
	{
		for (int i = 0; i < activeItems.Length; i++)
		{
			if (activeItems[i] == item)
			{
				slot = i;
				return true;
			}
		}
		slot = -1;
		return false;
	}

	public bool isPassiveItem(Item item, out int slot)
	{
		for (int i = 0; i < passiveItems.Length; i++)
		{
			if (passiveItems[i] == item)
			{
				slot = i;
				return true;
			}
		}
		slot = -1;
		return false;
	}

	public int numActiveItems
	{
		get
		{
			int result = 0;
			for (int i = 0; i < activeItems.Length; i++)
			{
				if (activeItems[i] != null)
					result++;
			}
			return result;
		}
	}

	public int numPassiveItems
	{
		get
		{
			int result = 0;
			for (int i = 0; i < passiveItems.Length; i++)
			{
				if (passiveItems[i] != null)
					result++;
			}
			return result;
		}
	}

	public int numTotalEquippedItems
	{
		get => (handItem != null ? 1 : 0) + (offhandItem != null ? 1 : 0) + numActiveItems + numPassiveItems;
	}

	public bool canEquipPassiveItem(Item item)
	{
		if (item.type == ItemType.Armor)
		{
			int slotIdx = (int)item.armorSlot;
			return passiveItems[slotIdx] == null;
		}
		else if (item.type == ItemType.Ring)
		{
			for (int i = (int)ArmorSlot.Ring1; i <= (int)ArmorSlot.Ring2; i++)
			{
				if (passiveItems[i] == null)
					return true;
			}
			return false;
		}
		else
		{
			Debug.Assert(false);
			return false;
		}
	}

	public void throwItem(Item item, bool shortThrow = false, bool farThrow = false)
	{
		Vector2 itemVelocity = velocity;
		if (InputManager.IsDown("Up"))
		{
			itemVelocity += new Vector2(direction * 0.05f, MathHelper.RandomFloat(0.9f, 1.1f)).normalized * 14;
		}
		else if (InputManager.IsDown("Down"))
		{
			itemVelocity += new Vector2(direction * 0.05f, MathHelper.RandomFloat(-1.1f, -0.9f)) * 14;
			if (!isGrounded)
				velocity.y = MathF.Max(velocity.y, 0) + 5.0f;
		}
		else
		{
			itemVelocity += new Vector2(direction, 1) * (shortThrow ? new Vector2(0.4f, MathHelper.RandomFloat(0.14f, 0.16f)) : farThrow ? new Vector2(2, MathHelper.RandomFloat(0.14f, 0.16f)) : new Vector2(1, MathHelper.RandomFloat(0.14f, 0.16f))) * 14;
		}
		Vector2 throwOrigin = position + new Vector2(0, shortThrow ? 0.25f : 0.25f);
		ItemEntity obj = new ItemEntity(item, this, itemVelocity);
		GameState.instance.level.addEntity(obj, throwOrigin);
	}

	public void throwItem(Item item, Vector2 direction, float speed = 14)
	{
		Vector2 itemVelocity = velocity + direction * speed;
		if (!isGrounded && Vector2.Dot(direction, Vector2.UnitY) < -0.8f)
			velocity.y = MathF.Max(velocity.y, 0) + 5.0f;
		Vector2 throwOrigin = position + collider.center;
		ItemEntity obj = new ItemEntity(item, this, itemVelocity);
		GameState.instance.level.addEntity(obj, throwOrigin);
	}

	public void addImpulse(Vector2 impulse)
	{
		impulseVelocity.x += impulse.x;
		velocity.y += impulse.y;
	}

	public int getTotalArmor()
	{
		int totalArmor = (handItem != null ? handItem.armor : 0) + (offhandItem != null ? offhandItem.armor : 0);
		for (int i = 0; i < activeItems.Length; i++)
		{
			if (activeItems[i] != null)
				totalArmor += activeItems[i].armor;
		}
		for (int i = 0; i < passiveItems.Length; i++)
		{
			if (passiveItems[i] != null)
				totalArmor += passiveItems[i].armor;
		}
		return totalArmor;
	}

	public void hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true)
	{
		if (!isAlive)
			return;

		bool invincible = (Time.currentTime - lastHit) / 1e9f < HIT_COOLDOWN;
		if (invincible)
		{
		}
		else if (blockingItem != null && ((BlockAction)actions.currentAction).isBlocking)
		{
			// play sound
			// play particle effect
			if (by is Mob)
			{
				Mob mob = by as Mob;
				mob.stun();
				mob.addImpulse(new Vector2(direction, 0.1f) * 8);
				if (blockingItem.damageReflect > 0)
					mob.hit(blockingItem.damageReflect, this, blockingItem, null, false);

				GameState.instance.level.addEntity(new ParryEffect(this), position + new Vector2(0.25f * direction, getWeaponOrigin(((BlockAction)actions.currentAction).mainHand).y));
			}
		}
		else
		{
			int totalArmor = getTotalArmor();
			float armorAbsorption = Item.GetArmorAbsorption(totalArmor);
			damage *= 1 - armorAbsorption;

			health = MathF.Max(health - damage, 0);

			GameState.instance.run.hitsTaken++;

			if (by != null && by.collider != null)
			{
				Vector2 enemyPosition = by.position + by.collider.center;
				float knockbackStrength = item != null ? item.knockback : 8.0f;
				Vector2 knockback = (position - enemyPosition).normalized * knockbackStrength;
				addImpulse(knockback);
			}

			if (health <= 0)
			{
				onDeath(by, byName);
			}

			if (triggerInvincibility)
				lastHit = Time.currentTime;
		}
	}

	void stun()
	{
		isStunned = true;
		stunTime = Time.currentTime;
	}

	void onDeath(Entity by, string byName)
	{
		if (handItem != null)
		{
			Item handItemCopy = handItem.copy();
			Vector2 direction = new Vector2(0, 1) + MathHelper.RandomVector2(-0.5f, 0.5f);
			throwItem(handItemCopy, direction);
		}
		if (offhandItem != null)
		{
			Item offhandItemCopy = offhandItem.copy();
			Vector2 direction = new Vector2(0, 1) + MathHelper.RandomVector2(-0.5f, 0.5f);
			throwItem(offhandItemCopy, direction);
		}

		for (int i = 0; i < passiveItems.Length; i++)
		{
			if (passiveItems[i] != null && passiveItems[i].ingameSprite == null)
			{
				passiveItems[i].onUnequip(this);
				passiveItems[i] = null;
			}
		}

		if (interactableInFocus != null)
		{
			interactableInFocus.onFocusLeft(this);
			interactableInFocus = null;
		}

		for (int i = 0; i < statusEffects.Count; i++)
			statusEffects[i].destroy(this);
		statusEffects.Clear();

		actions.cancelAllActions();

		GameState.instance.run.active = false;
		GameState.instance.run.killedBy = by;
		GameState.instance.run.killedByName = by != null && by.displayName != null ? by.displayName : byName;
		GameState.instance.run.endedTime = Time.currentTime;

		Input.cursorMode = CursorMode.Normal;
	}

	public void consumeMana(float amount)
	{
		mana = MathF.Max(mana - amount, 0);
	}

	public void addStatusEffect(StatusEffect effect)
	{
		statusEffects.Add(effect);
		effect.init(this);
	}

	public void onKill(Mob mob)
	{
		GameState.instance.run.kills++;
		if (mana < maxMana)
			mana = MathF.Min(mana + MANA_KILL_REWARD, maxMana);
	}

	void updateMovement()
	{
		Vector2 delta = Input.GamepadAxis;
		if (delta.lengthSquared < 0.25f)
			delta = Vector2.Zero;

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
						TileType tile = GameState.instance.level.getTile(position);
						TileType up = GameState.instance.level.getTile(position + new Vector2(0, 0.2f));
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
				{
					delta.y--;
				}
			}
			else
			{
				if (InputManager.IsDown("Down"))
				{
					TileType tile = GameState.instance.level.getTile(position);
					if (tile != null && tile.isPlatform && MathHelper.Fract(position.y) > 0.75f)
						position.y = MathF.Floor(position.y) + 0.74f;
				}
			}

#if DEBUG
			isSprinting = Input.IsKeyDown(KeyCode.Ctrl);
#endif

			isDucked = InputManager.IsDown("Down") && numOverlaysOpen == 0;
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
							velocity.y = jumpPower * 0.75f;
							wallJumpVelocity = -wallJumpPower;
							wallJumpFactor = 1.0f;
							lastWallTouchRight = 0;
						}

						if ((Time.currentTime - lastWallTouchLeft) / 1e9f < COYOTE_TIME)
						{
							velocity.y = jumpPower * 0.75f;
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
			//if (delta.x > 0)
			//	direction = 1;
			//else if (delta.x < 0)
			//	direction = -1;

			currentSpeed = isSprinting ? SPRINT_MULTIPLIER * speed : isDucked ? DUCKED_MULTIPLIER * speed : speed;
			if (actions.currentAction != null)
				currentSpeed *= actions.currentAction.speedMultiplier;
			velocity.x = delta.x * currentSpeed;

			isMoving = true;
		}
		else
		{
			velocity.x = 0.0f;

			isMoving = false;
		}

		/*
		if (Renderer.cursorMove != Vector2i.Zero && numOverlaysOpen == 0)
		{
			aimPosition += Renderer.cursorMove * (Vector2.Dot((Vector2)Renderer.cursorMove, (Vector2)aimPosition) < 0 ? 2 : 1);
			if (aimPosition == Vector2i.Zero)
				aimPosition += Renderer.cursorMove;
			aimPosition.x = MathHelper.Clamp(aimPosition.x, -20, 20);
			aimPosition.y = MathHelper.Clamp(aimPosition.y, -20, 20);
			if (aimPosition.length > 1)
				aimPosition = (Vector2i)Vector2.Round(aimPosition.normalized);

			float currentAngle = lookDirection.angle;
			float dstAngle = (aimPosition * new Vector2i(1, -1)).normalized.angle;
			float angle = MathHelper.LerpAngle(currentAngle, dstAngle, 50 * Time.deltaTime);
			lookDirection = Vector2.Rotate(Vector2.UnitX, angle);
			direction = Math.Sign(lookDirection.x);
		}
		*/

		if (numOverlaysOpen == 0)
		{
			Vector2 controllerAim = Input.GamepadAxisRight;
			if (controllerAim.lengthSquared > 0.25f)
				lookDirection = controllerAim;

			if (Input.cursorHasMoved)
			{
				if (GameSettings.aimMode == AimMode.Directional)
				{
					float maxCursorDistance = handItem != null ? handItem.attackRange * 2 : 1.8f;
					Vector2i playerScreenPos = Display.viewportSize / 2; // new Vector2i(Renderer.UIWidth, Renderer.UIHeight) / 2; // GameState.instance.camera.worldToScreen(position + collider.center);
					if (MathF.Abs(Input.cursorPosition.x - playerScreenPos.x) > maxCursorDistance * 16 * GameState.instance.camera.scale ||
						MathF.Abs(Input.cursorPosition.y - playerScreenPos.y) > maxCursorDistance * 16 * GameState.instance.camera.scale)
					{
						int x = Math.Clamp(Input.cursorPosition.x, (int)MathF.Round(playerScreenPos.x - maxCursorDistance * 16 * GameState.instance.camera.scale), (int)MathF.Round(playerScreenPos.x + maxCursorDistance * 16 * GameState.instance.camera.scale));
						int y = Math.Clamp(Input.cursorPosition.y, (int)MathF.Round(playerScreenPos.y - maxCursorDistance * 16 * GameState.instance.camera.scale), (int)MathF.Round(playerScreenPos.y + maxCursorDistance * 16 * GameState.instance.camera.scale));
						Vector2i newCursorPos = new Vector2i(x, y);
						Input.cursorPosition = newCursorPos; // * Display.viewportSize / new Vector2i(Renderer.UIWidth, Renderer.UIHeight);
					}
					/*
					if ((Renderer.cursorPosition - playerScreenPos).length > maxCursorDistance * 16)
					{
						Vector2i newCursorPos = playerScreenPos + (Vector2i)Vector2.Round((Renderer.cursorPosition - playerScreenPos).normalized * maxCursorDistance * 16);
						Input.cursorPosition = newCursorPos * Display.viewportSize / new Vector2i(Renderer.UIWidth, Renderer.UIHeight);
					}
					*/
					lookDirection = GameState.instance.camera.screenToWorld(Input.cursorPosition) - GameState.instance.camera.screenToWorld(Display.viewportSize / 2); // (position + collider.center);
					if (MathF.Abs(lookDirection.x) > maxCursorDistance)
						lookDirection.x = MathF.Sign(lookDirection.x) * maxCursorDistance;
					if (MathF.Abs(lookDirection.y) > maxCursorDistance)
						lookDirection.y = MathF.Sign(lookDirection.y) * maxCursorDistance;
				}
				else
				{
					lookDirection = GameState.instance.camera.screenToWorld(Input.cursorPosition) - (position + collider.center);
				}
			}

			direction = Math.Sign(lookDirection.x);
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

			impulseVelocity.x = MathHelper.Lerp(impulseVelocity.x, 0, 8 * Time.deltaTime);
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

		int collisionFlags = GameState.instance.level.doCollision(ref position, collider, ref displacement, isDucked);

		isGrounded = false;
		if ((collisionFlags & Level.COLLISION_Y) != 0)
		{
			if (fallDistance >= FALL_DAMAGE_DISTANCE)
				stun();
			if (velocity.y < -10)
				onLand();

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
			wallJumpFactor = 0;
		}

		position += displacement;

		//TileType below = TileType.Get(GameState.instance.level.getTile((Vector2i)Vector2.Floor(position - new Vector2(0, 0.1f))));
		//isGrounded = below != null && below.isSolid;

		float rotationDst = direction == 1 ? 0 : MathF.PI;
		rotation = MathHelper.Lerp(rotation, rotationDst, 5 * Time.deltaTime);
	}

	public bool useActiveItem(Item item)
	{
		if (item.stackable && item.stackSize > 1)
		{
			Item copy = item.copy();
			copy.stackSize = 1;
			if (copy.use(this))
			{
				item.stackSize--;
				return true;
			}
		}
		else
		{
			if (item.use(this))
			{
				removeItem(item);
				return true;
			}
		}

		return false;
	}

	void updateActions()
	{
		if (isAlive && GameState.instance.run.active)
		{
			if (InputManager.IsPressed("SwitchItem"))
			{
				bool switched = false;
				for (int i = 0; i < activeItems.Length; i++)
				{
					if (activeItems[(selectedActiveItem + 1 + i) % activeItems.Length] != null)
					{
						selectedActiveItem = (selectedActiveItem + 1 + i) % activeItems.Length;
						switched = true;
						hud.onItemSwitch();
						break;
					}
				}
				if (!switched)
					selectedActiveItem = (selectedActiveItem + 1) % activeItems.Length;
			}
			for (int i = 0; i < Math.Min(activeItems.Length, 10); i++)
			{
				if (Input.IsKeyPressed(KeyCode.Key1 + i))
				{
					selectedActiveItem = i;
					hud.onItemSwitch();
				}
			}
			if (InputManager.IsPressed("UseItem"))
			{
				if (activeItems[selectedActiveItem] != null)
					useActiveItem(activeItems[selectedActiveItem]);
			}

			Span<HitData> hits = new HitData[16];
			int numHits = GameState.instance.level.overlap(position + collider.min, position + collider.max, hits, FILTER_MOB);
			for (int i = 0; i < numHits; i++)
			{
				if (hits[i].entity != null && hits[i].entity is Mob)
				{
					Mob mob = hits[i].entity as Mob;
					hit(mob.damage, mob, mob.handItem);
				}
			}

			if (!isStunned && numOverlaysOpen == 0)
			{
				Interactable interactable = isAlive ? GameState.instance.level.getInteractable(position + new Vector2(0, 0.5f), this) : null;
				if (interactableInFocus != null && interactableInFocus != interactable)
					interactableInFocus.onFocusLeft(this);
				if (interactable != null && interactable != interactableInFocus)
					interactable.onFocusEnter(this);
				interactableInFocus = interactable;

				if (interactableInFocus != null)
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

					if (InputManager.IsDown("Attack"))
					{
						if (handItem.trigger)
						{
							if (InputManager.IsPressed("Attack"))
							{
								InputManager.ConsumeEvent("Attack");
								if (handItem.use(this))
									removeItemSingle(handItem);
							}
							else if (lastItemUseDown != -1 && (Time.currentTime - lastItemUseDown) / 1e9f > handItem.secondaryChargeTime && (Time.currentTime - lastItemUseDown) / 1e9f < handItem.secondaryChargeTime + 1)
							{
								if (handItem.useSecondary(this))
									removeItem(handItem);
								lastItemUseDown = -1;
							}
						}
						else
						{
							if (actions.currentAction == null)
							{
								if (handItem.use(this))
									removeItem(handItem);
							}
						}
					}

					if (InputManager.IsPressed("Interact"))
					{
						if (isDucked)
						{
							InputManager.ConsumeEvent("Interact");
							throwItem(handItem, true);
							removeItem(handItem);
						}
					}
				}
				else
				{
					if (InputManager.IsPressed("Attack"))
					{
						InputManager.ConsumeEvent("Attack");
						DefaultWeapon.instance.use(this);
					}
				}

				if (offhandItem != null)
				{
					if (InputManager.IsDown("Attack2"))
					{
						if (offhandItem.trigger)
						{
							if (InputManager.IsPressed("Attack2", true))
							{
								if (offhandItem.use(this))
									removeItem(offhandItem);
							}
						}
						else
						{
							if (actions.currentAction == null)
							{
								if (offhandItem.use(this))
									removeItem(offhandItem);
							}
						}
					}
				}
			}

			handItem?.update(this);
			for (int i = 0; i < activeItems.Length; i++)
			{
				if (activeItems[i] != null)
					activeItems[i].update(this);
			}
			for (int i = 0; i < passiveItems.Length; i++)
			{
				if (passiveItems[i] != null)
					passiveItems[i].update(this);
			}

			actions.update();
		}
	}

	void updateStatus()
	{
		if (mana < maxMana)
			mana = MathF.Min(mana + MANA_RECHARGE_RATE * Time.deltaTime, maxMana);

		for (int i = 0; i < statusEffects.Count; i++)
		{
			if (!statusEffects[i].update(this) && isAlive)
			{
				statusEffects[i].destroy(this);
				statusEffects.RemoveAt(i--);
			}
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
			if (isGrounded)
			{
				animator.setAnimation("dead");
			}
			else
			{
				animator.setAnimation("dead_falling");
			}
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
		GameState.instance.run.stepsWalked++;

		TileType tile = GameState.instance.level.getTile(position - new Vector2(0, 0.5f));
		if (tile != null)
			GameState.instance.level.addEntity(Effects.CreateStepEffect(MathHelper.RandomInt(2, 4), MathHelper.ARGBToVector(tile.particleColor).xyz), position);
	}

	void onLand()
	{
		TileType tile = GameState.instance.level.getTile(position - new Vector2(0, 0.5f));
		if (tile != null)
			GameState.instance.level.addEntity(Effects.CreateStepEffect(MathHelper.RandomInt(4, 8), MathHelper.ARGBToVector(tile.particleColor).xyz), position);
	}

	public override void update()
	{
		updateMovement();
		updateActions();
		updateStatus();
		updateAnimation();

		Input.cursorMode = CursorMode.Hidden;
	}

	public bool isAlive
	{
		get => health > 0;
	}

	public Vector2 getWeaponOrigin(bool mainHand)
	{
		int frame = (animator.lastFrameIdx + animator.getAnimation(animator.currentAnimation).length - 1) % animator.getAnimation(animator.currentAnimation).length; // sway
		Vector2i animOffset = Vector2i.Zero;
		if (animator.currentAnimation == "idle")
			animOffset.y = -frame / 2;
		else if (animator.currentAnimation == "run")
			animOffset.y = frame % 4 - frame % 4 / 3 * 2;
		else if (animator.currentAnimation == "jump")
			animOffset = new Vector2i(1, 2);
		else if (animator.currentAnimation == "fall")
			animOffset.y = 2;
		else if (animator.currentAnimation == "stun")
			animOffset.y = -2;
		return new Vector2((!mainHand ? 4 / 16.0f : -3 / 16.0f) + animOffset.x / 16.0f, (!mainHand ? 5 / 16.0f : 4 / 16.0f) + animOffset.y / 16.0f);
	}

	void renderHandItem(float layer, bool mainHand, Item item)
	{
		if (!isAlive)
			return;

		uint color = mainHand ? 0xFFFFFFFF : 0xFF7F7F7F;

		if (item == null)
			item = DefaultWeapon.instance;

		if (item.sprite != null)
		{
			if (actions.currentAction != null && actions.currentAction.mainHand == mainHand)
			{
				Matrix weaponTransform = Matrix.CreateTranslation(position.x, position.y, layer)
					* actions.currentAction.getItemTransform(this);
				Renderer.DrawSprite(item.size.x, item.size.y, weaponTransform, item.sprite, color);
			}
			else
			{
				if (item != DefaultWeapon.instance)
					Renderer.DrawSprite(position.x - 0.5f * item.size.x + (item.renderOffset.x + getWeaponOrigin(mainHand).x) * direction, position.y - 0.5f + getWeaponOrigin(mainHand).y, layer, item.size.x, item.size.y, 0, item.sprite, direction == -1, color);
			}
		}
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
				if (passiveItems[i] != null)
				{
					if (passiveItems[i].ingameSprite != null)
						Renderer.DrawSprite(position.x - 0.5f, position.y, LAYER_PLAYER_ARMOR, 1, isDucked ? 0.5f : 1, 0, passiveItems[i].ingameSprite, direction == -1, passiveItems[i].ingameSpriteColor);
					passiveItems[i].render(this);
				}
			}

			renderHandItem(LAYER_PLAYER_ITEM_MAIN, true, handItem);
			renderHandItem(LAYER_PLAYER_ITEM_SECONDARY, false, offhandItem);
		}

		for (int i = 0; i < statusEffects.Count; i++)
		{
			statusEffects[i].render(this);
		}

		if (isStunned)
		{
			Renderer.DrawSprite(position.x - 0.5f, position.y + 1.0f, 1, 1, stunnedIcon, false);
		}

		Renderer.DrawLight(position + new Vector2(0, 0.5f), new Vector3(1.0f) * 1.5f, 7);

		if (GameState.instance.run.active)
		{
			hud.render();
			inventoryUI.render();
		}
	}
}
