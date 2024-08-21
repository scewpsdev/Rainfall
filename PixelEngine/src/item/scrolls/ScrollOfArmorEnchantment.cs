using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ScrollOfArmorEnchantment : Item
{
	public ScrollOfArmorEnchantment()
		: base("scroll_enchant_armor", ItemType.Scroll)
	{
		displayName = "Scroll of Armor Enchantment";

		value = 22;

		sprite = new Sprite(tileset, 14, 2);
	}

	public override bool use(Player player)
	{
		bool wasUsed = false;
		for (int i = 0; i < player.passiveItems.Length; i++)
		{
			if (player.passiveItems[i] != null && player.passiveItems[i].armor > 0)
			{
				player.passiveItems[i].armor++;
				wasUsed = true;
			}
		}
		if (wasUsed)
			player.hud.showMessage("Your armor shimmers lightly.");
		else
			player.hud.showMessage("The scroll was lost without use.");
		return true;
	}
}
