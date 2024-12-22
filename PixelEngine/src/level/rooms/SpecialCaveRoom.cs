using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SpecialCaveRoom : Entity
{
	Room room;
	LevelGenerator generator;
	Sound ambience;

	public SpecialCaveRoom(Room room, LevelGenerator generator)
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
				level.setBGTile(x, y, TileType.wood);
			}
		}

		if (room.doorways[0].otherDoorway == null)
		{
			level.setTile(room.x + room.doorways[0].position.x, room.y + room.doorways[0].position.y, TileType.stone);
			level.setTile(room.x + room.doorways[0].position.x, room.y + room.doorways[0].position.y - 1, null);
		}
		if (room.doorways[1].otherDoorway == null)
		{
			level.setTile(room.x + room.doorways[1].position.x, room.y + room.doorways[1].position.y, TileType.stone);
		}
		else
		{
			level.addEntity(new IronDoor(), new Vector2(room.x + room.doorways[1].position.x + 0.5f, room.y + room.doorways[1].position.y));
		}
		if (room.doorways[2].otherDoorway == null)
		{
			level.setTile(room.x + room.doorways[2].position.x, room.y + room.doorways[2].position.y, TileType.stone);
		}
		else
		{
			level.addEntity(new IronDoor(), new Vector2(room.x + room.doorways[2].position.x + 0.5f, room.y + room.doorways[2].position.y));
		}

		level.addEntity(new TorchEntity(), position + new Vector2(2.5f, 3.5f));
		level.addEntity(new TorchEntity(), position + new Vector2(6.5f, 3.5f));


		if (generator.random.NextSingle() < 0.8f)
			generator.spawnNPC(room.x + MathHelper.RandomInt(2, 5, generator.random), room.y + 1);
		else
			level.addEntity(new Chest(Item.CreateRandom(generator.random, DropRates.chest, generator.getRoomLootValue(room) * 2), false, true),
				new Vector2(room.x + MathHelper.RandomInt(2, 5, generator.random), room.y + 1));

		level.addEntity(new EventTrigger(new Vector2(room.width, room.height), onRoomEnter, onRoomLeave), position);
	}

	void onRoomEnter(Player player)
	{
		ambience = GameState.instance.ambience;
		GameState.instance.setAmbience(null);
	}

	void onRoomLeave(Player player)
	{
		GameState.instance.setAmbience(ambience);
	}
}
