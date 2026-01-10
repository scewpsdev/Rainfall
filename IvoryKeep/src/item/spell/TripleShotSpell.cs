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

	public TripleShotSpell()
		: base("triple_shot")
	{
		displayName = "Triple Shot";
		description = "Discharges a wide spread of magic bolts at close range.";

		baseValue = 19;

		baseDamage = 0.9f;
		baseAttackRate = 1;
		baseAttackRange = 5;
		manaCost = 0.3f;
		knockback = 1.0f;
		trigger = false;
		canCastWithoutMana = true;

		spellIcon = new Sprite(tileset, 5, 8);
	}

	public override bool cast(Player player, Item staff, float manaCost, float duration)
	{
		this.player = player;
		this.staff = staff;

		shoot();

		return true;
	}

	void shoot()
	{
		int numBullets = 3;
		for (int i = 0; i < numBullets; i++)
		{
			Vector2 position = player.center + new Vector2(player.direction * 0.3f, 0.0f);
			Vector2 offset = new Vector2(player.direction * 0.3f, 0.0f);

			float coneSize = MathF.PI / 6 / player.getAccuracyModifier();
			Vector2 direction = Vector2.Rotate(player.lookDirection.normalized, (i / (float)(numBullets - 1) - 0.5f) * coneSize);
			Vector2 inaccuracy = Mathf.RandomPointOnCircle(Random.Shared) * 0.08f;
			direction = (direction + inaccuracy / (staff.accuracy * player.getAccuracyModifier())).normalized;

			MagicProjectile projectile = new MagicProjectile(direction, player.velocity, offset, player, this, staff);
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
}
