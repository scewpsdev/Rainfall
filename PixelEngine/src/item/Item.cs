using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;


public enum ItemType
{
	Weapon,
	Shield,
	Armor,
	Food,
	Potion,
	Ring,
	Staff,
	Scroll,
	Spell,
	Utility,
	Ammo,
	Gem,

	Count
}

public enum ArmorSlot
{
	None = -1,

	Helmet,
	Body,
	Gloves,
	Boots,
	Back,
	Ring1,
	Ring2,

	Count
}

public abstract class Item
{
	public static SpriteSheet tileset = new SpriteSheet(Resource.GetTexture("res/sprites/items.png", false), 16, 16);

	public static Sound[] weaponHit = Resource.GetSounds("res/sounds/hit_weapon", 6);
	public static Sound[] parryHit = [Resource.GetSound("res/sounds/parry.ogg")];
	public static Sound[] woodHit = Resource.GetSounds("res/sounds/hit_wood", 6);

	public static Sound[] defaultPickup = [Resource.GetSound("res/sounds/pickup.ogg")];
	public static Sound[] weaponPickup = Resource.GetSounds("res/sounds/pickup_weapon", 2);
	public static Sound[] potionPickup = [Resource.GetSound("res/sounds/pickup_potion.ogg")];

	public static Sound[] lightEquip = [Resource.GetSound("res/sounds/equip_light.ogg")];
	public static Sound[] mediumEquip = [Resource.GetSound("res/sounds/equip_medium.ogg")];
	public static Sound[] heavyEquip = [Resource.GetSound("res/sounds/equip_heavy.ogg")];
	public static Sound[] ringEquip = [Resource.GetSound("res/sounds/equip_ring.ogg")];

	public static Sound[] weaponUse = Resource.GetSounds("res/sounds/swing", 3);
	public static Sound[] potionUse = [Resource.GetSound("res/sounds/use_potion.ogg")];


	public string name;
	public ItemType type;
	public string displayName = "???";
	public string description = null;
	public bool stackable = false;
	public int stackSize = 1;
	public int value = 1;
	public bool canDrop = true;
	public bool isHandItem;
	public bool isActiveItem;
	public bool isPassiveItem;
	public bool isSecondaryItem = false;
	public bool twoHanded = false;
	public ArmorSlot armorSlot = ArmorSlot.None;
	public bool identified = true;

	public float attackDamage = 1;
	public float attackRange = 1;
	public float attackAngle = MathF.PI;
	public float attackAngleOffset = -0.25f * MathF.PI;
	public float attackRate = 2.0f;
	public float attackCooldown = 1.0f;
	public float secondaryChargeTime = 0.5f;
	public float blockDuration = 0.7f;
	public float damageReflect = 0.0f;
	public bool trigger = true;
	public int maxPierces = 0;
	public int maxRicochets = 0;
	public float knockback = 4.0f;
	public float manaCost = 0;
	public string requiredAmmo = null;
	public int staffCharges = 0;
	public int maxStaffCharges = 0;

	public bool stab = true;
	public Vector2 size = new Vector2(1);
	public Vector2 renderOffset = new Vector2(0.0f, 0.0f);
	public float attackRotationOffset = 0.0f;
	public FloatRect collider = new FloatRect(-0.25f, -0.25f, 0.5f, 0.5f);

	public int armor = 0;

	public bool projectileItem = false;
	public float projectileRotationOffset = 0.0f;
	public bool projectileSticks = false;
	public bool projectileSpins = false;
	public bool projectileAims = false;
	public bool breakOnWallHit = false;
	public bool breakOnEnemyHit = false;

	public Sprite sprite = null;
	public Sprite icon = null;
	public Vector4 spriteColor = Vector4.One;
	public Sprite ingameSprite = null;
	public int ingameSpriteSize = 1;
	public Vector4 ingameSpriteColor = Vector4.One;

	public bool hasParticleEffect = false;
	public Vector2 particlesOffset = Vector2.Zero;

	public bool upgradable = false;
	public int upgradeLevel = 0;

	public Sound[] useSound;
	public Sound[] hitSound;
	public Sound[] blockSound;
	public Sound[] pickupSound;
	public Sound[] equipSound;


	public Item(string name, ItemType type)
	{
		this.name = name;
		this.type = type;

		isHandItem = type == ItemType.Weapon || type == ItemType.Staff || type == ItemType.Ammo;
		isActiveItem = type == ItemType.Potion || type == ItemType.Scroll || type == ItemType.Spell || type == ItemType.Food || type == ItemType.Utility;
		isPassiveItem = type == ItemType.Armor || type == ItemType.Ring;

		upgradable = type == ItemType.Weapon || type == ItemType.Staff || type == ItemType.Spell || type == ItemType.Armor;

		useSound = type == ItemType.Weapon ? weaponUse : type == ItemType.Potion ? potionUse : null;
		hitSound = type == ItemType.Weapon ? weaponHit : woodHit;
		blockSound = type == ItemType.Weapon ? parryHit : weaponHit;
		pickupSound = type == ItemType.Weapon ? weaponPickup : type == ItemType.Potion ? potionPickup : defaultPickup;
		equipSound = type == ItemType.Ring ? ringEquip : type == ItemType.Weapon ? heavyEquip : type == ItemType.Armor ? mediumEquip : lightEquip;

		knockback = type == ItemType.Weapon || type == ItemType.Staff ? 4 : type == ItemType.Spell ? 1 : 4;
	}

	public Item copy()
	{
		return (Item)MemberwiseClone();
	}

	public uint id
	{
		get => Hash.hash(name);
	}

	public float rarity
	{
		get => MathF.Exp(-value * 0.04f);
	}

	public static float GetArmorAbsorption(int armor)
	{
		return armor / (10.0f + armor);
	}

	public string fullDisplayName
	{
		get => (stackable && stackSize > 1 ? stackSize + "x " : "") + displayName + (upgradeLevel > 0 ? " +" + upgradeLevel : "");
	}

	public string rarityString
	{
		get
		{
			float r = rarity;
			if (r >= 1.0f)
				return "Garbage";
			if (r >= 0.5f)
				return "Common";
			if (r >= 0.1f)
				return "Uncommon";
			if (r >= 0.02f)
				return "Rare";
			return "Exceedingly Rare";
		}
	}

	public Sprite getIcon()
	{
		if (icon == null)
			icon = new Sprite(sprite.spriteSheet, (sprite.position.x + sprite.size.x / 2) / sprite.spriteSheet.spriteSize.x, sprite.position.y / sprite.spriteSheet.spriteSize.y, 1, 1);
		return icon;
	}

	public void identify()
	{
		identified = true;
	}

	public virtual void upgrade()
	{
		upgradeLevel++;
		value *= 2;
		if (type == ItemType.Weapon || type == ItemType.Staff)
			attackDamage *= 1.34f;
		else if (type == ItemType.Armor || type == ItemType.Shield)
			armor++;
	}

	public int upgradePrice
	{
		get
		{
			if (type == ItemType.Weapon || type == ItemType.Staff)
			{
				float dps = attackDamage * attackRate;
				return (int)(dps * 10 * (1 + upgradeLevel * 0.5f));
			}
			else if (type == ItemType.Armor)
			{
				return (int)(armor * 10 + (1 + upgradeLevel * 0.5f));
			}
			return value * 2;
		}
	}

	public virtual bool use(Player player)
	{
		if (useSound != null)
			Audio.PlayOrganic(useSound, new Vector3(player.position, 0));
		return false;
	}

	public virtual bool useSecondary(Player player)
	{
		return false;
	}

	public virtual void onEquip(Player player)
	{
	}

	public virtual void onUnequip(Player player)
	{
	}

	public virtual void onDestroy(ItemEntity entity)
	{
	}

	public virtual void onEntityBreak(ItemEntity entity)
	{
	}

	public virtual void update(Entity entity)
	{
		if (entity is Player)
		{
			Player player = entity as Player;
			if (player.interactableInFocus != null && player.interactableInFocus is ItemEntity)
			{
				ItemEntity itemEntity = player.interactableInFocus as ItemEntity;
				if (itemEntity.velocity.lengthSquared < 4 && itemEntity.item.name == requiredAmmo)
					itemEntity.interact(player);
			}
		}
	}

	public virtual void render(Entity entity)
	{
	}

	public virtual ParticleEffect createParticleEffect(Entity entity)
	{
		return null;
	}


	static List<Item> itemTypes = new List<Item>();
	static Dictionary<string, int> nameMap = new Dictionary<string, int>();
	static Dictionary<uint, int> hashMap = new Dictionary<uint, int>();

	static Dictionary<ItemType, List<int>> typeLists = new Dictionary<ItemType, List<int>>();


	public static void InitTypes()
	{
		for (int i = 0; i < (int)ItemType.Count; i++)
		{
			typeLists.Add((ItemType)i, new List<int>());
		}

		InitType(new Skull());
		InitType(new Arrow());
		InitType(new Bomb());
		InitType(new Dagger());
		InitType(new Longsword());
		InitType(new Rope());
		InitType(new Pickaxe());
		InitType(new Rock());
		InitType(new WizardsCloak());
		InitType(new PotionOfHealing());
		InitType(new Boomerang());
		InitType(new Quarterstaff());
		InitType(new Spear());
		InitType(new Torch());
		InitType(new RingOfSwiftness());
		InitType(new RingOfVitality());
		InitType(new ProjectileStaff());
		InitType(new PotionOfGreaterHealing());
		InitType(new Lantern());
		InitType(new BarbarianHelmet());
		InitType(new Cheese());
		InitType(new Revolver());
		InitType(new Stick());
		InitType(new BlankPaper());
		InitType(new ScrollOfWeaponEnchantment());
		InitType(new ScrollOfArmorEnchantment());
		InitType(new ScrollOfTeleportation());
		InitType(new ScrollOfMonsterCreation());
		InitType(new ScrollOfEarth());
		InitType(new RingOfTears());
		InitType(new Sapphire());
		InitType(new LightningStaff());
		InitType(new Bread());
		InitType(new PotionOfEnergy());
		InitType(new Apple());
		InitType(new GoldenApple());
		InitType(new ChainmailHood());
		InitType(new LeatherArmor());
		InitType(new Diamond());
		InitType(new Emerald());
		InitType(new Ruby());
		InitType(new ScrollOfDexterity());
		InitType(new IronShield());
		InitType(new ThornShield());
		InitType(new Scimitar());
		InitType(new WizardsHood());
		InitType(new Zweihander());
		InitType(new TravellingCloak());
		InitType(new Shortbow());
		InitType(new Longbow());
		InitType(new AutomaticCrossbow());
		InitType(new Crossbow());
		InitType(new BrokenSword());
		InitType(new OldHuntersRing());
		InitType(new WoodenMallet());
		InitType(new ScrollOfMonsterTaming());
		InitType(new Backpack());
		InitType(new ThrowingKnife());
		InitType(new StaffOfIllumination());
		InitType(new Halberd());
		InitType(new WizardsHat());
		InitType(new RingOfDexterity());
		InitType(new Greathammer());
		InitType(new MoonBlossom());
		InitType(new MoonFruit());
		InitType(new Rapier());
		InitType(new Handaxe());
		InitType(new Greataxe());
		InitType(new Amogus());
		InitType(new GlassBottle());
		InitType(new BottleOfWater());
		InitType(new PoisonVial());
		InitType(new PotionOfTeleport());
		InitType(new ScrollOfIdentify());
		InitType(new Potion());
		InitType(new IronKey());
		InitType(new Lockpick());
		InitType(new AmethystRing());
		InitType(new AssassinsDagger());
		InitType(new RoyalGreatsword());
		InitType(new Magnet());
		InitType(new MagicProjectileSpell());
		InitType(new MagicStaff());
		InitType(new LightningSpell());
		InitType(new LightOrbSpell());
		InitType(new DarkHood());
		InitType(new DarkCloak());
	}

	static void InitType(Item item)
	{
		itemTypes.Add(item);
		nameMap.Add(item.name, itemTypes.Count - 1);
		hashMap.Add(Hash.hash(item.name), itemTypes.Count - 1);
		typeLists[item.type].Add(itemTypes.Count - 1);
	}

	public static Item GetItemPrototype(string name)
	{
		if (nameMap.TryGetValue(name, out int idx))
			return itemTypes[idx];
		return null;
	}

	public static Item GetItemPrototype(uint h)
	{
		if (hashMap.TryGetValue(h, out int idx))
			return itemTypes[idx];
		return null;
	}

	public static List<Item> GetItemPrototypesOfType(ItemType type)
	{
		List<Item> items = new List<Item>(typeLists[type].Count);
		foreach (int idx in typeLists[type])
			items.Add(itemTypes[idx]);
		return items;
	}

	public static Item CreateRandom(ItemType type, Random random, float meanValue)
	{
		float value = meanValue * MathF.Pow(2, MathHelper.RandomGaussian(random) * 0.5f); // MathF.Max(meanValue + meanValue * MathHelper.RandomGaussian(random) * 0.25f, 0.0f);
		List<Item> items = GetItemPrototypesOfType(type);
		for (int i = 0; i < items.Count; i++)
		{
			if (!items[i].canDrop)
				items.RemoveAt(i--);
		}
		items.Sort((Item item1, Item item2) =>
		{
			float r1 = MathF.Abs(item1.value - value);
			float r2 = MathF.Abs(item2.value - value);
			return r1 > r2 ? 1 : r1 < r2 ? -1 : 0;
		});
		for (int j = 0; j < 5; j++)
		{
			for (int i = items.Count - 2; i >= 0; i--)
			{
				if (random.NextSingle() < 0.5f)
				{
					// swap
					Item tmp = items[i];
					items[i] = items[i + 1];
					items[i + 1] = tmp;
				}
			}
		}
		Item item = items[0];
		Item newItem = item.copy();
		while (newItem.value < meanValue * 0.5f && newItem.upgradable)
			newItem.upgrade();
		while (newItem.stackable && newItem.value * newItem.stackSize < 0.5f * meanValue)
		{
			int difference = (int)(meanValue / newItem.value - newItem.stackSize);
			newItem.stackSize += MathHelper.RandomInt(1, difference, random);
		}
		//if (newItem.name == "arrow")
		//	newItem.stackSize = MathHelper.RandomInt(1, 35, random);
		//else if (newItem.name == "throwing_knife")
		//	newItem.stackSize = MathHelper.RandomInt(1, 10, random);
		if (newItem.type == ItemType.Staff)
			newItem.staffCharges = MathHelper.RandomInt(newItem.maxStaffCharges / 2, newItem.maxStaffCharges, random);
		return newItem;

		/*
		List<int> list = new List<int>(typeLists[type]);

		for (int i = 0; i < list.Count; i++)
		{
			Item item = itemTypes[list[i]];
			//if (item.value < minValue || item.value > maxValue)
			//	list.RemoveAt(i--);
		}
		MathHelper.ShuffleList(list, random);

		float cumulativeRarity = 0;
		foreach (int idx in list)
		{
			Item item = itemTypes[idx];
			cumulativeRarity += item.canDrop ? item.rarity : 0;
		}

		float f = random.NextSingle() * cumulativeRarity;
		cumulativeRarity = 0;
		foreach (int idx in list)
		{
			Item item = itemTypes[idx];
			cumulativeRarity += item.canDrop ? item.rarity : 0;
			if (f < cumulativeRarity)
			{
				Item newItem = item.copy();
				if (newItem.name == "arrow")
					newItem.stackSize = MathHelper.RandomInt(1, 35, random);
				else if (newItem.name == "throwing_knife")
					newItem.stackSize = MathHelper.RandomInt(1, 10, random);
				if (newItem.type == ItemType.Staff)
					newItem.staffCharges = MathHelper.RandomInt(newItem.maxStaffCharges / 2, newItem.maxStaffCharges, random);
				return newItem;
			}
		}

		return null;
		*/
	}

	public static Item[] CreateRandom(Random random, float[] distribution, float meanValue)
	{
		float f = random.NextSingle();

		float r = 0;
		for (int i = 0; i < (int)ItemType.Count; i++)
		{
			r += distribution[i];
			if (f < r)
			{
				Item item = CreateRandom((ItemType)i, random, meanValue);
				if (item != null)
				{
					if (item.requiredAmmo != null)
					{
						Item ammo = GetItemPrototype(item.requiredAmmo).copy();
						ammo.stackSize = MathHelper.RandomInt(3, 20, random);
						return [item, ammo];
					}
					else
					{
						return [item];
					}
				}
				else
				{
					f = random.NextSingle();
					r = 0;
					i = -1;
				}
			}
		}

		return null;
	}
}
