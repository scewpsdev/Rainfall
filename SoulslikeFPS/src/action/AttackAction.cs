using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AttackAction : FirstPersonAction
{
	const float HIT_FREEZE_LENGTH = 0.3f;
	const float HIT_FREEZE_SPEED = 0.2f;


	static Sound[] swing = Resource.GetSounds("sound/item/swing", 3);
	static Sound[] stab = Resource.GetSounds("sound/item/swing_stab", 4);
	static Sound wallHit = Resource.GetSound("sound/hit/hit_wall.ogg");


	public Weapon weapon;
	public AttackData attack;

	float damageMultiplier = 1;

	float damageStartTime;
	float damageEndTime;

	Vector3 lastTip, lastOrigin;
	WeaponTrail trail;

	List<Entity> hitEntities = new List<Entity>();

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
			animationSpeed = Mathf.Remap(chargeAmount, 0, 1, 1, 0.85f);
			damageMultiplier = Mathf.Remap(chargeAmount, 0, 1, 1.5f, 3);
			staminaCost = Mathf.Remap(chargeAmount, 0, 1, 1, 1.5f);
		}

		//animationSpeed *= 0.7f;

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
		//if (attack.damageType == DamageType.Slash)
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
		/*
		else if (attack.damageType == DamageType.Blunt || attack.damageType == DamageType.Thrust)
		{
			if (lastEnemyHit != -1 && (Time.currentTime - lastEnemyHit) / 1e9f < 0.125f && slowdownTime == -1)
			{
				animationSpeed *= HIT_FREEZE_SPEED;
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
		*/

		if (lastWallHit != -1 && (Time.currentTime - lastWallHit) / 1e9f < 0.5f && !inReflect)
		{
			animationSpeed *= -0.3f;
			inReflect = true;
		}
		else if (lastWallHit != -1 && (Time.currentTime - lastWallHit) / 1e9f >= 0.5f && inReflect)
		{
			cancel();
		}

		//lockYaw = inDamageWindow;

		base.update(player);
	}

	void processHit(ref PhysicsHit hit, Vector3 direction, Player player, int substep)
	{
		Entity entity = hit.body.entity as Entity;
		if (entity is Hittable)
		{
			Hittable hittable = entity as Hittable;

			//lockYaw = true;

			if (!hitEntities.Contains(entity))
			{
				hitEntities.Add(entity);

				float damage = weapon.damage * damageMultiplier;

				bool isEffective = hit.distance >= weapon.bladeEffectiveRange.x && hit.distance <= weapon.bladeEffectiveRange.y;
				if (!isEffective)
				{
					damage *= 0.5f;
					Debug.Warn("ineffective hit! " + hit.distance);
				}

				HitData hitData = new HitData(0);
				hitData.damage = (int)MathF.Ceiling(damage);
				hitData.hitDirection = direction;
				hitData.by = player;
				hitData.item = weapon;
				hitData.hitbox = hit.body;
				hittable.hit(hitData);

				if (hittable is Creature)
				{
					Creature creature = entity as Creature;
					Sound[] hitSound = attack.damageType == DamageType.Thrust ? creature.stabSound : creature.slashSound;
					Audio.PlayOrganic(hitSound, hit.position);

					Audio.PlayOrganic(weapon.hitSound, player.rightWeaponTransform * weapon.sfxSourcePosition);

					lastEnemyHit = Time.currentTime;
				}
			}

			if (entity is Creature && substep == 0)
			{
				// blood particles
				ParticleEffect bloodEffect = new ParticleEffect("effect/blood.rfs", null);
				GameState.instance.scene.addEntity(bloodEffect, hit.position, Quaternion.LookAt(-hit.normal));
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

		if (lastTip == Vector3.Zero)
		{
			lastTip = tip;
			lastOrigin = origin;
		}

		//float damageWindowProgress = Mathf.Clamp(Mathf.Remap(elapsedTime, damageStartTime, damageEndTime, 0, 1), 0, 1);
		//float trailAlpha = 1 - MathF.Pow(damageWindowProgress * 2 - 1, 2);
		//trail.update(origin, tip, trailAlpha);

		Span<PhysicsHit> hits = stackalloc PhysicsHit[16];

		int subSteps = 4;
		for (int j = 0; j < subSteps; j++)
		{
			interpolateCurve(lastOrigin, lastTip, origin, tip, (j + 0.5f) / subSteps, out Vector3 interpolatedOrigin, out Vector3 interpolatedTip);

			if (inDamageWindow)
			{
				//Vector3 dst = Vector3.Lerp(lastTip, tip, (j + 1) / (float)subSteps);
				Vector3 direction = interpolatedTip - interpolatedOrigin;
				float distance = direction.length;

				Vector3 hitDirection = (interpolatedTip - lastTip).normalized;

				int numHits = Physics.Raycast(interpolatedOrigin, direction / distance, distance, hits, QueryFilterFlags.Default, PhysicsFilter.Default | PhysicsFilter.CreatureHitbox);
				for (int i = 0; i < numHits; i++)
				{
					processHit(ref hits[i], hitDirection, player, j);
				}
			}

			float damageWindowProgress = Mathf.Clamp(Mathf.Remap(elapsedTime + (j + 0.5f) / subSteps * delta, damageStartTime, damageEndTime, 0, 1), 0, 1);
			float trailAlpha = 1 - MathF.Pow(damageWindowProgress * 2 - 1, 2);
			trail.update(interpolatedOrigin, interpolatedTip, trailAlpha);
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

		lastTip = tip;
	}

	void interpolateCurve(Vector3 origin0, Vector3 tip0, Vector3 origin1, Vector3 tip1, float t, out Vector3 origin, out Vector3 tip)
	{
		float d0 = (tip0 - origin0).length;
		float d1 = (tip1 - origin1).length;
		Vector3 direction0 = (tip0 - origin0) / d0;
		Vector3 direction1 = (tip1 - origin1) / d1;
		Vector3 direction = Vector3.Slerp(direction0, direction1, t);

		closestPointsOnLines(origin0, tip0, origin1, tip1, out Vector3 closest0, out Vector3 closest1);

		Vector2 range0 = new Vector2(((origin0 - closest0) / direction0).x, ((tip0 - closest0) / direction0).x);
		Vector2 range1 = new Vector2(((origin1 - closest1) / direction1).x, ((tip1 - closest1) / direction1).x);
		Vector2 range = Vector2.Lerp(range0, range1, t);

		//Quaternion q0 = Quaternion.LookAt(direction0);
		//Quaternion q1 = Quaternion.LookAt(direction1);
		//Quaternion q = Quaternion.Slerp(q0, q1, t).normalized;

		Vector3 intersection = Vector3.Lerp(closest0, closest1, t);
		origin = intersection + direction * range.x;
		tip = intersection + direction * range.y;
	}

	void closestPointsOnLines(Vector3 p1, Vector3 p2, Vector3 q1, Vector3 q2, out Vector3 result1, out Vector3 result2)
	{
		Vector3 u = p2 - p1;
		Vector3 v = q2 - q1;
		Vector3 w0 = p1 - q1;

		float a = Vector3.Dot(u, u); // u•u
		float b = Vector3.Dot(u, v); // u•v
		float c = Vector3.Dot(v, v); // v•v
		float d = Vector3.Dot(u, w0); // u•w0
		float e = Vector3.Dot(v, w0); // v•w0

		float denom = a * c - b * b;

		// Lines are nearly parallel
		if (MathF.Abs(denom) < 1e-6f)
		{
			// Arbitrarily choose s = 0
			float s = 0f;
			float t = (b > c ? d / b : e / c);

			result1 = p1 + s * u;
			result2 = q1 + t * v;
		}
		else
		{
			float s = (b * e - c * d) / denom;
			float t = (a * e - b * d) / denom;

			result1 = p1 + s * u;
			result2 = q1 + t * v;
		}
	}

	public override void draw(Player player)
	{
		trail.draw();

		Vector3 origin = player.rightWeaponTransform * weapon.bladeBase;
		Vector3 tip = player.rightWeaponTransform * weapon.bladeTip;

		if (inDamageWindow)
			Renderer.DrawDebugLine(origin, tip, 0xFFFF0000);
	}

	bool inDamageWindow => elapsedTime >= damageStartTime && elapsedTime < damageEndTime;
}
