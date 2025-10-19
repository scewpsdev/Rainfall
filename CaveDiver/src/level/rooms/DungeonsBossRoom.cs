using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DungeonsBossRoom : BossRoom
{
	GolemBoss secondGolem;

	public DungeonsBossRoom(Room room)
		: base(room, new GolemBoss() { health = 60 })
	{
	}
}
