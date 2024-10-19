using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Staff : Item
{
	public List<Spell> attunedSpells = new List<Spell>();


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
		if (idx == attunedSpells.Count)
			attunedSpells.Add(null);
		Spell oldSpell = attunedSpells[idx];
		attunedSpells[idx] = spell;
		return oldSpell;
	}

	public override bool use(Player player)
	{
		for (int i = 0; i < attunedSpells.Count; i++)
		{
			Spell spell = attunedSpells[i];
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
		}
		return staffCharges == 0;
	}

	public override bool useSecondary(Player player)
	{
		player.actions.queueAction(new AttackAction(this, player.handItem == this, true, 3, 0.5f, 1));
		return false;
	}
}
