using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BossRoom : Entity
{
	Room room;

	public Mob boss;

	BossGate gate0;
	BossGate gate1;


	public BossRoom(Room room, Mob boss)
	{
		this.room = room;
		this.boss = boss;
	}

	public override void init(Level level)
	{
		boss.isBoss = true;

		level.addEntity(gate0 = new BossGate(boss, room, true), (Vector2)room.getMarker(2));
		level.addEntity(gate1 = new BossGate(boss, room, true), (Vector2)room.getMarker(3));
	}

	void open()
	{
		gate0.open();
		gate1.open();
	}

	void close()
	{
		gate0.close();
		gate1.close();
	}

	bool isInRoom(Entity entity)
	{
		int roomMargin = 9;
		return room.containsEntity(entity) && entity.position.x + entity.collider.min.x > room.x + roomMargin && entity.position.x + entity.collider.max.x < room.x + room.width - roomMargin;
	}

	public override void update()
	{
		if (GameState.instance.currentBoss == null)
		{
			if (boss.level == null && isInRoom(GameState.instance.player))
			{
				GameState.instance.currentBoss = boss;
				GameState.instance.currentBossMaxHealth = boss.health;
				boss.ai.aggroRange = 100;
				boss.ai.loseRange = 100;

				level.addEntity(boss, room.getMarker(1) + new Vector2(0.5f));

				close();
			}
		}

		if (GameState.instance.currentBoss != null)
		{
			if (!boss.isAlive)
			{
				GameState.instance.currentBoss = null;
				open();

				foreach (WorldEventListener listener in GameState.instance.worldEventListeners)
					listener.onBossKilled(boss);
			}
		}
	}
}
