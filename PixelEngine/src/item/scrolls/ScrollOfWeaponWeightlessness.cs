using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ScrollOfWeaponWeightlessness : Item
{
	public ScrollOfWeaponWeightlessness()
		: base("scroll_weapon_weightlessness", ItemType.Scroll)
	{
		displayName = "Scroll of Weapon Weightlessness";

		value = 13;

		sprite = new Sprite(tileset, 15, 2);
	}

	public override bool use(Player player)
	{
		if (player.handItem != null)
		{
			player.handItem.attackRate *= 1.2f;
			player.hud.showMessage("Your weapon feels lighter.");
		}
		else
		{
			player.hud.showMessage("The scroll was lost without use.");
		}
		return true;
	}
}
