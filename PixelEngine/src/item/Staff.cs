using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Staff : Item
{
	public Staff(string name)
		: base(name, ItemType.Staff)
	{
		baseAttackRate = 1;
		trigger = false;
		secondaryChargeTime = 0;

		baseDamage = 1;
		manaCost = 1.0f;
		staffCharges = -1;
		knockback = 1;

		value = 30;

		useSound = null;
	}

	/*
	public Spell attuneSpell(int idx, Spell spell)
	{
		Debug.Assert(idx <= attunedSpells.Count);
		for (int i = attunedSpells.Count; i < staffAttunementSlots; i++)
			attunedSpells.Add(null);
		Spell oldSpell = attunedSpells[idx];
		attunedSpells[idx] = spell;
		return oldSpell;
	}
	*/

	public override bool use(Player player)
	{
		Spell spell = player.getSelectedSpell();
		if (spell != null)
		{
			float manaCost = this.manaCost * spell.manaCost * player.getManaCostModifier();
			if (player.mana >= manaCost || spell.canCastWithoutMana)
			{
				player.actions.queueAction(new SpellCastAction(this, player.handItem == this, spell, manaCost));
				base.use(player);
			}
		}
		return false;
	}

	protected virtual void getAttackAnim(int idx, out AttackAnim anim, out int swingDir, out float startAngle, out float endAngle)
	{
		anim = this.anim;
		swingDir = anim != AttackAnim.Stab && doubleBladed ? idx % 2 : 0;
		startAngle = attackStartAngle;
		endAngle = attackEndAngle;
	}

	public override bool useSecondary(Player player)
	{
		int attackIdx = 0;
		if (player.actions.currentAction != null && player.actions.currentAction is AttackAction && (player.actions.currentAction as AttackAction).weapon == this)
			attackIdx = (player.actions.currentAction as AttackAction).attackIdx + 1;
		getAttackAnim(attackIdx, out AttackAnim anim, out int swingDir, out float startAngle, out float endAngle);
		player.actions.queueAction(new AttackAction(this, player.handItem == this, anim, 2, 0.5f, 1, startAngle, endAngle) { swingDir = swingDir, attackIdx = attackIdx });
		return false;
	}
}
