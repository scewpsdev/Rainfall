using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class UnconciousAction : EntityAction
{
	const float GETTING_UP_DELAY = 0.5f;


	bool gettingUp = false;
	long gettingUpTime;


	public UnconciousAction()
		: base("unconcious", true)
	{
		animation = "dead";

		turnToCrosshair = false;
		renderWeapon = false;
		canMove = false;

		duration = 1000;
	}

<<<<<<< HEAD
	public override void onStarted(Player player)
	{
		base.onStarted(player);

		player.hud.enabled = false;
	}

	public override void onFinished(Player player)
	{
		base.onFinished(player);

		player.hud.enabled = true;
	}

=======
>>>>>>> d79438fbcb8d9274022f6ae886162149e731fa4a
	public override void update(Player player)
	{
		if (!gettingUp)
		{
			if (InputManager.IsDown("Left") || InputManager.IsDown("Right") || InputManager.IsDown("Up") || InputManager.IsDown("Down"))
			{
				gettingUp = true;
				gettingUpTime = Time.currentTime;
				animation = "stun";
			}
		}
		if (gettingUp)
		{
			if ((Time.currentTime - gettingUpTime) / 1e9f > GETTING_UP_DELAY)
			{
				duration = 0;
			}
		}
	}
}
