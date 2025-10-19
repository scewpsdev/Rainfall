using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PrisonCellRoom : RoomEntity
{
	public PrisonCellRoom(Room room)
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
				level.setBGTile(x, y, TileType.bricks);
			}
		}

		//generator.placeEntity(new IronDoor("iron_key"), room.position + room.doorways[0].position);

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
			generator.placeEntity(npc, npcPosition);
		}

		generator.placeEntity(new TorchEntity(), npcPosition + Vector2i.One, new Vector2(0, 0.5f));
	}
}
