using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ScrollOfWeaponEnchantment : Item
{
	public ScrollOfWeaponEnchantment()
		: base("scroll_enchant_weapon", ItemType.Scroll)
	{
		displayName = "Scroll of Weapon Enchantment";

		value = 13;

		sprite = new Sprite(tileset, 15, 2);
	}

	public override bool use(Player player)
	{
		if (player.handItem != null)
		{
			player.handItem.attackDamage++;
			player.hud.showMessage("Your weapon shimmers lightly.");
		}
		else
		{
			player.hud.showMessage("The scroll was lost without use.");
		}
		return true;
	}
}
