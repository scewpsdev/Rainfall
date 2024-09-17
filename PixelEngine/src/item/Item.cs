using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
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
	Utility,
	Ammo,
	Gem,

	Count
}

public enum ArmorSlot
{
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


	public string name;
	public ItemType type;
	public string displayName = "???";
	public string description = null;
	public bool stackable = false;
	public int stackSize = 1;
	public float value = 1;
	public bool canDrop = true;
	public bool isHandItem;
	public bool isActiveItem;
	public bool isPassiveItem;
	public bool isSecondaryItem = false;
	public bool twoHanded = false;
	public ArmorSlot armorSlot;

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
	public FloatRect collider = new FloatRect(-0.25f, -0.25f, 0.5f, 0.5f);

	public int armor = 0;

	public bool projectileItem = false;
	public float projectileRotationOffset = 0.0f;
	public bool projectileSticks = false;
	public bool projectileSpins = false;
	public bool breakOnWallHit = false;
	public bool breakOnEnemyHit = false;

	public Sprite sprite = null;
	public Sprite icon = null;
	public Vector4 spriteColor = Vector4.One;
	public Sprite ingameSprite = null;
	public int ingameSpriteSize = 1;
	public Vector4 ingameSpriteColor = Vector4.One;

	// modifiers


	public Item(string name, ItemType type)
	{
		this.name = name;
		this.type = type;

		isHandItem = type == ItemType.Weapon || type == ItemType.Staff || type == ItemType.Ammo;
		isActiveItem = type == ItemType.Potion || type == ItemType.Scroll || type == ItemType.Food || type == ItemType.Utility;
		isPassiveItem = type == ItemType.Armor || type == ItemType.Ring;
	}

	public Item copy()
	{
		return (Item)MemberwiseClone();
	}

	public int id
	{
		get => (int)Hash.hash(name);
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
		get => (stackable && stackSize > 1 ? stackSize + "x " : "") + displayName;
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

	public virtual bool use(Player player)
	{
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

	public virtual void onEntityBreak(ItemEntity entity)
	{
	}

	public virtual void update(Entity entity)
	{
	}

	public virtual void render(Entity entity)
	{
	}


	static List<Item> itemTypes = new List<Item>();
	static Dictionary<string, int> nameMap = new Dictionary<string, int>();

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
		InitType(new MagicStaff());
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
		InitType(new ScrollOfWeaponWeightlessness());
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
		InitType(new HuntersRing());
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
	}

	static void InitType(Item item)
	{
		itemTypes.Add(item);
		nameMap.Add(item.name, itemTypes.Count - 1);
		typeLists[item.type].Add(itemTypes.Count - 1);
	}

	public static Item GetItemPrototype(string name)
	{
		if (nameMap.TryGetValue(name, out int idx))
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

	public static Item CreateRandom(ItemType type, Random random, float minValue = 0, float maxValue = float.MaxValue)
	{
		List<int> list = new List<int>(typeLists[type]);

		for (int i = 0; i < list.Count; i++)
		{
			Item item = itemTypes[list[i]];
			if (item.value < minValue || item.value > maxValue)
				list.RemoveAt(i--);
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
					newItem.stackSize = MathHelper.RandomInt(1, 12, random);
				else if (newItem.name == "throwing_knife")
					newItem.stackSize = MathHelper.RandomInt(1, 6, random);
				if (newItem.type == ItemType.Staff)
					newItem.staffCharges = MathHelper.RandomInt(newItem.maxStaffCharges / 2, newItem.maxStaffCharges, random);
				return newItem;
			}
		}

		return null;
	}

	public static Item[] CreateRandom(Random random, float[] distribution, float minValue = 0, float maxValue = float.MaxValue)
	{
		float f = random.NextSingle();

		float r = 0;
		for (int i = 0; i < (int)ItemType.Count; i++)
		{
			r += distribution[i];
			if (f < r)
			{
				Item item = CreateRandom((ItemType)i, random, minValue, maxValue);
				if (item != null)
				{
					item = item.copy();
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
