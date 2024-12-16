using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


public struct DoorDef
{
	public Vector2i position;
	public Vector2i direction;
}

public struct RoomDef
{
	public int id;
	public RoomDefSet set;

	public int x;
	public int y;
	public int width;
	public int height;
	public bool mirrored;
	public List<DoorDef> doorDefs;

	public uint getTile(int x, int y)
	{
		if (mirrored)
			x = width - x - 1;
		y = height - y - 1;
		x += this.x;
		y += this.y;
		return set.rooms[x + y * set.roomsInfo.width];
	}
}

public class RoomDefSet
{
	public uint[] rooms;
	public TextureInfo roomsInfo;

	public int width { get => roomsInfo.width; }
	public int height { get => roomsInfo.height; }

	public List<RoomDef> roomDefs = new List<RoomDef>();

	public RoomDefSet(string path)
	{
		rooms = Resource.ReadImagePixels(path, out roomsInfo);
		for (int y = 0; y < roomsInfo.height; y++)
		{
			for (int x = 0; x < roomsInfo.width; x++)
			{
				uint pixel = rooms[x + y * roomsInfo.width];
				if (pixel != 0xFFFF00FF)
				{
					uint top = y > 0 ? rooms[x + (y - 1) * roomsInfo.width] : 0xFFFF00FF;
					uint left = x > 0 ? rooms[x - 1 + y * roomsInfo.width] : 0xFFFF00FF;
					if (top == 0xFFFF00FF && left == 0xFFFF00FF)
					{
						int roomWidth = 0, roomHeight = 0;
						for (int xx = x; xx < roomsInfo.width; xx++)
						{
							if (rooms[xx + y * roomsInfo.width] != 0xFFFF00FF)
								roomWidth++;
							else
								break;
						}
						for (int yy = y; yy < roomsInfo.height; yy++)
						{
							if (rooms[x + yy * roomsInfo.width] != 0xFFFF00FF)
								roomHeight++;
							else
								break;
						}

						List<DoorDef> doorDefs = new List<DoorDef>();

						for (int yy = y; yy < y + roomHeight; yy++)
						{
							for (int xx = x; xx < x + roomWidth; xx++)
							{
								if (rooms[xx + yy * roomsInfo.width] == 0xFFFF0000)
								{
									Vector2i doorPosition = new Vector2i(xx - x, roomHeight - (yy - y) - 1);
									Vector2i doorDirection =
										yy == y ? Vector2i.Up :
										yy == y + roomHeight - 1 ? Vector2i.Down :
										xx == x ? Vector2i.Left :
										xx == x + roomWidth - 1 ? Vector2i.Right :
										Vector2i.Zero;
									doorDefs.Add(new DoorDef { position = doorPosition, direction = doorDirection });
								}
							}
						}

						roomDefs.Add(new RoomDef { id = roomDefs.Count, set = this, x = x, y = y, width = roomWidth, height = roomHeight, doorDefs = doorDefs });
					}
				}
			}
		}

		// mirrored defs
		int numRoomDefs = roomDefs.Count;
		for (int i = 0; i < numRoomDefs; i++)
		{
			RoomDef def = roomDefs[i];
			def.id = roomDefs.Count;
			def.mirrored = true;
			def.doorDefs = new List<DoorDef>(roomDefs[i].doorDefs);
			for (int j = 0; j < def.doorDefs.Count; j++)
			{
				DoorDef doorDef = def.doorDefs[j];
				doorDef.position.x = def.width - doorDef.position.x - 1;
				doorDef.direction.x *= -1;
				def.doorDefs[j] = doorDef;
			}
			roomDefs.Add(def);
		}
	}
}

public class Room
{
	public int roomDefID;
	public RoomDefSet set;
	public int x, y;
	public int width, height;
	public List<Doorway> doorways = new List<Doorway>();
	public bool isMainPath = false;
	public bool hasObject = false;
	public bool spawnEnemies = true;

	Dictionary<uint, Vector2i> markers = new Dictionary<uint, Vector2i>();
	public List<Vector2i> spawnLocations = new List<Vector2i>();


	public Room()
	{
	}

	public Room(RoomDef def)
	{
		x = 0;
		y = 0;
		width = def.width;
		height = def.height;
		roomDefID = def.id;
		set = def.set;
	}

	public Room(string path)
		: this(new RoomDefSet(path).roomDefs[0])
	{
	}

	public void addMarker(uint id, int x, int y)
	{
		markers.Add(id, new Vector2i(x, y));
	}

	public bool tryGetMarker(uint id, out Vector2i value)
	{
		return markers.TryGetValue(id, out value);
	}

	public Vector2i getMarker(uint id)
	{
		if (markers.TryGetValue(id, out Vector2i pos))
			return pos;
		Debug.Assert(false);
		return Vector2i.Zero;
	}

	public int countConnectedDoorways()
	{
		int connectedDoorways = 0;
		for (int j = 0; j < doorways.Count; j++)
		{
			if (doorways[j].otherDoorway != null)
				connectedDoorways++;
		}
		return connectedDoorways;
	}

	public bool getFloorSpawn(Level level, Random random, bool[] objectFlags, out Vector2i pos)
	{
		int offset = random.Next() % this.width;
		for (int i = 0; i < this.width; i++)
		{
			int x = this.x + (offset + i) % this.width;
			for (int y = this.y; y < this.y + this.height; y++)
			{
				if (objectFlags[x + y * level.width])
					break;
				if (y > 0 && level.getTile(x, y) == null && (level.getTile(x, y - 1) == null || level.getTile(x, y - 1).isSolid) && level.getTile(x, y + 1) == null)
				{
					if (level.getTile(x, y - 1) == null)
						level.setTile(x, y - 1, TileType.platform);

					pos = new Vector2i(x, y);
					return true;
				}
			}
		}
		pos = Vector2i.Zero;
		return false;
	}

	public bool containsEntity(Entity entity)
	{
		return entity.position.x >= x + 1 && entity.position.x <= x + width - 1 &&
			entity.position.y >= y + 0.5f && entity.position.y <= y + height - 0.5f;
	}
}

public class Doorway
{
	public Room room;
	public DoorDef doorDef;
	public Doorway otherDoorway;
	public Vector2i position;
	public Vector2i direction;
}

public class LevelGenerator
{
	RoomDefSet miscSet;
	RoomDefSet specialSet;
	RoomDefSet cavesSet;
	RoomDefSet gardensSet;
	RoomDefSet minesSet;

	string seed;
	int floor;
	Level level;
	Random random;
	Simplex simplex;

	Level lastLevel, nextLevel;
	Door entrance;

	List<Room> rooms;

	bool[] objectFlags;
	float[] lootModifier;

	bool ratSpawned = false;


	public LevelGenerator()
	{
		miscSet = new RoomDefSet("res/level/rooms_misc.png");
		specialSet = new RoomDefSet("res/level/rooms_special.png");
		cavesSet = new RoomDefSet("res/level/level1/rooms1.png");
		gardensSet = new RoomDefSet("res/level/level2/rooms2.png");
		minesSet = new RoomDefSet("res/level/rooms_mines.png");
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

	TileType translateTileColor(uint color, int x, int y, int xx, int yy, Room room, RoomDef roomDef, Level level, Func<int, int, TileType> getTileFunc, Func<int, int, TileType> getTileSecondaryFunc, Func<int, int, TileType> getTileTertiaryFunc)
	{
		switch (color)
		{
			case 0xFF000000:
				return null;
			case 0xFFFF0000:
				return null;
			case 0xFFFFFFFF:
				return getTileFunc(x + xx, y + yy);
			case 0xFF7F7F7F:
				return getTileSecondaryFunc != null ? getTileSecondaryFunc(x + xx, y + yy) : TileType.stone;
			case 0xFFAFAFAF:
				return getTileTertiaryFunc != null ? getTileTertiaryFunc(x + xx, y + yy) : TileType.sand;
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
				level.addEntity(new Spring(), new Vector2(x + xx + 0.5f, y + yy));
				return null;
			case 0xFF00FFFF:
				return TileType.platform;
			case 0xFF007fff:
				return TileType.water;
			case 0xFFFFFF00:
				level.addEntity(new Spike(), new Vector2(x + xx, y + yy));
				return null;
			case 0xFF00cf5f:
				room.spawnLocations.Add(new Vector2i(xx, yy));
				return null;
			default:
				if ((color | 0x0000FF00) == 0xFFFFFFFF) // marker
					room.addMarker((color & 0x0000FF00) >> 8, x + xx, y + yy);
				else
					Debug.Assert(false);
				return null;
		}
	}

	void placeRoom(Room room, Level level, Func<int, int, TileType> getTileFunc, Func<int, int, TileType> getTileSecondaryFunc = null, Func<int, int, TileType> getTileTertiaryFunc = null)
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
				uint color = roomDef.getTile(xx, yy);
				TileType tile = translateTileColor(color, x, y, xx, yy, room, roomDef, level, getTileFunc, getTileSecondaryFunc, getTileTertiaryFunc);
				level.setTile(x + xx, y + yy, tile);
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

	void placeRoomBG(Room room, Level level, Func<int, int, TileType> getTileFunc, Func<int, int, TileType> getTileSecondaryFunc = null, Func<int, int, TileType> getTileTertiaryFunc = null)
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
				uint color = roomDef.getTile(xx, yy);
				TileType tile = translateTileColor(color, x, y, xx, yy, room, roomDef, level, getTileFunc, getTileSecondaryFunc, getTileTertiaryFunc);
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
				return false;
		}
		return true;
	}

	Room fillDoorway(Doorway lastDoorway, RoomDefSet set, bool allowDeadEnd = true)
	{
		Room lastRoom = lastDoorway.room;
		Vector2i matchingDirection = -lastDoorway.direction;

		List<RoomDef> candidates = new List<RoomDef>();
		candidates.AddRange(set.roomDefs);
		MathHelper.ShuffleList(candidates, random);

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
							Doorway doorway = new Doorway
							{
								room = room,
								doorDef = def.doorDefs[k],
								otherDoorway = null,
								position = def.doorDefs[k].position,
								direction = def.doorDefs[k].direction
							};
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

	void spawnItem(int x, int y, float roomLootValue)
	{
		float chestChance = 0.1f;
		float barrelChance = 0.6f;

		float scamChestChance = 0.02f;
		bool scam = random.NextSingle() < scamChestChance;

		float f = random.NextSingle();
		if (f < chestChance)
		{
			TileType left = level.getTile(x - 1, y);
			TileType right = level.getTile(x + 1, y);
			Item[] items = scam ? [new Bomb().cook()] : Item.CreateRandom(random, DropRates.chest, roomLootValue);
			Chest chest = new Chest(items, left != null && right == null);
			level.addEntity(chest, new Vector2(x + 0.5f, y));

			float chestCoinsChance = 0.03f;
			if (random.NextSingle() < chestCoinsChance)
			{
				int amount = MathHelper.RandomInt(10, 20, random);
				chest.coins = amount;
			}
		}
		else if (f < chestChance + barrelChance)
		{
			Item[] items = scam ? [new Bomb().cook()] : Item.CreateRandom(random, DropRates.barrel, roomLootValue);
			Barrel barrel = new Barrel(items);
			level.addEntity(barrel, new Vector2(x + 0.5f, y));

			float barrelCoinsChance = 0.08f;
			if (random.NextSingle() < barrelCoinsChance)
			{
				int amount = MathHelper.RandomInt(1, 6, random);
				barrel.coins = amount;
			}
		}
		else
		{
			Item[] items = Item.CreateRandom(random, DropRates.ground, roomLootValue);
			foreach (Item item in items)
			{
				ItemEntity itemEntity = new ItemEntity(item);
				level.addEntity(itemEntity, new Vector2(x + 0.5f, y + 0.5f));
			}
		}

		objectFlags[x + y * level.width] = true;
	}

	void generateMainRooms(RoomDefSet set, RoomDef? startingRoomDef)
	{
		Room lastRoom = null;
		while (true)
		{
			if (lastRoom == null) // First room
			{
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
					while (roomDef.height > level.height || roomDef.width > level.width)
					{
						roomDefID = random.Next() % set.roomDefs.Count;
						roomDef = set.roomDefs[roomDefID];
					}
				}

				Debug.Assert(level.width - roomDef.width - 6 >= 0 && level.height - roomDef.height >= 0);

				int startingRoomX = level.width / 2 - roomDef.width / 2; // random.Next() % Math.Max(level.width - roomDef.width - 6, 1) + 3;
				int startingRoomY = level.height - roomDef.height - 8; // random.Next() % Math.Max(level.height - roomDef.height - 6, 1) + 3;
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

				for (int i = 0; i < roomDef.doorDefs.Count; i++)
				{
					room.doorways.Add(new Doorway { room = room, doorDef = roomDef.doorDefs[i], otherDoorway = null, position = roomDef.doorDefs[i].position, direction = roomDef.doorDefs[i].direction });
				}
				rooms.Add(room);

				lastRoom = room;
			}
			else
			{
				List<Doorway> emptyDoorways = new List<Doorway>();
				for (int i = 0; i < lastRoom.doorways.Count; i++)
				{
					if (lastRoom.doorways[i].otherDoorway == null)
						emptyDoorways.Add(lastRoom.doorways[i]);
				}
				MathHelper.ShuffleList(emptyDoorways, random);
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

				bool found = false;
				for (int s = 0; s < emptyDoorways.Count; s++)
				{
					Doorway lastDoorway = emptyDoorways[s];
					Room room = fillDoorway(lastDoorway, set, false);

					if (room != null)
					{
						room.isMainPath = true;
						lastRoom = room;
						found = true;
						break;
					}
				}
				if (!found)
					break;
			}
		}

		Debug.Assert(rooms.Count > 1);
	}

	void generateExtraRooms(RoomDefSet set)
	{
		// Spawn special rooms
		for (int k = 0; k < 1; k++)
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
				//RoomDefSet set = random.NextSingle() < 0.8f ? set : miscSet;
				fillDoorway(emptyDoorway, set);
			}
		}
	}

	void createDoors(bool spawnStartingRoom, Room startingRoom, Room exitRoom, out Vector2i entrancePosition, out Vector2i exitPosition)
	{
		entrancePosition = Vector2i.Zero;
		exitPosition = Vector2i.Zero;

		if (spawnStartingRoom)
		{
			entrancePosition = new Vector2i(startingRoom.x + startingRoom.width / 2, startingRoom.y + 2);
			level.entrance = new Door(lastLevel, entrance);
			if (entrance != null)
				entrance.otherDoor = level.entrance;
			level.addEntity(level.entrance, new Vector2(entrancePosition.x + 0.5f, entrancePosition.y));

			objectFlags[entrancePosition.x + entrancePosition.y * level.width] = true;
		}
		else
		{
			if (startingRoom.getFloorSpawn(level, random, objectFlags, out entrancePosition))
			{
				level.entrance = new Door(lastLevel, entrance);
				if (entrance != null)
					entrance.otherDoor = level.entrance;
				level.addEntity(level.entrance, new Vector2(entrancePosition.x + 0.5f, entrancePosition.y));

				objectFlags[entrancePosition.x + entrancePosition.y * level.width] = true;
			}
			else
			{
				Debug.Assert(false);
			}
		}

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

			objectFlags[exitPosition.x + exitPosition.y * level.width] = true;
		}
		else
		{
			Debug.Assert(false);
		}
	}

	void spawnRoomObject(List<Room> roomList, float chance, bool allowMultiple, Action<Vector2i, Random, Room> spawnFunc)
	{
		if (random.NextSingle() < chance)
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

			for (int i = 0; i < roomList.Count; i++)
			{
				Room room = roomList[i];
				if (room.getFloorSpawn(level, random, objectFlags, out Vector2i tile))
				{
					spawnFunc(tile, random, room);
					objectFlags[tile.x + tile.y * level.width] = true;
					room.hasObject = true;
					if (!allowMultiple)
						break;
				}
			}
		}
	}

	void spawnTileObject(Action<int, int, TileType, TileType, TileType, TileType, TileType> spawnFunc)
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

	void lockDeadEnds(List<Room> deadEnds, List<Room> mainRooms)
	{
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

			if (doorPosition.x == room.x || doorPosition.x == room.x + room.width - 1)
			{
				TileType up = level.getTile(doorPosition.x, doorPosition.y + 1);
				TileType down = level.getTile(doorPosition.x, doorPosition.y - 1);

				if (up != null && up.isSolid && down != null && down.isSolid)
				{
					float lockedChance = 1; // 0.25f;
					if (random.NextSingle() < lockedChance)
					{
						Debug.Assert(doorPosition != Vector2i.Zero);

						if (room.getFloorSpawn(level, random, objectFlags, out Vector2i pos))
						{
							spawnItem(pos.x, pos.y, getRoomLootValue(room));
						}

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

							level.addEntity(new IronDoor(key.name), doorPosition + new Vector2(0.5f, 0));
						}
						else
						{
							//Debug.Assert(false);
						}

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

	public Level[] generateCaves(string seed)
	{
		/*
		int numCaveFloors = 5;
		int numInbetweenRooms = 3;
		Level[] areaCaves = new Level[numCaveFloors + numInbetweenRooms];
		Vector3 lightAmbience = Vector3.One;
		Vector3 darkAmbience = new Vector3(0.001f);
		areaCaves[0] = new Level(0, "Caves I", 80, 30, TileType.dirt) { ambientLight = lightAmbience };
		areaCaves[1] = new Level(1, "Caves II", 40, 60, TileType.dirt) { ambientLight = lightAmbience };
		areaCaves[2] = new Level(-1, "") { ambientLight = lightAmbience };
		areaCaves[3] = new Level(2, "Caves III", 50, 50, TileType.dirt) { ambientLight = darkAmbience };
		areaCaves[4] = new Level(3, "Caves IV", 30, 30, TileType.dirt) { ambientLight = darkAmbience };
		areaCaves[5] = new Level(-1, "") { ambientLight = lightAmbience };
		areaCaves[6] = new Level(4, "Caves V", 80, 80, TileType.dirt) { ambientLight = lightAmbience };
		areaCaves[7] = new Level(4, "") { ambientLight = lightAmbience };

		Level lastLevel = null;
		Door lastDoor = null;
		for (int i = 0; i < areaCaves.Length; i++)
		{
			bool startingRoom = i == 0;
			level = areaCaves[i];

			if (areaCaves[i].name != "")
			{
				generateCaveFloor(seed, areaCaves[i].floor - areaCaves[0].floor, startingRoom, areaCaves[i], i < areaCaves.Length - 1 ? areaCaves[i + 1] : null, lastLevel, lastDoor);

				areaCaves[i].addEntity(new ParallaxObject(Resource.GetTexture("res/level/level1/parallax1.png", false), 2.0f), new Vector2(areaCaves[i].width, areaCaves[i].height) * 0.5f);
				areaCaves[i].addEntity(new ParallaxObject(Resource.GetTexture("res/level/level1/parallax2.png", false), 1.0f), new Vector2(areaCaves[i].width, areaCaves[i].height) * 0.5f);
			}
			else
			{
				if (i == 2 || i == 5)
					generateRandomCaveFloor(areaCaves[i], i < areaCaves.Length - 1 ? areaCaves[i + 1] : null, lastLevel, lastDoor);
				else if (i == 7)
					generateCaveBossFloor(areaCaves[i], i < areaCaves.Length - 1 ? areaCaves[i + 1] : null, lastLevel, lastDoor);
				else
					Debug.Assert(false);
			}

			lastLevel = areaCaves[i];
			lastDoor = areaCaves[i].exit;
		}

		return areaCaves;
		*/


		Level[] areaCaves = new Level[2];
		Vector3 lightAmbience = Vector3.One;
		Vector3 darkAmbience = new Vector3(0.001f);
		areaCaves[0] = new Level(0, "Caves", 200, 300, TileType.dirt) { ambientLight = darkAmbience };
		areaCaves[1] = new Level(1, "") { ambientLight = lightAmbience };

		generateCaveFloor(seed, 0, true, areaCaves[0], areaCaves[1], null, null);
		areaCaves[0].addEntity(new ParallaxObject(Resource.GetTexture("res/level/level1/parallax1.png", false), 2.0f), new Vector2(areaCaves[0].width, areaCaves[0].height) * 0.5f);
		areaCaves[0].addEntity(new ParallaxObject(Resource.GetTexture("res/level/level1/parallax2.png", false), 1.0f), new Vector2(areaCaves[0].width, areaCaves[0].height) * 0.5f);

		generateCaveBossFloor(areaCaves[1], null, areaCaves[0], areaCaves[0].exit);

		return areaCaves;
	}

	float getRoomLootValue(Room room)
	{
		Vector2 roomPosition = new Vector2(room.x + 0.5f * room.width, room.y + 0.5f * room.height);
		Vector2 entrancePosition = level.entrance.position;
		Vector2 exitPosition = level.exit.position;
		Vector2 toRoom = roomPosition - entrancePosition;
		Vector2 toExit = exitPosition - entrancePosition;
		float progress = Vector2.Dot(toRoom, toExit.normalized) / toExit.length;
		return level.lootValue + progress * 5;
	}

	void generateRandomCaveFloor(Level level, Level nextLevel, Level lastLevel, Door lastDoor)
	{
		RoomDef def = specialSet.roomDefs[1];
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
		Vector2i entrancePosition = new Vector2i(2, 1);
		level.addEntity(level.entrance, new Vector2(entrancePosition.x + 0.5f, entrancePosition.y));
		lastDoor.otherDoor = level.entrance;

		level.exit = new Door(nextLevel);
		Vector2i exitPosition = new Vector2i(14, 1);
		level.addEntity(level.exit, new Vector2(exitPosition.x + 0.5f, exitPosition.y));

		level.addEntity(new CavesShopRoom(room));

		level.updateLightmap(0, 0, def.width, def.height);
	}

	void generateCaveBossFloor(Level level, Level nextLevel, Level lastLevel, Door lastDoor)
	{
		RoomDef def = specialSet.roomDefs[3];
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
		Vector2i entrancePosition = new Vector2i(2, 1);
		level.addEntity(level.entrance, new Vector2(entrancePosition.x + 0.5f, entrancePosition.y));
		lastDoor.otherDoor = level.entrance;

		level.exit = new Door(nextLevel);
		Vector2i exitPosition = new Vector2i(27, 1);
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

		level.addEntity(new CavesBossRoom(room));

		level.updateLightmap(0, 0, def.width, def.height);
	}

	void generateCaveFloor(string seed, int floor, bool spawnStartingRoom, Level level, Level nextLevel, Level lastLevel, Door entrance)
	{
		this.seed = seed;
		this.floor = floor;
		this.level = level;
		this.nextLevel = nextLevel;
		this.lastLevel = lastLevel;
		this.entrance = entrance;

		random = new Random((int)Hash.hash(seed) + floor);
		simplex = new Simplex(Hash.hash(seed) + (uint)floor, 3);
		rooms = new List<Room>();

		int width = level.width;
		int height = level.height;

		//int width = spawnStartingRoom ? MathHelper.RandomInt(60, 80, random) : MathHelper.RandomInt(40, 80, random);
		//int height = Math.Max((floor == 4 ? 3600 : 2400) / width, 20);

		level.rooms = rooms;
		level.ambientSound = Resource.GetSound("res/sounds/ambience.ogg");
		level.fogFalloff = 0.04f;
		level.fogColor = new Vector3(0.1f);

		objectFlags = new bool[width * height];
		Array.Fill(objectFlags, false);

		lootModifier = new float[width * height];
		Array.Fill(lootModifier, 1.0f);

		rooms.Clear();
		generateMainRooms(cavesSet, floor == 0 ? specialSet.roomDefs[2] : null);
		Room startingRoom = rooms[0];
		Room exitRoom = rooms[rooms.Count - 1];
		{
			int i = 2;
			while (exitRoom.width <= 2 || exitRoom.height <= 2)
				exitRoom = rooms[rooms.Count - i++];
		}

		if (spawnStartingRoom)
			startingRoom.spawnEnemies = false;

		generateExtraRooms(cavesSet);

		for (int i = 0; i < rooms.Count; i++)
		{
			placeRoom(rooms[i], level, (int x, int y) =>
			{
				float progress = 1 - y / (float)level.height;
				float type = simplex.sample2f(x * 0.05f, y * 0.05f) - progress * 0.4f;
				return type > -0.1f ? TileType.dirt : TileType.stone;
			});
		}

		createDoors(spawnStartingRoom, startingRoom, exitRoom, out Vector2i entrancePosition, out Vector2i exitPosition);

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
		roomsWithStartingWeapon.Add(deadEnds[0]);
		roomsWithStartingWeapon.Add(deadEnds[1]);
		spawnRoomObject(roomsWithStartingWeapon, 1, false, (Vector2i tile, Random random, Room room) =>
		{
			TileType left = level.getTile(tile.x - 1, tile.y);
			TileType right = level.getTile(tile.x + 1, tile.y);
			Item item = Item.CreateRandom(ItemType.Weapon, random, level.lootValue);
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
		spawnRoomObject(deadEnds, 0.2f, false, (Vector2i tile, Random random, Room room) =>
		{
			Fountain fountain = new Fountain(random);
			level.addEntity(fountain, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Coins
		spawnRoomObject(deadEnds, MathHelper.Remap(floor, 1, 5, 0.05f, 0.02f), true, (Vector2i tile, Random random, Room room) =>
		{
			int amount = MathHelper.RandomInt(2, 7, random);
			level.addEntity(new Gem(amount), new Vector2(tile.x + 0.5f, tile.y + 0.5f));
		});

		// Items
		spawnRoomObject(deadEnds, 0.65f, false, (Vector2i tile, Random random, Room room) =>
		{
			spawnItem(tile.x, tile.y, getRoomLootValue(room));
		});


		MathHelper.ShuffleList(deadEnds, random);
		MathHelper.ShuffleList(mainRooms, random);

		lockDeadEnds(deadEnds, mainRooms);




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
						level.addEntity(new Spring(), new Vector2(x + 0.5f, y));
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
			if (tile == null && down != null)
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
							items = Item.CreateRandom(random, DropRates.barrel, level.lootValue);

						level.addEntity(new Barrel(items), new Vector2(x + 0.5f, y));
					}
					objectFlags[x + y * width] = true;
				}
			}
		});

		// Enemy
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if (tile == null && (left == null && right == null) && !objectFlags[x + y * width] && getRoom(x, y) != startingRoom)
			{
				TileType downLeft = level.getTile(x - 1, y - 1);
				TileType downRight = level.getTile(x + 1, y - 1);

				float distanceToEntrance = (new Vector2i(x, y) - entrancePosition).length;
				Room room = getRoom(x, y);

				if (room.spawnEnemies && (distanceToEntrance > 8 || y < entrancePosition.y) && (downLeft != null || downRight != null))
				{
					float enemyChance = 0.15f;
					if (random.NextSingle() < enemyChance)
					{
						spawnEnemy(x, y, floor);

						/*
						bool flyingEnemy = random.NextSingle() < 0.15f;
						if (flyingEnemy)
						{
							if (down == null)
							{
								Mob enemy;

								float batType = random.NextSingle();
								if (batType < 0.9f)
									enemy = new Bat();
								else
									enemy = new OrangeBat();

								level.addEntity(enemy, new Vector2(x + 0.5f, y + 0.5f));
								objectFlags[x + y * width] = true;
							}
						}
						else
						{
							TileType upUp = level.getTile(x, y + 2);
							if (down != null && up == null && left == null && right == null)
							{
								float enemyType = random.NextSingle();

								Mob enemy;

								//if (enemyType > 0.9f)
								//	enemy = new Bob();
								//else 
								if (enemyType > 0.95f)
									enemy = new Gandalf();
								else if (enemyType > 0.9f)
									enemy = new SkeletonArcher();
								else if (enemyType > 0.85f && upUp == null)
									enemy = new Golem();
								else if (enemyType > 0.8f)
									enemy = new Leprechaun();
								else if (enemyType > 0.6f)
									enemy = new Snake();
								else if (enemyType > 0.3f)
								{
									float spiderType = random.NextSingle();
									if (spiderType < 0.9f)
										enemy = new Spider();
									else
										enemy = new GreenSpider();
								}
								else
								{
									enemy = new Rat();
								}

								level.addEntity(enemy, new Vector2(x + 0.5f, y));
								objectFlags[x + y * width] = true;
							}
						}
						*/
					}
				}
			}
		});


		// Builder merchant
		spawnRoomObject(deadEnds, MathHelper.Remap(floor, 1, 5, 0.1f, 0.03f), false, (Vector2i tile, Random random, Room room) =>
		{
			BuilderMerchant npc = new BuilderMerchant(random, level);
			npc.direction = random.Next() % 2 * 2 - 1;
			level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Traveller merchant
		spawnRoomObject(deadEnds, MathHelper.Remap(floor, 1, 5, 0.01f, 0.05f), false, (Vector2i tile, Random random, Room room) =>
		{
			TravellingMerchant npc = new TravellingMerchant(random, level);
			npc.direction = random.Next() % 2 * 2 - 1;
			level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Logan
		spawnRoomObject(deadEnds, MathHelper.Remap(floor, 1, 5, 0.01f, 0.05f), false, (Vector2i tile, Random random, Room room) =>
		{
			Logan npc = new Logan(random, level);
			npc.direction = random.Next() % 2 * 2 - 1;
			level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Blacksmith
		spawnRoomObject(deadEnds, floor == 3 ? 1 : MathHelper.Remap(floor, 1, 5, 0.1f, 0.02f), false, (Vector2i tile, Random random, Room room) =>
		{
			Blacksmith npc = new Blacksmith(random, level);
			npc.direction = random.Next() % 2 * 2 - 1;
			level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Tinkerer
		spawnRoomObject(deadEnds, MathHelper.Remap(floor, 1, 5, 0.01f, 0.05f), false, (Vector2i tile, Random random, Room room) =>
		{
			Tinkerer npc = new Tinkerer(random, level);
			npc.direction = random.Next() % 2 * 2 - 1;
			level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Rat NPC
		if (!GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_RAT_MET) || GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_RAT_QUESTLINE_COMPLETED) && !ratSpawned)
		{
			spawnRoomObject(deadEnds, !GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_RAT_MET) ? 0.1f : 0.02f, false, (Vector2i tile, Random random, Room room) =>
			{
				RatNPC npc = new RatNPC();
				npc.direction = random.Next() % 2 * 2 - 1;
				level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
				ratSpawned = true;
			});
		}


		level.updateLightmap(0, 0, width, height);
	}

	void spawnEnemy(int x, int y, int floor)
	{
		TileType up = level.getTile(x, y + 1);
		TileType down = level.getTile(x, y - 1);
		TileType left = level.getTile(x - 1, y);
		TileType right = level.getTile(x + 1, y);

		if (up == null && left == null && right == null)
		{
			Vector2 position = new Vector2(x, y);
			Vector2 entrancePosition = level.entrance.position;
			Vector2 exitPosition = level.exit.position;
			Vector2 toPosition = position - entrancePosition;
			Vector2 toExit = exitPosition - entrancePosition;
			float progress = Vector2.Dot(toPosition, toExit.normalized) / toExit.length;

			List<Mob> mobs = new List<Mob>();
			if (down != null)
			{
				mobs.Add(new Rat());
				mobs.Add(new Spider());
				mobs.Add(new Snake());
			}
			if (down == null)
				mobs.Add(new Bat());
			if (down != null)
			{
				mobs.Add(new Slime());
				mobs.Add(new SkeletonArcher());
				mobs.Add(new GreenSpider());
			}
			if (down == null)
				mobs.Add(new OrangeBat());
			if (down != null)
			{
				mobs.Add(new BlueSlime());
				mobs.Add(new Leprechaun());
				mobs.Add(new Gandalf());
			}

			int selection = MathHelper.Clamp((int)((progress + random.NextSingle() * 0.2f) * mobs.Count), 0, mobs.Count - 1);
			Mob enemy = mobs[selection];
			level.addEntity(enemy, new Vector2(x + 0.5f, y - enemy.collider.min.y));
			objectFlags[x + y * level.width] = true;
		}
	}

	public Level[] generateGardens(string seed)
	{
		int numGardenFloors = 3;
		Level[] areaGardens = new Level[numGardenFloors + 3];
		areaGardens[0] = new Level(5, "Gardens I");
		areaGardens[1] = new Level(-1, "");
		areaGardens[2] = new Level(6, "Gardens II");
		areaGardens[3] = new Level(-1, "");
		areaGardens[4] = new Level(7, "Gardens III");
		areaGardens[5] = new Level(7, "");

		Level lastLevel = null;
		Door lastDoor = null;
		for (int i = 0; i < areaGardens.Length; i++)
		{
			level = areaGardens[i];

			if (areaGardens[i].name != "")
			{
				generateGardenFloor(seed, areaGardens[i].floor - areaGardens[0].floor, areaGardens[i], i < areaGardens.Length - 1 ? areaGardens[i + 1] : null, lastLevel, lastDoor);
			}
			else
			{
				if (i == 1 || i == 3)
					generateRandomGardenFloor(areaGardens[i], i < areaGardens.Length - 1 ? areaGardens[i + 1] : null, lastLevel, lastDoor);
				else if (i == 5)
					generateGardenBossFloor(areaGardens[i], i < areaGardens.Length - 1 ? areaGardens[i + 1] : null, lastLevel, lastDoor);
				else
					Debug.Assert(false);
			}

			lastLevel = areaGardens[i];
			lastDoor = areaGardens[i].exit;
		}

		lastDoor.finalExit = true;

		return areaGardens;
	}

	void generateRandomGardenFloor(Level level, Level nextLevel, Level lastLevel, Door lastDoor)
	{
		RoomDef def = specialSet.roomDefs[1];
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

		placeRoom(room, level, (int x, int y) => TileType.dirt);
		level.rooms = [room];

		level.fogFalloff = 0.1f;
		level.fogColor = new Vector3(0.0f);

		level.entrance = new Door(lastLevel, lastDoor);
		Vector2i entrancePosition = new Vector2i(2, 1);
		level.addEntity(level.entrance, new Vector2(entrancePosition.x + 0.5f, entrancePosition.y));
		lastDoor.otherDoor = level.entrance;

		level.exit = new Door(nextLevel);
		Vector2i exitPosition = new Vector2i(14, 1);
		level.addEntity(level.exit, new Vector2(exitPosition.x + 0.5f, exitPosition.y));

		level.addEntity(new GardensShopRoom(room));

		level.updateLightmap(0, 0, def.width, def.height);
	}

	void generateGardenBossFloor(Level level, Level nextLevel, Level lastLevel, Door lastDoor)
	{
		RoomDef def = specialSet.roomDefs[3];
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

		placeRoom(room, level, (int x, int y) => TileType.dirt);
		level.rooms = [room];

		level.fogFalloff = 0.1f;
		level.fogColor = new Vector3(0.0f);

		level.entrance = new Door(lastLevel, lastDoor);
		Vector2i entrancePosition = new Vector2i(2, 1);
		level.addEntity(level.entrance, new Vector2(entrancePosition.x + 0.5f, entrancePosition.y));
		lastDoor.otherDoor = level.entrance;

		level.exit = new Door(nextLevel);
		Vector2i exitPosition = new Vector2i(27, 1);
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

		level.addEntity(new GardensBossRoom(room));

		level.updateLightmap(0, 0, def.width, def.height);
	}

	void generateGardenFloor(string seed, int floor, Level level, Level nextLevel, Level lastLevel, Door entrance)
	{
		this.seed = seed;
		this.floor = floor;
		this.level = level;
		this.nextLevel = nextLevel;
		this.lastLevel = lastLevel;
		this.entrance = entrance;

		random = new Random((int)Hash.hash(seed) + floor);
		simplex = new Simplex(Hash.hash(seed) + (uint)floor, 3);

		int width = MathHelper.RandomInt(40, 80, random);
		int height = Math.Max(2400 / width, 20);

		level.resize(width, height, TileType.dirt);
		level.rooms = rooms;
		level.ambientLight = MathHelper.ARGBToVector(0xFFdcffb5).xyz;
		level.ambientSound = Resource.GetSound("res/level/level2/ambience2.ogg");
		//level.fogColor = MathHelper.ARGBToVector(0xFFa0c7eb).xyz;
		//level.fogFalloff = 0.2f;
		//level.bg = Resource.GetTexture("res/level/level2/bg.png", false);

		objectFlags = new bool[width * height];
		Array.Fill(objectFlags, false);

		lootModifier = new float[width * height];
		Array.Fill(lootModifier, 1.0f);

		rooms.Clear();
		bool spawnStartingRoom = floor == 5;
		generateMainRooms(gardensSet, spawnStartingRoom ? specialSet.roomDefs[4] : null);
		Room mainRoom, startingRoom, exitRoom = null;

		if (floor == GameState.instance.firstGardenFloor)
		{
			mainRoom = rooms[0];
			mainRoom.spawnEnemies = false;
			level.addEntity(new GardensMainRoom(mainRoom));

			startingRoom = rooms[rooms.Count - 1];

			for (int i = rooms.Count - 2; i >= 0; i--)
			{
				if (rooms[i].countConnectedDoorways() == 1)
				{
					exitRoom = rooms[i];
					break;
				}
			}

			Debug.Assert(exitRoom != null);
		}
		else
		{
			startingRoom = rooms[0];
			exitRoom = rooms[rooms.Count - 1];
		}


		generateExtraRooms(gardensSet);

		for (int i = 0; i < rooms.Count; i++)
		{
			Room room = rooms[i];
			int x = room.x;
			int y = room.y;
			int w = room.width;
			int h = room.height;
			RoomDef roomDef = room.set.roomDefs[room.roomDefID];

			// why is this here??
			for (int yy = 0; yy < h; yy++)
			{
				for (int xx = 0; xx < w; xx++)
				{
					uint color = roomDef.getTile(xx, yy);
					if (color != 0xFF000000)
						level.setTile(x + xx, y + yy, TileType.dummy);
					else
						level.setTile(x + xx, y + yy, null);
				}
			}

			placeRoom(rooms[i], level, (int x, int y) =>
			{
				TileType left = level.getTile(x - 1, y);
				TileType right = level.getTile(x + 1, y);
				TileType down = level.getTile(x, y - 1);
				TileType up = level.getTile(x, y + 1);

				bool edgeTile = /*left == null || right == null || down == null ||*/ up == null;
				float type = simplex.sample2f(x * 0.05f, y * 0.05f);
				return edgeTile ? (type > -0.3f ? TileType.grass : TileType.path) : TileType.dirt;
			});
		}

		createDoors(false, startingRoom, exitRoom, out Vector2i entrancePosition, out Vector2i exitPosition);

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


		// Leaves
		{
			//level.addEntity(new ParallaxObject(Resource.GetTexture("res/level/level2/parallax1.png", false), 2.0f), new Vector2(level.width, level.height) * 0.5f);
			//level.addEntity(new ParallaxObject(Resource.GetTexture("res/level/level2/parallax2.png", false), 0.2f), new Vector2(level.width, level.height) * 0.5f);

			Texture leavesHoriz = Resource.GetTexture("res/level/level2/leaves_horiz.png", false);
			Texture leavesVert = Resource.GetTexture("res/level/level2/leaves_vert.png", false);
			Texture leavesCorner = Resource.GetTexture("res/level/level2/leaves_corner.png", false);

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					bool tile = level.getTile(x, y) != null && level.getTile(x, y).isSolid && level.getTile(x, y).visible;
					bool left = level.getTile(x - 1, y) != null && level.getTile(x - 1, y).isSolid && level.getTile(x - 1, y).visible;
					bool down = level.getTile(x, y - 1) != null && level.getTile(x, y - 1).isSolid && level.getTile(x, y - 1).visible;
					bool leftdown = level.getTile(x - 1, y - 1) != null && level.getTile(x - 1, y - 1).isSolid && level.getTile(x - 1, y - 1).visible;

					// top
					if ((!tile || !left) && down && leftdown)
					{
						ParallaxObject parallaxObject = new ParallaxObject(leavesHoriz, (x + y * 19) * 16, 0, 16, 32, 0.0f);
						level.addEntity(parallaxObject, new Vector2(x, y + 1));
					}
					// bottom
					else if (tile && left && (!down || !leftdown))
					{
						ParallaxObject parallaxObject = new ParallaxObject(leavesHoriz, (x + y * 19) * 16, 32, 16, 32, 0.0f);
						level.addEntity(parallaxObject, new Vector2(x, y - 1));
					}
					// left corners top/bottom pieces
					else if (!tile && !left && !leftdown && down ||
						tile && !left && !leftdown && !down)
					{
						ParallaxObject parallaxObject = new ParallaxObject(leavesCorner, (x + y * 19) * 32, 32, 32, 32, 0.0f);
						level.addEntity(parallaxObject, new Vector2(x + 0.5f, y));
					}
					// right corners top/bottom pieces
					else if (!tile && !left && leftdown && !down ||
						!tile && left && !leftdown && !down)
					{
						ParallaxObject parallaxObject = new ParallaxObject(leavesCorner, (x + y * 19) * 32, 32, 32, 32, 0.0f);
						level.addEntity(parallaxObject, new Vector2(x - 0.5f, y));
					}

					// right
					if ((!tile || !down) && left && leftdown)
					{
						ParallaxObject parallaxObject = new ParallaxObject(leavesVert, 16, (y + x * 19) * 16, 16, 16, 0.0f);
						level.addEntity(parallaxObject, new Vector2(x + 0.5f, y));
					}
					// left
					else if (tile && down && (!left || !leftdown))
					{
						ParallaxObject parallaxObject = new ParallaxObject(leavesVert, 0, (y + x * 19) * 16, 16, 16, 0.0f);
						level.addEntity(parallaxObject, new Vector2(x - 0.5f, y));
					}
					// top corners left/right pieces
					else if (!tile && !left && !leftdown && down ||
						!tile && !left && leftdown && !down)
					{
						ParallaxObject parallaxObject = new ParallaxObject(leavesCorner, (x + y * 19) * 32, 32, 32, 32, 0.0f);
						level.addEntity(parallaxObject, new Vector2(x, y - 0.5f));
					}
					// bottom corners left/right pieces
					else if (tile && !left && !leftdown && !down ||
						!tile && left && !leftdown && !down)
					{
						ParallaxObject parallaxObject = new ParallaxObject(leavesCorner, (x + y * 19) * 32, 32, 32, 32, 0.0f);
						level.addEntity(parallaxObject, new Vector2(x, y + 0.5f));
					}
				}
			}
		}


		foreach (Room room in rooms)
		{
			foreach (Vector2i spawnLocation in room.spawnLocations)
			{
				Vector2i pos = new Vector2i(room.x, room.y) + spawnLocation;

				float itemChance = 0.1f;
				if (random.NextSingle() < itemChance || !room.spawnEnemies)
				{
					spawnItem(pos.x, pos.y, getRoomLootValue(room));
				}
				else
				{
					float enemyChance = 0.2f;
					if (random.NextSingle() < enemyChance)
					{
						spawnEnemy(pos.x, pos.y, floor);
					}
				}
			}
		}

		// Fountain
		spawnRoomObject(deadEnds, 1, false, (Vector2i tile, Random random, Room room) =>
		{
			Fountain fountain = new Fountain(random);
			level.addEntity(fountain, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Coins
		spawnRoomObject(deadEnds, MathHelper.Remap(floor, 5, 7, 0.05f, 0.02f), true, (Vector2i tile, Random random, Room room) =>
		{
			int amount = MathHelper.RandomInt(2, 10, random);
			level.addEntity(new Gem(amount), new Vector2(tile.x + 0.5f, tile.y + 0.5f));
		});

		// Items
		spawnRoomObject(deadEnds, 0.65f, false, (Vector2i tile, Random random, Room room) =>
		{
			spawnItem(tile.x, tile.y, getRoomLootValue(room));
		});


		MathHelper.ShuffleList(deadEnds, random);
		MathHelper.ShuffleList(mainRooms, random);

		lockDeadEnds(deadEnds, mainRooms);


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
						level.addEntity(new Spring(), new Vector2(x + 0.5f, y));
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

		// Barrel
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if (tile == null && down != null)
			{
				float barrelChance = MathF.Max(simplex.sample2f(x * 0.03f, y * 0.03f) * 0.3f - 0.1f, 0);
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
						float itemChance = 0.05f;
						if (random.NextSingle() < itemChance)
							items = Item.CreateRandom(random, DropRates.barrel, level.lootValue);

						level.addEntity(new Barrel(items), new Vector2(x + 0.5f, y));
					}
					objectFlags[x + y * width] = true;
				}
			}
		});

		// Enemy
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if (tile == null && (left == null && right == null) && !objectFlags[x + y * width])
			{
				TileType downLeft = level.getTile(x - 1, y - 1);
				TileType downRight = level.getTile(x + 1, y - 1);

				float distanceToEntrance = (new Vector2i(x, y) - entrancePosition).length;
				Room room = getRoom(x, y);

				if (room.spawnEnemies && (distanceToEntrance > 8 || y < entrancePosition.y) && (downLeft != null || downRight != null))
				{
					float enemyChance = 0.2f;
					if (random.NextSingle() < enemyChance)
					{
						spawnEnemy(x, y, floor);
					}
				}
			}
		});


		// Builder merchant
		spawnRoomObject(deadEnds, MathHelper.Remap(floor, 5, 7, 0.1f, 0.03f), false, (Vector2i tile, Random random, Room room) =>
		{
			BuilderMerchant npc = new BuilderMerchant(random, level);
			npc.direction = random.Next() % 2 * 2 - 1;
			level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Traveller merchant
		spawnRoomObject(deadEnds, MathHelper.Remap(floor, 5, 7, 0.01f, 0.05f), false, (Vector2i tile, Random random, Room room) =>
		{
			TravellingMerchant npc = new TravellingMerchant(random, level);
			npc.direction = random.Next() % 2 * 2 - 1;
			level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Logan
		spawnRoomObject(deadEnds, MathHelper.Remap(floor, 5, 7, 0.01f, 0.05f), false, (Vector2i tile, Random random, Room room) =>
		{
			Logan npc = new Logan(random, level);
			npc.direction = random.Next() % 2 * 2 - 1;
			level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Blacksmith
		spawnRoomObject(deadEnds, MathHelper.Remap(floor, 5, 7, 0.1f, 0.02f), false, (Vector2i tile, Random random, Room room) =>
		{
			Blacksmith npc = new Blacksmith(random, level);
			npc.direction = random.Next() % 2 * 2 - 1;
			level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Tinkerer
		spawnRoomObject(deadEnds, MathHelper.Remap(floor, 5, 7, 0.01f, 0.05f), false, (Vector2i tile, Random random, Room room) =>
		{
			Tinkerer npc = new Tinkerer(random, level);
			npc.direction = random.Next() % 2 * 2 - 1;
			level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
		});


		level.updateLightmap(0, 0, width, height);
	}

	public void generateStartingCave(Level level)
	{
		RoomDef def = specialSet.roomDefs[6];
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
		//level.fogColor = new Vector3(0.1f);
		level.fogColor = new Vector3(0.0f);
		level.infiniteEnergy = true;

		level.updateLightmap(0, 0, def.width, def.height);
	}

	public void generateHub(Level level)
	{
		RoomDef def = specialSet.roomDefs[5];
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

		placeRoom(room, level, (int x, int y) => TileType.dirt);
		level.rooms = [room];

		level.fogFalloff = 0.1f;
		//level.fogColor = new Vector3(0.1f);
		level.fogColor = new Vector3(0.0f);
		level.infiniteEnergy = true;

		level.updateLightmap(0, 0, def.width, def.height);
	}

	public void generateCliffside(Level level)
	{
		Room room = new Room("res/level/cliffside/room.png");
		level.resize(room.width, room.height);

		placeRoom(room, level, (int x, int y) => TileType.dirt);
		placeRoomBG(new Room("res/level/cliffside/room1.png"), level, (int x, int y) => TileType.dirt);

		level.rooms = [room];
		level.infiniteEnergy = true;

		level.updateLightmap(0, 0, room.width, room.height);
	}

	public void generateTutorial(Level level)
	{
		RoomDef def = specialSet.roomDefs[1];
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

		placeRoom(room, level, (int x, int y) => TileType.dirt);
		level.rooms = [room];

		level.infiniteEnergy = true;

		level.updateLightmap(0, 0, def.width, def.height);
	}
}
