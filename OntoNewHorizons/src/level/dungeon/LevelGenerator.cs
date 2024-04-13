using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Rainfall;

internal class LevelGenerator
{
	static bool PlaceRoom(Room room, Doorway doorway, Level level, TileMap tilemap)
	{
		Doorway roomEntrance = room.doorways[0];
		Matrix roomTransform = doorway.room.transform * doorway.transform * Matrix.CreateRotation(Vector3.Up, MathF.PI) * roomEntrance.transform.inverted;
		room.setTransform(roomTransform);
		if (!room.overlaps(tilemap))
		{
			room.spawn(tilemap);

			roomEntrance.connectedRoom = doorway.room;
			doorway.connectedRoom = room;
			return true;
		}

		return false;
	}

	static Room CreateStartingRoom(Level level, TileMap tilemap)
	{
		Room room = new Testmap2(RoomType.StartingRoom, level);

		room.setTransform(Matrix.Identity);
		room.spawn(tilemap);

		return room;
	}

	static void Shuffle(List<Doorway> list, Random random)
	{
		int n = list.Count;
		while (n > 1)
		{
			n--;
			int k = random.Next(n + 1);
			Doorway value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}

	static Room CreateRandomRoom(Level level, TileMap tilemap, Random random)
	{
		List<Doorway> openDoorways = new List<Doorway>();

		for (int i = 0; i < level.rooms.Count; i++)
		{
			for (int j = 0; j < level.rooms[i].doorways.Count; j++)
			{
				if (level.rooms[i].doorways[j].connectedRoom == null)
					openDoorways.Add(level.rooms[i].doorways[j]);
			}
		}

		if (openDoorways.Count >= 1)
		{
			Shuffle(openDoorways, random);
			Doorway doorway = openDoorways[0];

			SectorType nextSectorType = doorway.room.type.sectorType == SectorType.Room ? SectorType.Corridor : SectorType.Room;
			RoomType type = RoomType.SelectRoom(nextSectorType, random);

			if (type != null)
			{
				Room room = new Room(type, level);

				//room.doorways.Add(new Doorway(room, new Vector3(0.0f, 0.0f, 12.0f), Quaternion.FromAxisAngle(Vector3.Up, MathF.PI)));

				if (PlaceRoom(room, doorway, level, tilemap))
				{
					return room;
				}
				else
				{
					Console.WriteLine("Failed to place room " + room.type.name + " at " + doorway.transform.translation);
				}
			}
			else
			{
				Console.WriteLine("Failed to find suitable room of type " + nextSectorType + " at " + doorway.transform.translation);
			}
		}

		return null;
	}

	static void SpawnEnemies(Room room, Random random)
	{
		int numEnemies = MathHelper.RandomInt(0, 3, random);
		for (int i = 0; i < numEnemies; i++)
		{
			//SkeletonEnemy enemy = new SkeletonEnemy();
			//room.addEntity(enemy, room.transform.translation);
		}
	}

	public static void GenerateLevel(Level level, int floor)
	{
		Random random = new Random(123456 * (floor + 1));

		TileMap tilemap = new TileMap();

		level.rooms.Add(CreateStartingRoom(level, tilemap));

		for (int i = 0; i < 10; i++)
		{
			Room room = CreateRandomRoom(level, tilemap, random);
			if (room != null)
			{
				SpawnEnemies(room, random);
				level.rooms.Add(room);
			}
		}
	}
}
