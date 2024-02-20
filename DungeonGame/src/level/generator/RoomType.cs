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

	public string coverModel;

	public DoorwayInfo(Vector3i position, Vector3i direction, string coverModel = null, float spawnChance = 0.5f)
	{
		this.position = position;
		this.direction = direction;
		this.spawnChance = spawnChance;
		this.coverModel = coverModel;
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
	public int id { get; private set; }
	public Model model;
	public Model collider;
	public SectorType sectorType = SectorType.Room;
	public Vector3i size;
	public uint[] tiles;

	MeshCollider[] meshColliders;

	public bool allowSecretDoorConnections = true;
	public bool generateWallMeshes = true;

	public List<DoorwayInfo> doorwayInfo = new List<DoorwayInfo>();
	public List<EnemySpawnInfo> enemySpawns = new List<EnemySpawnInfo>();

	public bool isTemplate = false;
	public RoomType originalTemplate = null;


	public RoomType()
	{
	}

	public void initTiles(uint value)
	{
		tiles = new uint[size.x * size.y * size.z];
		Array.Fill(tiles, value);
	}

	public void setTile(int x, int y, int z, uint value)
	{
		tiles[x + y * size.x + z * size.x * size.y] = value;
	}

	public void fillTiles(int x, int y, int z, int width, int height, int depth, uint value)
	{
		for (int zz = z; zz < z + depth; zz++)
		{
			for (int yy = y; yy < y + height; yy++)
			{
				for (int xx = x; xx < x + width; xx++)
				{
					setTile(xx, yy, zz, value);
				}
			}
		}
	}

	public uint getTile(int x, int y, int z)
	{
		if (x >= 0 && x < size.x && y >= 0 && y < size.y && z >= 0 && z < size.z)
		{
			if (tiles != null)
				return tiles[x + y * size.x + z * size.x * size.y];
			return 0;
		}
		return 0;
	}

	public uint getTile(Vector3i p)
	{
		return getTile(p.x, p.y, p.z);
	}

	protected T copy<T>(T type) where T : RoomType
	{
		type.id = id;
		type.model = model;
		type.collider = collider;
		type.sectorType = sectorType;
		type.size = size;
		type.tiles = tiles != null ? tiles.Clone() as uint[] : null;
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

	public virtual SectorType getNextSectorType(Doorway doorway, Random random)
	{
		return sectorType == SectorType.Room ? SectorType.Corridor : SectorType.Room;
	}

	public virtual void onTilemapPlaced(Room room, TileMap tilemap)
	{
	}

	public virtual void onSpawn(Room room, Level level, LevelGenerator generator, Random random)
	{
		Debug.Assert(room.type.size.x != 0 && room.type.size.y != 0 && room.type.size.z != 0);
		if (model != null)
		{
			for (int i = 0; i < model.meshCount; i++)
			{
				bool isDoorwayCover = false;
				Doorway doorway = null;
				for (int j = 0; j < room.doorways.Count; j++)
				{
					unsafe
					{
						if (room.doorways[j].doorwayCover != null && StringUtils.CompareStrings(room.doorways[j].doorwayCover, model.getMeshName(i)))
						{
							isDoorwayCover = true;
							doorway = room.doorways[j];
							break;
						}
					}
				}
				if (isDoorwayCover)
				{
					if (doorway.connectedDoorway == null)
					{
						level.levelMeshes.Add(new LevelMesh(model, i, room));
					}
				}
				else
				{
					level.levelMeshes.Add(new LevelMesh(model, i, room));
				}
			}
		}
		if (collider != null)
		{
			if (meshColliders == null)
			{
				meshColliders = new MeshCollider[collider.meshCount];
			}

			for (int i = 0; i < collider.meshCount; i++)
			{
				bool isDoorwayCover = false;
				for (int j = 0; j < room.doorways.Count; j++)
				{
					unsafe
					{
						if (room.doorways[j].doorwayCover != null && StringUtils.CompareStrings(room.doorways[j].doorwayCover, collider.getMeshName(i)))
						{
							isDoorwayCover = true;
							break;
						}
					}
				}
				if (!isDoorwayCover)
				{
					if (meshColliders[i] == null)
						meshColliders[i] = Physics.CreateMeshCollider(collider, i);
					level.body.addMeshCollider(meshColliders[i], room.transform);
				}
			}
		}
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
		FinalRoom = new BossRoom();
		MainRoom = new MainRoom();

		AddRoomType(new PotRoom());
		AddRoomType(new LibraryRoom());
		AddRoomType(new FountainRoom());
		AddRoomType(new PillarRoom());
		AddRoomType(new StorageRoom());
		AddRoomType(new StudyAlcove());
		AddRoomType(new Prison());

		AddRoomType(new StraightCorridor());
		AddRoomType(new LCorridor());
		AddRoomType(new TJunction());
		AddRoomType(new DiagonalCorridor());
		AddRoomType(new ZCorridor());
		AddRoomType(new Crossroads());
		AddRoomType(new Staircase());
		AddRoomType(new CircularJunction());

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

		uint[] tiles = new uint[size.x * size.y * size.z];
		Array.Fill(tiles, Tile.bricks.id);
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
							tiles[x + y * size.x + z * size.x * size.y] = 0;
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
		type.tiles = tiles;
		type.doorwayInfo = doorwayPositions;
		type.isTemplate = false;
		type.originalTemplate = null;
		type.path = path;

		transform = Matrix.CreateTranslation((Vector3)min);

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

		allowSecretDoorConnections = false;
		generateWallMeshes = false;

		doorwayInfo.Add(new DoorwayInfo(new Vector3i(7, 0, -1), new Vector3i(0, 0, -1)));
		//doorwayPositions.Add(new DoorwayTransform(new Vector3i(7, 0, 15), new Vector3i(0, 0, 1)));
		//doorwayPositions.Add(new DoorwayTransform(new Vector3i(15, 0, 7), new Vector3i(1, 0, 0)));
		//doorwayPositions.Add(new DoorwayTransform(new Vector3i(-1, 0, 7), new Vector3i(-1, 0, 0)));

		model = Resource.GetModel("res/level/level1/dungeon_cell/dungeon_cell.gltf");
		collider = Resource.GetModel("res/level/level1/dungeon_cell/dungeon_cell_collider.gltf");
	}

	public override void onSpawn(Room room, Level level, LevelGenerator generator, Random random)
	{
		base.onSpawn(room, level, generator, random);

		{
			Vector3 position = room.transform * new Vector3(1, 0, 1.5f);
			Quaternion rotation = room.transform.rotation * Quaternion.FromAxisAngle(Vector3.Up, MathF.PI * 0.5f);

			/*
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
			*/

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

			Matrix brokenSwordTransform = room.transform * Matrix.CreateTranslation(0.65f, 0.0f, 11.8f) * Matrix.CreateRotation(Vector3.Up, MathF.PI * 0.6f) * Matrix.CreateRotation(Vector3.Right, MathF.PI * -0.5f);
			room.addEntity(new ItemPickup(Item.Get("debug_weapon")), brokenSwordTransform);
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

public class BossRoom : RoomType
{
	Vector3i preRoomSize = new Vector3i(9, 5, 9);
	Vector3i bossRoomSize = new Vector3i(15, 7, 15);


	public BossRoom()
		: base()
	{
		sectorType = SectorType.Room;

		size = new Vector3i(bossRoomSize.x, bossRoomSize.y, bossRoomSize.z + 2 + preRoomSize.z);

		initTiles(Tile.bricks.id);
		fillTiles(0, 0, 0, bossRoomSize.x, bossRoomSize.y, bossRoomSize.z, 0);
		fillTiles(bossRoomSize.x / 2 - 1, 0, bossRoomSize.z, 3, 3, 2, 0);
		fillTiles(bossRoomSize.x / 2 - preRoomSize.x / 2, 0, bossRoomSize.z + 2, preRoomSize.x, preRoomSize.y, preRoomSize.z, 0);
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
				room.addEntity(new LightObject(new Vector3(1.0f, 0.5f, 0.2f) * 30), position, Quaternion.Identity);
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

		allowSecretDoorConnections = false;
		generateWallMeshes = false;

		doorwayInfo.Add(new DoorwayInfo(new Vector3i(-1, 0, 9), Vector3i.Left));
		doorwayInfo.Add(new DoorwayInfo(new Vector3i(20, 0, 9), Vector3i.Right));
		doorwayInfo.Add(new DoorwayInfo(new Vector3i(20, 16, 14), Vector3i.Right));
		doorwayInfo.Add(new DoorwayInfo(new Vector3i(1, 9, 20), Vector3i.Back));

		model = Resource.GetModel("res/level/level1/pillar_foundation/pillar_foundation.gltf");
		collider = model;
	}

	public override void onSpawn(Room room, Level level, LevelGenerator generator, Random random)
	{
		base.onSpawn(room, level, generator, random);

		//Matrix transform = room.transform * Matrix.CreateTranslation(size.x * 0.5f, 0, size.z * 0.5f);
		//level.levelMeshes.Add(new LevelMesh(model, transform));
		//level.body.addMeshCollider(model, model.getMeshIndex("Stairs"), transform);

		{
			Vector3 position = room.transform * new Vector3(18.5f, 16, 1);
			Quaternion rotation = room.transform.rotation;
			Chest chest = new Chest();
			generator.itemContainers.Add(chest.container);
			//chest.addItem(Item.Get("longsword"));
			//chest.addItem(Item.Get("longbow"));
			//chest.addItem(Item.Get("arrow"), 8);
			//chest.addItem(Item.Get("oak_staff"));
			//chest.addItem(Item.Get("magic_arrow"));
			//chest.addItem(Item.Get("homing_orbs"));
			//chest.addItem(Item.Get("magic_orb"));
			//chest.addItem(Item.Get("mana_flask"));
			//chest.addItem(Item.Get("map"));
			room.addEntity(chest, position, rotation);
		}

		Debug.Assert(room.doorways[2].connectedDoorway != null);
		//if (!generator.isDoorwayConnectedToRoom(room.doorways[2].connectedDoorway, room, true))
		{
			room.addEntity(new ResizableLadder(14), room.transform * new Vector3(0.5f, 9, 3.5f), Quaternion.FromAxisAngle(Vector3.Up, MathF.PI * 0.5f));
		}

		GraphicsManager.skybox = Resource.GetCubemap("res/level/level1/pillar_foundation/spiaggia_di_mondello_1k.hdr");
		GraphicsManager.skyboxIntensity = 5.0f;
		//GraphicsManager.environmentMap = Resource.GetCubemap("res/level/level1/pillar_foundation/spiaggia_di_mondello_1k.hdr");

		//GraphicsManager.sun = new DirectionalLight(new Vector3(-1, -1, -1).normalized, new Vector3(1.0f, 0.9f, 0.7f) * 10.0f, Renderer.graphics);

		ReflectionProbe reflection = new ReflectionProbe(64, room.transform.translation + new Vector3(10, 25, 10), new Vector3(20.1f, 50.1f, 20.1f), room.transform.translation + new Vector3(10, 1, 10), Renderer.graphics);
		level.reflections.Add(reflection);

		room.addEntity(new ReverbZone(new Vector3(20, 50, 20), true, Resource.GetSound("res/level/hub/ambience.ogg")), room.transform);

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

	public override SectorType getNextSectorType(Doorway doorway, Random random)
	{
		return size.x < 8 && size.z < 8 ? SectorType.Room : SectorType.Corridor;
	}

	public override void onSpawn(Room room, Level level, LevelGenerator generator, Random random)
	{
		base.onSpawn(room, level, generator, random);

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
							generator.itemContainers.Add(crate.container);

							/*
							float flaskDropChance = 0.05f;
							if (random.NextSingle() < flaskDropChance)
								crate.container.addItem(Item.Get("flask"));

							float manaFlaskDropChance = 0.05f;
							if (random.NextSingle() < manaFlaskDropChance)
								crate.container.addItem(Item.Get("mana_flask"));

							float firebombDropChance = 0.05f;
							if (random.NextSingle() < firebombDropChance)
								crate.container.addItem(Item.Get("firebomb"));

							float arrowChance = 0.05f;
							if (random.NextSingle() < arrowChance)
								crate.container.addItem(Item.Get("arrow"), MathHelper.RandomInt(3, 7));
							*/

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

	public override SectorType getNextSectorType(Doorway doorway, Random random)
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

	void placeChest(Room room, Vector3 position, Vector3i direction, LevelGenerator generator, Random random)
	{
		Quaternion rotation = Quaternion.LookAt((Vector3)direction);

		/*
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
		*/

		Chest chest = new Chest();
		generator.itemContainers.Add(chest.container);
		room.addEntity(chest, position, rotation);
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
							placeChest(room, position, Vector3i.Left, generator, random);
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
							placeChest(room, position, Vector3i.Right, generator, random);
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
							placeChest(room, position, Vector3i.Forward, generator, random);
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
							placeChest(room, position, Vector3i.Back, generator, random);
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

	public override SectorType getNextSectorType(Doorway doorway, Random random)
	{
		return size.x < 8 && size.z < 8 ? SectorType.Room : SectorType.Corridor;
	}

	public override void onSpawn(Room room, Level level, LevelGenerator generator, Random random)
	{
		//RoomInterior interior = RoomInterior.GetFitting(room, random);
		//interior.initialize(room, level, random);

		Vector3 roomCenter = (room.gridPosition + room.gridSize * new Vector3i(1, 0, 1) * 0.5f);
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

	public override SectorType getNextSectorType(Doorway doorway, Random random)
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
				Vector3 roomCenter = (room.gridPosition + room.gridSize * new Vector3i(1, 0, 1) * 0.5f);
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

public class StorageRoom : RoomType
{
	public StorageRoom()
	{
		size = new Vector3i(8, 5, 11);

		allowSecretDoorConnections = false;
		generateWallMeshes = false;

		doorwayInfo.Add(new DoorwayInfo(new Vector3i(-1, 2, 1), Vector3i.Left, "___DoorwayCover0"));
		doorwayInfo.Add(new DoorwayInfo(new Vector3i(-1, 0, 9), Vector3i.Left, "___DoorwayCover1"));
		doorwayInfo.Add(new DoorwayInfo(new Vector3i(8, 0, 9), Vector3i.Right, "___DoorwayCover2"));

		model = Resource.GetModel("res/level/level1/storage_room/storage_room.gltf");
		collider = Resource.GetModel("res/level/level1/storage_room/storage_room_collider.gltf");
	}

	public override SectorType getNextSectorType(Doorway doorway, Random random)
	{
		return random.Next() % 2 == 0 ? SectorType.Room : SectorType.Corridor;
	}

	public override void onSpawn(Room room, Level level, LevelGenerator generator, Random random)
	{
		base.onSpawn(room, level, generator, random);

		if (room.getDoorwayByID(0) == null)
		{
			float secretChance = 0.5f;
			if (random.NextSingle() < secretChance)
			{
				Vector3 position = room.transform * new Vector3(1.5f, 4.5f, 2.75f);
				Item item = Item.Get("flask"); // replace with food?
				room.addEntity(new ItemPickup(item), position);
			}
		}

		room.addEntity(new LightObject(new Vector3(1.0f, 0.510f, 0.271f) * 4), room.transform * new Vector3(3.5f, 3, 5.5f), Quaternion.Identity);
		room.addEntity(new LightObject(new Vector3(1.0f, 0.510f, 0.271f) * 4), room.transform * new Vector3(1.39074f, 3.16975f, 1.20128f), Quaternion.Identity);
	}
}

public class StudyAlcove : RoomType
{
	public StudyAlcove()
	{
		size = new Vector3i(5, 3, 7);

		allowSecretDoorConnections = false;
		generateWallMeshes = false;

		doorwayInfo.Add(new DoorwayInfo(new Vector3i(5, 0, 2), Vector3i.Right, "___DoorwayCover0"));
		doorwayInfo.Add(new DoorwayInfo(new Vector3i(2, 0, 7), Vector3i.Back, "___DoorwayCover1"));

		model = Resource.GetModel("res/level/level1/study_alcove/study_alcove.gltf");
		collider = Resource.GetModel("res/level/level1/study_alcove/study_alcove_collider.gltf");
	}

	public override SectorType getNextSectorType(Doorway doorway, Random random)
	{
		return SectorType.Room;
	}

	public override void onSpawn(Room room, Level level, LevelGenerator generator, Random random)
	{
		base.onSpawn(room, level, generator, random);

		room.addEntity(new LightObject(new Vector3(1.0f, 0.707586f, 0.48509f) * 2.5f), room.transform * new Vector3(2, 2.4f, 3.6f), Quaternion.Identity);
		room.addEntity(new LightObject(new Vector3(0.768808f, 0.890239f, 1) * 4), room.transform * new Vector3(0.4f, 2.6f, 3.6f), Quaternion.Identity);
	}
}

public class Prison : RoomType
{
	public Prison()
	{
		size = new Vector3i(15, 4, 17);

		allowSecretDoorConnections = false;
		generateWallMeshes = false;

		doorwayInfo.Add(new DoorwayInfo(new Vector3i(7, 0, -1), Vector3i.Forward, "___DoorwayCover0"));
		doorwayInfo.Add(new DoorwayInfo(new Vector3i(7, 0, 17), Vector3i.Back, "___DoorwayCover1"));
		doorwayInfo.Add(new DoorwayInfo(new Vector3i(15, 0, 2), Vector3i.Right, "___DoorwayCover2"));

		model = Resource.GetModel("res/level/level1/prison/prison.gltf");
		collider = Resource.GetModel("res/level/level1/prison/prison_collider.gltf");
	}

	public override SectorType getNextSectorType(Doorway doorway, Random random)
	{
		return SectorType.Corridor;
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
		type.initTiles(Tile.bricks.id);
		type.fillTiles(0, 0, 0, corridorLengthX, ceilingHeight, corridorWidth, 0);
		type.fillTiles(corridorLengthX - corridorWidth, 0, corridorWidth, corridorWidth, ceilingHeight, corridorLengthZ - corridorWidth, 0);

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
		type.initTiles(Tile.bricks.id);
		type.fillTiles(0, 0, 0, width, ceilingHeight, corridorWidth, 0);
		type.fillTiles(width / 2 - corridorWidth / 2, 0, corridorWidth, corridorWidth, ceilingHeight, height - corridorWidth, 0);

		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(width / 2, 0, height), new Vector3i(0, 0, 1)));
		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(-1, 0, corridorWidth / 2), new Vector3i(-1, 0, 0)));
		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(width, 0, corridorWidth / 2), new Vector3i(1, 0, 0)));

		return type;
	}
}

public class DiagonalCorridor : RoomType
{
	public DiagonalCorridor()
	{
		size = new Vector3i(12, 3, 12);
		sectorType = SectorType.Corridor;

		initTiles(0);
		for (int z = 0; z < size.z; z++)
		{
			for (int x = 0; x < size.x; x++)
			{
				int d = x + z;
				fillTiles(x, 0, z, 1, size.y, 1, d >= 8 && d <= 14 ? 0 : Tile.bricks.id);
			}
		}

		//model = Resource.GetModel("res/level/level1/corridor_diagonal/corridor_diagonal.gltf");
		//collider = model;

		doorwayInfo.Add(new DoorwayInfo(new Vector3i(1, 0, 12), Vector3i.Back, "___DoorwayCover0"));
		doorwayInfo.Add(new DoorwayInfo(new Vector3i(12, 0, 1), Vector3i.Right, "___DoorwayCover1"));

		allowSecretDoorConnections = false;
		generateWallMeshes = false;
	}

	public override SectorType getNextSectorType(Doorway doorway, Random random)
	{
		return random.Next() % 4 == 0 ? SectorType.Room : SectorType.Corridor;
	}
}

public class ZCorridor : RoomType
{
	public ZCorridor()
	{
		sectorType = SectorType.Corridor;
		isTemplate = true;
	}

	public override RoomType createTemplateInstance(Random random)
	{
		RoomType type = copy(new ZCorridor());
		int corridorLengthX = MathHelper.RandomInt(4, 16, random);
		int corridorLengthZ = MathHelper.RandomInt(5, 20, random);
		int startX = MathHelper.RandomInt(1, corridorLengthX - 2, random);
		int endX = MathHelper.RandomInt(1, corridorLengthX - 2, random);
		int crossZ = MathHelper.RandomInt(2, corridorLengthZ - 3, random);
		int corridorWidth = 3;
		int ceilingHeight = 4;
		type.size = new Vector3i(corridorLengthX, ceilingHeight, corridorLengthZ);
		type.initTiles(Tile.bricks.id);
		type.fillTiles(startX - 1, 0, 0, corridorWidth, ceilingHeight, crossZ + 1, 0);
		type.fillTiles(endX - 1, 0, crossZ - 1, corridorWidth, ceilingHeight, corridorLengthZ - crossZ + 1, 0);
		type.fillTiles(Math.Min(startX, endX) - 1, 0, crossZ - 1, Math.Abs(endX - startX) + 2, ceilingHeight, 3, 0);

		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(startX, 0, -1), Vector3i.Forward));
		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(endX, 0, corridorLengthZ), Vector3i.Back));

		return type;
	}
}

public class Crossroads : RoomType
{
	public Crossroads()
	{
		sectorType = SectorType.Corridor;
		isTemplate = true;
	}

	public override RoomType createTemplateInstance(Random random)
	{
		RoomType type = copy(new Crossroads());
		int width = MathHelper.RandomInt(5, 20, random);
		int depth = MathHelper.RandomInt(5, 20, random);
		int vertX = MathHelper.RandomInt(2, width - 3, random);
		int horizZ = MathHelper.RandomInt(2, depth - 3, random);
		int corridorWidth = 3;
		int ceilingHeight = 4;
		type.size = new Vector3i(width, ceilingHeight, depth);
		type.initTiles(Tile.bricks.id);
		type.fillTiles(vertX - 1, 0, 0, corridorWidth, ceilingHeight, depth, 0);
		type.fillTiles(0, 0, horizZ - 1, width, ceilingHeight, corridorWidth, 0);
		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(-1, 0, horizZ), Vector3i.Left));
		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(width, 0, horizZ), Vector3i.Right));
		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(vertX, 0, -1), Vector3i.Forward));
		type.doorwayInfo.Add(new DoorwayInfo(new Vector3i(vertX, 0, depth), Vector3i.Back));
		return type;
	}
}

public class Staircase : RoomType
{
	public Staircase()
	{
		size = new Vector3i(20, 14, 20);
		sectorType = SectorType.Corridor;

		model = Resource.GetModel("res/level/level1/staircase/staircase.gltf");
		collider = model;

		doorwayInfo.Add(new DoorwayInfo(new Vector3i(10, 0, 20), Vector3i.Back, "___DoorwayCover0"));
		doorwayInfo.Add(new DoorwayInfo(new Vector3i(10, 0, -1), Vector3i.Forward, "___DoorwayCover1"));
		doorwayInfo.Add(new DoorwayInfo(new Vector3i(10, 10, -1), Vector3i.Forward, "___DoorwayCover2"));
		doorwayInfo.Add(new DoorwayInfo(new Vector3i(20, 10, 3), Vector3i.Right, "___DoorwayCover3"));
		doorwayInfo.Add(new DoorwayInfo(new Vector3i(-1, 6, 10), Vector3i.Left, "___DoorwayCover4"));

		allowSecretDoorConnections = false;
		generateWallMeshes = false;
	}
}

public class CircularJunction : RoomType
{
	public CircularJunction()
	{
		size = new Vector3i(13, 4, 13);
		sectorType = SectorType.Corridor;

		//model = Resource.GetModel("res/level/level1/circular_junction/circular_junction.gltf");
		//collider = model;

		//allowSecretDoorConnections = false;
		//generateWallMeshes = false;

		initTiles(0);
		fillTiles(4, 0, 4, 5, 4, 5, Tile.bricks.id);

		doorwayInfo.Add(new DoorwayInfo(new Vector3i(6, 0, 13), Vector3i.Back, "___DoorwayCover0"));
		doorwayInfo.Add(new DoorwayInfo(new Vector3i(13, 0, 6), Vector3i.Right, "___DoorwayCover1"));
		doorwayInfo.Add(new DoorwayInfo(new Vector3i(6, 0, -1), Vector3i.Forward, "___DoorwayCover2"));
		doorwayInfo.Add(new DoorwayInfo(new Vector3i(-1, 0, 6), Vector3i.Left, "___DoorwayCover3"));
	}
}
