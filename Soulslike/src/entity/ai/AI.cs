using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AI
{
	public Creature creature;


	public AI(Creature creature)
	{
		this.creature = creature;
	}

	public virtual void update()
	{
	}

	public virtual void fixedUpdate(float delta)
	{
	}

	public virtual void tick1()
	{
	}

	public virtual void tick10()
	{
	}
}
