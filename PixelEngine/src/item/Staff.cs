using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Staff : Item
{
	public List<Spell> attunedSpells = [null]; // size of 1 just so attunedSpells[selectedSpell] can be accessed
	public int selectedSpell = 0;


	public Staff(string name)
		: base(name, ItemType.Staff)
	{
		baseAttackRate = 1;
		trigger = false;
		secondaryChargeTime = 0;

		baseDamage = 1;
		manaCost = 1.0f;
		staffCharges = 10000;
		knockback = 1;

		value = 30;

		useSound = null;
	}

	public Spell attuneSpell(int idx, Spell spell)
	{
		Debug.Assert(idx <= attunedSpells.Count);
		for (int i = attunedSpells.Count; i < staffAttunementSlots; i++)
			attunedSpells.Add(null);
		Spell oldSpell = attunedSpells[idx];
		attunedSpells[idx] = spell;
		return oldSpell;
	}

	public override bool use(Player player)
	{
		Spell spell = attunedSpells[selectedSpell];
		if (spell != null)
		{
			if (staffCharges > 0)
			{
				float manaCost = this.manaCost * spell.manaCost * player.getManaCostModifier();
				player.actions.queueAction(new SpellCastAction(this, player.handItem == this, spell, manaCost));
				player.consumeMana(manaCost);
				base.use(player);
			}
		}
		return false;
	}

	public override bool useSecondary(Player player)
	{
		player.actions.queueAction(new AttackAction(this, player.handItem == this, true, 3, 0.5f, 1));
		return false;
	}
}
