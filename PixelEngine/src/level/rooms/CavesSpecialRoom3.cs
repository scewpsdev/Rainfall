using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CavesSpecialRoom3 : Entity
{
	Room room;
	LevelGenerator generator;

	public CavesSpecialRoom3(Room room, LevelGenerator generator)
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

		level.setTile(room.x, room.y + 1, TileType.dirt);
		level.setTile(room.x + 1, room.y + 1, TileType.dirt);
		level.setTile(room.x + room.width - 1, room.y + 1, TileType.dirt);
		level.setTile(room.x + room.width - 2, room.y + 1, TileType.dirt);

		Vector2i chestPosition = room.getMarker(0x1);
		generator.spawnChest(chestPosition.x, chestPosition.y, generator.getRoomLootValue(room) * 2, generator.random.NextSingle() < 0.2f);

		level.addEntity(new TorchEntity(), position + new Vector2(4.5f, 4.5f));
		level.addEntity(new TorchEntity(), position + new Vector2(8.5f, 4.5f));

		for (int i = 2; i < room.width - 2; i++)
		{
			if (generator.random.NextSingle() < 0.5f)
				level.addEntity(new SpikeTrap(), position + new Vector2(i + 0.5f, room.height - 1.5f));
		}
	}
}
