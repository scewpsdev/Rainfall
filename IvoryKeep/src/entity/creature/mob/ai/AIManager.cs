using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class AIManager
{
	public static void NotifySound(Vector3 position, float range)
	{
		Span<HitData> hits = stackalloc HitData[32];
		int numHits = Physics.OverlapSphere(range, position, hits, QueryFilterFlags.Dynamic, PhysicsFiltering.CREATURE);
		for (int i = 0; i < numHits; i++)
		{
			if (hits[i].body != null)
			{
				Entity entity = hits[i].body.entity as Entity;
				if (entity is Mob)
				{
					Mob mob = entity as Mob;
					if (mob.ai != null)
						mob.ai.onSound(position);
				}
			}
		}
	}
}
