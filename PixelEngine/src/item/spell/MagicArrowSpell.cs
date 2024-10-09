using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MagicArrowSpell : Spell
{
	public MagicArrowSpell()
		: base("magic_arrow_spell")
	{
		displayName = "Magic Arrow";

		value = 14;

		attackDamage = 0.8f;
		attackRate = 4;
		manaCost = 0.1f;
		knockback = 1.0f;
		trigger = false;

		sprite = new Sprite(tileset, 0, 6);

		useSound = Resource.GetSounds("res/sounds/cast", 3);
	}

	public override void cast(Player player, Item staff)
	{
		Vector2 position = player.position + new Vector2(player.direction * 0.3f, 0.3f);
		Vector2 offset = new Vector2(player.direction * 0.3f, 0.1f);

		Vector2 direction = player.lookDirection.normalized;
		Vector2 inaccuracy = MathHelper.RandomPointOnCircle(Random.Shared) * 0.05f;
		direction = (direction + inaccuracy / (staff.accuracy * player.accuracyModifier)).normalized;

		GameState.instance.level.addEntity(new MagicProjectile(direction, player.velocity, offset, player, staff, this), position);
		GameState.instance.level.addEntity(new MagicProjectileCastEffect(player), position + offset);
	}
}
