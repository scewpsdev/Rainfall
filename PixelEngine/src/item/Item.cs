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
	public float value = 1;
	public bool canDrop = true;
	public bool canEquipMultiple = true;

	public int attackDamage = 1;
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


	public Item(string name)
	{
		this.name = name;
		id = (int)Hash.hash(name);
	}

	public Item copy()
	{
		return (Item)MemberwiseClone();
	}

	public float rarity
	{
		get => MathF.Exp(-value * 0.02f);
	}

	public static float GetArmorAbsorption(int armor)
	{
		return armor / (10.0f + armor);
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

	static List<int> toolItems = new List<int>();
	static List<int> activeItems = new List<int>();
	static List<int> passiveItems = new List<int>();


	public static void InitTypes()
	{
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
		InitType(new PotionOfMinorHealing());
		InitType(new PotionOfGreaterHealing());
		InitType(new PotionOfSupremeHealing());
		InitType(new Lantern());
		InitType(new BarbarianHelmet());
		InitType(new Cheese());
		InitType(new Revolver());
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

	public static Item GetRandomItem(ItemType type, Random random, float minValue = 0, float maxvalue = float.MaxValue)
	{
		List<int> list = new List<int>(type == ItemType.Tool ? toolItems : type == ItemType.Active ? activeItems : passiveItems);

		for (int i = 0; i < list.Count; i++)
		{
			Item item = itemTypes[list[i]];
			if (item.value < minValue || item.value > maxvalue)
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
				return item;
		}

		Debug.Assert(false);
		return null;
	}

	public static Item CreateRandom(Random random, float minValue = 0, float maxValue = float.MaxValue)
	{
		float toolChance = 0.2f;
		float activeChance = 0.4f;

		float f = random.NextSingle();
		Item item = null;

		if (f < toolChance)
			item = GetRandomItem(ItemType.Tool, random, minValue, maxValue);
		else if (f < toolChance + activeChance)
			item = GetRandomItem(ItemType.Active, random, minValue, maxValue);
		else
			item = GetRandomItem(ItemType.Passive, random, minValue, maxValue);

		item = item.copy();

		return item;
	}
}
