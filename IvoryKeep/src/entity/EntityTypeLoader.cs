using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class EntityTypeLoader
{
	public static EntityType Load(string path)
	{
		DatFile file = new DatFile(Resource.ReadText(path), path);
		string directory = Path.GetDirectoryName(path);

		EntityType entityType = new EntityType(file);

		file.getStringContent("name", out entityType.name);
		file.getStringContent("displayName", out entityType.displayName);
		file.getIdentifier("category", out entityType.category);

		if (file.getStringContent("entityData", out string entityFile))
		{
			string entityPath = directory + "/" + entityFile;
			entityType.entityDataPath = entityPath;
			if (File.Exists(entityPath + ".bin"))
			{
				FileStream stream = new FileStream(entityPath + ".bin", FileMode.Open);
				SceneFormat.DeserializeScene(stream, out List<SceneFormat.EntityData> entities, out uint selectedEntity);
				Debug.Assert(entities.Count > 0);
				SceneFormat.EntityData entityData = entities[0];
				entityData.load(Path.GetDirectoryName(entityPath));
				entityType.entityData = entityData;
				stream.Close();
			}
		}

		// Object Data
		if (file.getStringContent("idleSound", out string idleSoundPath))
			entityType.idleSound = Resource.GetSound(directory + "/" + idleSoundPath);

		// Creature Data
		file.getInteger("health", out entityType.health);
		file.getInteger("poise", out entityType.poise);
		if (file.getArray("stepSound", out DatArray stepSound))
		{
			entityType.stepSound = new Sound[stepSound.size];
			for (int i = 0; i < stepSound.size; i++)
				entityType.stepSound[i] = Resource.GetSound(directory + "/" + stepSound[i].stringContent);
		}
		if (file.getArray("landSound", out DatArray landSound))
		{
			entityType.landSound = new Sound[landSound.size];
			for (int i = 0; i < landSound.size; i++)
				entityType.landSound[i] = Resource.GetSound(directory + "/" + landSound[i].stringContent);
		}
		if (file.getArray("hitSound", out DatArray hitSound))
		{
			entityType.hitSound = new Sound[hitSound.size];
			for (int i = 0; i < hitSound.size; i++)
				entityType.hitSound[i] = Resource.GetSound(directory + "/" + hitSound[i].stringContent);
		}

		// Mob Data
		file.getIdentifier("ai", out entityType.ai);
		if (file.getIdentifier("rightHandItem", out string rightHandItem))
			entityType.rightHandItem = Item.Get(rightHandItem);
		if (file.getIdentifier("leftHandItem", out string leftHandItem))
			entityType.leftHandItem = Item.Get(leftHandItem);
		file.getInteger("baseDamage", out entityType.baseDamage);
		if (file.getArray("attacks", out DatArray attacks))
		{
			for (int i = 0; i < attacks.size; i++)
			{
				DatObject attackData = attacks[i].obj;

				MobAttack attack = new MobAttack(0);
				attackData.getIdentifier("name", out attack.name);
				attackData.getStringContent("animation", out attack.animation);
				attackData.getIdentifier("followUp", out attack.followUp);
				if (attackData.getArray("damageFrames", out DatArray damageFrames))
				{
					Debug.Assert(damageFrames.size == 2 && damageFrames[0].type == DatValueType.Number && damageFrames[1].type == DatValueType.Number);
					attack.damageTimeStart = damageFrames[0].integer / 24.0f;
					attack.damageTimeEnd = damageFrames[1].integer / 24.0f;
				}
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
				attackData.getNumber("staminaCost", out attack.staminaCost);
				attackData.getInteger("manaCost", out attack.manaCost);
				if (attackData.getArray("projectiles", out DatArray projectiles))
				{
					attack.projectiles = new AttackProjectile[projectiles.size];
					for (int j = 0; j < projectiles.size; j++)
					{
						DatObject projectileData = projectiles[j].obj;

						AttackProjectile projectile = new AttackProjectile();
						projectileData.getIdentifier("name", out projectile.name);
						if (projectileData.getInteger("frame", out int frame))
							projectile.time = frame / 24.0f;
						projectileData.getVector3("offset", out projectile.offset);
						if (projectileData.getStringContent("sfx", out string sfx))
							projectile.sfx = Resource.GetSound(directory + "/" + sfx);

						attack.projectiles[j] = projectile;
					}
				}
				if (attackData.getNumber("triggerDistanceMin", out float triggerDistanceMin))
					attack.triggerDistanceMin = triggerDistanceMin;
				if (attackData.getNumber("triggerDistanceMax", out float triggerDistanceMax))
					attack.triggerDistanceMax = triggerDistanceMax;
				if (attackData.getNumber("triggerAngleMin", out float triggerAngleMin))
					attack.triggerAngleMin = triggerAngleMin;
				if (attackData.getNumber("triggerAngleMax", out float triggerAngleMax))
					attack.triggerAngleMax = triggerAngleMax;

				entityType.attacks.Add(attack);
			}
		}

		// Projectile Data
		if (file.getNumber("projectileSpeed", out float projectileSpeed))
			entityType.projectileSpeed = projectileSpeed;
		if (file.getNumber("projectileGravity", out float projectileGravity))
			entityType.projectileGravity = projectileGravity;
		file.getBoolean("projectileRemoveOnHit", out entityType.projectileRemoveOnHit);
		file.getStringContent("projectileHitSpawnEntity", out entityType.projectileHitSpawnEntity);

		// Effect
		file.getInteger("effectDamage", out entityType.effectDamage);
		file.getInteger("effectPoiseDamage", out entityType.effectPoiseDamage);
		file.getNumber("effectDamageRadius", out entityType.effectDamageRadius);

		return entityType;
	}
}
