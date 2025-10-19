using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract class RoomEntity : Entity
{
	protected Room room;
	protected Sound ambience;


	public RoomEntity(Room room)
	{
		this.room = room;
	}

	public abstract void place(RoomBiomeGenerator generator);
}
