using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public class BossRoom : Entity
{
	public static MultilayerTrack battleTrack = new MultilayerTrack("sounds/ost/battle/ost", 2);


	Room room;
	protected List<Mob> bosses = new List<Mob>();
	int area;

	public BossGate gate0;
	public BossGate gate1;

	EventTrigger activateTrigger;

	protected MultilayerTrack track;
	protected bool trackHasIdleLayer;


	public BossRoom(Room room, int area)
	{
		this.room = room;
		this.area = area;
	}

	public override void init(Level level)
	{
		level.ambientTrack = track;
		level.ambientTrackHasIdleLayer = trackHasIdleLayer;

		foreach (Mob boss in bosses)
			boss.isBoss = true;
		bosses[0].itemDrops.Add(new IronKey());

		level.addEntity(gate0 = new BossGate(true), (Vector2)room.getMarker(0x2));
		level.addEntity(gate1 = new BossGate(true), (Vector2)room.getMarker(0x3));
		level.addEntity(new StashChest(StashChestMode.Store), (Vector2)room.getMarker(0x4));
		level.addEntity(new Blacksmith() { direction = -1 }, (Vector2)room.getMarker(0x4) + Vector2.Left * 2);

		//Elevator elevator = new Elevator();
		//level.addEntity(elevator, (Vector2)room.getMarker(0x4));
		//GameState.instance.generator.connectDoors(elevator, GameState.instance.hub.getEntity<Hub>().elevators[area]);
	}

	public override void destroy()
	{
	}

	public void setActivateTrigger(Vector2 position, Vector2 size)
	{
		level.addEntity(activateTrigger = new EventTrigger(size, (Player player) =>
		{
			if (GameState.instance.currentBoss == null && bosses[0].level == null)
			{
				startBossfight();
			}
		}, null), position);
	}

	public void onPhaseTransition(int phase)
	{
		AudioManager.SetAmbientTrackLayer(phase);
	}

	public override void onLevelSwitch(Level newLevel)
	{
	}

	void startBossfight()
	{
		GameState.instance.setBoss(bosses[0], this);
		for (int i = 0; i < bosses.Count; i++)
		{
			Mob boss = bosses[i];
			boss.ai.aggroRange = 100;
			boss.ai.loseRange = 100;

			level.addEntity(boss, room.getMarker(1) + new Vector2(0.5f + i * 2, 0.5f));
		}

		gate0.close();
		gate1.close();

		AudioManager.SetAmbientTrackLayer(0);
	}

	void stopBossfight()
	{
		GameState.instance.setBoss(null, null);

		gate0.open();
		gate1.open();

		for (int i = 0; i < 2; i++)
		{
			ChestType chestType = (ChestType)Mathf.RandomInt((int)ChestType.Red, (int)ChestType.Green, Random.Shared);
			Chest chest = new Chest(null, false, chestType);
			chest.items = chest.createThemedItems(level.avgLootValue * 2, DropRates.defaultDroprates, Random.Shared);
			level.addEntity(chest, new Vector2(gate0.position.x * 0.5f + gate1.position.x * 0.5f - 2.0f + i * 2.0f, gate0.bottomPosition.y + 1));
		}

		{
			level.addEntity(new RelicPlinth(level.avgLootValue * 2, Random.Shared), new Vector2(gate0.position.x * 0.5f + gate1.position.x * 0.5f + 2.0f, gate0.bottomPosition.y));
		}

		AudioManager.SetAmbientTrack(null, false);

		foreach (WorldEventListener listener in GameState.instance.worldEventListeners)
			listener.onBossKilled(bosses[0]);
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
			if (bosses[0].level == null && activateTrigger == null && isInRoom(GameState.instance.player))
			{
				startBossfight();
			}
		}

		if (GameState.instance.currentBoss != null)
		{
			if (!bossAlive)
			{
				stopBossfight();
			}
		}
	}

	public bool bossAlive
	{
		get
		{
			for (int i = 0; i < bosses.Count; i++)
			{
				if (bosses[i].isAlive)
					return true;
			}
			return false;
		}
	}
}
