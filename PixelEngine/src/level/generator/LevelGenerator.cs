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

	public RoomDefSet(string path, bool createMirroredRooms = true)
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

public partial class LevelGenerator
{
	RoomDefSet specialSet;
	RoomDefSet cavesSet;
	RoomDefSet gardensSet;
	RoomDefSet dungeonsSet;

	string seed;
	Level level;
	public Random random;
	Simplex simplex;

	Level lastLevel, nextLevel;
	Door entrance;

	List<Room> rooms;

	bool[] objectFlags;
	float[] lootModifier;

	bool ratSpawned = false;
	bool lockedDoorSpawned = false;


	public LevelGenerator()
	{
		specialSet = new RoomDefSet("res/level/rooms_special.png", false);
		cavesSet = new RoomDefSet("res/level/level1/rooms1.png");
		gardensSet = new RoomDefSet("res/level/level2/rooms2.png");
		dungeonsSet = new RoomDefSet("res/level/level3/rooms3.png");
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
				level.addEntity(new ExplosiveBarrel(), new Vector2(x + xx + 0.5f, y + yy));
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

	public void spawnBarrel(int x, int y, float roomLootValue)
	{
		float scamChestChance = 0.02f;
		bool scam = random.NextSingle() < scamChestChance;

		Item[] items = scam ? [new Bomb().cook()] : Item.CreateRandom(random, DropRates.barrel, roomLootValue);
		Barrel barrel = new Barrel(items);
		level.addEntity(barrel, new Vector2(x + 0.5f, y));

		float barrelCoinsChance = 0.08f;
		if (random.NextSingle() < barrelCoinsChance)
		{
			int amount = MathHelper.RandomInt(1, 6, random);
			barrel.coins = amount;
		}

		objectFlags[x + y * level.width] = true;
	}

	public void spawnGroundItem(int x, int y, float roomLootValue)
	{
		Item[] items = Item.CreateRandom(random, DropRates.ground, roomLootValue);
		foreach (Item item in items)
		{
			ItemEntity itemEntity = new ItemEntity(item);
			level.addEntity(itemEntity, new Vector2(x + 0.5f, y + 0.5f));
		}

		objectFlags[x + y * level.width] = true;
	}

	void spawnItem(int x, int y, float roomLootValue)
	{
		float chestChance = 0.1f;
		float barrelChance = 0.4f;

		float f = random.NextSingle();
		if (f < chestChance)
			spawnChest(x, y, roomLootValue);
		else if (f < chestChance + barrelChance)
			spawnBarrel(x, y, roomLootValue);
		else
			spawnGroundItem(x, y, roomLootValue);
	}

	public void spawnNPC(int x, int y)
	{
		List<NPC> npcs = new List<NPC>();
		npcs.Add(new BuilderMerchant(random, level));
		npcs.Add(new TravellingMerchant(random, level));
		npcs.Add(new Logan(random, level));
		npcs.Add(new Blacksmith(random, level));
		npcs.Add(new Tinkerer(random, level));

		if (!GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_RAT_MET) || GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_RAT_QUESTLINE_COMPLETED) && !ratSpawned)
			npcs.Add(new RatNPC(random));

		NPC npc = npcs[random.Next() % npcs.Count];
		npc.direction = random.Next() % 2 * 2 - 1;
		level.addEntity(npc, new Vector2(x + 0.5f, y));

		if (npc is RatNPC)
			ratSpawned = true;
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
		bool createSpecialRoom(Doorway doorway)
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
				/*
				for (int y = room.y; y < room.y + room.height; y++)
				{
					for (int x = room.x; x < room.x + room.width; x++)
					{
						objectFlags[x + y * level.width] = true;
					}
				}
				*/
				return true;
			}

			return false;
		}

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
				bool specialRoom = random.NextSingle() < 0.2f;
				if (!(specialRoom && createSpecialRoom(emptyDoorway)))
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
				bool specialRoom = random.NextSingle() < 0.2f;
				if (!(specialRoom && createSpecialRoom(emptyDoorway)))
					fillDoorway(emptyDoorway, set);
			}
		}
	}

	void createDoors(bool spawnStartingRoom, bool spawnBossRoom, Room startingRoom, Room exitRoom, out Vector2i entrancePosition, out Vector2i exitPosition)
	{
		entrancePosition = Vector2i.Zero;
		exitPosition = Vector2i.Zero;

		if (spawnStartingRoom)
		{
			entrancePosition = startingRoom.getMarker(0x1);
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

		if (spawnBossRoom)
		{
			exitPosition = exitRoom.getMarker(0x67);
			level.exit = new Door(nextLevel);
			level.addEntity(level.exit, new Vector2(exitPosition.x + 0.5f, exitPosition.y));

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

	void spawnRoomObject(List<Room> roomList, float chance, bool allowMultiple, Action<Vector2i, Random, Room> spawnFunc)
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

		for (int i = 0; i < roomList.Count; i++)
		{
			Room room = roomList[i];
			if (random.NextSingle() < chance)
			{
				if (room.getFloorSpawn(level, random, objectFlags, out Vector2i tile))
				{
					spawnFunc(tile, random, room);
					objectFlags[tile.x + tile.y * level.width] = true;
					room.hasObject = true;
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
							spawnItem(pos.x, pos.y, getRoomLootValue(room));
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

	public void spawnEnemy(int x, int y, List<Mob> mobs)
	{
		TileType up = level.getTile(x, y + 1);
		TileType down = level.getTile(x, y - 1);
		TileType left = level.getTile(x - 1, y);
		TileType right = level.getTile(x + 1, y);

		Vector2 exitPosition = level.exit.position;

		float furthestDistance = 0;
		for (int i = 0; i < rooms.Count; i++)
		{
			Vector2 roomCenter = new Vector2(rooms[i].x + 0.5f * rooms[i].width, rooms[i].y + 0.5f * rooms[i].height);
			Vector2 toRoom = roomCenter - exitPosition;
			float distance = toRoom.length;
			furthestDistance = MathF.Max(furthestDistance, distance);
		}

		if (up == null && left == null && right == null)
		{
			Vector2 position = new Vector2(x, y);
			float distance = (position - exitPosition).length;
			float progress = MathHelper.Remap(distance, 0, furthestDistance, 1, 0);
			if (progress < 0.5f)
				progress *= 0.5f + random.NextSingle();
			else
				progress = 1 - (1 - progress) * (0.5f + random.NextSingle());
			//progress = progress * progress;
			int selection = MathHelper.Clamp((int)(progress * mobs.Count), 0, mobs.Count - 1);
			Mob enemy = mobs[selection];
			level.addEntity(enemy, new Vector2(x + 0.5f, y + 0.5f));
			objectFlags[x + y * level.width] = true;
		}
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
		Room room = new Room("res/level/hub/hub.png");
		level.resize(room.width, room.height);

		placeRoom(room, level, (int x, int y) => TileType.dirt);
		placeRoomBG(new Room("res/level/hub/hub1.png"), level, (int x, int y) => TileType.dirt);

		level.rooms = [room];
		level.infiniteEnergy = true;
		level.fogFalloff = 0.1f;
		//level.fogColor = new Vector3(0.1f);
		level.fogColor = new Vector3(0.0f);

		level.updateLightmap(0, 0, room.width, room.height);
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

		Simplex simplex = new Simplex(12345, 3);
		generateCaveBackground(level, simplex);

		level.updateLightmap(0, 0, def.width, def.height);
	}
}
