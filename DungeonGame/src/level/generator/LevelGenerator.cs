using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Rainfall;

internal class LevelGenerator
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

	void propagateRooms()
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

	Room findRoomAtPosition(Vector3i position)
	{
		foreach (Room room in rooms)
		{
			if (position.x >= room.gridPosition.x && position.x < room.gridPosition.x + room.gridSize.x &&
				position.y >= room.gridPosition.y && position.y < room.gridPosition.y + room.gridSize.y &&
				position.z >= room.gridPosition.z && position.z < room.gridPosition.z + room.gridSize.z)
				return room;
		}
		return null;
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
		foreach (Doorway doorway in room.doorways)
		{
			if (doorway == doorway2)
				return 1;
		}
		checkedRooms.Add(room);

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

	int getNumDoorsBetween(Doorway doorway1, Doorway doorway2)
	{
		List<Room> checkedRooms = new List<Room>();
		int chainLength = checkRoomForDoorway(doorway1.room, doorway2, null, checkedRooms);
		if (chainLength != 0)
			return chainLength;

		//Debug.Assert(false);
		return -1;
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
					int numDoorsBetween = getNumDoorsBetween(openDoorways[i], openDoorways[j]);
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
	}

	Room placeRoom(RoomType type, Matrix transform)
	{
		Room room = new Room(type, transform, level);
		rooms.Add(room);

		tilemap.placeRoom(room);
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
								Doorway secretWall = new Doorway(otherRoom.doorways.Count, otherRoom, position, direction);
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
						doorway.room.type.id == 0xFFFF && doorway.connectedDoorway.room.type.sectorType == SectorType.Corridor ||
						doorway.room.type.sectorType == SectorType.Corridor && doorway.connectedDoorway.room.type.id == 0xFFFF;
					bool shouldSpawnDoor = doorway.room.type.id < doorway.connectedDoorway.room.type.id ||
						doorway.room.type.id == doorway.connectedDoorway.room.type.id && Hash.hash(doorway.room.gridPosition) < Hash.hash(doorway.connectedDoorway.room.gridPosition);
					shouldSpawnDoor = shouldSpawnDoor && !corridorConnectedToAStar;
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
			room.spawn(level, random);
		}

		level.levelMeshes.Add(new LevelMesh(wallBatch.createModel(), Matrix.Identity));
		level.levelMeshes.Add(new LevelMesh(floorBatch.createModel(), Matrix.Identity));
		level.levelMeshes.Add(new LevelMesh(ceilingBatch.createModel(), Matrix.Identity));


		Room spawnRoom = mainRoom;
		level.spawnPoint = (spawnRoom.gridPosition * 1.0f + new Vector3(spawnRoom.gridSize.x * 0.5f, 0.0f, spawnRoom.gridSize.z * 0.5f)) * TILE_SIZE;
		Vector3 startingChestPosition = (spawnRoom.gridPosition + new Vector3i(spawnRoom.gridSize.x / 4 * 3, 0, spawnRoom.gridSize.z)) * TILE_SIZE - new Vector3(0.0f, 0.0f, 1.0f);
		level.addEntity(new Chest(new Item[]
		{
			Item.Get("shortsword"),
			Item.Get("longsword"),
			Item.Get("longbow"),
			Item.Get("arrow"),
			Item.Get("torch"),
			Item.Get("wooden_round_shield"),
			Item.Get("leather_chestplate"),
			Item.Get("flask"),
			Item.Get("firebomb"),
		},
		new int[] { 1, 1, 1, 20, 1, 1, 1, 2, 10 }), startingChestPosition, Quaternion.FromAxisAngle(Vector3.Up, MathF.PI));

		//level.addEntity(new SkeletonEnemy(), level.spawnPoint + new Vector3(0.0f, 0.0f, -3.0f), Quaternion.Identity);
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
		finalRoom = placeRoom(RoomType.FinalRoom, Matrix.CreateTranslation(0, 0, -70));
		mainRoom = placeRoom(RoomType.MainRoom, Matrix.CreateTranslation(-20, 0, -40));

		while (rooms.Count < maxRooms)
		{
			propagateRooms();
		}
		//placeFinalRoom();
		interconnectRooms(2);

		createDoorways();
		createSecretWalls();
		closeEmptyDoorways();
		determineDoorSpawns();

		level.init();
		spawnRooms();

		for (int i = 0; i < 24; i++)
		{
			//level.addEntity(new TestFire(), level.spawnPoint + new Vector3(MathHelper.RandomFloat(-8.0f, 8.0f), 1.0f, MathHelper.RandomFloat(-8.0f, 8.0f)), Quaternion.Identity);
		}

		// decorate rooms & place loot

		// spawn enemies
	}
}
