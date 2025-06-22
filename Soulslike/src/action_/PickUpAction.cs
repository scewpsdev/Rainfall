using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PickUpAction : FirstPersonAction
{
	Item item;

	public PickUpAction(Item item)
		: base("pick_up", 0)
	{
		this.item = item;

		animationName[0] = "pick_up";

		addSoundEffect(new ActionSfx(Item.equipLight));
	}

	public override void onFinished(Player player)
	{
		player.giveItem(item);
	}
}
