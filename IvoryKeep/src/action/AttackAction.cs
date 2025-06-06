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
	float lastProgress = 0;
	float lastActionMovement = 0;

	public bool useSoundPlayed = false;
	bool hitSoundPlayed = false;

	public int attackIdx { get; private set; }

	Vector2 lastTip;
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

	Vector2 getWeaponTip(Player player, float progress, float fract = 1.0f)
	{
		float currentAngle = getCurrentAngle(progress);

		bool flip = charDirection < 0;
		Vector2 position = new Vector2(currentRange * fract, 0);
		if (anim == AttackAnim.SwingSideways)
		{
			if (attackIdx % 2 == 0)
			{
				position = Vector2.Rotate(position, currentAngle);
				if (MathF.Abs(Vector2.Dot(direction, Vector2.Right)) > 0.9f)
					position *= new Vector2(1, 0.5f);
				else if (MathF.Abs(Vector2.Dot(direction, Vector2.Up)) > 0.9f)
					position *= new Vector2(0.5f, 1);
			}
			else
			{
				if (MathF.Abs(Vector2.Dot(direction, Vector2.Right)) > 0.9f)
				{
					position.y = position.x * currentAngle / (MathF.PI * 0.5f) * 0.25f;
					position.x = position.x * MathF.Cos(currentAngle);
				}
				else if (MathF.Abs(Vector2.Dot(direction, Vector2.Up)) > 0.9f)
				{
					position.y = position.x * MathF.Cos(currentAngle - MathF.PI * 0.5f);
					position.x = position.x * -(currentAngle - MathF.PI * 0.5f) / (MathF.PI * 0.5f) * 0.25f;
				}
				//if (MathF.Abs(Vector2.Dot(direction, Vector2.Right)) > 0.9f)
				//	position *= new Vector2(1, 0.5f * MathF.Exp(-2 * MathF.Max(0, 1 - MathF.Abs(currentAngle / MathF.PI / 0.5f))));
				//else if (MathF.Abs(Vector2.Dot(direction, Vector2.Up)) > 0.9f)
				//	position *= new Vector2(0.5f, 1);
			}
		}
		else
		{
			position = Vector2.Rotate(position, currentAngle);
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
			trail = new WeaponTrail(20, weapon.trailSprite, new Vector4(1, 1, 1, 0.5f), true, getWeaponTip(player, 0), getWeaponTip(player, 0, 0.99f));
	}

	public override void onFinished(Player player)
	{
	}

	void processHit(HitData hit, Player player)
	{
		Entity entity = hit.entity;
		if (hit.entity != null && hit.entity != player && hit.entity is Hittable && !hitEntities.Contains(hit.entity))
		{
			Hittable hittable = hit.entity as Hittable;

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
						Vector2 knockback = ((entity.position - player.position).normalized + Vector2.Up * 0.2f) * weapon.knockback;
						if (mob.isAlive)
							mob.addImpulse(knockback);
						else
							mob.corpse.addImpulse(knockback);
					}
				}

				if (hittable is Mob)
				{
					Mob mob = hittable as Mob;
					player.onEnemyHit(mob);
					for (int j = 0; j < player.items.Count; j++)
					{
						if (player.isEquipped(player.items[j]))
							player.items[j].onEnemyHit(player, mob, damage, critical);
					}
				}

				//if (!player.isGrounded)
				{
					Vector2 currentImpulse = new Vector2(player.impulseVelocity, player.velocity.y);
					if (Vector2.Dot(currentImpulse, -direction) < 4)
						player.addImpulse(-direction * (4 - Vector2.Dot(currentImpulse, -direction)));

					float downwardsFactor = MathF.Max(Vector2.Dot(direction, Vector2.Down), 0);
					player.velocity.y = MathF.Max(player.velocity.y, downwardsFactor * player.jumpPower * 0.75f);
				}

				if (weapon.hitSound != null && !hitSoundPlayed)
				{
					Audio.PlayOrganic(weapon.hitSound, new Vector3(hit.entity.position + hit.entity.collider.center, 0));
					hitSoundPlayed = true;
				}

				//if (critical)
				//	GameState.instance.level.addEntity(Effects.CreateCriticalEffect(), hit.entity.position + hit.entity.collider.center);
			}
		}
	}

	void checkHit(Vector2 origin, float progress, Player player)
	{
		Vector2 direction = getWorldDirection(progress);
		float currentAngle = getCurrentAngle(progress);

		int currentSpin = anim == AttackAnim.Stab ? 0 : (int)(progress * MathF.Abs(weapon.attackEndAngle - weapon.attackStartAngle) / MathF.PI / 2 - 0.001f);
		if (currentSpin > lastSpin)
		{
			hitEntities.Clear();
			lastSpin = currentSpin;
		}

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
			processHit(hits[i], player);

		if (lastTip != Vector2.Zero)
		{
			Vector2 dest = origin + maxRange * direction;
			Vector2 fromLast = dest - lastTip;
			hits = new HitData[16];
			numHits = GameState.instance.level.sweepNoBlock(lastTip, new FloatRect(-0.125f, -0.125f, 0.25f, 0.25f), fromLast.normalized, fromLast.length, hits, Entity.FILTER_MOB | Entity.FILTER_DEFAULT);
			for (int i = 0; i < numHits; i++)
				processHit(hits[i], player);
		}


		if (trail != null)
		{
			trail.update();
			float thickness = 0.9f;
			if (anim == AttackAnim.SwingSideways)
				thickness = MathHelper.Remap(Vector2.Dot(direction, Vector2.Rotate(Vector2.Right, currentAngle) * new Vector2(MathF.Sign(direction.x), 1)), -1, 1, 0.9f, 0.6f);
			else if (anim == AttackAnim.SwingOverhead)
				thickness = MathHelper.Remap(Vector2.Dot(direction, Vector2.Rotate(Vector2.Right, currentAngle) * new Vector2(MathF.Sign(direction.x), 1)), -1, 1, 0.99f, 0.7f);
			else if (anim == AttackAnim.Stab)
				thickness = 0.9f;
			trail.setPosition(getWeaponTip(player, progress), getWeaponTip(player, progress, thickness));
		}


		lastTip = origin + direction * maxRange;
	}

	public override void update(Player player)
	{
		base.update(player);

		Vector2 origin = getWorldOrigin(player);

		if (inDamageWindow)
		{
			int subSteps = 4;
			for (int i = 0; i < subSteps; i++)
			{
				checkHit(origin, MathHelper.Lerp(lastProgress, currentProgress, (i + 1) / (float)subSteps), player);
			}
			lastProgress = currentProgress;

			if (!useSoundPlayed && weapon.useSound != null)
			{
				Audio.PlayOrganic(weapon.useSound, new Vector3(player.position, 0), 1, attackRate * 0.25f);
				useSoundPlayed = true;
			}
		}

		if (inDamageWindow)
			player.direction = charDirection;
		speedMultiplier = player.isGrounded ? (currentProgress < 1 ? 0 : 0.5f) : 1;

		float actionMovementDst = MathHelper.Smoothstep(0, 1, currentProgress) * weapon.attackDashDistance;
		actionMovement = currentProgress < 1 && player.isGrounded ? player.direction * (actionMovementDst - lastActionMovement) / Time.deltaTime : 0;
		lastActionMovement = actionMovementDst;

		canJump = currentProgress >= 1;
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
			float value = MathF.Min(elapsedTime / duration * (1 + attackCooldown * 2), 1);
			value = 1 - MathF.Pow(1 - value, weapon.attackAcceleration * 2);
			return value;
		}
	}

	public bool inDamageWindow
	{
		get => elapsedTime / duration + elapsedTime / duration * attackCooldown < 1 + 0.25f * attackCooldown;
	}

	public float currentRange
	{
		get => (anim == AttackAnim.Stab ? currentProgress * attackRange : attackRange) + 1.0f;
	}

	public float getCurrentAngle(float progress)
	{
		if (anim == AttackAnim.Stab)
			return new Vector2(MathF.Abs(direction.x), direction.y).angle;
		else
		{
			progress = swingDir % 2 == 0 ? progress : 1 - progress;
			float angle = MathHelper.Lerp(new Vector2(MathF.Abs(direction.x), direction.y).angle + startAngle, new Vector2(MathF.Abs(direction.x), direction.y).angle + endAngle, progress);
			return angle;
		}
	}

	public Vector2 getWorldOrigin(Player player)
	{
		return player.position + new Vector2(0, /*Vector2.Dot(direction, Vector2.Down) > 0.9f ? 0 : Vector2.Dot(direction, Vector2.Up) > 0.9f ? 1 : */player.getWeaponOrigin(mainHand).y);
	}

	public Vector2 getWorldDirection(float progress)
	{
		if (progress == -1)
			progress = currentProgress;

		float currentAngle = getCurrentAngle(progress);
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

	public override Matrix getItemTransform(Player player, bool mainHand)
	{
		float rotation = getCurrentAngle(currentProgress);
		bool flip = charDirection < 0;
		Matrix weaponTransform = Matrix.CreateTranslation(currentRange - 0.5f * weapon.size.x, 0, 0);
		if (swingDir % 2 == 1)
			weaponTransform = Matrix.CreateRotation(Vector3.UnitX, MathF.PI) * weaponTransform;
		if (anim == AttackAnim.SwingSideways)
		{
			if (attackIdx % 2 == 0)
			{
				weaponTransform = Matrix.CreateRotation(Vector3.UnitZ, rotation) * weaponTransform;
				if (MathF.Abs(Vector2.Dot(direction, Vector2.Right)) > 0.9f)
					weaponTransform.translation *= new Vector3(1, 0.5f, 1);
				else if (MathF.Abs(Vector2.Dot(direction, Vector2.Up)) > 0.9f)
					weaponTransform.translation *= new Vector3(0.5f, 1, 1);
			}
			else
			{
				if (MathF.Abs(Vector2.Dot(direction, Vector2.Right)) > 0.9f)
				{
					weaponTransform = Matrix.CreateRotation(Vector3.UnitZ, -rotation) * weaponTransform;
					weaponTransform = Matrix.CreateScale(1, -0.5f, 1) * weaponTransform;
				}
				else if (MathF.Abs(Vector2.Dot(direction, Vector2.Up)) > 0.9f)
				{
					weaponTransform = Matrix.CreateRotation(Vector3.UnitZ, -(rotation - 0.5f * MathF.PI) + 0.5f * MathF.PI) * weaponTransform;
					weaponTransform = Matrix.CreateScale(-0.5f, 1, 1) * weaponTransform;
				}
			}
		}
		else
		{
			weaponTransform = Matrix.CreateRotation(Vector3.UnitZ, rotation) * weaponTransform;
		}
		weaponTransform = Matrix.CreateTranslation(player.getWeaponOrigin(mainHand).x, Vector2.Dot(direction, Vector2.Down) > 0.9f ? 0 : Vector2.Dot(direction, Vector2.Up) > 0.9f ? 1 : player.getWeaponOrigin(mainHand).y, 0) * weaponTransform;
		if (flip)
			weaponTransform = Matrix.CreateRotation(Vector3.UnitY, MathF.PI) * weaponTransform;
		return weaponTransform;
	}
}
