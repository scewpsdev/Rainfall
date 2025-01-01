using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AstralScepter : Item
{
	public AstralScepter()
		: base("astral_scepter", ItemType.Staff)
	{
		displayName = "Astral Scepter";

		baseDamage = 2;
		baseAttackRate = 0.7f;
		manaCost = 2;
		trigger = false;
		//isSecondaryItem = true;
		secondaryChargeTime = 0;
		knockback = 2.0f;
		twoHanded = true;
		canDrop = false;

		staffCharges = 0;

		value = 75;

		sprite = new Sprite(tileset, 5, 7, 2, 1);
		size = new Vector2(2, 1);
		renderOffset.x = 0.4f;

		castSound = Resource.GetSounds("sounds/cast", 3);
	}

	public override void update(Entity entity)
	{
		if (entity is Player)
		{
			Player player = entity as Player;
			Spell spell = player.activeItems[player.selectedActiveItem] as Spell;
			if (spell != null)
			{
				//attackRate = spell.attackRate;
				trigger = spell.trigger;
				//attackDamage = spell.attackDamage;
			}
		}
	}

	public override bool use(Player player)
	{
		Spell spell = player.activeItems[player.selectedActiveItem] as Spell;
		if (spell != null)
		{
			float manaCost = spell.manaCost * this.manaCost * player.getManaCostModifier();
			if (player.mana >= manaCost)
				player.actions.queueAction(new SpellCastAction(this, player.handItem == this, spell, manaCost));
		}
		return false;
	}

	public override bool useSecondary(Player player)
	{
		bool anim = stab;
		if (player.actions.currentAction != null && player.actions.currentAction is AttackAction)
		{
			AttackAction attack = player.actions.currentAction as AttackAction;
			if (attack.weapon == this)
				anim = !attack.stab;
		}
		player.actions.queueAction(new AttackAction(this, player.handItem == this, anim, 2.0f, 1.5f, 1.2f));
		return false;
	}
}
