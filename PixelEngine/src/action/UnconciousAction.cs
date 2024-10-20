using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class UnconciousAction : EntityAction
{
	const float GETTING_UP_DELAY = 1.0f;


	bool gettingUp = false;
	long gettingUpTime;

	Sound wakeupSound;


	public UnconciousAction()
		: base("unconcious", true)
	{
		animation = "dead";

		turnToCrosshair = false;
		renderWeapon = false;
		canMove = false;

		duration = 1000;

		wakeupSound = Resource.GetSound("res/sounds/wakeup.ogg");
	}

	public override void onStarted(Player player)
	{
		base.onStarted(player);

		player.hud.enabled = false;
	}

	public override void onFinished(Player player)
	{
		base.onFinished(player);

		player.hud.enabled = true;
		player.hud.screenFade = 1;
	}

	public override void update(Player player)
	{
		base.update(player);

		if (elapsedTime < 2)
			player.hud.screenFade = 0;
		else if (elapsedTime < 8)
			player.hud.screenFade = (elapsedTime - 2) / 6.0f;
		else
			player.hud.screenFade = 1;

		if (!gettingUp)
		{
			if (InputManager.IsDown("Left") || InputManager.IsDown("Right") || InputManager.IsDown("Up") || InputManager.IsDown("Down"))
			{
				gettingUp = true;
				gettingUpTime = Time.currentTime;
				animation = "stun";
				Audio.Play(wakeupSound, new Vector3(player.position, 0));
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
