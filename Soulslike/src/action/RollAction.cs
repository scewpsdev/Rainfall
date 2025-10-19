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

		rootMotionMultiplier = 1.4f;
	}

	public override void onStarted(Player player)
	{
		player.snapInputPosition();
	}

	public override void onFinished(Player player)
	{
		player.isInvincible = false;
	}

	public override void update(Player player)
	{
		base.update(player);

		player.isInvincible = elapsedTime < 0.5f * duration;
	}
}
