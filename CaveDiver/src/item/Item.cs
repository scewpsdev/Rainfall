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
	Relic,
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

public enum AttackAnim
{
	None = 0,

	Stab,
	SwingSideways,
	SwingOverhead,
}

public class Infusion
{
	public static readonly Infusion Sharp = new Infusion("Sharp") { damageMultiplier = 1.1f };
	public static readonly Infusion Blunt = new Infusion("Blunt") { damageMultiplier = 0.8f };
	public static readonly Infusion Light = new Infusion("Light") { attackSpeedMultiplier = 1.25f, weightMultiplier = 0.5f, damageMultiplier = 0.8f };
	public static readonly Infusion Heavy = new Infusion("Heavy") { attackSpeedMultiplier = 0.8f, weightMultiplier = 1.5f, damageMultiplier = 1.25f };
	public static readonly Infusion Long = new Infusion("Long") { rangeMultiplier = 1.25f };
	public static readonly Infusion Short = new Infusion("Short") { rangeMultiplier = 0.8f };
	public static readonly Infusion Flawless = new Infusion("Flawless") { damageMultiplier = 1.05f, attackSpeedMultiplier = 1.05f, weightMultiplier = 0.9f, rangeMultiplier = 1.1f };
	public static readonly Infusion Broken = new Infusion("Broken") { damageMultiplier = 0.5f, attackSpeedMultiplier = 1.2f, weightMultiplier = 0.5f, rangeMultiplier = 0.7f };

	public static readonly Infusion ArmorHeavy = new Infusion("Heavy") { armorMultiplier = 1.5f, weightMultiplier = 1.5f };
	public static readonly Infusion ArmorLight = new Infusion("Light") { armorMultiplier = 0.75f, weightMultiplier = 0.5f };
	public static readonly Infusion ArmorFlawless = new Infusion("Flawless") { armorMultiplier = 1.25f, weightMultiplier = 0.75f };
	public static readonly Infusion ArmorBattered = new Infusion("Battered") { armorMultiplier = 0.75f, weightMultiplier = 1.0f };
	public static readonly Infusion ArmorBroken = new Infusion("Broken") { armorMultiplier = 0.5f, weightMultiplier = 0.75f };

	public static readonly Infusion[] weaponInfusions = [Sharp, Blunt, Light, Heavy, Long, Short, Flawless, Broken];
	public static readonly Infusion[] armorInfusions = [ArmorHeavy, ArmorLight, ArmorFlawless, ArmorBattered, ArmorBroken];

	public static Infusion GetRandom(ItemType type, Random random)
	{
		while (true)
		{
			Infusion infusion = weaponInfusions[random.Next() % weaponInfusions.Length];
			if (type == ItemType.Weapon)
			{
				return infusion;
			}
			else if (type == ItemType.Staff)
			{
				if (infusion == Light || infusion == Heavy || infusion == Flawless || infusion == Broken)
					return infusion;
			}
			else
			{
				Debug.Assert(false);
			}
		}
	}

	public static Infusion GetArmorRandom(Random random)
	{
		return armorInfusions[random.Next() % armorInfusions.Length];
	}


	public string name;

	public float damageMultiplier = 1.0f;
	public float attackSpeedMultiplier = 1.0f;
	public float weightMultiplier = 1.0f;
	public float rangeMultiplier = 1.0f;
	public float armorMultiplier = 1.0f;

	private Infusion(string name)
	{
		this.name = name;
	}
}

public abstract class Item
{
	public static readonly SpriteSheet tileset = new SpriteSheet(Resource.GetTexture("sprites/items.png", false), 16, 16);

	public static readonly Sound[] weaponHit = Resource.GetSounds("sounds/hit_weapon", 6);
	public static readonly Sound[] parryHit = [Resource.GetSound("sounds/parry3.ogg")];
	public static readonly Sound[] woodHit = Resource.GetSounds("sounds/hit_wood", 7);

	public static readonly Sound[] defaultPickup = [Resource.GetSound("sounds/pickup.ogg")];
	public static readonly Sound[] weaponPickup = Resource.GetSounds("sounds/pickup_weapon", 2);
	public static readonly Sound[] potionPickup = [Resource.GetSound("sounds/pickup_potion.ogg")];

	public static readonly Sound[] lightEquip = [Resource.GetSound("sounds/equip_light.ogg")];
	public static readonly Sound[] mediumEquip = [Resource.GetSound("sounds/equip_medium.ogg")];
	public static readonly Sound[] heavyEquip = [Resource.GetSound("sounds/equip_heavy.ogg")];
	public static readonly Sound[] ringEquip = [Resource.GetSound("sounds/equip_ring.ogg")];

	public static readonly Sound[] weaponSwing = Resource.GetSounds("sounds/swing", 3);
	public static readonly Sound[] weaponThrust = Resource.GetSounds("sounds/swing_dagger", 4);
	public static readonly Sound[] potionUse = [Resource.GetSound("sounds/use_potion.ogg")];

	static readonly string[] scalingLetters = ["-", "D", "C", "B", "A", "S"];

	public static string GetScalingLetter(float scaling)
	{
		int idx = (int)MathF.Ceiling(MathHelper.Remap(scaling, 0, 1, 0, scalingLetters.Length - 1));
		Debug.Assert(idx >= 0 && idx < scalingLetters.Length);
		return scalingLetters[idx];
	}


	public string name;
	public ItemType type;
	public string displayName = "???";
	public string description = null;
	public bool stackable = false;
	public int stackSize = 1;
	public int value = 1;
	public float rarity = 1;
	public bool canDrop = true;
	public bool isHandItem;
	public bool isActiveItem;
	public bool isPassiveItem;
	public bool isSecondaryItem = false;
	public bool twoHanded = false;
	public ArmorSlot armorSlot = ArmorSlot.None;
	public bool identified = true;

	public float baseDamage = 1;
	public float getInfusedDamage()
	{
		float damage = baseDamage;

		foreach (Infusion infusion in infusions)
			damage *= infusion.damageMultiplier;

		return damage;
	}
	public float getAttackDamage(Player player)
	{
		float damage = getInfusedDamage();

		float hardCap = 20;
		float strengthSaturation = 1 - MathF.Pow(1 - MathF.Min((player.strength - 1) / (hardCap - 1), 1), 2);
		float dexteritySaturation = 1 - MathF.Pow(1 - MathF.Min((player.dexterity - 1) / (hardCap - 1), 1), 2);
		float intelligenceSaturation = 1 - MathF.Pow(1 - MathF.Min((player.intelligence - 1) / (hardCap - 1), 1), 2);

		damage *= 1 + strengthScaling * strengthSaturation;
		damage *= 1 + dexterityScaling * dexteritySaturation;
		damage *= 1 + intelligenceScaling * intelligenceSaturation;

		return damage;
	}

	protected float baseAttackRange = 1;
	public float attackRange
	{
		get
		{
			float range = baseAttackRange;
			foreach (Infusion infusion in infusions)
				range *= infusion.rangeMultiplier;
			return range;
		}
	}

	//public float attackAngle = 1.5f * MathF.PI;
	//public float attackAngleOffset = -0.75f * MathF.PI;
	public float attackStartAngle = 0.75f * MathF.PI;
	public float attackEndAngle = -0.75f * MathF.PI;

	protected float baseAttackRate = 2.0f;
	public float attackRate
	{
		get
		{
			float rate = baseAttackRate;
			foreach (Infusion infusion in infusions)
				rate *= infusion.attackSpeedMultiplier;
			return rate;
		}
	}

	public float strengthScaling = 0;
	public float dexterityScaling = 0;
	public float intelligenceScaling = 0;

	public float secondaryChargeTime = 0;
	public bool canParry = false;
	public bool canBlock = false;
	public float parryWindow = 0.25f;
	public float parryWeaponRotation = 0.5f * MathF.PI;
	public float blockCharge = 0.15f;
	public float actionMovementSpeed = 0.4f;
	public float blockAbsorption = 0.8f;
	public float damageReflect = 0.0f;
	public bool doubleBladed = false;
	public float criticalChanceModifier = 1.0f;
	public float accuracy = 1.0f;
	public bool trigger = true;
	public int maxPierces = 0;
	public int maxRicochets = 0;
	public float knockback = 6.0f;
	public float manaCost = 0;
	public string requiredAmmo = null;
	public int staffCharges = 0;
	public int maxStaffCharges = 0;
	public int staffAttunementSlots = 3;

	public float baseArmor = 0;
	public float armor
	{
		get
		{
			float armor = baseArmor;
			foreach (Infusion infusion in infusions)
				armor *= infusion.armorMultiplier;
			return armor;
		}
	}

	protected float baseWeight = 0.0f;
	public float weight
	{
		get
		{
			float value = baseWeight;
			foreach (Infusion infusion in infusions)
				value *= infusion.weightMultiplier;
			return value;
		}
	}

	public ItemBuff buff = null;

	public HashSet<Infusion> infusions = new HashSet<Infusion>();

	public AttackAnim anim = AttackAnim.SwingSideways;
	public float attackAcceleration = 3;
	public float attackCooldown = 1.0f;
	public bool customAttackRender = false;
	public float postAttackLinger = 0.25f;
	public Vector2 size = new Vector2(1);
	public Vector2 renderOffset = new Vector2(0.0f, 0.0f);
	public FloatRect collider = new FloatRect(-0.25f, -0.25f, 0.5f, 0.5f);

	public bool projectileItem = false;
	public float projectileRotationOffset = 0.0f;
	public bool projectileSticks = false;
	public bool projectileSpins = false;
	public bool projectileAims = false;
	public bool breakOnWallHit = false;
	public bool breakOnEnemyHit = false;
	public bool tumbles = true;
	public bool canIgnite = false;

	public Sprite sprite = null;
	Sprite _icon = null;
	public Sprite icon
	{
		get
		{
			if (_icon == null)
				_icon = new Sprite(sprite.spriteSheet, (sprite.position.x + sprite.size.x / 2) / sprite.spriteSheet.spriteSize.x, sprite.position.y / sprite.spriteSheet.spriteSize.y, 1, 1);
			return _icon;
		}
		set
		{
			_icon = value;
		}
	}
	public Sprite spellIcon = null;
	public Vector4 spriteColor = Vector4.One;
	public Sprite ingameSprite = null;
	public Vector4 ingameSpriteColor = Vector4.One;
	public float ingameSpriteLayer = Entity.LAYER_PLAYER_ARMOR;
	public int ingameSpriteSize { get => ingameSprite.width / GameState.instance.player.sprite.width; }
	public bool ingameSpriteCoversArms = false;

	public Vector4 gloveColor = new Vector4(1, 0, 1, 1);

	public bool hasParticleEffect = false;
	public Vector2 particlesOffset = Vector2.Zero;

	public bool upgradable = false;
	public int upgradeLevel = 0;

	public Sound[] useSound;
	public Sound[] castSound;
	public Sound[] hitSound;
	public Sound[] blockSound;
	public Sound[] parrySound;
	public Sound[] pickupSound;
	public Sound[] equipSound;
	public Sound[] stepSound;
	public Sound[] landSound;


	public Item(string name, ItemType type)
	{
		this.name = name;
		this.type = type;

		isHandItem = type == ItemType.Weapon || type == ItemType.Staff || type == ItemType.Ammo;
		isActiveItem = type == ItemType.Potion || type == ItemType.Scroll || type == ItemType.Food || type == ItemType.Utility;
		isPassiveItem = type == ItemType.Armor || type == ItemType.Relic;

		stackable = type == ItemType.Food || type == ItemType.Potion || type == ItemType.Relic || type == ItemType.Scroll || type == ItemType.Gem || type == ItemType.Ammo;
		upgradable = type == ItemType.Weapon || type == ItemType.Staff || type == ItemType.Spell || type == ItemType.Armor;

		useSound = type == ItemType.Weapon || type == ItemType.Staff ? weaponSwing : type == ItemType.Potion ? potionUse : null;
		hitSound = type == ItemType.Weapon ? weaponHit : woodHit;
		blockSound = weaponHit;
		parrySound = parryHit;
		pickupSound = type == ItemType.Weapon ? weaponPickup : type == ItemType.Potion ? potionPickup : defaultPickup;
		equipSound = type == ItemType.Relic ? ringEquip : type == ItemType.Weapon ? heavyEquip : type == ItemType.Armor ? mediumEquip : lightEquip;

		knockback = type == ItemType.Weapon || type == ItemType.Staff ? 4 : type == ItemType.Spell ? 1 : 4;
		baseWeight = type == ItemType.Weapon ? 2 : type == ItemType.Shield ? 2 : type == ItemType.Staff ? 1 : type == ItemType.Armor ? 1 : 0;
	}

	public Item copy()
	{
		Item copy = (Item)MemberwiseClone();
		if (buff != null)
		{
			copy.buff = buff.copy();
			copy.buff.item = copy;
		}
		copy.infusions = new HashSet<Infusion>(infusions);
		return copy;
	}

	public uint id
	{
		get => Hash.hash(name);
	}

	public static float GetArmorAbsorption(float armor)
	{
		return armor / (20.0f + armor);
	}

	/*
	public static float GetRarity(float value)
	{
		return MathF.Exp(-value * 0.04f);
	}
	*/

	public string fullDisplayName
	{
		get
		{
			StringBuilder result = new StringBuilder();
			if (stackable && stackSize > 1)
				result.Append(stackSize).Append("x ");
			foreach (Infusion infusion in infusions)
				result.Append(infusion.name).Append(' ');
			result.Append(displayName);
			if (upgradeLevel > 0)
				result.Append(" +").Append(upgradeLevel);
			return result.ToString();
		}
	}

	public string rarityString
	{
		get
		{
			float r = rarity * MathF.Exp(-0.04f * value);
			if (r >= 1.0f)
				return "Garbage";
			if (r >= 0.3f)
				return "Common";
			if (r >= 0.01f)
				return "Uncommon";
			if (r >= 0.001f)
				return "Rare";
			if (r >= 0.0001f)
				return "Exceedingly Rare";
			return "Legendary";
		}
	}

	public uint rarityColor
	{
		get
		{
			float r = rarity * MathF.Exp(-0.04f * value);
			if (r >= 1.0f)
				return UIColors.TEXT_RARITY_GARBAGE;
			if (r >= 0.3f)
				return UIColors.TEXT_RARITY_COMMON;
			if (r >= 0.01f)
				return UIColors.TEXT_RARITY_UNCOMMON;
			if (r >= 0.001f)
				return UIColors.TEXT_RARITY_RARE;
			if (r >= 0.0001f)
				return UIColors.TEXT_RARITY_EXCEEDINGLY_RARE;
			return UIColors.TEXT_RARITY_LEGENDARY;
		}
	}

	public void identify()
	{
		identified = true;
	}

	public virtual void upgrade()
	{
		value += upgradeCost / 2;
		upgradeLevel++;
		//value = value + MathHelper.IPow(upgradeLevel, 2) * 10; //Math.Min(value * 3 / 2, value + 1);
		if (type == ItemType.Weapon)
			baseDamage *= 1.1f;
		else if (type == ItemType.Staff)
		{
			baseDamage *= 1.05f;
			baseAttackRate *= 1.05f;
		}
		else if (type == ItemType.Armor || type == ItemType.Shield)
			baseArmor++;
	}

	public int upgradeCost
	{
		get
		{
			if (type == ItemType.Weapon)
			{
				float dps = MathF.Pow(baseDamage, 1.5f) * baseAttackRate;
				return (int)(dps * 5 * (1 + upgradeLevel * 0.5f));
			}
			else if (type == ItemType.Staff)
			{
				float dps = MathF.Pow(baseDamage, 1.5f) * baseAttackRate;
				return (int)(dps * 15 * (1 + upgradeLevel * 0.5f));
			}
			else if (type == ItemType.Armor)
			{
				return (int)(armor * 5 + (1 + upgradeLevel * 0.5f));
			}
			return value * 2;
		}
	}

	public bool addInfusion(Infusion infusion)
	{
		if (!infusions.Contains(infusion))
		{
			infusions.Add(infusion);
			return true;
		}
		return false;
	}

	public virtual bool use(Player player)
	{
		if (useSound != null)
			Audio.PlayOrganic(useSound, new Vector3(player.position, 0), 1, 1, 0.0f, 0.15f);
		return false;
	}

	public virtual bool useSecondary(Player player)
	{
		return false;
	}

	public virtual void onEquip(Player player)
	{
		if (buff != null)
			player.itemBuffs.Add(buff);
	}

	public virtual void onUnequip(Player player)
	{
		if (buff != null)
			player.itemBuffs.Remove(buff);
	}

	public virtual void onDestroy(ItemEntity entity)
	{
	}

	public virtual void onEntityBreak(ItemEntity entity)
	{
	}

	public virtual void onKill(Player player, Mob mob)
	{
	}

	public virtual void onHit(Player player, Entity by, float damage)
	{
	}

	public virtual void onEnemyHit(Player player, Mob mob, float damage, bool critical)
	{
	}

	public virtual void update(Entity entity)
	{
		if (entity is Player && requiredAmmo != null)
		{
			Player player = entity as Player;
			if (player.getItem(requiredAmmo) != null)
			{
				HitData[] hits = new HitData[16];
				int numHits = player.level.overlap(player.position + player.collider.center - 1, player.position + player.collider.center + 1, hits, Entity.FILTER_ITEM);
				for (int i = 0; i < numHits; i++)
				{
					Debug.Assert(hits[i].entity is ItemEntity);
					ItemEntity item = hits[i].entity as ItemEntity;
					if (item.item.name == requiredAmmo)
						item.interact(player);
				}
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

		InitType(new Arrow());
		InitType(new Bomb());
		InitType(new Dagger());
		InitType(new Longsword());
		InitType(new Rope());
		InitType(new Pickaxe());
		InitType(new WizardsCloak());
		InitType(new PotionOfHealing());
		InitType(new Boomerang());
		InitType(new Quarterstaff());
		InitType(new Spear());
		InitType(new Torch());
		InitType(new RingOfSwiftness());
		InitType(new AmethystRing());
		InitType(new MagicArrowStaff());
		InitType(new PotionOfGreaterHealing());
		InitType(new Lantern());
		InitType(new BarbarianHelmet());
		InitType(new Cheese());
		InitType(new Revolver());
		InitType(new Club());
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
		InitType(new WizardHat());
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
		InitType(new LargeWizardHat());
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
		InitType(new SapphireRing());
		//InitType(new AssassinsDagger());
		InitType(new RoyalGreatsword());
		InitType(new Magnet());
		InitType(new MagicArrowSpell());
		InitType(new MagicStaff());
		InitType(new LightningSpell());
		InitType(new IlluminationSpell());
		InitType(new DarkHood());
		InitType(new DarkCloak());
		InitType(new WizardsLegacy());
		InitType(new NimbleGloves());
		InitType(new Spellweaver());
		InitType(new Nightstalker());
		InitType(new BerserkersChain());
		InitType(new GlassRing());
		InitType(new EaglesEye());
		InitType(new RingOfRetaliation());
		InitType(new Flamberge());
		InitType(new LifegemRing());
		InitType(new Bloodfang());
		InitType(new Deadeye());
		InitType(new KeenEdge());
		InitType(new PotionOfInvisibility());
		InitType(new HuntersHat());
		InitType(new OldHuntersHat());
		InitType(new BurstShotSpell());
		InitType(new ElderwoodStaff());
		InitType(new AstralScepter());
		InitType(new MissileSpell());
		InitType(new WoodenShield());
		InitType(new BattleAxe());
		InitType(new MoonbladeAxe());
		InitType(new Shortsword());
		InitType(new IronArmor());
		InitType(new Formation());
		InitType(new LeatherCap());
		InitType(new TripleShotSpell());
		InitType(new HealingSpell());
		InitType(new ClimbingGear());
		InitType(new WingProsthetics());
		InitType(new ChainmailArmor());
		InitType(new RoundShield());
		InitType(new Twinblades());
		InitType(new AdventurersHood());
		InitType(new AK47());
		InitType(new WizardsHood());
		InitType(new QuestlineLoganStaff());
		InitType(new LostSigil());
		InitType(new LeatherGauntlets());
		InitType(new ChainmailGauntlets());
		InitType(new IronGauntlets());
		InitType(new LeatherBoots());
		InitType(new ChainmailBoots());
		InitType(new IronSabatons());
		InitType(new Parachute());
		InitType(new Jetpack());
		InitType(new SpectralShield());
		InitType(new MissileStaff());
		InitType(new BlademastersRing());
		InitType(new GiantsGauntlet());
		InitType(new Parsley());
		InitType(new LargeWizardHatRed());
		InitType(new DankHat());
		InitType(new BlacksteelGlaive());
		InitType(new RingOfThorns());
		InitType(new AdventurersHoodBlue());
		InitType(new Flail());
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

	static Item CreateRandom(List<Item> items, Random random, float meanValue)
	{
		items = new List<Item>(items);
		MathHelper.ShuffleList(items, random);
		for (int i = 0; i < items.Count; i++)
		{
			if (!items[i].canDrop)
				items.RemoveAt(i--);
		}

		for (int i = 0; i < items.Count; i++)
		{
			float gaussian = items[i].value <= meanValue ?
				MathHelper.Gaussian(items[i].value / meanValue - 1, 1, 1.0f) :
				MathHelper.Gaussian((items[i].value - meanValue) / 8, 0, 1);
			gaussian /= MathHelper.inv_sqrt_2pi;
			if (random.NextSingle() > gaussian && items.Count > 1)
				items.RemoveAt(i--);
		}

		float cumulativeRarity = 0;
		for (int i = 0; i < items.Count; i++)
			cumulativeRarity += items[i].rarity;

		float choice = random.NextSingle();
		float f = 0;
		Item item = null;
		for (int i = 0; i < items.Count; i++)
		{
			f += items[i].rarity / cumulativeRarity;
			if (f >= choice)
			{
				item = items[i];
				break;
			}
		}

		Debug.Assert(item != null);

		Item newItem = item.copy();
		ItemType type = item.type;

		while (newItem.value < meanValue * 0.5f && newItem.upgradable)
			newItem.upgrade();

		if (type == ItemType.Weapon || type == ItemType.Staff)
		{
			const float infusionChance = 0.1f;
			if (random.NextSingle() < infusionChance)
			{
				Infusion infusion = Infusion.GetRandom(type, random);
				newItem.addInfusion(infusion);
			}
		}
		else if (type == ItemType.Armor || type == ItemType.Shield)
		{
			const float infusionChance = 0.1f;
			if (random.NextSingle() < infusionChance)
			{
				Infusion infusion = Infusion.GetArmorRandom(random);
				newItem.addInfusion(infusion);
			}
		}

		while (newItem.stackable && newItem.type != ItemType.Food && newItem.value * newItem.stackSize * 3 / 2 < 0.5f * meanValue)
		{
			int difference = (int)(meanValue / newItem.value - newItem.stackSize);
			newItem.stackSize += MathHelper.RandomInt(1, difference, random);
		}

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

	public static Item CreateRandom(ItemType type, Random random, float meanValue)
	{
		List<Item> items = GetItemPrototypesOfType(type);
		return CreateRandom(items, random, meanValue);
	}

	public static Item[] CreateRandom(Random random, float[] distribution, float meanValue)
	{
		ItemType type = ItemType.Count;

		float f = random.NextSingle();
		float r = 0;
		for (int i = 0; i < (int)ItemType.Count; i++)
		{
			r += distribution[i];
			if (f < r)
			{
				type = (ItemType)i;
				break;
			}
		}

		if (type != ItemType.Count)
		{
			Item item = CreateRandom(type, random, meanValue);
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

		return null;
	}
}
