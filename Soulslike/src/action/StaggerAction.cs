using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class StaggerAction : PlayerAction
{
	public StaggerAction()
		: base("stagger")
	{
		animationName = "stagger";

		lockRotation = true;

		canJump = false;
	}
}
