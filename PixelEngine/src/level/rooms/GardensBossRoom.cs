using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GardensBossRoom : BossRoom
{
	GolemBoss secondGolem;
	Gandalf gandalf1;
	Gandalf gandalf2;

	public GardensBossRoom(Room room)
		: base(room, new GolemBoss() { health = 120 })
	{
	}

	public override void update()
	{
		base.update();

		if (boss.level != null && secondGolem == null && gandalf1 == null && gandalf2 == null)
		{
			secondGolem = new GolemBoss() { health = 120 };
			secondGolem.ai.aggroRange = 100;
			level.addEntity(secondGolem, boss.position - Vector2.Right * 3);

			gandalf1 = new Gandalf() { health = 25 };
			gandalf1.ai.aggroRange = 100;
			level.addEntity(gandalf1, boss.position + Vector2.Right * 3);

			gandalf2 = new Gandalf() { health = 25 };
			gandalf2.ai.aggroRange = 100;
			level.addEntity(gandalf2, boss.position + Vector2.Right * 4);
		}
	}
}
