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
	Armor,
	Utility,
	Consumable,
	Arrow,
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

public enum ArmorPiece
{
	None = 0,

	Head,
	Torso,
	Arms,
	Legs,
	Shoes,
}

public struct Light
{
	public Vector3 position;
	public Vector3 color;
	public bool flicker;

	public Light(Vector3 position, Vector3 color, bool flicker)
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

	public Model model;
	public Texture icon;
	public Model moveset;

	public bool hasDrawAnim = false;

	public List<Light> lights = new List<Light>();
	public List<Collider> colliders = new List<Collider>();
	public Vector3 colliderCenterOfMass = Vector3.Zero;

	public float hitboxRadius = 0.0f;
	public float hitboxRange = 0.0f;
	public Vector3 hitboxPosition = Vector3.Zero;
	public Quaternion hitboxRotation = Quaternion.Identity;

	public Texture particleTexture = null;
	public int particleAtlasColumns = 0;
	public int particleFrameSize = 0;
	public int particleFrameCount = 0;
	public float particleEmissionRate = 0.0f;
	public float particleLifetime = 1.0f;
	public Vector3 particleSpawnOffset = Vector3.Zero;
	public float particleSpawnRadius = 0.0f;
	public ParticleSpawnShape particleSpawnShape = ParticleSpawnShape.Point;
	public ParticleFollowMode particleFollowMode = ParticleFollowMode.Trail;
	public float particleSize = 0.1f;
	public Vector3 particleInitialVelocity = Vector3.Zero;
	public float particleGravity = -1.0f;
	public bool particleAdditive = false;

	public bool consumableThrowable = false;
	public float consumableUseTime = 0.0f;
	public int consumableHealAmount = 0;
	public float consumableHealDuration = 0.0f;

	public WeaponType weaponType = WeaponType.None;
	public bool twoHanded = false;
	public int baseDamage = 0;
	public List<Attack> attacks = new List<Attack>();

	public int shieldDamageAbsorption = 0;
	public int shieldHitStaminaCost = 0;

	public ArmorPiece armorPiece = ArmorPiece.None;
	public int armorDamageAbsorption = 0;

	public int spellManaCost = 0;
	public SpellProjectile[] spellProjectiles;

	public Sound sfxHit;
	public Sound sfxSwing;
	public Sound sfxTake;
	public Sound sfxDraw;
	public Sound sfxShoot;
	public Sound sfxBowDraw;


	public Item(int id, string name, string displayName, ItemCategory category, string modelPath, string iconPath, string movesetPath)
	{
		this.id = id;
		this.name = name;
		this.displayName = displayName;
		this.category = category;
		this.stackable = category == ItemCategory.Consumable || category == ItemCategory.Arrow;

		model = Resource.GetModel(modelPath);
		icon = Resource.GetTexture(iconPath);

		if (movesetPath != null)
		{
			moveset = Resource.GetModel(movesetPath);
			hasDrawAnim = moveset.getAnimationData("draw") != null;
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

	public Attack getNextAttack(Attack attack)
	{
		int index = (attack.index + 1) % getNumAttacksForType(attack.type);
		return getAttack(attack.type, index).Value;
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
					default:
						Debug.Assert(false);
						return "???";
				}
			}
			else
			{
				return category.ToString();
			}
		}
	}


	static Dictionary<int, Item> items = new Dictionary<int, Item>();
	static Dictionary<string, Item> nameMap = new Dictionary<string, Item>();


	static Vector3 ParseVector3(DatValue value)
	{
		Debug.Assert(value.type == DatValueType.Array);
		Debug.Assert(value.array.values.Count == 3);
		return new Vector3((float)value.array.values[0].number, (float)value.array.values[1].number, (float)value.array.values[2].number);
	}

	static ItemCategory ParseCategory(string identifier)
	{
		if (identifier == "weapon")
			return ItemCategory.Weapon;
		if (identifier == "shield")
			return ItemCategory.Shield;
		if (identifier == "armor")
			return ItemCategory.Armor;
		if (identifier == "utility")
			return ItemCategory.Utility;
		if (identifier == "consumable")
			return ItemCategory.Consumable;
		if (identifier == "arrow")
			return ItemCategory.Arrow;
		if (identifier == "spell")
			return ItemCategory.Spell;
		if (identifier == "artifact")
			return ItemCategory.Artifact;
		Debug.Assert(false);
		return ItemCategory.None;
	}

	static WeaponType ParseWeaponType(string identifier)
	{
		if (identifier == "melee")
			return WeaponType.Melee;
		if (identifier == "bow")
			return WeaponType.Bow;
		if (identifier == "staff")
			return WeaponType.Staff;
		Debug.Assert(false);
		return WeaponType.None;
	}

	static ArmorPiece ParseArmorType(string identifier)
	{
		if (identifier == "head")
			return ArmorPiece.Head;
		if (identifier == "torso")
			return ArmorPiece.Torso;
		if (identifier == "arms")
			return ArmorPiece.Arms;
		if (identifier == "legs")
			return ArmorPiece.Legs;
		if (identifier == "shoes")
			return ArmorPiece.Shoes;
		Debug.Assert(false);
		return ArmorPiece.None;
	}

	static ColliderType ParseColliderType(string identifier)
	{
		if (identifier == "box")
			return ColliderType.Box;
		if (identifier == "sphere")
			return ColliderType.Sphere;
		if (identifier == "capsule")
			return ColliderType.Capsule;
		Debug.Assert(false);
		return ColliderType.None;
	}

	static ParticleSpawnShape ParseParticleSpawnShape(string identifier)
	{
		if (identifier == "point")
			return ParticleSpawnShape.Point;
		if (identifier == "circle")
			return ParticleSpawnShape.Circle;
		if (identifier == "sphere")
			return ParticleSpawnShape.Sphere;
		if (identifier == "line")
			return ParticleSpawnShape.Line;
		Debug.Assert(false);
		return ParticleSpawnShape.None;
	}

	static ParticleFollowMode ParseParticleFollowMode(string identifier)
	{
		if (identifier == "trail")
			return ParticleFollowMode.Trail;
		if (identifier == "follow")
			return ParticleFollowMode.Follow;
		Debug.Assert(false);
		return ParticleFollowMode.None;
	}

	static AttackType ParseAttackType(string identifier)
	{
		if (identifier == "light")
			return AttackType.Light;
		if (identifier == "heavy")
			return AttackType.Heavy;
		if (identifier == "running")
			return AttackType.Running;
		if (identifier == "sneak")
			return AttackType.Sneak;
		if (identifier == "dodging")
			return AttackType.Dodging;
		Debug.Assert(false);
		return AttackType.None;
	}

	static SpellProjectileType ParseSpellProjectileType(string identifier)
	{
		if (identifier == "orb")
			return SpellProjectileType.Orb;
		if (identifier == "homing")
			return SpellProjectileType.Homing;
		if (identifier == "arrow")
			return SpellProjectileType.Arrow;
		Debug.Assert(false);
		return SpellProjectileType.None;
	}

	static Item ParseItem(DatFile file, string directory)
	{
		file.getInteger("id", out int id);
		file.getIdentifier("name", out string name);
		file.getStringContent("displayName", out string displayName);
		file.getIdentifier("category", out string categoryName);
		file.getStringContent("model", out string modelFile);
		file.getStringContent("icon", out string iconFile);
		file.getStringContent("moveset", out string movesetFile);

		Item item = new Item(
			id,
			name,
			displayName,
			ParseCategory(categoryName),
			directory + "/" + modelFile,
			directory + "/" + iconFile,
			movesetFile != null ? directory + "/" + movesetFile : null
		);


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

				item.lights.Add(new Light(position, color * intensity, flicker));
			}
		}

		DatField collidersNode = file.getField("colliders");
		if (collidersNode != null)
		{
			foreach (DatValue colliderNode in collidersNode.array.values)
			{
				ColliderType type = ColliderType.Box;
				Vector3 offset = new Vector3(0);
				Vector3 size = new Vector3(1.0f);
				float radius = 1.0f;

				DatField typeNode = colliderNode.obj.getField("type");
				DatField offsetNode = colliderNode.obj.getField("offset");
				DatField sizeNode = colliderNode.obj.getField("size");
				DatField radiusNode = colliderNode.obj.getField("radius");
				DatField heightNode = colliderNode.obj.getField("height");

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
					item.colliders.Add(new Collider(offset, size));
				else if (type == ColliderType.Sphere)
					item.colliders.Add(new Collider(offset, radius));
				else if (type == ColliderType.Capsule)
					item.colliders.Add(new Collider(offset, radius, size.y));
				else
				{
					Debug.Assert(false);
				}
			}
		}

		DatField colliderCenterOfMassNode = file.getField("colliderCenterOfMass");
		if (colliderCenterOfMassNode != null)
			item.colliderCenterOfMass = ParseVector3(colliderCenterOfMassNode.value);


		if (file.getStringContent("sfxTake", out string sfxTakePath))
			item.sfxTake = Resource.GetSound(directory + "/sfx/" + sfxTakePath);
		else
			item.sfxTake = Resource.GetSound("res/entity/player/sfx/take.ogg");

		if (file.getStringContent("sfxDraw", out string sfxDrawPath))
			item.sfxDraw = Resource.GetSound(directory + "/sfx/" + sfxDrawPath);
		else
			item.sfxDraw = Resource.GetSound("res/entity/player/sfx/draw.ogg");

		if (file.getStringContent("sfxShoot", out string sfxShootPath))
			item.sfxShoot = Resource.GetSound(directory + "/sfx/" + sfxShootPath);

		if (file.getStringContent("sfxBowDraw", out string sfxBowDrawPath))
			item.sfxBowDraw = Resource.GetSound(directory + "/sfx/" + sfxBowDrawPath);


		// Particles
		{
			DatField particleTextureNode = file.getField("particleTexture");
			DatField particleAtlasColumnsNode = file.getField("particleAtlasColumns");
			DatField particleFrameSizeNode = file.getField("particleFrameSize");
			DatField particleFrameCountNode = file.getField("particleFrameCount");
			DatField particleEmissionRateNode = file.getField("particleEmissionRate");
			DatField particleLifetimeNode = file.getField("particleLifetime");
			DatField particleSpawnOffsetNode = file.getField("particleSpawnOffset");
			DatField particleSpawnRadiusNode = file.getField("particleSpawnRadius");
			DatField particleSpawnShapeNode = file.getField("particleSpawnShape");
			DatField particleFollowModeNode = file.getField("particleFollowMode");
			DatField particleSizeNode = file.getField("particleSize");
			DatField particleInitialVelocityNode = file.getField("particleInitialVelocity");
			DatField particleGravityNode = file.getField("particleGravity");
			DatField particleAdditiveNode = file.getField("particleAdditive");

			if (particleTextureNode != null)
				item.particleTexture = Resource.GetTexture(particleTextureNode.stringContent);
			if (particleAtlasColumnsNode != null)
				item.particleAtlasColumns = particleAtlasColumnsNode.integer;
			if (particleFrameSizeNode != null)
				item.particleFrameSize = particleFrameSizeNode.integer;
			if (particleFrameCountNode != null)
				item.particleFrameCount = particleFrameCountNode.integer;
			if (particleEmissionRateNode != null)
				item.particleEmissionRate = (float)particleEmissionRateNode.number;
			if (particleLifetimeNode != null)
				item.particleLifetime = (float)particleLifetimeNode.number;
			if (particleSpawnOffsetNode != null)
				item.particleSpawnOffset = ParseVector3(particleSpawnOffsetNode.value);
			if (particleSpawnRadiusNode != null)
				item.particleSpawnRadius = (float)particleSpawnRadiusNode.number;
			if (particleSpawnShapeNode != null)
				item.particleSpawnShape = ParseParticleSpawnShape(particleSpawnShapeNode.identifier);
			if (particleFollowModeNode != null)
				item.particleFollowMode = ParseParticleFollowMode(particleFollowModeNode.identifier);
			if (particleSizeNode != null)
				item.particleSize = (float)particleSizeNode.number;
			if (particleInitialVelocityNode != null)
				item.particleInitialVelocity = ParseVector3(particleInitialVelocityNode.value);
			if (particleGravityNode != null)
				item.particleGravity = (float)particleGravityNode.number;
			if (particleAdditiveNode != null)
				item.particleAdditive = particleAdditiveNode.integer != 0;
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
					attack.followUpCancelTime = followUpCancelFrameNode != null ? followUpCancelFrameNode.integer / 24.0f : 0.0f;
					attack.damageMultiplier = damageMultiplierNode != null ? (float)damageMultiplierNode.number : 1.0f;
					attack.staminaCost = staminaCostNode != null ? (float)staminaCostNode.number : 0.0f;

					item.attacks.Add(attack);
				}
			}
		}

		// Shields
		{
			file.getInteger("shieldDamageAbsorption", out item.shieldDamageAbsorption);
			file.getInteger("shieldHitStaminaCost", out item.shieldHitStaminaCost);
		}

		// Armor
		{
			if (file.getIdentifier("armorType", out string armorTypeName))
				item.armorPiece = ParseArmorType(armorTypeName);
			DatField baseAbsorptionNode = file.getField("baseAbsorption");
		}

		// Consumables
		{
			if (file.getInteger("consumableUseTime", out int consumableHealFrame))
				item.consumableUseTime = consumableHealFrame / 24.0f;
			file.getBoolean("consumableThrowable", out item.consumableThrowable);

			file.getInteger("consumableHealAmount", out item.consumableHealAmount);
			file.getNumber("consumableHealDuration", out item.consumableHealDuration);
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


		return item;
	}

	static void Load(string location, string name)
	{
		string directory = "res/item/" + location + "/" + name;
		string datPath = directory + "/" + name + ".dat";
		DatFile dat = new DatFile(datPath);
		Item item = ParseItem(dat, directory);
		Debug.Assert(!items.ContainsKey(item.id) && !nameMap.ContainsKey(item.name));
		items.Add(item.id, item);
		nameMap.Add(item.name, item);
	}

	public static void LoadContent()
	{
		//Load("weapon", "zweihander");
		Load("weapon", "longsword");
		Load("weapon", "shortsword");
		//Load("weapon", "dagger");

		//Load("weapon", "longbow");

		//Load("weapon", "oak_staff");

		Load("shield", "wooden_round_shield");

		Load("armor", "soldier_helmet");

		//Load("utility", "torch");

		//Load("consumable", "quemick");
		//Load("consumable", "firebomb");

		//Load("ammo", "arrow");

		//Load("spell", "magic_arrow");
		//Load("spell", "homing_orbs");
		//Load("spell", "magic_orb");
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
}
