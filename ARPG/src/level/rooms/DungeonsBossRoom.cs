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

	public override void update()
	{
		base.update();

		if (boss.level != null && secondGolem == null)
		{
			secondGolem = new GolemBoss() { health = 60 };
			level.addEntity(secondGolem, boss.position + Vector2.Right * 3);
		}
	}
}
