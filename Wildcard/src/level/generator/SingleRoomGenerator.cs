using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public class SingleRoomGenerator : RoomBiomeGenerator
{
	RoomBiomeGenerator parentGenerator;

	Room room;
	Room bgRoom;


	public SingleRoomGenerator(Room room, Room bgRoom, RoomBiomeGenerator parentGenerator)
		: base(null)
	{
		this.room = room;
		this.bgRoom = bgRoom;
		this.parentGenerator = parentGenerator;
	}

	public override void generateBaseLevel()
	{
		Debug.Assert(false);
	}

	public override void spawnNPC(Vector2i tile)
	{
		Debug.Assert(false);
	}

	public void generate(uint entranceMarker, uint exitMarker, Door entranceDoor = null, Door exitDoor = null)
	{
		level.resize(room.width, room.height);

		placeRoom(room);
		level.rooms = [room];

		if (bgRoom != null)
			placeRoomBG(bgRoom);

		RoomDef def = room.set.roomDefs[room.roomDefID];
		for (int i = 0; i < def.doorDefs.Count; i++)
		{
			Vector2 position = (Vector2)def.doorDefs[i].position + def.doorDefs[i].direction;
			Vector2i size = def.doorDefs[i].direction.x != 0 ? new Vector2i(1, 3) : new Vector2i(3, 1);
			if (def.doorDefs[i].direction == Vector2i.Up)
				position += Vector2i.Up;
			LevelTransition door = new LevelTransition(null, null, size, def.doorDefs[i].direction);
			level.addEntity(door, position);
			room.doorways.Add(new Doorway(room, def.doorDefs[i]) { door = door });

			if (level.entrance == null && entranceMarker == 0)
				level.entrance = door;
			else if (level.exit == null && exitMarker == 0)
				level.exit = door;
		}

		if (entranceMarker != 0)
		{
			level.entrance = entranceDoor != null ? entranceDoor : new Door(null, null);
			Vector2 position = room.getMarker(entranceMarker) + new Vector2(0.5f, 0);
			level.addEntity(level.entrance, position);
		}
		if (exitMarker != 0)
		{
			level.exit = exitDoor != null ? exitDoor : new Door(null, null);
			Vector2 position = room.getMarker(exitMarker) + new Vector2(0.5f, 0);
			level.addEntity(level.exit, position);
		}

		level.updateLightmap(0, 0, room.width, room.height);
	}

	public override List<NPC> createNPC(Vector2i tile)
	{
		return parentGenerator.createNPC(tile);
	}

	public override List<Mob> getEnemyList()
	{
		return parentGenerator.getEnemyList();
	}

	public override Container createContainer(Item[] items)
	{
		return parentGenerator.createContainer(items);
	}

	public override ExplosiveObject createExplosiveObject()
	{
		return parentGenerator.createExplosiveObject();
	}
}
