using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class HealingSpell : Spell
{
	public HealingSpell()
		: base("heal")
	{
		displayName = "Heal";

		baseValue = 25;

		baseAttackRate = 1.5f;
		baseDamage = 0;
		manaCost = 2;
		trigger = false;
		upgradable = false;

		spellIcon = new Sprite(tileset, 4, 8);

		castSound = [Resource.GetSound("sounds/heal.ogg")];
	}

	public override bool cast(Player player, Item staff, float manaCost, float duration)
	{
		if (player.mana >= manaCost)
		{
			float amount = 1 * player.getMagicDamageModifier();

			player.addStatusEffect(new HealStatusEffect(amount, 5));

			Vector2 position = player.position + new Vector2(0.0f, 0.3f);
			Vector2 offset = new Vector2(player.direction * 0.5f, 0.1f);

			GameState.instance.level.addEntity(new MagicProjectileCastEffect(player), position + offset);

			return true;
		}
		return false;
	}
}
