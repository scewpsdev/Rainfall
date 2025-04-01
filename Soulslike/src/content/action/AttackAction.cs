using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AttackAction : PlayerAction
{
	const float HIT_FREEZE_LENGTH = 0.3f;
	const float HIT_FREEZE_SPEED = 0.1f;


	static Sound[] swing = Resource.GetSounds("sound/item/swing", 3);
	static Sound[] stab = Resource.GetSounds("sound/item/swing_stab", 4);
	static Sound wallHit = Resource.GetSound("sound/hit/hit_wall.ogg");


	public Weapon weapon;
	public AttackData attack;

	float damageMultiplier = 1;

	float damageStartTime;
	float damageEndTime;

	Vector3 lastTip = Vector3.Zero;

	List<Entity> hitEntities = new List<Entity>();

	WeaponTrail trail;

	long lastEnemyHit = -1;
	long slowdownTime = -1;
	long lastWallHit = -1;
	bool inReflect = false;


	public AttackAction(Weapon weapon, AttackData attack, int hand, float chargeAmount = -1)
		: base("attack", hand)
	{
		this.weapon = weapon;
		this.attack = attack;

		animationName[hand] = attack.animation;
		animationSet[hand] = weapon.moveset;

		if (chargeAmount != -1)
		{
			animationSpeed = MathHelper.Remap(chargeAmount, 0, 1, 1, 0.85f);
			damageMultiplier = MathHelper.Remap(chargeAmount, 0, 1, 1.5f, 3);
			staminaCost = MathHelper.Remap(chargeAmount, 0, 1, 1, 1.5f);
		}

		if (weapon.twoHanded)
		{
			animationName[hand ^ 1] = attack.animation;
			animationSet[hand ^ 1] = weapon.moveset;
		}

		mirrorAnimation = hand == 1;

		damageStartTime = attack.damageRange.x / 24.0f;
		damageEndTime = attack.damageRange.y / 24.0f;
		followUpCancelTime = attack.cancelFrame / 24.0f;

		//lockYaw = true;

		viewmodelAim = 1;

		addSoundEffect(new ActionSfx(attack.damageType == DamageType.Thrust ? stab : swing, 1, 1.0f / followUpCancelTime * 0.5f * animationSpeed * (chargeAmount != -1 ? 0.8f : 1), damageStartTime, true));
	}

	public override void onStarted(Player player)
	{
		trail = new WeaponTrail(20, player.rightWeaponTransform.translation);
	}

	public override void update(Player player)
	{
		if (attack.damageType == DamageType.Slash)
		{
			if (lastEnemyHit != -1 && (Time.currentTime - lastEnemyHit) / 1e9f < HIT_FREEZE_LENGTH && slowdownTime == -1)
			{
				animationSpeed *= HIT_FREEZE_SPEED;
				slowdownTime = Time.currentTime;
			}
			else if (slowdownTime != -1 && (Time.currentTime - slowdownTime) / 1e9f >= HIT_FREEZE_LENGTH)
			{
				animationSpeed /= HIT_FREEZE_SPEED;
				slowdownTime = -1;
			}
		}
		else if (attack.damageType == DamageType.Blunt || attack.damageType == DamageType.Thrust)
		{
			if (lastEnemyHit != -1 && (Time.currentTime - lastEnemyHit) / 1e9f < 0.125f && slowdownTime == -1)
			{
				animationSpeed *= 0.1f;
				slowdownTime = Time.currentTime;
			}
			else if (slowdownTime != -1 && (Time.currentTime - slowdownTime) / 1e9f >= 0.125f && animationSpeed > 0)
			{
				animationSpeed = 0;
			}
			else if (slowdownTime != -1 && (Time.currentTime - slowdownTime) / 1e9f >= 0.3f && animationSpeed >= 0)
			{
				animationSpeed = -0.1f;
			}
			else if (slowdownTime != -1 && (Time.currentTime - slowdownTime) / 1e9f >= 0.4f)
			{
				cancel();
			}
		}

		if (lastWallHit != -1 && (Time.currentTime - lastWallHit) / 1e9f < 0.5f && !inReflect)
		{
			animationSpeed *= -0.3f;
			inReflect = true;
		}
		else if (lastWallHit != -1 && (Time.currentTime - lastWallHit) / 1e9f >= 0.5f && inReflect)
		{
			cancel();
		}

		base.update(player);

		//lockYaw = inDamageWindow;
	}

	void processHit(ref HitData hit, Vector3 direction, Player player)
	{
		Entity entity = hit.body.entity as Entity;
		if (entity is Hittable)
		{
			Hittable hittable = entity as Hittable;

			lockYaw = true;

			if (!hitEntities.Contains(entity))
			{
				hitEntities.Add(entity);

				float damage = weapon.damage * damageMultiplier;
				hittable.hit((int)MathF.Ceiling(damage), false, direction, player, weapon, hit.body);

				if (hittable is Creature)
				{
					Creature creature = entity as Creature;
					Sound[] hitSound = attack.damageType == DamageType.Thrust ? creature.stabSound : creature.slashSound;
					Audio.PlayOrganic(hitSound, hit.position);

					// blood particles
					ParticleEffect bloodEffect = new ParticleEffect("effect/blood.rfs", null);
					GameState.instance.scene.addEntity(bloodEffect, hit.position, Quaternion.LookAt(-hit.normal));
					lastEnemyHit = Time.currentTime;
				}
			}
		}
		else
		{
			float bladeLength = (weapon.bladeTip - weapon.bladeBase).length;
			float hitNormalizedDist = hit.distance / bladeLength;
			if (hitNormalizedDist < 0.5f && lastWallHit == -1)
			{
				// wall hit sound
				Audio.PlayOrganic(wallHit, hit.position);

				// spark particles
				ParticleEffect bloodEffect = new ParticleEffect("effect/spark.rfs", null);
				GameState.instance.scene.addEntity(bloodEffect, hit.position, Quaternion.LookAt(-hit.normal));

				lastWallHit = Time.currentTime;
			}
		}
	}

	public override void fixedUpdate(Player player, float delta)
	{
		Vector3 origin = player.rightWeaponTransform * weapon.bladeBase;
		Vector3 tip = player.rightWeaponTransform * weapon.bladeTip;

		float damageWindowProgress = MathHelper.Clamp(MathHelper.Remap(elapsedTime, damageStartTime, damageEndTime, 0, 1), 0, 1);
		float trailAlpha = 1 - MathF.Pow(damageWindowProgress * 2 - 1, 2);
		trail.update(origin, tip, trailAlpha);

		if (inDamageWindow)
		{
			Vector3 hitDirection = (tip - lastTip).normalized;

			int subSteps = 4;
			for (int j = 0; j < subSteps; j++)
			{
				Vector3 dst = Vector3.Lerp(lastTip, tip, (j + 1) / (float)subSteps);
				Vector3 direction = dst - origin;
				float distance = direction.length;

				Span<HitData> hits = stackalloc HitData[16];
				int numHits = Physics.Raycast(origin, direction / distance, distance, hits, QueryFilterFlags.Default, PhysicsFilter.Default | PhysicsFilter.CreatureHitbox);
				for (int i = 0; i < numHits; i++)
				{
					processHit(ref hits[i], hitDirection, player);
				}
			}

			/*
			if (lastTip != Vector3.Zero && lastTip != tip)
			{
				direction = tip - lastTip;
				distance = direction.length;

				Span<HitData> hits = stackalloc HitData[16];
				int numHits = Physics.Raycast(tip, direction / distance, distance, hits, QueryFilterFlags.Default, PhysicsFilter.Default | PhysicsFilter.CreatureHitbox);
				for (int i = 0; i < numHits; i++)
				{
					//processHit(ref hits[i], player);
				}
			}
			*/
		}
		else
		{
			lockYaw = false;
		}

		lastTip = tip;
	}

	public override void draw(Player player)
	{
		trail.draw();
	}

	bool inDamageWindow => elapsedTime >= damageStartTime && elapsedTime < damageEndTime;
}
