using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum SectorType
{
	None = 0,

	Room,
	Corridor,
}

public struct DoorwayInfo
{
	public Vector3i position;
	public Vector3i direction;
	public float spawnChance;

	public DoorwayInfo(Vector3i position, Vector3i direction, float spawnChance)
	{
		this.position = position;
		this.direction = direction;
		this.spawnChance = spawnChance;
	}

	public DoorwayInfo(Vector3i position, Vector3i direction)
		: this(position, direction, 0.5f)
	{
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

	public bool allowSecretDoorConnections = true;
	public bool generateWallMeshes = true;

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

	public bool getMask(int x, int y, int z)
	{
		if (x >= 0 && x < size.x && y >= 0 && y < size.y && z >= 0 && z < size.z)
		{
			if (mask != null)
				return mask[x + y * size.x + z * size.x * size.y];
			return true;
		}
		return false;
	}

	public bool getMask(Vector3i p)
	{
		return getMask(p.x, p.y, p.z);
	}

	protected T copy<T>(T type) where T : RoomType
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

	public virtual void onTilemapPlaced(Room room, TileMap tilemap)
	{
	}

	public virtual void onSpawn(Room room, Level level, LevelGenerator generator, Random random)
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
	public static RoomType MainRoom;

	public static void Init()
	{
		//StartingRoom = CreateRoomType(1, "room1", SectorType.Room);// new RoomType() { name = "room1", model = Resource.GetModel("res/level/room/room1/room1.gltf"), sectorType = SectorType.Room };
		StartingRoom = new StartingRoom();
		FinalRoom = new FinalRoom();
		MainRoom = new MainRoom();
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
		type.id = 100 + types.Count;
		idMap.Add(type.id, types.Count - 1);
	}

	public static RoomType Get(int id)
	{
		if (id == StartingRoom.id)
			return StartingRoom;
		else if (id == FinalRoom.id)
			return FinalRoom;
		else if (id == MainRoom.id)
			return MainRoom;
		else if (idMap.ContainsKey(id))
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

		AStarRoomType type = new AStarRoomType();
		type.id = 0xFF;
		type.model = null;
		type.collider = null;
		type.sectorType = SectorType.Corridor;
		type.size = size;
		type.mask = mask;
		type.doorwayInfo = doorwayPositions;
		type.isTemplate = false;
		type.originalTemplate = null;
		type.path = path;

		transform = Matrix.CreateTranslation(min * LevelGenerator.TILE_SIZE);

		return type;
	}
}

public class AStarRoomType : RoomType
{
	internal List<Vector3i> path;


	public AStarRoomType()
	{
	}

	public override void onTilemapPlaced(Room room, TileMap tilemap)
	{
		foreach (Vector3i p in path)
		{
			tilemap.setFlag(p, TileMap.FLAG_ASTAR_PATH, true);
		}
	}
}

public class StartingRoom : RoomType
{
	public StartingRoom()
		: base()
	{
		sectorType = SectorType.Room;
		size = new Vector3i(21, 4, 21);
		id = 1;

		allowSecretDoorConnections = false;
		generateWallMeshes = false;

		doorwayInfo.Add(new DoorwayInfo(new Vector3i(7, 0, -1), new Vector3i(0, 0, -1)));
		//doorwayPositions.Add(new DoorwayTransform(new Vector3i(7, 0, 15), new Vector3i(0, 0, 1)));
		//doorwayPositions.Add(new DoorwayTransform(new Vector3i(15, 0, 7), new Vector3i(1, 0, 0)));
		//doorwayPositions.Add(new DoorwayTransform(new Vector3i(-1, 0, 7), new Vector3i(-1, 0, 0)));
	}

	public override void onSpawn(Room room, Level level, LevelGenerator generator, Random random)
	{
		Model model = Resource.GetModel("res/level/room/dungeon_cell/dungeon_cell.gltf");
		level.levelMeshes.Add(new LevelMesh(model, room.transform));

		Model collider = Resource.GetModel("res/level/room/dungeon_cell/dungeon_cell_collider.gltf");
		level.body.addMeshColliders(collider, room.transform);

		{
			Vector3 position = room.transform * new Vector3(1, 0, 1.5f);
			Quaternion rotation = room.transform.rotation * Quaternion.FromAxisAngle(Vector3.Up, MathF.PI * 0.5f);

			// Starting equipment chest
			Chest chest = new Chest();
			chest.addItem(Item.Get("shortsword"));
			chest.addItem(Item.Get("torch"));
			chest.addItem(Item.Get("leather_chestplate"));
			chest.addItem(Item.Get("wooden_round_shield"));
			chest.addItem(Item.Get("flask"));
			//chest.addItem(Item.Get("longsword"));
			//chest.addItem(Item.Get("flask"), 2);
			//chest.addItem(Item.Get("firebomb"), 10);
			room.addEntity(chest, position, rotation);

			level.spawnPoint = room.transform * Matrix.CreateTranslation(2.5f, 0.0f, 12.0f) * Matrix.CreateRotation(Vector3.Up, MathF.PI * 0.5f);
		}

		{
			Matrix doorTransform = room.transform * Matrix.CreateTranslation(5.5f, 0.0f, 6.5f) * Matrix.CreateRotation(Vector3.Up, MathF.PI * -0.5f);
			room.addEntity(new IronDoor(), doorTransform.translation, doorTransform.rotation);

			Matrix torchTransform = room.transform * Matrix.CreateTranslation(0, 1.5f, 6.5f) * Matrix.CreateRotation(Vector3.Up, MathF.PI * 0.5f);
			room.addEntity(new WallTorch(TorchState.Off), torchTransform);
		}
		{
			//Matrix doorTransform = room.transform * Matrix.CreateTranslation(9.5f, 0.0f, 6.5f) * Matrix.CreateRotation(Vector3.Up, MathF.PI * 0.5f);
			//room.addEntity(new IronDoor(), doorTransform.translation, doorTransform.rotation);

			Matrix torchTransform = room.transform * Matrix.CreateTranslation(15, 1.5f, 6.5f) * Matrix.CreateRotation(Vector3.Up, MathF.PI * -0.5f);
			room.addEntity(new WallTorch(TorchState.Off), torchTransform);
		}
		{
			Matrix doorTransform = room.transform * Matrix.CreateTranslation(5.5f, 0.0f, 12.5f) * Matrix.CreateRotation(Vector3.Up, MathF.PI * -0.5f);
			room.addEntity(new IronDoor(Item.Get("key_cell")), doorTransform.translation, doorTransform.rotation);

			Matrix torchTransform = room.transform * Matrix.CreateTranslation(0, 1.5f, 12.5f) * Matrix.CreateRotation(Vector3.Up, MathF.PI * 0.5f);
			WallTorch torch = new WallTorch();
			torch.state = TorchState.Glimming;
			room.addEntity(torch, torchTransform);

			Matrix keyTransform = room.transform * Matrix.CreateTranslation(0.65f, 0.0f, 13.2f) * Matrix.CreateRotation(Vector3.Up, MathF.PI * 0.6f) * Matrix.CreateRotation(Vector3.Right, MathF.PI * -0.5f);
			room.addEntity(new ItemPickup(Item.Get("key_cell")), keyTransform);
		}
		{
			Matrix doorTransform = room.transform * Matrix.CreateTranslation(9.5f, 0.0f, 12.5f) * Matrix.CreateRotation(Vector3.Up, MathF.PI * 0.5f);
			room.addEntity(new IronDoor(), doorTransform.translation, doorTransform.rotation);

			Matrix torchTransform = room.transform * Matrix.CreateTranslation(15, 1.5f, 12.5f) * Matrix.CreateRotation(Vector3.Up, MathF.PI * -0.5f);
			room.addEntity(new WallTorch(TorchState.Off), torchTransform);
		}
		{
			//Matrix doorTransform = room.transform * Matrix.CreateTranslation(5.5f, 0.0f, 18.5f) * Matrix.CreateRotation(Vector3.Up, MathF.PI * -0.5f);
			//room.addEntity(new IronDoor(), doorTransform.translation, doorTransform.rotation);

			//Matrix torchTransform = room.transform * Matrix.CreateTranslation(0, 1.5f, 18.5f) * Matrix.CreateRotation(Vector3.Up, MathF.PI * 0.5f);
			//room.addEntity(new WallTorch(TorchState.Off), torchTransform);
		}
		{
			//Matrix doorTransform = room.transform * Matrix.CreateTranslation(9.5f, 0.0f, 18.5f) * Matrix.CreateRotation(Vector3.Up, MathF.PI * 0.5f);
			//room.addEntity(new IronDoor(), doorTransform.translation, doorTransform.rotation);

			Matrix torchTransform = room.transform * Matrix.CreateTranslation(15, 1.5f, 18.5f) * Matrix.CreateRotation(Vector3.Up, MathF.PI * -0.5f);
			room.addEntity(new WallTorch(TorchState.Off), torchTransform);
		}

		//ReflectionProbe cellReflection = new ReflectionProbe(32, room.transform * new Vector3(2.5f, 2, 12.5f), new Vector3(5.1f, 4.1f, 5.1f), room.transform * new Vector3(2.5f, 2, 12.5f), Renderer.graphics);
		//level.reflections.Add(cellReflection);

		/*
		GraphicsManager.skybox = Resource.GetCubemap("res/level/room/pillar_foundation/spiaggia_di_mondello_1k.hdr");
		GraphicsManager.skyboxIntensity = 5.0f;

		GraphicsManager.sun = new DirectionalLight(new Vector3(-1, -1, -1).normalized, new Vector3(1.0f, 0.9f, 0.7f) * 10.0f, Renderer.graphics);

		ReflectionProbe reflection = new ReflectionProbe(64, transform.translation + new Vector3(0, 25, 0), new Vector3(20.1f, 50.1f, 20.1f), transform.translation + new Vector3(0, 1, 0), Renderer.graphics);
		level.reflections.Add(reflection);
		*/
	}
}

public class FinalRoom : RoomType
{
	Vector3i preRoomSize = new Vector3i(9, 5, 9);
	Vector3i bossRoomSize = new Vector3i(31, 12, 31);


	public FinalRoom()
		: base()
	{
		sectorType = SectorType.Room;
		id = 2;

		size = new Vector3i(bossRoomSize.x, bossRoomSize.y, bossRoomSize.z + 2 + preRoomSize.z);

		initMask(false);
		fillMask(0, 0, 0, bossRoomSize.x, bossRoomSize.y, bossRoomSize.z, true);
		fillMask(bossRoomSize.x / 2 - 1, 0, bossRoomSize.z, 3, 3, 2, true);
		fillMask(bossRoomSize.x / 2 - preRoomSize.x / 2, 0, bossRoomSize.z + 2, preRoomSize.x, preRoomSize.y, preRoomSize.z, true);
		//fillMask(0, 0, 0, size.x, size.y, 2, false);
		//fillMask(size.x / 2 - 1, 0, 0, 3, 3, 2, true);

		doorwayInfo.Add(new DoorwayInfo(new Vector3i(size.x / 2, 0, size.z), new Vector3i(0, 0, 1)));
	}

	public override void onSpawn(Room room, Level level, LevelGenerator generator, Random random)
	{
		Matrix gateTransform = room.transform * Matrix.CreateTranslation(size.x * 0.5f, 0.0f, bossRoomSize.z + 2);
		ExitGate gate = new ExitGate();

		Matrix leverTransform = room.transform * Matrix.CreateTranslation(size.x * 0.5f + 2.0f, 1.5f, bossRoomSize.z + 2);
		Lever lever = new Lever(gate);

		Matrix doorTransform = room.transform * Matrix.CreateTranslation(size.x * 0.5f, 0.0f, bossRoomSize.z);
		DoubleDoor door = new DoubleDoor(gate);

		room.addEntity(gate, gateTransform.translation, gateTransform.rotation);
		room.addEntity(lever, leverTransform.translation, leverTransform.rotation);
		room.addEntity(door, doorTransform.translation, doorTransform.rotation);

		Creature boss = new Jerry();
		room.addEntity(boss, room.transform.translation + 0.5f * bossRoomSize, Quaternion.Identity);
		room.addEntity(new BossRegion(new Vector3(bossRoomSize.x, bossRoomSize.y, bossRoomSize.z - 1), boss), room.transform.translation, Quaternion.Identity);

		for (int z = 0; z < 2; z++)
		{
			for (int x = 0; x < 2; x++)
			{
				Vector3 position = room.transform.translation + new Vector3(bossRoomSize.x * 0.5f * x + bossRoomSize.x * 0.25f, bossRoomSize.y * 0.75f, bossRoomSize.z * 0.5f * z + bossRoomSize.z * 0.25f);
				room.addEntity(new LightObject(new Vector3(1.0f, 0.5f, 0.2f) * 200), position, Quaternion.Identity);
			}
		}

		//level.spawnPoint = room.transform * Matrix.CreateTranslation(2.5f, 0.0f, 12.0f) * Matrix.CreateRotation(Vector3.Up, MathF.PI * 0.5f);
	}
}

public class MainRoom : RoomType
{
	public MainRoom()
		: base()
	{
		sectorType = SectorType.Room;
		size = new Vector3i(20, 50, 20);
		id = 3;

		allowSecretDoorConnections = false;
		generateWallMeshes = false;

		doorwayInfo.Add(new DoorwayInfo(new Vector3i(-1, 0, 9), Vector3i.Left));
		doorwayInfo.Add(new DoorwayInfo(new Vector3i(20, 0, 9), Vector3i.Right));
		doorwayInfo.Add(new DoorwayInfo(new Vector3i(20, 16, 14), Vector3i.Right));
		doorwayInfo.Add(new DoorwayInfo(new Vector3i(1, 9, 20), Vector3i.Back));
	}

	public override void onSpawn(Room room, Level level, LevelGenerator generator, Random random)
	{
		Model model = Resource.GetModel("res/level/room/pillar_foundation/pillar_foundation.gltf");
		Matrix transform = room.transform * Matrix.CreateTranslation(size.x * 0.5f, 0, size.z * 0.5f);
		level.levelMeshes.Add(new LevelMesh(model, transform));
		level.body.addMeshCollider(model, model.getMeshIndex("Stairs"), transform);

		{
			Vector3 position = room.transform * new Vector3(18.5f, 16, 1);
			Quaternion rotation = room.transform.rotation;
			Chest chest = new Chest();
			//chest.addItem(Item.Get("longsword"));
			chest.addItem(Item.Get("longbow"));
			chest.addItem(Item.Get("arrow"), 8);
			chest.addItem(Item.Get("oak_staff"));
			chest.addItem(Item.Get("magic_arrow"));
			chest.addItem(Item.Get("homing_orbs"));
			chest.addItem(Item.Get("magic_orb"));
			chest.addItem(Item.Get("mana_flask"));
			chest.addItem(Item.Get("map"));
			room.addEntity(chest, position, rotation);
		}

		Debug.Assert(room.doorways[2].connectedDoorway != null);
		if (!generator.isDoorwayConnectedToRoom(room.doorways[2].connectedDoorway, room))
		{
			room.addEntity(new ResizableLadder(14), room.transform * new Vector3(0.5f, 9, 3.5f), Quaternion.FromAxisAngle(Vector3.Up, MathF.PI * 0.5f));
		}

		GraphicsManager.skybox = Resource.GetCubemap("res/level/room/pillar_foundation/spiaggia_di_mondello_1k.hdr");
		GraphicsManager.skyboxIntensity = 5.0f;

		GraphicsManager.sun = new DirectionalLight(new Vector3(-1, -1, -1).normalized, new Vector3(1.0f, 0.9f, 0.7f) * 10.0f, Renderer.graphics);

		ReflectionProbe reflection = new ReflectionProbe(64, transform.translation + new Vector3(0, 25, 0), new Vector3(20.1f, 50.1f, 20.1f), transform.translation + new Vector3(0, 1, 0), Renderer.graphics);
		level.reflections.Add(reflection);

		room.addEntity(new ReverbZone(new Vector3(20, 50, 20), false, Resource.GetSound("res/level/hub/ambience.ogg")), room.transform);

		//level.spawnPoint = room.transform * Matrix.CreateTranslation(2.5f, 0.0f, 12.0f) * Matrix.CreateRotation(Vector3.Up, MathF.PI * 0.5f);
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

	public override void onSpawn(Room room, Level level, LevelGenerator generator, Random random)
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
						int i = random.Next() % 10;
						if (i == 0)
						{
							int potType = random.Next() % 3;
							Vector3 position = room.gridPosition + new Vector3(x + 0.5f + MathHelper.RandomFloat(-0.3f, 0.3f, random), 0.0f, z + 0.5f + MathHelper.RandomFloat(-0.3f, 0.3f, random));
							Quaternion rotation = Quaternion.FromAxisAngle(Vector3.Up, random.NextSingle() * MathF.PI * 2);
							level.addEntity(new Pot(potType), position, rotation);
						}
						else if (i == 1)
						{
							Vector3 position = room.gridPosition + new Vector3(x + 0.5f + MathHelper.RandomFloat(-0.3f, 0.3f, random), 0.0f, z + 0.5f + MathHelper.RandomFloat(-0.3f, 0.3f, random));
							Quaternion rotation = Quaternion.FromAxisAngle(Vector3.Up, random.NextSingle() * MathF.PI * 2);

							Crate crate = new Crate();

							float flaskDropChance = 0.05f;
							if (Random.Shared.NextSingle() < flaskDropChance)
								crate.container.addItem(Item.Get("flask"));

							float manaFlaskDropChance = 0.05f;
							if (Random.Shared.NextSingle() < manaFlaskDropChance)
								crate.container.addItem(Item.Get("mana_flask"));

							float firebombDropChance = 0.05f;
							if (Random.Shared.NextSingle() < firebombDropChance)
								crate.container.addItem(Item.Get("firebomb"));

							float arrowChance = 0.05f;
							if (Random.Shared.NextSingle() < arrowChance)
								crate.container.addItem(Item.Get("arrow"), MathHelper.RandomInt(3, 7));

							level.addEntity(crate, position, rotation);
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
			if (!doorway.secret)
			{
				// Wall torches

				Matrix globalTransform = room.transform * doorway.transform;

				bool spawnLeftTorch = random.Next() % 2 == 1;
				if (spawnLeftTorch)
				{
					Matrix torchTransform = globalTransform * Matrix.CreateTranslation(-2.0f, 1.8f, 0.5f);
					room.addEntity(new WallTorch(), torchTransform.translation, torchTransform.rotation);
				}

				bool spawnRightTorch = random.Next() % 2 == 1;
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

	void placeShelf(Room room, Vector3 position, Vector3i direction, Random random)
	{
		Quaternion rotation = Quaternion.LookAt((Vector3)direction);

		List<Item> items = new List<Item>();
		List<int> amounts = new List<int>();

		float flaskChance = 0.12f;
		if (random.NextSingle() < flaskChance)
		{
			items.Add(Item.Get("flask"));
			amounts.Add(1);
		}
		float manaFlaskChance = 0.12f;
		if (random.NextSingle() < manaFlaskChance)
		{
			items.Add(Item.Get("mana_flask"));
			amounts.Add(1);
		}

		float arrowChance = 0.12f;
		if (random.NextSingle() < arrowChance)
		{
			items.Add(Item.Get("arrow"));
			int amount = MathHelper.RandomInt(1, 4, random);
			amounts.Add(amount);
		}

		float goldChance = 0.1f;
		if (random.NextSingle() < goldChance)
		{
			items.Add(Item.Get("gold"));
			int amount = MathHelper.RandomInt(3, 10, random);
			amounts.Add(amount);
		}

		if (items.Count > 0)
			room.addEntity(new BookShelf(items.ToArray(), amounts.ToArray(), random), position, rotation);
	}

	void placeChest(Room room, Vector3 position, Vector3i direction, Random random)
	{
		Quaternion rotation = Quaternion.LookAt((Vector3)direction);

		List<Item> items = new List<Item>();
		List<int> amounts = new List<int>();

		float flaskChance = 0.1f;
		if (random.NextSingle() < flaskChance)
		{
			items.Add(Item.Get("flask"));
			amounts.Add(1);
		}
		float manaFlaskChance = 0.1f;
		if (random.NextSingle() < manaFlaskChance)
		{
			items.Add(Item.Get("mana_flask"));
			amounts.Add(1);
		}

		float firebombChance = 0.1f;
		if (random.NextSingle() < firebombChance)
		{
			items.Add(Item.Get("firebomb"));
			int amount = MathHelper.RandomInt(1, 3, random);
			amounts.Add(amount);
		}

		float arrowChance = 0.08f;
		if (random.NextSingle() < arrowChance)
		{
			items.Add(Item.Get("arrow"));
			int amount = MathHelper.RandomInt(1, 4, random);
			amounts.Add(amount);
		}

		float goldChance = 0.25f;
		if (random.NextSingle() < goldChance || items.Count == 0)
		{
			items.Add(Item.Get("gold"));
			int amount = MathHelper.RandomInt(3, 10, random);
			amounts.Add(amount);
		}

		room.addEntity(new Chest(items.ToArray(), amounts.ToArray()), position, rotation);
	}

	public override void onSpawn(Room room, Level level, LevelGenerator generator, Random random)
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
					Vector3 position = room.gridPosition + p + new Vector3(0.4f, 0.0f, 1.0f);

					bool spawnShelf = random.Next() % 5 == 0;
					if (spawnShelf)
					{
						placeShelf(room, position, Vector3i.Left, random);
					}
					else
					{
						bool spawnChest = random.Next() % 10 == 0;
						if (spawnChest)
						{
							placeChest(room, position, Vector3i.Left, random);
						}
					}
				}
			}
			{
				int x = room.gridSize.x - 1;
				Vector3i p = new Vector3i(x, 0, z);
				if (!isInFrontOfDoorway(room.gridPosition + p, room) && !isInFrontOfDoorway(room.gridPosition + p + Vector3i.Back, room))
				{
					Vector3 position = room.gridPosition + p + new Vector3(1 - 0.4f, 0.0f, 1.0f);

					bool spawnShelf = random.Next() % 5 == 0;
					if (spawnShelf)
					{
						placeShelf(room, position, Vector3i.Right, random);
					}
					else
					{
						bool spawnChest = random.Next() % 10 == 0;
						if (spawnChest)
						{
							placeChest(room, position, Vector3i.Right, random);
						}
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
					Vector3 position = room.gridPosition + p + new Vector3(1.0f, 0.0f, 0.4f);

					bool spawnShelf = random.Next() % 5 == 0;
					if (spawnShelf)
					{
						placeShelf(room, position, Vector3i.Forward, random);
					}
					else
					{
						bool spawnChest = random.Next() % 10 == 0;
						if (spawnChest)
						{
							placeChest(room, position, Vector3i.Forward, random);
						}
					}
				}
			}
			{
				int z = room.gridSize.z - 1;
				Vector3i p = new Vector3i(x, 0, z);
				if (!isInFrontOfDoorway(room.gridPosition + p, room) && !isInFrontOfDoorway(room.gridPosition + p + Vector3i.Right, room))
				{
					Vector3 position = room.gridPosition + p + new Vector3(1.0f, 0.0f, 1 - 0.4f);

					bool spawnShelf = random.Next() % 5 == 0;
					if (spawnShelf)
					{
						placeShelf(room, position, Vector3i.Back, random);
					}
					else
					{
						bool spawnChest = random.Next() % 10 == 0;
						if (spawnChest)
						{
							placeChest(room, position, Vector3i.Back, random);
						}
					}
				}
			}
		}

		/*
		foreach (Doorway doorway in room.doorways)
		{
			if (!doorway.secret)
			{
				// Wall torches

				Matrix globalTransform = room.transform * doorway.transform;

				bool spawnLeftTorch = random.Next() % 2 == 1;
				if (spawnLeftTorch)
				{
					Matrix torchTransform = globalTransform * Matrix.CreateTranslation(-2.0f, 1.8f, 0.5f);
					room.addEntity(new WallTorch(), torchTransform.translation, torchTransform.rotation);
				}

				bool spawnRightTorch = random.Next() % 2 == 1;
				if (spawnRightTorch)
				{
					Matrix torchTransform = globalTransform * Matrix.CreateTranslation(2.0f, 1.8f, 0.5f);
					room.addEntity(new WallTorch(), torchTransform.translation, torchTransform.rotation);
				}
			}
		}
		*/
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

	public override void onSpawn(Room room, Level level, LevelGenerator generator, Random random)
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

	public override void onSpawn(Room room, Level level, LevelGenerator generator, Random random)
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
