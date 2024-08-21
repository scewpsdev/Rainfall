using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BlankPaper : Item
{
	public BlankPaper()
		: base("scroll_blank", ItemType.Scroll)
	{
		displayName = "Blank Paper";

		value = 3;

		sprite = new Sprite(tileset, 14, 1);
	}

	public override bool use(Player player)
	{
		player.hud.showMessage("It has no effect.");
		return false;
	}
}
