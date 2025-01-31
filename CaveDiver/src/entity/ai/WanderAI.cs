using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WanderAI : AdvancedAI
{
	public WanderAI(Mob mob)
		: base(mob)
	{
		aggroRange = 0;
		loseRange = 0;

		addJumpAction();
	}
}
