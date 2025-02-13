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

	EventTrigger activateTrigger;

	MultilayerTrack track;


	public BossRoom(Room room, Mob boss)
	{
		this.room = room;
		this.boss = boss;

		track = new MultilayerTrack("sounds/ost/battle/ost", 2);
	}

	public override void init(Level level)
	{
		boss.isBoss = true;

		level.addEntity(gate0 = new BossGate(true), (Vector2)room.getMarker(2));
		level.addEntity(gate1 = new BossGate(true), (Vector2)room.getMarker(3));
	}

	public override void destroy()
	{
		if (track != null)
		{
			track.stop();
			track = null;
		}
	}

	public void setActivateTrigger(Vector2 position, Vector2 size)
	{
		level.addEntity(activateTrigger = new EventTrigger(size, (Player player) =>
		{
			if (GameState.instance.currentBoss == null && boss.level == null)
			{
				startBossfight();
			}
		}, null), position);
	}

	public void onPhaseTransition()
	{
		track.setLayer(1);
	}

	void startBossfight()
	{
		GameState.instance.setBoss(boss, this);
		boss.ai.aggroRange = 100;
		boss.ai.loseRange = 100;

		level.addEntity(boss, room.getMarker(1) + new Vector2(0.5f));

		gate0.close();
		gate1.close();

		track.start();
		track.setLayer(0);
	}

	void stopBossfight()
	{
		GameState.instance.setBoss(null, null);

		gate0.open();
		gate1.open();

		track.stop();
		track = null;

		foreach (WorldEventListener listener in GameState.instance.worldEventListeners)
			listener.onBossKilled(boss);
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
			if (boss.level == null && activateTrigger == null && isInRoom(GameState.instance.player))
			{
				startBossfight();
			}
		}

		if (GameState.instance.currentBoss != null)
		{
			if (!boss.isAlive)
			{
				stopBossfight();
			}
		}
	}
}
