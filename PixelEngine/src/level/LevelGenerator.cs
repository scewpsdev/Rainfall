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
	uint seed;
	int floor;

	Random random;

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

	void placeRoom(Room room, Level level)
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
						level.setTile(x + xx, y + yy, 0);
						break;
					case 0xFFFFFFFF:
						level.setTile(x + xx, y + yy, 2);
						break;
					case 0xFF0000FF:
						level.setTile(x + xx, y + yy, 3);
						break;
					case 0xFFFF00FF:
						level.setTile(x + xx, y + yy, 1);
						//uint left = rooms[roomDef.x + xx - 1 + (roomDef.y + roomDef.height - yy - 1) * roomsInfo.width];
						//uint right = rooms[roomDef.x + xx + 1 + (roomDef.y + roomDef.height - yy - 1) * roomsInfo.width];
						uint left = roomDef.getTile(xx - 1, yy);
						uint right = roomDef.getTile(xx + 1, yy);
						Vector2 direction = (right == 0xFFFFFFFF) ? new Vector2(-1, 0) : new Vector2(1, 0);
						level.addEntity(new ArrowTrap(direction), new Vector2(x + xx, y + yy));
						break;
					case 0xFF00FF00:
						level.setTile(x + xx, y + yy, 0);
						if (yy == room.set.height - 1 ||
							(roomDef.getTile(xx, yy - 1) != 0xFF00FF00 && roomDef.getTile(xx, yy - 1) != 0xFF00FFFF))
							level.addEntity(new Ladder(countLadderHeight(xx, yy, roomDef)), new Vector2(x + xx, y + yy));
						break;
					case 0xFFFF7F00:
						level.setTile(x + xx, y + yy, 0);
						level.addEntity(new Spring(), new Vector2(x + xx + 0.5f, y + yy));
						break;
					case 0xFF00FFFF:
						level.setTile(x + xx, y + yy, 3);
						break;
					case 0xFFFF0000:
						level.setTile(x + xx, y + yy, 0);
						//level.addEntity(new Spike(), new Vector2(x + xx, y + yy));
						break;
					default:
						level.setTile(x + xx, y + yy, 0);
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

	public unsafe void run(uint seed, int floor, Level level, Level nextLevel, Level lastLevel)
	{
		this.seed = seed;
		this.floor = floor;

		random = new Random((int)seed + floor);

		int width = 50;
		int height = 40;
		level.resize(width, height);

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
				int startingRoomX = random.Next() % (width - roomDef.width);
				int startingRoomY = random.Next() % (height - roomDef.height);
				Room room = new Room
				{
					x = startingRoomX,
					y = startingRoomY,
					width = roomDef.width,
					height = roomDef.height,
					roomDefID = roomDefID,
					set = defaultSet
				};
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
			placeRoom(room, level);
		}

		bool[] objectFlags = new bool[width * height];
		Array.Fill(objectFlags, false);

		float[] lootModifier = new float[width * height];
		Array.Fill(lootModifier, 1.0f);

		if (lastLevel != null)
		{
			for (int y = startingRoom.y + 1; y < startingRoom.y + startingRoom.height; y++)
			{
				int x = startingRoom.x + startingRoom.width / 2;
				if (y > 0 && level.getTile(x, y) == 0)
				{
					Vector2 entrancePosition = new Vector2(x + 0.5f, y);
					level.entrance = new Door(lastLevel, lastLevel.exit);
					lastLevel.exit.otherDoor = level.entrance;
					level.addEntity(level.entrance, entrancePosition);

					if (level.getTile(x, y - 1) == 0)
						level.setTile(x, y - 1, 3);

					objectFlags[x + y * width] = true;

					break;
				}
			}
		}

		//if (nextLevel != null)
		{
			for (int y = exitRoom.y; y < exitRoom.y + exitRoom.height; y++)
			{
				int x = exitRoom.x + exitRoom.width / 2;
				if (level.getTile(x, y) == 0)
				{
					Vector2 exitPosition = new Vector2(x + 0.5f, y);
					level.exit = new Door(nextLevel);
					level.addEntity(level.exit, exitPosition);
					objectFlags[x + y * width] = true;
					break;
				}
			}
		}

		for (int i = 0; i < rooms.Count; i++)
		{
			Room room = rooms[i];
			int connectedDoorways = 0;
			for (int j = 0; j < room.doorways.Count; j++)
			{
				if (room.doorways[j].otherDoorway != null)
					connectedDoorways++;
			}
			Debug.Assert(connectedDoorways > 0);
			bool isDeadEnd = connectedDoorways == 1;
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

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				if (objectFlags[x + y * width])
					continue;

				TileType tile = TileType.Get(level.getTile(x, y));

				if (tile != null)
				{
					TileType up = TileType.Get(level.getTile(x, y + 1));
					TileType down = TileType.Get(level.getTile(x, y - 1));
					TileType left = TileType.Get(level.getTile(x - 1, y));
					TileType right = TileType.Get(level.getTile(x + 1, y));

					if (tile.isSolid && (left == null || right == null))
					{
						float arrowTrapChance = 0.001f;
						if (random.NextSingle() < arrowTrapChance)
						{
							int direction = right == null ? 1 : left == null ? -1 : random.Next() % 2 * 2 - 1;
							level.setTile(x, y, 1);
							level.addEntity(new ArrowTrap(new Vector2(direction, 0)), new Vector2(x, y));
						}
					}
				}
				else if (tile == null)
				{
					TileType up = TileType.Get(level.getTile(x, y + 1));
					TileType down = TileType.Get(level.getTile(x, y - 1));
					TileType left = TileType.Get(level.getTile(x - 1, y));
					TileType right = TileType.Get(level.getTile(x + 1, y));

					if (down != null && !objectFlags[x + y * width])
					{
						float gemChance = up != null ? 0.04f : 0.01f;
						if (random.NextSingle() < gemChance)
						{
							level.addEntity(new Gem(1), new Vector2(x + 0.5f, y + 0.5f));
							objectFlags[x + y * width] = true;
						}
					}

					if (down != null && up == null && !objectFlags[x + y * width])
					{
						TileType upUp = TileType.Get(level.getTile(x, y + 2));
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
						TileType upLeft = TileType.Get(level.getTile(x - 1, y + 1));
						TileType upRight = TileType.Get(level.getTile(x + 1, y + 1));

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
						TileType downDown = TileType.Get(level.getTile(x, y - 2));
						TileType downLeft = TileType.Get(level.getTile(x - 1, y - 1));
						TileType downRight = TileType.Get(level.getTile(x + 1, y - 1));
						if (down == null && downDown == null && (left != null && right != null || left == null && downLeft == null || right == null && downRight == null))
						{
							float spikeTrapChance = 0.01f;
							if (random.NextSingle() < spikeTrapChance)
							{
								level.addEntity(new SpikeTrap(), new Vector2(x + 0.5f, y + 0.5f));
								objectFlags[x + y * width] = true;
							}
						}
					}

					if (down == null && up == null && !objectFlags[x + y * width])
					{
						TileType downDown = TileType.Get(level.getTile(x, y - 2));
						if (downDown != null)
						{
							float torchChance = 0.03f;
							if (random.NextSingle() < torchChance)
							{
								level.addEntity(new Torch(), new Vector2(x + 0.5f, y + 0.5f));
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
						itemChance *= 3;
						itemChance *= lootModifier[x + y * width];

						if (random.NextSingle() < itemChance)
						{
							float scamChestChance = 0.02f;
							if (random.NextSingle() < scamChestChance)
							{
								level.addEntity(new Chest(null, left != null && right == null), new Vector2(x + 0.5f, y));
								objectFlags[x + y * width] = true;
							}
							else
							{
								Item item = Item.CreateRandom(random);
								level.addEntity(new Chest([item], left != null && right == null), new Vector2(x + 0.5f, y));
								objectFlags[x + y * width] = true;
							}
						}
					}

					if (down != null && up == null && (left == null && right == null) && !objectFlags[x + y * width])
					{
						TileType downLeft = TileType.Get(level.getTile(x - 1, y - 1));
						TileType downRight = TileType.Get(level.getTile(x + 1, y - 1));

						float distanceToEntrance = (new Vector2(x, y) + 0.5f - level.entrance.position).length;

						if ((distanceToEntrance > 8 || y < (int)level.entrance.position.y) && (downLeft != null || downRight != null))
						{
							float enemyChance = 0.1f;
							if (random.NextSingle() < enemyChance)
							{
								float enemyType = random.NextSingle();

								Mob enemy;

								if (enemyType > 0.666f)
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

	public void generateLobby(Level level)
	{
		RoomDef def = specialSet.roomDefs[0];
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

		placeRoom(room, level);
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

		placeRoom(room, level);
	}
}
