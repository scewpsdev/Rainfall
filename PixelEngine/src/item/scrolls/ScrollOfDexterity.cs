using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ScrollOfDexterity : Item
{
	public ScrollOfDexterity()
		: base("scroll_of_dexterity", ItemType.Scroll)
	{
		displayName = "Scroll of Dexterity";

		value = 13;

		sprite = new Sprite(tileset, 15, 2);
	}

	public override bool use(Player player)
	{
		if (player.handItem != null)
		{
			player.handItem.addInfusion(Infusion.Light);
			player.hud.showMessage("Your weapon feels lighter.");
		}
		else
		{
			player.hud.showMessage("The scroll was lost without use.");
		}
		return true;
	}
}
