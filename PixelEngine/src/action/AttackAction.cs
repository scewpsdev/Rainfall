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
	public Vector2 direction;
	public int charDirection;
	public AttackAnim anim;
	public int swingDir;
	float attackDamage;
	float attackRange;
	float attackRate;
	float startAngle, endAngle;

	public List<Entity> hitEntities = new List<Entity>();
	float maxRange = 0;

	public bool useSoundPlayed = false;
	bool hitSoundPlayed = false;

	public int attackIdx = 0;

	int lastSpin = 0;

	Trail trail;
	Trail secondaryTrail;


	public AttackAction(Item weapon, bool mainHand, AttackAnim anim, float attackRate, float attackDamage, float attackRange, float startAngle, float endAngle)
		: base("attack", mainHand)
	{
		this.weapon = weapon;
		this.anim = anim;
		this.duration = 1 / attackRate;
		this.attackRate = attackRate;
		this.attackDamage = attackDamage;
		this.attackRange = attackRange;
		this.startAngle = startAngle;
		this.endAngle = endAngle;

		renderWeapon = true;

		postActionLinger = 0.25f;
	}

	public AttackAction(Item weapon, bool mainHand, AttackAnim anim, float attackRate, float attackDamage, float attackRange)
		: this(weapon, mainHand, anim, attackRate, attackDamage, attackRange, weapon.attackAngleOffset + weapon.attackAngle, weapon.attackAngleOffset)
	{
	}

	public AttackAction(Item weapon, bool mainHand)
		: this(weapon, mainHand, weapon.anim, weapon.attackRate, weapon.attackDamage, weapon.attackRange, weapon.attackAngleOffset + weapon.attackAngle, weapon.attackAngleOffset)
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
		position += new Vector2(0, player.getWeaponOrigin(mainHand).y);
		if (flip)
			position.x *= -1;
		position += player.position;
		return position;
	}

	public override void onStarted(Player player)
	{
		duration /= player.getAttackSpeedModifier();

		direction = player.lookDirection.normalized;
		if (MathF.Abs(direction.x) < 0.001f)
			direction.x = 0;
		charDirection = direction.x != 0 ? MathF.Sign(direction.x) : player.direction;

		if (anim != AttackAnim.Stab)
		{
			trail = new Trail(20, Vector4.One, getWeaponTip(player));
			secondaryTrail = new Trail(20, new Vector4(1, 1, 1, 0.5f), getWeaponTip(player, 0.9f));
		}
	}

	public override void onFinished(Player player)
	{
	}

	public override void update(Player player)
	{
		base.update(player);

		if (inDamageWindow)
		{
			int currentSpin = anim == AttackAnim.Stab ? 0 : (int)(currentProgress * weapon.attackAngle / MathF.PI / 2 - 0.001f);
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
							Vector2 knockback = (entity.position - (player.position + player.collider.center)).normalized * weapon.knockback;
							mob.addImpulse(knockback);
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
							Vector2 knockback = -direction * 4;
							if (Vector2.Dot(player.impulseVelocity, -direction) < 4)
								player.addImpulse(knockback);

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

		if (anim != AttackAnim.Stab)
		{
			trail.update();
			secondaryTrail.update();
			if (inDamageWindow)
			{
				trail.setPosition(getWeaponTip(player));
				secondaryTrail.setPosition(getWeaponTip(player, MathF.Abs(currentProgress - 0.5f) + 0.4f));
			}
		}

		if (inDamageWindow)
			player.direction = charDirection;
		speedMultiplier = inDamageWindow && player.isGrounded ? weapon.actionMovementSpeed : 1;
		//actionMovement = inDamageWindow && player.isGrounded ? MathF.Sign(direction.x) * (1 - elapsedTime / (duration / (1 + weapon.attackCooldown))) * 4 : 0;
	}

	public override void render(Player player)
	{
		if (anim != AttackAnim.Stab)
		{
			//trail.render();
			//secondaryTrail.render();

			for (int i = 0; i < trail.points.Length - 1; i++)
			{
				Vector2 v0 = trail.points[i];
				Vector2 v1 = trail.points[i + 1];
				Vector2 v2 = secondaryTrail.points[i];
				Vector2 v3 = secondaryTrail.points[i + 1];
				v2 = Vector2.Lerp(v2, v0, i / (float)(trail.points.Length - 1));
				v3 = Vector2.Lerp(v3, v1, (i + 1) / (float)(trail.points.Length - 1));
				float alpha = 1 - i / (float)(trail.points.Length - 1);
				//alpha = alpha * alpha;
				Renderer.DrawSpriteEx(new Vector3(v2, 0), new Vector3(v0, 0), new Vector3(v1, 0), new Vector3(v3, 0), null, false, new Vector4(1, 1, 1, alpha));
			}
		}
	}

	public float currentProgress
	{
		get
		{
			float value = MathF.Min(elapsedTime / duration * (1 + weapon.attackCooldown), 1);
			value = 1 - MathF.Pow(1 - value, weapon.attackAcceleration);
			return value;
		}
	}

	public bool inDamageWindow
	{
		get => elapsedTime / duration + elapsedTime / duration * weapon.attackCooldown < 1 + 0.25f * weapon.attackCooldown;
	}

	public float currentRange
	{
		get => (anim == AttackAnim.Stab ? currentProgress * attackRange : attackRange) + 0.25f;
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

	public override Matrix getItemTransform(Player player)
	{
		float rotation = currentAngle;
		bool flip = charDirection < 0;
		Matrix weaponTransform = Matrix.CreateTranslation(currentRange - 0.5f * weapon.size.x, 0, 0);
		weaponTransform = Matrix.CreateRotation(Vector3.UnitZ, rotation) * weaponTransform;
		if (anim == AttackAnim.SwingSideways)
		{
			if (MathF.Abs(Vector2.Dot(direction, Vector2.Right)) > 0.9f)
				weaponTransform.translation *= new Vector3(1, 0.5f, 1);
			else if (MathF.Abs(Vector2.Dot(direction, Vector2.Up)) > 0.9f)
				weaponTransform.translation *= new Vector3(0.5f, 1, 1);
		}
		weaponTransform = Matrix.CreateTranslation(0, player.getWeaponOrigin(mainHand).y, 0) * weaponTransform;
		if (flip)
			weaponTransform = Matrix.CreateRotation(Vector3.UnitY, MathF.PI) * weaponTransform;
		return weaponTransform;
	}
}
