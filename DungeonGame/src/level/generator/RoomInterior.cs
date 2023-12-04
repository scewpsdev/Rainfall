using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract class RoomInterior
{
	public static readonly List<RoomInterior> interiors = new List<RoomInterior>();


	static RoomInterior()
	{
		interiors.Add(new EmptyRoom());
		//interiors.Add(new LibraryRoom());
		//interiors.Add(new FountainRoom());
		//interiors.Add(new PillarRoom());
	}

	public static RoomInterior GetFitting(Room room, Random random)
	{
		List<RoomInterior> fittingInteriors = new List<RoomInterior>();
		foreach (RoomInterior interior in interiors)
		{
			if (interior.doesFit(room.type))
				fittingInteriors.Add(interior);
		}
		Debug.Assert(fittingInteriors.Count > 0);
		RoomInterior selectedInterior = fittingInteriors[random.Next() % fittingInteriors.Count];
		return selectedInterior;
	}


	public int ceilingHeight = 2;


	public abstract bool doesFit(RoomType type);

	public abstract void initialize(Room room, Level level, Random random);

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
}

public class EmptyRoom : RoomInterior
{
	public override bool doesFit(RoomType type)
	{
		return true;
	}

	public override void initialize(Room room, Level level, Random random)
	{
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
	}
}

public class LibraryRoom_ : RoomInterior
{
	public override bool doesFit(RoomType type)
	{
		return true;
	}

	public override void initialize(Room room, Level level, Random random)
	{
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

public class FountainRoom_ : RoomInterior
{
	public override bool doesFit(RoomType type)
	{
		if (type.size.x * type.size.z >= 50 && type.size.x == type.size.z)
			return true;
		return false;
	}

	public override void initialize(Room room, Level level, Random random)
	{
		Vector3 roomCenter = (room.gridPosition + room.gridSize * new Vector3i(1, 0, 1) * 0.5f) * LevelGenerator.TILE_SIZE;
		level.addEntity(new Fountain(), roomCenter, Quaternion.Identity);
	}
}

public class PillarRoom_ : RoomInterior
{
	public PillarRoom_()
	{
		ceilingHeight = 3;
	}

	public override bool doesFit(RoomType type)
	{
		if (type.size.x >= 15 && type.size.z >= 15)
			return true;
		return false;
	}

	public override void initialize(Room room, Level level, Random random)
	{
		int gap = 3;
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
