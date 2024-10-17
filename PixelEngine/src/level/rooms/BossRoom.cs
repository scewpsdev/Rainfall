using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BossRoom : Entity
{
	Room room;

	Mob boss;

	BossGate gate0;
	BossGate gate1;

	public BossRoom(Room room)
	{
		this.room = room;
	}

	public override void init(Level level)
	{
		boss = new GolemBoss();
		boss.isBoss = true;
		boss.itemDropChance = 1;

		foreach (Room room in level.rooms)
		{
			if (room.tryGetMarker(100, out Vector2i p))
			{
				level.addEntity(boss, (Vector2)p);

				level.addEntity(gate0 = new BossGate(boss, room, true), (Vector2)room.getMarker(101));
				level.addEntity(gate1 = new BossGate(boss, room, false), (Vector2)room.getMarker(102));

				break;
			}
		}
	}

	public override void update()
	{
		if (!gate0.isOpen && GameState.instance.currentBoss == null)
		{
			GameState.instance.currentBoss = boss;
			GameState.instance.currentBossMaxHealth = boss.maxHealth;
		}
		else if (gate0.isOpen && GameState.instance.currentBoss == boss)
		{
			GameState.instance.currentBoss = null;
		}
	}
}
