using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class AIBehavior
{
	protected Creature creature;

	bool initialized = false;


	protected AIBehavior(Creature creature)
	{
		this.creature = creature;
	}

	public virtual void onHit(int damage, Entity from)
	{
	}

	public virtual void init()
	{
		initialized = true;
	}

	public virtual void update()
	{
		if (!initialized)
			init();
	}
}
