using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DeathAction : EntityAction
{
	public DeathAction()
		: base("death")
	{
		duration = 100000;
	}
}
