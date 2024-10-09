using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TripleShotSpell : Spell
{
	Player player;
	Item staff;

	long castTime = -1;
	int castedProjectiles = 0;

	public TripleShotSpell()
		: base("triple_shot_spell")
	{
		displayName = "Triple Shot";

		value = 25;

		attackDamage = 0.7f;
		attackRate = 1;
		manaCost = 0.3f;
		knockback = 1.0f;
		trigger = false;

		sprite = new Sprite(tileset, 4, 7);

		useSound = Resource.GetSounds("res/sounds/cast", 3);
	}

	public override void cast(Player player, Item staff)
	{
		this.player = player;
		this.staff = staff;
		castTime = Time.currentTime;
		castedProjectiles = 0;
	}

	void shoot()
	{
		Vector2 position = player.position + new Vector2(player.direction * 0.3f, 0.3f);
		Vector2 offset = new Vector2(player.direction * 0.3f, 0.1f);

		Vector2 direction = player.lookDirection.normalized;
		Vector2 inaccuracy = MathHelper.RandomPointOnCircle(Random.Shared) * 0.03f;
		direction = (direction + inaccuracy / (staff.accuracy * player.accuracyModifier)).normalized;
		GameState.instance.level.addEntity(new MagicProjectile(direction, player.velocity, offset, player, staff, this), position);
		GameState.instance.level.addEntity(new MagicProjectileCastEffect(player), position + offset);

		Audio.PlayOrganic(useSound, new Vector3(player.position, 0));
	}
	public override void update(Entity entity)
	{
		base.update(entity);

		if (castTime != -1)
		{
			int projectilesShouldCast = Math.Min((int)((Time.currentTime - castTime) / 1e9f / 0.1f) + 1, 3);
			if (castedProjectiles < projectilesShouldCast)
			{
				shoot();
				castedProjectiles++;
			}
		}
	}
}
