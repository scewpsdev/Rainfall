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
	public int mirroredFrom;
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

	public unsafe RoomDefSet(string path, bool createMirroredRooms = true)
	{
		Texture roomsTexture = Resource.GetTexture(path, false, true);
		roomsTexture.getImageData(out ImageData image);
		rooms = new uint[image.width * image.height];
		for (int i = 0; i < image.width * image.height; i++)
			rooms[i] = image.data[i];
		roomsInfo = roomsTexture.info;
		image.free();

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

		if (createMirroredRooms)
		{
			// mirrored defs
			int numRoomDefs = roomDefs.Count;
			for (int i = 0; i < numRoomDefs; i++)
			{
				RoomDef def = roomDefs[i];
				def.id = roomDefs.Count;
				def.mirrored = true;
				def.mirroredFrom = roomDefs[i].id;
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
	public Entity entity = null;

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
				if (y > 0 && level.getTile(x, y) == null && (level.getTile(x, y - 1) == null || level.getTile(x, y - 1).isSolid && level.getTile(x, y - 1).visible) && level.getTile(x, y + 1) == null)
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

	public bool getSpawn(Level level, Random random, bool[] objectFlags, out Vector2i pos)
	{
		int offset = random.Next() % this.width;
		for (int i = 0; i < this.width; i++)
		{
			int x = this.x + (offset + i) % this.width;
			for (int y = this.y; y < this.y + this.height; y++)
			{
				if (objectFlags[x + y * level.width])
					break;
				if (y > 0 && level.getTile(x, y) == null)
				{
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
	public Door door;

	public Doorway(Room room, DoorDef doorDef)
	{
		this.room = room;
		this.doorDef = doorDef;
	}

	public Vector2i position => doorDef.position;
	public Vector2i direction => doorDef.direction;
}

public partial class LevelGenerator
{
	public RoomDefSet specialSet;
	RoomDefSet cavesSet;
	RoomDefSet minesSet;
	RoomDefSet dungeonsSet;
	RoomDefSet gardensSet;

	Func<Item[], Container> createContainer = null;
	Func<ExplosiveObject> createExplosiveObject = null;

	string seed;
	Level level;
	public Random random;
	//Simplex simplex;

	Level lastLevel, nextLevel;
	Door lastExit;

	List<Room> rooms;

	bool[] objectFlags;

	List<Type> spawnedNPCs = new List<Type>();

	bool lockedDoorSpawned = false;


	public LevelGenerator()
	{
		specialSet = new RoomDefSet("level/rooms_special.png", false);
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
			case 0x00000000:
				return null;
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
				level.addEntity(createExplosiveObject != null ? createExplosiveObject() : new ExplosiveBarrel(), new Vector2(x + xx + 0.5f, y + yy));
				setObjectFlag(x + xx, y + yy);
				return null;
			case 0xFF00cf5f:
				room.spawnLocations.Add(new Vector2i(xx, yy));
				return null;
			case 0xFFFFFF00:
				float tileType = random.NextSingle();
				if (tileType < 0.5f)
					return null;
				else if (tileType < 0.95f)
					return getTileFunc(x + xx, y + yy);
				return getTileSecondaryFunc != null ? getTileSecondaryFunc(x + xx, y + yy) : TileType.stone;
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

	void spawnItem(int x, int y, Item[] items)
	{
		float chestChance = 0.1f;
		float barrelChance = 0.4f;

		float f = random.NextSingle();
		if (f < chestChance)
		{
			TileType left = level.getTile(x - 1, y);
			TileType right = level.getTile(x + 1, y);
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
			Container container = createContainer(items);
			level.addEntity(container, new Vector2(x + 0.5f, y));

			float coinsChance = 0.08f;
			if (random.NextSingle() < coinsChance)
			{
				int amount = MathHelper.RandomInt(1, 6, random);
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
		Item[] items = scam ? [new Bomb().cook()] : Item.CreateRandom(random, DropRates.chest, roomLootValue);
		Chest chest = new Chest(items, left != null && right == null);
		level.addEntity(chest, new Vector2(x + 0.5f, y));

		float chestCoinsChance = 0.03f;
		if (random.NextSingle() < chestCoinsChance)
		{
			int amount = MathHelper.RandomInt(10, 20, random);
			chest.coins = amount;
		}

		objectFlags[x + y * level.width] = true;
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

			Debug.Assert(placed);

			items.RemoveAt(0);
		}
	}

	public void spawnNPC(int x, int y, List<NPC> npcs)
	{
		npcs = new List<NPC>(npcs);
		for (int i = 0; i < npcs.Count; i++)
		{
			if (spawnedNPCs.Contains(npcs[i].GetType()))
				npcs.RemoveAt(i--);
		}

		if (npcs.Count == 0)
			return;

		NPC npc = npcs[random.Next() % npcs.Count];
		npc.direction = random.Next() % 2 * 2 - 1;
		level.addEntity(npc, new Vector2(x + 0.5f, y));
		setObjectFlag(x, y);

		spawnedNPCs.Add(npc.GetType());
	}

	Room propagateMainRooms(Doorway doorway, RoomDefSet set, bool firstLeafPath, int minRooms)
	{
		Room room = fillDoorway(doorway, set, false);

		if (room == null)
			return null;

		room.isMainPath = true;

		List<Doorway> emptyDoorways = new List<Doorway>();
		for (int i = 0; i < room.doorways.Count; i++)
		{
			if (room.doorways[i].otherDoorway == null)
				emptyDoorways.Add(room.doorways[i]);
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

		while (emptyDoorways.Count > 0 && (rooms.Count <= minRooms || firstLeafPath))
		{
			propagateMainRooms(emptyDoorways[0], set, firstLeafPath, minRooms);
			firstLeafPath = false;
			emptyDoorways.RemoveAt(0);
		}

		return room;
	}

	void generateMainRooms(RoomDefSet set, RoomDef? startingRoomDef, int minRooms = 10)
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
			while (roomDef.height > level.height || roomDef.width > level.width)
			{
				roomDefID = random.Next() % set.roomDefs.Count;
				roomDef = set.roomDefs[roomDefID];
			}
		}

		Debug.Assert(level.width - roomDef.width - 6 >= 0 && level.height - roomDef.height >= 0);

		int startingRoomX = level.width / 2 - roomDef.width / 2; // random.Next() % Math.Max(level.width - roomDef.width - 6, 1) + 3;
		int startingRoomY = Math.Max(level.height - roomDef.height - 8, 0); // random.Next() % Math.Max(level.height - roomDef.height - 6, 1) + 3;
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

		bool firstLeafPath = true;
		while (emptyDoorways.Count > 0 && (rooms.Count <= minRooms || firstLeafPath))
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
			MathHelper.ShuffleList(emptyDoorways, random);
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
			MathHelper.ShuffleList(emptyDoorways, random);
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
					float lockedChance = 1; // 0.25f;
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
						lockedDoorSpawned = true;

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
		Vector2 entrancePosition = level.entrance.position;
		Vector2 exitPosition = level.exit.position;
		Vector2 toRoom = position - entrancePosition;
		Vector2 toExit = exitPosition - entrancePosition;
		float progress = MathHelper.Clamp(Vector2.Dot(toRoom, toExit.normalized) / toExit.length, 0, 1);
		return MathHelper.Lerp(level.minLootValue, level.maxLootValue, progress);
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
		float progress = MathHelper.Remap(distance, 0, furthestDistance, 1, 0);
		if (progress < 0.5f)
			progress *= 0.5f + random.NextSingle();
		else
			progress = 1 - (1 - progress) * (0.5f + random.NextSingle());

		if (!enemy.canFly && enemy.gravity != 0 && left == null && right == null && up == null && down != null && (downLeft != null || downRight != null)
			|| enemy.canFly && left == null && right == null
			|| enemy.gravity == 0 && (down != null || up != null))
		{
			enemy.direction = random.NextSingle() < 0.5f ? 1 : -1;
			enemy.itemDropChance = MathHelper.Lerp(0.05f, 0.1f, progress);
			level.addEntity(enemy, new Vector2(x + 0.5f, y + 0.5f));
			objectFlags[x + y * level.width] = true;
			return true;
		}

		return false;
	}

	void spawnEnemies(Func<List<Mob>> createEnemy, Vector2i entrancePosition)
	{
		List<Mob> mobInstances = new List<Mob>();
		int numMobs = rooms.Count * 2 / 3; // MathHelper.RandomInt(rooms.Count, rooms.Count * 3 / 2, random);
		for (int i = 0; i < numMobs; i++)
		{
			List<Mob> mobTypes = createEnemy();
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

	public void generateSingleRoomLevel(Level level, Room room, Room bgRoom, TileType primaryTile, TileType secondaryTile, uint entranceMarker = 0, uint exitMarker = 0, Door entranceDoor = null, Door exitDoor = null)
	{
		level.resize(room.width, room.height);

		createContainer = null;
		createExplosiveObject = null;

		this.level = level;
		objectFlags = new bool[level.width * level.height];
		Array.Fill(objectFlags, false);

		placeRoom(room, level, (int x, int y) => primaryTile, (int x, int y) => secondaryTile);
		level.rooms = [room];

		if (bgRoom != null)
			placeRoomBG(bgRoom, level, (int x, int y) => primaryTile, (int x, int y) => secondaryTile);

		RoomDef def = room.set.roomDefs[room.roomDefID];
		for (int i = 0; i < def.doorDefs.Count; i++)
		{
			LevelTransition door = new LevelTransition(null, null, new Vector2i(1, 3), def.doorDefs[i].direction);
			Vector2 position = (Vector2)def.doorDefs[i].position + def.doorDefs[i].direction;
			level.addEntity(door, position);
			room.doorways.Add(new Doorway(room, def.doorDefs[i]) { door = door });

			if (i == 0 && entranceMarker == 0 && level.entrance == null)
				level.entrance = door;
			else if (i == 1 && exitMarker == 0 && level.exit == null)
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

		level.updateLightmap(0, 0, room.width, room.height);
	}

	public Room generateSingleRoomLevel(Level level, RoomDefSet set, int idx, TileType primaryTile, TileType secondaryTile, uint entranceMarker = 0, uint exitMarker = 0, Door entranceDoor = null, Door exitDoor = null)
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
		generateSingleRoomLevel(level, room, null, primaryTile, secondaryTile, entranceMarker, exitMarker, entranceDoor, exitDoor);
		level.infiniteEnergy = true;
		level.ambientSound = Resource.GetSound("sounds/ambience.ogg");
		return room;
	}

	public void generateStartingCave(Level level)
	{
		generateSingleRoomLevel(level, specialSet, 6, TileType.stone, TileType.dirt);

		level.fogFalloff = 0.1f;
		//level.fogColor = new Vector3(0.1f);
		level.fogColor = new Vector3(0.0f);
		level.infiniteEnergy = true;
	}

	public void generateHub(Level level)
	{
		generateSingleRoomLevel(level, new Room("level/hub/hub.png"), new Room("level/hub/hub1.png"), TileType.dirt, TileType.stone);

		level.infiniteEnergy = true;
		level.fogFalloff = 0.1f;
		//level.fogColor = new Vector3(0.1f);
		level.fogColor = new Vector3(0.0f);
	}

	public void generateCliffside(Level level)
	{
		generateSingleRoomLevel(level, new Room("level/cliffside/room.png"), new Room("level/cliffside/room1.png"), TileType.dirt, TileType.stone);

		level.infiniteEnergy = true;
	}

	public void generateTutorial(Level level)
	{
		generateSingleRoomLevel(level, specialSet, 1, TileType.dirt, TileType.stone, 0, 0x0a);

		level.infiniteEnergy = true;

		Simplex simplex = new Simplex(12345, 3);
		generateCaveBackground(level, simplex, TileType.dirt, TileType.stone);
	}

	public void generateIntroBridge(Level level)
	{
		generateSingleRoomLevel(level, new Room("level/intro/bridge.png"), null, TileType.bricks, TileType.stone);
	}
}
