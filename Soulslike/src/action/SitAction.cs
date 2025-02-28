using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SitAction : PlayerAction
{
	public SitAction()
		: base("sit", 0)
	{
		animationName[0] = "sit";
		animationName[1] = "sit";
		animationName[2] = "sit";

		overrideWeaponModel[0] = true;
		overrideWeaponModel[1] = true;

		fullBodyAnimation = true;
		animateCameraRotation = true;

		duration = 5;
		viewmodelAim = 0;
		lockYaw = true;
		movementSpeedMultiplier = 0.0f;
	}
}
