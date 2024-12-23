using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract class Spell : Item
{
	protected Spell(string name)
		: base(name, ItemType.Spell)
	{
		tumbles = false;

		sprite = new Sprite(tileset, 3, 8);
	}

	public override bool use(Player player)
	{
		Item staff = player.handItem != null && player.handItem.type == ItemType.Staff ? player.handItem : player.offhandItem != null && player.offhandItem.type == ItemType.Staff ? player.offhandItem : null;
		if (staff != null)
		{
			float manaCost = this.manaCost * staff.manaCost * player.getManaCostModifier();
			if (player.mana >= manaCost)
				player.actions.queueAction(new SpellCastAction(staff, player.handItem == staff, this, manaCost));
		}
		return false;
	}

	public abstract void cast(Player player, Item staff, float manaCost, float duration);
}
