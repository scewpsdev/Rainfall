using Rainfall;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum StaggerType
{
	Short,
	Long,
	Headshot,
	Block,
	Parry,
}

public class MobStaggerAction : MobAction
{
	public MobStaggerAction(StaggerType staggerType, Mob mob)
		: base("stagger")
	{
		animationName = "stagger_short";
		if (staggerType == StaggerType.Long && mob.model.getAnimationData("stagger_long") != null)
			animationName = "stagger_long";
		if (staggerType == StaggerType.Headshot && mob.model.getAnimationData("stagger_headshot") != null)
			animationName = "stagger_headshot";
		if (staggerType == StaggerType.Block && mob.model.getAnimationData("stagger_block") != null)
			animationName = "stagger_block";
		if (staggerType == StaggerType.Parry && mob.model.getAnimationData("stagger_parry") != null)
			animationName = "stagger_parry";
	}
}
