using Rainfall;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum SectorType
{
	Room,
	Corridor,
}

public class RoomType
{
	public int id;
	public string name;
	public Model model;
	public Model collider;
	public SectorType sectorType;


	static List<RoomType> types = new List<RoomType>();
	static Dictionary<string, int> nameMap = new Dictionary<string, int>();

	public static RoomType StartingRoom;

	static RoomType CreateRoomType(int id, string name, SectorType sectorType)
	{
		RoomType type = new RoomType();
		type.id = id;
		type.name = name;
		type.model = Resource.GetModel("res/level/room/" + name + "/" + name + ".gltf");
		type.collider = Resource.GetModel("res/level/room/" + name + "/" + name + "_collider.gltf");
		if (type.collider == null)
			type.collider = type.model;
		type.sectorType = sectorType;
		return type;
	}

	static RoomType LoadRoomType(string name, SectorType sectorType)
	{
		int id = types.Count + 2;
		RoomType type = CreateRoomType(id, name, sectorType);
		types.Add(type);
		nameMap.Add(name, types.Count - 1);
		return type;
	}

	public static void Init()
	{
		StartingRoom = CreateRoomType(1, "room1", SectorType.Room);// new RoomType() { name = "room1", model = Resource.GetModel("res/level/room/room1/room1.gltf"), sectorType = SectorType.Room };

		LoadRoomType("corridor1", SectorType.Corridor);
		LoadRoomType("corridor2", SectorType.Corridor);
		LoadRoomType("corridor3", SectorType.Corridor);

		LoadRoomType("room2", SectorType.Room);
		LoadRoomType("room3", SectorType.Room);
	}

	public static RoomType Get(string name)
	{
		if (nameMap.ContainsKey(name))
			return types[nameMap[name]];
		return null;
	}

	public static RoomType SelectRoom(SectorType sectorType, Random random)
	{
		int numSectors = 0;
		for (int i = 0; i < types.Count; i++)
		{
			if (types[i].sectorType == sectorType)
				numSectors++;
		}

		if (numSectors == 0)
			return null;

		int sectorID = (int)(Math.Abs(random.NextInt64()) % numSectors);
		numSectors = 0;

		for (int i = 0; i < types.Count; i++)
		{
			if (types[i].sectorType == sectorType)
			{
				if (numSectors == sectorID)
					return types[i];
				numSectors++;
			}
		}

		return null;
	}
}

public class Doorway
{
	public int id;
	public Room room;
	public Room connectedRoom;
	public Matrix transform;


	public Doorway(int id, Room room, Vector3 position, Quaternion rotation)
	{
		this.id = id;
		this.room = room;
		transform = Matrix.CreateTranslation(position) * Matrix.CreateRotation(rotation);
	}
}

public class Room
{
	public RoomType type;
	public Level level;

	public Matrix transform { get; private set; }
	public Vector2i gridPosition, gridSize;

	public List<Doorway> doorways = new List<Doorway>();

	public List<Entity> entities = new List<Entity>();


	public Room(RoomType type, Level level)
	{
		this.type = type;
		this.level = level;

		transform = Matrix.Identity;

		for (int i = 0; i < type.model.skeleton.rootNode.children.Length; i++)
		{
			Node node = type.model.skeleton.rootNode.children[i];
			if (node.name.StartsWith("_tag_node_0"))
			{
				Matrix doorwayTransform = node.transform * Matrix.CreateRotation(Vector3.Right, MathF.PI * 0.5f);
				doorways.Add(new Doorway(int.Parse(node.name.Substring("_tag_node_0".Length)), this, doorwayTransform.translation, doorwayTransform.rotation));
			}
		}

		doorways.Sort((Doorway a, Doorway b) =>
		{
			return a.id < b.id ? -1 : a.id > b.id ? 1 : 0;
		});
	}

	BoundingBox transformBoundingBox(BoundingBox boundingBox, Matrix transform)
	{
		Vector4 p000 = transform * new Vector4(boundingBox.x0, boundingBox.y0, boundingBox.z0, 1.0f);
		Vector4 p001 = transform * new Vector4(boundingBox.x0, boundingBox.y0, boundingBox.z1, 1.0f);
		Vector4 p010 = transform * new Vector4(boundingBox.x0, boundingBox.y1, boundingBox.z0, 1.0f);
		Vector4 p011 = transform * new Vector4(boundingBox.x0, boundingBox.y1, boundingBox.z1, 1.0f);
		Vector4 p100 = transform * new Vector4(boundingBox.x1, boundingBox.y0, boundingBox.z0, 1.0f);
		Vector4 p101 = transform * new Vector4(boundingBox.x1, boundingBox.y0, boundingBox.z1, 1.0f);
		Vector4 p110 = transform * new Vector4(boundingBox.x1, boundingBox.y1, boundingBox.z0, 1.0f);
		Vector4 p111 = transform * new Vector4(boundingBox.x1, boundingBox.y1, boundingBox.z1, 1.0f);

		float x0 = MathF.Min(MathF.Min(MathF.Min(p000.x, p001.x), MathF.Min(p010.x, p011.x)), MathF.Min(MathF.Min(p100.x, p101.x), MathF.Min(p110.x, p111.x)));
		float x1 = MathF.Max(MathF.Max(MathF.Max(p000.x, p001.x), MathF.Max(p010.x, p011.x)), MathF.Max(MathF.Max(p100.x, p101.x), MathF.Max(p110.x, p111.x)));
		float y0 = MathF.Min(MathF.Min(MathF.Min(p000.y, p001.y), MathF.Min(p010.y, p011.y)), MathF.Min(MathF.Min(p100.y, p101.y), MathF.Min(p110.y, p111.y)));
		float y1 = MathF.Max(MathF.Max(MathF.Max(p000.y, p001.y), MathF.Max(p010.y, p011.y)), MathF.Max(MathF.Max(p100.y, p101.y), MathF.Max(p110.y, p111.y)));
		float z0 = MathF.Min(MathF.Min(MathF.Min(p000.z, p001.z), MathF.Min(p010.z, p011.z)), MathF.Min(MathF.Min(p100.z, p101.z), MathF.Min(p110.z, p111.z)));
		float z1 = MathF.Max(MathF.Max(MathF.Max(p000.z, p001.z), MathF.Max(p010.z, p011.z)), MathF.Max(MathF.Max(p100.z, p101.z), MathF.Max(p110.z, p111.z)));

		return new BoundingBox() { x0 = x0, x1 = x1, y0 = y0, y1 = y1, z0 = z0, z1 = z1 };
	}

	public void setTransform(Matrix transform)
	{
		this.transform = transform;

		BoundingBox boundingBox = transformBoundingBox(type.model.boundingBox.Value, transform);

		int x0 = (int)MathF.Floor(boundingBox.x0 + 0.1f);
		int x1 = (int)MathF.Floor(boundingBox.x1 - 0.1f);
		int z0 = (int)MathF.Floor(boundingBox.z0 + 0.1f);
		int z1 = (int)MathF.Floor(boundingBox.z1 - 0.1f);
		int w = x1 - x0 + 1;
		int d = z1 - z0 + 1;

		gridPosition = new Vector2i(x0, z0);
		gridSize = new Vector2i(w, d);
	}

	public void spawn(TileMap tilemap)
	{
		tilemap.placeRoom(gridPosition, gridSize, type);

		//Model levelColliderMesh = Resource.GetModel("res/level/room/" + name + "/" + name + ".gltf");
		//collider = new RigidBody(null, RigidBodyType.Static, transform.translation, transform.rotation);
		//collider.addMeshColliders(levelColliderMesh, Matrix.Identity);
		level.body.addMeshColliders(type.collider, transform);

		for (int i = 0; i < doorways.Count; i++)
		{
			Doorway doorway = doorways[i];
			if (doorway.id != 0)
			{
				Matrix globalTransform = transform * doorway.transform;
				addEntity(new Door(DoorType.Windowed), globalTransform.translation, globalTransform.rotation);
			}
		}
	}

	public bool overlaps(TileMap tilemap)
	{
		return tilemap.overlapsRoom(gridPosition, gridSize);

		/*
		Vector3 position = transform.translation;
		Quaternion rotation = transform.rotation;

		Vector3 aabbMin = new Vector3(type.model.boundingBox.Value.x0, type.model.boundingBox.Value.y0, type.model.boundingBox.Value.z0);
		Vector3 aabbMax = new Vector3(type.model.boundingBox.Value.x1, type.model.boundingBox.Value.y1, type.model.boundingBox.Value.z1);
		aabbMin = (transform * new Vector4(aabbMin, 1.0f)).xyz;
		aabbMax = (transform * new Vector4(aabbMax, 1.0f)).xyz;
		Vector3 middle = (aabbMin + aabbMax) * 0.5f;
		Vector3 halfExtents = Vector3.Abs(aabbMax - aabbMin) * 0.5f;
		halfExtents -= 1.9f;

		//middle.z += -0.5f;

		OverlapHit[] hits = new OverlapHit[16];
		int numHits = PhysicsManager.OverlapBox(halfExtents, middle, rotation, hits, 16, QueryFilterFlags.Static);

		return numHits > 0;
		*/
	}

	public void addEntity(Entity entity, Vector3 position, Quaternion rotation)
	{
		entity.position = position;
		entity.rotation = rotation;
		entities.Add(entity);
		level.addEntity(entity);
	}

	public void update()
	{
		for (int i = 0; i < entities.Count; i++)
		{
			if (entities[i].removed)
			{
				entities.RemoveAt(i);
				i--;
			}
		}
	}

	public void draw(GraphicsDevice graphics)
	{
		Renderer.DrawModel(type.model, transform);

		for (int i = 0; i < entities.Count; i++)
		{
			entities[i].draw(graphics);
		}
	}
}
