using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum ItemCategory
{
	None = 0,

	Weapon,
	Consumable,
	Armor,
	Arrow,
	Spell,
	Key,

	Count
}

public enum WeaponType
{
	None = 0,

	Melee,
	Bow,
	Staff,
	Shield,
}

public enum ArmorType
{
	None = 0,

	Helmet,
	Torso,
	Pants,
	Boots,
	Gloves
}

public enum AttackType
{
	None = 0,

	Light,
	Heavy,
	Running,
	Dodging,
	Sneak,
	Riposte,
	Cast,
}

public struct Attack
{
	public AttackType type;
	public string name;
	public string animation;
	public string followUp;
	public float damageTimeStart;
	public float damageTimeEnd;
	public float blockTimeStart;
	public float blockTimeEnd;
	public float parryTimeStart;
	public float parryTimeEnd;
	public float followUpCancelTime;
	public float damageMultiplier = 1.0f;
	public float poiseDamageMultiplier = 1.0f;
	public int staminaCost;
	public int manaCost;
	public AttackProjectile[] projectiles;

	public Attack(AttackType type)
	{
		this.type = type;
	}
}

public struct AttackProjectile
{
	public string name;
	public float time;
	public Vector3 offset;
	public bool follow;
	public bool consumesItem;
	public Sound sfx;
}

public enum ConsumableType
{
	Regeneration,
	Throwable,
}

public enum ConsumableEffectStat
{
	Health,
	Mana,
}

public class Item
{
	public string name;
	public string displayName;
	public string description;
	public ItemCategory category;

	public string entityPath;
	public SceneFormat.EntityData? entity;
	public Model moveset;
	public Texture icon;
	public bool isLoot;

	public float baseDamage = 5;
	public float criticalModifier = 5; // damage = fullDamage + criticalModifier / 10
	public float poiseDamage = 10;
	public float baseAbsorption = 5;
	public float blockStability = 5;
	public bool twoHanded = false;

	// Weapon Data
	public WeaponType weaponType;
	public Vector3 castOrigin;

	public List<Attack> attacks = new List<Attack>();

	// Armor Data
	public ArmorType armorType;

	// Consumable Data
	public ConsumableType consumableType;
	public ConsumableEffectStat consumableEffectStat;
	public float consumableEffectAmount;
	public float consumableEffectTime;
	public float consumableEffectDuration;
	public string consumableThrowEntity;
	public bool consumableUseTwoHanded;

	public AnimationLayer idleAnim;
	public AnimationLayer runAnim;
	public AnimationLayer jumpAnim;
	public AnimationLayer fallAnim;
	public AnimationLayer duckAnim;
	public AnimationLayer duckWalkAnim;

	public List<ActionSfx> useSounds;
	public Sound hitSound;
	public Sound sfxSwing;
	public Sound sfxSwingHeavy;
	public Sound blockSound;
	public Sound parrySound;
	public Sound sfxGuardBreak;
	public Sound sfxTake;
	public Sound equipSound;
	public Sound dropSound;
	public Sound bowDrawSound;
	public float bowDrawSoundTime;
	public Sound bowShootSound;
	public Sound sfxCast;
	public float sfxCastTime;


	public bool stackable
	{
		get => category == ItemCategory.Consumable || category == ItemCategory.Arrow;
	}

	public string typeSpecifier
	{
		get
		{
			if (category == ItemCategory.Weapon)
			{
				switch (weaponType)
				{
					case WeaponType.Melee:
						return "Melee Weapon";
					case WeaponType.Bow:
						return "Bow";
					case WeaponType.Staff:
						return "Staff";
					case WeaponType.Shield:
						return "Shield";
					default:
						return "???";
				}
			}
			else if (category == ItemCategory.Armor)
			{
				return armorType.ToString();
			}
			else
			{
				return category.ToString();
			}
		}
	}

	public Attack? getAttack(AttackType type, Attack? lastAttack = null)
	{
		if (lastAttack != null && lastAttack.Value.followUp != null)
		{
			for (int i = 0; i < attacks.Count; i++)
			{
				if (attacks[i].name == lastAttack.Value.followUp)
					return attacks[i];
			}
		}
		for (int i = 0; i < attacks.Count; i++)
		{
			if (attacks[i].type == type)
				return attacks[i];
		}
		return null;
	}

	public float getAbsorptionDamageModifier()
	{
		return 1 - baseAbsorption / 10.0f;
	}

	public float getCriticalDamageModifier()
	{
		return criticalModifier / 10.0f;
	}

	public float getStabilityStaminaModifier()
	{
		return 1 - blockStability / 10.0f;
	}


	static List<Item> items = new List<Item>();
	static Dictionary<string, Item> nameMap = new Dictionary<string, Item>();
	static Dictionary<ItemCategory, List<Item>> categoryMap = new Dictionary<ItemCategory, List<Item>>();

	public static void LoadContent()
	{
		string itemDir = "res/item";
		DirectoryInfo directoryInfo = new DirectoryInfo(itemDir);
		foreach (FileInfo fileInfo in directoryInfo.EnumerateFiles("", SearchOption.AllDirectories))
		{
			string relativePath = Path.GetRelativePath(".", fileInfo.FullName);
			if (relativePath.EndsWith(".dat.bin"))
			{
				Item item = ItemLoader.Load(relativePath.Substring(0, relativePath.Length - 4));
				if (item != null)
				{
					Debug.Assert(!items.Contains(item));
					items.Add(item);
					nameMap.Add(item.name, item);
					if (categoryMap.TryGetValue(item.category, out List<Item> categoryList))
						categoryList.Add(item);
					else
					{
						categoryList = new List<Item>();
						categoryList.Add(item);
						categoryMap.Add(item.category, categoryList);
					}
					Console.WriteLine("Loaded item " + item.name);
				}
			}
		}
	}

	public static Item Get(string name)
	{
		if (nameMap.TryGetValue(name, out Item item))
			return item;
		return null;
	}

	public static Item GetRandom(ItemCategory category, Random random)
	{
		if (categoryMap.TryGetValue(category, out List<Item> categoryList))
		{
			while (true)
			{
				Item item = categoryList[random.Next() % categoryList.Count];
				if (item.isLoot)
					return item;
			}
		}
		return null;
	}
}
