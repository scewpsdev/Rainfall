using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class SpellCastAction : Action
{
	Item staff;
	Item spell;
	int handID;

	bool[] casted;


	public SpellCastAction(Item staff, Item spell, int handID)
		: base(ActionType.SpellCast, "spell_cast")
	{
		this.staff = staff;
		this.spell = spell;
		this.handID = handID;

		casted = new bool[spell.spellProjectiles.Length];

		if (staff.twoHanded)
		{
			animationName[0] = "spell_cast";
			animationName[1] = "spell_cast";
			animationSet[0] = spell.moveset;
			animationSet[1] = spell.moveset;
		}
		else
		{
			animationName[handID] = "spell_cast";
			animationSet[handID] = spell.moveset;
		}
	}

	public override void update(Player player)
	{
		for (int i = 0; i < casted.Length; i++)
		{
			if (elapsedTime > spell.spellProjectiles[i].castTime && !casted[i] && player.stats.mana >= spell.spellManaCost)
			{
				Vector3 castDirection = player.lookDirection;
				Vector3 castPosition = player.lookOrigin + castDirection * 2;

				Entity projectile = null;
				if (spell.spellProjectiles[i].type == SpellProjectileType.Arrow)
					projectile = new MagicArrow(castDirection, player, spell.baseDamage);
				if (spell.spellProjectiles[i].type == SpellProjectileType.Orb)
					projectile = new MagicOrb(castDirection, player, spell.baseDamage);
				if (spell.spellProjectiles[i].type == SpellProjectileType.Homing)
					projectile = new HomingOrb(player, spell.baseDamage);

				player.level.addEntity(projectile, castPosition, Quaternion.Identity);

				player.stats.consumeMana(spell.spellManaCost);

				casted[i] = true;
			}
		}
	}
}
