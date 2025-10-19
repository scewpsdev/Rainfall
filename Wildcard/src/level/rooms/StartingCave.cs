using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class StartingCave : Entity
{
	Room room;


	public StartingCave(Room room)
	{
		this.room = room;
	}

	public override void init(Level level)
	{
		level.addEntity(new IronDoor("iron_key"), (Vector2)room.getMarker(2) + new Vector2(0.5f, 0));
		level.addEntity(new ItemEntity(new IronKey()), (Vector2)room.getMarker(2) + new Vector2(-3, 0.5f));
	}
}
