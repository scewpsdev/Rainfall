using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class MobStaggerAction : MobAction
{
	static string getAnimName(MobActionType type)
	{
		return type == MobActionType.StaggerShort ? "stagger_short" : type == MobActionType.StaggerBlocked ? "stagger_blocked" : type == MobActionType.StaggerParry ? "stagger_parry" : null;
	}

	public MobStaggerAction(MobActionType type)
		: base(type)
	{
		animationName = getAnimName(type);

		rootMotion = true;
	}
}
