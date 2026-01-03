using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GardensBossRoom : BossRoom
{
	public GardensBossRoom(Room room)
		: base(room, 3)
	{
		bosses.Add(new GolemBoss());
		bosses.Add(new Golem() { health = 30 });

		track = battleTrack;
	}
}
