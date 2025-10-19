using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;



public class DoorwayInstance
{
	public Vector2i position;
	public Vector2i direction;
	public int height;
	public Vector3 worldPosition;
	public Quaternion worldRotation;
	public RoomInstance room;

	public List<DoorwayInstance> connectedDoorways = new List<DoorwayInstance>();
}

public class RoomInstance
{
	public Vector2i pos;
	public Vector2i size;
	public int height;

	public List<DoorwayInstance> doorways = new List<DoorwayInstance>();

	public RoomInterior interior;


	public Vector3 worldPosition
	{
		get => new Vector3(pos.x, height, pos.y) * LevelGenerator.TILE_SIZE;
	}
}

public struct LevelMesh
{
	public Model model;
	public Matrix transform;

	public LevelMesh(Model model, Matrix transform)
	{
		this.model = model;
		this.transform = transform;
	}
}


enum VoxelType_ : uint
{
	Null = 0,

	Air,
	StoneBricks,
	DirtFloor,
	Cobble,
	StairsNorth,
	StairsSouth,
	StairsWest,
	StairsEast,
	StairsShallowNorth,
	StairsShallowSouth,
	StairsShallowWest,
	StairsShallowEast,
}

struct EntitySpawn
{
	public Type entityType;
	public Quaternion rotation;

	public EntitySpawn(Type entityType, Quaternion rotation)
	{
		this.entityType = entityType;
		this.rotation = rotation;
	}
}

internal class LevelGenerator
{
	public const float TILE_SIZE = 3.0f;

	public const uint CID_WALL = 0xFF000000;
	public const uint CID_ROOM = 0xFFFF0000;
	public const uint CID_CORRIDOR = 0xFFFFFFFF;

	const uint FLAG_ROOM_WALL = 1 << 1;


	int seed;
	int width, height, layers = 10;

	Random random;

	uint[] tiles;
	int[] heightmap;
	int[] ceilingHeight;
	uint[] flags;

	uint[] voxels;

	List<EntitySpawn> entities = new List<EntitySpawn>();

	List<RoomInstance> rooms = new List<RoomInstance>();
	Level level;
	int numLoopsRemaining = 3;

	RoomInstance startingRoom = null;

	Model floor, wall, ceiling;
	Model stairs, stairsShallow;
	ModelBatch wallBatch, floorBatch, stairsBatch;


	public LevelGenerator(int width, int height, int seed)
	{
		this.width = width;
		this.height = height;
		this.seed = seed;

		random = new Random(seed);

		floor = Resource.GetModel("res/models/tiles/floor.gltf");
		ceiling = Resource.GetModel("res/models/tiles/ceiling.gltf");
		wall = Resource.GetModel("res/models/tiles/wall.gltf");
		stairs = Resource.GetModel("res/models/tiles/stairs/stairs.gltf");
		stairsShallow = Resource.GetModel("res/models/tiles/stairs/stairs_shallow.gltf");

		wallBatch = new ModelBatch();
		floorBatch = new ModelBatch();
		stairsBatch = new ModelBatch();
	}

	void generateTilemap()
	{
		WFCOptions options = new WFCOptions();
		options.periodicInput = true;
		options.periodicOutput = false;
		options.outWidth = width;
		options.outHeight = height;
		options.symmetry = 8;
		options.ground = false;
		options.patternSize = 4;

		byte[] input = Resource.ReadImage("res/level/wfc.png", out TextureInfo info);
		long before = Time.timestamp;
		bool result = WFC.Run(options, (uint)seed, input, info.width, info.height, tiles);
		long after = Time.timestamp;
		long delta = after - before;
		Console.WriteLine("Generated level in " + delta / 1e6f + " ms");
	}

	void setTile(int x, int z, uint tile)
	{
		if (x >= 0 && x < width && z >= 0 && z < height)
			tiles[x + z * width] = tile;
	}

	uint getTile(int x, int z)
	{
		if (x >= 0 && x < width && z >= 0 && z < height)
			return tiles[x + z * width];
		return 0;
	}

	void setHeight(int x, int z, int h)
	{
		if (x >= 0 && x < width && z >= 0 && z < height)
			heightmap[x + z * width] = h;
	}

	void setHeight(Vector2i position, int height)
	{
		setHeight(position.x, position.y, height);
	}

	int getHeight(int x, int z)
	{
		if (x >= 0 && x < width && z >= 0 && z < height)
			return heightmap[x + z * width];
		return int.MaxValue;
	}

	void setCeilingHeight(int x, int z, int ch)
	{
		if (x >= 0 && x < width && z >= 0 && z < height)
			ceilingHeight[x + z * width] = ch;
	}

	int getCeilingHeight(int x, int z)
	{
		if (x >= 0 && x < width && z >= 0 && z < height)
			return ceilingHeight[x + z * width];
		return int.MaxValue;
	}

	void setFlag(int x, int z, uint flag, bool set)
	{
		if (x >= 0 && x < width && z >= 0 && z < height)
		{
			if (set)
				flags[x + z * width] |= flag;
			else
				flags[x + z * width] = flags[x + z * width] | flag ^ flag;
		}
	}

	bool hasFlag(int x, int z, uint flag)
	{
		if (x >= 0 && x < width && z >= 0 && z < height)
			return (flags[x + z * width] & flag) != 0;
		return false;
	}

	void setVoxel(int x, int y, int z, VoxelType type)
	{
		if (x >= 0 && x < width && z >= 0 && z < height && y >= 0 && y < layers)
			voxels[x + z * width + y * width * height] = type != null ? type.id : 0;
	}

	VoxelType getVoxel(int x, int y, int z)
	{
		if (x >= 0 && x < width && z >= 0 && z < height && y >= 0 && y < layers)
			return VoxelType.Get(voxels[x + z * width + y * width * height]);
		return null;
	}

	bool isSolid(VoxelType voxel)
	{
		if (voxel != null)
			return !voxel.isTransparent;
		return true;
	}

	void checkRoomSize(int x, int z, out Vector2i pos, out Vector2i size)
	{
		int x0 = x, x1 = x, z0 = z, z1 = z;
		while (getTile(x0 - 1, z) == CID_ROOM)
			x0--;
		while (getTile(x1 + 1, z) == CID_ROOM)
			x1++;
		while (getTile(x, z0 - 1) == CID_ROOM)
			z0--;
		while (getTile(x, z1 + 1) == CID_ROOM)
			z1++;
		pos = new Vector2i(x0, z0);
		size = new Vector2i(x1 - x0 + 1, z1 - z0 + 1);
	}

	void fillMask(Vector2i pos, Vector2i size, bool[] mask, int width, int height)
	{
		for (int z = pos.y; z < pos.y + size.y; z++)
		{
			for (int x = pos.x; x < pos.x + size.x; x++)
			{
				mask[x + z * width] = true;
			}
		}
	}

	void collectRooms()
	{
		bool[] mask = new bool[width * height];
		for (int z = 0; z < height; z++)
		{
			for (int x = 0; x < width; x++)
			{
				if (!mask[x + z * width])
				{
					uint color = tiles[x + z * width];
					if (color == CID_ROOM) // room
					{
						checkRoomSize(x, z, out Vector2i pos, out Vector2i size);
						fillMask(pos, size, mask, width, height);
						rooms.Add(new RoomInstance() { pos = pos, size = size });
					}
					mask[x + z * width] = true;
				}
			}
		}
	}

	bool isIsolated(RoomInstance room)
	{
		for (int x = room.pos.x - 1; x < room.pos.x + room.size.x + 1; x++)
		{
			if (getTile(x, room.pos.y - 1) != 0)
				return false;
			if (getTile(x, room.pos.y + room.size.y) != 0)
				return false;
		}
		for (int z = room.pos.y; z < room.pos.y + room.size.y; z++)
		{
			if (getTile(room.pos.x - 1, z) != 0)
				return false;
			if (getTile(room.pos.x + room.size.x, z) != 0)
				return false;
		}
		return true;
	}

	void removeRoom(RoomInstance room)
	{
		for (int z = room.pos.y; z < room.pos.y + room.size.y; z++)
		{
			for (int x = room.pos.x; x < room.pos.x + room.size.x; x++)
			{
				setTile(x, z, 0);
			}
		}
		rooms.Remove(room);
	}

	void filterRooms()
	{
		for (int i = 0; i < rooms.Count; i++)
		{
			RoomInstance room = rooms[i];
			if (isIsolated(room))
				removeRoom(room);
			//else if (room.size.x < 2 || room.size.y < 2)
			//	removeRoom(room);
		}

		foreach (RoomInstance room in rooms)
		{
			for (int z = room.pos.y - 1; z < room.pos.y + room.size.y + 1; z++)
			{
				for (int x = room.pos.x - 1; x < room.pos.x + room.size.x + 1; x++)
				{
					if (x < room.pos.x || x >= room.pos.x + room.size.x || z < room.pos.y || z >= room.pos.y + room.size.y)
						setFlag(x, z, FLAG_ROOM_WALL, true);
				}
			}
		}

		startingRoom = rooms[rooms.Count / 2];

		MathHelper.ShuffleList(rooms, random);
	}

	List<RoomInstance> getConnectedRooms(RoomInstance room)
	{
		List<RoomInstance> connectedRooms = new List<RoomInstance>();
		foreach (RoomInstance r in rooms)
		{
			bool[] walkable = new bool[tiles.Length];
			for (int i = 0; i < tiles.Length; i++)
				walkable[i] = tiles[i] != CID_WALL;
			List<Vector2i> path = AStar.Run(r.pos + r.size / 2, room.pos + room.size / 2, width, height, walkable);
			if (path != null)
				connectedRooms.Add(r);
		}
		return connectedRooms;
	}

	int manhattanDistance(Vector2i a, Vector2i b)
	{
		int dx = Math.Abs(a.x - b.x);
		int dy = Math.Abs(a.y - b.y);
		return dx + dy;
	}

	void sortRoomsByDistanceToPos(List<RoomInstance> rooms, Vector2i pos)
	{
		rooms.Sort((RoomInstance room1, RoomInstance room2) =>
		{
			int distance1 = manhattanDistance(room1.pos + room1.size / 2, pos);
			int distance2 = manhattanDistance(room2.pos + room2.size / 2, pos);
			if (distance1 < distance2)
				return -1;
			else if (distance1 > distance2)
				return 1;
			else
				return 0;
		});
	}

	void buildCorridor(RoomInstance room1, RoomInstance room2)
	{
		bool[] walkable = new bool[tiles.Length];
		Array.Fill(walkable, true);
		int[] costs = new int[tiles.Length];
		for (int i = 0; i < tiles.Length; i++)
		{
			int x = i % width;
			int y = i / width;
			int cost = 1;
			// Make new connections use existing corridors if possible
			if (tiles[i] == CID_WALL)
			{
				// Make corridors avoid going parallel next to rooms, so that we can place doors correctly
				uint l = getTile(x - 1, y);
				uint r = getTile(x + 1, y);
				uint b = getTile(x, y - 1);
				uint t = getTile(x, y + 1);
				uint lb = getTile(x - 1, y - 1);
				uint rb = getTile(x + 1, y - 1);
				uint lt = getTile(x - 1, y + 1);
				uint rt = getTile(x + 1, y + 1);
				if (l == CID_ROOM || r == CID_ROOM || b == CID_ROOM || t == CID_ROOM ||
					lb == CID_ROOM || rb == CID_ROOM || lt == CID_ROOM || rt == CID_ROOM)
					cost = 3; // maybe change back to 3?
				else
					cost = 2; // maybe change back to 2?
			}
			costs[i] = cost;
		}
		List<Vector2i> path = AStar.Run(room1.pos + room1.size / 2, room2.pos + room2.size / 2, width, height, walkable, costs);
		for (int i = 0; i < path.Count; i++)
		{
			Vector2i pos = path[i];
			if (getTile(pos.x, pos.y) == CID_WALL)
				setTile(pos.x, pos.y, CID_CORRIDOR);
		}
	}

	void connectRoomsIfNot(RoomInstance room1, RoomInstance room2)
	{
		List<RoomInstance> connectedRooms1 = getConnectedRooms(room1);

		if (connectedRooms1.Contains(room2))
			return;

		// Rooms do not connect yet

		List<RoomInstance> connectedRooms2 = getConnectedRooms(room2);

		sortRoomsByDistanceToPos(connectedRooms1, room2.pos + room2.size / 2);
		sortRoomsByDistanceToPos(connectedRooms2, room1.pos + room1.size / 2);

		RoomInstance closest1 = connectedRooms1[0];
		RoomInstance closest2 = connectedRooms2[0];

		buildCorridor(closest1, closest2);

		if (numLoopsRemaining > 0)
		{
			RoomInstance additional1 = connectedRooms1[connectedRooms1.Count / 2];
			RoomInstance additional2 = connectedRooms2[connectedRooms2.Count / 2];

			buildCorridor(additional1, additional2);

			numLoopsRemaining--;
		}
	}

	void connectRooms()
	{
		foreach (RoomInstance room in rooms)
		{
			if (room != startingRoom)
				connectRoomsIfNot(startingRoom, room);
		}
	}

	void findDoorways()
	{
		foreach (RoomInstance room in rooms)
		{
			List<Tuple<Vector2i, Vector2i>> possibleDoorways = new List<Tuple<Vector2i, Vector2i>>();

			// Gather possible doorway positions
			{
				Vector2i start = room.pos - new Vector2i(1, 1);
				Vector2i current = start;
				Vector2i direction = new Vector2i(1, 0);
				while (true)
				{
					bool isCorridor = getTile(current.x, current.y) == CID_CORRIDOR;
					bool placeDoor = isCorridor && (current.x >= room.pos.x && current.x < room.pos.x + room.size.x || current.y >= room.pos.y && current.y < room.pos.y + room.size.y);
					if (placeDoor)
					{
						Vector2i doorwayDirection = new Vector2i(direction.y, -direction.x);
						possibleDoorways.Add(new Tuple<Vector2i, Vector2i>(current, doorwayDirection));
					}

					if (direction.x == 1 && current.x == room.pos.x + room.size.x)
						direction = new Vector2i(0, 1);
					else if (direction.y == 1 && current.y == room.pos.y + room.size.y)
						direction = new Vector2i(-1, 0);
					else if (direction.x == -1 && current.x == room.pos.x - 1)
						direction = new Vector2i(0, -1);
					else if (direction.y == -1 && current.y == room.pos.y - 1)
						direction = new Vector2i(1, 0);

					current += direction;
					if (current == start)
						break;
				}
			}

			// Filter doorway positions
			{
				Debug.Assert(possibleDoorways.Count > 0);
				var last = possibleDoorways[0];
				for (int i = 1; i < possibleDoorways.Count; i++)
				{
					var current = possibleDoorways[i];
					if (manhattanDistance(current.Item1, last.Item1) == 1)
					{
						int doorwayToRemove = random.Next() % 2 == 0 ? i - 1 : i;
						possibleDoorways.RemoveAt(doorwayToRemove);
						i--;

						last = current;
					}
					else
					{
						last = current;
					}
				}
			}

			foreach (var possibleDoorway in possibleDoorways)
			{
				Vector2i doorwayPos = possibleDoorway.Item1;
				Vector2i doorwayDir = possibleDoorway.Item2;
				Quaternion rotation = Quaternion.LookAt(new Vector3(doorwayDir.x, 0, doorwayDir.y));
				Vector3 position = (new Vector3(doorwayPos.x, room.height, doorwayPos.y) + new Vector3(0.5f, 0.0f, 0.5f)) * TILE_SIZE + rotation * new Vector3(0.0f, 0.0f, 0.5f * TILE_SIZE);

				DoorwayInstance doorway = new DoorwayInstance();
				doorway.position = doorwayPos;
				doorway.direction = doorwayDir;
				doorway.worldPosition = position;
				doorway.worldRotation = rotation;
				doorway.room = room;
				room.doorways.Add(doorway);
			}
		}

		// Find connections
		foreach (RoomInstance room in rooms)
		{
			foreach (DoorwayInstance doorway in room.doorways)
			{
				foreach (RoomInstance otherRoom in rooms)
				{
					if (room != otherRoom)
					{
						foreach (DoorwayInstance otherDoorway in otherRoom.doorways)
						{
							bool[] walkable = new bool[width * height];
							for (int i = 0; i < tiles.Length; i++)
								walkable[i] = tiles[i] == CID_CORRIDOR;
							List<Vector2i> path = AStar.Run(doorway.position, otherDoorway.position, width, height, walkable);
							if (path != null)
								doorway.connectedDoorways.Add(otherDoorway);
						}
					}
				}
			}
		}
	}

	void floodFillCorridorHeight(Vector2i pos, int height, Vector2i dir, int iteration, int maxIterations = int.MaxValue)
	{
		heightmap[pos.x + pos.y * width] = height;

		if (iteration + 1 == maxIterations)
			return;

		if (dir != Vector2i.Right && getTile(pos.x - 1, pos.y) == CID_CORRIDOR)
		{
			int tileHeight = getHeight(pos.x - 1, pos.y);
			int padding = tileHeight < height ? 1 : 2;
			if (tileHeight == int.MaxValue || iteration + 1 < padding)
				floodFillCorridorHeight(new Vector2i(pos.x - 1, pos.y), height, Vector2i.Left, iteration + 1);
			else if (Math.Abs(tileHeight - height) > 1)
				floodFillCorridorHeight(new Vector2i(pos.x - 1, pos.y), height + Math.Sign(tileHeight - height), Vector2i.Left, iteration + 1);
		}
		if (dir != Vector2i.Left && getTile(pos.x + 1, pos.y) == CID_CORRIDOR)
		{
			int tileHeight = getHeight(pos.x + 1, pos.y);
			int padding = tileHeight < height ? 1 : 2;
			if (tileHeight == int.MaxValue || iteration + 1 < padding)
				floodFillCorridorHeight(new Vector2i(pos.x + 1, pos.y), height, Vector2i.Right, iteration + 1);
			else if (Math.Abs(tileHeight - height) > 1)
				floodFillCorridorHeight(new Vector2i(pos.x + 1, pos.y), height + Math.Sign(tileHeight - height), Vector2i.Right, iteration + 1);
		}
		if (dir != Vector2i.Up && getTile(pos.x, pos.y - 1) == CID_CORRIDOR)
		{
			int tileHeight = getHeight(pos.x, pos.y - 1);
			int padding = tileHeight < height ? 1 : 2;
			if (tileHeight == int.MaxValue || iteration + 1 < padding)
				floodFillCorridorHeight(new Vector2i(pos.x, pos.y - 1), height, Vector2i.Down, iteration + 1);
			else if (Math.Abs(tileHeight - height) > 1)
				floodFillCorridorHeight(new Vector2i(pos.x, pos.y - 1), height + Math.Sign(tileHeight - height), Vector2i.Down, iteration + 1);
		}
		if (dir != Vector2i.Down && getTile(pos.x, pos.y + 1) == CID_CORRIDOR)
		{
			int tileHeight = getHeight(pos.x, pos.y + 1);
			int padding = tileHeight < height ? 1 : 2;
			if (tileHeight == int.MaxValue || iteration + 1 < padding)
				floodFillCorridorHeight(new Vector2i(pos.x, pos.y + 1), height, Vector2i.Up, iteration + 1);
			else if (Math.Abs(tileHeight - height) > 1)
				floodFillCorridorHeight(new Vector2i(pos.x, pos.y + 1), height + Math.Sign(tileHeight - height), Vector2i.Up, iteration + 1);
		}
	}

	int calculateCorridorCeilingHeight(int x, int z)
	{
		int height = getHeight(x, z);

		uint left = getTile(x - 1, z);
		uint right = getTile(x + 1, z);
		uint top = getTile(x, z - 1);
		uint bottom = getTile(x, z + 1);

		int leftHeight = getHeight(x - 1, z);
		int rightHeight = getHeight(x + 1, z);
		int topHeight = getHeight(x, z - 1);
		int bottomHeight = getHeight(x, z + 1);

		int ceilingHeight = 2;

		if (top == CID_CORRIDOR || top == CID_ROOM)
			ceilingHeight = Math.Max(ceilingHeight, topHeight - height + 1);
		if (bottom == CID_CORRIDOR || bottom == CID_ROOM)
			ceilingHeight = Math.Max(ceilingHeight, bottomHeight - height + 1);
		if (left == CID_CORRIDOR || left == CID_ROOM)
			ceilingHeight = Math.Max(ceilingHeight, leftHeight - height + 1);
		if (right == CID_CORRIDOR || right == CID_ROOM)
			ceilingHeight = Math.Max(ceilingHeight, rightHeight - height + 1);

		return ceilingHeight;
	}

	void selectInteriors()
	{
		foreach (RoomInstance room in rooms)
		{
			List<RoomInterior> interiors = new List<RoomInterior>();
			interiors.AddRange(RoomInterior.interiors);
			for (int i = 0; i < interiors.Count; i++)
			{
				if (!interiors[i].doesFit(room))
				{
					interiors.RemoveAt(i);
					i--;
				}
			}
			Debug.Assert(interiors.Count > 0);
			RoomInterior selectedInterior = interiors[random.Next() % interiors.Count];
			room.interior = selectedInterior;
		}
	}

	void generateHeightmap()
	{
		foreach (RoomInstance room in rooms)
		{
			int height = random.Next() % 3 + 1;

			room.height = height;

			for (int z = room.pos.y; z < room.pos.y + room.size.y; z++)
			{
				for (int x = room.pos.x; x < room.pos.x + room.size.x; x++)
				{
					heightmap[x + z * width] = height;
				}
			}

			foreach (DoorwayInstance doorway in room.doorways)
			{
				doorway.height = height;
				doorway.worldPosition.y += height * TILE_SIZE;
				//heightmap[doorway.tilePos.x + doorway.tilePos.y * width] = height;
				floodFillCorridorHeight(doorway.position, height, doorway.direction, 0); // TODO maybe randomize maxIterations here?
			}
		}

		// Ensure that doorways are accessible
		foreach (RoomInstance room in rooms)
		{
			foreach (DoorwayInstance doorway in room.doorways)
			{
				int currentHeight = heightmap[doorway.position.x + doorway.position.y * width];
				setHeight(doorway.position, Math.Min(doorway.height, currentHeight));

				// If the corridor is just 2 tiles long and there is a height difference of 1 (stairs will be placed),
				// then let the stairs be closer to the higher door
				if (doorway.connectedDoorways.Count == 1)
				{
					DoorwayInstance otherDoorway = doorway.connectedDoorways[0];
					if (manhattanDistance(doorway.position, otherDoorway.position) == 1 && otherDoorway.height == doorway.height + 1)
						setHeight(otherDoorway.position, doorway.height);
				}
			}
		}

		for (int z = 0; z < height; z++)
		{
			for (int x = 0; x < width; x++)
			{
				uint tile = getTile(x, z);
				if (tile == CID_ROOM)
				{
					setCeilingHeight(x, z, 2);
				}
				else if (tile == CID_CORRIDOR)
				{
					setCeilingHeight(x, z, calculateCorridorCeilingHeight(x, z));
				}
			}
		}

		level.spawnPoint = new Vector3(startingRoom.pos.x + 0.5f * startingRoom.size.x, startingRoom.height, startingRoom.pos.y + 0.5f * startingRoom.size.y) * TILE_SIZE;
	}

	void generateVoxels()
	{
		Array.Fill(voxels, VoxelType.WALL.id);

		foreach (RoomInstance room in rooms)
		{
			int ceilingHeight = room.interior.ceilingHeight;

			for (int z = room.pos.y; z < room.pos.y + room.size.y; z++)
			{
				for (int x = room.pos.x; x < room.pos.x + room.size.x; x++)
				{
					for (int y = room.height; y < room.height + ceilingHeight; y++)
					{
						setVoxel(x, y, z, null);
					}
					setVoxel(x, room.height - 1, z, VoxelType.FLOOR);
				}
			}
			foreach (DoorwayInstance doorway in room.doorways)
			{
				Quaternion rotation = Quaternion.LookAt(new Vector3(doorway.direction.x, 0.0f, doorway.direction.y));
				entities.Add(new EntitySpawn(typeof(Door), rotation));
				//setVoxel(doorway.position.x, doorway.height, doorway.position.y, type);
				setVoxel(doorway.position.x, doorway.height - 1, doorway.position.y, VoxelType.FLOOR);
			}
		}

		for (int z = 0; z < height; z++)
		{
			for (int x = 0; x < width; x++)
			{
				uint tile = getTile(x, z);
				int height = getHeight(x, z);
				VoxelType voxel = getVoxel(x, height, z);
				if (tile == CID_CORRIDOR && voxel == VoxelType.WALL)
				{
					setVoxel(x, height, z, null);
					setVoxel(x, height + 1, z, null);
					setVoxel(x, height - 1, z, VoxelType.FLOOR);
				}
			}
		}
	}

	void generateStairs()
	{
		for (int z = 0; z < height; z++)
		{
			for (int x = 0; x < width; x++)
			{
				uint tile = getTile(x, z);
				int height = getHeight(x, z);
				if (tile == CID_CORRIDOR)
				{
					uint left = getTile(x - 1, z);
					uint right = getTile(x + 1, z);
					uint top = getTile(x, z - 1);
					uint bottom = getTile(x, z + 1);

					int leftHeight = getHeight(x - 1, z);
					int rightHeight = getHeight(x + 1, z);
					int topHeight = getHeight(x, z - 1);
					int bottomHeight = getHeight(x, z + 1);

					int leftHeight2 = getHeight(x - 2, z);
					int rightHeight2 = getHeight(x + 2, z);
					int topHeight2 = getHeight(x, z - 2);
					int bottomHeight2 = getHeight(x, z + 2);

					if ((top == CID_CORRIDOR || top == CID_ROOM && isDoorway(x, z)) && topHeight == height + 1)
					{
						if (bottomHeight == height)
						{
							if (bottomHeight2 == height && !isDoorway(x, z + 1))
							{
								setVoxel(x, height, z, VoxelType.STAIRS_SHALLOW_NORTH);
								if (!hasFlag(x, z + 1, FLAG_ROOM_WALL))
									setVoxel(x, height + 1, z + 1, null);
							}
							else
							{
								setVoxel(x, height, z, VoxelType.STAIRS_NORTH);
								if (!hasFlag(x, z + 1, FLAG_ROOM_WALL))
									setVoxel(x, height + 1, z + 1, null);
							}
						}
					}
					if ((bottom == CID_CORRIDOR || bottom == CID_ROOM && isDoorway(x, z)) && bottomHeight == height + 1)
					{
						if (topHeight == height)
						{
							if (topHeight2 == height && !isDoorway(x, z - 1))
							{
								setVoxel(x, height, z, VoxelType.STAIRS_SHALLOW_SOUTH);
								if (!hasFlag(x, z - 1, FLAG_ROOM_WALL))
									setVoxel(x, height + 1, z - 1, null);
							}
							else
							{
								setVoxel(x, height, z, VoxelType.STAIRS_SOUTH);
								if (!hasFlag(x, z - 1, FLAG_ROOM_WALL))
									setVoxel(x, height + 1, z - 1, null);
							}
						}
					}
					if ((left == CID_CORRIDOR || left == CID_ROOM && isDoorway(x, z)) && leftHeight == height + 1)
					{
						if (rightHeight == height)
						{
							if (rightHeight2 == height && !isDoorway(x + 1, z))
							{
								setVoxel(x, height, z, VoxelType.STAIRS_SHALLOW_WEST);
								if (!hasFlag(x + 1, z, FLAG_ROOM_WALL))
									setVoxel(x + 1, height + 1, z, null);
							}
							else
							{
								setVoxel(x, height, z, VoxelType.STAIRS_WEST);
								if (!hasFlag(x + 1, z, FLAG_ROOM_WALL))
									setVoxel(x + 1, height + 1, z, null);
							}
						}
					}
					if ((right == CID_CORRIDOR || right == CID_ROOM && isDoorway(x, z)) && rightHeight == height + 1)
					{
						if (leftHeight == height)
						{
							if (leftHeight2 == height && !isDoorway(x - 1, z))
							{
								setVoxel(x, height, z, VoxelType.STAIRS_SHALLOW_EAST);
								if (!hasFlag(x - 1, z, FLAG_ROOM_WALL))
									setVoxel(x - 1, height + 1, z, null);
							}
							else
							{
								setVoxel(x, height, z, VoxelType.STAIRS_EAST);
								if (!hasFlag(x - 1, z, FLAG_ROOM_WALL))
									setVoxel(x - 1, height + 1, z, null);
							}
						}
					}
				}
			}
		}
	}

	bool isDoorway(int x, int z, RoomInstance room)
	{
		if (x < 0 || x >= width || z < 0 || z >= height)
			return false;
		foreach (DoorwayInstance doorway in room.doorways)
		{
			if (doorway.position.x == x && doorway.position.y == z)
				return true;
		}
		return false;
	}

	bool isDoorway(int x, int z)
	{
		if (x < 0 || x >= width || z < 0 || z >= height)
			return false;
		foreach (RoomInstance room in rooms)
		{
			foreach (DoorwayInstance doorway in room.doorways)
			{
				if (doorway.position.x == x && doorway.position.y == z)
					return true;
			}
		}
		return false;
	}

	void generateLevelMeshesAndColliders()
	{
		for (int z = 0; z < height; z++)
		{
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < layers; y++)
				{
					VoxelType voxel = getVoxel(x, y, z);

					Matrix tileMatrix = Matrix.CreateScale(TILE_SIZE) * Matrix.CreateTranslation(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));

					if (voxel == VoxelType.WALL)
					{
						VoxelType left = getVoxel(x - 1, y, z);
						VoxelType right = getVoxel(x + 1, y, z);
						VoxelType forward = getVoxel(x, y, z - 1);
						VoxelType back = getVoxel(x, y, z + 1);
						VoxelType down = getVoxel(x, y - 1, z);
						VoxelType up = getVoxel(x, y + 1, z);
						
						if (!isSolid(back))
							wallBatch.addModel(wall, tileMatrix);
						if (!isSolid(forward))
							wallBatch.addModel(wall, tileMatrix * Matrix.CreateRotation(Vector3.Up, MathF.PI));
						if (!isSolid(left))
							wallBatch.addModel(wall, tileMatrix * Matrix.CreateRotation(Vector3.Up, MathF.PI * -0.5f));
						if (!isSolid(right))
							wallBatch.addModel(wall, tileMatrix * Matrix.CreateRotation(Vector3.Up, MathF.PI * 0.5f));
						if (!isSolid(down))
							wallBatch.addModel(wall, tileMatrix * Matrix.CreateRotation(Vector3.Right, MathF.PI * 0.5f));
						if (!isSolid(up))
							wallBatch.addModel(wall, tileMatrix * Matrix.CreateRotation(Vector3.Right, MathF.PI * -0.5f));

						if (!isSolid(left) || !isSolid(right) || !isSolid(forward) || !isSolid(back) || !isSolid(down) || !isSolid(up))
						{
							level.body.addBoxCollider(new Vector3(0.5f * TILE_SIZE), tileMatrix.translation, Quaternion.Identity);
						}
					}
					else if (voxel == VoxelType.FLOOR)
					{
						VoxelType left = getVoxel(x - 1, y, z);
						VoxelType right = getVoxel(x + 1, y, z);
						VoxelType forward = getVoxel(x, y, z - 1);
						VoxelType back = getVoxel(x, y, z + 1);
						VoxelType down = getVoxel(x, y - 1, z);
						VoxelType up = getVoxel(x, y + 1, z);
						if (!isSolid(back))
							floorBatch.addModel(floor, tileMatrix);
						if (!isSolid(forward))
							floorBatch.addModel(floor, tileMatrix * Matrix.CreateRotation(Vector3.Up, MathF.PI));
						if (!isSolid(left))
							floorBatch.addModel(floor, tileMatrix * Matrix.CreateRotation(Vector3.Up, MathF.PI * -0.5f));
						if (!isSolid(right))
							floorBatch.addModel(floor, tileMatrix * Matrix.CreateRotation(Vector3.Up, MathF.PI * 0.5f));
						if (!isSolid(down))
							floorBatch.addModel(floor, tileMatrix * Matrix.CreateRotation(Vector3.Right, MathF.PI * 0.5f));
						if (!isSolid(up))
							floorBatch.addModel(floor, tileMatrix * Matrix.CreateRotation(Vector3.Right, MathF.PI * -0.5f));
						if (!isSolid(up))
						{
							level.body.addBoxCollider(new Vector3(0.5f * TILE_SIZE), tileMatrix.translation, Quaternion.Identity);
						}
					}
					/*
					else if (voxel == VoxelType.DoorNorth || voxel == VoxelType.DoorSouth || voxel == VoxelType.DoorWest || voxel == VoxelType.DoorEast)
					{
						Quaternion rotation =
							voxel == VoxelType.DoorEast ? Quaternion.FromAxisAngle(Vector3.Up, MathF.PI * -0.5f) :
							voxel == VoxelType.DoorWest ? Quaternion.FromAxisAngle(Vector3.Up, MathF.PI * 0.5f) :
							voxel == VoxelType.DoorNorth ? Quaternion.Identity :
							voxel == VoxelType.DoorSouth ? Quaternion.FromAxisAngle(Vector3.Up, MathF.PI) :
							Quaternion.Identity;
						Vector3 offset = rotation * new Vector3(0.0f, 0.0f, 0.5f * TILE_SIZE - 0.2f);
						level.addEntity(new Door(DoorType.Normal), new Vector3(x + 0.5f, y, z + 0.5f) * TILE_SIZE + offset, rotation);
					}
					*/
					else if (voxel == VoxelType.STAIRS_NORTH || voxel == VoxelType.STAIRS_SOUTH || voxel == VoxelType.STAIRS_WEST || voxel == VoxelType.STAIRS_EAST)
					{
						Matrix transform = Matrix.CreateTranslation(new Vector3(x + 0.5f, y, z + 0.5f) * TILE_SIZE) *
							Matrix.CreateScale(TILE_SIZE);
						Quaternion rotation = Quaternion.Identity;
						if (voxel == VoxelType.STAIRS_SOUTH)
							rotation = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI);
						else if (voxel == VoxelType.STAIRS_WEST)
							rotation = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI * 0.5f);
						else if (voxel == VoxelType.STAIRS_EAST)
							rotation = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI * -0.5f);
						transform *= Matrix.CreateRotation(rotation);

						stairsBatch.addModel(stairs, transform);
						level.body.addBoxCollider(new Vector3(0.5f, 0.1f, 0.5f * MathHelper.Sqrt2) * TILE_SIZE,
							transform.translation + new Vector3(0.0f, 0.5f - 0.1f, 0.0f) * TILE_SIZE,
							rotation * Quaternion.FromAxisAngle(Vector3.Right, MathF.PI * 0.25f));
					}
					else if (voxel == VoxelType.STAIRS_SHALLOW_NORTH || voxel == VoxelType.STAIRS_SHALLOW_SOUTH || voxel == VoxelType.STAIRS_SHALLOW_WEST || voxel == VoxelType.STAIRS_SHALLOW_EAST)
					{
						Matrix transform = Matrix.CreateTranslation(new Vector3(x + 0.5f, y, z + 0.5f) * TILE_SIZE) *
							Matrix.CreateScale(TILE_SIZE);
						Quaternion rotation = Quaternion.Identity;
						if (voxel == VoxelType.STAIRS_SHALLOW_SOUTH)
							rotation = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI);
						else if (voxel == VoxelType.STAIRS_SHALLOW_WEST)
							rotation = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI * 0.5f);
						else if (voxel == VoxelType.STAIRS_SHALLOW_EAST)
							rotation = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI * -0.5f);
						transform *= Matrix.CreateRotation(rotation);

						stairsBatch.addModel(stairsShallow, transform);
						level.body.addBoxCollider(new Vector3(0.5f, 0.1f, 0.5f * MathHelper.Sqrt5) * TILE_SIZE,
							transform.translation + rotation * new Vector3(0.0f, 0.5f - 0.1f, 0.5f) * TILE_SIZE,
							rotation * Quaternion.FromAxisAngle(Vector3.Right, MathF.PI / 6));
					}
				}
			}
		}

		level.levelMeshes.Add(new LevelMesh(wallBatch.createModel(), Matrix.Identity));
		level.levelMeshes.Add(new LevelMesh(floorBatch.createModel(), Matrix.Identity));
		level.levelMeshes.Add(new LevelMesh(stairsBatch.createModel(), Matrix.Identity));

		/*
		foreach (RoomInstance room in rooms)
		{
			for (int z = room.pos.y; z < room.pos.y + room.size.y; z++)
			{
				for (int x = room.pos.x; x < room.pos.x + room.size.x; x++)
				{
					int height = room.height;
					int ceilingHeight = getCeilingHeight(x, z);

					Matrix tileMatrix = Matrix.CreateScale(TILE_SIZE) * Matrix.CreateTranslation(new Vector3(x + 0.5f, height, z + 0.5f));

					modelBatch.addModel(floor, tileMatrix));
					modelBatch.addModel(ceiling, Matrix.CreateTranslation(0, (ceilingHeight - 1) * TILE_SIZE, 0) * tileMatrix));

					if (z == room.pos.y && !isDoorway(x, z - 1, room))
						modelBatch.addModel(wall, tileMatrix));
					if (z == room.pos.y + room.size.y - 1 && !isDoorway(x, z + 1, room))
						modelBatch.addModel(wall, tileMatrix * Matrix.CreateRotation(Vector3.Up, MathF.PI)));
					if (x == room.pos.x && !isDoorway(x - 1, z, room))
						modelBatch.addModel(wall, tileMatrix * Matrix.CreateRotation(Vector3.Up, MathF.PI * 0.5f)));
					if (x == room.pos.x + room.size.x - 1 && !isDoorway(x + 1, z, room))
						modelBatch.addModel(wall, tileMatrix * Matrix.CreateRotation(Vector3.Up, MathF.PI * -0.5f)));
				}
			}

			level.body.addBoxCollider(new Vector3(room.size.x * 0.5f, 0.5f, room.size.y * 0.5f) * TILE_SIZE, new Vector3(room.pos.x + room.size.x * 0.5f, room.height - 0.5f, room.pos.y + room.size.y * 0.5f) * TILE_SIZE, Quaternion.Identity);
		}
		for (int z = 0; z < height; z++)
		{
			for (int x = 0; x < width; x++)
			{
				uint tile = getTile(x, z);
				int height = getHeight(x, z);

				if (height == int.MaxValue)
					continue;

				uint left = getTile(x - 1, z);
				uint right = getTile(x + 1, z);
				uint top = getTile(x, z - 1);
				uint bottom = getTile(x, z + 1);

				if (tile != CID_WALL)
				{
					Debug.Assert(height != int.MaxValue);
					if (tile == CID_CORRIDOR)
					{
						int leftHeight = getHeight(x - 1, z);
						int rightHeight = getHeight(x + 1, z);
						int topHeight = getHeight(x, z - 1);
						int bottomHeight = getHeight(x, z + 1);

						int ceilingHeight = getCeilingHeight(x, z);

						int leftCeilingHeight = getCeilingHeight(x - 1, z) + (leftHeight - height);
						int rightCeilingHeight = getCeilingHeight(x + 1, z) + (rightHeight - height);
						int topCeilingHeight = getCeilingHeight(x, z - 1) + (topHeight - height);
						int bottomCeilingHeight = getCeilingHeight(x, z + 1) + (bottomHeight - height);

						Matrix tileMatrix = Matrix.CreateScale(TILE_SIZE) * Matrix.CreateTranslation(new Vector3(x + 0.5f, height, z + 0.5f));

						if (top == CID_WALL || top == 0)
							for (int i = 0; i < ceilingHeight; i++)
								modelBatch.addModel(wall, Matrix.CreateTranslation(0, i * TILE_SIZE, 0) * tileMatrix));
						if (bottom == CID_WALL || bottom == 0)
							for (int i = 0; i < ceilingHeight; i++)
								modelBatch.addModel(wall, Matrix.CreateTranslation(0, i * TILE_SIZE, 0) * tileMatrix * Matrix.CreateRotation(Vector3.Up, MathF.PI)));
						if (left == CID_WALL || left == 0)
							for (int i = 0; i < ceilingHeight; i++)
								modelBatch.addModel(wall, Matrix.CreateTranslation(0, i * TILE_SIZE, 0) * tileMatrix * Matrix.CreateRotation(Vector3.Up, MathF.PI * 0.5f)));
						if (right == CID_WALL || right == 0)
							for (int i = 0; i < ceilingHeight; i++)
								modelBatch.addModel(wall, Matrix.CreateTranslation(0, i * TILE_SIZE, 0) * tileMatrix * Matrix.CreateRotation(Vector3.Up, MathF.PI * -0.5f)));

						if (top == CID_CORRIDOR)
						{
							for (int i = 0; i < topHeight - height; i++)
								modelBatch.addModel(wall, Matrix.CreateTranslation(0, i * TILE_SIZE, 0) * tileMatrix));
							for (int i = 0; i < ceilingHeight - topCeilingHeight; i++)
								modelBatch.addModel(wall, Matrix.CreateTranslation(0, (ceilingHeight - 1 - i) * TILE_SIZE, 0) * tileMatrix));
						}
						if (bottom == CID_CORRIDOR)
						{
							Matrix rotatedTileMatrix = tileMatrix * Matrix.CreateRotation(Vector3.Up, MathF.PI);
							for (int i = 0; i < bottomHeight - height; i++)
								modelBatch.addModel(wall, Matrix.CreateTranslation(0, i * TILE_SIZE, 0) * rotatedTileMatrix));
							for (int i = 0; i < ceilingHeight - bottomCeilingHeight; i++)
								modelBatch.addModel(wall, Matrix.CreateTranslation(0, (ceilingHeight - 1 - i) * TILE_SIZE, 0) * rotatedTileMatrix));
						}
						if (left == CID_CORRIDOR)
						{
							Matrix rotatedTileMatrix = tileMatrix * Matrix.CreateRotation(Vector3.Up, MathF.PI * 0.5f);
							for (int i = 0; i < leftHeight - height; i++)
								modelBatch.addModel(wall, Matrix.CreateTranslation(0, i * TILE_SIZE, 0) * rotatedTileMatrix));
							for (int i = 0; i < ceilingHeight - leftCeilingHeight; i++)
								modelBatch.addModel(wall, Matrix.CreateTranslation(0, (ceilingHeight - 1 - i) * TILE_SIZE, 0) * rotatedTileMatrix));
						}
						if (right == CID_CORRIDOR)
						{
							Matrix rotatedTileMatrix = tileMatrix * Matrix.CreateRotation(Vector3.Up, -MathF.PI * 0.5f);
							for (int i = 0; i < rightHeight - height; i++)
								modelBatch.addModel(wall, Matrix.CreateTranslation(0, i * TILE_SIZE, 0) * rotatedTileMatrix));
							for (int i = 0; i < ceilingHeight - rightCeilingHeight; i++)
								modelBatch.addModel(wall, Matrix.CreateTranslation(0, (ceilingHeight - 1 - i) * TILE_SIZE, 0) * tileMatrix * Matrix.CreateRotation(Vector3.Up, -MathF.PI * 0.5f)));
						}

						modelBatch.addModel(floor, tileMatrix));
						modelBatch.addModel(ceiling, Matrix.CreateTranslation(0, (ceilingHeight - 1) * TILE_SIZE, 0) * tileMatrix));


						level.body.addBoxCollider(new Vector3(TILE_SIZE * 0.5f), new Vector3(x + 0.5f, height - 0.5f, z + 0.5f) * TILE_SIZE, Quaternion.Identity);
						level.body.addBoxCollider(new Vector3(TILE_SIZE * 0.5f), new Vector3(x + 0.5f, height + ceilingHeight + 0.5f, z + 0.5f) * TILE_SIZE, Quaternion.Identity);

						if (left == CID_WALL || left == 0)
							for (int i = 0; i < ceilingHeight; i++)
								level.body.addBoxCollider(new Vector3(0.1f, 0.5f, 0.5f) * TILE_SIZE, new Vector3(x, i + height + 0.5f, z + 0.5f) * TILE_SIZE, Quaternion.Identity);
						if (right == CID_WALL || right == 0)
							for (int i = 0; i < ceilingHeight; i++)
								level.body.addBoxCollider(new Vector3(0.1f, 0.5f, 0.5f) * TILE_SIZE, new Vector3(x + 1.0f, i + height + 0.5f, z + 0.5f) * TILE_SIZE, Quaternion.Identity);
						if (top == CID_WALL || top == 0)
							for (int i = 0; i < ceilingHeight; i++)
								level.body.addBoxCollider(new Vector3(0.5f, 0.5f, 0.1f) * TILE_SIZE, new Vector3(x + 0.5f, i + height + 0.5f, z) * TILE_SIZE, Quaternion.Identity);
						if (bottom == CID_WALL || bottom == 0)
							for (int i = 0; i < ceilingHeight; i++)
								level.body.addBoxCollider(new Vector3(0.5f, 0.5f, 0.1f) * TILE_SIZE, new Vector3(x + 0.5f, i + height + 0.5f, z + 1.0f) * TILE_SIZE, Quaternion.Identity);
					}
					else if (tile == CID_ROOM)
					{
						if (left == CID_WALL || left == 0)
							level.body.addBoxCollider(new Vector3(0.1f, 0.5f, 0.5f) * TILE_SIZE, new Vector3(x, height + 0.5f, z + 0.5f) * TILE_SIZE, Quaternion.Identity);
						if (right == CID_WALL || right == 0)
							level.body.addBoxCollider(new Vector3(0.1f, 0.5f, 0.5f) * TILE_SIZE, new Vector3(x + 1.0f, height + 0.5f, z + 0.5f) * TILE_SIZE, Quaternion.Identity);
						if (top == CID_WALL || top == 0)
							level.body.addBoxCollider(new Vector3(0.5f, 0.5f, 0.1f) * TILE_SIZE, new Vector3(x + 0.5f, height + 0.5f, z) * TILE_SIZE, Quaternion.Identity);
						if (bottom == CID_WALL || bottom == 0)
							level.body.addBoxCollider(new Vector3(0.5f, 0.5f, 0.1f) * TILE_SIZE, new Vector3(x + 0.5f, height + 0.5f, z + 1.0f) * TILE_SIZE, Quaternion.Identity);
					}
				}
			}
		}
		*/
	}

	void generateInteriors()
	{
		foreach (RoomInstance room in rooms)
		{
			room.interior.initialize(room, level);
		}
	}

	void placeDoors()
	{
		foreach (RoomInstance room in rooms)
		{
			foreach (DoorwayInstance doorway in room.doorways)
			{
				level.addEntity(new Door(DoorType.Normal), doorway.worldPosition, doorway.worldRotation);
			}
		}
	}

	void placeStairsAndLadders()
	{
		for (int z = 0; z < height; z++)
		{
			for (int x = 0; x < width; x++)
			{
				uint tile = getTile(x, z);
				int height = getHeight(x, z);
				if (tile == CID_CORRIDOR)
				{
					uint left = getTile(x - 1, z);
					uint right = getTile(x + 1, z);
					uint top = getTile(x, z - 1);
					uint bottom = getTile(x, z + 1);

					int leftHeight = getHeight(x - 1, z);
					int rightHeight = getHeight(x + 1, z);
					int topHeight = getHeight(x, z - 1);
					int bottomHeight = getHeight(x, z + 1);

					if ((top == CID_CORRIDOR || top == CID_ROOM && isDoorway(x, z)) && topHeight == height + 1)
					{
						Matrix transform = Matrix.CreateTranslation(new Vector3(x + 0.5f, height, z + 0.5f) * TILE_SIZE) *
							Matrix.CreateScale(TILE_SIZE);
						wallBatch.addModel(stairs, transform);
						level.body.addBoxCollider(new Vector3(0.5f, 0.1f, 0.5f * MathHelper.Sqrt2) * TILE_SIZE,
							transform.translation + new Vector3(0.0f, 0.5f - 0.1f, 0.0f) * TILE_SIZE,
							Quaternion.FromAxisAngle(Vector3.Right, MathF.PI * 0.25f));
					}
					if ((bottom == CID_CORRIDOR || bottom == CID_ROOM && isDoorway(x, z)) && bottomHeight == height + 1)
					{
						Matrix transform = Matrix.CreateTranslation(new Vector3(x + 0.5f, height, z + 0.5f) * TILE_SIZE) *
							Matrix.CreateRotation(Vector3.Up, MathF.PI) *
							Matrix.CreateScale(TILE_SIZE);
						wallBatch.addModel(stairs, transform);
						level.body.addBoxCollider(new Vector3(0.5f, 0.1f, 0.5f * MathHelper.Sqrt2) * TILE_SIZE,
							transform.translation + new Vector3(0.0f, 0.5f - 0.1f, 0.0f) * TILE_SIZE,
							Quaternion.FromAxisAngle(Vector3.Right, MathF.PI * -0.25f));
					}
					if ((left == CID_CORRIDOR || left == CID_ROOM && isDoorway(x, z)) && leftHeight == height + 1)
					{
						Matrix transform = Matrix.CreateTranslation(new Vector3(x + 0.5f, height, z + 0.5f) * TILE_SIZE) *
							Matrix.CreateRotation(Vector3.Up, MathF.PI * 0.5f) *
							Matrix.CreateScale(TILE_SIZE);
						wallBatch.addModel(stairs, transform);
						level.body.addBoxCollider(new Vector3(MathHelper.Sqrt2 * 0.5f, 0.1f, 0.5f) * TILE_SIZE,
							transform.translation + new Vector3(0.0f, 0.5f - 0.1f, 0.0f) * TILE_SIZE,
							Quaternion.FromAxisAngle(Vector3.UnitZ, MathF.PI * -0.25f));
					}
					if ((right == CID_CORRIDOR || right == CID_ROOM && isDoorway(x, z)) && rightHeight == height + 1)
					{
						Matrix transform = Matrix.CreateTranslation(new Vector3(x + 0.5f, height, z + 0.5f) * TILE_SIZE) *
							Matrix.CreateRotation(Vector3.Up, MathF.PI * -0.5f) *
							Matrix.CreateScale(TILE_SIZE);
						wallBatch.addModel(stairs, transform);
						level.body.addBoxCollider(new Vector3(MathHelper.Sqrt2 * 0.5f, 0.1f, 0.5f) * TILE_SIZE,
							transform.translation + new Vector3(0.0f, 0.5f - 0.1f, 0.0f) * TILE_SIZE,
							Quaternion.FromAxisAngle(Vector3.UnitZ, MathF.PI * 0.25f));
					}
				}
			}
		}
	}

	void fillRooms()
	{
		// Place objects and decoration
		// Place loot
		// Place enemies

		Vector3 spawnPos = new Vector3(startingRoom.pos.x + 0.5f * startingRoom.size.x, startingRoom.height, startingRoom.pos.y + 0.25f * startingRoom.size.y) * TILE_SIZE;
		level.addEntity(new SkeletonEnemy(), spawnPos, Quaternion.Identity);
	}

	public Level run()
	{
		tiles = new uint[width * height];
		heightmap = new int[width * height];
		Array.Fill(heightmap, int.MaxValue);
		ceilingHeight = new int[width * height];
		Array.Fill(ceilingHeight, 0);
		flags = new uint[width * height];
		Array.Fill(flags, 0u);
		voxels = new uint[width * height * layers];
		Array.Fill(voxels, 0u);

		level = new Level();
		level.width = width;
		level.height = height;
		level.tiles = tiles;
		level.heightmap = heightmap;
		level.rooms = rooms;

		generateTilemap();
		collectRooms();
		filterRooms();
		connectRooms();
		findDoorways();
		selectInteriors();
		generateHeightmap();
		generateVoxels();
		generateStairs();
		generateLevelMeshesAndColliders();
		generateInteriors();
		//placeDoors();
		//placeStairsAndLadders();
		//fillRooms();

		return level;
	}
}
