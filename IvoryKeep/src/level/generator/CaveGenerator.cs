using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CaveEntranceDoor : Door
{
	Sprite stairs;
	Sprite bg;

	public CaveEntranceDoor(Level destination, Door otherDoor)
		: base(destination, otherDoor, true, 0)
	{
		sprite = new Sprite(tileset, 4, 6, 3, 3);
		bg = new Sprite(tileset, 7, 6, 3, 3);
		stairs = new Sprite(tileset, 10, 7, 2, 2);
		rect = new FloatRect(-1.5f, 0, 3, 3);

		collider = new FloatRect(-1.5f, 0.0f, 3, 2);
	}

	public override void render()
	{
		base.render();

		Renderer.DrawSprite(position.x + rect.position.x, position.y + rect.position.y, LAYER_BG + 0.00001f, rect.size.x, rect.size.y, 0, bg, false, 0xFF3F3F3F);

		float width = 2;
		float depth = 0.2f;
		Vector3 vertex0 = ParallaxObject.ParallaxEffect(new Vector3(position + new Vector2(-0.5f * width, 0), 0));
		Vector3 vertex1 = ParallaxObject.ParallaxEffect(new Vector3(position + new Vector2(0.5f * width, 0), 0));
		Vector3 vertex2 = ParallaxObject.ParallaxEffect(new Vector3(position + new Vector2(0.5f * width, 2), depth));
		Vector3 vertex3 = ParallaxObject.ParallaxEffect(new Vector3(position + new Vector2(-0.5f * width, 2), depth));
		Renderer.DrawSpriteEx(vertex0, vertex1, vertex2, vertex3, stairs, false, Vector4.One);
	}
}

public class DungeonEntrance : Door
{
	public DungeonEntrance(Level destination, Door otherDoor)
		: base(destination, otherDoor)
	{
	}

	public override bool canInteract(Player player)
	{
		return player.getItem("lost_sigil") != null || !locked;
	}

	public override void interact(Player player)
	{
		base.interact(player);
		if (player.getItem("lost_sigil") != null)
			player.removeItem(player.getItem("lost_sigil"));
	}
}

public partial class LevelGenerator
{
	Door secretDoor;


	public void generateCaveBackground(Level level, Simplex simplex, TileType tile1, TileType tile2)
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

	public void generateCaves(string seed, out Level[] areaCaves)
	{
		random = new Random((int)Hash.hash(seed));

		int numFloors = 5;
		areaCaves = new Level[numFloors + 1];
		for (int i = 0; i < numFloors; i++)
		{
			int width = 40;
			int height = 40;
			TileType defaultTile = TileType.dirt;

			float wideLevelChance = 0.2f;
			if (random.NextSingle() < wideLevelChance)
				width = 50;
			float highLevelChance = 0.2f;
			if (random.NextSingle() < highLevelChance)
				height = 50;

			float rockyLevelChance = 0.1f;
			if (random.NextSingle() < rockyLevelChance)
				defaultTile = TileType.stone;

			areaCaves[i] = new Level(i, "caves" + i, "Caves " + StringUtils.ToRoman(i + 1), width, height, defaultTile, (i + 1) * 4);
		}
		areaCaves[numFloors + 0] = new Level(-1, "caves" + (numFloors + 0), "");

		List<Mob> createEnemy()
		{
			List<Mob> mobs = new List<Mob>();
			mobs.Add(new Rat());
			mobs.Add(new Spider());
			mobs.Add(new Snake());
			mobs.Add(new Bat());
			mobs.Add(new Slime());
			mobs.Add(new Beetle());
			return mobs;
		};

		createContainer = (Item[] items) => new Barrel(items);

		for (int i = 0; i < numFloors; i++)
		{
			generateCaveFloor(seed, i, i == 0, i == numFloors - 1, areaCaves[i], areaCaves[i + 1], i > 0 ? areaCaves[i - 1] : null, i > 0 ? areaCaves[i - 1].exit : null, createEnemy);
		}

		generateCaveBossFloor(areaCaves[numFloors], null, areaCaves[numFloors - 1], areaCaves[numFloors - 1].exit);

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
			Level level = new Level(-1, "caves_secret_floor", "", 20);
			Room room = createSecretRoom(null);

			generateSingleRoomLevel(level, room, null, TileType.dirt, TileType.rock);

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

	public List<NPC> getCaveNPCList()
	{
		List<NPC> npcs = new List<NPC>();
		npcs.Add(new BuilderMerchant(random, level));
		npcs.Add(new TravellingMerchant(random, level));
		if (!QuestManager.tryGetQuest("logan", "logan_quest", out Quest loganQuest) || loganQuest.state != QuestState.InProgress)
			npcs.Add(new Logan() /*NPCManager.logan*/);
		//npcs.Add(new Tinkerer() /*NPCManager.tinkerer*/);

		if (!GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_RAT_MET) || GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_RAT_QUESTLINE_COMPLETED))
			npcs.Add(new RatNPC() /*NPCManager.rat*/);

		return npcs;
	}

	void generateCaveBossFloor(Level level, Level nextLevel, Level lastLevel, Door lastDoor)
	{
		Room room = generateSingleRoomLevel(level, specialSet, 4, TileType.dirt, TileType.stone);

		level.fogFalloff = 0.1f;
		level.fogColor = new Vector3(0.0f);

		level.entrance.destination = lastLevel;
		level.entrance.otherDoor = lastDoor;
		lastDoor.otherDoor = level.entrance;

		Simplex simplex = new Simplex(Hash.hash(seed) + (uint)level.floor, 3);
		generateCaveBackground(level, simplex, TileType.dirt, TileType.stone);

		level.addEntity(new CavesBossRoom(room));
	}

	Room createSecretRoom(Doorway doorway)
	{
		int type = random.Next() % 5;

		Room room = null;
		if (type == 0)
		{
			room = doorway != null ? fillDoorway(doorway, specialSet, 6) : new Room(specialSet, 6);
			if (room != null)
				room.entity = new CavesSpecialRoom1(room, this);
		}
		else if (type == 1)
		{
			int id = random.Next() % 2 == 0 ? 7 : 8;
			room = doorway != null ? fillDoorway(doorway, specialSet, id) : new Room(specialSet, id);
			if (room != null)
				room.entity = new CavesSpecialRoom2(room, this);
		}
		else if (type == 2)
		{
			int id = random.Next() % 2 == 0 ? 9 : 10;
			room = doorway != null ? fillDoorway(doorway, specialSet, id) : new Room(specialSet, id);
			if (room != null)
				room.entity = new CavesSpecialRoom3(room, this);
		}
		else if (type == 3)
		{
			room = doorway != null ? fillDoorway(doorway, specialSet, 11) : new Room(specialSet, 11);
			if (room != null)
				room.entity = new CavesPlatformingRoom1(room, this);
		}
		else if (type == 4)
		{
			//int id = random.Next() % 2 == 0 ? 14 : 15;
			int id = 0;
			room = doorway != null ? fillDoorway(doorway, cavesSpecialSet, id) : new Room(cavesSpecialSet, id);
			if (room != null)
				room.entity = new PrisonCellRoom(room, this);
		}
		else
		{
			Debug.Assert(false);
		}

		return room;
	}

	void generateCaveFloor(string seed, int floor, bool spawnStartingRoom, bool spawnBossRoom, Level level, Level nextLevel, Level lastLevel, Door lastExit, Func<List<Mob>> createEnemy)
	{
		this.seed = seed;
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
		level.ambientSound = Resource.GetSound("sounds/ambience.ogg");
		level.ambientTrack = caveAmbientTrack;
		level.ambientTrackHasIdleLayer = true;
		//level.fogFalloff = 0.04f;
		//level.fogColor = new Vector3(0.1f);

		float darkLevelChance = 0.05f;
		float dimLevelChance = 0.3f;
		if (random.NextSingle() < darkLevelChance)
			level.ambientLight = new Vector3(0.001f);
		else if (random.NextSingle() < dimLevelChance)
			level.ambientLight = new Vector3(0.2f);
		else
			level.ambientLight = new Vector3(0.5f);

		objectFlags = new bool[width * height];
		Array.Fill(objectFlags, false);

		rooms.Clear();
		RoomDef? startingRoomDef = spawnStartingRoom ? specialSet.roomDefs[2] : spawnBossRoom ? specialSet.roomDefs[3] : null;
		generateMainRooms(cavesSet, startingRoomDef, spawnBossRoom);
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

		generateExtraRooms(cavesSet, (Doorway doorway) =>
		{
			Room room = createSecretRoom(doorway);
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
			placeRoom(rooms[i], level, (int x, int y, int idx) =>
			{
				if (idx == 0)
				{
					float progress = 1 - y / (float)level.height;
					float type = simplex.sample2f(x * 0.05f, y * 0.05f) - progress * 0.4f;
					return type > -0.1f ? TileType.dirt : TileType.stone;
				}
				return TileType.stone;
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

				secretDoor = door;
			}
		}


		generateCaveBackground(level, simplex, TileType.dirt, TileType.stone);


		Door entranceDoor = floor == 0 ? new CaveEntranceDoor(lastLevel, lastExit) : new Door(lastLevel, lastExit);
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
						level.setBGTile(x, y, TileType.stone);
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


		List<Item[]> items = generateItems(level.avgLootValue, DropRates.defaultDroprates);

		if (floor == 0)
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

		lockDeadEnds(deadEnds, items);


		spawnItems(items, deadEnds);


		float lockedChestChance = 0.1f;
		spawnRoomObject(deadEnds, lockedChestChance, false, (Vector2i tile, Random random, Room room) =>
		{
			Item[] item = Item.CreateRandom(random, DropRates.defaultDroprates, getRoomLootValue(room));
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
					objectFlags[x + y * width] = true;
				}
			}
		});


		spawnEnemies(createEnemy, entrancePosition);


		spawnRoomObject(deadEnds, 0.5f, false, (Vector2i tile, Random random, Room room) =>
		{
			spawnNPC(tile.x, tile.y, getCaveNPCList());
		});

		if (spawnBossRoom)
		{
			spawnRoomObject([exitRoom], 1.0f, false, (Vector2i pos, Random random, Room room) =>
			{
				spawnNPC(pos.x, pos.y, getCaveNPCList());
			});
		}

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
}
