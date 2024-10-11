using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ElderwoodStaff : Item
{
	public ElderwoodStaff()
		: base("elderwood_staff", ItemType.Staff)
	{
		displayName = "Elderwood Staff";

		attackDamage = 0.7f;
		attackRate = 2;
		manaCost = 0.7f;
		trigger = false;
		//isSecondaryItem = true;
		secondaryChargeTime = 0;

		staffCharges = 0;

		value = 37;

		sprite = new Sprite(tileset, 1, 6);
		renderOffset.x = 0.4f;

		castSound = Resource.GetSounds("res/sounds/cast", 3);
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
		player.actions.queueAction(new AttackAction(this, player.handItem == this) { soundPlayed = true });
		return false;
	}
}
