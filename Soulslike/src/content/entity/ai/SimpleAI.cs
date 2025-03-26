using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SimpleAI : AI
{
	public SimpleAI(Creature creature)
		: base(creature)
	{
	}

	public override void tick10()
	{
		if (creature.actionManager.currentAction == null)
		{
			creature.actionManager.queueAction(new CreatureAttackAction(creature.attacks[0]));
		}
	}
}
