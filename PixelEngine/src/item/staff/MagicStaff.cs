using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MagicStaff : Item
{
	public MagicStaff()
		: base("magic_staff", ItemType.Staff)
	{
		displayName = "Magic Staff";

		attackRate = 4;
		trigger = false;
		isSecondaryItem = true;
		secondaryChargeTime = 0;

		attackDamage = 1.0f;
		manaCost = 1.0f;
		staffCharges = 0;

		value = 17;

		sprite = new Sprite(tileset, 2, 6);
		renderOffset.x = 0.4f;

		useSound = Resource.GetSounds("res/sounds/cast", 3);
	}

	public override void update(Entity entity)
	{
		if (entity is Player)
		{
			Player player = entity as Player;
			Spell spell = player.activeItems[player.selectedActiveItem] as Spell;
			if (spell != null)
			{
				attackRate = spell.attackRate;
				trigger = spell.trigger;
				//attackDamage = spell.attackDamage;
			}
		}
	}

	public override void onEquip(Player player)
	{
		player.manaRechargeRate *= 2;
	}

	public override void onUnequip(Player player)
	{
		player.manaRechargeRate /= 2;
	}

	public override bool use(Player player)
	{
		Spell spell = player.activeItems[player.selectedActiveItem] as Spell;
		if (spell != null)
			player.actions.queueAction(new SpellCastAction(this, player.handItem == this, spell));
		return false;
	}

	public override bool useSecondary(Player player)
	{
		player.actions.queueAction(new AttackAction(this, player.handItem == this) { soundPlayed = true });
		return false;
	}
}
