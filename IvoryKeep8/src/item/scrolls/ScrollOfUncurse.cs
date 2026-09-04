using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ScrollOfUncurse : Item
{
	public ScrollOfUncurse()
		: base("scroll_of_uncurse", ItemType.Scroll)
	{
		displayName = "Scroll of Uncurse";
		description = "Severs malignant bindings from all carried items.";

		baseValue = 69;

		sprite = new Sprite(tileset, 7, 12);
		//spellIcon = new Sprite(tileset, 11, 2);
	}

	public override bool use(Player player)
	{
		int numUncursedItems = 0;
		for (int i = 0; i < player.items.Count; i++)
		{
			if (player.items[i].cursed)
			{
				player.items[i].setCursed(false);
				numUncursedItems++;
			}
		}
		player.hud.showMessage($"Removed curse from {numUncursedItems} items.");
		return true;
	}
}
