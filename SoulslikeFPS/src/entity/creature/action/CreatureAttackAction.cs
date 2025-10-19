using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public struct CreatureAttack
{
	public string name;
	public string nextAttack;
	public string animation;
	public Vector2i damageRange;
	public int cancelFrame;
	public DamageType damageType;

	public float rarity = 1.0f;
	public bool blockable = true;

	public float triggerDistanceMin = 0;
	public float triggerDistanceMax = 5;
	public float triggerAngleMin = -180;
	public float triggerAngleMax = 180;

	public int turnFrameStart = 0;
	public int turnFrameEnd = 0;
	public float turnSpeed = 5.0f;

	public CreatureActionEvent[] events;


	public CreatureAttack(string name, string animation, Vector2i damageRange, int cancelFrame, DamageType damageType = DamageType.Slash, string nextAttack = null)
	{
		this.name = name;
		this.nextAttack = nextAttack;
		this.animation = animation;
		this.damageRange = damageRange;
		this.cancelFrame = cancelFrame;
		this.damageType = damageType;
	}
}

public class CreatureAttackAction : CreatureAction
{
	public CreatureAttack attack;

	WeaponTrail trail;
	Vector3 lastTip, lastOrigin;

	float damageStartTime;
	float damageEndTime;

	List<Entity> hitEntities = new List<Entity>();


	public CreatureAttackAction(CreatureAttack attack)
		: base("attack")
	{
		this.attack = attack;

		animationName = attack.animation;

		damageStartTime = attack.damageRange.x / 24.0f;
		damageEndTime = attack.damageRange.y / 24.0f;



		if (attack.events != null)
		{
			foreach (CreatureActionEvent ev in attack.events)
				addEvent(ev);
		}
	}

	public override void onStarted(Creature creature)
	{
		trail = new WeaponTrail(16, creature.rightWeaponTransform.translation);
	}

	public override void update(Creature mob)
	{
		base.update(mob);

		rotationSpeedMultiplier = (elapsedTime >= attack.turnFrameStart / 24.0f && elapsedTime < attack.turnFrameEnd / 24.0f) ? attack.turnSpeed : 0.0f;

		return;

		if (inDamageWindow)
		{
			//if (mob.rightWeapon != null)
			//	;
			//else
			{
				Matrix handTransform = mob.getModelMatrix() * mob.animator.getNodeTransform(mob.rightWeaponNode);
				Span<PhysicsHit> hits = stackalloc PhysicsHit[16];
				int numHits = Physics.OverlapSphere(0.2f, handTransform.translation, hits, QueryFilterFlags.Dynamic, PhysicsFilter.PlayerHitbox);
				for (int i = 0; i < numHits; i++)
				{
					PhysicsHit hit = hits[i];
					if (hit.body.entity != null)
					{
						if (!hitEntities.Contains(hit.body.entity))
						{
							hitEntities.Add(hit.body.entity as Entity);

							if (hit.body.entity is Hittable)
							{
								Hittable hittable = hit.body.entity as Hittable;
								Vector3 hitDirection = (hit.body.entity.getPosition() - mob.position).normalized;
								hittable.hit(new HitData(1, false, hitDirection, mob, null, hit.body));
							}
						}
					}
				}
			}

			//float toTarget = -(GameState.instance.player.position.xz - mob.position.xz).angle + MathF.PI * 0.5f;
			//mob.yaw = Mathf.LinearAngle(mob.yaw, toTarget, 3 * Time.deltaTime);
		}
	}

	void processHit(ref PhysicsHit hit, Vector3 direction, Creature creature)
	{
		Entity entity = hit.body.entity as Entity;
		if (entity is Hittable)
		{
			Hittable hittable = entity as Hittable;

			if (!hitEntities.Contains(entity))
			{
				hitEntities.Add(entity);

				float damage = 15; // weapon.damage * damageMultiplier;

				HitData hitData = new HitData(0);
				hitData.damage = (int)MathF.Ceiling(damage);
				hitData.blockable = attack.blockable;
				hitData.hitDirection = direction;
				hitData.by = creature;
				hitData.hitbox = hit.body;
				hittable.hit(hitData);

				if (hittable is Creature)
				{
					Creature otherCreature = entity as Creature;
					Sound[] hitSound = attack.damageType == DamageType.Thrust ? otherCreature.stabSound : otherCreature.slashSound;
					Audio.PlayOrganic(hitSound, hit.position);

					// blood particles
					ParticleEffect bloodEffect = new ParticleEffect("effect/blood.rfs", null);
					GameState.instance.scene.addEntity(bloodEffect, hit.position, Quaternion.LookAt(-hit.normal));
				}
			}
		}
		else
		{
			float bladeLength = creature.weaponReach;
			float hitNormalizedDist = hit.distance / bladeLength;
			if (hitNormalizedDist < 0.5f)
			{
				// wall hit sound
				//Audio.PlayOrganic(wallHit, hit.position);

				// spark particles
				ParticleEffect bloodEffect = new ParticleEffect("effect/spark.rfs", null);
				GameState.instance.scene.addEntity(bloodEffect, hit.position, Quaternion.LookAt(-hit.normal));
			}
		}
	}

	public override void fixedUpdate(Creature creature, float delta)
	{
		base.fixedUpdate(creature, delta);

		Vector3 origin = creature.rightWeaponTransform.translation;
		Vector3 tip = creature.rightWeaponTransform * new Vector3(0, 0, creature.weaponReach);

		if (lastTip == Vector3.Zero)
		{
			lastTip = tip;
			lastOrigin = origin;
		}

		Span<PhysicsHit> hits = stackalloc PhysicsHit[16];

		int subSteps = 4;
		for (int i = 0; i < subSteps; i++)
		{
			interpolateCurve(lastOrigin, lastTip, origin, tip, (i + 0.5f) / subSteps, out Vector3 interpolatedOrigin, out Vector3 interpolatedTip);

			if (inDamageWindow)
			{
				Vector3 direction = interpolatedTip - interpolatedOrigin;
				float distance = direction.length;

				int numHits = Physics.SweepSphere(creature.weaponRadius, interpolatedOrigin, direction / distance, distance, hits, QueryFilterFlags.Default, PhysicsFilter.Default | PhysicsFilter.PlayerHitbox);
				for (int j = 0; j < numHits; j++)
				{
					Vector3 hitNormal = (tip - lastTip).normalized;
					processHit(ref hits[j], hitNormal, creature);
				}

				//player.scene.addEntity(new DebugLine(interpolatedOrigin, interpolatedTip, 0xFFFF0000));
			}

			//Vector3 interpolatedOrigin = Vector3.Lerp(lastOrigin, origin, (i + 0.5f) / subSteps);
			//Vector3 interpolatedTip = Vector3.Lerp(lastTip, tip, (i + 0.5f) / subSteps);
			float damageWindowProgress = Mathf.Clamp(Mathf.Remap(elapsedTime + (i + 0.5f) / subSteps * delta, damageStartTime, damageEndTime, 0, 1), 0, 1);
			float trailAlpha = 1 - MathF.Pow(damageWindowProgress * 2 - 1, 2);
			trail.update(interpolatedOrigin, interpolatedTip, trailAlpha);
		}

		//if (inDamageRange)
		//	player.scene.addEntity(new DebugLine(origin, tip, 0xFFFF00FF));

		lastTip = tip;
		lastOrigin = origin;
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

	public override void draw(Creature player)
	{
		base.draw(player);

		trail.draw();

		Vector3 origin = player.rightWeaponTransform.translation;
		Vector3 tip = player.rightWeaponTransform * new Vector3(0, 0, player.weaponReach);

		if (inDamageWindow)
			Renderer.DrawDebugLine(origin, tip, 0xFFFF0000);
	}

	public bool inDamageWindow => elapsedTime >= attack.damageRange.x / 24.0f && elapsedTime < attack.damageRange.y / 24.0f;
}
