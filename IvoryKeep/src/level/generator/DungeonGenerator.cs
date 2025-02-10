using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


public partial class LevelGenerator
{
	void generateDungeonBackground(Level level, Simplex simplex)
	{
		for (int y = 0; y < level.height; y++)
		{
			for (int x = 0; x < level.width; x++)
			{
				float progress = 1 - y / (float)level.height;
				float type = simplex.sample2f(x * 0.05f, -y * 0.05f) - progress * 0.4f;
				float mask = simplex.sample2f(-x * 0.05f, y * 0.05f);
				TileType tile = mask < 0 ? null : type > -0.1f ? TileType.dirt : TileType.stone;
				level.setBGTile(x, y, tile);
			}
		}
	}

	public void generateDungeons(string seed, out Level[] areaDungeons)
	{
		areaDungeons = new Level[5];
		Vector3 ambience = MathHelper.ARGBToVector(0xFF3b3159).xyz;
		areaDungeons[0] = new Level(9, "Weeping Catacombs", 60, 40, null, 20, 40) { ambientLight = ambience };
		areaDungeons[1] = new Level(10, "", 40, 50, null, 22, 50) { ambientLight = ambience };
		areaDungeons[2] = new Level(11, "", 50, 50, null, 25, 60) { ambientLight = ambience };
		areaDungeons[3] = new Level(12, "", 40, 60, TileType.bricks, 27, 70) { ambientLight = ambience };
		areaDungeons[4] = new Level(-1, "Forgotten Chamber", 40, 20, TileType.bricks, 27, 70) { ambientLight = ambience }; // loot value will affect what the blacksmith sells in the hub

		List<Mob> createEnemy()
		{
			List<Mob> mobs = new List<Mob>();
			mobs.Add(new GreenSpider());
			mobs.Add(new OrangeBat());
			mobs.Add(new BlueSlime());
			mobs.Add(new SkeletonArcher());
			mobs.Add(new Leprechaun());
			mobs.Add(new Gandalf());
			mobs.Add(new Stalker());
			return mobs;
		};

		//createBarrelEntity = (Item[] items) => new Pot(items);
		createContainer = (Item[] items) => new Pot(items);
		createExplosiveObject = () => new ExplosivePot();

		generateDungeonFloor(seed, 0, true, false, areaDungeons[0], areaDungeons[1], null, null, () => createEnemy().Slice(0, 4));
		generateDungeonFloor(seed, 1, false, false, areaDungeons[1], areaDungeons[2], areaDungeons[0], areaDungeons[0].exit, () => createEnemy().Slice(0, 5));
		generateDungeonFloor(seed, 2, false, false, areaDungeons[2], areaDungeons[3], areaDungeons[1], areaDungeons[1].exit, () => createEnemy().Slice(0, 6));
		generateDungeonFloor(seed, 3, false, false, areaDungeons[3], areaDungeons[4], areaDungeons[2], areaDungeons[2].exit, () => createEnemy().Slice(0, 7));

		generateDungeonBossFloor(areaDungeons[4], null, areaDungeons[3], areaDungeons[3].exit);
	}

	public List<NPC> getDungeonNPCList()
	{
		List<NPC> npcs = new List<NPC>();
		npcs.Add(new TravellingMerchant(random, level));
		if (!QuestManager.tryGetQuest("logan", "logan_quest", out Quest loganQuest) || loganQuest.state != QuestState.InProgress)
			npcs.Add(NPCManager.logan);
		npcs.Add(NPCManager.tinkerer);

		return npcs;
	}

	void generateDungeonBossFloor(Level level, Level nextLevel, Level lastLevel, Door lastDoor)
	{
		Room room = generateSingleRoomLevel(level, specialSet, 18, TileType.stone, TileType.bricks, 0, 0x4);

		level.fogFalloff = 0.1f;
		level.fogColor = new Vector3(0.0f);

		level.entrance.destination = lastLevel;
		level.entrance.otherDoor = lastDoor;
		lastDoor.otherDoor = level.entrance;

		Simplex simplex = new Simplex(Hash.hash(seed) + (uint)level.floor, 3);
		generateCaveBackground(level, simplex, TileType.bricks, TileType.stone);

		level.addEntity(new DungeonsBossRoom(room));
	}

	void generateDungeonFloor(string seed, int floor, bool spawnStartingRoom, bool spawnBossRoom, Level level, Level nextLevel, Level lastLevel, Door entrance, Func<List<Mob>> createEnemy)
	{
		this.seed = seed;
		this.level = level;
		this.nextLevel = nextLevel;
		this.lastLevel = lastLevel;
		this.lastExit = entrance;

		random = new Random((int)Hash.hash(seed) + level.floor);
		rooms = new List<Room>();

		int width = level.width;
		int height = level.height;

		level.rooms = rooms;
		level.ambientSound = Resource.GetSound("sounds/ambience.ogg");

		objectFlags = new bool[width * height];
		Array.Fill(objectFlags, false);

		rooms.Clear();
		RoomDef? startingRoomDef = spawnStartingRoom ? specialSet.roomDefs[12] : spawnBossRoom ? specialSet.roomDefs[13] : null;
		generateMainRooms(dungeonsSet, startingRoomDef);
		if (spawnBossRoom)
			rooms.Reverse();
		Room startingRoom = rooms[0];
		Room exitRoom = rooms[rooms.Count - 1];
		{
			int i = 2;
			while ((exitRoom.width <= 2 || exitRoom.height <= 2) && rooms.Count - i >= 1)
				exitRoom = rooms[rooms.Count - i++];
		}

		// TODO create new special rooms
		generateExtraRooms(dungeonsSet, (Doorway doorway) =>
		{
			int type = random.Next() % 4;
			Room room = null;
			if (type == 0)
			{
				room = fillDoorway(doorway, specialSet.roomDefs[6], specialSet);
				if (room != null)
					room.entity = new CavesSpecialRoom1(room, this);
			}
			else if (type == 1)
			{
				RoomDef def = specialSet.roomDefs[random.Next() % 2 == 0 ? 7 : 8];
				room = fillDoorway(doorway, def, specialSet);
				if (room != null)
					room.entity = new CavesSpecialRoom2(room, this);
			}
			else if (type == 2)
			{
				RoomDef def = specialSet.roomDefs[random.Next() % 2 == 0 ? 9 : 10];
				room = fillDoorway(doorway, def, specialSet);
				if (room != null)
					room.entity = new CavesSpecialRoom3(room, this);
			}
			else if (type == 3)
			{
				RoomDef def = specialSet.roomDefs[11];
				room = fillDoorway(doorway, def, specialSet);
				if (room != null)
					room.entity = new CavesPlatformingRoom1(room, this);
			}
			else
			{
				Debug.Assert(false);
			}

			if (room != null)
			{
				room.spawnEnemies = false;
				return true;
			}

			return false;
		});


		Simplex simplex = new Simplex(Hash.hash(seed) + (uint)level.floor, 3);

		for (int i = 0; i < rooms.Count; i++)
		{
			placeRoom(rooms[i], level, (int x, int y) =>
			{
				float progress = 1 - y / (float)level.height;
				float type = simplex.sample2f(x * 0.05f, y * 0.05f);
				return type > -0.5f ? TileType.bricks : TileType.stone;
			});
		}

		generateCaveBackground(level, simplex, TileType.stone, TileType.bricks);


		Door entranceDoor = new Door(lastLevel, lastExit);
		createDoors(spawnStartingRoom, spawnBossRoom, startingRoom, exitRoom, entranceDoor, out Vector2i entrancePosition, out Vector2i exitPosition);

		for (int i = 0; i < rooms.Count; i++)
		{
			if (rooms[i].entity != null)
				level.addEntity(rooms[i].entity, new Vector2(rooms[i].x, rooms[i].y));
		}

		if (spawnStartingRoom)
			startingRoom.spawnEnemies = false;

		/*
		if (spawnBossRoom)
		{
			exitRoom.spawnEnemies = false;
			level.addEntity(new CavesBossRoom(exitRoom));
		}
		*/

		List<Room> deadEnds = new List<Room>();
		List<Room> mainRooms = new List<Room>();
		for (int i = 0; i < rooms.Count; i++)
		{
			Room room = rooms[i];
			bool isDeadEnd = room.countConnectedDoorways() == 1 || !room.isMainPath;
			if (isDeadEnd)
				deadEnds.Add(room);
			else if (room.isMainPath)
				mainRooms.Add(room);
		}


		List<Item[]> items = generateItems(level.minLootValue, level.maxLootValue, DropRates.dungeons);

		MathHelper.ShuffleList(deadEnds, random);
		MathHelper.ShuffleList(mainRooms, random);

		lockDeadEnds(deadEnds, items);

		spawnItems(items, deadEnds);


		// Guaranteed key per floor
		if (lockedDoorSpawned)
		{
			spawnRoomObject(rooms, 0.5f, false, (Vector2i tile, Random random, Room room) =>
			{
				spawnItem(tile.x, tile.y, [new IronKey()]);
			});
		}


		// Fountain
		spawnRoomObject(deadEnds, 0.5f, false, (Vector2i tile, Random random, Room room) =>
		{
			Fountain fountain = new Fountain(random);
			level.addEntity(fountain, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Coins
		spawnRoomObject(deadEnds, 0.05f, true, (Vector2i tile, Random random, Room room) =>
		{
			int amount = MathHelper.RandomInt(2, 7, random);
			level.addEntity(new CoinStack(amount), new Vector2(tile.x + 0.5f, tile.y + 0.5f));
		});

		// Arrow trap
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if (tile != null && tile.isSolid && (x > 0 && left == null || x < width - 1 && right == null) && y != entrancePosition.y)
			{
				float arrowTrapChance = 0.001f;
				if (random.NextSingle() < arrowTrapChance)
				{
					int direction = right == null ? 1 : left == null ? -1 : random.Next() % 2 * 2 - 1;
					level.setTile(x, y, TileType.dummy);
					level.addEntity(new ArrowTrap(new Vector2(direction, 0)), new Vector2(x, y));
					objectFlags[x + y * width] = true;
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
						level.addEntity(new Trampoline(), new Vector2(x + 0.5f, y));
						objectFlags[x + y * width] = true;
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
						level.addEntity(new Spike(), new Vector2(x, y));
						objectFlags[x + y * width] = true;
					}
				}
			}
		});

		// Sconces
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if (tile == null && down != null && up == null)
			{
				float sconceChance = 0.02f;
				if (random.NextSingle() < sconceChance)
				{
					level.addEntity(new FireSconce(), new Vector2(x + 0.5f, y));
					objectFlags[x + y * width] = true;
				}
			}
		});

		// Barrel
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if (tile == null && down != null && down.isSolid && down.visible)
			{
				float barrelChance = MathF.Max(simplex.sample2f(x * 0.04f, y * 0.04f) * 0.3f - 0.12f, 0);
				if (random.NextSingle() < barrelChance)
				{
					float explosiveBarrel = 0.1f;
					if (random.NextSingle() < explosiveBarrel)
					{
						level.addEntity(new ExplosiveCrate(), new Vector2(x + 0.5f, y));
					}
					else
					{
						level.addEntity(new Crate(null), new Vector2(x + 0.5f, y));
					}
					objectFlags[x + y * width] = true;
				}
			}
		});


		spawnEnemies(createEnemy, entrancePosition);


		if (QuestManager.tryGetQuest("logan", "logan_quest", out Quest loganQuest) && loganQuest.state == QuestState.InProgress)
		{
			if (level == GameState.instance.areaDungeons[GameState.instance.areaDungeons.Length - 2])
			{
				spawnRoomObject(rooms, rooms.Count, false, (Vector2i pos, Random random, Room room) =>
				{
					spawnItem(pos.x, pos.y, [new QuestlineLoganStaff()]);
				});
			}
		}

		spawnRoomObject(deadEnds, 0.1f, false, (Vector2i tile, Random random, Room room) =>
		{
			spawnNPC(tile.x, tile.y, getDungeonNPCList());
		});

		if (floor == 3)
		{
			spawnRoomObject([exitRoom], 1.0f, false, (Vector2i pos, Random random, Room room) =>
			{
				spawnNPC(pos.x, pos.y, getDungeonNPCList());
			});
		}


		level.updateLightmap(0, 0, width, height);
	}
}
