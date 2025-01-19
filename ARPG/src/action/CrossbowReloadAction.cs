using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CrossbowReloadAction : EntityAction
{
	Crossbow crossbow;
	Item arrow;


	public CrossbowReloadAction(Crossbow crossbow, Item arrow, bool mainHand = true)
		: base("crossbow_reload", mainHand)
	{
		this.crossbow = crossbow;
		this.arrow = arrow;

		duration = 0;
	}

	public override void onStarted(Player player)
	{
		crossbow.loadedArrow = arrow;
		Audio.PlayOrganic(crossbow.reloadSound, new Vector3(player.position, 0), 3);
	}
}
