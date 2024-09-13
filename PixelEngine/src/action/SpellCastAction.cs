using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SpellCastAction : EntityAction
{
	Item weapon;
	Spell spell;

	public List<Entity> hitEntities = new List<Entity>();


	public SpellCastAction(Item weapon, bool mainHand, Spell spell)
		: base("spell_cast", mainHand)
	{
		duration = 1.0f / weapon.attackRate;

		this.weapon = weapon;
		this.spell = spell;
	}

	public override void onStarted(Player player)
	{
		spell.cast(player, weapon);
	}
}
