using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AttackAction : PlayerAction
{
	const float HIT_FREEZE_LENGTH = 0.1f;
	const float HIT_FREEZE_SPEED = 0.1f;


	static Sound[] swing = Resource.GetSounds("audio/swing", 3);
	static Sound[] stab = Resource.GetSounds("audio/swing_stab", 4);


	public Weapon weapon;
	public AttackData attack;

	float damageMultiplier;

	float damageStartTime;

	Vector3 lastTip = Vector3.Zero;

	List<Entity> hitEntities = new List<Entity>();

	WeaponTrail trail;

	long lastEnemyHit = -1;
	bool inFreeze = false;


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
			damageMultiplier = MathHelper.Remap(chargeAmount, 0, 1, 1, 2);
			staminaCost = MathHelper.Remap(chargeAmount, 0, 1, 1, 2);
		}

		if (weapon.twoHanded)
		{
			animationName[hand ^ 1] = attack.animation;
			animationSet[hand ^ 1] = weapon.moveset;
		}

		mirrorAnimation = hand == 1;

		damageStartTime = attack.damageFrame / 24.0f;
		followUpCancelTime = attack.cancelFrame / 24.0f;

		lockYaw = true;

		viewmodelAim = 1;

		addSoundEffect(new ActionSfx(attack.damageType == DamageType.Thrust ? stab : swing, 1, 1.0f / followUpCancelTime * 0.5f * animationSpeed * (chargeAmount != -1 ? 0.8f : 1), damageStartTime, true));
	}

	public override void onStarted(Player player)
	{
		trail = new WeaponTrail(20, player.rightWeaponTransform.translation);
	}

	void processHit(ref HitData hit, Player player)
	{
		Entity entity = hit.body.entity as Entity;
		if (!hitEntities.Contains(entity))
		{
			hitEntities.Add(entity);
			if (entity is Hittable)
			{
				Hittable hittable = entity as Hittable;
				hittable.hit(player, weapon);

				if (hittable is Creature)
				{
					Creature creature = entity as Creature;
					Sound[] hitSound = attack.damageType == DamageType.Thrust ? creature.stabSound : creature.slashSound;
					Audio.PlayOrganic(hitSound, hit.position);

					// blood particles
				}

				lastEnemyHit = Time.currentTime;
			}
		}
	}

	public override void update(Player player)
	{
		if (lastEnemyHit != -1 && (Time.currentTime - lastEnemyHit) / 1e9f < HIT_FREEZE_LENGTH && !inFreeze)
		{
			animationSpeed *= HIT_FREEZE_SPEED;
			inFreeze = true;
		}
		else if (inFreeze)
		{
			animationSpeed /= HIT_FREEZE_SPEED;
			inFreeze = false;
		}

		base.update(player);

		lockYaw = inDamageWindow;
	}

	public override void fixedUpdate(Player player, float delta)
	{
		Vector3 origin = player.rightWeaponTransform * weapon.bladeBase;
		Vector3 tip = player.rightWeaponTransform * weapon.bladeTip;

		trail.update(origin, tip, inDamageWindow ? 1 : 0);

		if (inDamageWindow)
		{
			Vector3 direction = tip - origin;
			float distance = direction.length;

			{
				Span<HitData> hits = stackalloc HitData[16];
				int numHits = Physics.Raycast(origin, direction / distance, distance, hits, QueryFilterFlags.Default, PhysicsFilter.Creature | PhysicsFilter.CreatureHitbox);
				for (int i = 0; i < numHits; i++)
				{
					processHit(ref hits[i], player);
				}
			}

			if (lastTip != Vector3.Zero)
			{
				direction = tip - lastTip;
				distance = direction.length;

				Span<HitData> hits = stackalloc HitData[16];
				int numHits = Physics.Raycast(tip, direction / distance, distance, hits, QueryFilterFlags.Default, PhysicsFilter.Creature | PhysicsFilter.CreatureHitbox);
				for (int i = 0; i < numHits; i++)
				{
					processHit(ref hits[i], player);
				}
			}

			lastTip = tip;
		}
	}

	public override void draw(Player player)
	{
		trail.draw();
	}

	bool inDamageWindow => elapsedTime >= damageStartTime && elapsedTime < followUpCancelTime;
}
