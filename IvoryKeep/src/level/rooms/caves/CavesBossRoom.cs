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
		boss.direction = -1;
		AdvancedAI ai = boss.ai as AdvancedAI;
		ai.walkDirection = -1;
	}
}
