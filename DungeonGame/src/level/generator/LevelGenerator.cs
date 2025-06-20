﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Rainfall;

public class LevelGenerator
{
	//public const float TILE_SIZE = 1.0f;


	Level level;
	TileMap tilemap;

	int maxRooms = 12;

	int seed;
	Random random;

	List<Room> rooms = new List<Room>();
	Room startingRoom, finalRoom, mainRoom;

	public List<ItemContainer> itemContainers = new List<ItemContainer>();

	List<Item> usedKeyTypes = new List<Item>();


	public LevelGenerator()
	{
		tilemap = new TileMap();
	}

	public void reset(int seed, Level level)
	{
		this.level = level;
		this.tilemap = level.tilemap;

		this.seed = seed;
		random = new Random(seed);

		rooms.Clear();
		startingRoom = finalRoom = mainRoom = null;

		itemContainers.Clear();

		usedKeyTypes.Clear();
	}

	void propagateRooms(SectorType type)
	{
		List<Doorway> openDoorways = new List<Doorway>();
		foreach (Room room in rooms)
		{
			if (type == SectorType.None || room.type.sectorType == type)
			{
				foreach (Doorway doorway in room.doorways)
				{
					if (doorway.connectedDoorway == null)
						openDoorways.Add(doorway);
				}
			}
		}

		//MathHelper.ShuffleList(openDoorways, random);
		openDoorways.Sort((doorway1, doorway2) =>
		{
			if (doorway1.room.type.sectorType == SectorType.Corridor && doorway2.room.type.sectorType != SectorType.Corridor)
				return -1;
			if (doorway2.room.type.sectorType == SectorType.Corridor && doorway1.room.type.sectorType != SectorType.Corridor)
				return 1;
			return 0;
		});

		foreach (Doorway openDoorway in openDoorways)
		{
			if (rooms.Count >= maxRooms)
				break;
			SectorType nextSectorType = openDoorway.room.type.getNextSectorType(openDoorway, random);
			RoomType connectedType = RoomType.GetRandom(nextSectorType, random);
			Matrix transform = openDoorway.room.transform * openDoorway.transform * Matrix.CreateRotation(Vector3.Up, MathF.PI);
			int entranceDoorwayIdx = random.Next() % connectedType.doorwayInfo.Count; // connectedType.getEntranceDoorwayIdx(openDoorway.room.type.sectorType, random);
			transform = transform * connectedType.getDoorwayTransform(entranceDoorwayIdx).inverted;
			if (!tilemap.overlapsRoom(connectedType, transform))
			{
				Room room = placeRoom(connectedType, transform);
				Doorway otherDoorway = room.doorways[entranceDoorwayIdx];
				openDoorway.connectedDoorway = otherDoorway;
				otherDoorway.connectedDoorway = openDoorway;
			}
		}
	}

	void placeFinalRoom()
	{
		List<Doorway> openDoorways = new List<Doorway>();
		foreach (Room room in rooms)
		{
			foreach (Doorway doorway in room.doorways)
			{
				if (doorway.connectedDoorway == null)
					openDoorways.Add(doorway);
			}
		}

		openDoorways.Sort((Doorway doorway1, Doorway doorway2) =>
		{
			int distance1 = manhattanDistance(doorway1.globalPosition, startingRoom.gridPosition + startingRoom.gridSize / 2);
			int distance2 = manhattanDistance(doorway2.globalPosition, startingRoom.gridPosition + startingRoom.gridSize / 2);
			return distance1 < distance2 ? 1 : distance2 < distance1 ? -1 : 0;
		});
		openDoorways.Reverse();


		RoomType roomType = RoomType.FinalRoom;
		Doorway selectedDoorway = null;
		Matrix transform = Matrix.Identity;

		foreach (Doorway openDoorway in openDoorways)
		{
			transform = openDoorway.room.transform * openDoorway.transform * Matrix.CreateRotation(Vector3.Up, MathF.PI);
			transform = transform * roomType.getDoorwayTransform(0).inverted;
			if (!tilemap.overlapsRoom(roomType, transform))
			{
				selectedDoorway = openDoorway;
				break;
			}
		}

		if (selectedDoorway != null)
		{
			finalRoom = placeRoom(roomType, transform);
			Doorway otherDoorway = finalRoom.doorways[0];
			selectedDoorway.connectedDoorway = otherDoorway;
			otherDoorway.connectedDoorway = selectedDoorway;
		}
		else
		{
			Debug.Assert(false);
		}
	}

	Room getRoomByID(int id)
	{
		foreach (Room room in rooms)
		{
			if (room.id == id)
				return room;
		}
		return null;
	}

	Room findRoomAtPosition(Vector3i position)
	{
		int roomID = tilemap.getRoomID(position);
		Room room = getRoomByID(roomID);
		return room;
	}

	Vector3i localToGlobal(Vector3i position, Matrix roomTransform)
	{
		Vector3 tileCenter = position + new Vector3(0.5f, 0.0f, 0.5f);
		Vector4 local = roomTransform * new Vector4(tileCenter, 1.0f);
		return (Vector3i)Vector3.Floor(local.xyz);
	}

	Vector3i globalToLocal(Vector3i position, Matrix roomTransform)
	{
		Vector3 tileCenter = position + new Vector3(0.5f, 0.0f, 0.5f);
		Vector4 local = roomTransform.inverted * new Vector4(tileCenter, 1.0f);
		return (Vector3i)Vector3.Floor(local.xyz);
	}

	int manhattanDistance(Vector3i a, Vector3i b)
	{
		int dx = Math.Abs(a.x - b.x);
		int dy = Math.Abs(a.y - b.y);
		int dz = Math.Abs(a.z - b.z);
		return dx + dy + dz;
	}

	int checkRoomForDoorway(Room room, Doorway doorway2, Doorway lastDoorway, List<Room> checkedRooms)
	{
		if (lastDoorway != null)
		{
			foreach (Doorway doorway in room.doorways)
			{
				if (doorway == doorway2)
					return 1;
			}
			checkedRooms.Add(room);
		}

		int shortestFound = int.MaxValue;
		foreach (Doorway doorway in room.doorways)
		{
			if (doorway != lastDoorway && doorway.connectedDoorway != null)
			{
				if (!checkedRooms.Contains(doorway.connectedDoorway.room))
				{
					int found = checkRoomForDoorway(doorway.connectedDoorway.room, doorway2, doorway, checkedRooms);
					if (found > 0)
					{
						if (found < shortestFound)
							shortestFound = found;
					}
				}
			}
		}
		if (shortestFound != int.MaxValue)
			return shortestFound + 1;

		return 0;
	}

	int getNumDoorsBetween(Room room1, Doorway doorway2)
	{
		List<Room> checkedRooms = new List<Room>();
		int chainLength = checkRoomForDoorway(room1, doorway2, null, checkedRooms);
		if (chainLength != 0)
			return chainLength;

		//Debug.Assert(false);
		return -1;
	}

	int checkDoorwayForRoom(Doorway doorway2, Room room, Room lastRoom, List<Room> checkedRooms, bool lockedDoorsBlocking)
	{
		if (lastRoom != null)
		{
			if (doorway2.room == room)
				return 1;
			checkedRooms.Add(doorway2.room);
		}

		int shortestFound = int.MaxValue;
		foreach (Doorway doorway in doorway2.room.doorways)
		{
			if (doorway != doorway2 && doorway.connectedDoorway != null && (doorway.lockedSide == 0 && doorway.requiredKey == null || !lockedDoorsBlocking))
			{
				if (!checkedRooms.Contains(doorway.connectedDoorway.room))
				{
					int found = checkDoorwayForRoom(doorway.connectedDoorway, room, doorway2.room, checkedRooms, lockedDoorsBlocking);
					if (found > 0)
					{
						if (found < shortestFound)
							shortestFound = found;
					}
				}
			}
		}
		if (shortestFound != int.MaxValue)
			return shortestFound + 1;

		return 0;
	}

	int getNumDoorsBetween(Doorway doorway, Room room, bool lockedDoorsBlocking = false)
	{
		List<Room> checkedRooms = new List<Room>();
		int chainLength = checkDoorwayForRoom(doorway, room, null, checkedRooms, lockedDoorsBlocking);
		if (chainLength != 0)
			return chainLength;

		//Debug.Assert(false);
		return -1;
	}

	public bool isDoorwayConnectedToRoom(Doorway doorway, Room room, bool lockedDoorsBlocking = false)
	{
		return getNumDoorsBetween(doorway, room, lockedDoorsBlocking) != -1;
	}

	void connectDoorways(Doorway doorway1, Doorway doorway2)
	{
		bool[] walkable = new bool[tilemap.mapSize.x * tilemap.mapSize.y * tilemap.mapSize.z];
		Array.Fill(walkable, true);
		foreach (Room room in rooms)
		{
			for (int z = room.gridPosition.z - 2; z < room.gridPosition.z + room.gridSize.z + 2; z++)
			{
				for (int x = room.gridPosition.x - 2; x < room.gridPosition.x + room.gridSize.x + 2; x++)
				{
					for (int y = room.gridPosition.y - 2; y < room.gridPosition.y + room.gridSize.y + 2; y++)
					{
						Vector3i p = new Vector3i(x, y, z);

						bool isInsideRoom = p >= room.gridPosition && p < room.gridPosition + room.gridSize;

						bool isDoorway = false;
						foreach (Doorway doorway in room.doorways)
						{
							if (p == doorway.globalPosition ||
								p == doorway.globalPosition + doorway.globalDirection)
							//p == doorway.globalPosition + new Vector3i(doorway.globalDirection.z, 0, doorway.globalDirection.x) ||
							//p == doorway.globalPosition - new Vector3i(doorway.globalDirection.z, 0, doorway.globalDirection.x))
							{
								isDoorway = true;
								break;
							}
						}

						if (room.type.sectorType == SectorType.Room)
						{
							if (!isInsideRoom)
							{
								if (!isDoorway)
								{
									int xx = x - tilemap.mapPosition.x;
									int yy = y - tilemap.mapPosition.y;
									int zz = z - tilemap.mapPosition.z;
									walkable[xx + yy * tilemap.mapSize.x + zz * tilemap.mapSize.x * tilemap.mapSize.y] = false;
								}
							}
						}
					}
				}
			}

			foreach (Doorway doorway in room.doorways)
			{
				for (int z = doorway.globalPosition.z - 1; z <= doorway.globalPosition.z + 1; z++)
				{
					for (int x = doorway.globalPosition.x - 1; x <= doorway.globalPosition.x + 1; x++)
					{
						int xx = x - tilemap.mapPosition.x;
						int yy = doorway.globalPosition.y - 1 - tilemap.mapPosition.y;
						int zz = z - tilemap.mapPosition.z;
						walkable[xx + yy * tilemap.mapSize.x + zz * tilemap.mapSize.x * tilemap.mapSize.y] = false;
					}
				}
			}
		}
		{
			Vector3i behindDoorway = doorway1.globalPosition - doorway1.globalDirection - tilemap.mapPosition;
			walkable[behindDoorway.x + behindDoorway.y * tilemap.mapSize.x + behindDoorway.z * tilemap.mapSize.x * tilemap.mapSize.y] = false;
		}
		{
			Vector3i behindDoorway = doorway2.globalPosition - doorway2.globalDirection - tilemap.mapPosition;
			walkable[behindDoorway.x + behindDoorway.y * tilemap.mapSize.x + behindDoorway.z * tilemap.mapSize.x * tilemap.mapSize.y] = false;
		}

		int[] costs = new int[tilemap.mapSize.x * tilemap.mapSize.y * tilemap.mapSize.z];
		Array.Fill(costs, 1);
		for (int z = 0; z < tilemap.mapSize.z; z++)
		{
			for (int y = 0; y < tilemap.mapSize.y; y++)
			{
				for (int x = 0; x < tilemap.mapSize.x; x++)
				{
					if (tilemap.getRoomID(tilemap.mapPosition + new Vector3i(x, y, z)) != 0)
						costs[x + y * tilemap.mapSize.x + z * tilemap.mapSize.x * tilemap.mapSize.y] = 2;
				}
			}
		}

		Vector3i start = doorway1.globalPosition;
		Vector3i end = doorway2.globalPosition;

		List<Vector3i> path = AStar3D.Run(start - tilemap.mapPosition, end - tilemap.mapPosition, tilemap.mapSize, walkable, costs);
		if (path != null)
		{
			for (int j = 0; j < path.Count; j++)
				path[j] = path[j] + tilemap.mapPosition;
			Vector3i startDirection = -doorway1.globalDirection;
			Vector3i endDirection = -doorway2.globalDirection;
			RoomType type = RoomType.GetAStarCorridor(path, startDirection, endDirection, tilemap, out Matrix transform);
			Room newRoom = placeRoom(type, transform);

			bool directConnection = doorway1.room.type.sectorType == SectorType.Corridor || doorway2.room.type.sectorType == SectorType.Corridor;
			directConnection = false;
			if (directConnection)
			{
				newRoom.doorways[0].connectedDoorway = doorway1;
				newRoom.doorways[1].connectedDoorway = doorway2;
				doorway1.connectedDoorway = doorway2;
				doorway2.connectedDoorway = doorway1;
			}
			else
			{
				newRoom.doorways[0].connectedDoorway = doorway1;
				newRoom.doorways[1].connectedDoorway = doorway2;
				doorway1.connectedDoorway = newRoom.doorways[0];
				doorway2.connectedDoorway = newRoom.doorways[1];
			}
		}
	}

	void interconnectRooms(int count)
	{
		List<Doorway> openDoorways = new List<Doorway>();
		foreach (Room room in rooms)
		{
			foreach (Doorway doorway in room.doorways)
			{
				if (doorway.connectedDoorway == null)
					openDoorways.Add(doorway);
			}
		}

		List<Tuple<Doorway, Doorway>> openDoorwayPairs = new List<Tuple<Doorway, Doorway>>();
		for (int i = 0; i < openDoorways.Count - 1; i++)
		{
			for (int j = i + 1; j < openDoorways.Count; j++)
			{
				if (openDoorways[i].room != openDoorways[j].room &&
					(openDoorways[i].room.type.sectorType == SectorType.Corridor || openDoorways[j].room.type.sectorType == SectorType.Corridor))
				{
					int numDoorsBetween = getNumDoorsBetween(openDoorways[i].room, openDoorways[j]);
					if (numDoorsBetween == -1 || numDoorsBetween > 6)
						openDoorwayPairs.Add(new Tuple<Doorway, Doorway>(openDoorways[i], openDoorways[j]));
					//if (numDoorsBetween > 6)
					//	openDoorwayPairs.Add(new Tuple<Doorway, Doorway>(openDoorways[i], openDoorways[j]));
				}
			}
		}

		openDoorwayPairs.Sort((Tuple<Doorway, Doorway> pair1, Tuple<Doorway, Doorway> pair2) =>
		{
			int distance1 = manhattanDistance(pair1.Item1.globalPosition, pair1.Item2.globalPosition);
			int distance2 = manhattanDistance(pair2.Item1.globalPosition, pair2.Item2.globalPosition);
			return distance1 > distance2 ? 1 : distance1 < distance2 ? -1 : 0;
		});

		tilemap.resize(tilemap.mapPosition.x - 3, tilemap.mapPosition.y - 3, tilemap.mapPosition.z - 3, tilemap.mapPosition.x + tilemap.mapSize.x + 3, tilemap.mapPosition.y + tilemap.mapSize.y + 3, tilemap.mapPosition.z + tilemap.mapSize.z + 3);

		for (int i = 0; i < Math.Min(count, openDoorwayPairs.Count); i++)
		{
			Doorway doorway1 = openDoorwayPairs[i].Item1;
			Doorway doorway2 = openDoorwayPairs[i].Item2;

			if (doorway1.connectedDoorway != null || doorway2.connectedDoorway != null)
			{
				count++;
				continue;
			}

			connectDoorways(doorway1, doorway2);
		}
	}

	void collectRoomConnections(Room room, List<Room> rooms, Doorway lastDoorway)
	{
		foreach (Doorway doorway in room.doorways)
		{
			if (doorway.connectedDoorway != null)
			{
				Room connectedRoom = doorway.connectedDoorway.room;
				if (!rooms.Contains(connectedRoom))
				{
					rooms.Add(connectedRoom);
					collectRoomConnections(connectedRoom, rooms, doorway.connectedDoorway);
				}
			}
		}
	}

	List<Doorway> getEmptyDoorwaysConnectedToRoom(Room room)
	{
		List<Room> connectedRooms = new List<Room>();
		connectedRooms.Add(room);
		collectRoomConnections(room, connectedRooms, null);

		List<Doorway> emptyDoorways = new List<Doorway>();
		foreach (Room connectedRoom in connectedRooms)
		{
			foreach (Doorway doorway in connectedRoom.doorways)
			{
				if (doorway.connectedDoorway == null)
					emptyDoorways.Add(doorway);
			}
		}
		return emptyDoorways;
	}

	void connectRoomsIfNot(Room room1, Room room2)
	{
		int numDoorsBetween = getNumDoorsBetween(room1, room2.doorways[0]);
		if (numDoorsBetween == -1)
		{
			List<Doorway> doorways1 = getEmptyDoorwaysConnectedToRoom(room1);
			List<Doorway> doorways2 = getEmptyDoorwaysConnectedToRoom(room2);
			List<Tuple<Doorway, Doorway>> doorwayPairs = new List<Tuple<Doorway, Doorway>>();
			for (int i = 0; i < doorways1.Count; i++)
			{
				for (int j = 0; j < doorways2.Count; j++)
				{
					doorwayPairs.Add(new Tuple<Doorway, Doorway>(doorways1[i], doorways2[j]));
				}
			}

			doorwayPairs.Sort((Tuple<Doorway, Doorway> pair1, Tuple<Doorway, Doorway> pair2) =>
			{
				int distance1 = manhattanDistance(pair1.Item1.globalPosition, pair1.Item2.globalPosition);
				int distance2 = manhattanDistance(pair2.Item1.globalPosition, pair2.Item2.globalPosition);
				return distance1 > distance2 ? 1 : distance1 < distance2 ? -1 : 0;
			});

			Doorway doorway1 = doorwayPairs[0].Item1;
			Doorway doorway2 = doorwayPairs[0].Item2;
			connectDoorways(doorway1, doorway2);
		}
	}

	void connectDoorwayToRandomRoom(Doorway doorway)
	{
		List<Doorway> doorways = new List<Doorway>();
		foreach (Room room in rooms)
			doorways.AddRange(getEmptyDoorwaysConnectedToRoom(room));

		List<Tuple<Doorway, Doorway>> doorwayPairs = new List<Tuple<Doorway, Doorway>>();
		for (int i = 0; i < doorways.Count; i++)
		{
			if (doorways[i] != doorway)
				doorwayPairs.Add(new Tuple<Doorway, Doorway>(doorways[i], doorway));
		}

		doorwayPairs.Sort((Tuple<Doorway, Doorway> pair1, Tuple<Doorway, Doorway> pair2) =>
		{
			int distance1 = manhattanDistance(pair1.Item1.globalPosition, pair1.Item2.globalPosition);
			int distance2 = manhattanDistance(pair2.Item1.globalPosition, pair2.Item2.globalPosition);
			return distance1 > distance2 ? 1 : distance1 < distance2 ? -1 : 0;
		});

		Doorway doorway1 = doorwayPairs[0].Item1;
		Doorway doorway2 = doorwayPairs[0].Item2;
		connectDoorways(doorway1, doorway2);
	}

	void removeEmptyCorridors()
	{
		for (int i = 0; i < rooms.Count; i++)
		{
			Room room = rooms[i];
			if (room.type.sectorType == SectorType.Corridor)
			{
				int numConnectedDoorways = 0;
				Doorway nonEmptyDoorway = null;
				foreach (Doorway doorway in room.doorways)
				{
					if (doorway.connectedDoorway != null)
					{
						numConnectedDoorways++;
						nonEmptyDoorway = doorway;
					}
				}
				Debug.Assert(numConnectedDoorways > 0);
				if (numConnectedDoorways == 1)
				{
					if (room.type.id == 0xFF)
						Debug.Assert(false);
					nonEmptyDoorway.connectedDoorway.connectedDoorway = null;
					nonEmptyDoorway.connectedDoorway = null;
					tilemap.removeRoom(room);
					rooms.RemoveAt(i--);
				}
			}
		}
	}

	Room placeRoom(RoomType type, Matrix transform)
	{
		Room room = new Room(type, transform, level);
		rooms.Add(room);

		tilemap.placeRoom(room);
		type.onTilemapPlaced(room, tilemap);

		room.placeDoorways(tilemap);
		room.chooseEnemies(random);

		return room;
	}

	void placeDoorwayToTilemap(Doorway doorway, TileMap tilemap)
	{
		tilemap.setFlag(doorway.globalPosition, TileMap.FLAG_DOORWAY, true);
		tilemap.setFlag(doorway.globalPosition + new Vector3i(doorway.globalDirection.z, 0, doorway.globalDirection.x), TileMap.FLAG_DOORWAY, true);
		tilemap.setFlag(doorway.globalPosition + new Vector3i(-doorway.globalDirection.z, 0, -doorway.globalDirection.x), TileMap.FLAG_DOORWAY, true);
		tilemap.setFlag(doorway.globalPosition + new Vector3i(0, 1, 0), TileMap.FLAG_DOORWAY, true);
		tilemap.setFlag(doorway.globalPosition + new Vector3i(doorway.globalDirection.z, 1, doorway.globalDirection.x), TileMap.FLAG_DOORWAY, true);
		tilemap.setFlag(doorway.globalPosition + new Vector3i(-doorway.globalDirection.z, 1, -doorway.globalDirection.x), TileMap.FLAG_DOORWAY, true);
		tilemap.setFlag(doorway.globalPosition + new Vector3i(0, 2, 0), TileMap.FLAG_DOORWAY, true);
		tilemap.setFlag(doorway.globalPosition + new Vector3i(doorway.globalDirection.z, 2, doorway.globalDirection.x), TileMap.FLAG_DOORWAY, true);
		tilemap.setFlag(doorway.globalPosition + new Vector3i(-doorway.globalDirection.z, 2, -doorway.globalDirection.x), TileMap.FLAG_DOORWAY, true);

		if (tilemap.getTile(doorway.globalPosition + new Vector3i(0, -1, 0)) == 0)
			tilemap.setTile(doorway.globalPosition + new Vector3i(0, -1, 0), Tile.dirt.id);
		if (tilemap.getTile(doorway.globalPosition + new Vector3i(doorway.globalDirection.z, -1, doorway.globalDirection.x)) == 0)
			tilemap.setTile(doorway.globalPosition + new Vector3i(doorway.globalDirection.z, -1, doorway.globalDirection.x), Tile.dirt.id);
		if (tilemap.getTile(doorway.globalPosition + new Vector3i(-doorway.globalDirection.z, -1, doorway.globalDirection.x)) == 0)
			tilemap.setTile(doorway.globalPosition + new Vector3i(-doorway.globalDirection.z, -1, doorway.globalDirection.x), Tile.dirt.id);

		if (tilemap.getTile(doorway.globalPosition + new Vector3i(0, 3, 0)) == 0)
			tilemap.setTile(doorway.globalPosition + new Vector3i(0, 3, 0), Tile.cobblestone.id);
		if (tilemap.getTile(doorway.globalPosition + new Vector3i(doorway.globalDirection.z, 3, doorway.globalDirection.x)) == 0)
			tilemap.setTile(doorway.globalPosition + new Vector3i(doorway.globalDirection.z, 3, doorway.globalDirection.x), Tile.cobblestone.id);
		if (tilemap.getTile(doorway.globalPosition + new Vector3i(-doorway.globalDirection.z, 3, doorway.globalDirection.x)) == 0)
			tilemap.setTile(doorway.globalPosition + new Vector3i(-doorway.globalDirection.z, 3, doorway.globalDirection.x), Tile.cobblestone.id);

		if (tilemap.getTile(doorway.globalPosition + new Vector3i(doorway.globalDirection.z * 2, 0, doorway.globalDirection.x * 2)) == 0)
			tilemap.setTile(doorway.globalPosition + new Vector3i(doorway.globalDirection.z * 2, 0, doorway.globalDirection.x * 2), Tile.bricks.id);
		if (tilemap.getTile(doorway.globalPosition + new Vector3i(doorway.globalDirection.z * 2, 1, doorway.globalDirection.x * 2)) == 0)
			tilemap.setTile(doorway.globalPosition + new Vector3i(doorway.globalDirection.z * 2, 1, doorway.globalDirection.x * 2), Tile.bricks.id);
		if (tilemap.getTile(doorway.globalPosition + new Vector3i(doorway.globalDirection.z * 2, 2, doorway.globalDirection.x * 2)) == 0)
			tilemap.setTile(doorway.globalPosition + new Vector3i(doorway.globalDirection.z * 2, 2, doorway.globalDirection.x * 2), Tile.bricks.id);

		if (tilemap.getTile(doorway.globalPosition + new Vector3i(-doorway.globalDirection.z * 2, 0, doorway.globalDirection.x * 2)) == 0)
			tilemap.setTile(doorway.globalPosition + new Vector3i(-doorway.globalDirection.z * 2, 0, doorway.globalDirection.x * 2), Tile.bricks.id);
		if (tilemap.getTile(doorway.globalPosition + new Vector3i(-doorway.globalDirection.z * 2, 1, doorway.globalDirection.x * 2)) == 0)
			tilemap.setTile(doorway.globalPosition + new Vector3i(-doorway.globalDirection.z * 2, 1, doorway.globalDirection.x * 2), Tile.bricks.id);
		if (tilemap.getTile(doorway.globalPosition + new Vector3i(-doorway.globalDirection.z * 2, 2, doorway.globalDirection.x * 2)) == 0)
			tilemap.setTile(doorway.globalPosition + new Vector3i(-doorway.globalDirection.z * 2, 2, doorway.globalDirection.x * 2), Tile.bricks.id);
	}

	Item getRandomKey()
	{
		for (int i = 0; i < 100; i++)
		{
			Item key = Item.GetItemByCategory(ItemCategory.Key, random);
			if (!usedKeyTypes.Contains(key))
			{
				usedKeyTypes.Add(key);
				return key;
			}
		}
		return null;
	}

	void lockCertainDoors()
	{
		void getAccessibleRooms(Room room, Doorway blockedDoorway, List<Room> accessibleRooms)
		{
			foreach (Doorway doorway in room.doorways)
			{
				if (doorway.connectedDoorway != null)
				{
					Room connectedRoom = doorway.connectedDoorway.room;
					if (!accessibleRooms.Contains(connectedRoom))
					{
						accessibleRooms.Add(connectedRoom);
						getAccessibleRooms(connectedRoom, blockedDoorway, accessibleRooms);
					}
				}
			}
		}

		foreach (Room room in rooms)
		{
			foreach (Doorway doorway in room.doorways)
			{
				if (doorway.connectedDoorway != null)
				{
					float lockedChance = 0.25f;
					if (random.NextSingle() < lockedChance)
					{
						doorway.lockedSide = 69420;
						bool alternativePathAvail = getNumDoorsBetween(doorway, doorway.connectedDoorway.room, true) != -1;
						doorway.lockedSide = 0;

						if (alternativePathAvail)
							doorway.lockedSide = random.Next() % 2 * 2 - 1;
						else
						{
							Item key = getRandomKey();
							doorway.requiredKey = key;

							if (key != null)
							{
								// Place required key in one of the accessible rooms
								List<Room> accessibleRooms = new List<Room>();
								accessibleRooms.Add(room);
								getAccessibleRooms(room, doorway, accessibleRooms);
								MathHelper.ShuffleList(accessibleRooms);

								bool keyPlaced = false;
								for (int i = 0; i < accessibleRooms.Count; i++)
								{
									Room accessibleRoom = accessibleRooms[i];
									List<Entity> accessibleRoomEntities = new List<Entity>();
									accessibleRoomEntities.AddRange(accessibleRoom.entities);
									MathHelper.ShuffleList(accessibleRoomEntities);
									foreach (Entity entity in accessibleRoomEntities)
									{
										if (entity is Creature)
										{
											Creature creature = entity as Creature;
											creature.itemDrops.Add(new Creature.ItemDrop(key.id, 1, 1.0f));
											keyPlaced = true;
											break;
										}
										else if (entity is ItemContainerEntity)
										{
											ItemContainerEntity container = entity as ItemContainerEntity;
											ItemSlot slot = container.getContainer().addItem(key);
											if (slot != null)
											{
												keyPlaced = true;
												break;
											}
										}
									}
									if (keyPlaced)
										break;
								}

								if (!keyPlaced)
									doorway.requiredKey = null;
							}
						}
					}
				}
			}
		}
	}

	void closeEmptyDoorways()
	{
		foreach (Room room in rooms)
		{
			for (int i = 0; i < room.doorways.Count; i++)
			{
				Doorway doorway = room.doorways[i];

				if (doorway.connectedDoorway == null)
				{
					room.doorways.RemoveAt(i--);
				}
			}
		}
	}

	void createDoorways()
	{
		foreach (Room room in rooms)
		{
			foreach (Doorway doorway in room.doorways)
			{
				if (doorway.connectedDoorway != null)
					placeDoorwayToTilemap(doorway, tilemap);
			}
		}
	}

	void createSecretWalls()
	{
		foreach (Room room in rooms)
		{
			foreach (Doorway doorway in room.doorways)
			{
				if (doorway.connectedDoorway == null && tilemap.getFlag(doorway.globalPosition, TileMap.FLAG_WALL))
				{
					if (tilemap.getFlag(doorway.globalPosition + doorway.globalDirection, TileMap.FLAG_ROOM) ||
						tilemap.getFlag(doorway.globalPosition + doorway.globalDirection, TileMap.FLAG_CORRIDOR))
					{
						Vector3i side = new Vector3i(doorway.globalDirection.z, 0, doorway.globalDirection.x);
						Vector3i l = doorway.globalPosition + side;
						Vector3i ll = doorway.globalPosition + 2 * side;
						Vector3i r = doorway.globalPosition - side;
						Vector3i rr = doorway.globalPosition - 2 * side;

						if (!tilemap.getFlag(l, TileMap.FLAG_DOORWAY) && !tilemap.getFlag(ll, TileMap.FLAG_DOORWAY) &&
							!tilemap.getFlag(r, TileMap.FLAG_DOORWAY) && !tilemap.getFlag(rr, TileMap.FLAG_DOORWAY))
						{
							Room otherRoom = findRoomAtPosition(doorway.globalPosition + doorway.globalDirection);
							Debug.Assert(otherRoom != null);

							if (otherRoom.type.allowSecretDoorConnections)
							{
								Vector3i position = globalToLocal(doorway.globalPosition, otherRoom.transform);
								Vector3i direction = (Vector3i)Vector3.Round((otherRoom.transform.inverted * new Vector4(-doorway.globalDirection * 1.0f, 0.0f)).xyz);
								Doorway secretWall = new Doorway(otherRoom.doorways.Count, otherRoom, position, direction, 1.0f);
								secretWall.secret = true;
								otherRoom.doorways.Add(secretWall);

								doorway.connectedDoorway = secretWall;
								secretWall.connectedDoorway = doorway;

								placeDoorwayToTilemap(secretWall, tilemap);
							}
						}
					}
					/*
					else if (tilemap.getFlag(doorway.globalPosition + doorway.globalDirection * 2, TileMap.FLAG_ROOM) ||
						tilemap.getFlag(doorway.globalPosition + doorway.globalDirection * 2, TileMap.FLAG_CORRIDOR))
					{
						Vector3i side = new Vector3i(doorway.globalDirection.z, 0, doorway.globalDirection.x);
						Vector3i l = doorway.globalPosition + doorway.globalDirection + side;
						Vector3i ll = doorway.globalPosition + doorway.globalDirection + 2 * side;
						Vector3i r = doorway.globalPosition + doorway.globalDirection - side;
						Vector3i rr = doorway.globalPosition + doorway.globalDirection - 2 * side;

						if (!tilemap.getFlag(l, TileMap.FLAG_DOORWAY) && !tilemap.getFlag(ll, TileMap.FLAG_DOORWAY) &&
							!tilemap.getFlag(r, TileMap.FLAG_DOORWAY) && !tilemap.getFlag(rr, TileMap.FLAG_DOORWAY))
						{
							Room otherRoom = findRoomAtPosition(doorway.globalPosition + doorway.globalDirection * 2);
							Debug.Assert(otherRoom != null);

							Vector3i position = globalToLocal(doorway.globalPosition + doorway.globalDirection, otherRoom.transform);
							Vector3i direction = (Vector3i)Vector3.Round((otherRoom.transform.inverted * new Vector4(-doorway.globalDirection * 1.0f, 0.0f)).xyz);
							Doorway secretWall = new Doorway(otherRoom.doorways.Count, otherRoom, position, direction);
							secretWall.secret = true;
							otherRoom.doorways.Add(secretWall);

							doorway.connectedDoorway = secretWall;
							secretWall.connectedDoorway = doorway;

							placeDoorwayToTilemap(secretWall, tilemap);
						}
					}
					*/
				}
			}
		}
	}

	void determineDoorSpawns()
	{
		foreach (Room room in rooms)
		{
			foreach (Doorway doorway in room.doorways)
			{
				if (doorway.connectedDoorway != null)
				{
					// only spawn the door for one of the two rooms. preferrably let the room spawn it instead of the corridor, otherwise just choose an arbitrary criteria.
					// also don't spawn doors for doorways connected to a* generated corridors.
					bool corridorConnectedToAStar =
						doorway.room.type.id == 0xFF && doorway.connectedDoorway.room.type.sectorType == SectorType.Corridor ||
						doorway.room.type.sectorType == SectorType.Corridor && doorway.connectedDoorway.room.type.id == 0xFF;
					bool shouldSpawnDoor = doorway.room.type.id < doorway.connectedDoorway.room.type.id ||
						doorway.room.type.id == doorway.connectedDoorway.room.type.id && Hash.hash(doorway.room.gridPosition) < Hash.hash(doorway.connectedDoorway.room.gridPosition);
					if (doorway.lockedSide == 0 && doorway.requiredKey == null)
						shouldSpawnDoor = shouldSpawnDoor && !corridorConnectedToAStar && random.NextSingle() < doorway.spawnChance;
					doorway.spawnDoor = shouldSpawnDoor;

					if (!corridorConnectedToAStar)
					{

					}
				}
			}
		}
	}

	static int mod(int x, int m) => (x % m + m) % m;

	void spawnRooms()
	{
		level.rooms.AddRange(rooms);
		for (int i = 0; i < rooms.Count; i++)
			level.roomIDMap.Add(rooms[i].id, i);

		ModelBatch batch = new ModelBatch();
		tilemap.updateMesh(batch, level);
		level.mesh = batch.createModel();

		foreach (Room room in rooms)
		{
			room.spawn(level, this, random);
		}
	}

	void placeStairsAndLadders()
	{
		int getCeilingHeight(Vector3i p)
		{
			for (int y = p.y; y < tilemap.mapPosition.y + tilemap.mapSize.y; y++)
			{
				if (tilemap.isWall(new Vector3i(p.x, y, p.z)))
					return y - p.y;
			}
			return -1;
		};
		int getElevation(Vector3i p, int maxHeight)
		{
			bool hasMetAir = false;
			for (int y = p.y + maxHeight - 1; y >= p.y; y--)
			{
				if (tilemap.isWall(new Vector3i(p.x, y, p.z)))
				{
					if (hasMetAir)
						return y - p.y + 1;
				}
				else
				{
					hasMetAir = true;
				}
			}
			return -1;
		};
		bool isBelowAStarPath(Vector3i p, int maxHeight)
		{
			for (int y = p.y; y < p.y + maxHeight; y++)
			{
				if (tilemap.getFlag(new Vector3i(p.x, y, p.z), TileMap.FLAG_ASTAR_PATH))
					return true;
			}
			return false;
		};

		for (int z = tilemap.mapPosition.z; z < tilemap.mapPosition.z + tilemap.mapSize.z; z++)
		{
			for (int x = tilemap.mapPosition.x; x < tilemap.mapPosition.x + tilemap.mapSize.x; x++)
			{
				for (int y = tilemap.mapPosition.y; y < tilemap.mapPosition.y + tilemap.mapSize.y; y++)
				{
					Vector3i p = new Vector3i(x, y, z);
					Vector3i down = p + Vector3i.Down;
					Vector3i left = p + Vector3i.Left;
					Vector3i right = p + Vector3i.Right;
					Vector3i forward = p + Vector3i.Forward;
					Vector3i back = p + Vector3i.Back;

					//if (x == -19 && z == -12 && y == 0)
					//	Debug.Assert(false);
					//if (x == -19 && z == -11 && y == 0)
					//	Debug.Assert(false);

					bool isCorridorFloor = tilemap.getFlag(p, TileMap.FLAG_STRUCTURE) && tilemap.getFlag(down, TileMap.FLAG_CORRIDOR_WALL);
					if (isCorridorFloor)
					{
						int ceilingHeight = getCeilingHeight(p);
						bool isAStarFloor = isBelowAStarPath(p, ceilingHeight);
						if (tilemap.isWall(left) && !tilemap.isWall(right) && isBelowAStarPath(right, ceilingHeight) && isBelowAStarPath(right + Vector3i.Up, ceilingHeight) && !tilemap.isWall(right + Vector3i.Right) && !tilemap.isWall(right + Vector3i.Forward) && !tilemap.isWall(right + Vector3i.Back))
							isAStarFloor = true;
						if (tilemap.isWall(right) && !tilemap.isWall(left) && isBelowAStarPath(left, ceilingHeight) && isBelowAStarPath(left + Vector3i.Up, ceilingHeight) && !tilemap.isWall(left + Vector3i.Left) && !tilemap.isWall(left + Vector3i.Forward) && !tilemap.isWall(left + Vector3i.Back))
							isAStarFloor = true;
						if (tilemap.isWall(forward) && !tilemap.isWall(back) && isBelowAStarPath(back, ceilingHeight) && isBelowAStarPath(back + Vector3i.Up, ceilingHeight) && !tilemap.isWall(back + Vector3i.Back) && !tilemap.isWall(back + Vector3i.Left) && !tilemap.isWall(back + Vector3i.Right))
							isAStarFloor = true;
						if (tilemap.isWall(back) && !tilemap.isWall(forward) && isBelowAStarPath(forward, ceilingHeight) && isBelowAStarPath(forward + Vector3i.Up, ceilingHeight) && !tilemap.isWall(forward + Vector3i.Forward) && !tilemap.isWall(forward + Vector3i.Left) && !tilemap.isWall(forward + Vector3i.Right))
							isAStarFloor = true;

						if (isAStarFloor)
						{
							Debug.Assert(ceilingHeight != -1);

							ResizableLadder ladder = null;
							Vector3 position = Vector3.Zero;
							Quaternion rotation = Quaternion.Identity;

							// Try straight ladders

							if (tilemap.isWall(left))
							{
								int elevation = getElevation(left, ceilingHeight);
								if (elevation != -1)
								{
									ladder = new ResizableLadder(elevation);
									position = p + new Vector3(0.5f, 0.0f, 0.5f);
									rotation = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI * 0.5f);
								}
							}
							if (tilemap.isWall(right))
							{
								int elevation = getElevation(right, ceilingHeight);
								if (elevation != -1)
								{
									ladder = new ResizableLadder(elevation);
									position = p + new Vector3(0.5f, 0.0f, 0.5f);
									rotation = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI * -0.5f);
								}
							}
							if (tilemap.isWall(forward))
							{
								int elevation = getElevation(forward, ceilingHeight);
								if (elevation != -1)
								{
									ladder = new ResizableLadder(elevation);
									position = p + new Vector3(0.5f, 0.0f, 0.5f);
									rotation = Quaternion.Identity;
								}
							}
							if (tilemap.isWall(back))
							{
								int elevation = getElevation(back, ceilingHeight);
								if (elevation != -1)
								{
									ladder = new ResizableLadder(elevation);
									position = p + new Vector3(0.5f, 0.0f, 0.5f);
									rotation = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI);
								}
							}

							if (ladder == null)
							{
								int leftElevation = getElevation(left, ceilingHeight);
								if (leftElevation != -1)
								{
									if (!tilemap.isWall(left))
									{
										if (tilemap.isWall(left + Vector3i.Forward))
										{
											ladder = new ResizableLadder(leftElevation);
											position = left + Vector3i.Forward + new Vector3(0.5f, 0.0f, 0.5f);
											rotation = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI * 0.5f);
										}
										else if (tilemap.isWall(left + Vector3i.Back))
										{
											ladder = new ResizableLadder(leftElevation);
											position = left + Vector3i.Back + new Vector3(0.5f, 0.0f, 0.5f);
											rotation = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI * 0.5f);
										}
										else if (tilemap.isWall(p + Vector3i.Forward * 2))
										{
											ladder = new ResizableLadder(leftElevation);
											position = p + Vector3i.Forward + new Vector3(0.5f, 0.0f, 0.5f);
											rotation = Quaternion.Identity;
										}
										else if (tilemap.isWall(p + Vector3i.Back * 2))
										{
											ladder = new ResizableLadder(leftElevation);
											position = p + Vector3i.Back + new Vector3(0.5f, 0.0f, 0.5f);
											rotation = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI);
										}
									}
								}
							}

							if (ladder != null)
							{
								level.addEntity(ladder, position, rotation);
								tilemap.setFlag(p, TileMap.FLAG_LADDER, true);
							}
						}
					}
				}
			}
		}
	}

	void placeLoot()
	{
		List<Item> loot = new List<Item>();
		List<int> amounts = new List<int>();

		int numFlasks = MathHelper.RandomInt(1, 4, random);
		for (int i = 0; i < numFlasks; i++)
		{
			loot.Add(Item.Get("flask"));
			amounts.Add(1);
		}

		int numManaFlasks = MathHelper.RandomInt(1, 3, random);
		for (int i = 0; i < numManaFlasks; i++)
		{
			loot.Add(Item.Get("mana_flask"));
			amounts.Add(1);
		}

		int numCoins = MathHelper.RandomInt(8, 15, random);
		for (int i = 0; i < numCoins; i++)
		{
			int amount = MathHelper.RandomInt(3, 10, random);
			loot.Add(Item.Get("gold"));
			amounts.Add(amount);
		}

		int numArrows = MathHelper.RandomInt(5, 9, random);
		for (int i = 0; i < numArrows; i++)
		{
			int amount = MathHelper.RandomInt(1, 7, random);
			loot.Add(Item.Get("arrow"));
			amounts.Add(amount);
		}

		int numFirebombs = MathHelper.RandomInt(0, 2, random);
		for (int i = 0; i < numFirebombs; i++)
		{
			int amount = MathHelper.RandomInt(1, 2, random);
			loot.Add(Item.Get("firebomb"));
			amounts.Add(amount);
		}

		int numWeapons = MathHelper.RandomInt(1, 3, random);
		for (int i = 0; i < numWeapons; i++)
		{
			Item weapon = Item.GetItemByCategory(ItemCategory.Weapon, random);
			if (weapon.name == "default")
				continue;
			loot.Add(weapon);
			amounts.Add(1);
		}

		int numShields = MathHelper.RandomInt(0, 2, random);
		for (int i = 0; i < numShields; i++)
		{
			Item shield = Item.GetItemByCategory(ItemCategory.Shield, random);
			loot.Add(shield);
			amounts.Add(1);
		}

		int numSpells = MathHelper.RandomInt(1, 3, random);
		for (int i = 0; i < numSpells; i++)
		{
			Item spell = Item.GetItemByCategory(ItemCategory.Spell, random);
			loot.Add(spell);
			amounts.Add(1);
		}

		int numArmor = MathHelper.RandomInt(1, 5, random);
		for (int i = 0; i < numArmor; i++)
		{
			Item armor = Item.GetItemByCategory(ItemCategory.Armor, random);
			loot.Add(armor);
			amounts.Add(1);
		}


		if (itemContainers.Count > 0)
		{
			for (int i = 0; i < loot.Count; i++)
			{
				Item item = loot[i];
				int amount = amounts[i];
				ItemContainer container = itemContainers[random.Next() % itemContainers.Count];
				ItemSlot slot = container.addItem(item, amount);
				// TODO add to random slot in container
				if (slot != null)
				{
					loot.RemoveAt(i);
					amounts.RemoveAt(i);
					i--;
				}
			}
		}
	}

	public void generateLevel()
	{
		Console.WriteLine("Generating level on seed " + seed);

		level.tilemap = tilemap;

		startingRoom = placeRoom(RoomType.StartingRoom, Matrix.CreateTranslation(0, 0, 30));
		//finalRoom = placeRoom(RoomType.FinalRoom, Matrix.CreateTranslation(0, 0, -64 - 15));
		mainRoom = placeRoom(RoomType.MainRoom, Matrix.CreateTranslation(-20, 0, -40));

		while (rooms.Count < maxRooms)
		{
			propagateRooms(SectorType.None);
		}
		interconnectRooms(2);
		propagateRooms(SectorType.Corridor);
		removeEmptyCorridors();
		connectRoomsIfNot(startingRoom, mainRoom);
		//connectRoomsIfNot(finalRoom, mainRoom);
		/*
		foreach (Doorway doorway in mainRoom.doorways)
		{
			if (doorway.connectedDoorway == null)
				connectDoorwayToRandomRoom(doorway);
		}
		*/

		lockCertainDoors();

		createDoorways();
		createSecretWalls();
		closeEmptyDoorways();
		determineDoorSpawns();

		level.init();
		spawnRooms();
		placeStairsAndLadders();

		placeLoot();
	}
}
