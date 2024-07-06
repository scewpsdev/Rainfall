using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GreenSpiderAI : SpiderAI
{
	public GreenSpiderAI()
	{
		aggroRange = 12;
		loseRange = 15;
		jumpChargeTime = 0.5f;
	}
}
