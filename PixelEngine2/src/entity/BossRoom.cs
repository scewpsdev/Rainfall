using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BossRoom : Entity
{
	Room room;

	public BossRoom(Room room)
	{
		this.room = room;
	}

	public override void init(Level level)
	{
		GolemBoss boss = new GolemBoss();
		boss.isBoss = true;
		boss.itemDropChance = 1;

		foreach (Room room in level.rooms)
		{
			if (room.tryGetMarker(100, out Vector2i p))
			{
				level.addEntity(boss, (Vector2)p);

				level.addEntity(new BossGate(boss, room, true), (Vector2)room.getMarker(101));
				level.addEntity(new BossGate(boss, room, false), (Vector2)room.getMarker(102));

				break;
			}
		}
	}
}
