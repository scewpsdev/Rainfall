using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CavesSpecialRoom1 : Entity
{
	Room room;
	LevelGenerator generator;
	Sound ambience;

	public CavesSpecialRoom1(Room room, LevelGenerator generator)
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
			generator.setObjectFlag(room.x + room.doorways[1].position.x, room.y + room.doorways[1].position.y);
		}
		if (room.doorways[2].otherDoorway == null)
		{
			level.setTile(room.x + room.doorways[2].position.x, room.y + room.doorways[2].position.y, TileType.stone);
		}
		else
		{
			level.addEntity(new IronDoor(), new Vector2(room.x + room.doorways[2].position.x + 0.5f, room.y + room.doorways[2].position.y));
			generator.setObjectFlag(room.x + room.doorways[2].position.x, room.y + room.doorways[2].position.y);
		}

		level.addEntity(new TorchEntity(), position + new Vector2(2.5f, 3.5f));
		level.addEntity(new TorchEntity(), position + new Vector2(6.5f, 3.5f));

		if (generator.random.NextSingle() < 0.8f)
			generator.spawnNPC(room.x + Mathf.RandomInt(2, 5, generator.random), room.y + 1);
		else
		{
			int numChests = Mathf.RandomInt(1, 6, generator.random);
			numChests = numChests <= 3 ? 1 : numChests <= 5 ? 2 : 3;
			int xpos = Mathf.RandomInt(2, room.width - 4 - numChests, generator.random);
			for (int i = 0; i < numChests; i++)
			{
				ChestType chestType = (ChestType)Mathf.RandomInt((int)ChestType.Red, (int)ChestType.Silver, generator.random);
				Chest chest = new Chest(null, i < numChests / 2, chestType);
				chest.items = chest.createThemedItems(generator.getRoomLootValue(room) * 2, generator.getDroprates(), generator.random);
				level.addEntity(chest, new Vector2(room.x + xpos + i * 1.5f + 0.5f, room.y + 1));
				generator.setObjectFlag(room.x + xpos + i, room.y + 1);
			}
		}

		level.addEntity(new EventTrigger(new Vector2(room.width, room.height), onRoomEnter, onRoomLeave), position);
	}

	void onRoomEnter(Player player)
	{
		ambience = AudioManager.ambience;
		AudioManager.SetAmbience(null);
	}

	void onRoomLeave(Player player)
	{
		AudioManager.SetAmbience(ambience);
	}
}
