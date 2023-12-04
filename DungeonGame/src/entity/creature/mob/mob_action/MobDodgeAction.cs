using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class MobDodgeAction : MobAction
{
	public MobDodgeAction()
		: base(MobActionType.Dodge)
	{
		animationName = "dodge";

		rootMotion = true;
	}
}
