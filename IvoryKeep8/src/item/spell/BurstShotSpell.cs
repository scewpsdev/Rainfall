using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BurstShotSpell : Spell
{
	Player player;
	Item staff;
	float duration;

	int bulletCount = 3;

	long castTime = -1;
	int castedProjectiles = 0;
	bool noMana = false;

	public BurstShotSpell()
		: base("burst_shot")
	{
		displayName = "Burst Shot";
		description = "Releases three rapid charges of arcane force.";

		baseValue = 17;

		baseDamage = 0.7f;
		baseAttackRate = 1;
		baseAttackRange = 5;
		manaCost = 0.3f;
		knockback = 1.0f;
		trigger = false;
		needsCharging = false;
		canCastWithoutMana = true;

		spellIcon = new Sprite(tileset, 4, 7);
	}

	public override void upgrade()
	{
		base.upgrade();
		bulletCount++;
	}

	/*
	public override bool charge(Player player, Item staff, float manaCost, float duration)
	{
		this.player = player;
		this.staff = staff;
		this.duration = duration;

		castTime = Time.currentTime;
		castedProjectiles = 0;

		return true;
	}
	*/

	public override bool cast(Player player, Item staff, float manaCost, float duration)
	{
		this.player = player;
		this.staff = staff;
		this.duration = duration;

		castTime = Time.currentTime;
		castedProjectiles = 0;
		noMana = player.mana < manaCost;

		return true;
	}

	void shoot()
	{
		Vector2 position = player.center;
		Vector2 offset = Vector2.Zero;

		Vector2 direction = player.lookDirection.normalized;
		Vector2 inaccuracy = Mathf.RandomPointOnCircle(Random.Shared) * 0.08f;
		direction = (direction + inaccuracy / (staff.accuracy * player.getAccuracyModifier())).normalized;

		MagicProjectile projectile = new MagicProjectile(direction, player.velocity, offset, player, this, staff);
		if (noMana)
		{
			projectile.maxRange *= 0.5f;
			projectile.damage *= 0.5f;
			projectile.spriteColor.w = 0.5f;
		}

		GameState.instance.level.addEntity(projectile, position);
		GameState.instance.level.addEntity(new MagicProjectileCastEffect(player), position + offset);

		Audio.PlayOrganic(useSound, new Vector3(player.position, 0));
	}

	public override void update(Entity entity)
	{
		base.update(entity);

		if (castTime != -1)
		{
			float elapsed = (Time.currentTime - castTime) / 1e9f;
			int projectilesShouldCast = Math.Min((int)(elapsed * 3 * bulletCount * attackRate) + 1, bulletCount);
			if (castedProjectiles < projectilesShouldCast)
			{
				shoot();
				castedProjectiles++;
			}
		}
	}
}
