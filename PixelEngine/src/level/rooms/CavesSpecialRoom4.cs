using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CavesSpecialRoom4 : Entity
{
	Room room;
	LevelGenerator generator;

	public CavesSpecialRoom4(Room room, LevelGenerator generator)
	{
		this.room = room;
		this.generator = generator;
	}

	public override void init(Level level)
	{
		for (int y = room.y + 1; y < room.y + room.height - 1; y++)
		{
			for (int x = room.x + 1; x < room.x + room.width - 1; x++)
			{
				level.setBGTile(x, y, TileType.stone);
			}
		}

		Vector2i chestPosition = room.getMarker(0x1);
		generator.spawnChest(chestPosition.x, chestPosition.y, generator.getRoomLootValue(room) * 2);

		level.addEntity(new TorchEntity(), position + new Vector2(4.5f, 4.5f));
		level.addEntity(new TorchEntity(), position + new Vector2(8.5f, 4.5f));

		for (int i = 0; i < 6; i++)
		{
			int x = room.x + i % 2 == 0 ? 1 : room.width - 2;
			int y = room.y + 3 + i * 2;
			if (level.getTile(x, y) == null && !generator.getObjectFlag(x, y))
				level.addEntity(new TorchEntity(), new Vector2(x + 0.5f, y + 0.5f));
		}
	}
}
