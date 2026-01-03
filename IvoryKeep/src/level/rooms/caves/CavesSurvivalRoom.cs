using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public struct Wave
{
	public List<Mob> enemies;
}

public class CavesSurvivalRoom : Entity
{
	Room room;
	LevelGenerator generator;

	BossGate gate;

	MultilayerTrack track;

	List<Wave> waves = new List<Wave>();
	int currentWave = -1;


	public CavesSurvivalRoom(Room room, LevelGenerator generator)
	{
		this.room = room;
		this.generator = generator;

		track = BossRoom.battleTrack;

		// init enemies
		waves.Add(new Wave() { enemies = [new Rat(), new Rat(), new Rat()] });
		waves.Add(new Wave() { enemies = [new Spider(), new Spider(), new Snake()] });
		waves.Add(new Wave() { enemies = [new Bat(), new Bat(), new Bat(), new Bat(), new Bat()] });
		waves.Add(new Wave() { enemies = [new GreenSpider(), new SkeletonArcher()] });
		waves.Add(new Wave() { enemies = [new SkeletonArcher(), new SkeletonArcher(), new OrangeBat()] });
		waves.Add(new Wave() { enemies = [new Golem() { itemDrops = [Item.CreateRandom(ItemType.Relic, generator.random, generator.getRoomLootValue(room) * 2)] }] });
	}

	public override void init(Level level)
	{
		level.ambientTrack = track;
		level.ambientTrackHasIdleLayer = false;

		level.addEntity(new EventTrigger(new Vector2(5), (Player player) =>
		{
			if (currentWave == -1)
			{
				startBossfight();
			}
		}, null), position + new Vector2(10, 1));

		level.addEntity(gate = new BossGate(true), (Vector2)room.getMarker(0x2));
	}

	void startWave(int wave)
	{
		currentWave = wave;

		for (int i = 0; i < waves[wave].enemies.Count; i++)
		{
			Mob enemy = waves[wave].enemies[i];

			enemy.ai.aggroRange = 100;
			enemy.ai.loseRange = 100;
			enemy.ai.awareness = 100;
			enemy.itemDropChance = 0;

			if (enemy.canFly)
				level.addEntity(enemy, room.getMarker(4) + new Vector2(Random.Shared.NextSingle() * 8, 0.5f));
			else if (enemy.gravity == 0)
				level.addEntity(enemy, room.getMarker(5) + new Vector2(Random.Shared.NextSingle() * 8, 0.75f));
			else
				level.addEntity(enemy, room.getMarker(1) + new Vector2(Random.Shared.NextSingle() * 8, 0.5f));
		}
	}

	void startBossfight()
	{
		gate.close();
		AudioManager.SetAmbientTrackLayer(0);

		startWave(0);
	}

	void stopBossfight()
	{
		gate.open();

		AudioManager.SetAmbientTrack(null, false);
	}

	public override void update()
	{
		if (currentWave != -1)
		{
			bool enemiesDead = true;
			foreach (Mob enemy in waves[currentWave].enemies)
			{
				if (enemy.isAlive)
				{
					enemiesDead = false;
					break;
				}
			}

			if (enemiesDead)
			{
				if (currentWave == waves.Count - 1)
				{
					stopBossfight();
				}
				else
				{
					startWave(currentWave + 1);
				}
			}
		}
	}
}
