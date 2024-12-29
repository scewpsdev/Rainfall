using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PrisonCellRoom : Entity
{
	Room room;
	LevelGenerator generator;

	public PrisonCellRoom(Room room, LevelGenerator generator)
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
				level.setBGTile(x, y, TileType.bricks);
			}
		}

		level.addEntity(new IronDoor("iron_key"), new Vector2(room.x, room.y) + room.doorways[0].position + new Vector2(0.5f, 0));

		Vector2i npcPosition = room.getMarker(0x1);

		List<NPC> npcs = new List<NPC>();
		if (!GameState.instance.save.isStartingClassUnlocked(StartingClass.barbarian))
			npcs.Add(new Barbarian());
		if (!GameState.instance.save.isStartingClassUnlocked(StartingClass.knight))
			npcs.Add(new Knight());
		if (!GameState.instance.save.isStartingClassUnlocked(StartingClass.hunter))
			npcs.Add(new Hunter());
		if (!GameState.instance.save.isStartingClassUnlocked(StartingClass.thief))
			npcs.Add(new Thief());

		if (npcs.Count > 0)
		{
			NPC npc = npcs[generator.random.Next() % npcs.Count];
			level.addEntity(npc, npcPosition + new Vector2(0.5f, 0));
		}

		level.addEntity(new TorchEntity(), npcPosition + new Vector2(1.5f, 1.5f));
	}
}
