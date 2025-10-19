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
		awareness = 0;

		addJumpAction();
	}

	public override void onHit(Entity by)
	{
		//base.onHit(by);
	}
}
