using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Doorway
{
	public Room room;
	public int doorwayId;
	public Doorway otherDoorway;

	public Doorway(Room room, int doorwayId)
	{
		this.room = room;
		this.doorwayId = doorwayId;
	}

	public DoorwayDefinition definition => room.definition.doorwayDefinitions[doorwayId];
}

public class Room : Entity
{
	public int definitionId;
	public List<Doorway> doorways = new List<Doorway>();


	public Vector3 center => position + model.boundingBox.center * new Vector3(1, 0, 1);

	public RoomDefinition definition => DungeonGenerator.roomDefinitions[definitionId];
}
