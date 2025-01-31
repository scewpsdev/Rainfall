using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BossRoom : Entity
{
	Room room;

	Mob[] bosses;
	bool spawned = false;

	BossGate gate0;
	BossGate gate1;


	public BossRoom(Room room, Mob[] bosses)
	{
		this.room = room;
		this.bosses = bosses;
	}

	public BossRoom(Room room, Mob boss)
		: this(room, [boss])
	{
	}

	public override void init(Level level)
	{
		foreach (Mob boss in bosses)
			boss.isBoss = true;

		level.addEntity(gate0 = new BossGate(true), (Vector2)room.getMarker(2));
		level.addEntity(gate1 = new BossGate(true), (Vector2)room.getMarker(3));
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
		int roomMargin = 4;
		return room.containsEntity(entity) && entity.position.x + entity.collider.min.x > room.x + roomMargin && entity.position.x + entity.collider.max.x < room.x + room.width - roomMargin;
	}

	public override void update()
	{
		if (GameState.instance.currentBoss == null)
		{
			if (!spawned && isInRoom(GameState.instance.player))
			{
				GameState.instance.setBoss(bosses);
				for (int i = 0; i < bosses.Length; i++)
				{
					Mob boss = bosses[i];

					boss.ai.aggroRange = 100;
					boss.ai.loseRange = 100;
					boss.itemDropChance = 0;

					if (boss.canFly)
						level.addEntity(boss, room.getMarker(4) + new Vector2(Random.Shared.NextSingle() * 8, 0.5f));
					else if (boss.gravity == 0)
						level.addEntity(boss, room.getMarker(5) + new Vector2(Random.Shared.NextSingle() * 8, 0.75f));
					else
						level.addEntity(boss, room.getMarker(1) + new Vector2(Random.Shared.NextSingle() * 8, 0.5f));
				}

				spawned = true;

				close();
			}
		}

		if (GameState.instance.currentBoss != null)
		{
			bool bossesDead = true;
			foreach (Mob boss in bosses)
			{
				if (boss.isAlive)
				{
					bossesDead = false;
					break;
				}
			}

			if (bossesDead)
			{
				GameState.instance.setBoss(null);
				open();

				GameState.instance.player.hud.onBossDefeat();

				foreach (WorldEventListener listener in GameState.instance.worldEventListeners)
					listener.onBossKilled(bosses);
			}
		}
	}
}
