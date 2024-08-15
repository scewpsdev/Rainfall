using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum ItemType
{
	Tool,
	Active,
	Passive,
}

public abstract class Item
{
	public static SpriteSheet tileset = new SpriteSheet(Resource.GetTexture("res/sprites/items.png", false), 16, 16);


	public int id;
	public string name;
	public string displayName = "???";
	public ItemType type = ItemType.Tool;
	public bool stackable = false;
	public int stackSize = 1;
	public float rarity = 1;
	public float value = 1.0f;

	public int attackDamage = 1;
	public float attackRange = 1;
	public float attackAngle = MathF.PI;
	public float attackRate = 2.0f;
	public float secondaryChargeTime = 0.5f;
	public bool trigger = true;
	public int maxPierces = 0;

	public bool stab = true;
	public Vector2 size = new Vector2(1);

	public int armor = 0;

	public bool projectileItem = false;
	public bool breakOnHit = false;

	public Sprite sprite = null;
	public Sprite ingameSprite = null;

	// modifiers


	public Item(string name)
	{
		this.name = name;
		id = (int)Hash.hash(name);
	}

	public abstract Item createNew();

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


	static List<Item> itemTypes = new List<Item>();
	static Dictionary<string, int> nameMap = new Dictionary<string, int>();

	static List<int> toolItems = new List<int>();
	static List<int> activeItems = new List<int>();
	static List<int> passiveItems = new List<int>();


	public static void InitTypes()
	{
		InitType(new Skull());
		InitType(new Arrow());
		InitType(new Bomb());
		InitType(new Dagger());
		InitType(new Sword());
		InitType(new RopeItem());
		InitType(new Pickaxe());
		InitType(new Rock());
		InitType(new Cloak());
		InitType(new HealthPotion());
		InitType(new Boomerang());
		InitType(new Stick());
		InitType(new Spear());
		InitType(new TorchItem());
		InitType(new SpeedUpgrade());
		InitType(new HealthUpgrade());
		InitType(new Staff());
	}

	static void InitType(Item item)
	{
		itemTypes.Add(item);
		nameMap.Add(item.name, itemTypes.Count - 1);

		if (item.type == ItemType.Tool)
			toolItems.Add(itemTypes.Count - 1);
		else if (item.type == ItemType.Active)
			activeItems.Add(itemTypes.Count - 1);
		else if (item.type == ItemType.Passive)
			passiveItems.Add(itemTypes.Count - 1);
	}

	public static Item GetRandomItem(ItemType type, Random random)
	{
		List<int> list = type == ItemType.Tool ? toolItems : type == ItemType.Active ? activeItems : passiveItems;

		float cumulativeRarity = 0;
		foreach (int idx in list)
		{
			Item item = itemTypes[idx];
			cumulativeRarity += item.rarity;
		}

		float f = random.NextSingle() * cumulativeRarity;
		cumulativeRarity = 0;
		foreach (int idx in list)
		{
			Item item = itemTypes[idx];
			cumulativeRarity += item.rarity;
			if (f < cumulativeRarity)
				return item;
		}

		Debug.Assert(false);
		return null;
	}

	public static Item CreateRandom(Random random)
	{
		float toolChance = 0.4f;
		float activeChance = 0.4f;

		float f = random.NextSingle();
		Item item = null;

		if (f < toolChance)
			item = GetRandomItem(ItemType.Tool, random);
		else if (f < toolChance + activeChance)
			item = GetRandomItem(ItemType.Active, random);
		else
			item = GetRandomItem(ItemType.Passive, random);

		item = item.createNew();

		return item;
	}
}
