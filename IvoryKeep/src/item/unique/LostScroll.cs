using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LostScroll : Item
{
	public LostScroll()
		: base("lost_scroll", ItemType.Utility)
	{
		name = "lost_scroll";
		displayName = "Gatekeeper's Lost Scroll";
		description = "A long forgotten scroll once written by one of the royal knights. What is it doing in these catacombs?";

		sprite = new Sprite(tileset, 9, 11);

		canDrop = false;
		baseValue = 0;
	}
}
