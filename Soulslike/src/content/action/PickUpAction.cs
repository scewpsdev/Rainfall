using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PickUpAction : PlayerAction
{
	Item item;

	public PickUpAction(Item item)
		: base("pick_up", 0)
	{
		this.item = item;

		animationName[0] = "pickup";
		viewmodelAim[0] = 1;

		addSoundEffect(new ActionSfx(Item.equipLight));
	}

	public override void onFinished(Player player)
	{
		player.giveItem(item);
	}
}
