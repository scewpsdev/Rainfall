using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CavesSpecialRoom2 : Entity
{
	Room room;
	LevelGenerator generator;

	public CavesSpecialRoom2(Room room, LevelGenerator generator)
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
		generator.spawnChest(chestPosition.x, chestPosition.y, generator.getRoomLootValue(room));

		level.addEntity(new IronDoor(), new Vector2(room.x + room.doorways[0].position.x + 0.5f, room.y + room.doorways[0].position.y));
		generator.setObjectFlag(room.x + room.doorways[0].position.x, room.y + room.doorways[0].position.y);

		level.addEntity(new TorchEntity(), position + new Vector2(2.5f, 3.5f));
		level.addEntity(new TorchEntity(), position + new Vector2(6.5f, 3.5f));

		for (int y = room.y + 1; y < room.y + room.height - 1; y++)
		{
			for (int x = room.x + 1; x < room.x + room.width - 1; x++)
			{
				if (level.getTile(x, y) == null)
				{
					if (!generator.getObjectFlag(x, y))
					{
						float enemyChance = 0.4f;
						if (generator.random.NextSingle() < enemyChance)
						{
							generator.spawnEnemy(x, y, new Bat());
						}
					}
				}
			}
		}
	}
}
