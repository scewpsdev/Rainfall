using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RollAction : PlayerAction
{
	public RollAction()
		: base("roll")
	{
		animationName = "roll";

		lockRotation = true;
	}

	public override void onStarted(Player player)
	{
		player.snapInputPosition();
	}
}
