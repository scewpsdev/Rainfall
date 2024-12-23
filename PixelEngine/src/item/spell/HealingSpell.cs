using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class HealingSpell : Spell
{
	const int numTicks = 20;

	public HealingSpell()
		: base("healing_spell")
	{
		displayName = "Heal";

		value = 15;

		baseAttackRate = numTicks / 10.0f;
		baseDamage = 0;
		manaCost = 4.0f / numTicks;
		trigger = false;
		upgradable = false;

		spellIcon = new Sprite(tileset, 4, 8);
	}

	public override void cast(Player player, Item staff, float manaCost, float duration)
	{
		if (player.mana >= manaCost)
		{
			player.heal(player.maxHealth / 2 / numTicks);

			Vector2 position = player.position + new Vector2(0.0f, 0.3f);
			Vector2 offset = new Vector2(player.direction * 0.5f, 0.1f);

			GameState.instance.level.addEntity(new MagicProjectileCastEffect(player), position + offset);
		}
	}
}
