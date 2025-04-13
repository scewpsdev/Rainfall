using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;


public struct DoorwayDefinition
{
	public Vector3i localPosition;
	public Vector3i localDirection;

	public Matrix localTransform => Matrix.CreateTranslation((localPosition + new Vector3(0.5f, 0, 0.5f) - localDirection * 0.5f) * DungeonGenerator.TILE_SIZE) * Matrix.CreateRotation(Quaternion.LookAt((Vector3)localDirection));
}

public struct RoomDefinition
{
	public int id;
	public Model model;
	public Vector3i min;
	public Vector3i max;
	public List<DoorwayDefinition> doorwayDefinitions;
	public Func<Room> roomEntityCreator;
}

public struct TileData
{
	public Room room;
}

public class Tilemap
{
	public Vector3i size;
	public TileData[] tiles;

	public Tilemap(int width, int height, int depth)
	{
		size = new Vector3i(width, height, depth);
		tiles = new TileData[width * height * depth];
	}

	public bool canPlaceRoom(Room room, Matrix transform)
	{
		RoomDefinition definition = DungeonGenerator.roomDefinitions[room.definitionId];
		Vector3 v0 = transform * (definition.min * DungeonGenerator.TILE_SIZE);
		Vector3 v1 = transform * (definition.max * DungeonGenerator.TILE_SIZE);
		Vector3i min = (Vector3i)Vector3.Floor(Vector3.Min(v0, v1) / DungeonGenerator.TILE_SIZE + 0.01f);
		Vector3i max = (Vector3i)Vector3.Floor(Vector3.Max(v0, v1) / DungeonGenerator.TILE_SIZE - 0.01f);

		if (min.x < 0 || min.y < 0 || min.z < 0 || max.x >= size.x || max.y >= size.y || max.z >= size.z)
			return false;

		for (int z = min.z; z <= max.z; z++)
		{
			for (int y = min.y; y <= max.y; y++)
			{
				for (int x = min.x; x <= max.x; x++)
				{
					ref TileData tile = ref tiles[x + y * size.x + z * size.x * size.y];
					if (tile.room != null)
						return false;
				}
			}
		}

		return true;
	}

	public void placeRoom(Room room, Matrix transform)
	{
		RoomDefinition definition = DungeonGenerator.roomDefinitions[room.definitionId];
		Vector3 v0 = transform * (definition.min * DungeonGenerator.TILE_SIZE);
		Vector3 v1 = transform * (definition.max * DungeonGenerator.TILE_SIZE);
		Vector3i min = (Vector3i)Vector3.Floor(Vector3.Min(v0, v1) / DungeonGenerator.TILE_SIZE + 0.01f);
		Vector3i max = (Vector3i)Vector3.Floor(Vector3.Max(v0, v1) / DungeonGenerator.TILE_SIZE - 0.01f);
		for (int z = min.z; z <= max.z; z++)
		{
			for (int y = min.y; y <= max.y; y++)
			{
				for (int x = min.x; x <= max.x; x++)
				{
					ref TileData tile = ref tiles[x + y * size.x + z * size.x * size.y];
					tile.room = room;
				}
			}
		}
	}
}

public class DungeonGenerator
{
	public const float TILE_SIZE = 1;

	public static List<RoomDefinition> roomDefinitions = new List<RoomDefinition>();


	static void LoadRoomDefinition(string path, Func<Room> roomEntityCreator = null)
	{
		RoomDefinition definition = new RoomDefinition();

		definition.id = roomDefinitions.Count;
		definition.model = Resource.GetModel(path);
		definition.min = (Vector3i)Vector3.Ceil(definition.model.boundingBox.min / TILE_SIZE - 0.01f);
		definition.max = Vector3i.Max((Vector3i)Vector3.Ceil(definition.model.boundingBox.max / TILE_SIZE - 0.01f), Vector3i.One);
		definition.doorwayDefinitions = new List<DoorwayDefinition>();
		definition.roomEntityCreator = roomEntityCreator;

		for (int i = 0; i < definition.model.skeleton.nodes.Length; i++)
		{
			Node node = definition.model.skeleton.nodes[i];
			if (node.name.StartsWith("__connector"))
			{
				DoorwayDefinition connector = new DoorwayDefinition();
				connector.localPosition = (Vector3i)Vector3.Floor(node.transform.translation / TILE_SIZE);
				connector.localDirection = (Vector3i)Vector3.Round(node.transform.rotation.forward);
				Debug.Assert(MathF.Abs(connector.localDirection.length - 1) < 0.01f);
				definition.doorwayDefinitions.Add(connector);
			}
		}

		roomDefinitions.Add(definition);
	}

	static void LoadRoomDefinition<T>(string path) where T : Room, new()
	{
		LoadRoomDefinition(path, () => new T());
	}

	public static void Init()
	{
		LoadRoomDefinition("level/dungeon/rooms/room1/room1.gltf");
		LoadRoomDefinition("level/dungeon/rooms/room2/room2.gltf");
		LoadRoomDefinition("level/dungeon/rooms/room3/room3.gltf");
		LoadRoomDefinition("level/dungeon/rooms/room4/room4.gltf");
		LoadRoomDefinition("level/dungeon/rooms/room5/room5.gltf");
		LoadRoomDefinition<ElevatorRoom>("level/dungeon/rooms/room6/room6.gltf");
	}

	static Room CreateRoom(RoomDefinition definition)
	{
		Room room = definition.roomEntityCreator != null ? definition.roomEntityCreator() : new Room();
		room.definitionId = definition.id;
		room.model = definition.model;
		room.body = new RigidBody(room, RigidBodyType.Static);
		room.body.addMeshColliders(definition.model, Matrix.Identity);

		for (int i = 0; i < definition.doorwayDefinitions.Count; i++)
		{
			Doorway doorway = new Doorway(room, i);
			room.doorways.Add(doorway);
		}

		return room;
	}

	static void PlaceRoom(Room room, Matrix transform, Scene scene, Tilemap tilemap, List<Room> rooms)
	{
		tilemap.placeRoom(room, transform);
		rooms.Add(room);

		scene.addEntity(room, transform);
	}

	static Matrix RoomTransform(Vector3i position, Vector3i direction)
	{
		return Matrix.CreateTranslation(position * TILE_SIZE) * Matrix.CreateRotation(Quaternion.LookAt((Vector3)direction));
	}

	static void ConnectRooms(Room room1, int doorway1, Room room2, int doorway2)
	{
		room1.doorways[doorway1].otherDoorway = room2.doorways[doorway2];
		room2.doorways[doorway2].otherDoorway = room1.doorways[doorway1];
	}

	static bool PropagateDoorway(Doorway doorway, Scene scene, Tilemap tilemap, List<Room> rooms, Random random)
	{
		List<Tuple<int, int>> roomCandidates = new List<Tuple<int, int>>();
		for (int i = 0; i < roomDefinitions.Count; i++)
		{
			bool roomTypeFound = false;
			for (int j = 0; j < rooms.Count; j++)
			{
				if (rooms[j].definitionId == i)
				{
					roomTypeFound = true;
					break;
				}
			}
			if (roomTypeFound)
				continue;

			for (int j = 0; j < roomDefinitions[i].doorwayDefinitions.Count; j++)
			{
				roomCandidates.Add(new Tuple<int, int>(i, j));
			}
		}
		MathHelper.ShuffleList(roomCandidates, random);

		for (int i = 0; i < roomCandidates.Count; i++)
		{
			int nextRoomId = roomCandidates[i].Item1; // MathHelper.RandomInt(0, roomDefinitions.Count - 1, random);
			Room nextRoom = CreateRoom(roomDefinitions[nextRoomId]);
			int otherDoorwayId = roomCandidates[i].Item2; // MathHelper.RandomInt(0, roomDefinitions[nextRoom.definitionId].doorwayDefinitions.Count - 1, random);
			DoorwayDefinition otherDoorwayDefinition = roomDefinitions[nextRoom.definitionId].doorwayDefinitions[otherDoorwayId];

			DoorwayDefinition doorwayDefinition = roomDefinitions[doorway.room.definitionId].doorwayDefinitions[doorway.doorwayId];
			Matrix doorwayTransform = doorway.room.getModelMatrix() * doorwayDefinition.localTransform;
			Matrix otherDoorwayTransform = doorwayTransform * Matrix.CreateRotation(Vector3.Up, MathF.PI);
			Matrix nextRoomTransform = otherDoorwayTransform * otherDoorwayDefinition.localTransform.inverted;

			if (tilemap.canPlaceRoom(nextRoom, nextRoomTransform))
			{
				PlaceRoom(nextRoom, nextRoomTransform, scene, tilemap, rooms);
				ConnectRooms(nextRoom, otherDoorwayId, doorway.room, doorway.doorwayId);
				return true;
			}
		}

		return false;
	}

	static bool PropagateRooms(int maxRooms, Scene scene, Tilemap tilemap, List<Room> rooms, Random random)
	{
		List<Doorway> openDoorways = new List<Doorway>();
		foreach (Room room in rooms)
		{
			foreach (Doorway doorway in room.doorways)
			{
				if (doorway.otherDoorway == null)
					openDoorways.Add(doorway);
			}
		}

		MathHelper.ShuffleList(openDoorways, random);

		bool roomPlaced = false;
		for (int i = 0; i < openDoorways.Count && rooms.Count < maxRooms; i++)
		{
			Doorway doorway = openDoorways[i];
			if (PropagateDoorway(doorway, scene, tilemap, rooms, random))
			{
				roomPlaced = true;
				openDoorways.RemoveAt(i--);
			}
		}

		return roomPlaced;
	}

	public static void Generate(WorldManager world, Scene scene)
	{
		Tilemap tilemap = new Tilemap(100, 100, 100);
		List<Room> rooms = new List<Room>();

		uint seed = Hash.hash("12345");
		Random random = new Random((int)seed);

		Room room = CreateRoom(roomDefinitions[0]);
		PlaceRoom(room, RoomTransform(new Vector3i(20, 20, 20), Vector3i.Forward), scene, tilemap, rooms);

		int maxRooms = 9;
		while (rooms.Count < maxRooms)
		{
			if (!PropagateRooms(maxRooms, scene, tilemap, rooms, random))
				break;
		}

		world.spawnPoint = Matrix.CreateTranslation(room.center);

		//scene.addEntity(loadMapBlender("level/hub/hub_level.gltf"));

		//map1 = loadMap(1);
		//spawnPoint = map1.spawnPoint;
		//scene.addEntity(new Entity().load("level/testmap/testmap.rfs"));

		scene.addEntity(new Fireplace(), room.center + new Vector3(-1, 0, -2));

		scene.addEntity(new ItemEntity(new Longsword()), room.center + new Vector3(2, 1.5f, 0));
		scene.addEntity(new ItemEntity(new KingsSword()), room.center + new Vector3(3, 1.5f, 0));
		scene.addEntity(new ItemEntity(new Dagger()), room.center + new Vector3(4, 1.5f, 0));
		scene.addEntity(new ItemEntity(new Spear()), room.center + new Vector3(5, 1.5f, 0));

		scene.addEntity(new Hollow(), room.center + new Vector3(2, 0, -2));

		AudioManager.SetAmbientSound(Resource.GetSound("sound/ambient/dungeon_ambient_1.ogg"), 0.2f);
	}

	static Entity loadMapBlender(string path)
	{
		Entity entity = new Entity();
		entity.model = Resource.GetModel(path);
		entity.body = new RigidBody(entity, RigidBodyType.Static);
		entity.body.addMeshColliders(entity.model, Matrix.Identity);
		return entity;
	}

	static MapPiece loadMap(int map, Scene scene)
	{
		if (SceneFormat.Read($"level/map{map}/map{map}.rfs", out List<SceneFormat.EntityData> entities, out _))
		{
			MapPiece mapPiece = new MapPiece();
			for (int i = 0; i < entities.Count; i++)
			{
				SceneFormat.EntityData entityData = entities[i];

				if (entityData.name.StartsWith("entity_"))
				{
					string entityName = entityData.name.Substring("entity_".Length);
					if (int.TryParse(entityName.Substring(entityName.LastIndexOf('_') + 1), out int _))
						entityName = entityName.Substring(0, entityName.LastIndexOf('_'));

					Entity entity = null;
					if (entityName == "crate")
						entity = new Crate();
					else if (entityName == "iron_door")
						entity = new IronDoor();
					else if (entityName == "torch")
						entity = new Torch();
					else if (entityName.StartsWith("ladder"))
						entity = new Ladder(int.Parse(entityName.Substring(6)));

					if (entity != null)
					{
						scene.addEntity(entity, entityData.position, entityData.rotation, entityData.scale);
						mapPiece.entities.Add(entity);
					}
				}
				else if (entityData.name.StartsWith("creature_"))
				{
					string entityName = entityData.name.Substring("creature_".Length);
					if (int.TryParse(entityName.Substring(entityName.LastIndexOf('_') + 1), out int _))
						entityName = entityName.Substring(0, entityName.LastIndexOf('_'));

					Entity entity = null;
					if (entityName == "hollow")
						entity = new Hollow();

					if (entity != null)
					{
						scene.addEntity(entity, entityData.position, entityData.rotation, entityData.scale);
						mapPiece.entities.Add(entity);
					}
				}
				else if (entityData.name.StartsWith("spawn_"))
				{
					mapPiece.spawnPoint = Matrix.CreateTransform(entityData.position, entityData.rotation);
				}
				else if (entityData.name.StartsWith("item_"))
				{
					string itemName = entityData.name.Substring("item_".Length);
					if (int.TryParse(itemName.Substring(itemName.LastIndexOf('_') + 1), out int _))
						itemName = itemName.Substring(0, itemName.LastIndexOf('_'));

					Item item = null;
					if (itemName == "kings_sword")
						item = new KingsSword();
					else if (itemName == "longsword")
						item = new Longsword();
					else if (itemName == "broken_sword")
						item = new BrokenSword();
					else if (itemName == "dagger")
						item = new Dagger();
					else if (itemName == "darkwood_staff")
						item = new DarkwoodStaff();
					else if (itemName == "crossbow")
						item = new LightCrossbow();
					else if (itemName == "sapphire_ring")
						item = new SapphireRing();

					if (item != null)
					{
						ItemEntity itemEntity = new ItemEntity(item);
						scene.addEntity(itemEntity, entityData.position, entityData.rotation);
						mapPiece.entities.Add(itemEntity);
					}
				}
				else if (entityData.name.StartsWith("envir_"))
				{
					string envirName = entityData.name.Substring("envir_".Length);
					EnvirTrigger trigger = new EnvirTrigger(Resource.GetCubemap($"level/map{map}/{envirName}_cubemap_equirect.png"));
					scene.addEntity(trigger, entityData.position, entityData.rotation, entityData.scale);
					trigger.load(entityData, 0, PhysicsFilter.Player);
					mapPiece.entities.Add(trigger);
				}
				else
				{
					Entity entity = new Entity();
					scene.addEntity(entity, entityData.position, entityData.rotation, entityData.scale);
					entity.load(entityData);
					mapPiece.entities.Add(entity);
				}
			}
			return mapPiece;
		}
		return null;
	}
}
