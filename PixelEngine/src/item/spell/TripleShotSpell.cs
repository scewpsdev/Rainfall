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
		: base("triple_shot_spell")
	{
		displayName = "Triple Shot";

		value = 19;

		baseDamage = 0.7f;
		baseAttackRate = 1;
		manaCost = 0.3f;
		knockback = 1.0f;
		trigger = false;

		spellIcon = new Sprite(tileset, 5, 8);

		useSound = Resource.GetSounds("res/sounds/cast", 3);
	}

	public override void cast(Player player, Item staff, float manaCost, float duration)
	{
		this.player = player;
		this.staff = staff;

		shoot();
	}

	void shoot()
	{
		int numBullets = 3;
		for (int i = 0; i < numBullets; i++)
		{
			Vector2 position = player.position + new Vector2(player.direction * 0.3f, 0.5f);
			Vector2 offset = new Vector2(player.direction * 0.3f, -0.1f);

			float coneSize = MathF.PI / 6 / player.getAccuracyModifier();
			Vector2 direction = Vector2.Rotate(player.lookDirection.normalized, (i / (float)(numBullets - 1) - 0.5f) * coneSize);
			Vector2 inaccuracy = MathHelper.RandomPointOnCircle(Random.Shared) * 0.08f;
			direction = (direction + inaccuracy / (staff.accuracy * player.getAccuracyModifier())).normalized;

			float damage = this.attackDamage * staff.attackDamage;

			GameState.instance.level.addEntity(new MagicProjectile(direction, player.velocity, offset, player, this, damage, player.mana >= manaCost ? 1 : 0.5f), position);
			GameState.instance.level.addEntity(new MagicProjectileCastEffect(player), position + offset);

			Audio.PlayOrganic(useSound, new Vector3(player.position, 0));
		}
	}
}
