using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MagicArrow : Spell
{
	protected int bulletCount = 1;


	public MagicArrow()
		: base("magic_arrow")
	{
		displayName = "Magic Arrow";
		description = "A simple, reliable projection of focussed mana.";

		baseValue = 14;

		baseDamage = 0.8f;
		baseAttackRate = 3;
		baseAttackRange = 5;
		manaCost = 0.1f;
		knockback = 1.0f;
		trigger = false;
		canCastWithoutMana = true;

		spellIcon = new Sprite(tileset, 0, 6);
	}

	public override bool cast(Player player, Item staff, float manaCost, float duration)
	{
		for (int i = 0; i < bulletCount; i++)
		{
			Vector2 position = player.center;
			Vector2 offset = Vector2.Zero; // new Vector2(player.direction * 0.3f, 0.0f);

			float coneSize = MathF.PI / 6 / player.getAccuracyModifier();
			Vector2 direction = bulletCount > 1 ? Vector2.Rotate(player.lookDirection.normalized, (i / (float)(bulletCount - 1) - 0.5f) * coneSize) : player.lookDirection.normalized;
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

		return true;
	}
}
