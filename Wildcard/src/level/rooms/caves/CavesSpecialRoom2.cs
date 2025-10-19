using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public class CavesSpecialRoom2 : RoomEntity
{
	public CavesSpecialRoom2(Room room)
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

		Vector2i chestPosition = room.getMarker(0x1);
		generator.spawnChest(chestPosition);

		generator.placeEntity(new IronDoor(), room.position + room.doorways[0].position);

		level.addEntity(new TorchEntity(), position + new Vector2(2.5f, 3.5f));
		level.addEntity(new TorchEntity(), position + new Vector2(6.5f, 3.5f));

		for (int y = room.y + 1; y < room.y + room.height - 1; y++)
		{
			for (int x = room.x + 1; x < room.x + room.width - 1; x++)
			{
				if (level.getTile(x, y) == null)
				{
					//if (!generator.getObjectFlag(x, y))
					{
						float enemyChance = 0.4f;
						if (generator.random.NextSingle() < enemyChance)
						{
							generator.spawnEnemy(new Vector2i(x, y), new Bat());
						}
					}
				}
			}
		}
	}
}
