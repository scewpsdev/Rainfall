using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BossRoom : Entity
{
	public Mob boss;
	Vector2i bossSpawn;

	BossGate gate0;
	BossGate gate1;

	EventTrigger activateTrigger;

	protected MultilayerTrack track;
	protected bool trackHasRoomLayer;


	public void setup(Level level, Vector2i gate0Pos, Vector2i gate1Pos, Vector2i bossPos, Vector2i chestPos, Vector2i blacksmithPos)
	{
		bossSpawn = bossPos;

		boss.isBoss = true;

		level.addEntity(gate0 = new BossGate(), gate0Pos + new Vector2(0.5f, 0));
		level.addEntity(gate1 = new BossGate(), gate1Pos + new Vector2(0.5f, 0));
		level.addEntity(new StashChest(StashChestMode.Store), chestPos + new Vector2(0.5f, 0));
		level.addEntity(new Blacksmith() { direction = -1 }, blacksmithPos + new Vector2(0.5f, 0));

		//Elevator elevator = new Elevator();
		//level.addEntity(elevator, (Vector2)room.getMarker(0x4));
		//GameState.instance.generator.connectDoors(elevator, GameState.instance.hub.getEntity<Hub>().elevators[area]);
	}

	public override void destroy()
	{
		if (track != null && track.running)
			track.stop();
	}

	public void setActivateTrigger(Vector2 position, Vector2 size)
	{
		level.addEntity(activateTrigger = new EventTrigger(size, (Player player) =>
		{
			if (GameState.instance.currentBoss == null && boss.level == null)
			{
				startBossfight(player);
			}
		}, null), position);
	}

	public void onPhaseTransition(int phase)
	{
		if (track != null)
		{
			track.setLayer(phase + (trackHasRoomLayer ? 1 : 0));
		}
	}

	public override void onLevelSwitch(Level newLevel)
	{
		if (newLevel == level)
		{
			if (trackHasRoomLayer && track != null)
			{
				track.start();
				track.setLayer(0);
			}
		}
		else
		{
			if (track != null)
				track.stop();
		}
	}

	void startBossfight(Player player)
	{
		GameState.instance.setBoss(boss, this);
		boss.ai.aggroRange = 100;
		boss.ai.loseRange = 100;
		boss.ai.setTarget(player);

		level.addEntity(boss, bossSpawn + new Vector2(0.5f));

		gate0.close();
		gate1.close();

		if (track != null)
		{
			track.start();
			track.setLayer(0 + (trackHasRoomLayer ? 1 : 0));
		}
	}

	void stopBossfight()
	{
		GameState.instance.setBoss(null, null);

		gate0.open();
		gate1.open();

		if (track != null)
		{
			track.stop();
			track = null;
		}

		foreach (WorldEventListener listener in GameState.instance.world.worldEventListeners)
			listener.onBossKilled(boss);
	}

	/*
	bool isInRoom(Entity entity)
	{
		int roomMargin = 9;
		return room.containsEntity(entity) && entity.position.x + entity.collider.min.x > room.x + roomMargin && entity.position.x + entity.collider.max.x < room.x + room.width - roomMargin;
	}
	*/

	public override void update()
	{
		/*
		if (GameState.instance.currentBoss == null)
		{
			if (boss.level == null && activateTrigger == null && isInRoom(GameState.instance.player))
			{
				startBossfight(GameState.instance.player);
			}
		}
		*/

		if (GameState.instance.currentBoss != null)
		{
			if (!boss.isAlive)
			{
				stopBossfight();
			}
		}
	}
}
