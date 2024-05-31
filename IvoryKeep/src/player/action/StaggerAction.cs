using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;


public class StaggerAction : Action
{
	public StaggerAction()
		: base("stagger")
	{
		animationName[0] = "stagger";
		animationName[1] = "stagger";
		animationName[2] = "stagger";

		rootMotion = true;
		animationTransitionDuration = 0.1f;

		movementSpeedMultiplier = 0.0f;
		rotationSpeedMultiplier = 0.0f;
	}
}
