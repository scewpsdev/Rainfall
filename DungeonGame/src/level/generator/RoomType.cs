using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum SectorType
{
	Room,
	Corridor,
}

public struct DoorwayInfo
{
	public Vector3i position;
	public Vector3i direction;

	public DoorwayInfo(Vector3i position, Vector3i direction)
	{
		this.position = position;
		this.direction = direction;
	}
}

public struct EnemySpawnInfo
{
	public Vector3i tile;

	public EnemySpawnInfo(Vector3i tile)
	{
		this.tile = tile;
	}
}

public class RoomType
{
	public int id;
	public Model model;
	public Model collider;
	public SectorType sectorType;
	public Vector3i size;
	public bool[] mask;
	public List<DoorwayInfo> doorwayInfo = new List<DoorwayInfo>();
	public List<EnemySpawnInfo> enemySpawns = new List<EnemySpawnInfo>();

	public bool isTemplate = false;
	public RoomType originalTemplate = null;


	public RoomType()
	{
	}

	protected void initMask(bool defaultValue)
	{
		mask = new bool[size.x * size.y * size.z];
		Array.Fill(mask, defaultValue);
	}

	protected void setMask(int x, int y, int z, bool value)
	{
		mask[x + y * size.x + z * size.x * size.y] = value;
	}

	protected void fillMask(int x, int y, int z, int width, int height, int depth, bool value)
	{
		for (int zz = z; zz < z + depth; zz++)
		{
			for (int yy = y; yy < y + height; yy++)
			{
				for (int xx = x; xx < x + width; xx++)
				{
					setMask(xx, yy, zz, value);
				}
			}
		}
	}

	protected RoomType copy(RoomType type)
	{
		type.id = id;
		type.model = model;
		type.collider = collider;
		type.sectorType = sectorType;
		type.size = size;
		type.mask = mask != null ? mask.Clone() as bool[] : null;
		type.doorwayInfo = new List<DoorwayInfo>(doorwayInfo.Count);
		type.doorwayInfo.AddRange(doorwayInfo);
		type.isTemplate = false;
		type.originalTemplate = this;
		return type;
	}

	public virtual RoomType createTemplateInstance(Random random)
	{
		Debug.Assert(false);
		return null;
	}

	public virtual SectorType getNextSectorType(Doorway doorway)
	{
		return sectorType == SectorType.Room ? SectorType.Corridor : SectorType.Room;
	}

	public virtual void onSpawn(Room room, Level level, Random random)
	{
	}

	public Matrix getDoorwayTransform(int idx)
	{
		Vector3i position = doorwayInfo[idx].position;
		Vector3i direction = doorwayInfo[idx].direction;
		Matrix transform = Matrix.CreateTranslation((position * 1.0f + new Vector3(0.5f, 0.0f, 0.5f))) * Matrix.CreateRotation(Quaternion.LookAt(direction * 1.0f));
		return transform;
	}

	protected bool isInFrontOfDoorway(Vector3i p, Room room)
	{
		foreach (Doorway doorway in room.doorways)
		{
			Vector3i side = new Vector3i(doorway.globalDirection.z, 0, doorway.globalDirection.x);
			if (doorway.globalPosition - doorway.globalDirection == p ||
				doorway.globalPosition - doorway.globalDirection + side == p ||
				doorway.globalPosition - doorway.globalDirection - side == p)
				return true;
		}
		return false;
	}

	/*
	public int getEntranceDoorwayIdx(SectorType sectorType, Random random)
	{
		while (true)
		{
			int idx = random.Next() % doorwayInfo.Count;
			if (doorwayInfo[idx].sectorType == sectorType || doorwayInfo[idx].sectorType == SectorType.Both)
				return idx;
		}
		//return -1;
	}
	*/


	public static List<RoomType> types = new List<RoomType>();
	public static Dictionary<int, int> idMap = new Dictionary<int, int>();

	public static RoomType StartingRoom;
	public static RoomType FinalRoom;

	public static void Init()
	{
		//StartingRoom = CreateRoomType(1, "room1", SectorType.Room);// new RoomType() { name = "room1", model = Resource.GetModel("res/level/room/room1/room1.gltf"), sectorType = SectorType.Room };
		AddRoomType(StartingRoom = new StartingRoom());
		AddRoomType(FinalRoom = new FinalRoom());
		AddRoomType(new PotRoom());
		AddRoomType(new LibraryRoom());
		AddRoomType(new FountainRoom());
		AddRoomType(new PillarRoom());
		AddRoomType(new StraightCorridor());
		AddRoomType(new LCorridor());
		AddRoomType(new TJunction());

		//LoadRoomType("corridor1", SectorType.Corridor);
		//LoadRoomType("corridor2", SectorType.Corridor);
		//LoadRoomType("corridor3", SectorType.Corridor);

		//LoadRoomType("room2", SectorType.Room);
		//LoadRoomType("room3", SectorType.Room);
	}

	static void AddRoomType(RoomType type)
	{
		types.Add(type);
		type.id = types.Count;
		idMap.Add(type.id, types.Count - 1);
	}

	public static RoomType Get(int id)
	{
		if (idMap.ContainsKey(id))
			return types[idMap[id]];
		return null;
	}

	public static RoomType GetRandom(SectorType sectorType, Random random)
	{
		List<RoomType> candidates = new List<RoomType>();
		foreach (RoomType type in types)
		{
			// TODO check for backwards sector type contraint
			if (type != StartingRoom && type != FinalRoom && type.sectorType == sectorType)
				candidates.Add(type);
		}
		if (candidates.Count == 0)
		{
			foreach (RoomType type in types)
			{
				if (type != StartingRoom && type != FinalRoom)
					candidates.Add(type);
			}
		}
		if (candidates.Count > 0)
		{
			RoomType candidate = candidates[random.Next() % candidates.Count];
			if (candidate.isTemplate)
				return candidate.createTemplateInstance(random);
			else
				return candidate;
		}
		return null;
	}

	public static RoomType GetAStarCorridor(List<Vector3i> path, Vector3i startDirection, Vector3i endDirection, TileMap tilemap, out Matrix transform)
	{
		Vector3i start = path[0];
		Vector3i end = path[path.Count - 1];

		// Remove doorway positions
		//path.RemoveAt(0);
		//path.RemoveAt(path.Count - 1);

		Vector3i min = new Vector3i(int.MaxValue), max = new Vector3i(int.MinValue);
		foreach (Vector3i p in path)
		{
			min = Vector3i.Min(min, p);
			max = Vector3i.Max(max, p);
		}

		int ceilingHeight = 4;
		min.x -= 1;
		min.z -= 1;
		max.x += 1;
		max.z += 1;
		max.y += ceilingHeight - 1;

		Vector3i size = max - min + 1;

		bool[] mask = new bool[size.x * size.y * size.z];
		Array.Fill(mask, false);
		foreach (Vector3i p in path)
		{
			Vector3i local = p - min;
			for (int z = local.z - 1; z <= local.z + 1; z++)
			{
				for (int x = local.x - 1; x <= local.x + 1; x++)
				{
					for (int y = local.y; y < local.y + ceilingHeight; y++)
					{
						if (!tilemap.getFlag(min + new Vector3i(x, y, z), TileMap.FLAG_ROOM_WALL) &&
							!tilemap.getFlag(min + new Vector3i(x, y, z), TileMap.FLAG_STRUCTURE))
						{
							mask[x + y * size.x + z * size.x * size.y] = true;
						}
					}
				}
			}
		}

		List<DoorwayInfo> doorwayPositions = new List<DoorwayInfo>
		{
			new DoorwayInfo(start - min, startDirection),
			new DoorwayInfo(end - min, endDirection)
		};

		RoomType type = new RoomType();
		type.id = 0xFFFF;
		type.model = null;
		type.collider = null;
		type.sectorType = SectorType.Corridor;
		type.size = size;
		type.mask = mask;
		type.doorwayInfo = doorwayPositions;
		type.isTemplate = false;
		type.originalTemplate = null;

		transform = Matrix.CreateTranslation(min * LevelGenerator.TILE_SIZE);

		return type;
	}
}

public class StartingRoom : RoomType
{
	public StartingRoom()
		: base()
	{
		sectorType = SectorType.Room;
		size = new Vector3i(15, 9, 15);

		doorwayInfo.Add(new DoorwayInfo(new Vector3i(7, 0, -1), new Vector3i(0, 0, -1)));
		//doorwayPositions.Add(new DoorwayTransform(new Vector3i(7, 0, 15), new Vector3i(0, 0, 1)));
		//doorwayPositions.Add(new DoorwayTransform(new Vector3i(15, 0, 7), new Vector3i(1, 0, 0)));
		//doorwayPositions.Add(new DoorwayTransform(new Vector3i(-1, 0, 7), new Vector3i(-1, 0, 0)));
	}
}

public class FinalRoom : RoomType
{
	public FinalRoom()
		: base()
	{
		sectorType = SectorType.Room;
		size = new Vector3i(15, 9, 15);

		initMask(true);
		fillMask(0, 0, 0, size.x, size.y, 2, false);
		fillMask(size.x / 2 - 1, 0, 0, 3, 3, 2, true);

		doorwayInfo.Add(new DoorwayInfo(new Vector3i(7, 0, 15), new Vector3i(0, 0, 1)));
	}

	public override void onSpawn(Room room, Level level, Random random)
	{
		Matrix gateTransform = room.transform * Matrix.CreateTranslation(size.x * 0.5f, 0.0f, 2);
		ExitGate gate = new ExitGate();

		Matrix leverTransform = room.transform * Matrix.CreateTranslation(size.x * 0.5f + 2.0f, 1.5f, 2);
		Lever lever = new Lever(gate);

		level.addEntity(gate, gateTransform.translation, gateTransform.rotation);
		level.addEntity(lever, leverTransform.translation, leverTransform.rotation);
	}
}

public class PotRoom : RoomType
{
	public PotRoom()
		: base()
	{
		sectorType = SectorType.Room;
		isTemplate = true;
	}

	public override RoomType createTemplateInstance(Random random)
	{
		RoomType type = copy(new PotRoom());

		int width = MathHelper.RandomInt(6, 20, random);
		int height = MathHelper.RandomInt(6, 20, random);
		int ceilingHeight = 5;
		type.size = new Vector3i(width, ceilingHeight, height);

		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(MathHelper.RandomInt(1, width - 2, random), 0, -1), new Vector3i(0, 0, -1)));
		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(MathHelper.RandomInt(1, width - 2, random), 0, height), new Vector3i(0, 0, 1)));
		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(width, 0, MathHelper.RandomInt(1, height - 2, random)), new Vector3i(1, 0, 0)));
		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(-1, 0, MathHelper.RandomInt(1, height - 2, random)), new Vector3i(-1, 0, 0)));

		int numEnemies = MathHelper.RandomInt(0, 3, random);
		for (int i = 0; i < numEnemies; i++)
		{
			Vector3i position = new Vector3i(MathHelper.RandomInt(1, width - 2, random), 0, MathHelper.RandomInt(1, height - 2, random));
			type.enemySpawns.Add(new EnemySpawnInfo(position));
		}

		return type;
	}

	public override SectorType getNextSectorType(Doorway doorway)
	{
		return size.x < 8 && size.z < 8 ? SectorType.Room : SectorType.Corridor;
	}

	public override void onSpawn(Room room, Level level, Random random)
	{
		//RoomInterior interior = RoomInterior.GetFitting(room, random);
		//interior.initialize(room, level, random);

		// Spawn pots
		for (int z = 0; z < room.gridSize.z; z++)
		{
			for (int x = 0; x < room.gridSize.x; x++)
			{
				if (z < 1 || z >= room.gridSize.z - 1 || x < 1 || x >= room.gridSize.x - 1)
				{
					if (!isInFrontOfDoorway(room.gridPosition + new Vector3i(x, 0, z), room))
					{
						bool spawnPot = random.Next() % 10 == 0;
						if (spawnPot)
						{
							int potType = random.Next() % 3;
							Vector3 position = room.gridPosition + new Vector3(x + 0.5f + MathHelper.RandomFloat(-0.3f, 0.3f, random), 0.0f, z + 0.5f + MathHelper.RandomFloat(-0.3f, 0.3f, random));
							Quaternion rotation = Quaternion.FromAxisAngle(Vector3.Up, random.NextSingle() * MathF.PI * 2);
							level.addEntity(new Pot(potType), position, rotation);
						}
					}
				}
			}
		}

		// Spawn weapon stand
		bool spawnWeaponStand = random.Next() % 10 == 0;
		if (spawnWeaponStand)
		{
			Vector3 position = room.gridPosition + new Vector3(room.gridSize.x * MathHelper.RandomFloat(0.1f, 0.9f, random), 0.0f, room.gridSize.z * MathHelper.RandomFloat(0.1f, 0.9f, random));
			Vector3 roomCenter = room.gridPosition + new Vector3(room.gridSize.x * 0.5f, 0.0f, room.gridSize.z * 0.5f);
			Quaternion rotation = Quaternion.LookAt(roomCenter, position);
			level.addEntity(new WeaponStand(new Item[] { null, Item.Get("longsword"), null }), position, rotation);
		}

		foreach (Doorway doorway in room.doorways)
		{
			if (doorway.spawnDoor && !doorway.secret)
			{
				// Wall torches

				Matrix globalTransform = room.transform * doorway.transform;

				bool spawnLeftTorch = random.Next() % 3 == 1;
				if (spawnLeftTorch)
				{
					Matrix torchTransform = globalTransform * Matrix.CreateTranslation(-2.0f, 1.8f, 0.5f);
					room.addEntity(new WallTorch(), torchTransform.translation, torchTransform.rotation);
				}

				bool spawnRightTorch = random.Next() % 3 == 1;
				if (spawnRightTorch)
				{
					Matrix torchTransform = globalTransform * Matrix.CreateTranslation(2.0f, 1.8f, 0.5f);
					room.addEntity(new WallTorch(), torchTransform.translation, torchTransform.rotation);
				}
			}
		}
	}
}

public class LibraryRoom : RoomType
{
	public LibraryRoom()
		: base()
	{
		sectorType = SectorType.Room;
		isTemplate = true;
	}

	public override RoomType createTemplateInstance(Random random)
	{
		RoomType type = copy(new LibraryRoom());

		int width = MathHelper.RandomInt(6, 20, random);
		int height = MathHelper.RandomInt(6, 20, random);
		int ceilingHeight = 5;
		type.size = new Vector3i(width, ceilingHeight, height);

		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(MathHelper.RandomInt(1, width - 2, random), 0, -1), new Vector3i(0, 0, -1)));
		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(MathHelper.RandomInt(1, width - 2, random), 0, height), new Vector3i(0, 0, 1)));
		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(width, 0, MathHelper.RandomInt(1, height - 2, random)), new Vector3i(1, 0, 0)));
		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(-1, 0, MathHelper.RandomInt(1, height - 2, random)), new Vector3i(-1, 0, 0)));

		return type;
	}

	public override SectorType getNextSectorType(Doorway doorway)
	{
		return size.x < 8 && size.z < 8 ? SectorType.Room : SectorType.Corridor;
	}

	public override void onSpawn(Room room, Level level, Random random)
	{
		//RoomInterior interior = RoomInterior.GetFitting(room, random);
		//interior.initialize(room, level, random);

		for (int z = 1; z < room.gridSize.z - 1; z += 2)
		{
			{
				int x = 0;
				Vector3i p = new Vector3i(x, 0, z);
				if (!isInFrontOfDoorway(room.gridPosition + p, room) && !isInFrontOfDoorway(room.gridPosition + p + Vector3i.Back, room))
				{
					bool spawnShelf = random.Next() % 3 == 0;
					if (spawnShelf)
					{
						Vector3 position = room.gridPosition + p + new Vector3(0.4f, 0.0f, 1.0f);
						Quaternion rotation = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI * 0.5f);
						level.addEntity(new BookShelf(new Item[] { Item.Get("gold") }, random), position, rotation);
					}
				}
			}
			{
				int x = room.gridSize.x - 1;
				Vector3i p = new Vector3i(x, 0, z);
				if (!isInFrontOfDoorway(room.gridPosition + p, room) && !isInFrontOfDoorway(room.gridPosition + p + Vector3i.Back, room))
				{
					bool spawnShelf = random.Next() % 3 == 0;
					if (spawnShelf)
					{
						Vector3 position = room.gridPosition + p + new Vector3(1 - 0.4f, 0.0f, 1.0f);
						Quaternion rotation = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI * -0.5f);
						level.addEntity(new BookShelf(new Item[] { Item.Get("gold") }, random), position, rotation);
					}
				}
			}
		}
		for (int x = 1; x < room.gridSize.x - 1; x += 2)
		{
			{
				int z = 0;
				Vector3i p = new Vector3i(x, 0, z);
				if (!isInFrontOfDoorway(room.gridPosition + p, room) && !isInFrontOfDoorway(room.gridPosition + p + Vector3i.Right, room))
				{
					bool spawnShelf = random.Next() % 3 == 0;
					if (spawnShelf)
					{
						Vector3 position = room.gridPosition + p + new Vector3(1.0f, 0.0f, 0.4f);
						Quaternion rotation = Quaternion.Identity;
						level.addEntity(new BookShelf(new Item[] { Item.Get("gold") }, random), position, rotation);
					}
				}
			}
			{
				int z = room.gridSize.z - 1;
				Vector3i p = new Vector3i(x, 0, z);
				if (!isInFrontOfDoorway(room.gridPosition + p, room) && !isInFrontOfDoorway(room.gridPosition + p + Vector3i.Right, room))
				{
					bool spawnShelf = random.Next() % 3 == 0;
					if (spawnShelf)
					{
						Vector3 position = room.gridPosition + p + new Vector3(1.0f, 0.0f, 1 - 0.4f);
						Quaternion rotation = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI);
						level.addEntity(new BookShelf(new Item[] { Item.Get("gold") }, random), position, rotation);
					}
				}
			}
		}
	}
}

public class FountainRoom : RoomType
{
	public FountainRoom()
		: base()
	{
		sectorType = SectorType.Room;
		isTemplate = true;
	}

	public override RoomType createTemplateInstance(Random random)
	{
		RoomType type = copy(new FountainRoom());

		int size = MathHelper.RandomInt(10, 20, random);
		int ceilingHeight = 5;
		type.size = new Vector3i(size, ceilingHeight, size);

		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(MathHelper.RandomInt(1, size - 2, random), 0, -1), new Vector3i(0, 0, -1)));
		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(MathHelper.RandomInt(1, size - 2, random), 0, size), new Vector3i(0, 0, 1)));
		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(size, 0, MathHelper.RandomInt(1, size - 2, random)), new Vector3i(1, 0, 0)));
		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(-1, 0, MathHelper.RandomInt(1, size - 2, random)), new Vector3i(-1, 0, 0)));

		return type;
	}

	public override SectorType getNextSectorType(Doorway doorway)
	{
		return size.x < 8 && size.z < 8 ? SectorType.Room : SectorType.Corridor;
	}

	public override void onSpawn(Room room, Level level, Random random)
	{
		//RoomInterior interior = RoomInterior.GetFitting(room, random);
		//interior.initialize(room, level, random);

		Vector3 roomCenter = (room.gridPosition + room.gridSize * new Vector3i(1, 0, 1) * 0.5f) * LevelGenerator.TILE_SIZE;
		level.addEntity(new Fountain(), roomCenter, Quaternion.Identity);
	}
}

public class PillarRoom : RoomType
{
	public PillarRoom()
		: base()
	{
		sectorType = SectorType.Room;
		isTemplate = true;
	}

	public override RoomType createTemplateInstance(Random random)
	{
		RoomType type = copy(new PillarRoom());

		int width = MathHelper.RandomInt(15, 20, random);
		int height = MathHelper.RandomInt(15, 20, random);
		int ceilingHeight = 11;
		type.size = new Vector3i(width, ceilingHeight, height);

		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(MathHelper.RandomInt(1, width - 2, random), 0, -1), new Vector3i(0, 0, -1)));
		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(MathHelper.RandomInt(1, width - 2, random), 0, height), new Vector3i(0, 0, 1)));
		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(width, 0, MathHelper.RandomInt(1, height - 2, random)), new Vector3i(1, 0, 0)));
		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(-1, 0, MathHelper.RandomInt(1, height - 2, random)), new Vector3i(-1, 0, 0)));

		int numEnemies = MathHelper.RandomInt(0, 3, random);
		for (int i = 0; i < numEnemies; i++)
		{
			Vector3i position = new Vector3i(MathHelper.RandomInt(1, width - 2, random), 0, MathHelper.RandomInt(1, height - 2, random));
			type.enemySpawns.Add(new EnemySpawnInfo(position));
		}

		return type;
	}

	public override SectorType getNextSectorType(Doorway doorway)
	{
		return size.x < 8 && size.z < 8 ? SectorType.Room : SectorType.Corridor;
	}

	public override void onSpawn(Room room, Level level, Random random)
	{
		//RoomInterior interior = RoomInterior.GetFitting(room, random);
		//interior.initialize(room, level, random);

		int gap = 8;
		int numPillarsX = room.type.size.x / gap;
		int numPillarsZ = room.type.size.z / gap;
		for (int z = 0; z < numPillarsZ; z++)
		{
			for (int x = 0; x < numPillarsX; x++)
			{
				Vector3 roomCenter = (room.gridPosition + room.gridSize * new Vector3i(1, 0, 1) * 0.5f) * LevelGenerator.TILE_SIZE;
				Vector3 position = roomCenter
					+ new Vector3(
					(x - 0.5f * (numPillarsX - 1)) * gap,
					0.0f,
					(z - 0.5f * (numPillarsZ - 1)) * gap
				);
				level.addEntity(new Pillar(), position, Quaternion.Identity);
			}
		}
	}
}

public class StraightCorridor : RoomType
{
	public StraightCorridor()
		: base()
	{
		sectorType = SectorType.Corridor;
		isTemplate = true;
	}

	public override RoomType createTemplateInstance(Random random)
	{
		RoomType type = copy(new StraightCorridor());
		int corridorLength = MathHelper.RandomInt(3, 12, random);
		type.size = new Vector3i(3, 4, corridorLength);
		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(1, 0, corridorLength), new Vector3i(0, 0, 1)));
		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(1, 0, -1), new Vector3i(0, 0, -1)));
		return type;
	}
}

public class LCorridor : RoomType
{
	public LCorridor()
		: base()
	{
		sectorType = SectorType.Corridor;
		isTemplate = true;
	}

	public override RoomType createTemplateInstance(Random random)
	{
		RoomType type = copy(new LCorridor());
		int corridorLengthX = MathHelper.RandomInt(3, 16, random);
		int corridorLengthZ = MathHelper.RandomInt(3, 16, random);
		int corridorWidth = 3;
		int ceilingHeight = 4;
		type.size = new Vector3i(corridorLengthX, ceilingHeight, corridorLengthZ);
		type.mask = new bool[corridorLengthX * ceilingHeight * corridorLengthZ];
		for (int x = 0; x < corridorLengthX; x++)
		{
			for (int z = 0; z < corridorWidth; z++)
			{
				for (int y = 0; y < ceilingHeight; y++)
					type.mask[x + y * type.size.x + z * type.size.x * type.size.y] = true;
			}
		}
		for (int x = corridorLengthX - corridorWidth; x < corridorLengthX; x++)
		{
			for (int z = corridorWidth; z < corridorLengthZ; z++)
			{
				for (int y = 0; y < ceilingHeight; y++)
					type.mask[x + y * type.size.x + z * type.size.x * type.size.y] = true;
			}
		}
		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(corridorLengthX - 2, 0, corridorLengthZ), new Vector3i(0, 0, 1)));
		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(-1, 0, 1), new Vector3i(-1, 0, 0)));
		return type;
	}
}

public class TJunction : RoomType
{
	public TJunction()
		: base()
	{
		sectorType = SectorType.Corridor;
		isTemplate = true;
	}

	public override RoomType createTemplateInstance(Random random)
	{
		RoomType type = copy(new TJunction());

		int width = MathHelper.RandomInt(3, 16, random);
		int height = MathHelper.RandomInt(3, 16, random);
		int corridorWidth = 3;
		int ceilingHeight = 4;

		type.size = new Vector3i(width, ceilingHeight, height);
		type.mask = new bool[width * ceilingHeight * height];
		for (int x = 0; x < width; x++)
		{
			for (int z = 0; z < corridorWidth; z++)
			{
				for (int y = 0; y < ceilingHeight; y++)
					type.mask[x + y * type.size.x + z * type.size.x * type.size.y] = true;
			}
		}
		for (int x = width / 2 - corridorWidth / 2; x < width / 2 - corridorWidth / 2 + corridorWidth; x++)
		{
			for (int z = corridorWidth; z < height; z++)
			{
				for (int y = 0; y < ceilingHeight; y++)
					type.mask[x + y * type.size.x + z * type.size.x * type.size.y] = true;
			}
		}

		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(width / 2, 0, height), new Vector3i(0, 0, 1)));
		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(-1, 0, corridorWidth / 2), new Vector3i(-1, 0, 0)));
		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(width, 0, corridorWidth / 2), new Vector3i(1, 0, 0)));

		return type;
	}
}
