using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public partial class LevelGenerator
{
	void generateMinesBackground(Level level, Simplex simplex, TileType tile1, TileType tile2)
	{
		for (int y = 0; y < level.height; y++)
		{
			for (int x = 0; x < level.width; x++)
			{
				float progress = 1 - y / (float)level.height;
				float type = simplex.sample2f(x * 0.05f, -y * 0.05f) - progress * 0.4f;
				float mask = simplex.sample2f(-x * 0.05f, y * 0.05f);
				TileType tile = mask < 0 ? null : type > -0.1f ? tile1 : tile2;
				level.setBGTile(x, y, tile);
			}
		}
	}

	public void generateMines(string seed, out Level[] areaMines)
	{
		areaMines = new Level[5];
		Vector3 lightAmbience = Vector3.One;
		Vector3 mediumAmbience = new Vector3(0.2f);
		Vector3 darkAmbience = new Vector3(0.001f);
		areaMines[0] = new Level(5, "Crystal Mines", 30, 70, TileType.dirt, 12, 20) { ambientLight = mediumAmbience };
		areaMines[1] = new Level(6, "", 70, 30, TileType.dirt, 14, 25) { ambientLight = mediumAmbience };
		areaMines[2] = new Level(7, "", 40, 40, TileType.dirt, 16, 30) { ambientLight = mediumAmbience };
		areaMines[3] = new Level(8, "", 30, 80, TileType.dirt, 18, 35) { ambientLight = mediumAmbience };
		areaMines[4] = new Level(-1, "", 18, 35) { ambientLight = mediumAmbience }; // loot value will affect what the blacksmith sells in the hub

		List<Mob> createEnemy()
		{
			List<Mob> mobs = new List<Mob>();
			mobs.Add(new Spider());
			mobs.Add(new Snake());
			mobs.Add(new Bat());
			mobs.Add(new Slime());
			mobs.Add(new GreenSpider());
			mobs.Add(new OrangeBat());
			mobs.Add(new BlueSlime());
			mobs.Add(new SkeletonArcher());
			return mobs;
		};

		createContainer = (Item[] items) => new Crate(items);
		createExplosiveObject = () => new ExplosiveCrate();

		generateMinesFloor(seed, 0, true, false, areaMines[0], areaMines[1], null, null, () => createEnemy().Slice(0, 5));
		generateMinesFloor(seed, 1, false, false, areaMines[1], areaMines[2], areaMines[0], areaMines[0].exit, () => createEnemy().Slice(0, 6));
		generateMinesFloor(seed, 2, false, false, areaMines[2], areaMines[3], areaMines[1], areaMines[1].exit, () => createEnemy().Slice(0, 7));
		generateMinesFloor(seed, 3, false, false, areaMines[3], areaMines[4], areaMines[2], areaMines[2].exit, () => createEnemy().Slice(0, 8));

		generateMinesBossFloor(areaMines[4], null, areaMines[3], areaMines[3].exit);
	}

	public List<NPC> getMinesNPCList()
	{
		List<NPC> npcs = new List<NPC>();
		npcs.Add(new BuilderMerchant(random, level));
		npcs.Add(new TravellingMerchant(random, level));
		if (!QuestManager.tryGetQuest("logan", "logan_quest", out Quest loganQuest) || loganQuest.state != QuestState.InProgress)
			npcs.Add(NPCManager.logan);
		npcs.Add(NPCManager.tinkerer);

		if (!GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_RAT_MET) || GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_RAT_QUESTLINE_COMPLETED))
			npcs.Add(NPCManager.rat);

		return npcs;
	}

	void generateMinesBossFloor(Level level, Level nextLevel, Level lastLevel, Door lastDoor)
	{
		Room room = generateSingleRoomLevel(level, specialSet, 4, TileType.stone, TileType.rock);

		level.fogFalloff = 0.1f;
		level.fogColor = new Vector3(0.0f);

		level.entrance.destination = lastLevel;
		level.entrance.otherDoor = lastDoor;
		lastDoor.otherDoor = level.entrance;

		Simplex simplex = new Simplex(Hash.hash(seed) + (uint)level.floor, 3);
		generateCaveBackground(level, simplex, TileType.dirt, TileType.stone);

		level.addEntity(new MinesBossRoom(room));
	}

	void generateMinesFloor(string seed, int floor, bool spawnStartingRoom, bool spawnBossRoom, Level level, Level nextLevel, Level lastLevel, Door lastExit, Func<List<Mob>> createEnemy)
	{
		this.seed = seed;
		this.level = level;
		this.nextLevel = nextLevel;
		this.lastLevel = lastLevel;
		this.lastExit = lastExit;

		random = new Random((int)Hash.hash(seed) + floor);
		rooms = new List<Room>();

		int width = level.width;
		int height = level.height;

		//int width = spawnStartingRoom ? MathHelper.RandomInt(60, 80, random) : MathHelper.RandomInt(40, 80, random);
		//int height = Math.Max((floor == 4 ? 3600 : 2400) / width, 20);

		level.rooms = rooms;
		level.ambientSound = Resource.GetSound("sounds/ambience.ogg");
		//level.fogFalloff = 0.04f;
		//level.fogColor = new Vector3(0.1f);

		objectFlags = new bool[width * height];
		Array.Fill(objectFlags, false);

		rooms.Clear();
		RoomDef? startingRoomDef = spawnStartingRoom ? specialSet.roomDefs[2] : spawnBossRoom ? specialSet.roomDefs[3] : null;
		generateMainRooms(minesSet, startingRoomDef);
		if (spawnBossRoom)
			rooms.Reverse();
		Room startingRoom = rooms[0];
		Room exitRoom = rooms[rooms.Count - 1];
		if (rooms.Count > 2)
		{
			int i = 2;
			while ((exitRoom.width <= 2 || exitRoom.height <= 2) && rooms.Count - i >= 1)
				exitRoom = rooms[rooms.Count - i++];
		}

		generateExtraRooms(minesSet, (Doorway doorway) =>
		{
			int type = random.Next() % 5;
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
			else if (type == 4)
			{
				RoomDef def = specialSet.roomDefs[random.Next() % 2 == 0 ? 14 : 15];
				room = fillDoorway(doorway, def, specialSet);
				if (room != null)
					room.entity = new PrisonCellRoom(room, this);
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


		Simplex simplex = new Simplex(Hash.hash(seed) + (uint)floor, 3);

		for (int i = 0; i < rooms.Count; i++)
		{
			placeRoom(rooms[i], level, (int x, int y) =>
			{
				float progress = 1 - y / (float)level.height;
				float type = simplex.sample2f(x * 0.05f, y * 0.05f) - progress * 0.4f;
				return type > -0.1f ? TileType.rock : TileType.stone;
			});
		}

		generateCaveBackground(level, simplex, TileType.rock, TileType.stone);


		Door entranceDoor = floor == 0 ? new CaveEntranceDoor(lastLevel, lastExit) : new Door(lastLevel, lastExit);
		createDoors(spawnStartingRoom, spawnBossRoom, startingRoom, exitRoom, entranceDoor, out Vector2i entrancePosition, out Vector2i exitPosition);

		for (int i = 0; i < rooms.Count; i++)
		{
			if (rooms[i].entity != null)
				level.addEntity(rooms[i].entity, new Vector2(rooms[i].x, rooms[i].y));
		}

		if (spawnStartingRoom)
		{
			startingRoom.spawnEnemies = false;

			for (int y = entrancePosition.y; y < entrancePosition.y + 4; y++)
			{
				for (int x = entrancePosition.x - 2; x < entrancePosition.x + 3; x++)
				{
					if (x >= entrancePosition.x - 1 && x <= entrancePosition.x + 1 && y >= entrancePosition.y && y <= entrancePosition.y + 2)
						level.setBGTile(x, y, null);
					else
						level.setBGTile(x, y, TileType.rock);
				}
			}
		}

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
			bool isDeadEnd = !room.isMainPath;
			if (isDeadEnd)
				deadEnds.Add(room);
			else if (room.isMainPath)
				mainRooms.Add(room);
		}


		// Starting weapon
		List<Room> roomsWithStartingWeapon = mainRooms.Slice(0, 2);
		if (deadEnds.Count > 0)
			roomsWithStartingWeapon.Add(deadEnds[0]);
		if (deadEnds.Count > 1)
			roomsWithStartingWeapon.Add(deadEnds[1]);
		spawnRoomObject(roomsWithStartingWeapon, 1, false, (Vector2i tile, Random random, Room room) =>
		{
			TileType left = level.getTile(tile.x - 1, tile.y);
			TileType right = level.getTile(tile.x + 1, tile.y);
			Item item = Item.CreateRandom(ItemType.Weapon, random, getLootValue((Vector2)tile));
			Item[] items;
			if (item.requiredAmmo != null)
			{
				Item ammo = Item.GetItemPrototype(item.requiredAmmo).copy();
				ammo.stackSize = MathHelper.RandomInt(20, 36, random);
				items = [item, ammo];
			}
			else
			{
				items = [item];
			}
			Chest chest = new Chest(items, left != null && right == null);
			level.addEntity(chest, new Vector2(tile.x + 0.5f, tile.y));
		});


		List<Item[]> items = generateItems(level.minLootValue, level.maxLootValue, DropRates.mines);

		float keyChance = 0.25f;
		if (random.NextSingle() < keyChance)
			items.Add([new IronKey()]);

		MathHelper.ShuffleList(deadEnds, random);
		MathHelper.ShuffleList(mainRooms, random);

		lockDeadEnds(deadEnds, items);

		spawnItems(items, deadEnds);


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

		// Torch
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if ((floor == 2 || floor == 3) && tile == null && down == null && up == null)
			{
				TileType downDown = level.getTile(x, y - 2);
				if (downDown != null)
				{
					float torchChance = 0.01f;
					if (random.NextSingle() < torchChance)
					{
						level.addEntity(new TorchEntity(), new Vector2(x + 0.5f, y + 0.5f));
						objectFlags[x + y * width] = true;
					}
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
						level.addEntity(new ExplosiveBarrel(), new Vector2(x + 0.5f, y));
					}
					else
					{
						level.addEntity(new Barrel(null), new Vector2(x + 0.5f, y));
					}
					objectFlags[x + y * width] = true;
				}
			}
		});


		spawnEnemies(createEnemy, entrancePosition);


		spawnRoomObject(deadEnds, 0.2f, false, (Vector2i tile, Random random, Room room) =>
		{
			spawnNPC(tile.x, tile.y, getCaveNPCList());
		});

		if (floor == 3)
		{
			spawnRoomObject([exitRoom], 1.0f, false, (Vector2i pos, Random random, Room room) =>
			{
				spawnNPC(pos.x, pos.y, getCaveNPCList());
			});
		}

		level.updateLightmap(0, 0, width, height);
	}
}
