using System;
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
	public const float TILE_SIZE = 1.0f;


	Level level;
	TileMap tilemap;

	int maxRooms = 25;

	Random random;

	List<Room> rooms = new List<Room>();
	Room startingRoom, finalRoom, mainRoom;

	Model floor, wall, ceiling;


	public LevelGenerator(int seed, Level level)
	{
		this.level = level;

		random = new Random(seed);

		floor = Resource.GetModel("res/models/tiles/floor.gltf");
		wall = Resource.GetModel("res/models/tiles/wall.gltf");
		ceiling = Resource.GetModel("res/models/tiles/ceiling.gltf");
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
			SectorType nextSectorType = openDoorway.room.type.getNextSectorType(openDoorway);
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
		int roomID = tilemap.getTile(position);
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

	int checkDoorwayForRoom(Doorway doorway2, Room room, Room lastRoom, List<Room> checkedRooms)
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
			if (doorway != doorway2 && doorway.connectedDoorway != null)
			{
				if (!checkedRooms.Contains(doorway.connectedDoorway.room))
				{
					int found = checkDoorwayForRoom(doorway.connectedDoorway, room, doorway2.room, checkedRooms);
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

	int getNumDoorsBetween(Doorway doorway, Room room)
	{
		List<Room> checkedRooms = new List<Room>();
		int chainLength = checkDoorwayForRoom(doorway, room, null, checkedRooms);
		if (chainLength != 0)
			return chainLength;

		//Debug.Assert(false);
		return -1;
	}

	public bool isDoorwayConnectedToRoom(Doorway doorway, Room room)
	{
		return getNumDoorsBetween(doorway, room) != -1;
	}

	void connectDoorways(Doorway doorway1, Doorway doorway2)
	{
		bool[] walkable = new bool[tilemap.mapSize.x * tilemap.mapSize.y * tilemap.mapSize.z];
		Array.Fill(walkable, true);
		foreach (Room room in rooms)
		{
			if (room.type.sectorType == SectorType.Room)
			{
				for (int z = room.gridPosition.z - 2; z < room.gridPosition.z + room.gridSize.z + 2; z++)
				{
					for (int x = room.gridPosition.x - 2; x < room.gridPosition.x + room.gridSize.x + 2; x++)
					{
						for (int y = room.gridPosition.y - 2; y < room.gridPosition.y + room.gridSize.y + 2; y++)
						{
							Vector3i p = new Vector3i(x, y, z);
							bool isInsideRoom = p >= room.gridPosition && p < room.gridPosition + room.gridSize;
							if (!isInsideRoom)
							{
								bool isDoorway = false;
								foreach (Doorway doorway in room.doorways)
								{
									if (p == doorway.globalPosition || p == doorway.globalPosition + doorway.globalDirection)
									{
										isDoorway = true;
										break;
									}
								}
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
		Array.Fill(costs, 2);
		for (int z = 0; z < tilemap.mapSize.z; z++)
		{
			for (int y = 0; y < tilemap.mapSize.y; y++)
			{
				for (int x = 0; x < tilemap.mapSize.x; x++)
				{
					if (tilemap.getTile(tilemap.mapPosition + new Vector3i(x, y, z)) != 0)
						costs[x + y * tilemap.mapSize.x + z * tilemap.mapSize.x * tilemap.mapSize.y] = 1;
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

		tilemap.resize(tilemap.mapPosition.x - 3, tilemap.mapPosition.y, tilemap.mapPosition.z - 3, tilemap.mapPosition.x + tilemap.mapSize.x + 3, tilemap.mapPosition.y + tilemap.mapSize.y - 3, tilemap.mapPosition.z + tilemap.mapSize.z + 3);

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
					bool randomFactor = random.NextSingle() < doorway.spawnChance;
					shouldSpawnDoor = shouldSpawnDoor && !corridorConnectedToAStar && randomFactor;
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
		level.rooms = rooms;
		level.roomIDMap = new Dictionary<int, int>();
		for (int i = 0; i < rooms.Count; i++)
			level.roomIDMap.Add(rooms[i].id, i);

		ModelBatch wallBatch = new ModelBatch();
		ModelBatch floorBatch = new ModelBatch();
		ModelBatch ceilingBatch = new ModelBatch();

		for (int z = tilemap.mapPosition.z; z < tilemap.mapPosition.z + tilemap.mapSize.z; z++)
		{
			for (int x = tilemap.mapPosition.x; x < tilemap.mapPosition.x + tilemap.mapSize.x; x++)
			{
				for (int y = tilemap.mapPosition.y; y < tilemap.mapPosition.y + tilemap.mapSize.y; y++)
				{
					if (tilemap.isWall(x, y, z))
						continue;

					Vector3i p = new Vector3i(x, y, z);
					Matrix tileTransform = Matrix.CreateTranslation(new Vector3(x + 0.5f, y, z + 0.5f));

					Room room = findRoomAtPosition(p);

					if (tilemap.isWall(p + Vector3i.Down))
					{
						if (room == null || room.type.generateWallMeshes)
							floorBatch.addModel(floor, tileTransform, mod(x, 3) + mod(z, 3) * 3, new Vector2i(3));
						level.body.addBoxCollider(
							new Vector3(0.5f),
							tileTransform.translation + new Vector3(0, -0.5f, 0),
							Quaternion.Identity);
					}
					if (tilemap.isWall(p + Vector3i.Up))
					{
						if (room == null || room.type.generateWallMeshes)
							ceilingBatch.addModel(ceiling, Matrix.CreateTranslation(0, 1, 0) * tileTransform, mod(-x, 3) + mod(z, 3) * 3, new Vector2i(3));
						level.body.addBoxCollider(
							new Vector3(0.5f),
							tileTransform.translation + new Vector3(0, 1.5f, 0),
							Quaternion.Identity);
					}
					if (tilemap.isWall(p + Vector3i.Forward))
					{
						if (room == null || room.type.generateWallMeshes)
							wallBatch.addModel(wall, tileTransform, mod(x + z - 1, 3) + mod(-y, 3) * 3, new Vector2i(3));
						level.body.addBoxCollider(new Vector3(0.5f), tileTransform.translation + new Vector3(0.0f, 0.5f, -1.0f), Quaternion.Identity);
					}
					if (tilemap.isWall(p + Vector3i.Back))
					{
						if (room == null || room.type.generateWallMeshes)
							wallBatch.addModel(wall, tileTransform * Matrix.CreateRotation(Vector3.Up, MathF.PI), mod(-x + z + 1, 3) + mod(-y, 3) * 3, new Vector2i(3));
						level.body.addBoxCollider(new Vector3(0.5f), tileTransform.translation + new Vector3(0.0f, 0.5f, 1.0f), Quaternion.Identity);
					}
					if (tilemap.isWall(p + Vector3i.Left))
					{
						if (room == null || room.type.generateWallMeshes)
							wallBatch.addModel(wall, tileTransform * Matrix.CreateRotation(Vector3.Up, MathF.PI * 0.5f), mod(x - 1 - z, 3) + mod(-y, 3) * 3, new Vector2i(3));
						level.body.addBoxCollider(new Vector3(0.5f), tileTransform.translation + new Vector3(-1.0f, 0.5f, 0.0f), Quaternion.Identity);
					}
					if (tilemap.isWall(p + Vector3i.Right))
					{
						if (room == null || room.type.generateWallMeshes)
							wallBatch.addModel(wall, tileTransform * Matrix.CreateRotation(Vector3.Up, MathF.PI * -0.5f), mod(x + 1 + z, 3) + mod(-y, 3) * 3, new Vector2i(3));
						level.body.addBoxCollider(new Vector3(0.5f), tileTransform.translation + new Vector3(1.0f, 0.5f, 0.0f), Quaternion.Identity);
					}
				}
			}
		}

		foreach (Room room in rooms)
		{
			room.spawn(level, this, random);
		}

		level.levelMeshes.Add(new LevelMesh(wallBatch.createModel(), Matrix.Identity));
		level.levelMeshes.Add(new LevelMesh(floorBatch.createModel(), Matrix.Identity));
		level.levelMeshes.Add(new LevelMesh(ceilingBatch.createModel(), Matrix.Identity));
	}

	void placeLadders()
	{
		var getCeilingHeight = (Vector3i p) =>
		{
			for (int y = p.y; y < tilemap.mapPosition.y + tilemap.mapSize.y; y++)
			{
				if (tilemap.isWall(new Vector3i(p.x, y, p.z)))
					return y - p.y;
			}
			return -1;
		};
		var getElevation = (Vector3i p, int maxHeight) =>
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
		var isBelowAStarPath = (Vector3i p, int maxHeight) =>
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

					bool isCorridorFloor = tilemap.getFlag(p, TileMap.FLAG_STRUCTURE) && tilemap.getFlag(down, TileMap.FLAG_CORRIDOR_WALL);
					if (isCorridorFloor)
					{
						int ceilingHeight = getCeilingHeight(p);
						bool isAStarFloor = isBelowAStarPath(p, ceilingHeight);
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
							}
						}
					}
				}
			}
		}
	}

	public void generateLevel()
	{
		Console.WriteLine("Generating level");

		RoomType.Init();

		tilemap = new TileMap();
		level.tilemap = tilemap;

		//rooms.Add(createStartingRoom());

		//tilemap.resize(-10, 0, -10, 10, 10, 10);

		startingRoom = placeRoom(RoomType.StartingRoom, Matrix.CreateTranslation(0, 0, 30));
		finalRoom = placeRoom(RoomType.FinalRoom, Matrix.CreateTranslation(-8, 0, -64 - 31));
		mainRoom = placeRoom(RoomType.MainRoom, Matrix.CreateTranslation(-20, 0, -40));

		while (rooms.Count < maxRooms)
		{
			propagateRooms(SectorType.None);
		}
		//placeFinalRoom();
		interconnectRooms(2);
		propagateRooms(SectorType.Corridor);
		removeEmptyCorridors();
		connectRoomsIfNot(startingRoom, mainRoom);
		connectRoomsIfNot(finalRoom, mainRoom);
		foreach (Doorway doorway in mainRoom.doorways)
		{
			if (doorway.connectedDoorway == null)
				connectDoorwayToRandomRoom(doorway);
		}

		createDoorways();
		createSecretWalls();
		closeEmptyDoorways();
		determineDoorSpawns();

		level.init();
		spawnRooms();
		placeLadders();

		for (int i = 0; i < 24; i++)
		{
			//level.addEntity(new TestFire(), level.spawnPoint + new Vector3(MathHelper.RandomFloat(-8.0f, 8.0f), 1.0f, MathHelper.RandomFloat(-8.0f, 8.0f)), Quaternion.Identity);
		}

		// decorate rooms & place loot

		// spawn enemies
	}
}
