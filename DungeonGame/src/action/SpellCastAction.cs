using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class SpellCastAction : Action
{
	Item spell;
	int handID;

	bool[] casted;


	public SpellCastAction(Item spell, int handID)
		: base(ActionType.SpellCast)
	{
		this.spell = spell;
		this.handID = handID;

		casted = new bool[spell.spellProjectiles.Length];

		if (spell.twoHanded)
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

		mirrorAnimation = handID == 1;

		movementSpeedMultiplier = 0.5f;

		if (spell.sfxCast != null)
			addSoundEffect(spell.sfxCast, handID, spell.sfxCastTime, true);
	}

	public override void update(Player player)
	{
		base.update(player);
		for (int i = 0; i < casted.Length; i++)
		{
			if (elapsedTime > spell.spellProjectiles[i].castTime && !casted[i] && player.stats.mana >= spell.spellManaCost)
			{
				Vector3 castDirection = player.lookDirection;
				Vector3 castPosition = player.lookOrigin;
				Vector3 handPosition = player.getWeaponTransform(handID).translation;
				Vector3 offset = handPosition - castPosition;

				Entity projectile = null;
				if (spell.spellProjectiles[i].type == SpellProjectileType.Arrow)
					projectile = new MagicArrow(castDirection, offset, player, spell.baseDamage);
				if (spell.spellProjectiles[i].type == SpellProjectileType.Orb)
				{
					projectile = new MagicOrb(castDirection, player, spell.baseDamage);
					castPosition += player.lookDirection * 0.5f;
				}
				if (spell.spellProjectiles[i].type == SpellProjectileType.Homing)
					projectile = new HomingOrb(offset, player, spell.baseDamage);
				if (spell.spellProjectiles[i].type == SpellProjectileType.Fireball)
					projectile = new Fireball(castDirection, offset, player, spell.baseDamage);

				DungeonGame.instance.level.addEntity(projectile, castPosition, Quaternion.Identity);

				player.stats.consumeMana(spell.spellManaCost);

				if (spell.spellProjectiles[i].sfx != null)
					Audio.Play(spell.spellProjectiles[i].sfx, handPosition);

				casted[i] = true;
			}
		}
	}
}
