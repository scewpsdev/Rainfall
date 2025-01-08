using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public partial class LevelGenerator
{
	public Level[] generateGardens(string seed)
	{
		int numGardenFloors = 3;
		Level[] areaGardens = new Level[numGardenFloors];
		areaGardens[0] = new Level(5, "Gardens I", 50, 50, TileType.dirt, 10, 15);
		areaGardens[1] = new Level(6, "Gardens II", 50, 50, TileType.dirt, 15, 20);
		areaGardens[2] = new Level(7, "Gardens III", 50, 50, TileType.dirt, 20, 25);

		List<Mob> createEnemy()
		{
			List<Mob> mobs = new List<Mob>();
			mobs.Add(new Rat());
			mobs.Add(new Spider());
			mobs.Add(new Snake());
			mobs.Add(new Bat());
			mobs.Add(new Slime());
			mobs.Add(new SkeletonArcher());
			mobs.Add(new GreenSpider());
			mobs.Add(new OrangeBat());
			mobs.Add(new BlueSlime());
			mobs.Add(new Leprechaun());
			mobs.Add(new Gandalf());
			return mobs;
		}

		generateGardenFloor(seed, 0, true, false, areaGardens[0], areaGardens[1], null, null, () => createEnemy().Slice(4, 7));
		generateGardenFloor(seed, 1, false, false, areaGardens[1], areaGardens[2], areaGardens[0], areaGardens[0].exit, () => createEnemy().Slice(4, 7));
		generateGardenFloor(seed, 2, false, true, areaGardens[2], null, areaGardens[1], areaGardens[1].exit, () => createEnemy().Slice(4, 7));

		return areaGardens;
	}

	void generateRandomGardenFloor(Level level, Level nextLevel, Level lastLevel, Door lastDoor)
	{
		RoomDef def = specialSet.roomDefs[1];
		level.resize(def.width, def.height);

		Room room = new Room
		{
			x = 0,
			y = 0,
			width = def.width,
			height = def.height,
			roomDefID = def.id,
			set = specialSet
		};

		placeRoom(room, level, (int x, int y) => TileType.dirt);
		level.rooms = [room];

		level.fogFalloff = 0.1f;
		level.fogColor = new Vector3(0.0f);

		level.entrance = new Door(lastLevel, lastDoor);
		Vector2i entrancePosition = new Vector2i(2, 1);
		level.addEntity(level.entrance, new Vector2(entrancePosition.x + 0.5f, entrancePosition.y));
		lastDoor.otherDoor = level.entrance;

		level.exit = new Door(nextLevel);
		Vector2i exitPosition = new Vector2i(14, 1);
		level.addEntity(level.exit, new Vector2(exitPosition.x + 0.5f, exitPosition.y));

		level.addEntity(new GardensShopRoom(room));

		level.updateLightmap(0, 0, def.width, def.height);
	}

	void generateGardenBossFloor(Level level, Level nextLevel, Level lastLevel, Door lastDoor)
	{
		RoomDef def = specialSet.roomDefs[3];
		level.resize(def.width, def.height);

		Room room = new Room
		{
			x = 0,
			y = 0,
			width = def.width,
			height = def.height,
			roomDefID = def.id,
			set = specialSet
		};

		placeRoom(room, level, (int x, int y) => TileType.dirt);
		level.rooms = [room];

		level.fogFalloff = 0.1f;
		level.fogColor = new Vector3(0.0f);

		level.entrance = new Door(lastLevel, lastDoor);
		Vector2i entrancePosition = new Vector2i(2, 1);
		level.addEntity(level.entrance, new Vector2(entrancePosition.x + 0.5f, entrancePosition.y));
		lastDoor.otherDoor = level.entrance;

		level.exit = new Door(nextLevel);
		Vector2i exitPosition = new Vector2i(27, 1);
		level.addEntity(level.exit, new Vector2(exitPosition.x + 0.5f, exitPosition.y));

		if (level.getTile(exitPosition.x - 1, exitPosition.y) == null && !objectFlags[exitPosition.x - 1 + exitPosition.y * level.width])
		{
			level.addEntity(new TorchEntity(), new Vector2(exitPosition.x - 0.5f, exitPosition.y + 0.5f));
			objectFlags[exitPosition.x - 1 + exitPosition.y * level.width] = true;
		}
		if (level.getTile(exitPosition.x + 1, exitPosition.y) == null && !objectFlags[exitPosition.x + 1 + exitPosition.y * level.width])
		{
			level.addEntity(new TorchEntity(), new Vector2(exitPosition.x + 1.5f, exitPosition.y + 0.5f));
			objectFlags[exitPosition.x + 1 + exitPosition.y * level.width] = true;
		}

		level.addEntity(new GardensBossRoom(room));

		level.updateLightmap(0, 0, def.width, def.height);
	}

	void generateGardenFloor(string seed, int floor, bool spawnStartingRoom, bool spawnBossRoom, Level level, Level nextLevel, Level lastLevel, Door entrance, Func<List<Mob>> createEnemy)
	{
		this.seed = seed;
		this.level = level;
		this.nextLevel = nextLevel;
		this.lastLevel = lastLevel;
		this.lastExit = entrance;

		random = new Random((int)Hash.hash(seed) + floor);
		simplex = new Simplex(Hash.hash(seed) + (uint)floor, 3);

		int width = MathHelper.RandomInt(40, 80, random);
		int height = Math.Max(2400 / width, 20);

		level.resize(width, height, TileType.dirt);
		level.rooms = rooms;
		level.ambientLight = MathHelper.ARGBToVector(0xFFdcffb5).xyz;
		level.ambientSound = Resource.GetSound("level/level2/ambience2.ogg");
		//level.fogColor = MathHelper.ARGBToVector(0xFFa0c7eb).xyz;
		//level.fogFalloff = 0.2f;
		//level.bg = Resource.GetTexture("level/level2/bg.png", false);

		objectFlags = new bool[width * height];
		Array.Fill(objectFlags, false);

		rooms.Clear();
		RoomDef? startingRoomDef = spawnStartingRoom ? specialSet.roomDefs[5] : spawnBossRoom ? specialSet.roomDefs[3] : null;
		generateMainRooms(gardensSet, startingRoomDef);
		if (spawnBossRoom)
			rooms.Reverse();
		Room exitRoom = rooms[rooms.Count - 1];
		{
			int i = 2;
			while (exitRoom.width <= 2 || exitRoom.height <= 2)
				exitRoom = rooms[rooms.Count - i++];
		}

		Room startingRoom = null;
		Room mainRoom = rooms[0];
		if (spawnStartingRoom)
		{
			mainRoom = rooms[0];
			mainRoom.spawnEnemies = false;
			level.addEntity(new GardensMainRoom(mainRoom));

			startingRoom = rooms[rooms.Count - 1];

			for (int i = rooms.Count - 2; i >= 0; i--)
			{
				if (rooms[i].countConnectedDoorways() == 1)
				{
					exitRoom = rooms[i];
					break;
				}
			}

			Debug.Assert(exitRoom != null);
		}
		else
		{
			startingRoom = rooms[0];
		}


		generateExtraRooms(gardensSet, null);

		for (int i = 0; i < rooms.Count; i++)
		{
			Room room = rooms[i];
			int x = room.x;
			int y = room.y;
			int w = room.width;
			int h = room.height;
			RoomDef roomDef = room.set.roomDefs[room.roomDefID];

			// why is this here??
			for (int yy = 0; yy < h; yy++)
			{
				for (int xx = 0; xx < w; xx++)
				{
					uint color = roomDef.getTile(xx, yy);
					if (color != 0xFF000000)
						level.setTile(x + xx, y + yy, TileType.dummy);
					else
						level.setTile(x + xx, y + yy, null);
				}
			}

			placeRoom(rooms[i], level, (int x, int y) =>
			{
				TileType left = level.getTile(x - 1, y);
				TileType right = level.getTile(x + 1, y);
				TileType down = level.getTile(x, y - 1);
				TileType up = level.getTile(x, y + 1);

				bool edgeTile = /*left == null || right == null || down == null ||*/ up == null;
				float type = simplex.sample2f(x * 0.05f, y * 0.05f);
				return edgeTile ? (type > -0.3f ? TileType.grass : TileType.path) : TileType.dirt;
			});
		}

		Door entranceDoor = new Door(lastLevel, lastExit);
		createDoors(false, spawnBossRoom, startingRoom, exitRoom, entranceDoor, out Vector2i entrancePosition, out Vector2i exitPosition);

		if (spawnStartingRoom)
			startingRoom.spawnEnemies = false;

		if (spawnBossRoom)
		{
			exitRoom.spawnEnemies = false;
			level.addEntity(new GardensBossRoom(exitRoom));
		}

		List<Room> deadEnds = new List<Room>();
		List<Room> mainRooms = new List<Room>();
		for (int i = 0; i < rooms.Count; i++)
		{
			Room room = rooms[i];
			bool isDeadEnd = room.countConnectedDoorways() == 1 && !room.isMainPath;
			if (isDeadEnd)
				deadEnds.Add(room);
			else if (room.isMainPath)
				mainRooms.Add(room);
		}


		// Leaves
		{
			//level.addEntity(new ParallaxObject(Resource.GetTexture("level/level2/parallax1.png", false), 2.0f), new Vector2(level.width, level.height) * 0.5f);
			//level.addEntity(new ParallaxObject(Resource.GetTexture("level/level2/parallax2.png", false), 0.2f), new Vector2(level.width, level.height) * 0.5f);

			Texture leavesHoriz = Resource.GetTexture("level/level2/leaves_horiz.png", false);
			Texture leavesVert = Resource.GetTexture("level/level2/leaves_vert.png", false);
			Texture leavesCorner = Resource.GetTexture("level/level2/leaves_corner.png", false);

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					bool tile = level.getTile(x, y) != null && level.getTile(x, y).isSolid && level.getTile(x, y).visible;
					bool left = level.getTile(x - 1, y) != null && level.getTile(x - 1, y).isSolid && level.getTile(x - 1, y).visible;
					bool down = level.getTile(x, y - 1) != null && level.getTile(x, y - 1).isSolid && level.getTile(x, y - 1).visible;
					bool leftdown = level.getTile(x - 1, y - 1) != null && level.getTile(x - 1, y - 1).isSolid && level.getTile(x - 1, y - 1).visible;

					// top
					if ((!tile || !left) && down && leftdown)
					{
						ParallaxObject parallaxObject = new ParallaxObject(leavesHoriz, (x + y * 19) * 16, 0, 16, 32, 0.0f);
						level.addEntity(parallaxObject, new Vector2(x, y + 1));
					}
					// bottom
					else if (tile && left && (!down || !leftdown))
					{
						ParallaxObject parallaxObject = new ParallaxObject(leavesHoriz, (x + y * 19) * 16, 32, 16, 32, 0.0f);
						level.addEntity(parallaxObject, new Vector2(x, y - 1));
					}
					// left corners top/bottom pieces
					else if (!tile && !left && !leftdown && down ||
						tile && !left && !leftdown && !down)
					{
						ParallaxObject parallaxObject = new ParallaxObject(leavesCorner, (x + y * 19) * 32, 32, 32, 32, 0.0f);
						level.addEntity(parallaxObject, new Vector2(x + 0.5f, y));
					}
					// right corners top/bottom pieces
					else if (!tile && !left && leftdown && !down ||
						!tile && left && !leftdown && !down)
					{
						ParallaxObject parallaxObject = new ParallaxObject(leavesCorner, (x + y * 19) * 32, 32, 32, 32, 0.0f);
						level.addEntity(parallaxObject, new Vector2(x - 0.5f, y));
					}

					// right
					if ((!tile || !down) && left && leftdown)
					{
						ParallaxObject parallaxObject = new ParallaxObject(leavesVert, 16, (y + x * 19) * 16, 16, 16, 0.0f);
						level.addEntity(parallaxObject, new Vector2(x + 0.5f, y));
					}
					// left
					else if (tile && down && (!left || !leftdown))
					{
						ParallaxObject parallaxObject = new ParallaxObject(leavesVert, 0, (y + x * 19) * 16, 16, 16, 0.0f);
						level.addEntity(parallaxObject, new Vector2(x - 0.5f, y));
					}
					// top corners left/right pieces
					else if (!tile && !left && !leftdown && down ||
						!tile && !left && leftdown && !down)
					{
						ParallaxObject parallaxObject = new ParallaxObject(leavesCorner, (x + y * 19) * 32, 32, 32, 32, 0.0f);
						level.addEntity(parallaxObject, new Vector2(x, y - 0.5f));
					}
					// bottom corners left/right pieces
					else if (tile && !left && !leftdown && !down ||
						!tile && left && !leftdown && !down)
					{
						ParallaxObject parallaxObject = new ParallaxObject(leavesCorner, (x + y * 19) * 32, 32, 32, 32, 0.0f);
						level.addEntity(parallaxObject, new Vector2(x, y + 0.5f));
					}
				}
			}
		}


		// Fountain
		spawnRoomObject(deadEnds, 1, false, (Vector2i tile, Random random, Room room) =>
		{
			Fountain fountain = new Fountain(random);
			level.addEntity(fountain, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Coins
		spawnRoomObject(deadEnds, MathHelper.Remap(floor, 5, 7, 0.05f, 0.02f), true, (Vector2i tile, Random random, Room room) =>
		{
			int amount = MathHelper.RandomInt(2, 10, random);
			level.addEntity(new CoinStack(amount), new Vector2(tile.x + 0.5f, tile.y + 0.5f));
		});

		// Items
		spawnRoomObject(deadEnds, 0.65f, false, (Vector2i tile, Random random, Room room) =>
		{
			//spawnItem(tile.x, tile.y, getRoomLootValue(room));
		});


		MathHelper.ShuffleList(deadEnds, random);
		MathHelper.ShuffleList(mainRooms, random);

		//lockDeadEnds(deadEnds, mainRooms);


		// Arrow trap
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if (tile != null && tile.isSolid && (left == null || right == null) && y != entrancePosition.y)
			{
				float arrowTrapChance = 0.001f;
				if (random.NextSingle() < arrowTrapChance)
				{
					int direction = right == null ? 1 : left == null ? -1 : random.Next() % 2 * 2 - 1;
					level.setTile(x, y, TileType.dummy);
					level.addEntity(new ArrowTrap(new Vector2(direction, 0)), new Vector2(x, y));
					objectFlags[x + y * width] = true;
				}
			}
		});

		// Moneh
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if (tile == null && down != null)
			{
				float gemChance = up != null ? 0.04f : 0.01f;
				gemChance *= 0.25f;
				if (random.NextSingle() < gemChance)
				{
					//int amount = MathHelper.RandomInt(3, 12, random);
					int amount = 10;
					level.addEntity(new CoinStack(amount), new Vector2(x + 0.5f, y + 0.5f));
					objectFlags[x + y * width] = true;
				}
			}
		});

		// Spring
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if (tile == null && down != null && up == null)
			{
				TileType upUp = level.getTile(x, y + 2);
				if (upUp == null)
				{
					float springChance = 0.01f;
					if (random.NextSingle() < springChance)
					{
						level.addEntity(new Trampoline(), new Vector2(x + 0.5f, y));
						objectFlags[x + y * width] = true;
					}
				}
			}
		});

		// Spike
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if (tile == null && down != null && up == null)
			{
				TileType upLeft = level.getTile(x - 1, y + 1);
				TileType upRight = level.getTile(x + 1, y + 1);

				if (upLeft == null && left == null || upRight == null && right == null)
				{
					float spikeChance = 0.015f;
					if (random.NextSingle() < spikeChance)
					{
						level.addEntity(new Spike(), new Vector2(x, y));
						objectFlags[x + y * width] = true;
					}
				}
			}
		});

		// Spike Trap
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if (tile == null && up != null && up.isSolid)
			{
				TileType downDown = level.getTile(x, y - 2);
				TileType downLeft = level.getTile(x - 1, y - 1);
				TileType downRight = level.getTile(x + 1, y - 1);

				if (down == null && downDown == null && (left != null && right != null || left == null && downLeft == null || right == null && downRight == null) && x != entrancePosition.x)
				{
					float spikeTrapChance = 0.01f;
					if (random.NextSingle() < spikeTrapChance)
					{
						level.addEntity(new SpikeTrap(), new Vector2(x + 0.5f, y + 0.5f));
						objectFlags[x + y * width] = true;
					}
				}
			}
		});

		// Barrel
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if (tile == null && down != null && down.visible)
			{
				float barrelChance = MathF.Max(simplex.sample2f(x * 0.03f, y * 0.03f) * 0.3f - 0.1f, 0);
				if (random.NextSingle() < barrelChance)
				{
					float explosiveBarrel = 0.1f;
					if (random.NextSingle() < explosiveBarrel)
					{
						level.addEntity(new ExplosiveBarrel(), new Vector2(x + 0.5f, y));
					}
					else
					{
						Item[] items = null;
						float itemChance = 0.05f;
						if (random.NextSingle() < itemChance)
							items = Item.CreateRandom(random, DropRates.barrel, getLootValue(new Vector2(x, y)));

						level.addEntity(new Barrel(items), new Vector2(x + 0.5f, y));
					}
					objectFlags[x + y * width] = true;
				}
			}
		});


		// Builder merchant
		spawnRoomObject(deadEnds, MathHelper.Remap(floor, 5, 7, 0.1f, 0.03f), false, (Vector2i tile, Random random, Room room) =>
		{
			BuilderMerchant npc = new BuilderMerchant(random, level);
			npc.direction = random.Next() % 2 * 2 - 1;
			level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Traveller merchant
		spawnRoomObject(deadEnds, MathHelper.Remap(floor, 5, 7, 0.01f, 0.05f), false, (Vector2i tile, Random random, Room room) =>
		{
			TravellingMerchant npc = new TravellingMerchant(random, level);
			npc.direction = random.Next() % 2 * 2 - 1;
			level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Logan
		spawnRoomObject(deadEnds, MathHelper.Remap(floor, 5, 7, 0.01f, 0.05f), false, (Vector2i tile, Random random, Room room) =>
		{
			Logan npc = NPCManager.logan;
			npc.direction = random.Next() % 2 * 2 - 1;
			level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Blacksmith
		spawnRoomObject(deadEnds, MathHelper.Remap(floor, 5, 7, 0.1f, 0.02f), false, (Vector2i tile, Random random, Room room) =>
		{
			Blacksmith npc = new Blacksmith();
			npc.direction = random.Next() % 2 * 2 - 1;
			level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Tinkerer
		spawnRoomObject(deadEnds, MathHelper.Remap(floor, 5, 7, 0.01f, 0.05f), false, (Vector2i tile, Random random, Room room) =>
		{
			Tinkerer npc = new Tinkerer(random, level);
			npc.direction = random.Next() % 2 * 2 - 1;
			level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
		});


		level.updateLightmap(0, 0, width, height);
	}
}
