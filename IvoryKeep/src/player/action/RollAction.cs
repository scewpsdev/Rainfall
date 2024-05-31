using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;


public class RollAction : Action
{
	const float ROLL_IFRAMES = 15 / 24.0f;


	float direction;


	public RollAction(float direction)
		: base("roll")
	{
		this.direction = direction;

		animationName[0] = "roll";
		animationName[1] = "roll";
		animationName[2] = "roll";

		animationTransitionDuration = 0.1f;

		movementSpeedMultiplier = 0.0f;
		rotationSpeedMultiplier = 0.0f;

		rootMotion = true;

		iframesStartTime = 0;
		iframesEndTime = ROLL_IFRAMES;
	}

	public override void onStarted(Player player)
	{
		base.onStarted(player);

		player.setDirection(direction);
	}
}
