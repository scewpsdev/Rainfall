using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public class CavesSpecialRoom3 : RoomEntity
{
	public CavesSpecialRoom3(Room room)
		: base(room)
	{
		room.spawnEnemies = false;
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

		level.setTile(room.x, room.y + 1, TileType.wood);
		level.setTile(room.x + room.width - 1, room.y + 1, TileType.wood);

		Vector2i chestPosition = room.getMarker(0x1);
		generator.spawnChest(chestPosition, 2);
		level.addEntity(new TorchEntity(), position + new Vector2(4.5f, 4.5f));
		level.addEntity(new TorchEntity(), position + new Vector2(8.5f, 4.5f));

		for (int i = 2; i < room.width - 2; i++)
		{
			if (generator.random.NextSingle() < 0.5f)
				level.addEntity(new SpikeTrap(), position + new Vector2(i + 0.5f, room.height - 1.5f));
		}
	}
}
