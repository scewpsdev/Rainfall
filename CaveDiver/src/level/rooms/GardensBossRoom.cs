using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GardensBossRoom : BossRoom
{
	public GardensBossRoom(Room room)
		: base(room, new GolemBoss() { health = 120 })
	{
	}
}
