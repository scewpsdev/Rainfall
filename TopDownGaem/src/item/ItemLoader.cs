using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


public static class ItemLoader
{
	static AnimationLayer CreateItemAnimationLayer(Item item, string animation, bool looping)
	{
		bool[] mask = new bool[item.moveset.skeleton.nodes.Length];
		for (int i = 0; i < mask.Length; i++)
		{
			Node node = item.moveset.skeleton.nodes[i];
			bool isRight = node.name.EndsWith("_r");
			bool isArmNode =
				node.name.StartsWith("hand") ||
				node.name.StartsWith("thumb") ||
				node.name.StartsWith("index") ||
				node.name.StartsWith("middle") ||
				node.name.StartsWith("ring") ||
				node.name.StartsWith("pinky") ||
				node.name.StartsWith("weapon") ||
				node.name.StartsWith("lowerarm") ||
				node.name.StartsWith("upperarm") ||
				node.name.StartsWith("clavicle")
				;
			mask[i] = isRight && isArmNode;
		}
		return new AnimationLayer(item.moveset, animation, looping, mask);
	}

	public static Item Load(string path)
	{
		DatFile file = new DatFile(Resource.ReadText(path), path);
		string directory = Path.GetDirectoryName(path);

		Item item = new Item();

		file.getStringContent("name", out item.name);
		file.getStringContent("displayName", out item.displayName);
		file.getStringContent("description", out item.description);
		if (file.getIdentifier("category", out string category))
			item.category = Utils.ParseEnum<ItemCategory>(category);

		if (file.getStringContent("entity", out string entityFile))
		{
			string entityPath = directory + "/" + entityFile + ".bin";
			item.entityPath = entityPath;
			if (File.Exists(entityPath))
			{
				FileStream stream = new FileStream(entityPath, FileMode.Open);
				SceneFormat.DeserializeScene(stream, out List<SceneFormat.EntityData> entities, out _);
				Debug.Assert(entities.Count > 0);
				SceneFormat.EntityData entityData = entities[0];
				entityData.load(Path.GetDirectoryName(entityPath));
				item.entity = entityData;
				stream.Close();
			}
		}
		if (file.getStringContent("moveset", out string movesetFile))
		{
			item.moveset = Resource.GetModel(directory + "/" + movesetFile);
			item.idleAnim = CreateItemAnimationLayer(item, "idle", true);
			item.runAnim = CreateItemAnimationLayer(item, "run", true);
			item.jumpAnim = CreateItemAnimationLayer(item, "jump", false);
			item.fallAnim = CreateItemAnimationLayer(item, "fall", false);
			item.duckAnim = CreateItemAnimationLayer(item, "ducked", true);
			item.duckWalkAnim = CreateItemAnimationLayer(item, "ducked_walk", true);
		}
		if (file.getStringContent("icon", out string iconFile))
			item.icon = Resource.GetTexture(directory + "/" + iconFile);
		if (file.getBoolean("isLoot", out bool isLoot))
			item.isLoot = isLoot;
		else
			item.isLoot = true;

		if (file.getNumber("baseDamage", out float baseDamage))
			item.baseDamage = baseDamage;
		if (file.getNumber("criticalDamage", out float criticalModifier))
			item.criticalModifier = criticalModifier;
		if (file.getNumber("poiseDamage", out float poiseDamage))
			item.poiseDamage = poiseDamage;
		if (file.getNumber("baseAbsorption", out float baseAbsorption))
			item.baseAbsorption = baseAbsorption;
		if (file.getNumber("blockStability", out float blockStability))
			item.blockStability = blockStability;
		file.getBoolean("twoHanded", out item.twoHanded);

		if (file.getStringContent("equipSound", out string equipSoundPath))
			item.equipSound = Resource.GetSound(directory + "/" + equipSoundPath);
		if (file.getStringContent("dropSound", out string dropSoundPath))
			item.dropSound = Resource.GetSound(directory + "/" + dropSoundPath);

		if (item.category == ItemCategory.Weapon)
		{
			if (file.getIdentifier("weaponType", out string weaponType))
				item.weaponType = Utils.ParseEnum<WeaponType>(weaponType);

			file.getVector3("castOrigin", out item.castOrigin);

			if (file.getStringContent("blockSound", out string blockSoundPath))
				item.blockSound = Resource.GetSound(directory + "/" + blockSoundPath);
			if (file.getStringContent("parrySound", out string parrySoundPath))
				item.parrySound = Resource.GetSound(directory + "/" + parrySoundPath);
			else
				item.parrySound = Resource.GetSound("res/item/sfx/parry.ogg");
			if (file.getStringContent("bowDrawSound", out string bowDrawSoundPath))
			{
				item.bowDrawSound = Resource.GetSound(directory + "/" + bowDrawSoundPath);
				if (file.getInteger("bowDrawSoundFrame", out int bowDrawSoundFrame))
					item.bowDrawSoundTime = bowDrawSoundFrame / 24.0f;
			}
			if (file.getStringContent("bowShootSound", out string bowShootSoundPath))
				item.bowShootSound = Resource.GetSound(directory + "/" + bowShootSoundPath);
		}
		else if (item.category == ItemCategory.Armor)
		{
			if (file.getIdentifier("armorType", out string armorType))
				item.armorType = Utils.ParseEnum<ArmorType>(armorType);
		}
		else if (item.category == ItemCategory.Consumable)
		{
			if (file.getIdentifier("consumableType", out string consumableType))
				item.consumableType = Utils.ParseEnum<ConsumableType>(consumableType);
			if (file.getIdentifier("consumableEffectStat", out string consumableEffectStat))
				item.consumableEffectStat = Utils.ParseEnum<ConsumableEffectStat>(consumableEffectStat);
			file.getNumber("consumableEffectAmount", out item.consumableEffectAmount);
			if (file.getInteger("consumableEffectFrame", out int consumableEffectFrame))
				item.consumableEffectTime = consumableEffectFrame / 24.0f;
			file.getNumber("consumableEffectDuration", out item.consumableEffectDuration);
			file.getStringContent("consumableThrowEntity", out item.consumableThrowEntity);
			file.getBoolean("consumableUseTwoHanded", out item.consumableUseTwoHanded);

			if (file.getArray("useSounds", out DatArray useSounds))
			{
				item.useSounds = new List<ActionSfx>();
				for (int i = 0; i < useSounds.size; i++)
				{
					DatObject sfx = useSounds[i].obj;
					sfx.getStringContent("sound", out string soundPath);
					if (!sfx.getNumber("gain", out float gain))
						gain = 1.0f;
					sfx.getNumber("time", out float time);
					sfx.getBoolean("organic", out bool organic);
					item.useSounds.Add(new ActionSfx(Resource.GetSound(directory + "/" + soundPath), gain, time, organic));
				}
			}
		}

		if (file.getArray("attacks", out DatArray attacks))
		{
			for (int i = 0; i < attacks.size; i++)
			{
				DatObject attackData = attacks[i].obj;

				attackData.getIdentifier("type", out string attackType);
				Attack attack = new Attack(Utils.ParseEnum<AttackType>(attackType));

				attackData.getIdentifier("name", out attack.name);
				attackData.getStringContent("animation", out attack.animation);
				attackData.getIdentifier("followUp", out attack.followUp);
				if (attackData.getInteger("damageFrameStart", out int damageFrameStart))
					attack.damageTimeStart = damageFrameStart / 24.0f;
				if (attackData.getInteger("damageFrameEnd", out int damageFrameEnd))
					attack.damageTimeEnd = damageFrameEnd / 24.0f;
				if (attackData.getInteger("blockFrameStart", out int blockFrameStart))
					attack.blockTimeStart = blockFrameStart / 24.0f;
				if (attackData.getInteger("blockFrameEnd", out int blockFrameEnd))
					attack.blockTimeEnd = blockFrameEnd / 24.0f;
				if (attackData.getInteger("parryFrameStart", out int parryFrameStart))
					attack.parryTimeStart = parryFrameStart / 24.0f;
				if (attackData.getInteger("parryFrameEnd", out int parryFrameEnd))
					attack.parryTimeEnd = parryFrameEnd / 24.0f;
				if (attackData.getInteger("followUpCancelFrame", out int followUpCancelFrame))
					attack.followUpCancelTime = followUpCancelFrame / 24.0f;
				if (attackData.getNumber("damageMultiplier", out float damageMultiplier))
					attack.damageMultiplier = damageMultiplier;
				if (attackData.getNumber("poiseDamageMultiplier", out float poiseDamageMultiplier))
					attack.poiseDamageMultiplier = poiseDamageMultiplier;
				attackData.getInteger("staminaCost", out attack.staminaCost);
				attackData.getInteger("manaCost", out attack.manaCost);
				if (attackData.getArray("projectiles", out DatArray projectiles))
				{
					attack.projectiles = new AttackProjectile[projectiles.size];
					for (int j = 0; j < projectiles.size; j++)
					{
						DatObject projectileData = projectiles[j].obj;

						AttackProjectile projectile = new AttackProjectile();
						projectileData.getStringContent("name", out projectile.name);
						if (projectileData.getInteger("frame", out int frame))
							projectile.time = frame / 24.0f;
						projectileData.getVector3("offset", out projectile.offset);
						projectileData.getBoolean("follow", out projectile.follow);
						projectileData.getBoolean("consumesItem", out projectile.consumesItem);
						if (projectileData.getStringContent("sfx", out string sfx))
							projectile.sfx = Resource.GetSound(directory + "/" + sfx);

						attack.projectiles[j] = projectile;
					}
				}

				item.attacks.Add(attack);
			}
		}

		return item;
	}
}
