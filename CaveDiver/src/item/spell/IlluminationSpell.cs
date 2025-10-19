using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class IlluminationSpell : Spell
{
	public IlluminationSpell()
		: base("illumination_spell")
	{
		displayName = "Illumination";

		value = 8;

		baseAttackRate = 1;
		baseDamage = 0;
		manaCost = 0.5f;
		trigger = false;
		upgradable = false;

		spellIcon = new Sprite(tileset, 4, 6);
	}

	public override bool cast(Player player, Item staff, float manaCost, float duration)
	{
		if (player.mana >= manaCost)
		{
			Vector2 position = player.position + new Vector2(0.0f, 0.3f);
			Vector2 offset = new Vector2(player.direction * 0.5f, 0.1f);

			Vector2 direction = player.lookDirection.normalized;

			GameState.instance.level.addEntity(new LightOrb(direction, player.velocity, offset, player), position);
			GameState.instance.level.addEntity(new MagicProjectileCastEffect(player), position + offset);

			return true;
		}

		return false;
	}
}
