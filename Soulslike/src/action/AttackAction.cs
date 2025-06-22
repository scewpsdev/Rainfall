using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AttackAction : PlayerAction
{
	public Weapon weapon;
	public AttackData attack;

	float damageMultiplier = 1.0f;

	float damageStartTime;
	float damageEndTime;

	List<Entity> hitEntities = new List<Entity>();

	long lastEnemyHit = -1;
	long lastWallHit = -1;

	WeaponTrail trail;
	Vector3 lastTip, lastOrigin;


	public AttackAction(Weapon weapon, AttackData attack, int hand)
		: base("attack")
	{
		this.weapon = weapon;
		this.attack = attack;

		animationName = attack.animation;
		animationData = weapon.moveset;

		lockRotation = true;

		canJump = true;

		damageStartTime = attack.damageRange.x / 24.0f;
		damageEndTime = attack.damageRange.y / 24.0f;
		followUpCancelTime = attack.cancelFrame / 24.0f;

		overrideRotationLockStartTime = damageStartTime * 0.5f;
		overrideRotationLockEndTime = damageEndTime;
	}

	public override void onStarted(Player player)
	{
		trail = new WeaponTrail(16, player.rightWeaponTransform.translation);
	}

	public override void onFinished(Player player)
	{
		//trail.destroy();
	}

	public override void update(Player mob)
	{
		base.update(mob);

		if (inDamageRange)
		{
			Matrix handTransform = mob.getModelMatrix() * mob.animator.getNodeTransform(mob.rightWeaponNode);
			Span<HitData> hits = stackalloc HitData[16];
			int numHits = Physics.OverlapSphere(0.2f, handTransform.translation, hits, QueryFilterFlags.Dynamic, PhysicsFilter.PlayerHitbox);
			for (int i = 0; i < numHits; i++)
			{
				HitData hit = hits[i];
				if (hit.body.entity != null)
				{
					if (!hitEntities.Contains(hit.body.entity))
					{
						hitEntities.Add(hit.body.entity as Entity);

						if (hit.body.entity is Hittable)
						{
							Hittable hittable = hit.body.entity as Hittable;
							Vector3 hitDirection = (hit.body.entity.getPosition() - mob.position).normalized;
							hittable.hit(1, false, hitDirection, mob, null, hit.body);
						}
					}
				}
			}

			float toTarget = -(GameState.instance.player.position.xz - mob.position.xz).angle + MathF.PI * 0.5f;
			mob.yaw = MathHelper.LinearAngle(mob.yaw, toTarget, 3 * Time.deltaTime);
		}
	}

	void processHit(ref HitData hit, Vector3 direction, Player player)
	{
		Entity entity = hit.body.entity as Entity;
		if (entity is Hittable)
		{
			Hittable hittable = entity as Hittable;

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
				//Audio.PlayOrganic(wallHit, hit.position);

				// spark particles
				ParticleEffect bloodEffect = new ParticleEffect("effect/spark.rfs", null);
				GameState.instance.scene.addEntity(bloodEffect, hit.position, Quaternion.LookAt(-hit.normal));

				lastWallHit = Time.currentTime;
			}
		}
	}

	public override void fixedUpdate(Player player, float delta)
	{
		base.fixedUpdate(player, delta);

		Vector3 origin = player.rightWeaponTransform * weapon.bladeBase;
		Vector3 tip = player.rightWeaponTransform * weapon.bladeTip;

		if (lastTip == Vector3.Zero)
		{
			lastTip = tip;
			lastOrigin = origin;
		}

		Span<HitData> hits = stackalloc HitData[16];

		int subSteps = 4;
		for (int i = 0; i < subSteps; i++)
		{
			interpolateCurve(lastOrigin, lastTip, origin, tip, (i + 0.5f) / subSteps, out Vector3 interpolatedOrigin, out Vector3 interpolatedTip);

			Vector3 direction = interpolatedTip - interpolatedOrigin;
			float distance = direction.length;

			int numHits = Physics.Raycast(origin, direction / distance, distance, hits, QueryFilterFlags.Default, PhysicsFilter.Default | PhysicsFilter.CreatureHitbox);
			for (int j = 0; j < numHits; j++)
			{
				Vector3 hitNormal = (tip - lastTip).normalized;
				processHit(ref hits[j], hitNormal, player);
			}

			//Vector3 interpolatedOrigin = Vector3.Lerp(lastOrigin, origin, (i + 0.5f) / subSteps);
			//Vector3 interpolatedTip = Vector3.Lerp(lastTip, tip, (i + 0.5f) / subSteps);
			float damageWindowProgress = MathHelper.Clamp(MathHelper.Remap(elapsedTime + (i + 0.5f) / subSteps * delta, damageStartTime, damageEndTime, 0, 1), 0, 1);
			float trailAlpha = 1 - MathF.Pow(damageWindowProgress * 2 - 1, 2);
			trail.update(interpolatedOrigin, interpolatedTip, trailAlpha);
		}

		lastTip = tip;
		lastOrigin = origin;
	}

	void interpolateCurve(Vector3 origin0, Vector3 tip0, Vector3 origin1, Vector3 tip1, float t, out Vector3 origin, out Vector3 tip)
	{
		float d0 = (tip0 - origin0).length;
		float d1 = (tip1 - origin1).length;
		Vector3 direction0 = (tip0 - origin0) / d0;
		Vector3 direction1 = (tip1 - origin1) / d1;
		Quaternion q0 = Quaternion.LookAt(direction0);
		Quaternion q1 = Quaternion.LookAt(direction1);

		closestPointsOnLines(origin0, tip0, origin1, tip1, out Vector3 closest0, out Vector3 closest1);

		Vector2 range0 = new Vector2(((origin0 - closest0) / direction0).x, ((tip0 - closest0) / direction0).x);
		Vector2 range1 = new Vector2(((origin1 - closest1) / direction1).x, ((tip1 - closest1) / direction1).x);

		Quaternion q = Quaternion.Slerp(q0, q1, t);
		Vector2 range = Vector2.Lerp(range0, range1, t);

		Vector3 intersection = Vector3.Lerp(closest0, closest1, t);
		origin = intersection + q.forward * range.x;
		tip = intersection + q.forward * range.y;
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
		base.draw(player);

		trail.draw();
	}

	public bool inDamageRange => elapsedTime >= damageStartTime && elapsedTime < damageEndTime;
}
