using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;


public class AttackAction : EntityAction
{
	public Item weapon;
	public Item powerstancedWeapon;
	public Vector2 direction;
	public int charDirection;
	public AttackAnim anim;
	public int swingDir;
	float attackDamage;
	public float attackRange;
	float attackRate;
	float startAngle, endAngle;
	public float attackCooldown;

	public List<Entity> hitEntities = new List<Entity>();
	float maxRange = 0;

	public bool useSoundPlayed = false;
	bool hitSoundPlayed = false;

	public int attackIdx { get; private set; }

	int lastSpin = 0;

	WeaponTrail trail;


	public AttackAction(Item weapon, bool mainHand, AttackAnim anim, int attackIdx, float attackRate, float attackDamage, float attackRange, float startAngle, float endAngle, Item powerstancedWeapon = null)
		: base("attack", mainHand)
	{
		this.weapon = weapon;
		this.anim = anim;
		this.attackIdx = attackIdx;
		this.attackRate = attackRate;
		this.attackDamage = attackDamage;
		this.attackRange = attackRange;
		this.startAngle = startAngle;
		this.endAngle = endAngle;
		this.powerstancedWeapon = powerstancedWeapon;

		duration = 1 / attackRate;
		attackCooldown = weapon.attackCooldown;

		if (powerstancedWeapon != null && mainHand)
		{
			if (attackIdx % 2 == 0)
				setRenderWeapon(mainHand, !weapon.customAttackRender ? weapon : null);
			else
				setRenderWeapon(!mainHand, powerstancedWeapon);
		}
		else if (powerstancedWeapon != null && !mainHand)
		{
			setRenderWeapon(mainHand, !weapon.customAttackRender ? weapon : null);
			setRenderWeapon(!mainHand, powerstancedWeapon);
		}
		else
		{
			setRenderWeapon(mainHand, !weapon.customAttackRender ? weapon : null);
		}

		postActionLinger = weapon.postAttackLinger;
	}

	public AttackAction(Item weapon, bool mainHand, AttackAnim anim, int attackIdx, float attackRate, float attackDamage, float attackRange)
		: this(weapon, mainHand, anim, attackIdx, attackRate, attackDamage, attackRange, weapon.attackStartAngle, weapon.attackEndAngle)
	{
	}

	public AttackAction(Item weapon, bool mainHand, Player player)
		: this(weapon, mainHand, weapon.anim, 0, weapon.attackRate, weapon.getAttackDamage(player), weapon.attackRange, weapon.attackStartAngle, weapon.attackEndAngle)
	{
	}

	Vector2 getWeaponTip(Player player, float fract = 1.0f)
	{
		bool flip = charDirection < 0;
		Vector2 position = new Vector2(0.5f * weapon.size.x, 0);
		position += new Vector2(currentRange * fract - 0.5f * weapon.size.x, 0);
		position = Vector2.Rotate(position, currentAngle);
		if (anim == AttackAnim.SwingSideways)
		{
			if (MathF.Abs(Vector2.Dot(direction, Vector2.Right)) > 0.9f)
				position *= new Vector2(1, 0.5f);
			else if (MathF.Abs(Vector2.Dot(direction, Vector2.Up)) > 0.9f)
				position *= new Vector2(0.5f, 1);
		}
		position += new Vector2(0, Vector2.Dot(direction, Vector2.Down) > 0.9f ? 0 : Vector2.Dot(direction, Vector2.Up) > 0.9f ? 1 : player.getWeaponOrigin(mainHand).y);
		if (flip)
			position.x *= -1;
		return position;
	}

	public override void onStarted(Player player)
	{
		direction = player.lookDirection.normalized;
		if (MathF.Abs(direction.x) < 0.001f)
			direction.x = 0;
		charDirection = direction.x != 0 ? MathF.Sign(direction.x) : player.direction;

		duration /= player.getAttackSpeedModifier();

		if (anim != AttackAnim.Stab)
			trail = new WeaponTrail(20, null, Vector4.One, true, getWeaponTip(player), getWeaponTip(player, 0.99f));
	}

	public override void onFinished(Player player)
	{
	}

	public override void update(Player player)
	{
		base.update(player);

		if (inDamageWindow)
		{
			int currentSpin = anim == AttackAnim.Stab ? 0 : (int)(currentProgress * MathF.Abs(weapon.attackEndAngle - weapon.attackStartAngle) / MathF.PI / 2 - 0.001f);
			if (currentSpin > lastSpin)
			{
				hitEntities.Clear();
				lastSpin = currentSpin;
			}

			Vector2 origin = getWorldOrigin(player);
			Vector2 direction = worldDirection;

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
			int numHits = GameState.instance.level.sweepNoBlock(origin, new FloatRect(-0.125f, -0.125f, 0.25f, 0.25f), direction, maxRange, hits, Entity.FILTER_MOB | Entity.FILTER_DEFAULT);
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
							|| (mob.ai == null || mob.ai.target != player) && player.getStealthAttackModifier() > 1.5f;
					}
					if (critical)
						damage *= player.getCriticalAttackModifier();

					if (hittable.hit(damage, player, weapon, null, true, critical))
					{
						hitEntities.Add(entity);

						if (entity is Mob)
						{
							Mob mob = entity as Mob;
							if (damage > mob.poise)
							{
								Vector2 knockback = (entity.position - (player.position + player.collider.center)).normalized * weapon.knockback;
								mob.addImpulse(knockback);
							}
						}

						if (hittable is Mob)
						{
							Mob mob = hittable as Mob;
							for (int j = 0; j < player.items.Count; j++)
							{
								if (player.isEquipped(player.items[j]))
									player.items[j].onEnemyHit(player, mob, damage, critical);
							}
						}

						if (!player.isGrounded)
						{
							Vector2 currentImpulse = new Vector2(player.impulseVelocity, player.velocity.y);
							if (Vector2.Dot(currentImpulse, -direction) < 4)
								player.addImpulse(-direction * (4 - Vector2.Dot(currentImpulse, -direction)));

							float downwardsFactor = MathF.Max(Vector2.Dot(direction, Vector2.Down), 0);
							player.velocity.y = MathF.Max(player.velocity.y, downwardsFactor * player.jumpPower * 0.75f);
						}

						if (weapon.hitSound != null && !hitSoundPlayed)
						{
							Audio.PlayOrganic(weapon.hitSound, new Vector3(hits[i].entity.position + hits[i].entity.collider.center, 0));
							hitSoundPlayed = true;
						}

						//if (critical)
						//	GameState.instance.level.addEntity(Effects.CreateCriticalEffect(), hits[i].entity.position + hits[i].entity.collider.center);
					}
				}
			}

			if (!useSoundPlayed && weapon.useSound != null)
			{
				Audio.PlayOrganic(weapon.useSound, new Vector3(player.position, 0), 1, attackRate * 0.25f);
				useSoundPlayed = true;
			}
		}

		if (trail != null)
		{
			trail.update();
			if (inDamageWindow)
			{
				float thickness = 0.9f;
				if (anim == AttackAnim.SwingSideways)
					thickness = MathHelper.Remap(Vector2.Dot(direction, Vector2.Rotate(Vector2.Right, currentAngle) * new Vector2(MathF.Sign(direction.x), 1)), -1, 1, 0.9f, 0.5f);
				else if (anim == AttackAnim.SwingOverhead)
					thickness = MathHelper.Remap(Vector2.Dot(direction, Vector2.Rotate(Vector2.Right, currentAngle) * new Vector2(MathF.Sign(direction.x), 1)), -1, 1, 0.99f, 0.9f);
				else if (anim == AttackAnim.Stab)
					thickness = 0.9f;
				trail.setPosition(getWeaponTip(player), getWeaponTip(player, thickness));
			}
		}

		if (inDamageWindow)
			player.direction = charDirection;
		speedMultiplier = inDamageWindow && player.isGrounded ? weapon.actionMovementSpeed : 1;
		//actionMovement = inDamageWindow && player.isGrounded ? MathF.Sign(direction.x) * (1 - elapsedTime / (duration / (1 + attackCooldown))) * 4 : 0;
	}

	public override void render(Player player)
	{
		if (trail != null)
		{
			trail.render(player.position);
		}
	}

	public float currentProgress
	{
		get
		{
			float value = MathF.Min(elapsedTime / duration * (1 + attackCooldown), 1);
			value = 1 - MathF.Pow(1 - value, weapon.attackAcceleration);
			return value;
		}
	}

	public bool inDamageWindow
	{
		get => elapsedTime / duration + elapsedTime / duration * attackCooldown < 1 + 0.25f * attackCooldown;
	}

	public float currentRange
	{
		get => (anim == AttackAnim.Stab ? currentProgress * attackRange : attackRange) + 0.5f;
	}

	public float currentAngle
	{
		get
		{
			if (anim == AttackAnim.Stab)
				return new Vector2(MathF.Abs(direction.x), direction.y).angle;
			else
			{
				float progress = swingDir % 2 == 0 ? currentProgress : 1 - currentProgress;
				float angle = MathHelper.Lerp(new Vector2(MathF.Abs(direction.x), direction.y).angle + startAngle, new Vector2(MathF.Abs(direction.x), direction.y).angle + endAngle, progress);
				return angle;
			}
		}
	}

	public Vector2 getWorldOrigin(Player player)
	{
		return player.position + new Vector2(0, Vector2.Dot(direction, Vector2.Down) > 0.9f ? 0 : Vector2.Dot(direction, Vector2.Up) > 0.9f ? 1 : player.getWeaponOrigin(mainHand).y);
	}

	public Vector2 worldDirection
	{
		get
		{
			Vector2 direction = new Vector2(MathF.Cos(currentAngle) * charDirection, MathF.Sin(currentAngle));
			if (anim == AttackAnim.SwingSideways)
			{
				if (MathF.Abs(Vector2.Dot(direction, Vector2.Right)) > 0.9f)
					direction.y *= 0.5f;
				else if (MathF.Abs(Vector2.Dot(direction, Vector2.Up)) > 0.9f)
					direction.x *= 0.5f;
			}
			return direction;
		}
	}

	public override Matrix getItemTransform(Player player, bool mainHand)
	{
		float rotation = currentAngle;
		bool flip = charDirection < 0;
		Matrix weaponTransform = Matrix.CreateTranslation(currentRange - 0.5f * weapon.size.x, 0, 0);
		if (swingDir % 2 == 1)
			weaponTransform = Matrix.CreateRotation(Vector3.UnitX, MathF.PI) * weaponTransform;
		weaponTransform = Matrix.CreateRotation(Vector3.UnitZ, rotation) * weaponTransform;
		if (anim == AttackAnim.SwingSideways)
		{
			if (MathF.Abs(Vector2.Dot(direction, Vector2.Right)) > 0.9f)
				weaponTransform.translation *= new Vector3(1, 0.5f, 1);
			else if (MathF.Abs(Vector2.Dot(direction, Vector2.Up)) > 0.9f)
				weaponTransform.translation *= new Vector3(0.5f, 1, 1);
		}
		weaponTransform = Matrix.CreateTranslation(player.getWeaponOrigin(mainHand).x, Vector2.Dot(direction, Vector2.Down) > 0.9f ? 0 : Vector2.Dot(direction, Vector2.Up) > 0.9f ? 1 : player.getWeaponOrigin(mainHand).y, 0) * weaponTransform;
		if (flip)
			weaponTransform = Matrix.CreateRotation(Vector3.UnitY, MathF.PI) * weaponTransform;
		return weaponTransform;
	}
}
