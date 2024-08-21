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
	Armor,
	Potion,
	Ring,
	Staff,
	Scroll,
	Food,
	Gem,
	Utility,

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
	public bool canEquipMultiple = true;

	public float attackDamage = 1;
	public float attackRange = 1;
	public float attackAngle = MathF.PI;
	public float attackRate = 2.0f;
	public float secondaryChargeTime = 0.5f;
	public bool trigger = true;
	public int maxPierces = 0;
	public float knockback = 8.0f;

	public bool stab = true;
	public Vector2 size = new Vector2(1);
	public Vector2 renderOffset = new Vector2(0.2f, 0.5f - 0.2f);

	public int armor = 0;

	public bool projectileItem = false;
	public bool breakOnHit = false;

	public Sprite sprite = null;
	public Sprite ingameSprite = null;

	// modifiers


	public Item(string name, ItemType type)
	{
		this.name = name;
		this.type = type;
	}

	public Item copy()
	{
		return (Item)MemberwiseClone();
	}

	public int id
	{
		get => (int)Hash.hash(name);
	}

	public bool isHandItem
	{
		get => type == ItemType.Weapon || type == ItemType.Staff;
	}

	public bool isActiveItem
	{
		get => type == ItemType.Potion || type == ItemType.Scroll || type == ItemType.Food || type == ItemType.Utility;
	}

	public bool isPassiveItem
	{
		get => type == ItemType.Armor || type == ItemType.Ring;
	}

	public float rarity
	{
		get => MathF.Exp(-value * 0.02f);
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
			if (r >= 0.9f)
				return "Common";
			if (r >= 0.5f)
				return "Uncommon";
			if (r >= 0.1f)
				return "Rare";
			return "Exceedingly Rare";
		}
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
	}

	static void InitType(Item item)
	{
		itemTypes.Add(item);
		nameMap.Add(item.name, itemTypes.Count - 1);
		typeLists[item.type].Add(itemTypes.Count - 1);
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
				return item.copy();
		}

		return null;
	}

	public static Item CreateRandom(Random random, float minValue = 0, float maxValue = float.MaxValue)
	{
		float[] distribution = [
			0.14f, // Weapon
			0.14f, // Armor
			0.18f, // Potion
			0.04f, // Ring
			0.06f, // Staff
			0.18f, // Scroll
			0.1f, // Food
			0.08f, // Gem
			0.08f, // Utility
		];

		float f = random.NextSingle();

		float r = 0;
		for (int i = 0; i < (int)ItemType.Count; i++)
		{
			r += distribution[i];
			if (f < r)
			{
				Item item = CreateRandom((ItemType)i, random, minValue, maxValue);
				if (item != null)
					return item.copy();
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
