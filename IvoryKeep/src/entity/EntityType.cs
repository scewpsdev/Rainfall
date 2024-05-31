using Rainfall;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public struct MobAttack
{
	public string name;
	public string animation;
	public string followUp;
	public float damageTimeStart;
	public float damageTimeEnd;
	public float blockTimeStart;
	public float blockTimeEnd;
	public float parryTimeStart;
	public float parryTimeEnd;
	public float followUpCancelTime = 100;
	public float damageMultiplier = 1.0f;
	public float poiseDamageMultiplier = 1.0f;
	public float staminaCost;
	public int manaCost;
	public AttackProjectile[] projectiles;
	public float triggerDistanceMin = 0.5f, triggerDistanceMax = 2.0f;
	public float triggerAngleMin = -30, triggerAngleMax = 30;

	public MobAttack(int _)
	{
	}
}

public class EntityType
{
	public readonly DatFile file;

	public string name;
	public string displayName;
	public string category;

	public string entityDataPath;
	public SceneFormat.EntityData? entityData;

	// Object Data
	public Sound idleSound;

	// Creature Data
	public int health;
	public int poise;
	public Sound[] stepSound;
	public Sound[] landSound;
	public Sound[] hitSound;

	// Mob Data
	public string ai;
	public Item rightHandItem;
	public Item leftHandItem;
	public int baseDamage;
	public List<MobAttack> attacks = new List<MobAttack>();

	// Projectile Data
	public float projectileSpeed = 10;
	public float projectileGravity = -10;
	public bool projectileRemoveOnHit;
	public string projectileHitSpawnEntity;

	// Effect Data
	public int effectDamage;
	public int effectPoiseDamage;
	public float effectDamageRadius;


	public EntityType(DatFile file)
	{
		this.file = file;
	}

	public string getResourcePath(string name)
	{
		return Path.GetDirectoryName(file.path) + "/" + name;
	}

	public MobAttack? getAttack(string name)
	{
		for (int i = 0; i < attacks.Count; i++)
		{
			if (attacks[i].name == name)
				return attacks[i];
		}
		return null;
	}

	public MobAttack? getNextAttack(MobAttack attack)
	{
		if (attack.followUp != null)
			return getAttack(attack.followUp);
		return null;
	}

	Entity createInstance(string name, EntityType type)
	{
		switch (name.ToLower())
		{
			case "mob": return new Mob(type);
			case "combatdummy": return new CombatDummy(this);
			//case "effect": return new Effect(this);
			//case "projectile": return new Projectile(this);
			//case "object": return new Object(this);
			default:
				Debug.Assert(false);
				return null;
		}
	}

	public Entity create()
	{
		Entity entity = null;
		if (category != null)
		{
			entity = createInstance(category, this);
			/*
			Type? entityType = Type.GetType(category, false, true);
			if (entityType != null)
				entity = Activator.CreateInstance(entityType, this) as Entity;
			*/
		}
		if (entity == null)
		{
			entity = new Entity();
			if (entityData != null)
				EntityLoader.CreateEntityFromData(entityData.Value, entityDataPath, entity);
		}

		entity.name = name;

		return entity;
	}


	static List<EntityType> entityTypes = new List<EntityType>();
	static Dictionary<string, EntityType> nameMap = new Dictionary<string, EntityType>();
	static Dictionary<string, List<EntityType>> categoryMap = new Dictionary<string, List<EntityType>>();

	public static void LoadContent()
	{
		string itemDir = "res/entity";
		DirectoryInfo directoryInfo = new DirectoryInfo(itemDir);
		foreach (FileInfo fileInfo in directoryInfo.EnumerateFiles("", SearchOption.AllDirectories))
		{
			string relativePath = Path.GetRelativePath(".", fileInfo.FullName);
			if (relativePath.EndsWith(".dat.bin"))
			{
				EntityType entityType = EntityTypeLoader.Load(relativePath.Substring(0, relativePath.Length - 4));
				if (entityType != null)
				{
					Debug.Assert(!entityTypes.Contains(entityType));
					entityTypes.Add(entityType);
					nameMap.Add(entityType.name, entityType);
					if (!categoryMap.TryGetValue(entityType.category, out List<EntityType> categoryTypes))
					{
						categoryTypes = new List<EntityType>();
						categoryMap.Add(entityType.category, categoryTypes);
					}
					categoryTypes.Add(entityType);
					Console.WriteLine("Loaded entity type " + entityType.name);
				}
			}
		}
	}

	public static EntityType Get(string name)
	{
		if (nameMap.TryGetValue(name, out EntityType entityType))
			return entityType;
		return null;
	}

	public static EntityType GetRandom(string category, Random random)
	{
		if (categoryMap.TryGetValue(category, out List<EntityType> categoryTypes))
			return categoryTypes[random.Next() % categoryTypes.Count];
		return null;
	}
}
