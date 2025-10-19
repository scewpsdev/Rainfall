using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DungeonsBossRoom : BossRoom
{
	public DungeonsBossRoom(Room room)
		//: base(room, 2)
	{
		boss = new Garran();
	}
}
