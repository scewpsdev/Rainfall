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
		animationName = "stagger1";
		duration = 0.25f;
		animationTransitionDuration = 0.5f * duration;
	}
}
