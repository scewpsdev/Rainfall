using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ScrollOfTeleportation : Item
{
	public ScrollOfTeleportation()
		: base("scroll_teleport", ItemType.Scroll)
	{
		displayName = "Scroll of Teleportation";

		value = 7;

		sprite = new Sprite(tileset, 13, 2);
	}

	public override bool use(Player player)
	{
		SpellEffects.TeleportEntity(player);
		return true;
	}
}
