using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Room
{
	public int roomDefID;
	public RoomDefSet set;
	public int x, y;
	public int width, height;
	public List<Doorway> doorways = new List<Doorway>();
	public bool isMainPath = false;
	public bool hasObject = false;
	public bool spawnEnemies = true;
	public Entity entity = null;

	Dictionary<uint, Vector2> markers = new Dictionary<uint, Vector2>();
	public List<Vector2i> spawnLocations = new List<Vector2i>();


	public Room()
	{
	}

	public Room(RoomDef def)
	{
		x = 0;
		y = 0;
		width = def.width;
		height = def.height;
		roomDefID = def.id;
		set = def.set;
	}

	public Room(RoomDefSet set, int id)
		: this(set.roomDefs[id])
	{
	}

	public Room(string path)
		: this(new RoomDefSet(path).roomDefs[0])
	{
	}

	public void addMarker(uint id, float x, float y)
	{
		markers.Add(id, new Vector2(x, y));
	}

	public bool tryGetMarker(uint id, out Vector2i value)
	{
		if (markers.TryGetValue(id, out Vector2 pos))
		{
			value = (Vector2i)pos;
			return true;
		}
		value = Vector2i.Zero;
		return false;
	}

	public Vector2i getMarker(uint id)
	{
		if (markers.TryGetValue(id, out Vector2 pos))
			return (Vector2i)pos;
		Debug.Assert(false);
		return Vector2i.Zero;
	}

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
				if (y > 0 && level.getTile(x, y) == null && (level.getTile(x, y - 1) == null || level.getTile(x, y - 1).isSolid && level.getTile(x, y - 1).visible) && level.getTile(x, y + 1) == null)
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

	public bool getSpawn(Level level, Random random, bool[] objectFlags, out Vector2i pos)
	{
		int offset = random.Next() % this.width;
		for (int i = 0; i < this.width; i++)
		{
			int x = this.x + (offset + i) % this.width;
			for (int y = this.y; y < this.y + this.height; y++)
			{
				if (objectFlags[x + y * level.width])
					break;
				if (y > 0 && level.getTile(x, y) == null)
				{
					pos = new Vector2i(x, y);
					return true;
				}
			}
		}
		pos = Vector2i.Zero;
		return false;
	}

	public bool getSpawn(Level level, Random random, bool[] objectFlags, Func<Vector2i, bool> isTileSuitable, out Vector2i pos)
	{
		int offset = random.Next() % this.width;
		for (int i = 0; i < this.width; i++)
		{
			int x = this.x + (offset + i) % this.width;
			for (int y = this.y; y < this.y + this.height; y++)
			{
				if (objectFlags[x + y * level.width])
					break;
				if (isTileSuitable(new Vector2i(x, y)))
				{
					pos = new Vector2i(x, y);
					return true;
				}
			}
		}
		pos = Vector2i.Zero;
		return false;
	}

	public bool containsEntity(Entity entity)
	{
		return entity.position.x >= x + 1 && entity.position.x <= x + width - 1 &&
			entity.position.y >= y + 0.5f && entity.position.y <= y + height - 0.5f;
	}
}

public class Doorway
{
	public Room room;
	public DoorDef doorDef;
	public Doorway otherDoorway;
	public Door door;

	public Doorway(Room room, DoorDef doorDef)
	{
		this.room = room;
		this.doorDef = doorDef;
	}

	public Vector2i position => doorDef.position;
	public Vector2i direction => doorDef.direction;
}
