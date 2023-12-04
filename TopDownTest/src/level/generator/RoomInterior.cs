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
		interiors.Add(new FountainRoom());
		interiors.Add(new PillarRoom());
	}


	public int ceilingHeight = 2;


	public abstract bool doesFit(RoomInstance room);

	public abstract void initialize(RoomInstance room, Level level);
}

public class EmptyRoom : RoomInterior
{
	public override bool doesFit(RoomInstance room)
	{
		return true;
	}

	public override void initialize(RoomInstance room, Level level)
	{
	}
}

public class FountainRoom : RoomInterior
{
	public override bool doesFit(RoomInstance room)
	{
		if (room.size.x * room.size.y >= 20)
			return true;
		return false;
	}

	public override void initialize(RoomInstance room, Level level)
	{
		Vector3 position = new Vector3(room.pos.x + room.size.x * 0.5f, room.height, room.pos.y + room.size.y * 0.5f) * LevelGenerator.TILE_SIZE;
		//level.addEntity(new Fountain(), position, Quaternion.Identity);
	}
}

public class PillarRoom : RoomInterior
{
	public PillarRoom()
	{
		ceilingHeight = 3;
	}

	public override bool doesFit(RoomInstance room)
	{
		if (room.size.x >= 6 && room.size.y >= 6)
			return true;
		return false;
	}

	public override void initialize(RoomInstance room, Level level)
	{
		int gap = 3;
		int numPillarsX = room.size.x / gap;
		int numPillarsZ = room.size.y / gap;
		for (int z = 0; z < numPillarsZ; z++)
		{
			for (int x = 0; x < numPillarsX; x++)
			{
				Vector3 position = room.worldPosition + new Vector3(room.size.x * 0.5f, 0, room.size.y * 0.5f) * LevelGenerator.TILE_SIZE
					+ new Vector3(
					(x - 0.5f * (numPillarsX - 1)) * gap * LevelGenerator.TILE_SIZE,
					0.0f,
					(z - 0.5f * (numPillarsZ - 1)) * gap * LevelGenerator.TILE_SIZE
				);
				//level.addEntity(new Pillar(), position, Quaternion.Identity);
			}
		}
	}
}
