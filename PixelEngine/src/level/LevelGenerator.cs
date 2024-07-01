using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


public class LevelGenerator
{
	uint seed;
	int floor;

	Random random;

	byte[] horiz;
	TextureInfo horizInfo;

	byte[] drop;
	TextureInfo dropInfo;

	byte[] land;
	TextureInfo landInfo;

	byte[] dropland;
	TextureInfo droplandInfo;

	byte[] other;
	TextureInfo otherInfo;


	public LevelGenerator(uint seed, int floor)
	{
		this.seed = seed;
		this.floor = floor;

		random = new Random((int)seed + floor);

		horiz = Resource.ReadImage("res/level/rooms_horiz.png", out horizInfo);
		drop = Resource.ReadImage("res/level/rooms_drop.png", out dropInfo);
		land = Resource.ReadImage("res/level/rooms_land.png", out landInfo);
		dropland = Resource.ReadImage("res/level/rooms_dropland.png", out droplandInfo);
		other = Resource.ReadImage("res/level/rooms_other.png", out otherInfo);
	}

	unsafe int countLadderHeight(int x, int y, uint* input, int inputWidth, int inputHeight)
	{
		int result = 0;
		while (true)
		{
			uint color = input[x + (inputHeight - y - result - 1) * inputWidth];
			if (color == 0xFF00FF00 || color == 0xFF00FFFF)
				result++;
			else
				break;
		}
		return result;
	}

	unsafe void generateRoom(int x, int y, int width, int height, byte[] wfc, TextureInfo wfcInfo, Level level)
	{
		int numPossibleRooms = wfcInfo.width / width;
		int roomIndex = random.Next() % numPossibleRooms;

		fixed (byte* wfcPtr = wfc)
		{
			uint* input = (uint*)wfcPtr;

			for (int yy = 0; yy < height; yy++)
			{
				for (int xx = 0; xx < width; xx++)
				{
					uint color = input[roomIndex * width + xx + (wfcInfo.height - yy - 1) * wfcInfo.width];
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
							uint left = input[roomIndex * width + xx - 1 + (wfcInfo.height - yy - 1) * wfcInfo.width];
							uint right = input[roomIndex * width + xx + 1 + (wfcInfo.height - yy - 1) * wfcInfo.width];
							Vector2 direction = (right == 0xFFFFFFFF) ? new Vector2(-1, 0) : new Vector2(1, 0);
							level.addEntity(new ArrowTrap(direction), new Vector2(x + xx, y + yy));
							break;
						case 0xFF00FF00:
							level.setTile(x + xx, y + yy, 0);
							if (horizInfo.height - yy - 1 == 0 ||
								(input[roomIndex * width + xx + (wfcInfo.height - yy + 1 - 1) * wfcInfo.width] != 0xFF00FF00 && input[roomIndex * width + xx + (wfcInfo.height - yy + 1 - 1) * wfcInfo.width] != 0xFF00FFFF))
								level.addEntity(new Ladder(countLadderHeight(roomIndex * width + xx, yy, input, wfcInfo.width, wfcInfo.height)), new Vector2(x + xx, y + yy));
							break;
						case 0xFF00FFFF:
							level.setTile(x + xx, y + yy, 3);
							break;
						case 0xFFFF0000:
							level.setTile(x + xx, y + yy, 0);
							level.addEntity(new Spike(), new Vector2(x + xx, y + yy));
							break;
						default:
							level.setTile(x + xx, y + yy, 0);
							break;
					}
				}
			}
		}

		return;


		WFCOptions options = new WFCOptions();
		options.outWidth = width;
		options.outHeight = height;
		options.symmetry = 2;
		options.patternSize = 3;
		options.ground = true;

		uint[] output = new uint[width * height];

		while (true)
		{
			if (WFC.Run(options, (uint)random.Next(), horiz, horizInfo.width, horizInfo.height, output))
			{
				for (int yy = 0; yy < height / 2; yy++)
				{
					for (int xx = 0; xx < width; xx++)
					{
						uint tmp = output[xx + yy * width];
						output[xx + yy * width] = output[xx + (height - yy - 1) * width];
						output[xx + (height - yy - 1) * width] = tmp;
					}
				}

				for (int yy = 0; yy < height; yy++)
				{
					for (int xx = 0; xx < width; xx++)
					{
						uint color = output[xx + yy * width];
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
								level.setTile(x + xx, y + yy, 4);
								break;
							default:
								level.setTile(x + xx, y + yy, 4);
								break;
						}
					}
				}

				break;
			}
		}
	}

	struct RoomInfo
	{
		public Vector2i position;
		public bool land;
		public bool drop;
	}

	public unsafe void run(Level level, Level nextLevel, Level lastLevel)
	{
		int roomWidth = 10;
		int roomHeight = 8;
		int numRoomsX = 4;
		int numRoomsY = 4;
		int width = numRoomsX * roomWidth;
		int height = numRoomsY * roomHeight;
		level.resize(width, height);

		List<RoomInfo> rooms = new List<RoomInfo>();

		int roomX = random.Next() % numRoomsX;
		int roomY = 3;
		int direction = -1;

		while (true)
		{
			RoomInfo room = new RoomInfo();
			room.position = new Vector2i(roomX, roomY);

			if (direction == 2)
				room.land = true;

			int lastDirection = direction;
			direction = random.Next() % 3;

			if (lastDirection == 0 && direction == 1)
				direction = 0;
			else if (lastDirection == 1 && direction == 0)
				direction = 1;

			if (direction == 0 && roomX == 0)
				direction = 2;
			else if (direction == 1 && roomX == 3)
				direction = 2;

			if (direction == 2 && roomY > 0)
				room.drop = true;

			rooms.Add(room);

			if (direction == 2 && roomY == 0)
				break;

			switch (direction)
			{
				case 0:
					roomX--;
					break;
				case 1:
					roomX++;
					break;
				case 2:
					roomY--;
					break;
			}
		}

		bool[] grid = new bool[numRoomsX * numRoomsY];

		for (int i = 0; i < rooms.Count; i++)
		{
			RoomInfo room = rooms[i];
			if (room.drop && room.land)
				generateRoom(room.position.x * roomWidth, room.position.y * roomHeight, roomWidth, roomHeight, dropland, droplandInfo, level);
			else if (room.drop)
				generateRoom(room.position.x * roomWidth, room.position.y * roomHeight, roomWidth, roomHeight, drop, dropInfo, level);
			else if (room.land)
				generateRoom(room.position.x * roomWidth, room.position.y * roomHeight, roomWidth, roomHeight, land, landInfo, level);
			else
				generateRoom(room.position.x * roomWidth, room.position.y * roomHeight, roomWidth, roomHeight, horiz, horizInfo, level);

			grid[room.position.x + room.position.y * numRoomsX] = true;
		}

		for (int y = 0; y < numRoomsY; y++)
		{
			for (int x = 0; x < numRoomsX; x++)
			{
				if (!grid[x + y * numRoomsX])
				{
					generateRoom(x * roomWidth, y * roomHeight, roomWidth, roomHeight, other, otherInfo, level);
				}
			}
		}

		if (lastLevel != null)
		{
			RoomInfo startingRoom = rooms[0];
			for (int y = startingRoom.position.y * roomHeight + 1; y < (startingRoom.position.y + 1) * roomHeight; y++)
			{
				int x = startingRoom.position.x * roomWidth + roomWidth / 2;
				if (level.getTile(x, y) == 0)
				{
					Vector2 entrancePosition = new Vector2(x + 0.5f, y);
					level.entrance = new Door(lastLevel, lastLevel.exit);
					lastLevel.exit.otherDoor = level.entrance;
					level.addEntity(level.entrance, entrancePosition);

					if (level.getTile(x, y - 1) == 0)
						level.setTile(x, y - 1, 2);

					break;
				}
			}
		}

		if (nextLevel != null)
		{
			RoomInfo exitRoom = rooms[rooms.Count - 1];
			for (int y = exitRoom.position.y * roomHeight; y < (exitRoom.position.y + 1) * roomHeight; y++)
			{
				int x = exitRoom.position.x * roomWidth + roomWidth / 2;
				if (level.getTile(x, y) == 0)
				{
					Vector2 exitPosition = new Vector2(x + 0.5f, y);
					level.exit = new Door(nextLevel);
					level.addEntity(level.exit, exitPosition);
					break;
				}
			}
		}

		for (int y = 0; y < height; y++)
		{
			level.setTile(0, y, 2);
			level.setTile(width - 1, y, 2);
		}
		for (int x = 0; x < width; x++)
		{
			level.setTile(x, 0, 2);
			level.setTile(x, height - 1, 2);
		}

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				TileType tile = TileType.Get(level.getTile(x, y));

				if (tile == null)
				{
					TileType up = TileType.Get(level.getTile(x, y + 1));
					TileType down = TileType.Get(level.getTile(x, y - 1));
					TileType left = TileType.Get(level.getTile(x - 1, y));
					TileType right = TileType.Get(level.getTile(x + 1, y));

					if (down != null)
					{
						float gemChance = up != null ? 0.04f : 0.01f;
						if (random.NextSingle() < gemChance)
						{
							level.addEntity(new Gem(1), new Vector2(x + 0.5f, y + 0.5f));
						}
					}

					if (down != null && up == null)
					{
						TileType upUp = TileType.Get(level.getTile(x, y + 2));
						if (upUp == null)
						{
							float springChance = 0.02f;
							if (random.NextSingle() < springChance)
							{
								level.addEntity(new Spring(), new Vector2(x + 0.5f, y));
							}
						}
					}

					if (down == null && up == null)
					{
						TileType downDown = TileType.Get(level.getTile(x, y - 2));
						if (downDown != null)
						{
							float torchChance = 0.03f;
							if (random.NextSingle() < torchChance)
							{
								level.addEntity(new Torch(), new Vector2(x + 0.5f, y + 0.5f));
							}
						}
					}

					if (down != null)
					{
						float skullChance = 0.01f;
						if (random.NextSingle() < skullChance)
						{
							level.addEntity(new ItemEntity(new Skull()), new Vector2(x + 0.5f, y + 0.5f));
						}

						float swordChance = 0.005f;
						if (random.NextSingle() < swordChance)
						{
							level.addEntity(new ItemEntity(new Sword()), new Vector2(x + 0.5f, y + 0.5f));
						}
					}

					if (down != null && up == null && (left == null && right == null))
					{
						TileType downLeft = TileType.Get(level.getTile(x - 1, y - 1));
						TileType downRight = TileType.Get(level.getTile(x + 1, y - 1));

						if (downLeft != null || downRight != null)
						{
							float enemyChance = 0.1f;
							if (random.NextSingle() < enemyChance)
							{
								float enemyType = random.NextSingle();

								Mob enemy;

								if (enemyType > 0.5f)
									enemy = new Snake();
								else
									enemy = new Spider();

								level.addEntity(enemy, new Vector2(x + 0.5f, y));
							}
						}
					}
				}
			}
		}
	}
}
