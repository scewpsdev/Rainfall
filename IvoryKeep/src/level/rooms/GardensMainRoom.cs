using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GardensMainRoom : Entity
{
	Room room;

	public GardensMainRoom(Room room)
	{
		this.room = room;
	}

	public override void init(Level level)
	{
		level.addEntity(new Fountain(FountainEffect.None), new Vector2(room.x, room.y) + new Vector2(17, 12));
		level.addEntity(new Fountain(FountainEffect.None), new Vector2(room.x, room.y) + new Vector2(8.5f, 14));
		level.addEntity(new Fountain(FountainEffect.None), new Vector2(room.x, room.y) + new Vector2(25.5f, 14));
	}
}
