using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AI
{
	public readonly Creature creature;

	bool initialized = false;


	protected AI(Creature creature)
	{
		this.creature = creature;
	}

	public virtual void onSoundHeard(Vector3 from)
	{
	}

	public virtual void onHit(int damage, Entity from)
	{
	}

	public void init()
	{
		AIManager.RegisterAI(this);
		initialized = true;
	}

	public void destroy()
	{
		AIManager.RemoveAI(this);
	}

	public virtual void update()
	{
		if (!initialized)
			init();
	}
}
