using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CreatureStaggerAction : CreatureAction
{
	public CreatureStaggerAction()
		: base("stagger")
	{
		animationName = "stagger";
		//duration = 2.0f;
		//animationTransitionDuration = 0.5f * duration;
		animationTransitionDuration = 0.3f;
	}
}
