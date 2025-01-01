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
	float speed;

	long castTime = -1;
	int castedProjectiles = 0;

	public BurstShotSpell()
		: base("burst_shot_spell")
	{
		displayName = "Burst Shot";

		value = 17;

		baseDamage = 0.7f;
		baseAttackRate = 1;
		manaCost = 0.3f;
		knockback = 1.0f;
		trigger = false;

		spellIcon = new Sprite(tileset, 4, 7);

		useSound = Resource.GetSounds("sounds/cast", 3);
	}

	public override bool cast(Player player, Item staff, float manaCost, float duration)
	{
		this.player = player;
		this.staff = staff;
		this.speed = duration * attackRate;
		castTime = Time.currentTime;
		castedProjectiles = 0;

		return true;
	}

	void shoot()
	{
		Vector2 position = player.position + new Vector2(0.0f, 0.5f);
		Vector2 offset = new Vector2(player.direction * 0.5f, -0.1f);

		Vector2 direction = player.lookDirection.normalized;
		Vector2 inaccuracy = MathHelper.RandomPointOnCircle(Random.Shared) * 0.08f;
		direction = (direction + inaccuracy / (staff.accuracy * player.getAccuracyModifier())).normalized;

		GameState.instance.level.addEntity(new MagicProjectile(direction, player.velocity, offset, player, this, staff), position);
		GameState.instance.level.addEntity(new MagicProjectileCastEffect(player), position + offset);

		Audio.PlayOrganic(useSound, new Vector3(player.position, 0));
	}

	public override void update(Entity entity)
	{
		base.update(entity);

		if (castTime != -1)
		{
			float elapsed = (Time.currentTime - castTime) / 1e9f * speed;
			int projectilesShouldCast = Math.Min((int)(elapsed / 0.2f) + 1, 3);
			if (castedProjectiles < projectilesShouldCast)
			{
				shoot();
				castedProjectiles++;
			}
		}
	}
}
