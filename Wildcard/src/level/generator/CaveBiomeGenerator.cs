using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CaveBiomeGenerator : RoomBiomeGenerator
{
	Vector2i startPosition, startDirection;

	RoomDefSet specialSet;


	public CaveBiomeGenerator(Vector2i startPosition, Vector2i startDirection)
		: base(new RoomDefSet("level/level1/rooms1.png"))
	{
		this.startPosition = startPosition;
		this.startDirection = startDirection;

		specialSet = new RoomDefSet("level/rooms_special.png", false);

		secondaryTiles.Add(1, TileType.stone);
		secondaryTiles.Add(2, TileType.sand);
	}

	public override void generateBaseLevel()
	{
		RoomBiomeGeneratorSettings settings;
		settings.startPosition = startPosition;
		settings.startDirection = startDirection;
		settings.destination = new Vector2i(264, 426);
		settings.destinationStrictness = 1.0f;
		settings.minMainRooms = 10;
		settings.numLeafRooms = 0;
		settings.createSpecialRoom = createSpecialRoom;
		settings.secretLevelChance = 0.2f;
		settings.createSecretLevel = createSecretLevel;
		settings.generateSimplexBackground = true;

		generateBaseLevel(settings);


		// Arrow trap
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if (tile != null && tile.isSolid && (left == null || right == null))
			{
				float arrowTrapChance = 0.001f;
				if (random.NextSingle() < arrowTrapChance)
				{
					int direction = right == null ? 1 : left == null ? -1 : random.Next() % 2 * 2 - 1;
					level.setTile(x, y, TileType.dummy);
					placeEntity(new ArrowTrap(new Vector2(direction, 0)), new Vector2i(x, y));
				}
			}
		});

		// Spring
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if (tile == null && down != null && up == null)
			{
				TileType upUp = level.getTile(x, y + 2);
				if (upUp == null)
				{
					float springChance = 0.01f;
					if (random.NextSingle() < springChance)
					{
						placeEntity(new Trampoline(), new Vector2i(x, y));
					}
				}
			}
		});

		// Spike
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if (tile == null && down != null && up == null)
			{
				TileType upLeft = level.getTile(x - 1, y + 1);
				TileType upRight = level.getTile(x + 1, y + 1);

				if (upLeft == null && left == null || upRight == null && right == null)
				{
					float spikeChance = 0.015f;
					if (random.NextSingle() < spikeChance)
					{
						placeEntity(new Spike(), new Vector2i(x, y));
					}
				}
			}
		});

		// Spike Trap
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if (tile == null && up != null && up.isSolid)
			{
				TileType downDown = level.getTile(x, y - 2);
				TileType downLeft = level.getTile(x - 1, y - 1);
				TileType downRight = level.getTile(x + 1, y - 1);

				if (down == null && downDown == null && (left != null && right != null || left == null && downLeft == null || right == null && downRight == null))
				{
					float spikeTrapChance = 0.01f;
					if (random.NextSingle() < spikeTrapChance)
					{
						placeEntity(new SpikeTrap(), new Vector2i(x, y));
					}
				}
			}
		});

		// Torch
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if (tile == null && down == null && up == null)
			{
				TileType downDown = level.getTile(x, y - 2);
				if (downDown != null)
				{
					float torchChance = 0.01f;
					if (random.NextSingle() < torchChance)
					{
						placeEntity(new TorchEntity(), new Vector2i(x, y), new Vector2(0, 0.5f));
					}
				}
			}
		});

		// Rock
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if (tile == null && down != null && down.isSolid && down.visible)
			{
				float rockChance = 0.01f;
				if (random.NextSingle() < rockChance)
				{
					if (random.NextSingle() < 0.9f)
					{
						placeEntity(new Rock(), new Vector2i(x, y));
					}
					else
					{
						placeEntity(new Skull(), new Vector2i(x, y));
					}
				}
			}
		});
	}

	bool createSpecialRoom(Doorway doorway)
	{
		int type = random.Next() % 3;
		Room room = null;
		if (type == 0)
		{
			room = fillDoorway(doorway, specialSet, 6);
			if (room != null)
				room.entity = new CavesSpecialRoom1(room);
		}
		else if (type == 1)
		{
			int def = random.Next() % 2 == 0 ? 7 : 8;
			room = fillDoorway(doorway, specialSet, def);
			if (room != null)
				room.entity = new CavesSpecialRoom2(room);
		}
		else if (type == 2)
		{
			int def = random.Next() % 2 == 0 ? 9 : 10;
			room = fillDoorway(doorway, specialSet, def);
			if (room != null)
				room.entity = new CavesSpecialRoom3(room);
		}
		/*
		else if (type == 3)
		{
			RoomDef def = specialSet.roomDefs[11];
			room = fillDoorway(doorway, def, specialSet);
			if (room != null)
				room.entity = new CavesPlatformingRoom1(room, this);
		}
		else if (type == 4)
		{
			RoomDef def = specialSet.roomDefs[random.Next() % 2 == 0 ? 14 : 15];
			room = fillDoorway(doorway, def, specialSet);
			if (room != null)
				room.entity = new PrisonCellRoom(room, this);
		}
		*/
		else
		{
			Debug.Assert(false);
		}

		return room != null;
	}

	Room createSecretLevel(out uint entranceMarker, out uint exitMarker)
	{
		entranceMarker = 0;
		exitMarker = 0;

		int type = random.Next() % 2;
		if (type == 0)
		{
			Room room = new Room(specialSet, 11);
			room.entity = new CavesPlatformingRoom1(room);
			return room;
		}
		else if (type == 1)
		{
			Room room = new Room(specialSet, 14);
			room.entity = new PrisonCellRoom(room);
			return room;
		}
		else
		{
			Debug.Assert(false);
			return null;
		}
	}

	public override List<NPC> createNPC(Vector2i tile)
	{
		float lootValue = getLootValue(tile);

		List<NPC> npcs =
		[
			new BuilderMerchant(random, level, lootValue),
			new TravellingMerchant(random, level, lootValue)
		];
		if (!QuestManager.tryGetQuest("logan", "logan_quest", out Quest loganQuest) || loganQuest.state != QuestState.InProgress)
			npcs.Add(new Logan(random, level, lootValue) /*NPCManager.logan*/);
		//npcs.Add(new Tinkerer() /*NPCManager.tinkerer*/);

		if (!GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_RAT_MET) || GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_RAT_QUESTLINE_COMPLETED))
			npcs.Add(new RatNPC() /*NPCManager.rat*/);

		return npcs;
	}

	public override List<Mob> getEnemyList()
	{
		return
		[
			new Rat(),
			new Spider(),
			new Snake(),
			new Bat(),
			new Slime(),
			new Beetle(),
		];
	}

	public override Container createContainer(Item[] items)
	{
		return new Barrel(items);
	}

	public override ExplosiveObject createExplosiveObject()
	{
		return new ExplosiveBarrel();
	}
}
