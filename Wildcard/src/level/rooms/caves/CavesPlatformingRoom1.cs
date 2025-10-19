using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CavesPlatformingRoom1 : RoomEntity
{
	public CavesPlatformingRoom1(Room room)
		: base(room)
	{
		this.room = room;
	}

	public override void place(RoomBiomeGenerator generator)
	{
		for (int y = room.y + 1; y < room.y + room.height - 1; y++)
		{
			for (int x = room.x + 1; x < room.x + room.width - 1; x++)
			{
				level.setBGTile(x, y, TileType.stone);
			}
		}

		Vector2i chestPosition = room.getMarker(0x1);
		generator.spawnChest(chestPosition, 5);

		generator.placeEntity(new TorchEntity(), room.position + new Vector2i(4, 4), new Vector2(0, 0.5f));
		generator.placeEntity(new TorchEntity(), room.position + new Vector2i(8, 4), new Vector2(0, 0.5f));

		for (int i = 0; i < 6; i++)
		{
			int x = room.x + i % 2 == 0 ? 1 : room.width - 2;
			int y = room.y + 3 + i * 2;
			if (level.getTile(x, y) == null && !generator.getObjectFlag(x, y))
				generator.placeEntity(new TorchEntity(), new Vector2i(x, y), new Vector2(0, 0.5f));
		}

		level.ambientLight = Vector3.Zero;
	}
}
