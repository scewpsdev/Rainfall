using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class BlueSlime : Slime
{
	public BlueSlime()
		: base(4)
	{
		displayName = "Blue Slime";

		spriteColor = 0xFF508f94;

		health = 8;
		jumpPower = 12;

		ai = new SpiderAI(this)
		{
			aggroRange = 12,
			loseRange = 15,
			jumpChargeTime = 0.75f,
		};
	}
}
