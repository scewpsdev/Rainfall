using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;


public class AttackAction : EntityAction
{
	public Item weapon;
	public Vector2 direction;
	public float startAngle;
	public bool stab;
	float attackDamage;
	float attackRange;
	float attackRate;

	public List<Entity> hitEntities = new List<Entity>();

	public bool soundPlayed = false;


	public AttackAction(Item weapon, bool mainHand, bool stab, float attackRate, float attackDamage, float attackRange)
		: base("attack", mainHand)
	{
		this.weapon = weapon;
		this.stab = stab;
		this.duration = 1 / attackRate;
		this.attackRate = attackRate;
		this.attackDamage = attackDamage;
		this.attackRange = attackRange;
	}

	public AttackAction(Item weapon, bool mainHand)
		: this(weapon, mainHand, weapon.stab, weapon.attackRate, weapon.attackDamage, weapon.attackRange)
	{
	}

	public override void onStarted(Player player)
	{
		duration /= player.getAttackSpeedModifier();

		direction = player.lookDirection.normalized;
		startAngle = new Vector2(MathF.Abs(direction.x), direction.y).angle;
	}

	public override void update(Player player)
	{
		base.update(player);

		if (inDamageWindow)
		{
			Vector2 origin = player.position + new Vector2(0, player.getWeaponOrigin(mainHand).y);
			Vector2 direction = new Vector2(MathF.Cos(currentAngle) * MathF.Sign(this.direction.x), MathF.Sin(currentAngle));
			Span<HitData> hits = new HitData[16];
			int numHits = GameState.instance.level.sweepNoBlock(origin, new FloatRect(-0.1f, -0.1f, 0.2f, 0.2f), direction, currentRange, hits, Entity.FILTER_MOB | Entity.FILTER_DEFAULT);
			for (int i = 0; i < numHits; i++)
			{
				Entity entity = hits[i].entity;
				if (hits[i].entity != null && hits[i].entity != player && hits[i].entity is Hittable && !hitEntities.Contains(hits[i].entity))
				{
					Hittable hittable = hits[i].entity as Hittable;

					float damage = attackDamage * player.getMeleeDamageModifier();

					bool critical = false;
					if (entity is Mob)
					{
						Mob mob = entity as Mob;
						critical = mob.isStunned && mob.criticalStun
							|| Random.Shared.NextSingle() < player.criticalChance * player.getCriticalChanceModifier()
							|| mob.ai.target != player && player.getStealthAttackModifier() > 1.5f;
					}
					if (critical)
						damage *= 2 * player.getCriticalAttackModifier();

					if (hittable.hit(damage, player, weapon, null, true, critical))
					{
						hitEntities.Add(entity);

						if (entity is Mob)
						{
							Mob mob = entity as Mob;
							Vector2 knockback = (entity.position - player.position).normalized * weapon.knockback;
							mob.addImpulse(knockback);
						}

						if (hittable is Mob)
						{
							Mob mob = hittable as Mob;
							for (int j = 0; j < player.items.Count; j++)
							{
								if (player.isEquipped(player.items[j]))
									player.items[j].onEnemyHit(player, mob, damage);
							}
						}

						player.addImpulse(-direction * 4);
						if (!player.isGrounded)
						{
							float downwardsFactor = MathF.Max(Vector2.Dot(direction, Vector2.Down), 0);
							player.velocity.y = MathF.Max(player.velocity.y, downwardsFactor * player.jumpPower);
						}

						if (weapon.hitSound != null)
							Audio.PlayOrganic(weapon.hitSound, new Vector3(hits[i].entity.position + hits[i].entity.collider.center, 0));

						//if (critical)
						//	GameState.instance.level.addEntity(Effects.CreateCriticalEffect(), hits[i].entity.position + hits[i].entity.collider.center);
					}
				}
			}

			if (!soundPlayed)
			{
				Audio.PlayOrganic(weapon.useSound, new Vector3(player.position, 0), 1, attackRate * 0.25f);
				soundPlayed = true;
			}
		}
	}

	public float currentProgress
	{
		get => MathF.Min(elapsedTime / duration * (1 + weapon.attackCooldown), 1);
	}

	public bool inDamageWindow
	{
		get => elapsedTime / duration + elapsedTime / duration * weapon.attackCooldown < 1 + 0.25f * weapon.attackCooldown;
	}

	public float currentRange
	{
		get => stab ? currentProgress * attackRange : attackRange;
	}

	public float currentAngle
	{
		get => stab ? startAngle : startAngle + (1 - currentProgress) * weapon.attackAngle + weapon.attackAngleOffset;
	}

	public override Matrix getItemTransform(Player player)
	{
		float rotation = currentAngle;
		bool flip = direction.x < 0;
		Matrix weaponTransform = Matrix.CreateRotation(Vector3.UnitZ, rotation)
			* Matrix.CreateTranslation(currentRange - 0.5f * weapon.size.x, 0, 0)
			* Matrix.CreateRotation(Vector3.UnitZ, weapon.attackRotationOffset);
		if (flip)
			weaponTransform = Matrix.CreateRotation(Vector3.UnitY, MathF.PI) * weaponTransform;
		weaponTransform = Matrix.CreateTranslation(0, player.getWeaponOrigin(mainHand).y + player.getWeaponOrigin(mainHand).z, player.getWeaponOrigin(mainHand).z) * weaponTransform;
		return weaponTransform;
	}
}
