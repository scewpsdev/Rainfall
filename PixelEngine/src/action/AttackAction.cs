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
	public int charDirection;
	public bool stab;
	public int swingDir;
	float attackDamage;
	float attackRange;
	float attackRate;
	float startAngle, endAngle;

	public List<Entity> hitEntities = new List<Entity>();
	public bool soundPlayed = false;
	float maxRange = 0;

	public int attackIdx = 0;

	int lastSpin = 0;


	public AttackAction(Item weapon, bool mainHand, bool stab, float attackRate, float attackDamage, float attackRange, float startAngle, float endAngle)
		: base("attack", mainHand)
	{
		this.weapon = weapon;
		this.stab = stab;
		this.duration = 1 / attackRate;
		this.attackRate = attackRate;
		this.attackDamage = attackDamage;
		this.attackRange = attackRange;
		this.startAngle = startAngle;
		this.endAngle = endAngle;

		postActionLinger = 0.25f;
	}

	public AttackAction(Item weapon, bool mainHand, bool stab, float attackRate, float attackDamage, float attackRange)
		: this(weapon, mainHand, stab, attackRate, attackDamage, attackRange, weapon.attackAngleOffset + weapon.attackAngle, weapon.attackAngleOffset)
	{
	}

	public AttackAction(Item weapon, bool mainHand)
		: this(weapon, mainHand, weapon.stab, weapon.attackRate, weapon.attackDamage, weapon.attackRange, weapon.attackAngleOffset + weapon.attackAngle, weapon.attackAngleOffset)
	{
	}

	public override void onQueued(Player player)
	{
		duration /= player.getAttackSpeedModifier();

		direction = player.lookDirection.normalized;
		charDirection = MathF.Abs(player.lookDirection.x) > 0.001f ? MathF.Sign(player.lookDirection.x) : player.direction;
	}

	public override void onFinished(Player player)
	{
	}

	public override void update(Player player)
	{
		base.update(player);

		if (inDamageWindow)
		{
			int currentSpin = (int)(currentProgress * weapon.attackAngle / MathF.PI / 2);
			if (currentSpin > lastSpin)
			{
				hitEntities.Clear();
				lastSpin = currentSpin;
			}

			Vector2 origin = player.position + new Vector2(0, player.getWeaponOrigin(mainHand).y);
			Vector2 direction = new Vector2(MathF.Cos(currentAngle) * charDirection, MathF.Sin(currentAngle));

			HitData tileHit = GameState.instance.level.raycastTiles(origin, direction, currentRange);
			if (tileHit != null)
				maxRange = MathF.Max(maxRange, tileHit.distance);
			else
				maxRange = MathF.Max(maxRange, currentRange);

			HitData tileHit2 = GameState.instance.level.hitTiles(origin + direction * currentRange);
			if (tileHit2 != null)
			{
				Debug.Assert(tileHit != null);
				maxRange = tileHit.distance;
			}

			Span<HitData> hits = new HitData[16];
			int numHits = GameState.instance.level.sweepNoBlock(origin, new FloatRect(-0.1f, -0.1f, 0.2f, 0.2f), direction, maxRange, hits, Entity.FILTER_MOB | Entity.FILTER_DEFAULT);
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
							|| Random.Shared.NextSingle() < player.criticalChance * weapon.criticalChanceModifier * player.getCriticalChanceModifier()
							|| mob.ai.target != player && player.getStealthAttackModifier() > 1.5f;
					}
					if (critical)
						damage *= player.getCriticalAttackModifier();

					if (hittable.hit(damage, player, weapon, null, true, critical))
					{
						hitEntities.Add(entity);

						if (entity is Mob)
						{
							Mob mob = entity as Mob;
							Vector2 knockback = (entity.position - (player.position + player.collider.center)).normalized * weapon.knockback;
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

			if (!soundPlayed && weapon.useSound != null)
			{
				Audio.PlayOrganic(weapon.useSound, new Vector3(player.position, 0), 1, attackRate * 0.25f);
				soundPlayed = true;
			}
		}
	}

	public float currentProgress
	{
		get
		{
			float value = MathF.Min(elapsedTime / duration * (1 + weapon.attackCooldown), 1);
			value = 1 - MathF.Pow(1 - value, 2);
			//value = value < 0.5f ? MathF.Pow(value, 3) * 4 : 1 - MathF.Pow(1 - value, 3) * 4;
			return value;
		}
	}

	public bool inDamageWindow
	{
		get => elapsedTime / duration + elapsedTime / duration * weapon.attackCooldown < 1 + 0.25f * weapon.attackCooldown;
	}

	public float currentRange
	{
		get => stab ? currentProgress * attackRange : attackRange * 1.2f;
	}

	public float currentAngle
	{
		get
		{
			if (stab)
				return new Vector2(MathF.Abs(direction.x), direction.y).angle;
			else
			{
				float progress = swingDir % 2 == 0 ? currentProgress : 1 - currentProgress;
				float angle = MathHelper.Lerp(new Vector2(MathF.Abs(direction.x), direction.y).angle + startAngle, new Vector2(MathF.Abs(direction.x), direction.y).angle + endAngle, progress);
				return angle;
			}
		}
	}

	public override Matrix getItemTransform(Player player)
	{
		float rotation = currentAngle;
		bool flip = charDirection < 0;
		Matrix weaponTransform = Matrix.CreateTranslation(0, player.getWeaponOrigin(mainHand).y, 0)
			* Matrix.CreateRotation(Vector3.UnitZ, rotation)
			* Matrix.CreateTranslation(currentRange - 0.5f * weapon.size.x, 0, 0)
			* Matrix.CreateRotation(Vector3.UnitZ, weapon.attackRotationOffset);
		if (flip)
			weaponTransform = Matrix.CreateRotation(Vector3.UnitY, MathF.PI) * weaponTransform;
		return weaponTransform;
	}
}
