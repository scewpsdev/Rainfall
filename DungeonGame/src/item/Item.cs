using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum ItemCategory
{
	None = 0,

	Weapon,
	Shield,
	Utility,
	Consumable,
	Armor,
	Arrow,
	Collectible,
	Spell,
	Artifact,
}

public enum WeaponType
{
	None = 0,

	Melee,
	Bow,
	Staff,
}

public enum ArmorType
{
	None = 0,

	Helmet,
	Torso,
	Legs,
	Gloves,
	Shoes,
}

public struct ItemLight
{
	public Vector3 position;
	public Vector3 color;
	public bool flicker;

	public ItemLight(Vector3 position, Vector3 color, bool flicker)
	{
		this.position = position;
		this.color = color;
		this.flicker = flicker;
	}
}

public enum ColliderType
{
	None = 0,

	Box,
	Sphere,
	Capsule,
}

public struct Collider
{
	public ColliderType type;
	public Vector3 offset;
	public Vector3 size;
	public float radius;

	public Collider(Vector3 offset, Vector3 size)
	{
		this.type = ColliderType.Box;
		this.offset = offset;
		this.size = size;
		this.radius = 0.0f;
	}

	public Collider(Vector3 offset, float radius)
	{
		this.type = ColliderType.Sphere;
		this.offset = offset;
		this.size = new Vector3(0);
		this.radius = radius;
	}

	public Collider(Vector3 offset, float radius, float height)
	{
		this.type = ColliderType.Capsule;
		this.offset = offset;
		this.size = new Vector3(0.0f, height, 0.0f);
		this.radius = radius;
	}
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

	BowShoot,
}

public struct Attack
{
	public AttackType type;
	public int index;
	public string animationName;
	public float damageTimeStart;
	public float damageTimeEnd;
	public float followUpCancelTime;
	public float damageMultiplier;
	public float staminaCost;
}

public enum SpellProjectileType
{
	None = 0,

	Arrow,
	Orb,
	Homing,
}

public struct SpellProjectile
{
	public SpellProjectileType type;
	public float castTime;
	public Vector3 offset;
}

public class Item
{
	public int id;
	public string name;
	public string displayName;
	public ItemCategory category;
	public bool stackable;
	public Vector2i inventorySize = new Vector2i(1);
	public string typeSpecifier { get; private set; }

	public Model model;
	public Texture icon;
	public Model moveset;
	public float pitchFactor = 1.0f;

	public List<ItemLight> lights = new List<ItemLight>();
	public List<Collider> colliders = new List<Collider>();
	public Vector3 colliderCenterOfMass = Vector3.Zero;

	public bool flipOnLeft = false;
	public Quaternion pickupTransform = Quaternion.Identity;

	public Collider hitbox;

	//public float hitboxRadius = 0.0f;
	//public float hitboxRange = 0.0f;
	//public Vector3 hitboxPosition = Vector3.Zero;
	//public Quaternion hitboxRotation = Quaternion.Identity;

	public ParticleSystem particles = null;

	public bool consumableThrowable = false;
	public float consumableUseTime = 0.0f;
	public int consumableHealAmount = 0;
	public float consumableHealDuration = 0.0f;
	public int consumableManaRechargeAmount = 0;
	public float consumableManaRechargeDuration = 0.0f;
	public float consumableFollowUpTime = 1000.0f;
	public float consumableFollowUpStart = 0.0f;

	public WeaponType weaponType = WeaponType.None;
	public bool twoHanded = false;
	public int baseDamage = 0;
	public List<Attack> attacks = new List<Attack>();

	public ArmorType armorType = ArmorType.None;

	public int shieldHitStaminaCost = 0;
	public float blockRaiseDuration = 0.0f;
	public int blockDamageAbsorption = 0;
	public int parryFramesDelay = 0;
	public int parryFramesCount = 0;

	public int spellManaCost = 0;
	public SpellProjectile[] spellProjectiles;

	public Sound sfxUse;
	public Sound sfxHit;
	public Sound sfxSwing;
	public Sound sfxSwingHeavy;
	public Sound sfxBlock;
	public Sound sfxParry;
	public Sound sfxGuardBreak;
	public Sound sfxTake;
	public Sound sfxDraw;
	public Sound sfxDrop;
	public Sound sfxShoot;
	public Sound sfxBowDraw;


	public Item(int id, string name, string displayName, ItemCategory category)
	{
		this.id = id;
		this.name = name;
		this.displayName = displayName;
		this.category = category;
		this.stackable = category == ItemCategory.Consumable || category == ItemCategory.Arrow || category == ItemCategory.Collectible;
	}

	public float hitboxRange
	{
		get
		{
			switch (hitbox.type)
			{
				case ColliderType.Box:
					return hitbox.size.y * 0.5f + hitbox.offset.y;
				case ColliderType.Sphere:
					return hitbox.radius + hitbox.offset.y;
				case ColliderType.Capsule:
					return hitbox.size.y * 0.5f + hitbox.offset.y;
				default:
					return 0.0f;
			}
		}
	}

	public Attack? getAttack(AttackType type, int index)
	{
		int idx = 0;
		for (int i = 0; i < attacks.Count; i++)
		{
			if (attacks[i].type == type)
			{
				if (idx == index)
					return attacks[i];
				else
					idx++;
			}
		}
		return null;
	}

	public int getNumAttacksForType(AttackType type)
	{
		int result = 0;
		for (int i = 0; i < attacks.Count; i++)
		{
			if (attacks[i].type == type)
				result++;
		}
		return result;
	}

	public Attack getNextAttack(Attack attack, AttackType type)
	{
		int index = (attack.index + 1) % getNumAttacksForType(type);
		return getAttack(type, index).Value;
	}

	public bool hasPrimaryAction
	{
		get => category == ItemCategory.Weapon || category == ItemCategory.Shield || category == ItemCategory.Consumable;
	}

	public bool hasSecondaryAction
	{
		get => category == ItemCategory.Weapon;
	}


	static Dictionary<int, Item> items = new Dictionary<int, Item>();
	static Dictionary<string, Item> nameMap = new Dictionary<string, Item>();
	static Dictionary<ItemCategory, List<Item>> categoryMap = new Dictionary<ItemCategory, List<Item>>();


	static Vector3 ParseVector3(DatValue value)
	{
		Debug.Assert(value.type == DatValueType.Array);
		Debug.Assert(value.array.values.Count == 3);
		return new Vector3((float)value.array.values[0].number, (float)value.array.values[1].number, (float)value.array.values[2].number);
	}

	static ColliderType ParseColliderType(string identifier)
	{
		foreach (ColliderType type in Enum.GetValues<ColliderType>())
		{
			if (type.ToString().ToLower() == identifier)
				return type;
		}
		Debug.Assert(false);
		return ColliderType.None;
	}

	static bool ParseCollider(DatObject colliderNode, out Collider collider)
	{
		ColliderType type = ColliderType.Box;
		Vector3 offset = new Vector3(0);
		Vector3 size = new Vector3(1.0f);
		float radius = 1.0f;

		DatField typeNode = colliderNode.getField("type");
		DatField offsetNode = colliderNode.getField("offset");
		DatField sizeNode = colliderNode.getField("size");
		DatField radiusNode = colliderNode.getField("radius");
		DatField heightNode = colliderNode.getField("height");

		if (typeNode != null)
			type = ParseColliderType(typeNode.identifier);
		if (offsetNode != null)
			offset = ParseVector3(offsetNode.value);
		if (sizeNode != null)
			size = ParseVector3(sizeNode.value);
		if (radiusNode != null)
			radius = (float)radiusNode.number;
		if (heightNode != null)
			size = new Vector3(0.0f, (float)heightNode.number, 0.0f);

		if (type == ColliderType.Box)
			collider = new Collider(offset, size);
		else if (type == ColliderType.Sphere)
			collider = new Collider(offset, radius);
		else if (type == ColliderType.Capsule)
			collider = new Collider(offset, radius, size.y);
		else
		{
			collider = new Collider();
			return false;
		}
		return true;
	}

	static ItemCategory ParseCategory(string identifier)
	{
		foreach (ItemCategory category in Enum.GetValues<ItemCategory>())
		{
			if (category.ToString().ToLower() == identifier)
				return category;
		}
		Debug.Assert(false);
		return ItemCategory.None;
	}

	static WeaponType ParseWeaponType(string identifier)
	{
		foreach (WeaponType type in Enum.GetValues<WeaponType>())
		{
			if (type.ToString().ToLower() == identifier)
				return type;
		}
		Debug.Assert(false);
		return WeaponType.None;
	}

	static ArmorType ParseArmorType(string identifier)
	{
		foreach (ArmorType type in Enum.GetValues<ArmorType>())
		{
			if (type.ToString().ToLower() == identifier)
				return type;
		}
		Debug.Assert(false);
		return ArmorType.None;
	}

	static ParticleSpawnShape ParseParticleSpawnShape(string identifier)
	{
		foreach (ParticleSpawnShape shape in Enum.GetValues<ParticleSpawnShape>())
		{
			if (shape.ToString().ToLower() == identifier)
				return shape;
		}
		Debug.Assert(false);
		return ParticleSpawnShape.None;
	}

	static bool ParseParticleFollow(string identifier)
	{
		if (identifier == "follow")
			return true;
		else if (identifier == "trail")
			return false;
		Debug.Assert(false);
		return false;
	}

	static AttackType ParseAttackType(string identifier)
	{
		foreach (AttackType type in Enum.GetValues<AttackType>())
		{
			if (type.ToString().ToLower() == identifier)
				return type;
		}
		Debug.Assert(false);
		return AttackType.None;
	}

	static SpellProjectileType ParseSpellProjectileType(string identifier)
	{
		foreach (SpellProjectileType type in Enum.GetValues<SpellProjectileType>())
		{
			if (type.ToString().ToLower() == identifier)
				return type;
		}
		Debug.Assert(false);
		return SpellProjectileType.None;
	}

	static Item ParseItem(DatFile file, string directory)
	{
		file.getInteger("id", out int id);
		file.getIdentifier("name", out string name);
		file.getStringContent("displayName", out string displayName);
		file.getIdentifier("category", out string categoryName);

		Item item = new Item(
			id,
			name,
			displayName,
			ParseCategory(categoryName)
		);

		if (file.getStringContent("model", out string modelFile))
		{
			item.model = Resource.GetModel(directory + "/" + modelFile);
			item.model.maxDistance = (LOD.DISTANCE_SMALL);
			item.model.isStatic = false;
		}
		if (file.getStringContent("moveset", out string movesetFile))
			item.moveset = Resource.GetModel(directory + "/" + movesetFile);
		if (file.getStringContent("icon", out string iconFile))
			item.icon = Resource.GetTexture(directory + "/" + iconFile);

		if (file.getNumber("pitchFactor", out float pitchFactor))
			item.pitchFactor = pitchFactor;


		if (file.getVector2("inventorySize", out Vector2 inventorySize))
			item.inventorySize = (Vector2i)inventorySize;
		else
			item.inventorySize =
				item.category == ItemCategory.Weapon ? new Vector2i(1, 3) :
				item.category == ItemCategory.Shield ? new Vector2i(2, 3) :
				item.category == ItemCategory.Armor ? new Vector2i(2, 2) :
				new Vector2i(1, 1);

		DatField lights = file.getField("lights");
		if (lights != null)
		{
			foreach (DatValue light in lights.array.values)
			{
				Vector3 position = new Vector3(0);
				Vector3 color = new Vector3(1);
				float intensity = 1.0f;
				bool flicker = false;

				DatField positionNode = light.obj.getField("position");
				DatField colorNode = light.obj.getField("color");
				DatField intensityNode = light.obj.getField("intensity");
				DatField flickerNode = light.obj.getField("flicker");

				if (positionNode != null)
				{
					position = new Vector3(
						(float)positionNode.array.values[0].number,
						(float)positionNode.array.values[1].number,
						(float)positionNode.array.values[2].number
					);
				}
				if (colorNode != null)
				{
					color = new Vector3(
						(float)colorNode.array.values[0].number,
						(float)colorNode.array.values[1].number,
						(float)colorNode.array.values[2].number
					);
				}
				if (intensityNode != null)
					intensity = (float)intensityNode.value.number;
				if (flickerNode != null)
					flicker = flickerNode.value.identifier == "true";

				item.lights.Add(new ItemLight(position, color * intensity, flicker));
			}
		}

		if (file.getObject("colliders", out DatArray colliders))
		{
			foreach (DatValue colliderNode in colliders.values)
			{
				if (ParseCollider(colliderNode.obj, out Collider collider))
					item.colliders.Add(collider);
			}
		}

		DatField colliderCenterOfMassNode = file.getField("colliderCenterOfMass");
		if (colliderCenterOfMassNode != null)
			item.colliderCenterOfMass = ParseVector3(colliderCenterOfMassNode.value);


		if (file.getInteger("flipOnLeft", out int flipOnLeft))
			item.flipOnLeft = flipOnLeft != 0;
		if (file.getVector4("pickupTransform", out Vector4 pickupTransform))
			item.pickupTransform = Quaternion.FromAxisAngle(pickupTransform.xyz, MathHelper.ToRadians(pickupTransform.w));


		if (file.getObject("hitbox", out DatObject hitboxNode))
			if (ParseCollider(hitboxNode, out Collider hitbox))
				item.hitbox = hitbox;


		if (file.getStringContent("sfxTake", out string sfxTakePath))
			item.sfxTake = Resource.GetSound(directory + "/sfx/" + sfxTakePath);
		else
			item.sfxTake = Resource.GetSound("res/entity/player/sfx/take.ogg");

		if (file.getStringContent("sfxUse", out string sfxUsePath))
			item.sfxUse = Resource.GetSound(directory + "/sfx/" + sfxUsePath);

		if (file.getStringContent("sfxDraw", out string sfxDrawPath))
			item.sfxDraw = Resource.GetSound(directory + "/sfx/" + sfxDrawPath);
		else
			item.sfxDraw = Resource.GetSound("res/entity/player/sfx/draw.ogg");

		if (file.getStringContent("sfxDrop", out string sfxDropPath))
			item.sfxDrop = Resource.GetSound(directory + "/sfx/" + sfxDropPath);

		if (file.getStringContent("sfxShoot", out string sfxShootPath))
			item.sfxShoot = Resource.GetSound(directory + "/sfx/" + sfxShootPath);

		if (file.getStringContent("sfxBowDraw", out string sfxBowDrawPath))
			item.sfxBowDraw = Resource.GetSound(directory + "/sfx/" + sfxBowDrawPath);


		// Particles
		if (file.getField("particleEmissionRate") != null)
		{
			item.particles = new ParticleSystem(0);

			if (file.getNumber("particleEmissionRate", out float emissionRate))
				item.particles.emissionRate = emissionRate;
			if (file.getNumber("particleLifetime", out float lifetime))
				item.particles.lifetime = lifetime;
			if (file.getField("particleSize", out DatField size))
			{
				if (size.value.type == DatValueType.Number)
					file.getNumber("particleSize", out item.particles.particleSize);
				else if (size.value.type == DatValueType.Array)
				{
					file.getObject("particleSize", out DatArray arr);
					Debug.Assert(arr.values.Count > 0);
					Debug.Assert(arr.values[0].type == DatValueType.Number);
					float value0 = (float)arr.values[0].number;
					item.particles.particleSizeAnim = new Gradient<float>(value0);
					for (int i = 1; i < arr.values.Count; i++)
					{
						Debug.Assert(arr.values[i].type == DatValueType.Number);
						float value = (float)arr.values[i].number;
						item.particles.particleSizeAnim.setValue(i / (float)(arr.values.Count - 1), value);
					}
				}
				else
				{
					Debug.Assert(false);
				}
			}
			if (file.getVector3("particleSpawnOffset", out Vector3 spawnOffset))
				item.particles.spawnOffset = spawnOffset;
			if (file.getNumber("particleSpawnRadius", out float spawnRadius))
				item.particles.spawnRadius = spawnRadius;
			if (file.getIdentifier("particleSpawnShape", out string spawnShape))
				item.particles.spawnShape = ParseParticleSpawnShape(spawnShape);
			if (file.getIdentifier("particleFollowMode", out string followMode))
				item.particles.follow = ParseParticleFollow(followMode);
			if (file.getNumber("particleGravity", out float gravity))
				item.particles.gravity = gravity;
			if (file.getVector3("particleInitialVelocity", out Vector3 initialVelocity))
				item.particles.initialVelocity = initialVelocity;

			if (file.getStringContent("particleTexture", out string particleTextureStr))
				item.particles.textureAtlas = Resource.GetTexture(particleTextureStr);
			if (file.getInteger("particleFrameSize", out int particleFrameSize))
			{
				item.particles.frameWidth = particleFrameSize;
				item.particles.frameHeight = particleFrameSize;
			}
			if (file.getInteger("particleFrameCount", out int numFrames))
				item.particles.numFrames = numFrames;

			if (file.getVector3("particleTint", out Vector3 tint))
				item.particles.spriteTint = new Vector4(tint, 1.0f);
			if (file.getInteger("particleLinearFiltering", out int linearFiltering))
				item.particles.linearFiltering = linearFiltering != 0;
			if (file.getInteger("particleAdditive", out int additive))
				item.particles.additive = additive != 0;
		}


		// Weapons
		{
			if (file.getIdentifier("weaponType", out string weaponTypeName))
				item.weaponType = ParseWeaponType(weaponTypeName);
			DatField twoHandedNode = file.getField("twoHanded");
			DatField baseDamageNode = file.getField("baseDamage");
			DatField attacksNode = file.getField("attacks");

			if (file.getStringContent("sfxHit", out string sfxHitPath))
				item.sfxHit = Resource.GetSound(directory + "/sfx/" + sfxHitPath);

			if (file.getStringContent("sfxSwing", out string sfxSwingPath))
				item.sfxSwing = Resource.GetSound(directory + "/sfx/" + sfxSwingPath);
			if (file.getStringContent("sfxSwingHeavy", out string sfxSwingHeavyPath))
				item.sfxSwingHeavy = Resource.GetSound(directory + "/sfx/" + sfxSwingHeavyPath);

			if (twoHandedNode != null)
				item.twoHanded = twoHandedNode.integer != 0;

			if (baseDamageNode != null)
				item.baseDamage = baseDamageNode.integer;

			if (attacksNode != null)
			{
				DatArray attacksArray = attacksNode.array;
				for (int i = 0; i < attacksArray.values.Count; i++)
				{
					DatObject attackNode = attacksArray.values[i].obj;

					DatField typeNode = attackNode.getField("type");
					DatField indexNode = attackNode.getField("index");
					DatField animationNameNode = attackNode.getField("animName");
					DatField damageFramesStartNode = attackNode.getField("damageFramesStart");
					DatField damageFramesEndNode = attackNode.getField("damageFramesEnd");
					DatField followUpCancelFrameNode = attackNode.getField("followUpCancelFrame");
					DatField damageMultiplierNode = attackNode.getField("damageMultiplier");
					DatField staminaCostNode = attackNode.getField("staminaCost");

					Attack attack = new Attack();
					attack.type = ParseAttackType(typeNode.identifier);
					attack.index = indexNode != null ? indexNode.integer : 0;
					attack.animationName = animationNameNode.identifier;
					attack.damageTimeStart = damageFramesStartNode.integer / 24.0f;
					attack.damageTimeEnd = damageFramesEndNode.integer / 24.0f;
					attack.followUpCancelTime = followUpCancelFrameNode != null ? followUpCancelFrameNode.integer / 24.0f : 1000.0f;
					attack.damageMultiplier = damageMultiplierNode != null ? (float)damageMultiplierNode.number : 1.0f;
					attack.staminaCost = staminaCostNode != null ? (float)staminaCostNode.number : 0.0f;

					item.attacks.Add(attack);
				}
			}
		}

		// Shields
		{
			file.getInteger("shieldHitStaminaCost", out item.shieldHitStaminaCost);

			file.getNumber("blockRaiseDuration", out item.blockRaiseDuration);

			if (file.getStringContent("sfxBlock", out string sfxBlockPath))
				item.sfxBlock = Resource.GetSound(directory + "/sfx/" + sfxBlockPath);

			if (file.getStringContent("sfxParry", out string sfxParryPath))
				item.sfxParry = Resource.GetSound(directory + "/sfx/" + sfxParryPath);

			if (file.getStringContent("sfxGuardBreak", out string sfxGuardBreakPath))
				item.sfxGuardBreak = Resource.GetSound(directory + "/sfx/" + sfxGuardBreakPath);
		}

		// Armor
		{
			if (file.getIdentifier("armorType", out string armorTypeStr))
				item.armorType = ParseArmorType(armorTypeStr);
		}

		// Weapons and shields
		{
			file.getInteger("parryFramesDelay", out item.parryFramesDelay);
			file.getInteger("parryFramesCount", out item.parryFramesCount);
		}

		// Weapons, shields and armor
		{
			file.getInteger("damageAbsorption", out item.blockDamageAbsorption);
		}

		// Consumables
		{
			if (file.getInteger("consumableUseTime", out int consumableHealFrame))
				item.consumableUseTime = consumableHealFrame / 24.0f;
			file.getBoolean("consumableThrowable", out item.consumableThrowable);

			file.getInteger("consumableHealAmount", out item.consumableHealAmount);
			file.getNumber("consumableHealDuration", out item.consumableHealDuration);

			file.getInteger("consumableManaRechargeAmount", out item.consumableManaRechargeAmount);
			file.getNumber("consumableManaRechargeDuration", out item.consumableManaRechargeDuration);

			if (file.getNumber("consumableFollowUpTime", out float consumableFollowUpTime))
				item.consumableFollowUpTime = consumableFollowUpTime;
			if (file.getNumber("consumableFollowUpStart", out float consumableFollowUpStart))
				item.consumableFollowUpStart = consumableFollowUpStart;
		}

		// Spells
		{
			file.getInteger("spellManaCost", out item.spellManaCost);

			DatField projectiles = file.getField("spellProjectiles");
			if (projectiles != null)
			{
				Debug.Assert(projectiles.value.type == DatValueType.Array);

				SpellProjectile[] spellProjectiles = new SpellProjectile[projectiles.array.values.Count];

				for (int i = 0; i < projectiles.array.values.Count; i++)
				{
					DatValue projectile = projectiles.array.values[i];
					Debug.Assert(projectile.type == DatValueType.Object);

					projectile.obj.getNumber("castTime", out spellProjectiles[i].castTime);
					projectile.obj.getIdentifier("type", out string projectileType);
					spellProjectiles[i].type = ParseSpellProjectileType(projectileType);
				}

				item.spellProjectiles = spellProjectiles;
			}
		}


		if (item.category == ItemCategory.Weapon)
		{
			switch (item.weaponType)
			{
				case WeaponType.Melee:
					item.typeSpecifier = "Melee Weapon";
					break;
				case WeaponType.Bow:
					item.typeSpecifier = "Bow";
					break;
				case WeaponType.Staff:
					item.typeSpecifier = "Staff";
					break;
				default:
					Debug.Assert(false);
					item.typeSpecifier = "???";
					break;
			}
		}
		else
		{
			item.typeSpecifier = item.category.ToString();
		}


		return item;
	}

	static void Load(string location, string name)
	{
		string directory = "res/item/" + location + "/" + name;
		string datPath = directory + "/" + name + ".dat";
		string datStr = Resource.ReadText(datPath);
		DatFile dat = new DatFile(datStr, datPath);
		Item item = ParseItem(dat, directory);
		Debug.Assert(!items.ContainsKey(item.id) && !nameMap.ContainsKey(item.name));
		items.Add(item.id, item);
		nameMap.Add(item.name, item);

		if (!categoryMap.ContainsKey(item.category))
			categoryMap.Add(item.category, new List<Item>());
		categoryMap[item.category].Add(item);
	}

	public static void LoadContent()
	{
		Load("weapon", "default");

		Load("weapon", "zweihander");
		Load("weapon", "longsword");
		Load("weapon", "shortsword");
		//Load("weapon", "dagger");
		Load("weapon", "axe");
		Load("weapon", "broken_sword");

		Load("weapon", "longbow");

		Load("weapon", "oak_staff");

		Load("shield", "wooden_round_shield");

		Load("utility", "torch");

		Load("consumable", "flask");
		Load("consumable", "mana_flask");
		Load("consumable", "firebomb");

		Load("armor", "leather_chestplate");

		Load("collectible", "arrow");
		Load("collectible", "gold");

		Load("spell", "magic_arrow");
		Load("spell", "homing_orbs");
		Load("spell", "magic_orb");

		Load("artifact", "key_cell");
		Load("artifact", "map");
	}

	public static Item Get(int id)
	{
		if (items.ContainsKey(id))
			return items[id];
		return null;
	}

	public static Item Get(string name)
	{
		if (nameMap.ContainsKey(name))
			return nameMap[name];
		return null;
	}

	public static Item GetItemByCategory(ItemCategory category, Random random)
	{
		if (categoryMap.ContainsKey(category))
		{
			List<Item> items = categoryMap[category];
			return items[random.Next() % items.Count];
		}
		return null;
	}
}
