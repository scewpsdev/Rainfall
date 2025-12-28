using Rainfall;
using TiledCS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


public abstract class LevelGenerator
{
	public static RoomDefSet hubSet;
	public static RoomDefSet cavesSet;
	public static RoomDefSet cavesSpecialSet;

	protected static RoomDefSet specialSet;
	protected static RoomDefSet minesSet;
	protected static RoomDefSet dungeonsSet;
	protected static RoomDefSet gardensSet;

	protected static MultilayerTrack cavesAmbientTrack;
	protected static MultilayerTrack minesAmbientTrack;
	protected static MultilayerTrack dungeonsAmbientTrack;

	static LevelGenerator()
	{
		specialSet = new RoomDefSet("level/rooms_special.png", false);

		hubSet = new RoomDefSet(null);
		hubSet.loadTmx("level/rooms/hub.tmx");
		hubSet.loadTmx("level/rooms/hub2.tmx");

		cavesSet = new RoomDefSet("level/level1/rooms1.png");
		for (int i = 0; i < 16; i++)
		{
			cavesSet.loadTmx($"level/rooms/level1/room{i + 1}.tmx");
		}

		cavesSpecialSet = new RoomDefSet(null);
		for (int i = 0; i < 1; i++)
		{
			cavesSpecialSet.loadTmx($"level/rooms/level1/special{i + 1}.tmx");
		}

		minesSet = new RoomDefSet("level/level2/rooms2.png");
		dungeonsSet = new RoomDefSet("level/level3/rooms3.png");
		gardensSet = new RoomDefSet("level/level4/rooms4.png");

		cavesAmbientTrack = new MultilayerTrack("sounds/ost/area/caves", 1);
		minesAmbientTrack = new MultilayerTrack("sounds/ost/area/mines", 1);
		dungeonsAmbientTrack = new MultilayerTrack("sounds/ost/area/catacombs", 1);
	}


	public string name;
	public string displayName;

	RoomDefSet roomSet;
	TileType primaryTile;
	TileType secondaryTile;
	protected TileType bgPrimaryTile;
	protected TileType bgSecondaryTile;

	protected bool spawnStartingWeapon = false;

	string seed;
	public Random random;

	protected Level level;
	protected Simplex simplex;

	Level lastLevel, nextLevel;
	Door lastExit;

	List<Room> rooms;

	bool[] objectFlags;

	//List<Type> spawnedNPCs = new List<Type>();

	Door secretDoor;


	public LevelGenerator(string name, string displayName, RoomDefSet roomSet, TileType primaryTile, TileType secondaryTile, string seed)
	{
		this.name = name;
		this.displayName = displayName;
		this.roomSet = roomSet;
		this.primaryTile = primaryTile;
		this.secondaryTile = secondaryTile;
		this.seed = seed;

		random = new Random((int)Hash.hash(seed));
	}

	public abstract int getNumFloors();
	public abstract string getLevelName(int floor);
	public abstract int getLootValue(int floor);
	public abstract void getLevelSize(int floor, out int width, out int height);
	public virtual TileType getDefaultTile(int floor)
	{
		TileType defaultTile = primaryTile;
		float rockyLevelChance = 0.1f;
		if (random.NextSingle() < rockyLevelChance)
			defaultTile = secondaryTile;
		return defaultTile;
	}

	public abstract int getAmbientLight();
	public abstract Sound getAmbientSound();
	public abstract MultilayerTrack getAmbientTrack();

	public abstract Door createEntranceDoor(Level lastLevel, Door lastExit);

	public abstract BossRoom createBossRoom(Room room);
	public abstract RoomDef[] getSecretRoomDefs();
	public abstract Entity createSecretRoomEntity(int type, Room room);

	public virtual TileType getTile(int x, int y, int idx, TileType tile) => tile;

	public abstract float[] getDroprates();
	public abstract int getNumItems(int floor);

	public abstract Container createContainer(Item[] items);
	public abstract ExplosiveObject createExplosiveObject();

	public abstract List<Mob> createEnemy(Level level);
	public abstract NPC createNPC(int type, Level level);

	public virtual void onFloorFinish(Level level)
	{
	}


	public void generateNoiseBackground(Level level, Simplex simplex, TileType tile1, TileType tile2)
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

	public void generateArea(out Level[] levels)
	{
		int numFloors = getNumFloors();
		levels = new Level[numFloors + 1];
		for (int i = 0; i < numFloors; i++)
		{
			getLevelSize(i, out int width, out int height);

			levels[i] = new Level(i, name + i, getLevelName(i), width, height, getDefaultTile(i), getLootValue(i));
		}
		levels[numFloors + 0] = new Level(-1, name + (numFloors + 0), "", getLootValue(numFloors - 1));

		for (int i = 0; i < numFloors; i++)
		{
			generateFloor(i, i == 0, i == numFloors - 1, levels[i], levels[i + 1], i > 0 ? levels[i - 1] : null, i > 0 ? levels[i - 1].exit : null);
		}

		generateBossFloor(levels[numFloors], null, levels[numFloors - 1], levels[numFloors - 1].exit);

		/*
		generateSingleRoomLevel(areaCaves[numFloors + 1], specialSet, 16, TileType.stone, TileType.dirt, 0x1, 0x2);
		generateSingleRoomLevel(areaCaves[numFloors + 2], specialSet, 17, TileType.stone, TileType.dirt, 0x1, 0x2, null, new DungeonEntrance(null, null));

		// mines entrance
		LevelTransition minesEntrance = new LevelTransition(null, null, new Vector2i(7, 1), Vector2i.Down);
		areaCaves[numFloors + 1].addEntity(minesEntrance, areaCaves[numFloors + 1].rooms[0].getMarker(0x3) + new Vector2(-3, 0));
		areaCaves[numFloors + 1].rooms[0].doorways.Add(new Doorway(areaCaves[numFloors + 1].rooms[0], new DoorDef()) { door = minesEntrance });

		// elevator that leads to the hub
		Door elevator1 = new Door(null, null);
		areaCaves[numFloors + 2].addEntity(elevator1, (Vector2)areaCaves[numFloors + 2].rooms[0].getMarker(0x3) + new Vector2(0.5f, 0));
		areaCaves[numFloors + 2].rooms[0].doorways.Add(new Doorway(areaCaves[numFloors + 2].rooms[0], new DoorDef()) { door = elevator1 });

		// elevator that leads to the mines exit
		Door elevator2 = new Door(null, null);
		areaCaves[numFloors + 2].addEntity(elevator2, (Vector2)areaCaves[numFloors + 2].rooms[0].getMarker(0x4) + new Vector2(0.5f, 0));
		areaCaves[numFloors + 2].rooms[0].doorways.Add(new Doorway(areaCaves[numFloors + 2].rooms[0], new DoorDef()) { door = elevator2 });

		connectDoors(areaCaves[numFloors].exit, areaCaves[numFloors + 1].entrance);
		connectDoors(areaCaves[numFloors + 1].exit, areaCaves[numFloors + 2].entrance);
		*/


		if (secretDoor != null)
		{
			Level level = new Level(-1, name + "_secret_floor", "", numFloors * 4);
			Room room = createSecretRoom(null);

			generateSingleRoomLevel(level, room, null, primaryTile, secondaryTile);

			secretDoor.sprite = new Sprite(Entity.tileset, 5, 4, 2, 2);
			secretDoor.rect = new FloatRect(-1, -0.5f, 2, 2);
			secretDoor.interactRange = 0.5f;

			connectDoors(level.entrance, secretDoor);
		}


		//generateSingleRoomLevel(areaCaves[6], specialSet, 16, TileType.stone, TileType.dirt, 0x1, 0x2);
		//generateSingleRoomLevel(areaCaves[7], specialSet, 17, TileType.stone, TileType.dirt, 0x1, 0x2, null, new DungeonEntrance(null, null));

		/*
		// mines entrance
		LevelTransition minesEntrance = new LevelTransition(null, null, new Vector2i(11, 2), Vector2i.Down);
		areaCaves[6].addEntity(minesEntrance, areaCaves[6].rooms[0].getMarker(0x3) + new Vector2(-5, -2));
		areaCaves[6].rooms[0].doorways.Add(new Doorway(areaCaves[6].rooms[0], new DoorDef()) { door = minesEntrance });

		// elevator that leads to the hub
		Door elevator1 = new Door(null, null);
		areaCaves[7].addEntity(elevator1, (Vector2)areaCaves[7].rooms[0].getMarker(0x3) + new Vector2(0.5f, 0));
		areaCaves[7].rooms[0].doorways.Add(new Doorway(areaCaves[7].rooms[0], new DoorDef()) { door = elevator1 });

		// elevator that leads to the mines exit
		Door elevator2 = new Door(null, null);
		areaCaves[7].addEntity(elevator2, (Vector2)areaCaves[7].rooms[0].getMarker(0x4) + new Vector2(0.5f, 0));
		areaCaves[7].rooms[0].doorways.Add(new Doorway(areaCaves[7].rooms[0], new DoorDef()) { door = elevator2 });

		connectDoors(areaCaves[5].exit, areaCaves[6].entrance);
		connectDoors(areaCaves[6].exit, areaCaves[7].entrance);
		*/
	}

	void generateFloor(int floor, bool spawnStartingRoom, bool spawnBossRoom, Level level, Level nextLevel, Level lastLevel, Door lastExit)
	{
		this.level = level;
		this.nextLevel = nextLevel;
		this.lastLevel = lastLevel;
		this.lastExit = lastExit;

		rooms = new List<Room>();

		int width = level.width;
		int height = level.height;

		//int width = spawnStartingRoom ? Mathf.RandomInt(60, 80, random) : Mathf.RandomInt(40, 80, random);
		//int height = Math.Max((floor == 4 ? 3600 : 2400) / width, 20);

		level.rooms = rooms;
		level.ambientSound = getAmbientSound();
		level.ambientTrack = getAmbientTrack();
		level.ambientTrackHasIdleLayer = true;
		//level.fogFalloff = 0.04f;
		//level.fogColor = new Vector3(0.1f);

		level.lightLevel = getAmbientLight();

		objectFlags = new bool[width * height];
		Array.Fill(objectFlags, false);

		rooms.Clear();
		RoomDef? startingRoomDef = spawnStartingRoom ? specialSet.roomDefs[2] : spawnBossRoom ? specialSet.roomDefs[3] : null;
		generateMainRooms(roomSet, startingRoomDef, spawnBossRoom);
		if (spawnBossRoom)
			rooms.Reverse();
		Room startingRoom = rooms[0];

		Room exitRoom = rooms[rooms.Count - 1];

		if (!spawnBossRoom)
		{
			List<Room> mainRoomsCopy = new List<Room>(rooms);
			mainRoomsCopy.Sort((Room a, Room b) =>
			{
				int da = Math.Abs(a.x + a.width / 2 - (startingRoom.x + startingRoom.width / 2)) + Math.Abs(a.y + a.height / 2 - (startingRoom.y + startingRoom.height / 2));
				int db = Math.Abs(b.x + b.width / 2 - (startingRoom.x + startingRoom.width / 2)) + Math.Abs(b.y + b.height / 2 - (startingRoom.y + startingRoom.height / 2));
				return da < db ? -1 : da > db ? 1 : 0;
			});
			exitRoom = mainRoomsCopy[mainRoomsCopy.Count - 1];
		}

		generateExtraRooms(roomSet, (Doorway doorway) =>
		{
			Room room = createSecretRoom(doorway);
			if (room != null)
			{
				room.spawnEnemies = false;
				return true;
			}

			return false;
		});


		simplex = new Simplex(Hash.hash(seed) + (uint)floor, 3);

		for (int i = 0; i < rooms.Count; i++)
		{
			placeRoom(rooms[i], level, (int x, int y, int idx) =>
			{
				if (idx == 0)
				{
					float progress = 1 - y / (float)level.height;
					float type = simplex.sample2f(x * 0.05f, y * 0.05f) - progress * 0.4f;
					TileType tile = type > -0.1f ? primaryTile : secondaryTile;
					tile = getTile(x, y, idx, tile);
					return tile;
				}
				return secondaryTile;
			});
		}


		float randomFloorChance = 0.2f;
		if (secretDoor == null && random.NextSingle() < randomFloorChance)
		{
			Room room = rooms[random.Next() % rooms.Count];
			if (room.getSpawn(level, random, objectFlags, (Vector2i tile) =>
			{
				return level.getTile(tile) != null && level.getTile(tile).isSolid && level.getTile(tile).health == 1 && getRoom(tile.x, tile.y) != null && (level.getTile(tile.x - 1, tile.y) == null || level.getTile(tile.x + 1, tile.y) == null);
			},
			out Vector2i tile))
			{
				Door door = new Door(null);
				level.addEntity(door, tile + new Vector2(0.5f, 0));
				setObjectFlag(tile.x, tile.y);
				level.setTile(tile.x, tile.y, TileType.dirt);

				secretDoor = door;
			}
		}


		generateNoiseBackground(level, simplex, bgPrimaryTile != null ? bgPrimaryTile : primaryTile, bgSecondaryTile != null ? bgSecondaryTile : secondaryTile);


		Door entranceDoor = floor == 0 ? createEntranceDoor(lastLevel, lastExit) : new Door(lastLevel, lastExit);
		createDoors(spawnStartingRoom, spawnBossRoom, startingRoom, exitRoom, entranceDoor, out Vector2i entrancePosition, out Vector2i exitPosition);

		for (int i = 0; i < rooms.Count; i++)
		{
			if (rooms[i].entity != null)
			{
				level.addEntity(rooms[i].entity, new Vector2(rooms[i].x, rooms[i].y));
			}
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
						level.setBGTile(x, y, secondaryTile);
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


		List<Item[]> items = generateItems(level.avgLootValue, getDroprates());

		if (spawnStartingWeapon && floor == 0)
		{
			float staffChance = 0.1f;
			ItemType itemType = random.NextSingle() < staffChance ? ItemType.Staff : ItemType.Weapon;
			Item startingWeapon = Item.CreateRandom(ItemType.Weapon, random, 0);
			if (startingWeapon.requiredAmmo != null)
			{
				Item ammo = Item.GetItemPrototype(startingWeapon.requiredAmmo).copy();
				ammo.stackSize = 30;
				items.Add([startingWeapon, ammo]);
			}
			else if (startingWeapon.type == ItemType.Staff)
			{
				Item startingSpell = new MagicArrowSpell();
				items.Add([startingWeapon, startingSpell]);
			}
			else
			{
				items.Add([startingWeapon]);
			}
		}

		//float keyChance = 0.25f;
		//if (random.NextSingle() < keyChance)
		//	items.Add([new IronKey()]);

		Mathf.ShuffleList(deadEnds, random);
		Mathf.ShuffleList(mainRooms, random);


		spawnItems(items, deadEnds);

		lockDeadEnds(deadEnds, items); // this depends on spawned objects


		float lockedChestChance = 0.1f;
		spawnRoomObject(deadEnds, lockedChestChance, false, (Vector2i tile, Random random, Room room) =>
		{
			Item[] item = Item.CreateRandom(random, getDroprates(), getRoomLootValue(room));
			level.addEntity(new Chest(item, random.Next() % 2 == 1, ChestType.Silver), tile + new Vector2(0.5f, 0));

			IronKey key = new IronKey();
			spawnItems([[key]], deadEnds);
		});


		// Fountain
		spawnRoomObject(deadEnds, 0.5f, false, (Vector2i tile, Random random, Room room) =>
		{
			Fountain fountain = new Fountain(random);
			level.addEntity(fountain, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Coins
		spawnRoomObject(deadEnds, 1, true, (Vector2i tile, Random random, Room room) =>
		{
			int amount = Mathf.RandomInt(2, 7, random);
			level.addEntity(new CoinStack(amount), new Vector2(tile.x + 0.5f, tile.y + 0.5f));
		});

		// Gems
		spawnRoomObject(deadEnds, 0.2f, true, (Vector2i tile, Random random, Room room) =>
		{
			Item gem = Item.CreateRandom(ItemType.Gem, random, getRoomLootValue(room));
			level.addEntity(new ItemEntity(gem), tile + 0.5f);
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
						level.addEntity(createExplosiveObject(), new Vector2(x + 0.5f, y));
					}
					else
					{
						level.addEntity(createContainer(null), new Vector2(x + 0.5f, y));
					}
					setObjectFlag(x, y);
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
						level.addEntity(new Rock(), new Vector2(x + 0.5f, y));
					}
					else
					{
						level.addEntity(new Skull(), new Vector2(x + 0.5f, y));
					}
					setObjectFlag(x, y);
				}
			}
		});

		// Anvil
		if (floor == getNumFloors() - 1)
		{
			spawnRoomObject(deadEnds, deadEnds.Count, false, (Vector2i tile, Random random, Room room) =>
			{
				level.addEntity(new Anvil(), new Vector2(tile.x + 0.5f, tile.y));
			});
		}


		spawnEnemies(createEnemy, entrancePosition);


		if (spawnBossRoom)
		{
			spawnRoomObject([exitRoom], 1.0f, false, (Vector2i pos, Random random, Room room) =>
			{
				spawnNPC(pos.x, pos.y);
			});
		}

		spawnRoomObject(deadEnds, 1.0f, false, (Vector2i tile, Random random, Room room) =>
		{
			spawnNPC(tile.x, tile.y);
		});


		onFloorFinish(level);

		/*
		// Builder merchant
		spawnRoomObject(deadEnds, 0.5f, false, (Vector2i tile, Random random, Room room) =>
		{
			BuilderMerchant npc = new BuilderMerchant(random, level);
			npc.direction = random.Next() % 2 * 2 - 1;
			level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Traveller merchant
		spawnRoomObject(deadEnds, 0.2f, false, (Vector2i tile, Random random, Room room) =>
		{
			TravellingMerchant npc = new TravellingMerchant(random, level);
			npc.direction = random.Next() % 2 * 2 - 1;
			level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Logan
		spawnRoomObject(deadEnds, 0.3f, false, (Vector2i tile, Random random, Room room) =>
		{
			Logan npc = new Logan(random, level);
			npc.direction = random.Next() % 2 * 2 - 1;
			level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Blacksmith
		spawnRoomObject(deadEnds, 0.5f, false, (Vector2i tile, Random random, Room room) =>
		{
			Blacksmith npc = new Blacksmith(random, level);
			npc.direction = random.Next() % 2 * 2 - 1;
			level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Tinkerer
		spawnRoomObject(deadEnds, 0.3f, false, (Vector2i tile, Random random, Room room) =>
		{
			Tinkerer npc = new Tinkerer(random, level);
			npc.direction = random.Next() % 2 * 2 - 1;
			level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Rat NPC
		if (!GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_RAT_MET) || GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_RAT_QUESTLINE_COMPLETED) && !ratSpawned)
		{
			spawnRoomObject(deadEnds, !GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_RAT_MET) ? 0.7f : 0.1f, false, (Vector2i tile, Random random, Room room) =>
			{
				RatNPC npc = new RatNPC();
				npc.direction = random.Next() % 2 * 2 - 1;
				level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
				ratSpawned = true;
			});
		}
		*/

		level.updateLightmap(0, 0, width, height);
	}

	void generateBossFloor(Level level, Level nextLevel, Level lastLevel, Door lastDoor)
	{
		Room room = generateSingleRoomLevel(level, specialSet, 4, primaryTile, secondaryTile);

		//level.fogFalloff = 0.1f;
		//level.fogColor = new Vector3(0.0f);

		level.entrance.destination = lastLevel;
		level.entrance.otherDoor = lastDoor;
		lastDoor.otherDoor = level.entrance;

		Simplex simplex = new Simplex(Hash.hash(seed) + (uint)level.floor, 3);
		generateNoiseBackground(level, simplex, primaryTile, secondaryTile);

		level.addEntity(createBossRoom(room));
	}

	Room createSecretRoom(Doorway doorway)
	{
		RoomDef[] secretRooms = getSecretRoomDefs();

		int type = random.Next() % secretRooms.Length;
		RoomDef roomDef = secretRooms[type];
		Room room = doorway != null ? fillDoorway(doorway, roomDef, roomDef.set) : new Room(roomDef);
		if (room != null)
			room.entity = createSecretRoomEntity(type, room);

		return room;
	}

	public void setObjectFlag(int x, int y)
	{
		objectFlags[x + y * level.width] = true;
	}

	public bool getObjectFlag(int x, int y)
	{
		return objectFlags[x + y * level.width];
	}

	int countLadderHeight(int x, int y, RoomDef def)
	{
		int result = 0;
		while (true)
		{
			MapTile tile = def.getTile(x, y + result);
			if (tile == MapTile.Ladder || tile == MapTile.LadderPlatform)
				result++;
			else
				break;
		}
		return result;
	}

	TileType translateMapTile(MapTile tile, int x, int y, int xx, int yy, Room room, RoomDef roomDef, Level level, Func<int, int, int, TileType> getTileFunc)
	{
		switch (tile)
		{
			case MapTile.None:
				return null;
			case MapTile.Tile0:
				return getTileFunc(x + xx, y + yy, 0);
			case MapTile.Tile1:
				return getTileFunc(x + xx, y + yy, 1);
			case MapTile.Tile2:
				return getTileFunc(x + xx, y + yy, 2);
			case MapTile.PushableBlock:
				level.addEntity(new PushableBlock(), new Vector2(x + xx + 0.5f, y + yy + 0.5f));
				setObjectFlag(x + xx, y + yy);
				return null;
			case MapTile.Platform:
				return TileType.platform;
			case MapTile.ArrowTrap:
				return TileType.dummy;
			case MapTile.Ladder:
				if (yy == room.set.height - 1 ||
					(roomDef.getTile(xx, yy - 1) != MapTile.Ladder && roomDef.getTile(xx, yy - 1) != MapTile.LadderPlatform))
					level.addEntity(new Ladder(countLadderHeight(xx, yy, roomDef)), new Vector2(x + xx, y + yy));
				return null;
			case MapTile.Trampoline:
				level.addEntity(new Trampoline(), new Vector2(x + xx + 0.5f, y + yy));
				setObjectFlag(x + xx, y + yy);
				return null;
			case MapTile.LadderPlatform:
				return TileType.platform;
			case MapTile.Water:
				return TileType.water;
			case MapTile.Spike:
				level.addEntity(new Spike(), new Vector2(x + xx, y + yy));
				setObjectFlag(x + xx, y + yy);
				return null;
			case MapTile.ExplosiveObject:
				level.addEntity(createExplosiveObject != null ? createExplosiveObject() : new ExplosiveBarrel(), new Vector2(x + xx + 0.5f, y + yy));
				setObjectFlag(x + xx, y + yy);
				return null;
			case MapTile.ItemSpawn:
				room.spawnLocations.Add(new Vector2i(xx, yy));
				return null;
			case MapTile.EnemySpawn:
				room.spawnLocations.Add(new Vector2i(xx, yy));
				return null;
			case MapTile.RandomTile:
				float tileType = random.NextSingle();
				if (tileType < 0.5f)
					return null;
				else if (tileType < 0.95f)
					return getTileFunc(x + xx, y + yy, 0);
				return getTileFunc(x + xx, y + yy, 1);
			case MapTile.Placeholder:
				return TileType.dummy;
			default:
				Debug.Assert(false);
				return null;
		}
	}

	void placeRoom(Room room, Level level, Func<int, int, int, TileType> getTileFunc)
	{
		int x = room.x;
		int y = room.y;
		int width = room.width;
		int height = room.height;
		RoomDef roomDef = room.set.roomDefs[room.roomDefID];

		for (int yy = 0; yy < height; yy++)
		{
			for (int xx = 0; xx < width; xx++)
			{
				//uint color = rooms[roomDef.x + xx + (roomDef.y + roomDef.height - yy - 1) * roomsInfo.width];
				MapTile color = roomDef.getTile(xx, yy);
				TileType tile = translateMapTile(color, x, y, xx, yy, room, roomDef, level, getTileFunc);
				if (tile != TileType.dummy)
					level.setTile(x + xx, y + yy, tile);
			}
		}

		if (roomDef.markers != null)
		{
			for (int i = 0; i < roomDef.markers.Count; i++)
			{
				room.addMarker(roomDef.markers[i].id, x + roomDef.markers[i].position.x, y + roomDef.markers[i].position.y);
			}
		}

		if (roomDef.entities != null)
		{
			for (int j = 0; j < roomDef.entities.Count; j++)
			{
				Entity entity = EntityType.CreateInstance(roomDef.entities[j].name);
				if (entity != null)
				{
					level.addEntity(entity, roomDef.getEntityPosition(room, j));
				}
			}
		}

		/*
		for (int i = 0; i < room.doorways.Count; i++)
		{
			Doorway doorway = room.doorways[i];
			if (doorway.otherDoorway == null)
			{
				int xx = room.x + doorway.position.x;
				int yy = room.y + doorway.position.y;
				level.setTile(xx, yy, getTileFunc(xx, yy));
			}
		}
		*/
	}

	void placeRoomBG(Room room, Level level, Func<int, int, int, TileType> getTileFunc)
	{
		int x = room.x;
		int y = room.y;
		int width = room.width;
		int height = room.height;
		RoomDef roomDef = room.set.roomDefs[room.roomDefID];

		for (int yy = 0; yy < height; yy++)
		{
			for (int xx = 0; xx < width; xx++)
			{
				//uint color = rooms[roomDef.x + xx + (roomDef.y + roomDef.height - yy - 1) * roomsInfo.width];
				MapTile color = roomDef.getTile(xx, yy);
				TileType tile = translateMapTile(color, x, y, xx, yy, room, roomDef, level, getTileFunc);
				if (tile != null)
					level.setBGTile(x + xx, y + yy, tile);
			}
		}

		/*
		for (int i = 0; i < room.doorways.Count; i++)
		{
			Doorway doorway = room.doorways[i];
			if (doorway.otherDoorway == null)
			{
				int xx = room.x + doorway.position.x;
				int yy = room.y + doorway.position.y;
				level.setTile(xx, yy, getTileFunc(xx, yy));
			}
		}
		*/
	}

	bool fitRoom(Vector2i position, Vector2i size, List<Room> rooms, int width, int height)
	{
		if (position.x < 1 || position.x + size.x > width - 1 || position.y < 0 || position.y + size.y > height - 1)
			return false;
		for (int i = 0; i < rooms.Count; i++)
		{
			if (position.x + size.x > rooms[i].x && position.x < rooms[i].x + rooms[i].width &&
				position.y + size.y > rooms[i].y && position.y < rooms[i].y + rooms[i].height)
			{
				int xx = position.x - rooms[i].x;
				int yy = position.y - rooms[i].y;

				for (int y = yy; y < rooms[i].height; y++)
				{
					for (int x = xx; x < rooms[i].width; x++)
					{
						if (x >= 0 && x < rooms[i].width && y >= 0 && y < rooms[i].height)
						{
							MapTile tile = rooms[i].set.roomDefs[rooms[i].roomDefID].data[x + y * rooms[i].width];
							if (tile != MapTile.Placeholder)
							{
								return false;
							}
						}
					}
				}
			}
		}
		return true;
	}

	Room fillDoorway(Doorway lastDoorway, RoomDefSet set, bool allowDeadEnd = true)
	{
		Room lastRoom = lastDoorway.room;
		Vector2i matchingDirection = -lastDoorway.direction;

		List<RoomDef> candidates = new List<RoomDef>();
		candidates.AddRange(set.roomDefs);
		Mathf.ShuffleList(candidates, random);

		for (int i = 0; i < candidates.Count; i++)
		{
			// check if matching
			RoomDef def = candidates[i];

			if (def.doorDefs.Count == 1 && !allowDeadEnd)
				continue;

			for (int j = 0; j < def.doorDefs.Count; j++)
			{
				if (def.doorDefs[j].direction == matchingDirection)
				{
					Vector2i roomPosition = new Vector2i(lastRoom.x, lastRoom.y) + lastDoorway.position + lastDoorway.direction - def.doorDefs[j].position;
					Vector2i roomSize = new Vector2i(def.width, def.height);
					if (fitRoom(roomPosition, roomSize, rooms, level.width, level.height))
					{
						Room room = new Room
						{
							x = roomPosition.x,
							y = roomPosition.y,
							width = roomSize.x,
							height = roomSize.y,
							roomDefID = def.id,
							set = set
						};
						for (int k = 0; k < def.doorDefs.Count; k++)
						{
							Doorway doorway = new Doorway(room, def.doorDefs[k]);
							if (k == j)
							{
								doorway.otherDoorway = lastDoorway;
								lastDoorway.otherDoorway = doorway;
							}
							room.doorways.Add(doorway);
						}

						rooms.Add(room);

						return room;
					}
				}
			}
		}

		return null;
	}

	Room fillDoorway(Doorway lastDoorway, RoomDef def, RoomDefSet set)
	{
		Room lastRoom = lastDoorway.room;
		Vector2i matchingDirection = -lastDoorway.direction;

		for (int j = 0; j < def.doorDefs.Count; j++)
		{
			if (def.doorDefs[j].direction == matchingDirection)
			{
				Vector2i roomPosition = new Vector2i(lastRoom.x, lastRoom.y) + lastDoorway.position + lastDoorway.direction - def.doorDefs[j].position;
				Vector2i roomSize = new Vector2i(def.width, def.height);
				if (fitRoom(roomPosition, roomSize, rooms, level.width, level.height))
				{
					Room room = new Room
					{
						x = roomPosition.x,
						y = roomPosition.y,
						width = roomSize.x,
						height = roomSize.y,
						roomDefID = def.id,
						set = set
					};
					for (int k = 0; k < def.doorDefs.Count; k++)
					{
						Doorway doorway = new Doorway(room, def.doorDefs[k]);
						if (k == j)
						{
							doorway.otherDoorway = lastDoorway;
							lastDoorway.otherDoorway = doorway;
						}
						room.doorways.Add(doorway);
					}

					rooms.Add(room);

					return room;
				}
			}
		}

		return null;
	}

	Room fillDoorway(Doorway lastDoorway, RoomDefSet set, int id)
	{
		return fillDoorway(lastDoorway, set.roomDefs[id], set);
	}

	bool fillDoorway(Doorway lastDoorway, Room room)
	{
		Room lastRoom = lastDoorway.room;
		Vector2i matchingDirection = -lastDoorway.direction;

		RoomDef def = room.set.roomDefs[room.roomDefID];

		for (int j = 0; j < def.doorDefs.Count; j++)
		{
			if (def.doorDefs[j].direction == matchingDirection)
			{
				Vector2i roomPosition = new Vector2i(lastRoom.x, lastRoom.y) + lastDoorway.position + lastDoorway.direction - def.doorDefs[j].position;
				Vector2i roomSize = new Vector2i(def.width, def.height);
				if (fitRoom(roomPosition, roomSize, rooms, level.width, level.height))
				{
					for (int k = 0; k < def.doorDefs.Count; k++)
					{
						Doorway doorway = new Doorway(room, def.doorDefs[k]);
						if (k == j)
						{
							doorway.otherDoorway = lastDoorway;
							lastDoorway.otherDoorway = doorway;
						}
						room.doorways.Add(doorway);
					}

					rooms.Add(room);

					return true;
				}
			}
		}

		return false;
	}

	void spawnItem(int x, int y, Item[] items)
	{
		float chestChance = 0.1f;
		float barrelChance = 0.4f;

		float f = random.NextSingle();
		if (f < chestChance)
		{
			float scamChestChance = 0.05f;
			bool scam = random.NextSingle() < scamChestChance;
			if (scam)
				items = [new Bomb().cook()];

			TileType left = level.getTile(x - 1, y);
			TileType right = level.getTile(x + 1, y);
			Chest chest = new Chest(items, left != null && right == null);
			level.addEntity(chest, new Vector2(x + 0.5f, y));

			float chestCoinsChance = 0.03f;
			if (random.NextSingle() < chestCoinsChance)
			{
				int amount = Mathf.RandomInt(10, 20, random);
				chest.coins = amount;
			}
		}
		else if (f < chestChance + barrelChance)
		{
			Container container = createContainer(items);
			level.addEntity(container, new Vector2(x + 0.5f, y));

			float coinsChance = 0.08f;
			if (random.NextSingle() < coinsChance)
			{
				int amount = Mathf.RandomInt(1, 6, random);
				container.coins = amount;
			}
		}
		else
		{
			foreach (Item item in items)
			{
				ItemEntity itemEntity = new ItemEntity(item);
				level.addEntity(itemEntity, new Vector2(x + 0.5f, y + 0.5f));
			}
		}

		objectFlags[x + y * level.width] = true;
	}

	public void spawnChest(int x, int y, float roomLootValue, bool locked = false)
	{
		float scamChestChance = 0.02f;
		bool scam = random.NextSingle() < scamChestChance;

		TileType left = level.getTile(x - 1, y);
		TileType right = level.getTile(x + 1, y);
		Item[] items = scam ? [new Bomb().cook()] : Item.CreateRandom(random, getDroprates(), roomLootValue);
		Chest chest = new Chest(items, left != null && right == null);
		level.addEntity(chest, new Vector2(x + 0.5f, y));

		float chestCoinsChance = 0.03f;
		if (random.NextSingle() < chestCoinsChance)
		{
			int amount = Mathf.RandomInt(10, 20, random);
			chest.coins = amount;
		}

		objectFlags[x + y * level.width] = true;
	}

	List<Item[]> generateItems(float avgValue, float[] dropRates)
	{
		int numItems = 3; // getNumItems(level.floor); // rooms.Count / 4;
		int totalValue = (int)MathF.Ceiling(numItems * avgValue);
		List<Item[]> items = new List<Item[]>();
		while (totalValue > 0)
		{
			Item[] item = Item.CreateRandom(random, dropRates, Mathf.RandomFloat(1, totalValue));
			items.Add(item);
			totalValue -= item[0].getValue();
		}
		return items;
	}

	void spawnItems(List<Item[]> items, List<Room> deadEnds)
	{
		while (items.Count > 0)
		{
			Item[] item = items[0];

			bool placed = false;
			foreach (Room room in rooms)
			{
				foreach (Vector2i spawnLocation in room.spawnLocations)
				{
					if (random.NextSingle() < 0.5f)
					{
						if (!getObjectFlag(spawnLocation.x, spawnLocation.y))
						{
							spawnItem(spawnLocation.x, spawnLocation.y, item);
							placed = true;
						}
					}
				}
			}

			if (!placed)
			{
				placed = spawnRoomObject(deadEnds, deadEnds.Count, false, (Vector2i tile, Random random, Room room) =>
				{
					spawnItem(tile.x, tile.y, item);
				});
			}

			if (!placed)
			{
				placed = spawnRoomObject(rooms, rooms.Count, false, (Vector2i tile, Random random, Room room) =>
				{
					spawnItem(tile.x, tile.y, item);
				});
			}

			//Debug.Assert(placed);
			if (placed)
			{
				items.RemoveAt(0);
			}
		}
	}

	public void spawnNPC(int x, int y)
	{
		NPC npc = createNPC(random.Next(), level);
		npc.direction = random.Next() % 2 * 2 - 1;
		level.addEntity(npc, new Vector2(x + 0.5f, y));
		setObjectFlag(x, y);

		//spawnedNPCs.Add(npc.GetType());
	}

	Room propagateMainRooms(Doorway doorway, RoomDefSet set, bool firstLeafPath, int minRooms)
	{
		Room room = fillDoorway(doorway, set, false);

		if (room == null)
			return null;

		room.isMainPath = false;

		List<Doorway> emptyDoorways = new List<Doorway>();
		for (int i = 0; i < room.doorways.Count; i++)
		{
			if (room.doorways[i].otherDoorway == null)
			{
				if (firstLeafPath)
				{
					// prevent the main path from leading upwards and being unreachable
					if (room.doorways[i].direction != Vector2i.Up)
						emptyDoorways.Add(room.doorways[i]);
				}
				else
				{
					emptyDoorways.Add(room.doorways[i]);
				}
			}
		}

		if (emptyDoorways.Count > 0)
		{
			Mathf.ShuffleList(emptyDoorways, random);

			while (emptyDoorways.Count > 0 /*&& (rooms.Count <= minRooms || firstLeafPath)*/)
			{
				propagateMainRooms(emptyDoorways[0], set, firstLeafPath, minRooms);
				firstLeafPath = false;
				emptyDoorways.RemoveAt(0);
			}
		}

		return room;
	}

	void generateMainRooms(RoomDefSet set, RoomDef? startingRoomDef, bool spawnBossRoom, int minRooms = 10)
	{
		// Starting room

		int roomDefID;
		RoomDef roomDef;

		if (startingRoomDef != null)
		{
			roomDefID = startingRoomDef.Value.id;
			roomDef = startingRoomDef.Value;
		}
		else
		{
			roomDefID = random.Next() % set.roomDefs.Count;
			roomDef = set.roomDefs[roomDefID];
			while (roomDef.height > level.height || roomDef.width > level.width || roomDef.doorDefs.Count < 2)
			{
				roomDefID = random.Next() % set.roomDefs.Count;
				roomDef = set.roomDefs[roomDefID];
			}
		}

		//Debug.Assert(level.width - roomDef.width - 6 >= 0 && level.height - roomDef.height >= 0);

		int startingRoomX = Mathf.RandomInt(1, level.width - roomDef.width - 1, random); // level.width / 2 - roomDef.width / 2; // random.Next() % Math.Max(level.width - roomDef.width - 6, 1) + 3;
		int startingRoomY = spawnBossRoom ? Mathf.RandomInt(1, 8, random) : level.height - roomDef.height - Mathf.RandomInt(1, 8, random); // Math.Max(level.height - roomDef.height - 4, 0); // random.Next() % Math.Max(level.height - roomDef.height - 6, 1) + 3;
		Room room = new Room
		{
			x = startingRoomX,
			y = startingRoomY,
			width = roomDef.width,
			height = roomDef.height,
			roomDefID = roomDefID,
			set = roomDef.set
		};
		room.isMainPath = true;

		if (startingRoomX < 0 || startingRoomY < 0)
			Debug.Assert(false);

		for (int i = 0; i < roomDef.doorDefs.Count; i++)
			room.doorways.Add(new Doorway(room, roomDef.doorDefs[i]));

		rooms.Add(room);


		List<Doorway> emptyDoorways = new List<Doorway>();
		for (int i = 0; i < room.doorways.Count; i++)
		{
			if (room.doorways[i].otherDoorway == null)
				emptyDoorways.Add(room.doorways[i]);
		}
		Mathf.ShuffleList(emptyDoorways, random);
		if (random.NextSingle() < 0.95f)
		{
			emptyDoorways.Sort((Doorway a, Doorway b) =>
			{
				float da = Vector2.Dot((Vector2)a.direction, Vector2.Down);
				float db = Vector2.Dot((Vector2)b.direction, Vector2.Down);
				return da > db ? -1 : db > da ? 1 : 0;
			});
		}

		Debug.Assert(emptyDoorways.Count > 0);

		bool firstLeafPath = true;
		while (emptyDoorways.Count > 0 /*&& (rooms.Count <= minRooms || firstLeafPath)*/)
		{
			propagateMainRooms(emptyDoorways[0], set, firstLeafPath, minRooms);
			firstLeafPath = false;
			emptyDoorways.RemoveAt(0);
		}

		Debug.Assert(rooms.Count > 1);
	}

	void generateExtraRooms(RoomDefSet set, Func<Doorway, bool> createSpecialRoom)
	{
		// Spawn special rooms

		{
			List<Doorway> emptyDoorways = new List<Doorway>();
			for (int i = 0; i < rooms.Count; i++)
			{
				for (int j = 0; j < rooms[i].doorways.Count; j++)
				{
					if (rooms[i].doorways[j].otherDoorway == null)
						emptyDoorways.Add(rooms[i].doorways[j]);
				}
			}
			Mathf.ShuffleList(emptyDoorways, random);
			for (int i = 0; i < emptyDoorways.Count; i++)
			{
				Doorway emptyDoorway = emptyDoorways[i];
				bool specialRoom = random.NextSingle() < 0.5f;
				if (!(specialRoom && createSpecialRoom != null && createSpecialRoom(emptyDoorway)))
					fillDoorway(emptyDoorway, set);
			}
		}

		{
			List<Doorway> emptyDoorways = new List<Doorway>();
			for (int i = 0; i < rooms.Count; i++)
			{
				for (int j = 0; j < rooms[i].doorways.Count; j++)
				{
					if (rooms[i].doorways[j].otherDoorway == null)
						emptyDoorways.Add(rooms[i].doorways[j]);
				}
			}
			Mathf.ShuffleList(emptyDoorways, random);
			emptyDoorways.RemoveRange(emptyDoorways.Count / 3, emptyDoorways.Count - emptyDoorways.Count / 3);
			for (int i = 0; i < emptyDoorways.Count; i++)
			{
				Doorway emptyDoorway = emptyDoorways[i];
				bool specialRoom = random.NextSingle() < 0.1f;
				if (!(specialRoom && createSpecialRoom != null && createSpecialRoom(emptyDoorway)))
					fillDoorway(emptyDoorway, set);
			}
		}
	}

	void createDoors(bool spawnStartingRoom, bool spawnBossRoom, Room startingRoom, Room exitRoom, Door entranceDoor, out Vector2i entrancePosition, out Vector2i exitPosition)
	{
		entrancePosition = Vector2i.Zero;
		exitPosition = Vector2i.Zero;

		level.entrance = entranceDoor;
		if (lastExit != null)
			lastExit.otherDoor = level.entrance;

		if (spawnStartingRoom)
			entrancePosition = startingRoom.getMarker(0x1);
		else
			startingRoom.getFloorSpawn(level, random, objectFlags, out entrancePosition);
		Debug.Assert(entrancePosition != Vector2i.Zero);

		level.addEntity(level.entrance, new Vector2(entrancePosition.x + 0.5f, entrancePosition.y));
		setObjectFlag(entrancePosition.x, entrancePosition.y);

		if (spawnBossRoom)
		{
			exitPosition = exitRoom.getMarker(0x67);
			level.exit = new Door(nextLevel);
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

			objectFlags[exitPosition.x + exitPosition.y * level.width] = true;
		}
		else
		{
			if (exitRoom.getFloorSpawn(level, random, objectFlags, out exitPosition))
			{
				level.exit = new Door(nextLevel);
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
				if (level.getTile(exitPosition.x, exitPosition.y + 1) == null && !objectFlags[exitPosition.x + (exitPosition.y + 1) * level.width])
				{
					level.addEntity(new TorchEntity(), new Vector2(exitPosition.x + 0.5f, exitPosition.y + 1.5f));
					objectFlags[exitPosition.x + (exitPosition.y + 1) * level.width] = true;
				}

				objectFlags[exitPosition.x + exitPosition.y * level.width] = true;
			}
			else
			{
				Debug.Assert(false);
			}
		}
	}

	bool spawnRoomObject(List<Room> roomList, float chance, bool allowMultiple, Action<Vector2i, Random, Room> spawnFunc, bool floorSpawn = true)
	{
		roomList = new List<Room>(roomList);
		Mathf.ShuffleList(roomList, random);
		roomList.Sort((Room room1, Room room2) =>
		{
			if (!room1.hasObject && room2.hasObject)
				return -1;
			else if (room1.hasObject && !room2.hasObject)
				return 1;
			else
				return 0;
		});

		chance /= roomList.Count;

		bool spawned = false;
		for (int i = 0; i < roomList.Count; i++)
		{
			Room room = roomList[i];
			if (random.NextSingle() < chance)
			{
				if (floorSpawn ? room.getFloorSpawn(level, random, objectFlags, out Vector2i tile) : room.getSpawn(level, random, objectFlags, out tile))
				{
					spawnFunc(tile, random, room);
					objectFlags[tile.x + tile.y * level.width] = true;
					room.hasObject = true;
					spawned = true;
					if (!allowMultiple)
						break;
				}
				else
				{
					chance *= roomList.Count / (float)(roomList.Count - 1);
					roomList.RemoveAt(i--);
				}
			}
		}

		return spawned;
	}

	protected void spawnTileObject(Action<int, int, TileType, TileType, TileType, TileType, TileType> spawnFunc)
	{
		for (int y = 0; y < level.height; y++)
		{
			for (int x = 0; x < level.width; x++)
			{
				if (objectFlags[x + y * level.width])
					continue;

				TileType tile = level.getTile(x, y);
				TileType up = level.getTile(x, y + 1);
				TileType down = level.getTile(x, y - 1);
				TileType left = level.getTile(x - 1, y);
				TileType right = level.getTile(x + 1, y);

				spawnFunc(x, y, tile, left, right, down, up);
			}
		}
	}

	void lockDeadEnds(List<Room> deadEnds, List<Item[]> items)
	{
		if (items.Count == 0)
			return;

		foreach (Room room in deadEnds)
		{
			if (!room.hasObject)
				continue;

			Vector2i doorPosition = Vector2i.Zero;
			for (int i = 0; i < room.doorways.Count; i++)
			{
				if (room.doorways[i].otherDoorway != null)
				{
					doorPosition = new Vector2i(room.x, room.y) + room.doorways[i].position;
					break;
				}
			}

			if ((doorPosition.x == room.x || doorPosition.x == room.x + room.width - 1) && !getObjectFlag(doorPosition.x, doorPosition.y))
			{
				TileType up = level.getTile(doorPosition.x, doorPosition.y + 1);
				TileType down = level.getTile(doorPosition.x, doorPosition.y - 1);

				if (up != null && up.isSolid && down != null && down.isSolid)
				{
					float lockedChance = 0.25f;
					if (random.NextSingle() < lockedChance)
					{
						Debug.Assert(doorPosition != Vector2i.Zero);

						if (room.getFloorSpawn(level, random, objectFlags, out Vector2i pos))
						{
							spawnItem(pos.x, pos.y, items[0]);
							items.RemoveAt(0);
							//spawnItem(pos.x, pos.y, getRoomLootValue(room));
						}

						/*
						Room keyRoom = mainRooms[random.Next() % mainRooms.Count];
						if (keyRoom.getFloorSpawn(level, random, objectFlags, out Vector2i keySpawn))
						{
							Item key = new IronKey();
							float chestChance = 0.2f;
							float barrelChance = 0.4f;
							float f = random.NextSingle();
							if (f < chestChance)
								level.addEntity(new Chest(key), keySpawn + new Vector2(0.5f, 0));
							else if (f < chestChance + barrelChance)
								level.addEntity(new Barrel(key), keySpawn + new Vector2(0.5f, 0));
							else
								level.addEntity(new ItemEntity(key), keySpawn + 0.5f);
							*/

						level.addEntity(new IronDoor("iron_key"), doorPosition + new Vector2(0.5f, 0));

						break;
					}
				}
			}
		}
	}

	Room getRoom(int x, int y)
	{
		foreach (Room room in rooms)
		{
			if (x >= room.x && x < room.x + room.width && y >= room.y && y < room.y + room.height)
				return room;
		}
		return null;
	}

	public float getLootValue(Vector2 position)
	{
		if (level.entrance != null && level.exit != null)
		{
			Vector2 entrancePosition = level.entrance.position;
			Vector2 exitPosition = level.exit.position;
			Vector2 toRoom = position - entrancePosition;
			Vector2 toExit = exitPosition - entrancePosition;
			float progress = Mathf.Clamp(Vector2.Dot(toRoom, toExit.normalized) / toExit.length, 0, 1);
			return Mathf.Lerp(level.avgLootValue * 0.5f, level.avgLootValue * 1.5f, progress);
		}
		else
		{
			return level.avgLootValue;
		}
	}

	public float getRoomLootValue(Room room)
	{
		return getLootValue(new Vector2(room.x + 0.5f * room.width, room.y + 0.5f * room.height));
	}

	public bool spawnEnemy(int x, int y, Mob enemy)
	{
		TileType up = level.getTile(x, y + 1);
		TileType down = level.getTile(x, y - 1);
		TileType left = level.getTile(x - 1, y);
		TileType right = level.getTile(x + 1, y);

		TileType downLeft = level.getTile(x - 1, y - 1);
		TileType downRight = level.getTile(x + 1, y - 1);

		Vector2 exitPosition = level.exit.position;

		float furthestDistance = 0;
		for (int i = 0; i < rooms.Count; i++)
		{
			Vector2 roomCenter = new Vector2(rooms[i].x + 0.5f * rooms[i].width, rooms[i].y + 0.5f * rooms[i].height);
			Vector2 toRoom = roomCenter - exitPosition;
			furthestDistance = MathF.Max(furthestDistance, toRoom.length);
		}

		Vector2 position = new Vector2(x, y);
		float distance = (position - exitPosition).length;
		float progress = Mathf.Remap(distance, 0, furthestDistance, 1, 0);
		if (progress < 0.5f)
			progress *= 0.5f + random.NextSingle();
		else
			progress = 1 - (1 - progress) * (0.5f + random.NextSingle());

		if (!enemy.canFly && enemy.gravity != 0 && left == null && right == null && up == null && down != null && (downLeft != null || downRight != null)
			|| enemy.canFly && left == null && right == null
			|| enemy.gravity == 0 && (down != null || up != null))
		{
			enemy.direction = random.NextSingle() < 0.5f ? 1 : -1;
			float itemDropChance = Mathf.Lerp(0.05f, 0.1f, progress);

			while (itemDropChance > 0 && random.NextSingle() < itemDropChance)
			{
				Item[] drops = Item.CreateRandom(random, getDroprates(), getLootValue(new Vector2(x, y)) * enemy.itemDropValueMultiplier);
				foreach (Item drop in drops)
					enemy.itemDrops.Add(drop);
				itemDropChance--;
			}

			level.addEntity(enemy, new Vector2(x + 0.5f, y + 0.5f));
			objectFlags[x + y * level.width] = true;
			return true;
		}

		return false;
	}

	void spawnEnemies(Func<Level, List<Mob>> createEnemy, Vector2i entrancePosition)
	{
		List<Mob> mobInstances = new List<Mob>();
		int numMobs = rooms.Count * 2 / 3; // Mathf.RandomInt(rooms.Count, rooms.Count * 3 / 2, random);
		for (int i = 0; i < numMobs; i++)
		{
			List<Mob> mobTypes = createEnemy(level);
			float cumulativeRarity = 0;
			foreach (Mob type in mobTypes)
				cumulativeRarity += type.spawnRate;
			float f = random.NextSingle();
			float sum = 0;
			foreach (Mob type in mobTypes)
			{
				sum += type.spawnRate / cumulativeRarity;
				if (f < sum)
				{
					mobInstances.Add(type);
					break;
				}
			}
		}
		for (int i = 0; mobInstances.Count > 0 && i < 1000; i++)
		{
			Mob mob = mobInstances[0];

			spawnRoomObject(rooms, rooms.Count, false, (Vector2i pos, Random random, Room room) =>
			{
				TileType tile = level.getTile(pos);
				TileType left = level.getTile(pos.x - 1, pos.y);
				TileType right = level.getTile(pos.x + 1, pos.y);
				//TileType up = level.getTile(pos.x, pos.y + 1);
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
			}, !mob.canFly);
		}
	}

	public void connectDoors(Door door1, Door door2)
	{
		door1.otherDoor = door2;
		door1.destination = door2.level;
		door2.otherDoor = door1;
		door2.destination = door1.level;
	}

	public Door generateDoor(Level level, uint marker)
	{
		Door door = new Door(null, null);
		Vector2 position = level.rooms[0].getMarker(marker) + new Vector2(0.5f, 0);
		level.addEntity(door, position);
		return door;
	}

	public void generateSingleRoomLevel(Level level, Room room, Room bgRoom, TileType primaryTile, TileType secondaryTile, TileType tertiaryTile = null, uint entranceMarker = 0, uint exitMarker = 0, Door entranceDoor = null, Door exitDoor = null)
	{
		generateSingleRoomLevel(level, room, bgRoom, (int x, int y, int idx) =>
		{
			if (idx == 0)
				return primaryTile;
			else if (idx == 1 && secondaryTile != null)
				return secondaryTile;
			else if (idx == 2 && tertiaryTile != null)
				return tertiaryTile;
			return TileType.stone;
		}, entranceMarker, exitMarker, entranceDoor, exitDoor);
	}

	public void generateSingleRoomLevel(Level level, Room room, Room bgRoom, Func<int, int, int, TileType> getTile, uint entranceMarker = 0, uint exitMarker = 0, Door entranceDoor = null, Door exitDoor = null)
	{
		random = new Random((int)Hash.hash(level.name));

		level.resize(room.width, room.height);

		this.level = level;
		objectFlags = new bool[level.width * level.height];
		Array.Fill(objectFlags, false);

		placeRoom(room, level, getTile);
		level.rooms = [room];

		if (bgRoom != null)
			placeRoomBG(bgRoom, level, getTile);

		RoomDef def = room.set.roomDefs[room.roomDefID];
		for (int i = 0; i < def.doorDefs.Count; i++)
		{
			Vector2 position = (Vector2)def.doorDefs[i].position + def.doorDefs[i].direction;
			Vector2i size = def.doorDefs[i].direction.x != 0 ? new Vector2i(1, 3) : new Vector2i(3, 1);
			if (def.doorDefs[i].direction == Vector2i.Up)
				position += Vector2i.Up;
			LevelTransition door = new LevelTransition(null, null, size, def.doorDefs[i].direction);
			level.addEntity(door, position);
			room.doorways.Add(new Doorway(room, def.doorDefs[i]) { door = door });

			if (level.entrance == null && entranceMarker == 0)
				level.entrance = door;
			else if (level.exit == null && exitMarker == 0)
				level.exit = door;
		}

		if (entranceMarker != 0)
		{
			level.entrance = entranceDoor != null ? entranceDoor : new Door(null, null);
			Vector2 position = room.getMarker(entranceMarker) + new Vector2(0.5f, 0);
			level.addEntity(level.entrance, position);
		}
		if (exitMarker != 0)
		{
			level.exit = exitDoor != null ? exitDoor : new Door(null, null);
			Vector2 position = room.getMarker(exitMarker) + new Vector2(0.5f, 0);
			level.addEntity(level.exit, position);
		}

		if (room.entity != null)
			level.addEntity(room.entity, new Vector2(room.x, room.y));

		level.updateLightmap(0, 0, room.width, room.height);
	}

	Room generateSingleRoomLevel(Level level, RoomDefSet set, int idx, TileType primaryTile, TileType secondaryTile, uint entranceMarker = 0, uint exitMarker = 0, Door entranceDoor = null, Door exitDoor = null)
	{
		RoomDef def = set.roomDefs[idx];
		Room room = new Room
		{
			x = 0,
			y = 0,
			width = def.width,
			height = def.height,
			roomDefID = def.id,
			set = specialSet
		};
		generateSingleRoomLevel(level, room, null, primaryTile, secondaryTile, null, entranceMarker, exitMarker, entranceDoor, exitDoor);
		return room;
	}
}
