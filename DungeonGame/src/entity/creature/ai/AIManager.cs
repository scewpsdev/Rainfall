using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class AIManager
{
	static List<AI> ais = new List<AI>();


	public static void RegisterAI(AI ai)
	{
		ais.Add(ai);
	}

	public static void RemoveAI(AI ai)
	{
		ais.Remove(ai);
	}

	public static void NotifySound(Vector3 position, float range)
	{
		foreach (AI ai in ais)
		{
			Vector3 creaturePosition = ai.creature.position;
			Vector3 delta = creaturePosition - position;
			float distanceSq = Vector3.Dot(delta, delta);
			if (distanceSq < range * range)
				ai.onSoundHeard(position);
		}
	}
}
