using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class HomingSpell : Spell
{
	protected int bulletCount = 3;

	Player player;
	Item staff;
	float duration;

	float castTime = -1;
	int castedProjectiles = 0;


	public HomingSpell()
		: base("homing_stars")
	{
		displayName = "Homing Stars";
		description = "Summons hovering stars that linger briefly, then surge forward in the direction faced.";

		baseValue = 49;

		baseDamage = 0.8f;
		baseAttackRate = 1;
		baseAttackRange = 10;
		manaCost = 0.1f;
		knockback = 1.0f;
		trigger = false;
		canCastWithoutMana = true;

		spellIcon = new Sprite(tileset, 12, 10);
	}

	public override void upgrade()
	{
		base.upgrade();
		bulletCount++;
	}

	public override bool cast(Player player, Item staff, float manaCost, float duration)
	{
		this.player = player;
		this.staff = staff;
		this.duration = duration;

		castTime = Time.gameTime;
		castedProjectiles = 0;

		return true;
	}

	public void shoot()
	{
		//for (int i = 0; i < bulletCount; i++)
		{
			Vector2 position = player.center;
			Vector2 offset = Vector2.Zero; // new Vector2(player.direction * 0.3f, 0.0f);

			float coneSize = MathF.PI / player.getAccuracyModifier();
			float angle = player.direction * Random.Shared.NextSingle() * coneSize; //(i / (float)(bulletCount - 1) - 0.5f) * coneSize;
			Vector2 direction = bulletCount > 1 ? Vector2.Rotate(player.lookDirection.normalized, angle) : player.lookDirection.normalized;
			Vector2 inaccuracy = Mathf.RandomPointOnCircle(Random.Shared) * 0.08f;
			direction = (direction + inaccuracy / (staff.accuracy * player.getAccuracyModifier())).normalized;

			HomingProjectile projectile = new HomingProjectile(direction, player.velocity, offset, player, this, staff);
			if (player.mana < manaCost)
			{
				projectile.maxRange *= 0.5f;
				projectile.damage *= 0.5f;
				projectile.spriteColor.w = 0.5f;
			}

			GameState.instance.level.addEntity(projectile, position);
			GameState.instance.level.addEntity(new MagicProjectileCastEffect(player), position + offset);

			Audio.PlayOrganic(useSound, new Vector3(player.position, 0));
		}
	}

	public override void update(Entity entity)
	{
		base.update(entity);

		if (castTime != -1)
		{
			float elapsed = Time.gameTime - castTime;
			int projectilesShouldCast = Math.Min((int)(elapsed * 3 * bulletCount * attackRate) + 1, bulletCount);
			if (castedProjectiles < projectilesShouldCast)
			{
				shoot();
				castedProjectiles++;
			}
		}
	}
}
