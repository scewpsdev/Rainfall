using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LightningSpell : Spell
{
	public LightningSpell()
		: base("lightning_spell")
	{
		displayName = "Lightning";

		value = 27;

		baseAttackRate = 3;
		baseDamage = 1.2f;
		manaCost = 0.25f;
		trigger = false;

		sprite = new Sprite(tileset, 3, 6);

		castSound = Resource.GetSounds("res/sounds/lightning", 4);
	}

	public override void cast(Player player, Item staff, float manaCost)
	{
		Vector2 position = player.position + new Vector2(0.0f, 0.5f);
		Vector2 offset = new Vector2(player.direction * 0.5f, 0.0f);

		Vector2 direction = player.lookDirection.normalized;
		//Vector2 inaccuracy = MathHelper.RandomPointOnCircle(Random.Shared) * 0.05f;
		//direction = (direction + inaccuracy / (staff.accuracy * player.accuracyModifier)).normalized;

		float damage = this.attackDamage * staff.attackDamage * player.getMagicDamageModifier();

		GameState.instance.level.addEntity(new LightningProjectile(direction, Vector2.Zero, player, this, damage, player.mana >= manaCost ? 1 : 0.5f), position + offset);
		GameState.instance.level.addEntity(new MagicProjectileCastEffect(player), position + offset);
	}
}
