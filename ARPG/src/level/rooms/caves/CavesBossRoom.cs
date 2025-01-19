using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CavesBossRoom : BossRoom
{
	public CavesBossRoom(Room room)
		: base(room, new GolemBoss())
	{
	}
}
