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
		areaDungeons = new Level[6];
		Vector3 ambience = MathHelper.ARGBToVector(0xFF3b3159).xyz;
		areaDungeons[0] = new Level(5, "Weeping Catacombs", 50, 30, TileType.stone, 15, 20) { ambientLight = ambience };
		areaDungeons[1] = new Level(6, "", 30, 40, TileType.stone, 19, 24) { ambientLight = ambience };
		areaDungeons[2] = new Level(7, "", 40, 40, TileType.stone, 22, 30) { ambientLight = ambience };
		areaDungeons[3] = new Level(8, "", 30, 50, TileType.stone, 28, 35) { ambientLight = ambience };
		areaDungeons[4] = new Level(9, "", 50, 20, TileType.stone, 33, 40) { ambientLight = ambience };
		areaDungeons[5] = new Level(-1, "Forgotten Chamber", 40, 20, TileType.stone) { ambientLight = ambience };

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

		generateDungeonFloor(seed, true, false, areaDungeons[0], areaDungeons[1], null, null, () => createEnemy().Slice(0, 4));
		generateDungeonFloor(seed, false, false, areaDungeons[1], areaDungeons[2], areaDungeons[0], areaDungeons[0].exit, () => createEnemy().Slice(0, 4));
		generateDungeonFloor(seed, false, false, areaDungeons[2], areaDungeons[3], areaDungeons[1], areaDungeons[1].exit, () => createEnemy().Slice(0, 5));
		generateDungeonFloor(seed, false, false, areaDungeons[3], areaDungeons[4], areaDungeons[2], areaDungeons[2].exit, () => createEnemy().Slice(0, 7));
		generateDungeonFloor(seed, false, false, areaDungeons[4], areaDungeons[5], areaDungeons[3], areaDungeons[3].exit, () => createEnemy().Slice(0, 7));

		generateDungeonBossFloor(areaDungeons[5], null, areaDungeons[4], areaDungeons[4].exit);
	}

	public List<NPC> getDungeonNPCList()
	{
		List<NPC> npcs = new List<NPC>();
		npcs.Add(new TravellingMerchant(random, level));
		if (!QuestManager.tryGetQuest("logan", "logan_quest", out Quest loganQuest) || loganQuest.state != QuestState.InProgress)
			npcs.Add(NPCManager.logan);
		npcs.Add(new Blacksmith(random, level));
		npcs.Add(new Tinkerer(random, level));

		return npcs;
	}

	void generateDungeonBossFloor(Level level, Level nextLevel, Level lastLevel, Door lastDoor)
	{
		simplex = new Simplex(Hash.hash(seed) + (uint)level.floor, 3);

		RoomDef def = specialSet.roomDefs[4];
		level.resize(def.width, def.height);

		Room room = new Room
		{
			x = 0,
			y = 0,
			width = def.width,
			height = def.height,
			roomDefID = def.id,
			set = specialSet
		};

		placeRoom(room, level, (int x, int y) => TileType.stone);
		level.rooms = [room];

		level.fogFalloff = 0.1f;
		level.fogColor = new Vector3(0.0f);

		level.entrance = new Door(lastLevel, lastDoor);
		Vector2i entrancePosition = room.getMarker(0x4);
		level.addEntity(level.entrance, new Vector2(entrancePosition.x + 0.5f, entrancePosition.y));
		lastDoor.otherDoor = level.entrance;

		level.exit = new Door(nextLevel);
		Vector2i exitPosition = room.getMarker(0x5);
		level.addEntity(level.exit, new Vector2(exitPosition.x + 0.5f, exitPosition.y));

		if (level.getTile(exitPosition.x - 1, exitPosition.y) == null && !objectFlags[exitPosition.x - 1 + exitPosition.y * level.width])
		{
			level.addEntity(new TorchEntity(), new Vector2(exitPosition.x - 0.5f, exitPosition.y + 0.5f));
			objectFlags[exitPosition.x - 1 + exitPosition.y * level.width] = true;
		}
		if (level.getTile(exitPosition.x + 1, exitPosition.y) == null && !objectFlags[exitPosition.x + 1 + exitPosition.y * level.width])
		{
			level.addEntity(new TorchEntity(), new Vector2(exitPosition.x + 1.5f, exitPosition.y + 0.5f));
			objectFlags[exitPosition.x + 1 + exitPosition.y * level.width] = true;
		}

		generateCaveBackground(level, simplex, TileType.dirt, TileType.stone);

		level.addEntity(new DungeonsBossRoom(room));

		level.updateLightmap(0, 0, def.width, def.height);
	}

	void generateDungeonFloor(string seed, bool spawnStartingRoom, bool spawnBossRoom, Level level, Level nextLevel, Level lastLevel, Door entrance, Func<List<Mob>> createEnemy)
	{
		this.seed = seed;
		this.level = level;
		this.nextLevel = nextLevel;
		this.lastLevel = lastLevel;
		this.entrance = entrance;

		random = new Random((int)Hash.hash(seed) + level.floor);
		simplex = new Simplex(Hash.hash(seed) + (uint)level.floor, 3);
		rooms = new List<Room>();

		int width = level.width;
		int height = level.height;

		level.rooms = rooms;
		level.ambientSound = Resource.GetSound("sounds/ambience.ogg");

		objectFlags = new bool[width * height];
		Array.Fill(objectFlags, false);

		lootModifier = new float[width * height];
		Array.Fill(lootModifier, 1.0f);

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
					room.entity = new CavesSpecialRoom4(room, this);
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

		createDoors(spawnStartingRoom, spawnBossRoom, startingRoom, exitRoom, out Vector2i entrancePosition, out Vector2i exitPosition);

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
			bool isDeadEnd = room.countConnectedDoorways() == 1 && !room.isMainPath;
			if (isDeadEnd)
				deadEnds.Add(room);
			else if (room.isMainPath)
				mainRooms.Add(room);

			if (isDeadEnd)
			{
				for (int y = room.y; y < room.y + room.height; y++)
				{
					for (int x = room.x; x < room.x + room.width; x++)
					{
						lootModifier[x + y * width] = 3.0f;
					}
				}
			}
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
			level.addEntity(new Gem(amount), new Vector2(tile.x + 0.5f, tile.y + 0.5f));
		});

		// Items
		spawnRoomObject(deadEnds, 0.15f, false, (Vector2i tile, Random random, Room room) =>
		{
			spawnItem(tile.x, tile.y, getRoomLootValue(room));
		});


		MathHelper.ShuffleList(deadEnds, random);
		MathHelper.ShuffleList(mainRooms, random);

		lockDeadEnds(deadEnds, mainRooms);


		// Guaranteed key per floor
		if (lockedDoorSpawned)
		{
			spawnRoomObject(rooms, 0.5f, false, (Vector2i tile, Random random, Room room) =>
			{
				spawnItem(tile.x, tile.y, [new IronKey()]);
			});
		}


		// Arrow trap
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if (tile != null && tile.isSolid && (left == null || right == null) && y != entrancePosition.y)
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

		// Moneh
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if (tile == null && down != null)
			{
				float gemChance = up != null ? 0.04f : 0.01f;
				gemChance *= 0.25f;
				if (random.NextSingle() < gemChance)
				{
					//int amount = MathHelper.RandomInt(3, 12, random);
					int amount = 10;
					level.addEntity(new Gem(amount), new Vector2(x + 0.5f, y + 0.5f));
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

		// Spike Trap
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if (tile == null && up != null && up.isSolid)
			{
				TileType downDown = level.getTile(x, y - 2);
				TileType downLeft = level.getTile(x - 1, y - 1);
				TileType downRight = level.getTile(x + 1, y - 1);

				if (down == null && downDown == null && (left != null && right != null || left == null && downLeft == null || right == null && downRight == null) && x != entrancePosition.x)
				{
					float spikeTrapChance = 0.01f;
					if (random.NextSingle() < spikeTrapChance)
					{
						level.addEntity(new SpikeTrap(), new Vector2(x + 0.5f, y + 0.5f));
						objectFlags[x + y * width] = true;
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
						Item[] items = null;
						float itemChance = 0.1f;
						if (random.NextSingle() < itemChance)
							items = Item.CreateRandom(random, DropRates.barrel, getLootValue(new Vector2(x, y)));

						level.addEntity(new Barrel(items), new Vector2(x + 0.5f, y));
					}
					objectFlags[x + y * width] = true;
				}
			}
		});


		List<Mob> mobInstances = new List<Mob>();
		int numMobs = MathHelper.RandomInt(rooms.Count, rooms.Count * 3 / 2, random);
		for (int i = 0; i < numMobs; i++)
		{
			List<Mob> mobTypes = createEnemy();
			mobInstances.Add(mobTypes[random.Next() % mobTypes.Count]);
		}
		for (int i = 0; mobInstances.Count > 0 && i < 1000; i++)
		{
			Mob mob = mobInstances[0];

			spawnRoomObject(rooms, rooms.Count, false, (Vector2i pos, Random random, Room room) =>
			{
				TileType tile = level.getTile(pos);
				TileType left = level.getTile(pos.x - 1, pos.y);
				TileType right = level.getTile(pos.x + 1, pos.y);
				TileType up = level.getTile(pos.x, pos.y + 1);
				TileType down = level.getTile(pos.x, pos.y - 1);
				if (tile == null && (left == null && right == null) && !getObjectFlag(pos.x, pos.y))
				{
					TileType downLeft = level.getTile(pos.x - 1, pos.y - 1);
					TileType downRight = level.getTile(pos.x + 1, pos.y - 1);

					float distanceToEntrance = (pos - entrancePosition).length;

					if (room.spawnEnemies && (distanceToEntrance > 8 || pos.y < entrancePosition.y) && down != null && (downLeft != null && left == null || downRight != null && right == null))
					{
						if (spawnEnemy(pos.x, pos.y, mob))
							mobInstances.RemoveAt(0);
					}
				}
			});
		}


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


		if (level == GameState.instance.areaDungeons[GameState.instance.areaDungeons.Length - 2])
		{
			spawnRoomObject([exitRoom], 1.0f, false, (Vector2i pos, Random random, Room room) =>
			{
				spawnNPC(pos.x, pos.y, getDungeonNPCList());
			});
		}


		level.updateLightmap(0, 0, width, height);
	}
}
