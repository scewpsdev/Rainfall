using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MissileSpell : Spell
{
	public MissileSpell()
		: base("missile_spell")
	{
		displayName = "Missile";

		value = 80;

		attackDamage = 10;
		attackRate = 1;
		manaCost = 0.8f;
		knockback = 1.0f;
		trigger = false;

		sprite = new Sprite(tileset, 14, 7);

		useSound = Resource.GetSounds("res/sounds/cast", 3);
	}

	public override void cast(Player player, Item staff)
	{
		Vector2 position = player.position + new Vector2(player.direction * 0.3f, 0.5f);
		Vector2 offset = new Vector2(player.direction * 0.3f, -0.1f);

		Vector2 direction = player.lookDirection.normalized;
		Vector2 inaccuracy = MathHelper.RandomPointOnCircle(Random.Shared) * 0.05f;
		direction = (direction + inaccuracy / (staff.accuracy * player.getAccuracyModifier())).normalized;

		GameState.instance.level.addEntity(new MagicMissileProjectile(direction, player.velocity, offset, player, staff, this), position);
		GameState.instance.level.addEntity(new MagicProjectileCastEffect(player), position + offset);
	}
}
