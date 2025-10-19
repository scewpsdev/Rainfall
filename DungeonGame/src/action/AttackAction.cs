using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class AttackAction : Action
{
	const float HIT_SLOWDOWN_MAX_DURATION = 0.2f;
	const float HIT_SLOWDOWN_STRENGTH = 0.3f;

	const float HIT_KNOCKBACK_DURATION = 0.4f;


	public Item item;
	public int handID;
	public Attack attack;

	public List<Entity> hitEntities = new List<Entity>();

	long lastHittableHitTime = 0;
	long firstHittableHitTime = 0;
	long lastObjectHitTime = 0;


	public AttackAction(Item item, int handID, Attack attack, bool twoHanded)
		: base(ActionType.Attack)
	{
		this.item = item;
		this.handID = handID;
		this.attack = attack;

		if (twoHanded)
		{
			animationName[0] = attack.animationName;
			animationName[1] = attack.animationName;
			animationSet[0] = item.moveset;
			animationSet[1] = item.moveset;
		}
		else
		{
			animationName[handID] = attack.animationName;
			animationSet[handID] = item.moveset;
		}

		animationSpeed = 1.0f;
		mirrorAnimation = handID == 1;
		fullBodyAnimation = false;
		animateCameraRotation = false;
		rootMotion = true;
		animationTransitionDuration = 0.1f;
		followUpCancelTime = attack.followUpCancelTime;

		movementSpeedMultiplier = 0.5f;

		staminaCost = attack.staminaCost;
		staminaCostTime = attack.damageTimeStart;

		if (attack.type != AttackType.Heavy && item.sfxSwing != null || item.sfxSwingHeavy == null)
			addSoundEffect(item.sfxSwing, handID, attack.damageTimeStart, true);
		if (attack.type == AttackType.Heavy && item.sfxSwingHeavy != null)
			addSoundEffect(item.sfxSwingHeavy, handID, attack.damageTimeStart, true);
	}

	public void onHit(Entity entity, HitData hit, Player player, int handID)
	{
		if (entity is Creature)
		{
			lastHittableHitTime = Time.currentTime;
			if (firstHittableHitTime == 0)
				firstHittableHitTime = Time.currentTime;
		}
		else if (entity is not Hittable)
		{
			/*
			if (hit.distance < 0.1f)
				lastObjectHitTime = Time.currentTime;
			return;

			Span<HitData> hits = stackalloc HitData[16];
			Vector3 direction = player.handEntities[handID].transform.rotation.up;
			Vector3 start = player.handEntities[handID].transform.translation;
			float range = player.handEntities[handID].item.hitboxRange * 3;
			int numHits = Physics.Raycast(start, direction, range, hits, QueryFilterFlags.Static);
			if (numHits > 0)
			{
				float shortestDistance = 1000.0f;
				for (int i = 0; i < numHits; i++)
				{
					float distance = hits[i].distance;
					if (distance != 0 && distance < shortestDistance)
						shortestDistance = distance;
				}
				if (shortestDistance < 0.0f)
					lastObjectHitTime = Time.currentTime;
			}
			*/
		}
	}

	public override void update(Player player)
	{
		base.update(player);

		if ((Time.currentTime - lastHittableHitTime) / 1e9f < 0.05f && (Time.currentTime - firstHittableHitTime) / 1e9f < HIT_SLOWDOWN_MAX_DURATION)
		{
			animationSpeed = HIT_SLOWDOWN_STRENGTH;
		}
		else if (lastObjectHitTime != 0)
		{
			if ((Time.currentTime - lastObjectHitTime) / 1e9f < HIT_KNOCKBACK_DURATION)
			{
				if (animationSpeed > 0.0f)
				{
					animationSpeed = -0.35f;
					duration = Math.Max(duration, elapsedTime + HIT_KNOCKBACK_DURATION);
				}
			}
			else
			{
				player.cancelAction();
			}
		}
		else
		{
			animationSpeed = 1.0f;
		}
	}
}
