using Rainfall;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Doorway
{
	public int id;
	public Room room;
	public Doorway connectedDoorway;
	public Vector3i position, direction;
	public Matrix transform;
	public Vector3i globalPosition, globalDirection;
	public float spawnChance;

	public bool spawnDoor = false;
	public bool secret = false;


	public Doorway(int id, Room room, Vector3i position, Vector3i direction, float spawnChance)
	{
		this.id = id;
		this.room = room;
		this.position = position;
		this.direction = direction;
		this.spawnChance = spawnChance;

		transform = Matrix.CreateTranslation((position * 1.0f + new Vector3(0.5f, 0.0f, 0.5f))) * Matrix.CreateRotation(Quaternion.LookAt(direction * 1.0f));
		Matrix doorwayGlobalTransform = room.transform * transform;
		globalPosition = (Vector3i)Vector3.Floor(doorwayGlobalTransform.translation);
		globalDirection = (Vector3i)Vector3.Round(doorwayGlobalTransform.rotation.forward);

	}
}

public class EnemySpawn
{
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	public Type enemyType;
	public Vector3 position;
	public Quaternion rotation;

	public EnemySpawn([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type enemyType, Vector3 position, Quaternion rotation)
	{
		this.enemyType = enemyType;
		this.position = position;
		this.rotation = rotation;
	}
}

public class Room
{
	static int idCounter = 1;

	public readonly int id;
	public RoomType type;
	public Level level;

	public Matrix transform { get; private set; }
	public Vector3i gridPosition, gridSize;

	public List<Doorway> doorways = new List<Doorway>();
	public List<EnemySpawn> enemySpawns = new List<EnemySpawn>();

	public List<Entity> entities = new List<Entity>();


	public Room(RoomType type, Matrix transform, Level level)
	{
		id = type.id * 100 + idCounter++;
		this.type = type;
		this.level = level;
		this.transform = transform;

		BoundingBox boundingBox = new BoundingBox(0.0f, 0.0f, 0.0f, type.size.x, type.size.y, type.size.z);
		boundingBox = transformBoundingBox(boundingBox, transform);

		int x0 = (int)MathF.Floor(boundingBox.x0 + 0.1f);
		int x1 = (int)MathF.Floor(boundingBox.x1 - 0.1f);
		int y0 = (int)MathF.Floor(boundingBox.y0 + 0.1f);
		int y1 = (int)MathF.Floor(boundingBox.y1 - 0.1f);
		int z0 = (int)MathF.Floor(boundingBox.z0 + 0.1f);
		int z1 = (int)MathF.Floor(boundingBox.z1 - 0.1f);
		int w = x1 - x0 + 1;
		int h = y1 - y0 + 1;
		int d = z1 - z0 + 1;

		gridPosition = new Vector3i(x0, y0, z0);
		gridSize = new Vector3i(w, h, d);

		if (type.model != null)
		{
			for (int i = 0; i < type.model.skeleton.rootNode.children.Length; i++)
			{
				Node node = type.model.skeleton.rootNode.children[i];
				if (node.name.StartsWith("_tag_node_0"))
				{
					Matrix doorwayTransform = node.transform * Matrix.CreateRotation(Vector3.Right, MathF.PI * 0.5f);
					//doorways.Add(new Doorway(int.Parse(node.name.Substring("_tag_node_0".Length)), this, doorwayTransform.translation, doorwayTransform.rotation));
				}
			}
		}

		doorways.Sort((Doorway a, Doorway b) =>
		{
			return a.id < b.id ? -1 : a.id > b.id ? 1 : 0;
		});
	}

	public void placeDoorways(TileMap tilemap)
	{
		for (int i = 0; i < type.doorwayInfo.Count; i++)
		{
			Vector3i position = type.doorwayInfo[i].position;
			Vector3i direction = type.doorwayInfo[i].direction;
			float spawnChance = type.doorwayInfo[i].spawnChance;
			Doorway doorway = new Doorway(i, this, position, direction, spawnChance);
			doorways.Add(doorway);
		}
	}

	public void chooseEnemies(Random random)
	{
		for (int i = 0; i < type.enemySpawns.Count; i++)
		{
			Vector3 tilePosition = type.enemySpawns[i].tile + new Vector3(0.5f, 0.0f, 0.5f);
			Vector3 position = transform * tilePosition;
			Quaternion rotation = Quaternion.FromAxisAngle(Vector3.Up, random.NextSingle() * MathF.PI * 2);
			enemySpawns.Add(new EnemySpawn(typeof(SkeletonEnemy), position, rotation));
		}
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

	public void setTransform(Vector3i position, Vector3i direction)
	{
		transform = Matrix.CreateTranslation(position * 1.0f) * Matrix.CreateRotation(Quaternion.LookAt(direction * 1.0f));

		BoundingBox boundingBox = new BoundingBox(0.0f, 0.0f, 0.0f, type.size.x, 0.0f, type.size.y);
		boundingBox = transformBoundingBox(boundingBox, transform);

		int x0 = (int)MathF.Floor(boundingBox.x0 + 0.1f);
		int x1 = (int)MathF.Floor(boundingBox.x1 - 0.1f);
		int z0 = (int)MathF.Floor(boundingBox.z0 + 0.1f);
		int z1 = (int)MathF.Floor(boundingBox.z1 - 0.1f);
		int w = x1 - x0 + 1;
		int d = z1 - z0 + 1;

		//gridPosition = new Vector2i(x0, z0);
		//gridSize = new Vector2i(w, d);
	}

	bool isDoorway(int x, int y, int z)
	{
		foreach (Doorway doorway in doorways)
		{
			if (doorway.position.x == x && doorway.position.y == y && doorway.position.z == z)
				return true;
		}
		return false;
	}

	bool getMask(int x, int y, int z)
	{
		if (x >= 0 && x < type.size.x && y >= 0 && y < type.size.y && z >= 0 && z < type.size.z)
			if (type.mask != null)
				return type.mask[x + y * type.size.x + z * type.size.x * type.size.y];
			else
				return true;
		return false;
	}

	void getRandomLootSelection(out Item[] items, out int[] amounts, Random random)
	{
		List<Item> itemList = new List<Item>();
		List<int> amountList = new List<int>();

		float flaskChance = 0.1f;
		if (random.NextSingle() < flaskChance)
		{
			itemList.Add(Item.Get("flask"));
			amountList.Add(1);
		}

		float arrowChance = 0.08f;
		if (random.NextSingle() < arrowChance)
		{
			itemList.Add(Item.Get("arrow"));
			int amount = MathHelper.RandomInt(7, 15, random);
			amountList.Add(amount);
		}

		float goldChance = 0.25f;
		if (random.NextSingle() < goldChance || itemList.Count == 0)
		{
			itemList.Add(Item.Get("gold"));
			int amount = MathHelper.RandomInt(3, 10, random);
			amountList.Add(amount);
		}

		items = itemList.ToArray();
		amounts = amountList.ToArray();
	}

	public virtual void spawn(Level level, LevelGenerator generator, Random random)
	{
		for (int i = 0; i < doorways.Count; i++)
		{
			Doorway doorway = doorways[i];
			if (doorway.spawnDoor)
			{
				Matrix globalTransform = transform * doorway.transform;
				if (doorway.secret)
				{
					addEntity(new SecretWall(), globalTransform.translation, globalTransform.rotation);
				}
				else
				{
					DoorType doorType = (DoorType)(random.Next() % 2);
					addEntity(new WoodenDoor(doorType), globalTransform.translation, globalTransform.rotation);
				}
			}
		}

		for (int i = 0; i < enemySpawns.Count; i++)
		{
			Entity enemy = (Entity)Activator.CreateInstance(enemySpawns[i].enemyType);
			addEntity(enemy, enemySpawns[i].position, enemySpawns[i].rotation);
		}

		/*
		for (int i = 0; i < type.chestSpawns.Count; i++)
		{
			Vector3 position = transform * (type.chestSpawns[i].tile + new Vector3(0.5f, 0.0f, 0.5f));
			Quaternion rotation = transform * Quaternion.LookAt((Vector3)type.chestSpawns[i].direction);

			Item[] items = type.chestSpawns[i].items;
			int[] amounts = type.chestSpawns[i].amounts;
			if (items == null)
				getRandomLootSelection(out items, out amounts, random);

			Chest chest = new Chest(items, amounts);
			addEntity(chest, position, rotation);
		}
		*/

		type.onSpawn(this, level, generator, random);
	}

	public void addEntity(Entity entity, Vector3 position, Quaternion rotation)
	{
		entity.position = position;
		entity.rotation = rotation;
		entities.Add(entity);
		level.addEntity(entity);
	}

	public void addEntity(Entity entity, Matrix transform)
	{
		transform.decompose(out Vector3 position, out Quaternion rotation, out Vector3 _);
		addEntity(entity, position, rotation);
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

	public virtual void draw(GraphicsDevice graphics)
	{
		//Renderer.DrawModel(type.model, transform);

		for (int i = 0; i < entities.Count; i++)
		{
			entities[i].draw(graphics);
		}
	}
}
