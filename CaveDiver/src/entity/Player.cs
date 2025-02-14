using Rainfall;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public class Player : Entity, Hittable, StatusEffectReceiver
{
	const float JUMP_BUFFER = 0.2f;
	const float COYOTE_TIME = 0.15f;
	const float SPRINT_MULTIPLIER = 1; //1.5f;
	const float DUCKED_MULTIPLIER = 0.6f;
	const float MAX_FALL_SPEED = -18;
	const float HIT_COOLDOWN = 1.0f;
	const float STUN_DURATION = 1.0f;
	const float FALL_STUN_DISTANCE = 8;
	const float FALL_DAMAGE_DISTANCE = 10;
	const float MANA_KILL_REWARD = 0.5f;
#if DEBUG
	const float SPRINT_MANA_COST = 0.5f;
#else
	const float SPRINT_MANA_COST = 0.5f;
#endif


	public const float defaultSpeed = 8;
	public float speed = defaultSpeed;
	public float climbingSpeed = 5;
	public float jumpPower = 11; //12; //10.5f;
	public float gravity = -22;
	public bool canWallJump = true;
	public float wallJumpPower = 10;
	public float wallControl = 2;
	public int airJumps = 0;
	public int airJumpsLeft = 0;
	public const float defaultManaRecoveryRate = 0.04f;
	public float coinCollectDistance = 1.0f;
	public float aimDistance = 1.0f;
	public float criticalChance = 0.05f;

	public float maxHealth = 5;
	public float health = 5;
	//public float maxHealth => hp * 0.5f;

	public float maxMana = 2;
	public float mana = 2;
	//public float maxMana => magic * 0.5f;

	//public int hp = 6;
	//public int magic = 4;
	public int strength = 1;
	public int dexterity = 1;
	public int intelligence = 1;
	public int swiftness = 1;

	public int money = 0;
	public int playerLevel = 1;
	public int xp = 0;

	public int nextLevelXP => (int)MathF.Round(50 * (1 + 0.4f * (playerLevel - 1)));
	public int availableStatUpgrades = 0;

	public List<ItemBuff> itemBuffs = new List<ItemBuff>();

	public int direction = 1;
	public Vector2 lookDirection = Vector2.Right;
	public float impulseVelocity;
	float wallJumpVelocity;
	float wallJumpFactor;
	public bool isGrounded = false;
	bool isMoving = false;
	bool isSprinting = false;
	public bool isDucked = false;
	public bool isClimbing = false;
	public bool isLookingUp = false;
	float fallDistance = 0;

	ParticleEffect wallSlideParticles;

	// Status effects
	public bool isStunned = false;
	public bool isVisible = true;

	public float visibility { get => (isVisible ? 1 : 0.25f) * MathHelper.Lerp(0.5f, 1.0f, level.lightLevel) * (isDucked ? 0.5f : 1.0f); }

	Sprite stunnedIcon;
	MobCorpse corpse;

	public Sprite sprite;
	public FloatRect rect;
	public SpriteAnimator animator;

	public InputBinding currentAttackInput = null;

	long lastJumpInput = -10000000000;
	long lastGrounded = -10000000000;
	long lastWallTouchRight = -10000000000;
	long lastWallTouchLeft = -10000000000;

	public List<StatusEffect> statusEffects = new List<StatusEffect>();

	long startTime;
	long lastIFrameTrigger = -1;
	float iframeDuration;
	long lastHit = -1;
	long stunTime = -1;

	public ActionQueue actions;

	public Interactable interactableInFocus = null;
	public Climbable currentLadder = null;
	Climbable lastLadderJumpedFrom = null;

	public StartingClass startingClass;

	public List<Item> items = new List<Item>();
	public Item handItem = null;
	public Item offhandItem = null;
	public Item[] activeItems = new Item[4];
	public int selectedActiveItem = 0;
	public List<Spell> spellItems = new List<Spell>();
	public int spellCapacity = 4;
	public int selectedSpellItem = 0;
	public Spell getSelectedSpell() => spellItems.Count > 0 ? spellItems[selectedSpellItem = MathHelper.Clamp(selectedSpellItem, 0, spellItems.Count - 1)] : null;
	public List<Item> passiveItems = new List<Item>();
	public List<Item> storedItems = new List<Item>();
	public int storeCapacity = 4;

	ParticleEffect handParticles, offhandParticles;

	public Item blockingItem = null;
	public bool unlimitedArrows = false;
	public bool canEquipOffhand = false;
	public bool canEquipOnehanded = false;

	public HUD hud;
	InventoryUI inventoryUI;
	public int numOverlaysOpen = 0;
	public bool inventoryOpen = false;

	public Object carriedObject = null;

	Sound[] stepSound;
	Sound jumpSound;
	Sound landSound;
	Sound[] ladderSound;
	Sound[] hitSound;
	Sound wallTouchSound;


	public Player()
	{
		actions = new ActionQueue(this);

		collider = new FloatRect(-0.15f, 0, 0.3f, 0.9f);
		filterGroup = FILTER_PLAYER;

		sprite = new Sprite(Resource.GetTexture("sprites/player_.png", false), 0, 0, 32, 32);
		rect = new FloatRect(-1, 0, 2, 2);
		animator = new SpriteAnimator();

		animator.addAnimation("idle", 4, 1, true);
		animator.addAnimation("look_up", 4, 1, true);
		animator.addAnimation("run", 8, 0.666f, true);
		animator.addAnimation("jump", 2, 2, true);
		animator.addAnimation("fall", 3, 0.333f, true);
		animator.addAnimation("climb", 2, 0.5f, true);
		animator.addAnimation("dead", 1, 1, true);
		animator.addAnimation("dead_falling", 1, 1, true);
		animator.addAnimation("stun", 1, 1, true);

		animator.addAnimationEvent("run", 3, onStep);
		animator.addAnimationEvent("run", 7, onStep);
		animator.addAnimationEvent("climb", 0, onClimbStep);
		animator.addAnimationEvent("climb", 1, onClimbStep);

		stunnedIcon = new Sprite(Resource.GetTexture("sprites/status_stun.png", false));

		hud = new HUD(this);
		inventoryUI = new InventoryUI(this);

		stepSound = Resource.GetSounds("sounds/step", 6);
		jumpSound = Resource.GetSound("sounds/jump_bare.ogg");
		landSound = Resource.GetSound("sounds/land.ogg");
		ladderSound = Resource.GetSounds("sounds/step_wood", 3);
		hitSound = Resource.GetSounds("sounds/flesh", 2);
		wallTouchSound = Resource.GetSound("sounds/wall_touch.ogg");
	}

	public override void init(Level level)
	{
		startTime = Time.currentTime;

		level.addEntity(wallSlideParticles = ParticleEffects.CreateWallSlideEffect(this), position);
	}

	public override void onLevelSwitch(Level newLevel)
	{
		if (handParticles != null)
			GameState.instance.moveEntityToLevel(handParticles, newLevel);
		if (offhandParticles != null)
			GameState.instance.moveEntityToLevel(offhandParticles, newLevel);
		GameState.instance.moveEntityToLevel(wallSlideParticles, newLevel);
	}

	public void setStartingClass(StartingClass startingClass)
	{
		if (this.startingClass != null)
			money += startingClass.cost;
		clearInventory();
		for (int i = 0; i < startingClass.items.Length; i++)
			giveItem(startingClass.items[i].copy());

		strength = startingClass.strength;
		dexterity = startingClass.dexterity;
		intelligence = startingClass.intelligence;
		maxHealth = startingClass.hp * 0.5f;
		maxMana = startingClass.magic * 0.5f;

		health = maxHealth;
		mana = maxMana;

		xp = 0;
		playerLevel = 1;

		money = Math.Max(money - startingClass.cost, 0);

		this.startingClass = startingClass;
	}

	public bool equipHandItem(Item item)
	{
		if (handItem != null)
		{
			/*
			handItem.onUnequip(this);
			if (handParticles != null)
			{
				handParticles.remove();
				handParticles = null;
			}
			*/
			if (handItem.isSecondaryItem && (!item.twoHanded || canEquipOnehanded) && offhandItem == null)
			{
				Item _item = handItem;
				unequipItem(_item);
				equipOffhandItem(_item);
			}
			else
			{
				Item _item = handItem;
				if (storedItems.Count < storeCapacity)
					storeItem(_item);
				else
					dropItem(handItem);
			}
		}

		//if (item.twoHanded && offhandItem != null)
		//	unequipItem(offhandItem);
		if (item.twoHanded && !canEquipOnehanded && offhandItem != null)
		{
			if (storedItems.Count < storeCapacity)
				storeItem(offhandItem);
			else
				dropItem(offhandItem);
		}

		handItem = item;
		handItem.onEquip(this);
		if (handItem.hasParticleEffect)
			GameState.instance.level.addEntity(handParticles = handItem.createParticleEffect(null), position + handItem.particlesOffset);

		if (item.equipSound != null)
			Audio.PlayOrganic(item.equipSound, new Vector3(position, 0));

		return true;
	}

	public bool equipOffhandItem(Item item)
	{
		if (offhandItem != null)
		{
			/*
			offhandItem.onUnequip(this);
			if (offhandParticles != null)
			{
				offhandParticles.remove();
				offhandParticles = null;
			}
			*/
			if (storedItems.Count < storeCapacity)
				storeItem(offhandItem);
			else
				dropItem(offhandItem);
		}

		//if (handItem != null && handItem.twoHanded)
		//	unequipItem(handItem);
		if (handItem != null && handItem.twoHanded && !canEquipOnehanded)
		{
			if (storedItems.Count < storeCapacity)
				storeItem(handItem);
			else
				dropItem(handItem);
		}

		offhandItem = item;
		offhandItem.onEquip(this);
		if (offhandItem.hasParticleEffect)
			GameState.instance.level.addEntity(offhandParticles = offhandItem.createParticleEffect(null), position + offhandItem.particlesOffset);

		if (item.equipSound != null)
			Audio.PlayOrganic(item.equipSound, new Vector3(position, 0));

		return true;
	}

	public bool equipActiveItem(Item item)
	{
		for (int i = 0; i < activeItems.Length; i++)
		{
			if (activeItems[i] == null)
			{
				activeItems[i] = item;
				activeItems[i].onEquip(this);
				if (item.equipSound != null)
					Audio.PlayOrganic(item.equipSound, new Vector3(position, 0));
				return true;
			}
		}

		dropItem(activeItems[activeItems.Length - 1]);
		activeItems[activeItems.Length - 1] = item;
		activeItems[activeItems.Length - 1].onEquip(this);
		return true;
	}

	public bool equipPassiveItem(Item item)
	{
		if (item.armorSlot != ArmorSlot.None)
		{
			for (int i = 0; i < passiveItems.Count; i++)
			{
				if (passiveItems[i].armorSlot == item.armorSlot)
				{
					if (storedItems.Count < storeCapacity)
						storeItem(passiveItems[i]);
					else
						dropItem(passiveItems[i]);
					break;
				}
			}
		}

		passiveItems.Add(item);
		passiveItems.Sort((Item item1, Item item2) =>
		{
			int getScore(Item item) => item.isHandItem && !item.isSecondaryItem ? 1 :
			item.isHandItem ? 2 :
			item.isSecondaryItem ? 3 :
			item.isActiveItem ? 4 :
			item.isPassiveItem && item.armorSlot != ArmorSlot.None ? 5 + (int)item.armorSlot :
			item.isPassiveItem ? 5 + (int)ArmorSlot.Count : 100;
			int score1 = getScore(item1);
			int score2 = getScore(item2);
			return score1 > score2 ? 1 : score1 < score2 ? -1 : 0;
		});

		item.onEquip(this);
		if (item.equipSound != null)
			Audio.PlayOrganic(item.equipSound, new Vector3(position, 0));

		return true;
	}

	public bool attuneSpell(Spell spell)
	{
		if (spellItems.Count == spellCapacity)
			dropItem(spellItems[spellItems.Count - 1]);
		spellItems.Add(spell);
		return true;
	}

	public bool equipItem(Item item)
	{
		for (int i = 0; i < storedItems.Count; i++)
		{
			if (storedItems[i] == item)
			{
				storedItems.RemoveAt(i);
				break;
			}
		}

		if (canEquipOffhandItem(item))
			return equipOffhandItem(item);
		if (item.isHandItem)
			return equipHandItem(item);
		if (item.isSecondaryItem)
			return equipOffhandItem(item);
		if (item.isActiveItem)
			return equipActiveItem(item);
		if (item.isPassiveItem)
			return equipPassiveItem(item);
		if (item.type == ItemType.Spell)
			return attuneSpell((Spell)item);

		Debug.Assert(false);
		return false;
	}

	public bool unequipItem(Item item)
	{
		if (handItem == item)
		{
			handItem.onUnequip(this);
			if (handParticles != null)
			{
				handParticles.remove();
				handParticles = null;
			}
			handItem = null;
			return true;
		}
		if (offhandItem == item)
		{
			offhandItem.onUnequip(this);
			if (offhandParticles != null)
			{
				offhandParticles.remove();
				offhandParticles = null;
			}
			offhandItem = null;
			return true;
		}
		for (int i = 0; i < activeItems.Length; i++)
		{
			if (activeItems[i] == item)
			{
				activeItems[i].onUnequip(this);
				activeItems[i] = null;
				/*
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
				*/
				return true;
			}
		}
		for (int i = 0; i < spellItems.Count; i++)
		{
			if (spellItems[i] == item)
			{
				spellItems[i].onUnequip(this);
				spellItems.RemoveAt(i);
				return true;
			}
		}
		for (int i = 0; i < passiveItems.Count; i++)
		{
			if (passiveItems[i] == item)
			{
				passiveItems[i].onUnequip(this);
				passiveItems.RemoveAt(i);
				return true;
			}
		}
		return false;
	}

	bool canEquipHandItem(Item item)
	{
		return item.isHandItem && handItem == null;
	}

	bool canEquipOffhandItem(Item item)
	{
		return (item.isSecondaryItem || item.isHandItem && canEquipOffhand) && (offhandItem == null || handItem != null) && (!item.isHandItem || handItem != null) && (handItem == null || !handItem.twoHanded || canEquipOnehanded);
	}

	public void giveItem(Item item)
	{
		if (item.stackable)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i].id == item.id)
				{
					items[i].stackSize += item.stackSize;
					return;
				}
			}
		}

		items.Add(item);
		//items.Sort((a, b) => a.type.CompareTo(b.type));
		items.Sort((Item item1, Item item2) =>
		{
			int getScore(Item item) => item.isHandItem && !item.isSecondaryItem ? 1 :
				item.isHandItem ? 2 :
				item.isSecondaryItem ? 3 :
				item.isActiveItem ? 4 :
				item.isPassiveItem && item.armorSlot != ArmorSlot.None ? 5 + (int)item.armorSlot :
				item.isPassiveItem ? 5 + (int)ArmorSlot.Count : 100;
			int score1 = getScore(item1);
			int score2 = getScore(item2);
			return score1 > score2 ? 1 : score1 < score2 ? -1 : 0;
		});

		if (canEquipOffhandItem(item) && offhandItem == null)
			equipOffhandItem(item);
		else if (canEquipHandItem(item))
			equipHandItem(item);
		else if (item.isActiveItem)
			equipActiveItem(item);
		else if (item.type == ItemType.Spell && spellItems.Count < spellCapacity)
			attuneSpell((Spell)item);
		else if (canEquipPassiveItem(item))
			equipPassiveItem(item);
		else
		{
			if (storedItems.Count == storeCapacity)
				dropItem(storedItems[storeCapacity - 1]);
			storeItem(item);
		}

		QuestManager.onItemPickup(item);
	}

	public void removeItem(Item item)
	{
		unequipItem(item);
		items.Remove(item);
		for (int i = 0; i < storedItems.Count; i++)
		{
			if (storedItems[i] == item)
			{
				storedItems.RemoveAt(i);
				break;
			}
		}
	}

	public bool storeItem(Item item)
	{
		if (storedItems.Count < storeCapacity)
		{
			unequipItem(item);
			storedItems.Add(item);
			return true;
		}
		return false;
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
			if (items[i].name == name)
				return items[i];
		}
		return null;
	}

	public bool hasItem(Item item)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i] == item)
				return true;
		}
		return false;
	}

	public bool hasItemOfType(ItemType type)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].type == type)
				return true;
		}
		return false;
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
		if (isSpellItem(item, out _))
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
		for (int i = 0; i < passiveItems.Count; i++)
		{
			if (passiveItems[i] == item)
			{
				if (item.armorSlot != ArmorSlot.None)
					slot = i;
				else
					slot = -1;
				return true;
			}
		}
		slot = -1;
		return false;
	}

	public bool getArmorItem(ArmorSlot slot, out int slotIdx)
	{
		for (int i = 0; i < passiveItems.Count; i++)
		{
			if (passiveItems[i].armorSlot == slot)
			{
				slotIdx = i;
				return true;
			}
		}
		slotIdx = -1;
		return false;
	}

	public Item getArmorItem(ArmorSlot slot)
	{
		if (getArmorItem(slot, out int slotIdx))
			return passiveItems[slotIdx];
		return null;
	}

	public bool isSpellItem(Item item, out int slot)
	{
		for (int i = 0; i < spellItems.Count; i++)
		{
			if (spellItems[i] == item)
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

	public bool canEquipPassiveItem(Item item)
	{
		if (item.isPassiveItem)
		{
			if (item.armorSlot != ArmorSlot.None)
			{
				for (int i = 0; i < passiveItems.Count; i++)
				{
					if (passiveItems[i].armorSlot == item.armorSlot)
						return false;
				}
			}
			return true;
		}
		return false;
	}

	public ItemEntity dropItem(Item item)
	{
		removeItem(item);
		Vector2 itemVelocity = velocity; // + new Vector2(direction, 1) * new Vector2(0.4f, MathHelper.RandomFloat(0.14f, 0.16f)) * 14;
		ItemEntity obj = new ItemEntity(item, null, itemVelocity);
		GameState.instance.level.addEntity(obj, position + Vector2.Up * 0.5f);
		return obj;
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
				velocity.y = MathF.Max(velocity.y, jumpPower);
		}
		else
		{
			itemVelocity += new Vector2(direction, 1) * (shortThrow ? new Vector2(0.4f, MathHelper.RandomFloat(0.14f, 0.16f)) : farThrow ? new Vector2(2, MathHelper.RandomFloat(0.14f, 0.16f)) : new Vector2(1, MathHelper.RandomFloat(0.14f, 0.16f))) * 14;
		}
		Vector2 throwOrigin = position + new Vector2(0, 0.5f);
		ItemEntity obj = new ItemEntity(item, this, itemVelocity);
		GameState.instance.level.addEntity(obj, throwOrigin);
	}

	public ItemEntity throwItem(Item item, Vector2 direction, float speed = 14, bool throws = true)
	{
		direction = direction.normalized;
		Vector2 itemVelocity = velocity * 0.5f + direction * speed;
		if (!isGrounded && Vector2.Dot(direction, Vector2.UnitY) < -0.8f)
			velocity.y = MathF.Max(velocity.y, 0) + 5.0f;
		Vector2 throwOrigin = position + new Vector2(0, 0.5f) + direction.normalized * 0.1f;
		ItemEntity obj = new ItemEntity(item, throws ? this : null, itemVelocity);
		if (item.projectileSpins)
			obj.rotationVelocity = MathF.PI * MathHelper.RandomFloat(-5, 5);
		GameState.instance.level.addEntity(obj, throwOrigin);
		Audio.PlayOrganic(Resource.GetSound("sounds/swing3.ogg"), new Vector3(position, 0));
		return obj;
	}

	public void clearInventory()
	{
		for (int i = 0; i < items.Count; i++)
			removeItem(items[i--]);
	}

	public void carryObject(Object obj)
	{
		Debug.Assert(carriedObject == null);
		carriedObject = obj;
		level.removeEntity(obj);
	}

	public void dropObject()
	{
		level.addEntity(carriedObject, position + new Vector2(0, 0.05f - carriedObject.collider.min.y), false);
		carriedObject.velocity = Vector2.Zero;
		carriedObject.rotationVelocity = 0;
		carriedObject = null;
	}

	public void throwObject()
	{
		level.addEntity(carriedObject, position + new Vector2(0, 0.5f - carriedObject.collider.center.y), false);
		carriedObject.velocity = new Vector2(0.5f, 1) * velocity + (lookDirection + Vector2.Up * 0.2f) * 15;
		carriedObject.rotationVelocity = MathHelper.RandomFloat(-1, 1) * 10;
		carriedObject.throwTime = Time.currentTime;
		carriedObject.thrower = this;
		carriedObject = null;
	}

	public void addImpulse(Vector2 impulse)
	{
		impulseVelocity += impulse.x;
		velocity.y += impulse.y;
	}

	public bool hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true, bool buffedHit = false)
	{
		if (!isAlive)
			return false;

		Mob mob = by as Mob;

		Projectile projectile = by as Projectile;
		ItemEntity itemEntity = by as ItemEntity;

		if (projectile != null)
			mob = projectile.shooter as Mob;
		else if (itemEntity != null)
			mob = itemEntity.thrower as Mob;

		if (mob != null)
			by = mob;

		bool invincible = lastIFrameTrigger != -1 && (Time.currentTime - lastIFrameTrigger) / 1e9f < iframeDuration;
		if (invincible)
		{
			return false;
		}
		else if (blockingItem != null && (((BlockAction)actions.currentAction).isBlocking || ((BlockAction)actions.currentAction).isParrying))
		{
			BlockAction blockAction = actions.currentAction as BlockAction;

			// play sound
			// play particle effect

			if (blockAction.isParrying)
			{
				damage = 0;
				triggerInvincibility = false;

				blockAction.duration = blockAction.elapsedTime + 0.3f;

				lastIFrameTrigger = Time.currentTime;
				iframeDuration = 0.25f;

				level.addEntity(new ParryEffect(this), position + new Vector2(0.25f * direction, getWeaponOrigin(((BlockAction)actions.currentAction).mainHand).y));

				Audio.Play(blockingItem.parrySound, new Vector3(position, 0), 2);

				GameState.instance.freeze(0.2f);
			}
			else if (blockAction.isBlocking)
			{
				damage *= 1 - blockingItem.blockAbsorption;

				Audio.Play(blockingItem.blockSound, new Vector3(position, 0), 2);
			}

			if (mob != null)
			{
				if (projectile == null && itemEntity == null)
				{
					mob.stun(3, blockAction.isParrying);
					mob.addImpulse(new Vector2(direction, 0.1f) * 8);
					if (blockingItem.damageReflect > 0)
						mob.hit(blockingItem.damageReflect, this, blockingItem, null, false);
				}
				else
				{
					if (projectile != null)
					{
						projectile.velocity.x *= -1.0f;
						projectile.velocity.y = MathF.Max(projectile.velocity.y, 0);
					}
					else if (itemEntity != null)
					{
						itemEntity.velocity.x *= -1.0f;
						itemEntity.velocity.y = MathF.Max(itemEntity.velocity.y, 0);
					}
				}
			}

			if (damage < 0.0001f)
				return false;
		}

		if (damage > 0)
		{
			float totalArmor = getTotalArmor();
			float armorAbsorption = Item.GetArmorAbsorption(totalArmor);
			damage *= 1 - armorAbsorption;
			damage /= getDefenseModifier();

			health -= damage;

			GameState.instance.run.hitsTaken++;

			for (int i = 0; i < items.Count; i++)
			{
				if (isEquipped(items[i]))
					items[i].onHit(this, by, damage);
			}

			if (health <= 0)
			{
				onDeath(by, byName);
			}

			if (triggerInvincibility || health <= 0)
				Audio.PlayOrganic(hitSound, new Vector3(position, 0), 3);

			if (by != null)
			{
				Vector2 enemyPosition = by.position + (by.collider != null ? by.collider.center : Vector2.Zero);

				if (by.collider != null)
				{
					float knockbackStrength = item != null ? item.knockback : 8.0f;
					Vector2 knockback = (position - enemyPosition).normalized * knockbackStrength;
					addImpulse(knockback);
				}

				if (triggerInvincibility)
					GameState.instance.level.addEntity(ParticleEffects.CreateBloodEffect((position - enemyPosition).normalized), position + collider.center);
			}
			else
			{
				if (triggerInvincibility)
					GameState.instance.level.addEntity(ParticleEffects.CreateBloodEffect(Vector2.Up), position + collider.center);
			}

			if (triggerInvincibility)
			{
				lastIFrameTrigger = Time.currentTime;
				lastHit = Time.currentTime;
				iframeDuration = HIT_COOLDOWN;
				GameState.instance.freeze(0.2f);
			}

			return true;
		}

		Debug.Assert(false);
		return false;
	}

	public void stun()
	{
		isStunned = true;
		stunTime = Time.currentTime;
		addStatusEffect(new StunStatus(STUN_DURATION));
		if (actions.currentAction != null)
			actions.currentAction.cancel();
	}

	void onDeath(Entity by, string byName)
	{
		if (handItem != null)
		{
			Item handItemCopy = handItem.copy();
			Vector2 direction = new Vector2(0, 1) + MathHelper.RandomVector2(-0.5f, 0.5f);
			throwItem(handItemCopy, direction, 10, false);
		}
		if (offhandItem != null)
		{
			Item offhandItemCopy = offhandItem.copy();
			Vector2 direction = new Vector2(0, 1) + MathHelper.RandomVector2(-0.5f, 0.5f);
			throwItem(offhandItemCopy, direction, 10, false);
		}

		for (int i = 0; i < passiveItems.Count; i++)
		{
			//if (passiveItems[i] != null && passiveItems[i].ingameSprite == null)
			{
				passiveItems[i].onUnequip(this);
				//passiveItems[i] = null;
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

		GameState.instance.level.addEntity(corpse = new MobCorpse(sprite, Vector4.One, animator, rect, direction, velocity, impulseVelocity, collider, true, passiveItems), position);

		GameState.instance.stopRun(false, by, byName != null ? byName : by != null && by.displayName != null ? by.displayName : "???");

		Input.cursorMode = CursorMode.Normal;
	}

	public void consumeMana(float amount)
	{
		mana = MathF.Max(mana - amount, 0);
	}

	public StatusEffect addStatusEffect(StatusEffect effect)
	{
		statusEffects.Add(effect);
		effect.init(this);
		return effect;
	}

	public void heal(float amount)
	{
		health = MathF.Min(health + amount, maxHealth);
	}

	public void setVisible(bool visible)
	{
		isVisible = visible;
	}

	public void removeStatusEffect(StatusEffect effect)
	{
		Debug.Assert(statusEffects.Contains(effect));
		statusEffects.Remove(effect);
	}

	public bool hasStatusEffect(string name, out StatusEffect effect)
	{
		for (int i = 0; i < statusEffects.Count; i++)
		{
			if (statusEffects[i].name == name)
			{
				effect = statusEffects[i];
				return true;
			}
		}
		effect = null;
		return false;
	}

	public void onKill(Mob mob)
	{
		GameState.instance.run.kills++;

		if (mana < maxMana)
			mana = MathF.Min(mana + MANA_KILL_REWARD, maxMana);

		for (int i = 0; i < items.Count; i++)
			items[i].onKill(this, mob);

		QuestManager.onKill(mob);
	}

	public void awardXP(int amount)
	{
		xp += amount;
		while (xp >= nextLevelXP)
		{
			xp -= nextLevelXP;
			onLevelUp();
		}
	}

	void onLevelUp()
	{
		playerLevel++;

		if (playerLevel % 4 == 0)
		{
			maxHealth += 0.5f;
			maxMana += 0.5f;
		}

		//availableStatUpgrades++;
		//hp++;
		//magic++;
		strength++;
		dexterity++;
		intelligence++;

		//if (playerLevel % 5 == 0)
		//	GameState.instance.level.addEntity(new RelicOffer(), position + Vector2.Up * 0.5f);

		GameState.instance.level.addEntity(new LevelUpEffect(this), position + Vector2.Up * 1);

		addStatusEffect(new HealStatusEffect(maxHealth, 2));
		//addStatusEffect(new ManaRechargeEffect(maxMana, 2));
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

		if (isAlive && !isStunned && (actions.currentAction == null || actions.currentAction.canMove))
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
				if (isDucked)
				{
					TileType tile = GameState.instance.level.getTile(position);
					if (position.x - MathF.Floor(position.x) > -collider.position.x && MathF.Ceiling(position.x) - position.x > collider.position.x + collider.size.x &&
						tile != null && tile.isPlatform && MathHelper.Fract(position.y) > tile.platformHeight - 0.25f)
						position.y = MathF.Floor(position.y) + tile.platformHeight - 0.25f;
				}
			}

			if (canWallJump)
			{
				if (InputManager.IsDown("Right") && GameState.instance.level.overlapTiles(position + new Vector2(0, 0.1f), position + new Vector2(collider.max.x + 0.2f, collider.max.y - 0.1f)))
				{
					//if ((Time.currentTime - lastWallTouchRight) / 1e9f > COYOTE_TIME && velocity.y < -0.5f)
					//	Audio.PlayOrganic(wallTouchSound, new Vector3(position, 0), 1.0f);
					lastWallTouchRight = Time.currentTime;
				}
				if (InputManager.IsDown("Left") && GameState.instance.level.overlapTiles(position + new Vector2(collider.min.x - 0.2f, 0.1f), position + new Vector2(0.0f, collider.max.y - 0.1f)))
				{
					//if ((Time.currentTime - lastWallTouchLeft) / 1e9f > COYOTE_TIME && velocity.y < -0.5f)
					//	Audio.PlayOrganic(wallTouchSound, new Vector3(position, 0), 1.0f);
					lastWallTouchLeft = Time.currentTime;
				}
			}

			isSprinting = InputManager.IsDown("Sprint") && (isSprinting ? mana > 0 : mana > 0.2f) && delta.lengthSquared > 0;

			isDucked = InputManager.IsDown("Down") && numOverlaysOpen == 0;
			collider.size.y = isDucked ? 0.4f : 0.8f;
			if (!isDucked)
			{
				if (MathHelper.Fract(position.y) > 1 - collider.max.y)
				{
					TileType topTile = level.getTile(position + new Vector2(0, collider.max.y));
					if (topTile != null && topTile.isSolid)
						position.y = MathF.Min(position.y, MathF.Floor(position.y + collider.max.y) - collider.max.y);
				}
			}

			isLookingUp = isGrounded && InputManager.IsDown("Up");

			if (isGrounded)
			{
				lastGrounded = Time.currentTime;
				airJumpsLeft = airJumps;
			}

			if (InputManager.IsPressed("Jump") && (actions.currentAction == null || actions.currentAction.canJump))
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
						Audio.Play(jumpSound, new Vector3(position, 0), 0.5f);
						lastJumpInput = 0;
						lastGrounded = 0;
					}
					else if (airJumpsLeft > 0)
					{
						velocity.y = jumpPower;
						lastJumpInput = 0;
						airJumpsLeft--;
						level.addEntity(ParticleEffects.CreateAirJumpEffect(this), position);
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
					Audio.Play(jumpSound, new Vector3(position, 0), 0.5f);
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

		if (delta.x != 0 || actions.currentAction != null && actions.currentAction.actionMovement != 0)
		{
			velocity.x = delta.x * speed * currentSpeedModifier;
			if (actions.currentAction != null && actions.currentAction.actionMovement != 0)
				velocity.x += actions.currentAction.actionMovement;

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

		if (isAlive && numOverlaysOpen == 0)
		{
			float maxCursorDistance = 5; // (handItem != null ? MathF.Min(handItem.attackRange * 2, 5) : 1.8f) * 0.2f;

			Vector2 controllerAim = Input.GamepadAxisRight;
			if (controllerAim.lengthSquared > 0.25f)
				lookDirection = controllerAim * maxCursorDistance;

			if (delta.x != 0)
				direction = MathF.Sign(delta.x);

			if (Settings.game.aimMode == AimMode.Simple)
			{
				if (InputManager.IsDown("Up"))
					lookDirection = Vector2.Up;
				else if (/*!isGrounded &&*/ InputManager.IsDown("Down"))
					lookDirection = Vector2.Down;
				else
					lookDirection = new Vector2(direction, 0);
			}
			else if (Settings.game.aimMode == AimMode.Directional)
			{
				if (Input.cursorHasMoved)
				{
					/*
					if ((Renderer.cursorPosition - playerScreenPos).length > maxCursorDistance * 16)
					{
						Vector2i newCursorPos = playerScreenPos + (Vector2i)Vector2.Round((Renderer.cursorPosition - playerScreenPos).normalized * maxCursorDistance * 16);
						Input.cursorPosition = newCursorPos * Display.viewportSize / new Vector2i(Renderer.UIWidth, Renderer.UIHeight);
					}
					*/
					lookDirection = GameState.instance.camera.screenToWorld(Renderer.cursorPosition) - GameState.instance.camera.screenToWorld(Renderer.size / 2); // (position + collider.center);
					if (lookDirection.length > maxCursorDistance)
						lookDirection = lookDirection.normalized * maxCursorDistance;

					/*
					if (MathF.Abs(lookDirection.x) > maxCursorDistance)
						lookDirection.x = MathF.Sign(lookDirection.x) * maxCursorDistance;
					if (MathF.Abs(lookDirection.y) > maxCursorDistance)
						lookDirection.y = MathF.Sign(lookDirection.y) * maxCursorDistance;
					*/

					Vector2i playerScreenPos = Display.viewportSize / 2; // new Vector2i(Renderer.UIWidth, Renderer.UIHeight) / 2; // GameState.instance.camera.worldToScreen(position + collider.center);
					if ((Input.cursorPosition - playerScreenPos).length > maxCursorDistance * 16 * CaveDiver.instance.scale)
					//if (MathF.Abs(Input.cursorPosition.x - playerScreenPos.x) > maxCursorDistance * 16 * IvoryKeep.instance.scale ||
					//	MathF.Abs(Input.cursorPosition.y - playerScreenPos.y) > maxCursorDistance * 16 * IvoryKeep.instance.scale)
					{
						Vector2 dir = (Vector2)(Input.cursorPosition - playerScreenPos);
						dir = dir.normalized * maxCursorDistance * 16 * CaveDiver.instance.scale;
						dir += playerScreenPos;

						int x = (int)dir.x; //Math.Clamp(Input.cursorPosition.x, (int)MathF.Round(playerScreenPos.x - maxCursorDistance * 16 * IvoryKeep.instance.scale), (int)MathF.Round(playerScreenPos.x + maxCursorDistance * 16 * IvoryKeep.instance.scale));
						int y = (int)dir.y; //Math.Clamp(Input.cursorPosition.y, (int)MathF.Round(playerScreenPos.y - maxCursorDistance * 16 * IvoryKeep.instance.scale), (int)MathF.Round(playerScreenPos.y + maxCursorDistance * 16 * IvoryKeep.instance.scale));
						Vector2i newCursorPos = new Vector2i(x, y);
						Input.cursorPosition = newCursorPos; // * Display.viewportSize / new Vector2i(Renderer.UIWidth, Renderer.UIHeight);
					}
				}
			}
			else if (Settings.game.aimMode == AimMode.Crosshair)
			{
				if (Input.cursorHasMoved)
				{
					lookDirection = GameState.instance.camera.screenToWorld(Renderer.cursorPosition) - (position + collider.center);
				}

				if (actions.currentAction != null && actions.currentAction.turnToCrosshair)
					direction = Math.Sign(lookDirection.x);
				else if (delta.x != 0)
					direction = MathF.Sign(delta.x);
			}

			//lookDirection = Vector2.Rotate(Vector2.Right, MathF.Floor((lookDirection.angle + MathF.PI * 0.125f) / (MathF.PI * 0.25f)) * MathF.PI * 0.25f);
			if (MathF.Abs(lookDirection.x) < 0.001f)
				lookDirection.x = 0;
			if (MathF.Abs(lookDirection.y) < 0.001f)
				lookDirection.y = 0;
		}

		if (!isClimbing)
		{
			float gravityMultiplier = 1;
			if (!isAlive || !InputManager.IsDown("Jump"))
			{
				gravityMultiplier = 2;
				if (InputManager.IsReleased("Jump"))
					velocity.y = MathF.Min(velocity.y, 0);
			}
			//if (InputManager.IsDown("Down") && actions.currentAction == null)
			//	gravityMultiplier *= 1.5f;
			velocity.y += gravityMultiplier * gravity * Time.deltaTime;
			velocity.y = MathF.Max(velocity.y, MAX_FALL_SPEED);

			if (lastWallTouchLeft == Time.currentTime && InputManager.IsDown("Left") || lastWallTouchRight == Time.currentTime && InputManager.IsDown("Right"))
				velocity.y = MathF.Max(velocity.y, -16 / wallControl);

			wallJumpFactor = MathHelper.Linear(wallJumpFactor, 0, wallControl * getWallControlModifier() * Time.deltaTime);
			velocity.x = MathHelper.Lerp(velocity.x, wallJumpVelocity, wallJumpFactor);

			impulseVelocity = MathHelper.Lerp(impulseVelocity, 0, 8 * Time.deltaTime);
			if (MathF.Sign(impulseVelocity) == MathF.Sign(velocity.x))
				impulseVelocity = 0;
			else if (velocity.x == 0)
				impulseVelocity = MathF.Sign(impulseVelocity) * MathF.Min(MathF.Abs(impulseVelocity), speed);
			//impulseVelocity.x = impulseVelocity.x - velocity.x;
			velocity += new Vector2(impulseVelocity, 0);

			if (isGrounded && lastLadderJumpedFrom != null)
				lastLadderJumpedFrom = null;
		}
		else
		{
			velocity.y = delta.y * climbingSpeed * equipLoadModifier;
		}

		Vector2 displacement = velocity * Time.deltaTime;

		if (!isGrounded && !isClimbing && displacement.y < 0)
			fallDistance += -displacement.y;
		else
			fallDistance = 0;

		int collisionFlags = GameState.instance.level.doCollision(ref position, collider, ref displacement, isDucked, true);

		isGrounded = false;
		if ((collisionFlags & Level.COLLISION_Y) != 0)
		{
			if (fallDistance >= FALL_STUN_DISTANCE && velocity.y <= MAX_FALL_SPEED)
			{
				stun();
			}
			if (fallDistance >= FALL_DAMAGE_DISTANCE && velocity.y <= MAX_FALL_SPEED)
			{
				//float fallDmg = (fallDistance - FALL_DAMAGE_DISTANCE) * 0.5f / equipLoadModifier;
				//hit(fallDmg, null, null, "A high fall", false);
			}
			if (velocity.y < -2)
				onLand();

			if (velocity.y < 0)
				isGrounded = true;

			velocity.y = 0;
			//impulseVelocity.x *= 0.5f;
		}
		if ((collisionFlags & Level.COLLISION_X) != 0)
		{
			impulseVelocity = 0;
			wallJumpFactor = 0;
		}

		position += displacement;

		if (corpse != null)
			position = corpse.position;


		//TileType below = TileType.Get(GameState.instance.level.getTile((Vector2i)Vector2.Floor(position - new Vector2(0, 0.1f))));
		//isGrounded = below != null && below.isSolid;

		// why was this here again? idk
		//float rotationDst = direction == 1 ? 0 : MathF.PI;
		//rotation = MathHelper.Lerp(rotation, rotationDst, 5 * Time.deltaTime);
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

	void switchActiveItem()
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

	void switchSpellItem()
	{
		if (handItem != null && handItem.type == ItemType.Staff)
		{
			bool switched = false;
			for (int i = 0; i < spellItems.Count; i++)
			{
				if (spellItems[(selectedSpellItem + 1 + i) % spellItems.Count] != null)
				{
					selectedSpellItem = (selectedSpellItem + 1 + i) % spellItems.Count;
					switched = true;
					hud.onSpellSwitch();
					break;
				}
			}
			if (!switched && spellItems.Count > 0)
				selectedSpellItem = (selectedSpellItem + 1) % spellItems.Count;
		}
	}

	void updateActions()
	{
		if (isAlive && GameState.instance.run.active)
		{
			if (InputManager.IsPressed("SwitchItem"))
				switchActiveItem();
			if (InputManager.IsPressed("SwitchSpell"))
				switchSpellItem();
			if (InputManager.IsPressed("UseItem") && numOverlaysOpen == 0)
			{
				if (activeItems[selectedActiveItem] != null)
				{
					useActiveItem(activeItems[selectedActiveItem]);
					if (activeItems[selectedActiveItem] == null && numActiveItems > 0)
						switchActiveItem();
				}
			}
			for (int i = 0; i < Math.Min(activeItems.Length, 9); i++)
			{
				if (Input.IsKeyPressed(KeyCode.Key1 + i))
				{
					selectedActiveItem = i;
					//if (activeItems[i] != null)
					//	useActiveItem(activeItems[i]);
				}
			}

			currentAttackInput = null;
			if (!isStunned && numOverlaysOpen == 0)
			{
				if (carriedObject != null && isDucked && InputManager.IsPressed("Interact", true))
					dropObject();

				Interactable interactable = isAlive ? level.getInteractable(position + collider.center, this) : null;
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
					if (hoveredLadder != null && (InputManager.IsDown("Up") || InputManager.IsDown("Down")) && lastLadderJumpedFrom != hoveredLadder)
					{
						currentLadder = hoveredLadder;
						impulseVelocity = 0;
						wallJumpFactor = 0;
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


				{
					InputBinding attackInput = null;
					Vector2 directionalAttackDir = Vector2.Zero;
					bool secondary = false;
					if (InputManager.IsDown("Attack"))
					{
						attackInput = InputManager.GetBinding("Attack");
						directionalAttackDir += lookDirection;
					}
					if (InputManager.IsDown("Attack2"))
					{
						attackInput = InputManager.GetBinding("Attack2");
						directionalAttackDir += lookDirection;
						secondary = true;
					}
					if (InputManager.IsDown("AttackLeft"))
					{
						attackInput = InputManager.GetBinding("AttackLeft");
						directionalAttackDir += Vector2.Left;
						secondary = Input.IsKeyDown(KeyCode.Ctrl);
					}
					if (InputManager.IsDown("AttackRight"))
					{
						attackInput = InputManager.GetBinding("AttackRight");
						directionalAttackDir += Vector2.Right;
						secondary = Input.IsKeyDown(KeyCode.Ctrl);
					}
					if (InputManager.IsDown("AttackUp"))
					{
						attackInput = InputManager.GetBinding("AttackUp");
						directionalAttackDir += Vector2.Up;
						secondary = Input.IsKeyDown(KeyCode.Ctrl);
					}
					if (InputManager.IsDown("AttackDown"))
					{
						attackInput = InputManager.GetBinding("AttackDown");
						directionalAttackDir += Vector2.Down;
						secondary = Input.IsKeyDown(KeyCode.Ctrl);
					}

					if (MathF.Abs(directionalAttackDir.x) < 0.001f)
						directionalAttackDir.x = 0;
					if (MathF.Abs(directionalAttackDir.y) < 0.001f)
						directionalAttackDir.y = 0;

					if (attackInput != null)
					{
						currentAttackInput = attackInput;

						lookDirection = directionalAttackDir;
						if (directionalAttackDir.x != 0)
							direction = MathF.Sign(directionalAttackDir.x);

						Item handItem = this.handItem != null ? this.handItem : DefaultWeapon.instance;

						if (!secondary)
						{
							if (carriedObject != null)
							{
								if (attackInput.isPressed(true))
								{
									throwObject();
								}
							}
							else if (handItem != null)
							{
								if (handItem.trigger)
								{
									if (attackInput.isPressed(true))
									{
										if (handItem.use(this))
											removeItemSingle(handItem);
									}
								}
								else
								{
									if (actions.currentAction == null && attackInput.isPressed(true) || actions.actionQueue.Count == 1 && actions.currentAction.elapsedTime > 0.8f * actions.currentAction.duration)
									{
										if (handItem.use(this))
											removeItemSingle(handItem);
									}
								}
							}
							else
							{
								if (attackInput.isPressed(true) && (actions.currentAction == null || actions.actionQueue.Count == 1 && actions.currentAction.elapsedTime > 0.5f * actions.currentAction.duration))
								{
									DefaultWeapon.instance.use(this);
								}
							}
						}
						else
						{
							if (offhandItem != null)
							{
								if (offhandItem.trigger)
								{
									if (attackInput.isPressed(true))
									{
										if (offhandItem.use(this))
											removeItemSingle(offhandItem);
									}
								}
								else
								{
									if (actions.currentAction == null || actions.actionQueue.Count == 1 && actions.currentAction.elapsedTime > 0.8f * actions.currentAction.duration)
									{
										if (offhandItem.use(this))
											removeItemSingle(offhandItem);
									}
								}
							}
							else if (handItem != null)
							{
								if (handItem.trigger)
								{
									if (attackInput.isPressed(true))
									{
										if (handItem.useSecondary(this))
											removeItemSingle(handItem);
									}
								}
								else
								{
									if (actions.currentAction == null || actions.actionQueue.Count == 1 && actions.currentAction.elapsedTime > 0.8f * actions.currentAction.duration)
									{
										if (handItem.useSecondary(this))
											removeItemSingle(handItem);
									}
								}
							}
						}
					}
				}
			}

			if (handItem != null)
				handItem.update(this);
			else
				DefaultWeapon.instance.update(this);
			offhandItem?.update(this);
			for (int i = 0; i < activeItems.Length; i++)
			{
				if (activeItems[i] != null)
					activeItems[i].update(this);
			}
			for (int i = 0; i < passiveItems.Count; i++)
			{
				passiveItems[i].update(this);
			}

			actions.update();
		}
	}

	void updateStatus()
	{
		if (isSprinting)
			consumeMana(SPRINT_MANA_COST * Time.deltaTime);
		else if (mana < maxMana)
			mana = MathHelper.Clamp(mana + defaultManaRecoveryRate * maxMana * getManaRecoveryModifier() * Time.deltaTime, 0, maxMana);

		for (int i = 0; i < statusEffects.Count; i++)
		{
			if (!statusEffects[i].update(this) && isAlive)
			{
				statusEffects[i].destroy(this);
				statusEffects.RemoveAt(i--);
			}
		}
	}

	unsafe void updateAnimation()
	{
		if (isAlive)
		{
			if (actions.currentAction != null && actions.currentAction.animation != null)
			{
				animator.setAnimation(actions.currentAction.animation);
			}
			else
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
							animator.startTime = startTime;
							animator.getAnimation("run").fps = currentSpeedModifier * 12;
						}
						else
						{
							if (isLookingUp)
							{
								animator.setAnimation("look_up");
							}
							else
							{
								animator.setAnimation("idle");
							}
						}
					}
					else
					{
						if (isClimbing)
						{
							animator.setAnimation("climb");
							animator.getAnimation("climb").fps = velocity.y != 0 ? 4 : 0;
						}
						else
						{
							if (velocity.y < -2.5f)
							{
								animator.setAnimation("fall");
							}
							else
							{
								animator.setAnimation("jump");
								if (velocity.y > 2.5f)
									animator.currentFrame = 0;
								else
									animator.currentFrame = 1;
							}
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

		animator.update(DefaultWeapon.instance.ingameSprite);
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].ingameSprite != null)
			{
				animator.update(items[i].ingameSprite);
				items[i].ingameSprite.position *= items[i].ingameSpriteSize;
			}
		}


		if (lastWallTouchLeft == Time.currentTime && InputManager.IsDown("Left") || lastWallTouchRight == Time.currentTime && InputManager.IsDown("Right"))
		{
			wallSlideParticles.systems[0].handle->startVelocity.x = (lastWallTouchLeft == Time.currentTime ? 1 : -1) * 2;

			TileType tile = level.getTile(position + collider.center + (lastWallTouchLeft == Time.currentTime ? collider.min.x - 0.2f : collider.max.x + 0.2f));
			if (tile != null && tile.isSolid)
			{
				wallSlideParticles.systems[0].handle->colorAnim.value0.value.xyz = MathHelper.ARGBToVector(tile.particleColor).xyz;
				wallSlideParticles.systems[0].handle->colorAnim.value1.value.xyz = MathHelper.ARGBToVector(tile.particleColor).xyz;
				wallSlideParticles.systems[0].handle->colorAnim.value2.value.xyz = MathHelper.ARGBToVector(tile.particleColor).xyz;

				if (wallSlideParticles.systems[0].handle->emissionRate < 0.01f)
				{
					wallSlideParticles.systems[0].handle->emissionRate = 20;
					wallSlideParticles.systems[0].restartEffect();
				}
			}
			else
			{
				wallSlideParticles.systems[0].handle->emissionRate = 0.00001f;
			}
		}
		else
		{
			wallSlideParticles.systems[0].handle->emissionRate = 0.00001f;
		}
	}

	void onStep()
	{
		GameState.instance.run.stepsWalked++;

		if (!isDucked)
		{
			TileType tile = GameState.instance.level.getTile(position - new Vector2(0, 0.5f));
			if (tile != null)
				GameState.instance.level.addEntity(ParticleEffects.CreateStepEffect(MathHelper.RandomInt(2, 4), MathHelper.ARGBToVector(tile.particleColor).xyz), position);

			Audio.PlayOrganic(stepSound, new Vector3(position, 0), 0.5f);
			Item boots = getArmorItem(ArmorSlot.Boots) ?? DefaultWeapon.instance;
			if (boots.stepSound != null)
				Audio.PlayOrganic(boots.stepSound, new Vector3(position, 0), 0.5f);
		}
	}

	void onLand()
	{
		if (velocity.y < -10)
		{
			TileType tile = GameState.instance.level.getTile(position - new Vector2(0, 0.5f));
			if (tile != null)
				GameState.instance.level.addEntity(ParticleEffects.CreateStepEffect(MathHelper.RandomInt(4, 8), MathHelper.ARGBToVector(tile.particleColor).xyz), position);
		}

		Audio.PlayOrganic(landSound, new Vector3(position, 0), 0.5f);
		Item boots = getArmorItem(ArmorSlot.Boots) ?? DefaultWeapon.instance;
		if (boots.landSound != null)
			Audio.PlayOrganic(boots.landSound, new Vector3(position, 0));
	}

	void onClimbStep()
	{
		if (velocity.y != 0)
			Audio.PlayOrganic(ladderSound, new Vector3(position, 0));
	}

	public override void update()
	{
		updateMovement();
		updateActions();
		updateStatus();
		updateAnimation();

		//Audio.UpdateListener(new Vector3(position, 5), Quaternion.Identity);
		Audio.UpdateListener(new Vector3(GameState.instance.camera.position, 20), Quaternion.Identity);
		Audio.Set3DVolume(20.0f);

		if (numOverlaysOpen == 0)
			Input.cursorMode = CursorMode.Hidden;
	}

	public Vector2 getWeaponOrigin(bool mainHand)
	{
		int frame = mainHand ?
			((animator.lastFrameIdx + animator.getAnimation(animator.currentAnimation).length - 1) % animator.getAnimation(animator.currentAnimation).length)
			: ((animator.lastFrameIdx + animator.getAnimation(animator.currentAnimation).length - 1) % animator.getAnimation(animator.currentAnimation).length); // sway

		Vector2i animOffset = Vector2i.Zero;
		if (animator.currentAnimation == "idle")
			animOffset.y = -frame / 2;
		else if (animator.currentAnimation == "run")
		{
			animOffset.y = frame % 4 - frame % 4 / 3 * 2;
			animOffset.x = -Math.Abs((frame + (mainHand ? 5 : 1)) % 8 - 3) + 2;
		}
		else if (animator.currentAnimation == "jump")
			animOffset = new Vector2i(1, 2);
		else if (animator.currentAnimation == "fall")
			animOffset.y = 2;
		else if (animator.currentAnimation == "stun")
			animOffset.y = -2;
		if (isDucked && !isClimbing && isGrounded)
			animOffset.y = -3;
		return new Vector2((!mainHand ? 3 / 16.0f : -2 / 16.0f) + animOffset.x / 16.0f, (!mainHand ? 7 / 16.0f : 6 / 16.0f) + animOffset.y / 16.0f);
	}

	bool renderArms
	{
		get
		{
			for (int i = 0; i < passiveItems.Count; i++)
			{
				if (passiveItems[i].ingameSpriteCoversArms)
					return false;
			}
			return true;
		}
	}

	void renderHand(bool mainHand, float layer, Vector2 handPosition, float handRotation, bool flip)
	{
		if (!mainHand)
			layer += 0.0001f;

		Vector4 handColor = 0xFF282828;
		if (getArmorItem(ArmorSlot.Gloves, out int gloveSlot))
			handColor = passiveItems[gloveSlot].gloveColor;

		if (actions.currentAction != null && actions.currentAction.mainHand == mainHand)
		{
			Renderer.DrawSprite(position.x + handPosition.x - 1.0f / 16, position.y + handPosition.y - 2.0f / 16, layer, 2.0f / 16, 2.0f / 16, handRotation, null, false, handColor);
		}
		else
		{
			handPosition.x -= 1 / 16.0f;
			if (flip)
				handPosition.x *= -1;
			Renderer.DrawSprite(position.x + handPosition.x - 1.0f / 16, position.y + handPosition.y - 2.0f / 16, layer, 2.0f / 16, 2.0f / 16, 0, null, false, handColor);
		}
	}

	void renderHand(bool mainHand, float layer)
	{
		Vector2 handPosition = getWeaponOrigin(mainHand);
		renderHand(mainHand, layer, handPosition, 0, direction == -1);
	}

	void renderHand(bool mainHand, float layer, Matrix weaponTransform)
	{
		float handRotation = weaponTransform.rotation.angle * MathF.Sign(weaponTransform.rotation.axis.z);
		Vector2 handPosition = (weaponTransform * Matrix.CreateTranslation(-0.25f, 0, 0)).translation.xy;
		renderHand(mainHand, layer, handPosition, handRotation, false);
	}

	void renderHandItem(float layer, bool mainHand, Item item)
	{
		if (!isAlive)
			return;

		uint color = mainHand ? 0xFFFFFFFF : 0xFF9F9F9F;
		ParticleEffect particles = mainHand ? handParticles : offhandParticles;

		if (item == null)
			item = DefaultWeapon.instance;
		Item otherItem = mainHand ? offhandItem : handItem;

		if (item.sprite != null)
		{
			if (actions.currentAction != null && (actions.currentAction.getRenderWeapon(mainHand) != null || item.customAttackRender))
			{
				Item renderWeapon = actions.currentAction.getRenderWeapon(mainHand);
				if (renderWeapon != null)
				{
					item = renderWeapon;
					Matrix weaponTransform = actions.currentAction.getItemTransform(this, mainHand);
					Renderer.DrawSprite(item.size.x, item.size.y, Matrix.CreateTranslation(position.x, position.y, layer) * weaponTransform, item.sprite, color);

					if (item != DefaultWeapon.instance)
					{
						renderHand(mainHand, layer, weaponTransform);
						if (item.twoHanded && otherItem == null)
							renderHand(!mainHand, layer, weaponTransform);
					}

					if (particles != null)
					{
						particles.position = position + weaponTransform.translation.xy + item.particlesOffset * new Vector2i(direction, 1);
						particles.layer = layer - 0.01f;
					}
				}
			}
			else
			{
				if (item.ingameSprite != null && item != DefaultWeapon.instance)
				{
					Renderer.DrawSprite(position.x + rect.min.x * item.ingameSpriteSize, position.y + rect.min.y * item.ingameSpriteSize - 0.5f * (isDucked && !isClimbing && isGrounded ? 0.5f : 1), item.ingameSpriteLayer, rect.size.x * item.ingameSpriteSize, rect.size.y * item.ingameSpriteSize * (isDucked && !isClimbing && isGrounded ? 0.5f : 1), 0, item.ingameSprite, direction == -1, item.ingameSpriteColor);

					//Vector3 buffColor = MathHelper.ARGBToVector(0xFFdac66c).xyz;
					//float buffIntensity = MathHelper.Remap(MathF.Sin(Time.currentTime / 1e9f * 60), -1, 1, 0.1f, 1);
					//Renderer.DrawSpriteSolid(position.x - 0.5f * item.ingameSpriteSize, position.y + 0.5f - 0.5f * item.ingameSpriteSize, item.ingameSpriteLayer, item.ingameSpriteSize, (isDucked && !isClimbing ? 0.5f : 1) * item.ingameSpriteSize, 0, item.ingameSprite, direction == -1, new Vector4(buffColor, buffIntensity), true);

					if (particles != null)
					{
						Vector2 weaponPosition = new Vector2(position.x + (MathF.Round(item.renderOffset.x * 16) / 16 + getWeaponOrigin(mainHand).x) * direction, position.y + item.renderOffset.y + getWeaponOrigin(mainHand).y);
						particles.position = weaponPosition + item.particlesOffset * new Vector2i(direction, 1);
						particles.layer = layer - 0.01f;
					}
				}
				else if (!isClimbing && item != DefaultWeapon.instance)
				{
					Vector2 weaponPosition = new Vector2(position.x + (MathF.Round(item.renderOffset.x * 16) / 16 + getWeaponOrigin(mainHand).x) * direction, position.y + item.renderOffset.y + getWeaponOrigin(mainHand).y);
					Renderer.DrawSprite(weaponPosition.x - 0.5f * item.size.x, weaponPosition.y - 0.5f * item.size.y, layer, item.size.x, item.size.y, 0, item.sprite, direction == -1, color);

					renderHand(mainHand, layer);
					if (item.twoHanded && otherItem == null)
						renderHand(!mainHand, layer, getWeaponOrigin(mainHand) + Vector2.Right * 0.5f, 0, direction == -1);

					if (particles != null)
					{
						particles.position = weaponPosition + item.particlesOffset * new Vector2i(direction, 1);
						particles.layer = layer - 0.01f;
					}
				}
				else if (actions.currentAction == null || actions.currentAction.getRenderWeapon(mainHand) == null && actions.currentAction.getRenderWeapon(!mainHand) == null)
				{
					if (renderArms)
					{
						item = DefaultWeapon.instance;
						Renderer.DrawSprite(position.x + rect.min.x * item.ingameSpriteSize, position.y + rect.min.y * item.ingameSpriteSize /*- 0.5f * (isDucked && !isClimbing && isGrounded ? 0.5f : 1)*/, item.ingameSpriteLayer, rect.size.x * item.ingameSpriteSize, rect.size.y * item.ingameSpriteSize * (isDucked && !isClimbing && isGrounded ? 0.5f : 1), 0, item.ingameSprite, direction == -1, item.ingameSpriteColor);
					}

					/*
					if (actions.currentAction != null)
					{
						Vector2 weaponPosition = new Vector2(position.x + (MathF.Round(item.renderOffset.x * 16) / 16 + getWeaponOrigin(mainHand).x) * direction, position.y + item.renderOffset.y + getWeaponOrigin(mainHand).y);
						Renderer.DrawSprite(weaponPosition.x - 0.5f * item.size.x, weaponPosition.y - 0.5f * item.size.y, layer, item.size.x, item.size.y, 0, item.sprite, direction == -1, color);
						if (particles != null)
						{
							particles.position = weaponPosition + item.particlesOffset * new Vector2i(direction, 1);
							particles.layer = layer - 0.01f;
						}
					}
					*/
				}
			}
		}
	}

	void renderBackItem(float layer, Item item)
	{
		if (!isAlive)
			return;

		uint color = 0xFFFFFFFF;
		ParticleEffect particles = handParticles;

		if (item == null)
			item = DefaultWeapon.instance;

		if (item.sprite != null)
		{
			if (item != DefaultWeapon.instance)
			{
				if (item.ingameSprite != null)
				{
					Renderer.DrawSprite(position.x - 0.5f * item.ingameSpriteSize, position.y + 0.5f - 0.5f * item.ingameSpriteSize, item.ingameSpriteLayer, item.ingameSpriteSize, (isDucked && !isClimbing ? 0.5f : 1) * item.ingameSpriteSize, 0, item.ingameSprite, direction == -1, item.ingameSpriteColor);

					if (particles != null)
					{
						Vector2 weaponPosition = new Vector2(position.x + (MathF.Round(item.renderOffset.x * 16) / 16 + getWeaponOrigin(true).x) * direction, position.y + item.renderOffset.y + getWeaponOrigin(true).y);
						particles.position = weaponPosition + item.particlesOffset * new Vector2i(direction, 1);
						particles.layer = layer - 0.01f;
					}
				}
				else
				{
					Vector2 weaponPosition = new Vector2(position.x + (MathF.Round(item.renderOffset.x * 16) / 16 + getWeaponOrigin(true).x) * direction, position.y + item.renderOffset.y + getWeaponOrigin(true).y);
					Renderer.DrawSprite(weaponPosition.x - 0.5f * item.size.x, weaponPosition.y - 0.5f * item.size.y, layer, item.size.x, item.size.y, MathF.PI * 0.5f, item.sprite, false, color);
					if (particles != null)
					{
						particles.position = weaponPosition + item.particlesOffset * new Vector2i(direction, 1);
						particles.layer = layer - 0.01f;
					}
				}
			}
		}
	}

	public override void render()
	{
		if (!isAlive)
			return;

		bool hitCooldown = isAlive && (Time.currentTime - lastHit) / 1e9f < HIT_COOLDOWN;
		bool show = !hitCooldown || ((int)(Time.currentTime / 1e9f * 20) % 2 == 1);

		if (isVisible)
		{
			Vector2 snappedPosition = position;
			snappedPosition.x = MathF.Round(snappedPosition.x * 16) / 16;
			snappedPosition.y = MathF.Round(snappedPosition.y * 16) / 16;

			if (show)
				Renderer.DrawSprite(snappedPosition.x + rect.min.x, snappedPosition.y + rect.min.y, rect.size.x, (isDucked && !isClimbing && isGrounded ? 0.5f : 1) * rect.size.y, sprite, direction == -1, 0xFFFFFFFF);

			if (handItem != null)
				handItem.render(this);
			if (offhandItem != null)
				offhandItem.render(this);
			for (int i = passiveItems.Count - 1; i >= 0; i--)
			{
				if (passiveItems[i].ingameSprite != null && show && (passiveItems[i].armorSlot != ArmorSlot.Gloves || renderArms && (actions.currentAction == null || actions.currentAction.renderWeaponMain == null && actions.currentAction.renderWeaponSecondary == null)))
					Renderer.DrawSprite(position.x + rect.min.x * passiveItems[i].ingameSpriteSize, position.y + rect.min.y * passiveItems[i].ingameSpriteSize - 0.5f * (isDucked && !isClimbing && isGrounded ? 0.5f : 1), passiveItems[i].ingameSpriteLayer, rect.size.x * passiveItems[i].ingameSpriteSize, rect.size.y * passiveItems[i].ingameSpriteSize * (isDucked && !isClimbing && isGrounded ? 0.5f : 1), 0, passiveItems[i].ingameSprite, direction == -1, passiveItems[i].ingameSpriteColor);
				passiveItems[i].render(this);
			}
			for (int i = 0; i < activeItems.Length; i++)
			{
				if (activeItems[i] != null)
					activeItems[i].render(this);
			}

			if (show && carriedObject == null)
			{
				//if (isClimbing && actions.currentAction == null)
				//	renderBackItem(LAYER_PLAYER_ARMOR, handItem);
				//else 
				//if (actions.currentAction != null)
				{
					renderHandItem(LAYER_PLAYER_ITEM_MAIN, true, handItem);
					renderHandItem(LAYER_PLAYER_ITEM_SECONDARY, false, offhandItem);
				}
			}
		}

		if (actions.currentAction != null)
			actions.currentAction.render(this);

		if (carriedObject != null)
		{
			carriedObject.position = position + new Vector2(0, collider.max.y + 1 / 16.0f);
			carriedObject.rotation = 0;
			carriedObject.render();
		}

		for (int i = 0; i < statusEffects.Count; i++)
		{
			statusEffects[i].render(this);
		}
		for (int i = 0; i < itemBuffs.Count; i++)
		{
			itemBuffs[i].render(this);
		}

		Renderer.DrawLight(position + new Vector2(0, 0.5f), new Vector3(1.0f) * 1.5f, 7);

		if (GameState.instance.run.active)
		{
			hud.render();
			inventoryUI.render();
		}
	}

	public bool isAlive
	{
		get => health > 0;
	}

	public Vector2 center => position + collider.center;

	public float equipLoadModifier => MathF.Exp(-getTotalEquipLoad() * 0.02f);

	public float currentSpeedModifier
	{
		get
		{
			float value = (isSprinting ? SPRINT_MULTIPLIER : 1) * (isGrounded && isDucked ? DUCKED_MULTIPLIER : 1);
			value *= equipLoadModifier;
			value *= getMovementSpeedModifier();
			if (actions.currentAction != null)
				value *= actions.currentAction.speedMultiplier;
			if (carriedObject != null)
				value *= 0.7f;
			return value;
		}
	}

	public float getMovementSpeedModifier()
	{
		float value = 1;
		foreach (ItemBuff modifier in itemBuffs)
			value *= MathF.Pow(modifier.movementSpeedModifier, modifier.item.stackSize);
		value *= MathF.Pow(1.08f, swiftness - 1);
		return value;
	}

	public float getWallControlModifier()
	{
		float value = 1;
		foreach (ItemBuff modifier in itemBuffs)
			value *= MathF.Pow(modifier.wallControlModifier, modifier.item.stackSize);
		return value;
	}

	public float getMeleeDamageModifier()
	{
		float value = 1;
		foreach (ItemBuff modifier in itemBuffs)
			value *= MathF.Pow(modifier.meleeDamageModifier, modifier.item.stackSize);
		return value;
	}

	public float getRangedDamageModifier()
	{
		float value = 1;
		foreach (ItemBuff modifier in itemBuffs)
			value *= MathF.Pow(modifier.rangedDamageModifier, modifier.item.stackSize);
		return value;
	}

	public float getMagicDamageModifier()
	{
		float value = 1;
		foreach (ItemBuff modifier in itemBuffs)
			value *= MathF.Pow(modifier.magicDamageModifier, modifier.item.stackSize);
		return value;
	}

	public float getAttackSpeedModifier()
	{
		float value = 1;
		foreach (ItemBuff modifier in itemBuffs)
			value *= MathF.Pow(modifier.attackSpeedModifier, modifier.item.stackSize);
		return value;
	}

	public float getManaCostModifier()
	{
		float value = 1;
		foreach (ItemBuff modifier in itemBuffs)
			value *= MathF.Pow(modifier.manaCostModifier, modifier.item.stackSize);
		return value;
	}

	public float getManaRecoveryModifier()
	{
		float value = 1;
		foreach (ItemBuff modifier in itemBuffs)
			value *= MathF.Pow(modifier.manaRecoveryModifier, modifier.item.stackSize);
		return value;
	}

	public float getStealthAttackModifier()
	{
		float value = 1;
		foreach (ItemBuff modifier in itemBuffs)
			value *= MathF.Pow(modifier.stealthAttackModifier, modifier.item.stackSize);
		return value;
	}

	public float getDefenseModifier()
	{
		float value = 1;
		foreach (ItemBuff modifier in itemBuffs)
			value *= MathF.Pow(modifier.defenseModifier, modifier.item.stackSize);
		return value;
	}

	public float getAccuracyModifier()
	{
		float value = 1;
		foreach (ItemBuff modifier in itemBuffs)
			value *= MathF.Pow(modifier.accuracyModifier, modifier.item.stackSize);
		return value;
	}

	public float getCriticalChanceModifier()
	{
		float value = 1;
		foreach (ItemBuff modifier in itemBuffs)
			value *= MathF.Pow(modifier.criticalChanceModifier, modifier.item.stackSize);
		return value;
	}

	public float getCriticalAttackModifier()
	{
		float value = 2;
		foreach (ItemBuff modifier in itemBuffs)
			value *= MathF.Pow(modifier.criticalAttackModifier, modifier.item.stackSize);
		return value;
	}

	public float getTotalArmor()
	{
		float totalArmor = (handItem != null ? handItem.armor : 0) + (offhandItem != null ? offhandItem.armor : 0);
		for (int i = 0; i < activeItems.Length; i++)
		{
			if (activeItems[i] != null)
				totalArmor += activeItems[i].armor;
		}
		for (int i = 0; i < passiveItems.Count; i++)
		{
			totalArmor += passiveItems[i].armor;
		}
		return totalArmor;
	}

	public float getTotalEquipLoad()
	{
		float result = (handItem != null ? handItem.weight : 0) + (offhandItem != null ? offhandItem.weight : 0);
		for (int i = 0; i < passiveItems.Count; i++)
			result += passiveItems[i].weight;
		return result;
	}
}
