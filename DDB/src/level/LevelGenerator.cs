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
	static Room CreateRoom(RoomType type, Level level)
	{
		Room room = new Room(type, level);

		return room;
	}

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
		Room room = CreateRoom(RoomType.StartingRoom, level);

		room.addEntity(new Dummy(), new Vector3(0.0f, 0.0f, 8.0f), Quaternion.FromAxisAngle(Vector3.Up, MathF.PI));
		room.addEntity(new SkeletonEnemy(), new Vector3(3.0f, 1.0f, -7.0f), Quaternion.Identity);
		room.addEntity(new SkeletonEnemy(), new Vector3(0.0f, 1.0f, -7.0f), Quaternion.Identity);
		room.addEntity(new SkeletonEnemy(), new Vector3(-3.0f, 1.0f, -7.0f), Quaternion.Identity);

		room.addEntity(new WallTorch(), new Vector3(-6.0f, 2.1f, -10.0f), Quaternion.Identity);
		room.addEntity(new WallTorch(), new Vector3(6.0f, 2.1f, -10.0f), Quaternion.Identity);
		room.addEntity(new WallTorch(), new Vector3(-5.0f, 0.0f, -20.0f), Quaternion.FromAxisAngle(Vector3.Up, MathF.PI * 0.5f));
		room.addEntity(new WallTorch(), new Vector3(5.0f, 0.0f, -20.0f), Quaternion.FromAxisAngle(Vector3.Up, -MathF.PI * 0.5f));
		room.addEntity(new WallTorch(), new Vector3(-2.0f, -2.0f, -28.0f), Quaternion.Identity);
		room.addEntity(new WallTorch(), new Vector3(2.0f, -2.0f, -28.0f), Quaternion.Identity);
		room.addEntity(new Chest(new Item[] { Item.Get("quemick"), Item.Get("firebomb") }, new int[] { 2, 5 }), new Vector3(-2.5f, 0.0f, 0.0f), Quaternion.Identity);
		//room.addEntity(new Door(DoorType.Normal), new Vector3(-2.0f, 0.0f, -3.0f), Quaternion.Identity);

		room.addEntity(new ItemPickup(Item.Get("shortsword")), new Vector3(0.5f, 1.0f, 0.0f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.PiOver2));
		room.addEntity(new ItemPickup(Item.Get("zweihander")), new Vector3(-0.5f, 1.0f, 0.0f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.PiOver2));
		room.addEntity(new ItemPickup(Item.Get("longsword")), new Vector3(0.0f, 1.0f, -1.0f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.PiOver2));
		room.addEntity(new ItemPickup(Item.Get("longbow")), new Vector3(0.0f, 1.0f, -3.0f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.PiOver2));
		room.addEntity(new ItemPickup(Item.Get("arrow"), 5), new Vector3(-0.2f, 1.0f, -3.0f), Quaternion.Identity);
		room.addEntity(new ItemPickup(Item.Get("oak_staff"), 1, null, new Item[] { Item.Get("magic_arrow") }), new Vector3(-0.5f, 1.0f, -2.0f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.PiOver2));
		room.addEntity(new ItemPickup(Item.Get("oak_staff"), 1, null, new Item[] { Item.Get("homing_orbs") }), new Vector3(0.0f, 1.0f, -2.0f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.PiOver2));
		room.addEntity(new ItemPickup(Item.Get("oak_staff"), 1, null, new Item[] { Item.Get("magic_orb") }), new Vector3(0.5f, 1.0f, -2.0f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.PiOver2));
		room.addEntity(new ItemPickup(Item.Get("wooden_round_shield")), new Vector3(0.0f, 1.0f, 0.0f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.PiOver2));
		//room.addEntity(new ItemPickup(Item.Get("dagger")), new Vector3(0.0f, 1.0f, -0.5f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.PiOver2));

		//room.addEntity(new Door(DoorType.Windowed), new Vector3(0.0f, 0.0f, -10.0f), Quaternion.Identity);
		//room.addEntity(new SkeletonEnemy(), new Vector3(0.0f, 0.0f, -10.4f), Quaternion.Identity);

		//room.doorways.Add(new Doorway(room, new Vector3(0.0f, -7.5f, -35.0f), Quaternion.Identity));

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
				Room room = CreateRoom(type, level);

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
