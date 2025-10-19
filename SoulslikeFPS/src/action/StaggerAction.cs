using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class StaggerAction : FirstPersonAction
{
	public StaggerAction(int level)
		: base("stagger", 0)
	{
		animationName[0] = "stagger" + level;
		animationName[1] = "stagger" + level;
		overrideWeaponModel[hand] = true;
		animationTransitionDuration = 0.05f;

		movementSpeedMultiplier = 0.2f;
	}
}
