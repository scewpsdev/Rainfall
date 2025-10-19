using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract class RoomBiomeGenerator : BiomeGenerator
{
	protected RoomDefSet roomSet;
	protected Dictionary<int, TileType> secondaryTiles = new Dictionary<int, TileType>();

	protected List<Room> rooms = new List<Room>();


	public RoomBiomeGenerator(RoomDefSet roomSet)
	{
		this.roomSet = roomSet;
	}

	public override TileType getBackgroundTile(int x, int y)
	{
		float progress = 1 - y / (float)level.height;
		float type = simplex.sample2f(x * 0.05f, y * 0.05f) - progress * 0.4f;
		return type > -0.1f ? TileType.dirt : TileType.stone;
	}

	int countLadderHeight(int x, int y, RoomDef def)
	{
		int result = 0;
		while (true)
		{
			uint color = def.getTile(x, y + result);
			if (color == 0xFF00FF00 || color == 0xFF00FFFF)
				result++;
			else
				break;
		}
		return result;
	}

	TileType translateTileColor(uint color, int x, int y, int xx, int yy, Room room, RoomDef roomDef, Level level)
	{
		switch (color)
		{
			case 0x00000000:
				return null;
			case 0xFF000000:
				return null;
			case 0xFFFF0000:
				return null;
			case 0xFFFFFFFF:
				return getBackgroundTile(x + xx, y + yy);
			case 0xFF7F7F7F:
				return secondaryTiles[1];
			case 0xFFAFAFAF:
				return secondaryTiles[2];
			case 0xFF0000FF:
				return TileType.platform;
			case 0xFFFF7F7F:
				return TileType.dummy;
			case 0xFF00FF00:
				if (yy == room.set.height - 1 ||
					(roomDef.getTile(xx, yy - 1) != 0xFF00FF00 && roomDef.getTile(xx, yy - 1) != 0xFF00FFFF))
					level.addEntity(new Ladder(countLadderHeight(xx, yy, roomDef)), new Vector2(x + xx, y + yy));
				return null;
			case 0xFFFF7F00:
				level.addEntity(new Trampoline(), new Vector2(x + xx + 0.5f, y + yy));
				return null;
			case 0xFF00FFFF:
				return TileType.platform;
			case 0xFF007fff:
				return TileType.water;
			case 0xFFff6100:
				level.addEntity(new Spike(), new Vector2(x + xx, y + yy));
				return null;
			case 0xFFff9600:
				placeEntity(createExplosiveObject(), new Vector2i(x + xx, y + yy));
				return null;
			case 0xFF00cf5f:
				room.spawnLocations.Add(new Vector2i(xx, yy));
				return null;
			case 0xFFFFFF00:
				float tileType = random.NextSingle();
				if (tileType < 0.5f)
					return null;
				return getBackgroundTile(x + xx, y + yy);
			default:
				if ((color | 0x0000FF00) == 0xFFFFFFFF) // marker
					room.addMarker((color & 0x0000FF00) >> 8, x + xx, y + yy);
				else
					Debug.Assert(false);
				return null;
		}
	}

	public void placeRoom(Room room)
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
				uint color = roomDef.getTile(xx, yy);
				TileType tile = translateTileColor(color, x, y, xx, yy, room, roomDef, level);
				level.setTile(x + xx, y + yy, tile);
			}
		}

		if (room.entity != null)
		{
			level.addEntity(room.entity, room.entity.position);

			Debug.Assert(room.entity is RoomEntity);
			((RoomEntity)room.entity).place(this);
		}
	}

	public void placeRoomBG(Room room)
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
				uint color = roomDef.getTile(xx, yy);
				TileType tile = translateTileColor(color, x, y, xx, yy, room, roomDef, level);
				if (tile != null)
					level.setBGTile(x + xx, y + yy, tile);
			}
		}
	}

	bool checkMask(int x, int y, int width, int height)
	{
		for (int yy = y; yy < y + height; yy++)
		{
			for (int xx = x; xx < x + width; xx++)
			{
				if (!getMask(xx, yy))
					return false;
			}
		}
		return true;
	}

	bool fitRoom(Vector2i position, Vector2i size, List<Room> rooms, int width, int height)
	{
		if (position.x < 1 || position.x + size.x > width - 1 || position.y < 0 || position.y + size.y > height - 1)
			return false;
		if (!checkMask(position.x, position.y, size.x, size.y))
			return false;
		for (int i = 0; i < rooms.Count; i++)
		{
			if (position.x + size.x > rooms[i].x && position.x < rooms[i].x + rooms[i].width &&
				position.y + size.y > rooms[i].y && position.y < rooms[i].y + rooms[i].height)
				return false;
		}
		return true;
	}

	Room fillDoorway(Vector2i position, Vector2i direction, Doorway lastDoorway, RoomDef def, RoomDefSet set)
	{
		Vector2i matchingDirection = -direction;

		for (int j = 0; j < def.doorDefs.Count; j++)
		{
			if (def.doorDefs[j].direction == matchingDirection)
			{
				Vector2i roomPosition = position + direction - def.doorDefs[j].position;
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
							if (lastDoorway != null)
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

	protected Room fillDoorway(Doorway doorway, RoomDefSet set, int defID)
	{
		return fillDoorway(doorway.room.position + doorway.position, doorway.direction, doorway, set.roomDefs[defID], set);
	}

	Room createRandomRoom(Vector2i position, Vector2i direction, Doorway lastDoorway, bool allowDeadEnd)
	{
		List<RoomDef> candidates = new List<RoomDef>();
		candidates.AddRange(roomSet.roomDefs);
		MathHelper.ShuffleList(candidates, random);

		for (int i = 0; i < candidates.Count; i++)
		{
			// check if matching
			RoomDef def = candidates[i];

			if (def.doorDefs.Count == 1 && !allowDeadEnd)
				continue;

			Room room = fillDoorway(position, direction, lastDoorway, def, roomSet);
			if (room != null)
				return room;
		}

		return null;
	}

	protected Room createRandomRoom(Doorway doorway, bool allowDeadEnd)
	{
		return createRandomRoom(doorway.room.position + doorway.position, doorway.direction, doorway, allowDeadEnd);
	}

	Room propagateMainRooms(Doorway doorway, RoomDefSet set, bool firstLeafPath, RoomBiomeGeneratorSettings settings, ref bool destinationReached)
	{
		destinationReached = false;

		Room room = createRandomRoom(doorway.room.position + doorway.position, doorway.direction, doorway, false);

		if (room == null)
			return null;

		room.isMainPath = true;

		float destinationDistance = room.containsPosition(settings.destination + 0.5f) ? 0 : (doorway.room.position + doorway.room.size / 2.0f - (settings.destination + 0.5f) - Math.Max(doorway.room.width, doorway.room.height)).length;
		if (destinationDistance < 10)
		{
			destinationReached = true;
			return room;
		}

		List<Doorway> emptyDoorways = new List<Doorway>();
		for (int i = 0; i < room.doorways.Count; i++)
		{
			if (room.doorways[i].otherDoorway == null)
				emptyDoorways.Add(room.doorways[i]);
		}
		if (settings.destination != Vector2i.Zero)
		{
			emptyDoorways.Sort((Doorway a, Doorway b) =>
			{
				float da = (a.room.position + a.position - settings.destination).length;
				float db = (b.room.position + b.position - settings.destination).length;
				return da < db ? -1 : db < da ? 1 : 0;
			});

			if (random.NextSingle() > settings.destinationStrictness)
				MathHelper.ShuffleList(emptyDoorways, random);
		}
		else
		{
			MathHelper.ShuffleList(emptyDoorways, random);
		}

		Debug.Assert(emptyDoorways.Count > 0);

		while (emptyDoorways.Count > 0 && (rooms.Count <= settings.minMainRooms || firstLeafPath))
		{
			Room nextRoom = propagateMainRooms(emptyDoorways[0], set, firstLeafPath, settings, ref destinationReached);
			if (nextRoom != null && destinationReached)
				firstLeafPath = false;
			emptyDoorways.RemoveAt(0);
		}

		return room;
	}

	void generateMainRooms(Room startingRoom, RoomBiomeGeneratorSettings settings)
	{
		List<Doorway> emptyDoorways = new List<Doorway>();
		for (int i = 0; i < startingRoom.doorways.Count; i++)
		{
			if (startingRoom.doorways[i].otherDoorway == null)
				emptyDoorways.Add(startingRoom.doorways[i]);
		}
		Debug.Assert(emptyDoorways.Count > 0);
		MathHelper.ShuffleList(emptyDoorways, random);

		bool destinationReached = false;
		for (int i = 0; i < emptyDoorways.Count; i++)
		{
			propagateMainRooms(emptyDoorways[i], roomSet, true, settings, ref destinationReached);
			if (destinationReached)
				break;
		}

		Debug.Assert(destinationReached);

		//Debug.Assert(rooms.Count > 1);
	}

	void propagateEmptyDoorways(Func<Doorway, bool> createSpecialRoom)
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
		MathHelper.ShuffleList(emptyDoorways, random);
		for (int i = 0; i < emptyDoorways.Count; i++)
		{
			Doorway emptyDoorway = emptyDoorways[i];
			bool specialRoom = random.NextSingle() < 0.5f;
			if (specialRoom && createSpecialRoom != null && createSpecialRoom(emptyDoorway))
			{

			}
			else
			{
				createRandomRoom(emptyDoorway, false);
			}
		}
	}

	void connectDoors(Door door1, Door door2)
	{
		door1.otherDoor = door2;
		door1.destination = door2.level;
		door2.otherDoor = door1;
		door2.destination = door1.level;
	}

	void spawnSecretLevel(RoomBiomeGeneratorSettings settings)
	{
		Room room = rooms[random.Next() % rooms.Count];
		if (room.getSpawn(level, random, objectFlags, (Vector2i tile) =>
		{
			return level.getTile(tile) != null && level.getTile(tile).isSolid && level.getTile(tile).health == 1 && (level.getTile(tile.x - 1, tile.y) == null || level.getTile(tile.x + 1, tile.y) == null);
		},
		out Vector2i tile))
		{
			Door door = new Door(null);
			placeEntity(door, tile);

			Room secretLevelRoom = settings.createSecretLevel(out uint entranceMarker, out uint exitMarker);
			Level secretLevel = new Level(-1, "secret_level_" + biome, "", secretLevelRoom.width, secretLevelRoom.height, TileType.dirt);

			SingleRoomGenerator secretLevelGenerator = new SingleRoomGenerator(secretLevelRoom, null, this);
			secretLevelGenerator.setup(secretLevel, null, biome, [getLootValue(tile)], seed);
			secretLevelGenerator.generate(entranceMarker, exitMarker);

			door.sprite = new Sprite(Entity.tileset, 5, 4, 2, 2);
			door.rect = new FloatRect(-1, -0.5f, 2, 2);
			door.interactRange = 0.5f;

			connectDoors(secretLevel.entrance, door);
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
							spawnItem(pos, items[0]);
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

	List<Item[]> generateItems(float minValue, float maxValue, float[] dropRates)
	{
		int numItems = Math.Min(rooms.Count / 5, 5); // rooms.Count / 4;
		List<Item[]> items = new List<Item[]>();
		for (int i = 0; i < numItems; i++)
		{
			float value = MathHelper.RandomFloat(minValue, maxValue, random);
			Item[] item = Item.CreateRandom(random, dropRates, value);
			items.Add(item);
		}
		return items;
	}

	Item[] createStartingWeapon()
	{
		float staffChance = 0.1f;
		ItemType itemType = random.NextSingle() < staffChance ? ItemType.Staff : ItemType.Weapon;
		Item startingWeapon = Item.CreateRandom(itemType, random, 0);
		if (startingWeapon.requiredAmmo != null)
		{
			Item ammo = Item.GetItemPrototype(startingWeapon.requiredAmmo).copy();
			ammo.stackSize = 30;
			return [startingWeapon, ammo];
		}
		else if (startingWeapon.type == ItemType.Staff && startingWeapon is not SpellBook)
		{
			Item startingSpell = new MagicArrowSpell();
			return [startingWeapon, startingSpell];
		}
		else
		{
			return [startingWeapon];
		}
	}

	List<Vector2i> generateItemSpawns(List<Room> deadEnds)
	{
		List<Vector2i> spawns = new List<Vector2i>();

		foreach (Room room in rooms)
		{
			foreach (Vector2i spawnLocation in room.spawnLocations)
			{
				spawns.Add(spawnLocation);
			}
		}

		int numItems = Math.Min(rooms.Count / 5, 5); // rooms.Count / 4;

		while (spawns.Count < numItems)
		{
			bool placed = spawnRoomObject(deadEnds, deadEnds.Count, false, (Vector2i tile, Random random, Room room) =>
			{
				spawns.Add(tile);
			});

			if (!placed)
			{
				placed = spawnRoomObject(rooms, rooms.Count, false, (Vector2i tile, Random random, Room room) =>
				{
					spawns.Add(tile);
				});
			}
		}

		return spawns;
	}

	void spawnItems(List<Vector2i> itemSpawns, List<Item[]> items, List<Room> deadEnds)
	{
		for (int i = 0; i < itemSpawns.Count; i++)
		{
			float lootValue = getLootValue(itemSpawns[i]);
			Item[] item = Item.CreateRandom(random, DropRates.caves, lootValue);
			spawnItem(itemSpawns[i], item);
		}

		while (items.Count > 0)
		{
			Item[] item = items[0];

			bool placed = false;

			if (!placed)
			{
				placed = spawnRoomObject(deadEnds, deadEnds.Count, false, (Vector2i tile, Random random, Room room) =>
				{
					spawnItem(tile, item);
				});
			}

			if (!placed)
			{
				placed = spawnRoomObject(rooms, rooms.Count, false, (Vector2i tile, Random random, Room room) =>
				{
					spawnItem(tile, item);
				});
			}

			Debug.Assert(placed);

			items.RemoveAt(0);
		}
	}

	bool spawnRoomObject(List<Room> roomList, float chance, bool allowMultiple, Action<Vector2i, Random, Room> spawnFunc, bool floorSpawn = true)
	{
		MathHelper.ShuffleList(roomList, random);
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
				if (getObjectFlag(x, y))
					continue;
				if (!getMask(x, y))
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

	void spawnObjects(List<Room> deadEnds)
	{
		// Fountain
		spawnRoomObject(deadEnds, 0.5f, false, (Vector2i tile, Random random, Room room) =>
		{
			Fountain fountain = new Fountain(random);
			level.addEntity(fountain, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Coins
		spawnRoomObject(deadEnds, 0.1f, true, (Vector2i tile, Random random, Room room) =>
		{
			int amount = MathHelper.RandomInt(2, 7, random);
			level.addEntity(new CoinStack(amount), new Vector2(tile.x + 0.5f, tile.y + 0.5f));
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
						placeEntity(createExplosiveObject(), new Vector2i(x, y));
					}
					else
					{
						placeEntity(createContainer(null), new Vector2i(x, y));
					}
				}
			}
		});
	}

	public abstract List<NPC> createNPC(Vector2i tile);
	public abstract List<Mob> getEnemyList();

	void spawnEnemies()
	{
		List<Mob> mobInstances = new List<Mob>();
		int numMobs = rooms.Count * 2 / 3; // MathHelper.RandomInt(rooms.Count, rooms.Count * 3 / 2, random);
		for (int i = 0; i < numMobs; i++)
		{
			List<Mob> mobTypes = getEnemyList();
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

					if (room.spawnEnemies && down != null && (downLeft != null && left == null || downRight != null && right == null))
					{
						if (spawnEnemy(pos, mob))
							mobInstances.RemoveAt(0);
					}
				}
			}, !mob.canFly);
		}
	}

	void spawnNPCs(List<Room> deadEnds)
	{
		spawnRoomObject(deadEnds, 0.5f, false, (Vector2i tile, Random random, Room room) =>
		{
			spawnNPC(tile);
		});
	}

	protected delegate Room CreateSecretLevel_t(out uint entranceMarker, out uint exitMarker);

	protected struct RoomBiomeGeneratorSettings
	{
		public Vector2i startPosition;
		public Vector2i startDirection;
		public Vector2i destination;
		public float destinationStrictness;
		public int minMainRooms;
		public int numLeafRooms;
		public Func<Doorway, bool> createSpecialRoom;
		public float secretLevelChance;
		public CreateSecretLevel_t createSecretLevel;
		public bool generateSimplexBackground;
	}

	protected void generateBaseLevel(RoomBiomeGeneratorSettings settings)
	{
		Room startingRoom = createRandomRoom(settings.startPosition, settings.startDirection, null, false);
		Doorway doorway = startingRoom.getDoorway(settings.startPosition + settings.startDirection);
		doorway.otherDoorway = doorway;

		generateMainRooms(startingRoom, settings);

		for (int i = 0; i < settings.numLeafRooms; i++)
		{
			propagateEmptyDoorways(settings.createSpecialRoom);
		}

		for (int i = 0; i < rooms.Count; i++)
		{
			placeRoom(rooms[i]);
		}

		if (random.NextSingle() < settings.secretLevelChance)
			spawnSecretLevel(settings);

		if (settings.generateSimplexBackground)
			generateSimplexBackground();


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


		//List<Item[]> items = generateItems(level.minLootValue, level.maxLootValue, DropRates.caves);

		// Starting weapon
		{
			Item[] startingWeapon = createStartingWeapon();
			Room room = mainRooms[random.Next() % 3];
			spawnRoomObject(mainRooms.Slice(0, 3), 3, false, (Vector2i tile, Random random, Room room) =>
			{
				spawnItem(tile, startingWeapon);
			});
		}

		MathHelper.ShuffleList(deadEnds, random);
		MathHelper.ShuffleList(mainRooms, random);

		List<Vector2i> itemSpawns = generateItemSpawns(deadEnds);

		List<Item[]> items = new List<Item[]>();

		int numKeys = MathHelper.RandomInt(1, 3, random);
		for (int i = 0; i < numKeys; i++)
			items.Add([new IronKey()]);

		lockDeadEnds(deadEnds, items);

		spawnItems(itemSpawns, items, deadEnds);

		spawnObjects(deadEnds);

		spawnEnemies();

		spawnNPCs(deadEnds);
	}

	public override void spawnNPC(Vector2i tile)
	{
		List<NPC> npcs = createNPC(tile);
		NPC npc = npcs[random.Next() % npcs.Count];
		npc.direction = random.Next() % 2 * 2 - 1;
		placeEntity(npc, tile);
	}
}
