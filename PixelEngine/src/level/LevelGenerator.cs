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


struct DoorDef
{
	public Vector2i position;
	public Vector2i direction;
}

struct RoomDef
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

class RoomDefSet
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
					Debug.Assert(x > 0 && y > 0);
					uint top = rooms[x + (y - 1) * roomsInfo.width];
					uint left = rooms[x - 1 + y * roomsInfo.width];
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

public class LevelGenerator
{
	string seed;
	int floor;

	Random random;
	Simplex simplex;

	RoomDefSet defaultSet;
	RoomDefSet miscSet;
	RoomDefSet specialSet;


	public LevelGenerator()
	{
		defaultSet = new RoomDefSet("res/level/rooms.png");
		miscSet = new RoomDefSet("res/level/rooms_misc.png");
		specialSet = new RoomDefSet("res/level/rooms_special.png");
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

	void placeRoom(Room room, Level level, Simplex simplex)
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
				switch (color)
				{
					case 0xFF000000:
						level.setTile(x + xx, y + yy, null);
						break;
					case 0xFFFFFFFF:
						float type = simplex.sample2f(xx * 0.1f, yy * 0.1f);
						level.setTile(x + xx, y + yy, type > -0.2f ? TileType.wall : TileType.stone);
						break;
					case 0xFF0000FF:
						level.setTile(x + xx, y + yy, TileType.platform);
						break;
					case 0xFFFF00FF:
						level.setTile(x + xx, y + yy, TileType.dummy);
						//uint left = rooms[roomDef.x + xx - 1 + (roomDef.y + roomDef.height - yy - 1) * roomsInfo.width];
						//uint right = rooms[roomDef.x + xx + 1 + (roomDef.y + roomDef.height - yy - 1) * roomsInfo.width];
						uint left = roomDef.getTile(xx - 1, yy);
						uint right = roomDef.getTile(xx + 1, yy);
						Vector2 direction = (right == 0xFFFFFFFF) ? new Vector2(-1, 0) : new Vector2(1, 0);
						level.addEntity(new ArrowTrap(direction), new Vector2(x + xx, y + yy));
						break;
					case 0xFF00FF00:
						level.setTile(x + xx, y + yy, null);
						if (yy == room.set.height - 1 ||
							(roomDef.getTile(xx, yy - 1) != 0xFF00FF00 && roomDef.getTile(xx, yy - 1) != 0xFF00FFFF))
							level.addEntity(new Ladder(countLadderHeight(xx, yy, roomDef)), new Vector2(x + xx, y + yy));
						break;
					case 0xFFFF7F00:
						level.setTile(x + xx, y + yy, null);
						level.addEntity(new Spring(), new Vector2(x + xx + 0.5f, y + yy));
						break;
					case 0xFF00FFFF:
						level.setTile(x + xx, y + yy, TileType.platform);
						break;
					case 0xFFFF0000:
						level.setTile(x + xx, y + yy, null);
						//level.addEntity(new Spike(), new Vector2(x + xx, y + yy));
						break;
					default:
						level.setTile(x + xx, y + yy, null);
						break;
				}
			}
		}
	}

	class Room
	{
		public int roomDefID;
		public RoomDefSet set;
		public int x, y;
		public int width, height;
		public List<Doorway> doorways = new List<Doorway>();
		public bool isMainPath = false;


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
					if (y > 0 && level.getTile(x, y) == null && level.getTile(x, y + 1) == null)
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
	}

	class Doorway
	{
		public Room room;
		public DoorDef doorDef;
		public Doorway otherDoorway;
		public Vector2i position;
		public Vector2i direction;
	}

	bool fitRoom(Vector2i position, Vector2i size, List<Room> rooms, int width, int height)
	{
		if (position.x < 0 || position.x + size.x > width || position.y < 0 || position.y + size.y > height)
			return false;
		for (int i = 0; i < rooms.Count; i++)
		{
			if (position.x + size.x > rooms[i].x && position.x < rooms[i].x + rooms[i].width &&
				position.y + size.y > rooms[i].y && position.y < rooms[i].y + rooms[i].height)
				return false;
		}
		return true;
	}

	Room fillDoorway(Doorway lastDoorway, RoomDefSet set, List<Room> rooms, int width, int height)
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
			for (int j = 0; j < def.doorDefs.Count; j++)
			{
				if (def.doorDefs[j].direction == matchingDirection)
				{
					Vector2i roomPosition = new Vector2i(lastRoom.x, lastRoom.y) + lastDoorway.position + lastDoorway.direction - def.doorDefs[j].position;
					Vector2i roomSize = new Vector2i(def.width, def.height);
					if (fitRoom(roomPosition, roomSize, rooms, width, height))
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

	void spawnItem(int x, int y, Level level, bool[] objectFlags)
	{
		float chestChance = 0.3f;
		float barrelChance = 0.6f;

		float scamChestChance = 0.02f;
		bool scam = random.NextSingle() < scamChestChance;

		float f = random.NextSingle();
		if (f < chestChance)
		{
			TileType left = level.getTile(x - 1, y);
			TileType right = level.getTile(x + 1, y);
			Item[] items = scam ? [new Bomb().cook()] : Item.CreateRandom(random, DropRates.chest);
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
			Item[] items = scam ? [new Bomb().cook()] : Item.CreateRandom(random, DropRates.barrel);
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
			Item[] items = Item.CreateRandom(random, DropRates.ground);
			foreach (Item item in items)
			{
				ItemEntity itemEntity = new ItemEntity(item);
				level.addEntity(itemEntity, new Vector2(x + 0.5f, y + 0.5f));
			}
		}

		objectFlags[x + y * level.width] = true;
	}

	public unsafe void run(string seed, int floor, Level level, Level nextLevel, Level lastLevel)
	{
		this.seed = seed;
		this.floor = floor;

		random = new Random((int)Hash.hash(seed) + floor);
		simplex = new Simplex(Hash.hash(seed) + (uint)floor, 3);

		int width = MathHelper.RandomInt(24, 150, random);
		int height = (floor == 5 ? 4500 : 3200) / width;
		level.resize(width, height);

		bool darkLevel = floor == 2 || floor == 3;
		level.ambientLight = darkLevel ? new Vector3(0.001f) : new Vector3(1.0f);

		List<Room> rooms = new List<Room>();

		Room startingRoom = null;
		Room exitRoom = null;

		Room lastRoom = null;
		while (true)
		{
			if (lastRoom == null) // Starting room
			{
				int roomDefID = random.Next() % defaultSet.roomDefs.Count;
				RoomDef roomDef = defaultSet.roomDefs[roomDefID];
				while (roomDef.height > height || roomDef.width > width)
				{
					roomDefID = random.Next() % defaultSet.roomDefs.Count;
					roomDef = defaultSet.roomDefs[roomDefID];
				}

				int startingRoomX = random.Next() % Math.Max(width - roomDef.width, 1);
				int startingRoomY = random.Next() % Math.Max(height - roomDef.height, 1);
				Room room = new Room
				{
					x = startingRoomX,
					y = startingRoomY,
					width = roomDef.width,
					height = roomDef.height,
					roomDefID = roomDefID,
					set = defaultSet
				};
				room.isMainPath = true;

				for (int i = 0; i < roomDef.doorDefs.Count; i++)
				{
					room.doorways.Add(new Doorway { room = room, doorDef = roomDef.doorDefs[i], otherDoorway = null, position = roomDef.doorDefs[i].position, direction = roomDef.doorDefs[i].direction });
				}
				rooms.Add(room);

				startingRoom = room;

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

				Debug.Assert(emptyDoorways.Count > 0);

				bool found = false;
				for (int s = 0; s < emptyDoorways.Count; s++)
				{
					Doorway lastDoorway = emptyDoorways[s];
					Room room = fillDoorway(lastDoorway, defaultSet, rooms, width, height);

					if (room != null)
					{
						room.isMainPath = true;
						lastRoom = room;
						found = true;
						break;
					}
				}
				if (!found)
				{
					exitRoom = rooms[rooms.Count - 1];
					break;
				}
			}
		}

		// Spawn special rooms
		for (int k = 0; k < 2; k++)
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
				RoomDefSet set = random.NextSingle() < 0.8f ? defaultSet : miscSet;
				fillDoorway(emptyDoorway, set, rooms, width, height);
			}
		}

		for (int i = 0; i < rooms.Count; i++)
		{
			Room room = rooms[i];
			placeRoom(room, level, simplex);
		}

		bool[] objectFlags = new bool[width * height];
		Array.Fill(objectFlags, false);

		float[] lootModifier = new float[width * height];
		Array.Fill(lootModifier, 1.0f);

		Vector2i entrancePosition = Vector2i.Zero;
		Vector2i exitPosition = Vector2i.Zero;

		if (lastLevel != null)
		{
			if (startingRoom.getFloorSpawn(level, random, objectFlags, out entrancePosition))
			{
				level.entrance = new Door(lastLevel, lastLevel.exit);
				lastLevel.exit.otherDoor = level.entrance;
				level.addEntity(level.entrance, new Vector2(entrancePosition.x + 0.5f, entrancePosition.y));

				objectFlags[entrancePosition.x + entrancePosition.y * width] = true;
			}
			else
			{
				Debug.Assert(false);
			}
		}

		//if (nextLevel != null)
		{
			if (exitRoom.getFloorSpawn(level, random, objectFlags, out exitPosition))
			{
				level.exit = new Door(nextLevel);
				level.addEntity(level.exit, new Vector2(exitPosition.x + 0.5f, exitPosition.y));

				objectFlags[exitPosition.x + exitPosition.y * width] = true;
			}
			else
			{
				Debug.Assert(false);
			}
		}

		float builderChance = MathHelper.Remap(floor, 1, 3, 0.1f, 0.03f);
		float travellerChance = MathHelper.Remap(floor, 1, 3, 0.01f, 0.05f);
		float ratChance = 0.02f;
		float loganChance = MathHelper.Remap(floor, 1, 3, 0.01f, 0.05f);
		float blacksmithChance = MathHelper.Remap(floor, 1, 3, 0.08f, 0.02f);
		float tinkererChance = MathHelper.Remap(floor, 1, 3, 0.01f, 0.05f);
		float fountainChance = 1; // MathHelper.Remap(floor, 1, 3, 0.05f, 0.15f);
		float coinsChance = MathHelper.Remap(floor, 1, 3, 0.05f, 0.02f);

		List<Room> roomSpawnList = new List<Room>(rooms);
		MathHelper.ShuffleList(roomSpawnList, random);
		for (int i = 0; i < roomSpawnList.Count; i++)
		{
			Room room = roomSpawnList[i];
			bool isDeadEnd = room.countConnectedDoorways() == 1;
			if (isDeadEnd && !room.isMainPath)
			{
				for (int y = room.y; y < room.y + room.height; y++)
				{
					for (int x = room.x; x < room.x + room.width; x++)
					{
						lootModifier[x + y * width] = 3.0f;
					}
				}

				float f = random.NextSingle();
				float r = 0.0f;

				r += builderChance;
				if (f < r)
				{
					if (room.getFloorSpawn(level, random, objectFlags, out Vector2i npcPos))
					{
						BuilderMerchant npc = new BuilderMerchant(random);
						npc.direction = random.Next() % 2 * 2 - 1;
						level.addEntity(npc, new Vector2(npcPos.x + 0.5f, npcPos.y));
						builderChance = 0;
						f = 1;
						objectFlags[npcPos.x + npcPos.y * width] = true;
					}
				}

				r += travellerChance;
				if (f < r)
				{
					if (room.getFloorSpawn(level, random, objectFlags, out Vector2i npcPos))
					{
						TravellingMerchant npc = new TravellingMerchant(random);
						npc.direction = random.Next() % 2 * 2 - 1;
						level.addEntity(npc, new Vector2(npcPos.x + 0.5f, npcPos.y));
						travellerChance = 0;
						f = 1;
						objectFlags[npcPos.x + npcPos.y * width] = true;
					}
				}

				r += ratChance;
				if (f < r)
				{
					if (room.getFloorSpawn(level, random, objectFlags, out Vector2i npcPos))
					{
						RatNPC npc = new RatNPC();
						npc.direction = random.Next() % 2 * 2 - 1;
						level.addEntity(npc, new Vector2(npcPos.x + 0.5f, npcPos.y));
						ratChance = 0;
						f = 1;
						objectFlags[npcPos.x + npcPos.y * width] = true;
					}
				}

				r += loganChance;
				if (f < r)
				{
					if (room.getFloorSpawn(level, random, objectFlags, out Vector2i npcPos))
					{
						Logan npc = new Logan(random);
						npc.direction = random.Next() % 2 * 2 - 1;
						level.addEntity(npc, new Vector2(npcPos.x + 0.5f, npcPos.y));
						loganChance = 0;
						f = 1;
						objectFlags[npcPos.x + npcPos.y * width] = true;
					}
				}

				r += blacksmithChance;
				if (f < r)
				{
					if (room.getFloorSpawn(level, random, objectFlags, out Vector2i npcPos))
					{
						Blacksmith npc = new Blacksmith(random);
						npc.direction = random.Next() % 2 * 2 - 1;
						level.addEntity(npc, new Vector2(npcPos.x + 0.5f, npcPos.y));
						blacksmithChance = 0;
						f = 1;
						objectFlags[npcPos.x + npcPos.y * width] = true;
					}
				}

				r += tinkererChance;
				if (f < r)
				{
					if (room.getFloorSpawn(level, random, objectFlags, out Vector2i tile))
					{
						Tinkerer npc = new Tinkerer(random);
						npc.direction = random.Next() % 2 * 2 - 1;
						level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
						tinkererChance = 0;
						f = 1;
						objectFlags[tile.x + tile.y * width] = true;
					}
				}

				r += fountainChance;
				if (f < r)
				{
					if (room.getFloorSpawn(level, random, objectFlags, out Vector2i tile))
					{
						Fountain fountain = new Fountain(random);
						level.addEntity(fountain, new Vector2(tile.x + 0.5f, tile.y));
						fountainChance = 0;
						f = 1;
						objectFlags[tile.x + tile.y * width] = true;
					}
				}

				r += coinsChance;
				if (f < r)
				{
					if (room.getFloorSpawn(level, random, objectFlags, out Vector2i tile))
					{
						int amount = MathHelper.RandomInt(2, 10, random);
						level.addEntity(new Gem(amount), new Vector2(tile.x + 0.5f, tile.y + 0.5f));
						f = 1;
						objectFlags[tile.x + tile.y * width] = true;
					}
				}

				if (f != 1)
				{
					if (random.NextSingle() < 0.65f)
					{
						if (room.getFloorSpawn(level, random, objectFlags, out Vector2i tile))
						{
							spawnItem(tile.x, tile.y, level, objectFlags);
						}
					}

					// blacksmith
					// unlockable npcs
				}
			}
		}

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				if (objectFlags[x + y * width])
					continue;

				TileType tile = level.getTile(x, y);

				if (tile != null)
				{
					TileType up = level.getTile(x, y + 1);
					TileType down = level.getTile(x, y - 1);
					TileType left = level.getTile(x - 1, y);
					TileType right = level.getTile(x + 1, y);

					if (tile.isSolid && (left == null || right == null) && y != entrancePosition.y)
					{
						float arrowTrapChance = 0.001f;
						if (random.NextSingle() < arrowTrapChance)
						{
							int direction = right == null ? 1 : left == null ? -1 : random.Next() % 2 * 2 - 1;
							level.setTile(x, y, TileType.dummy);
							level.addEntity(new ArrowTrap(new Vector2(direction, 0)), new Vector2(x, y));
						}
					}
				}
				else if (tile == null)
				{
					TileType up = level.getTile(x, y + 1);
					TileType down = level.getTile(x, y - 1);
					TileType left = level.getTile(x - 1, y);
					TileType right = level.getTile(x + 1, y);

					if (down != null && !objectFlags[x + y * width])
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

					if (down != null && up == null && !objectFlags[x + y * width])
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

					if (down != null && up == null && !objectFlags[x + y * width])
					{
						TileType upLeft = level.getTile(x - 1, y + 1);
						TileType upRight = level.getTile(x + 1, y + 1);

						if (upLeft == null || upRight == null)
						{
							float spikeChance = 0.015f;
							if (random.NextSingle() < spikeChance)
							{
								level.addEntity(new Spike(), new Vector2(x, y));
								objectFlags[x + y * width] = true;
							}
						}
					}

					if (up != null && up.isSolid && !objectFlags[x + y * width])
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

					if (darkLevel && down == null && up == null && !objectFlags[x + y * width])
					{
						TileType downDown = level.getTile(x, y - 2);
						if (downDown != null)
						{
							float torchChance = 0.03f;
							if (random.NextSingle() < torchChance)
							{
								level.addEntity(new TorchEntity(), new Vector2(x + 0.5f, y + 0.5f));
								objectFlags[x + y * width] = true;
							}
						}
					}

					if (down != null && !objectFlags[x + y * width])
					{
						float itemChance = up != null && (left != null || right != null) ? 0.02f :
							up != null ? 0.005f :
							(left != null || right != null) ? 0.005f :
							0.002f;
						itemChance *= 0.25f;
						itemChance *= lootModifier[x + y * width];

						if (random.NextSingle() < itemChance)
						{
							spawnItem(x, y, level, objectFlags);
						}
					}

					if (down != null && !objectFlags[x + y * width])
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
								level.addEntity(new Barrel(), new Vector2(x + 0.5f, y));
							}
							objectFlags[x + y * width] = true;
						}
					}

					if ((left == null && right == null) && !objectFlags[x + y * width])
					{
						TileType downLeft = level.getTile(x - 1, y - 1);
						TileType downRight = level.getTile(x + 1, y - 1);

						float distanceToEntrance = (new Vector2i(x, y) - entrancePosition).length;

						if ((distanceToEntrance > 8 || y < entrancePosition.y) && (downLeft != null || downRight != null))
						{
							float enemyChance = 0.1f;
							if (random.NextSingle() < enemyChance)
							{
								bool flyingEnemy = random.NextSingle() < 0.25f;
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
									if (down != null && up == null)
									{
										float enemyType = random.NextSingle();

										Mob enemy;

										//if (enemyType > 0.9f)
										//	enemy = new Bob();
										//else 
										if (enemyType > 0.95f)
											enemy = new Gandalf();
										else if (enemyType > 0.666f)
											enemy = new Snake();
										else if (enemyType > 0.333f)
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
							}
						}
					}
				}
			}
		}

		level.updateLightmap(0, 0, width, height);
	}

	public void generateLobby(Level level)
	{
		RoomDef def = specialSet.roomDefs[0];
		level.resize(def.width, def.height);

		simplex = new Simplex();

		Room room = new Room
		{
			x = 0,
			y = 0,
			width = def.width,
			height = def.height,
			roomDefID = def.id,
			set = specialSet
		};

		placeRoom(room, level, simplex);

		level.updateLightmap(0, 0, def.width, def.height);
	}

	public void generateTutorial(Level level)
	{
		RoomDef def = specialSet.roomDefs[1];
		level.resize(def.width, def.height);

		simplex = new Simplex();

		Room room = new Room
		{
			x = 0,
			y = 0,
			width = def.width,
			height = def.height,
			roomDefID = def.id,
			set = specialSet
		};

		placeRoom(room, level, simplex);

		level.updateLightmap(0, 0, def.width, def.height);
	}
}
