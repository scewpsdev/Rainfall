﻿using Rainfall;
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

	public CreatureAttack(string name, string nextAttack, string animation, Vector2i damageRange, int cancelFrame, DamageType damageType = DamageType.Slash)
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
	CreatureAttack attack;

	List<Entity> hitEntities = new List<Entity>();


	public CreatureAttackAction(CreatureAttack attack)
		: base("attack")
	{
		this.attack = attack;

		animationName = attack.animation;
	}

	public override void update(Creature mob)
	{
		base.update(mob);

		if (inDamageRange)
		{
			//if (mob.rightWeapon != null)
			//	;
			//else
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
			}

			float toTarget = -(GameState.instance.player.position.xz - mob.position.xz).angle + MathF.PI * 0.5f;
			mob.yaw = MathHelper.LinearAngle(mob.yaw, toTarget, 3 * Time.deltaTime);
		}
	}

	public bool inDamageRange => elapsedTime >= attack.damageRange.x / 24.0f && elapsedTime < attack.damageRange.y / 24.0f;
}
