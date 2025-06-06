using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public partial class LevelGenerator
{
	void generateGardenBackground(Level level, Simplex simplex, TileType tile1, TileType tile2)
	{
		for (int y = 0; y < level.height; y++)
		{
			for (int x = 0; x < level.width; x++)
			{
				float progress = 1 - y / (float)level.height;
				float type = simplex.sample2f(x * 0.05f, -y * 0.05f) - progress * 0.4f;
				float mask = simplex.sample2f(-x * 0.05f, y * 0.05f);
				TileType tile = mask < -0.3f ? null : type > -0.1f ? tile1 : tile2;
				level.setBGTile(x, y, tile);
			}
		}
	}

	public void generateGardens(string seed, out Level[] areaGardens)
	{
		areaGardens = new Level[4];
		areaGardens[0] = new Level(13, "gardens1",  "Royal Gardens", 80, 80, TileType.dirt, 30, 80);
		areaGardens[1] = new Level(14, "gardens2", "", 100, 40, TileType.dirt, 35, 90);
		areaGardens[2] = new Level(15, "gardens3", "", 40, 100, TileType.dirt, 40, 100);
		areaGardens[3] = new Level(-1, "gardens_boss_room", "", 40, 100);

		List<Mob> createEnemy()
		{
			List<Mob> mobs = new List<Mob>();
			mobs.Add(new Snake());
			mobs.Add(new GreenSpider());
			mobs.Add(new BlueSlime());
			mobs.Add(new Leprechaun());
			mobs.Add(new Gandalf());
			return mobs;
		}

		createContainer = (Item[] items) => new Pot(items);
		createExplosiveObject = () => new ExplosivePot();

		generateGardenFloor(seed, 0, true, false, areaGardens[0], areaGardens[1], null, null, () => createEnemy());
		generateGardenFloor(seed, 1, false, false, areaGardens[1], areaGardens[2], areaGardens[0], areaGardens[0].exit, () => createEnemy());
		generateGardenFloor(seed, 2, false, true, areaGardens[2], areaGardens[3], areaGardens[1], areaGardens[1].exit, () => createEnemy());

		generateGardenBossFloor(areaGardens[3], null, areaGardens[2], areaGardens[2].exit);
	}

	public List<NPC> getGardenNPCList()
	{
		List<NPC> npcs = new List<NPC>();
		npcs.Add(new TravellingMerchant(random, level));
		if (!QuestManager.tryGetQuest("logan", "logan_quest", out Quest loganQuest) || loganQuest.state != QuestState.InProgress)
			npcs.Add(NPCManager.logan);
		npcs.Add(NPCManager.tinkerer);

		return npcs;
	}

	void generateGardenBossFloor(Level level, Level nextLevel, Level lastLevel, Door lastDoor)
	{
		Room room = generateSingleRoomLevel(level, specialSet, 18, TileType.dirt, TileType.path, 0, 0x4);

		level.entrance.destination = lastLevel;
		level.entrance.otherDoor = lastDoor;
		lastDoor.otherDoor = level.entrance;

		Simplex simplex = new Simplex(Hash.hash(seed) + (uint)level.floor, 3);
		generateCaveBackground(level, simplex, TileType.dirt, TileType.dirt);

		level.addEntity(new GardensBossRoom(room));
	}

	void generateGardenFloor(string seed, int floor, bool spawnStartingRoom, bool spawnBossRoom, Level level, Level nextLevel, Level lastLevel, Door entrance, Func<List<Mob>> createEnemy)
	{
		this.seed = seed;
		this.level = level;
		this.nextLevel = nextLevel;
		this.lastLevel = lastLevel;
		this.lastExit = entrance;

		random = new Random((int)Hash.hash(seed) + floor);
		rooms = new List<Room>();

		int width = level.width;
		int height = level.height;

		level.rooms = rooms;
		level.ambientLight = MathHelper.ARGBToVector(0xFFdcffb5).xyz * 0.3f;
		level.ambientSound = Resource.GetSound("sounds/ambience2.ogg");
		level.bg = Resource.GetTexture("level/level4/bg.png", false);

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


		Simplex simplex = new Simplex(Hash.hash(seed) + (uint)floor, 3);

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

		generateGardenBackground(level, simplex, TileType.leaf, TileType.tree);


		Door entranceDoor = new Door(lastLevel, lastExit);
		createDoors(false, spawnBossRoom, startingRoom, exitRoom, entranceDoor, out Vector2i entrancePosition, out Vector2i exitPosition);

		for (int i = 0; i < rooms.Count; i++)
		{
			if (rooms[i].entity != null)
				level.addEntity(rooms[i].entity, new Vector2(rooms[i].x, rooms[i].y));
		}

		if (spawnStartingRoom)
			startingRoom.spawnEnemies = false;

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
		if (false)
		{
			//level.addEntity(new ParallaxObject(Resource.GetTexture("level/level2/parallax1.png", false), 2.0f), new Vector2(level.width, level.height) * 0.5f);
			//level.addEntity(new ParallaxObject(Resource.GetTexture("level/level2/parallax2.png", false), 0.2f), new Vector2(level.width, level.height) * 0.5f);

			Texture leavesHoriz = Resource.GetTexture("level/level4/leaves_horiz.png", false);
			Texture leavesVert = Resource.GetTexture("level/level4/leaves_vert.png", false);
			Texture leavesCorner = Resource.GetTexture("level/level4/leaves_corner.png", false);

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
						ParallaxObject parallaxObject = new ParallaxObject(leavesHoriz, (x + y * 19) * 16, 0, 16, 32, new FloatRect(-0.5f, -1, 1, 2), 0.0f);
						level.addEntity(parallaxObject, new Vector2(x, y + 1));
					}
					// bottom
					else if (tile && left && (!down || !leftdown))
					{
						ParallaxObject parallaxObject = new ParallaxObject(leavesHoriz, (x + y * 19) * 16, 32, 16, 32, new FloatRect(-0.5f, -1, 1, 2), 0.0f);
						level.addEntity(parallaxObject, new Vector2(x, y - 1));
					}
					// left corners top/bottom pieces
					else if (!tile && !left && !leftdown && down ||
						tile && !left && !leftdown && !down)
					{
						ParallaxObject parallaxObject = new ParallaxObject(leavesCorner, (x + y * 19) * 32, 32, 32, 32, new FloatRect(-1, -1, 2, 2), 0.0f);
						level.addEntity(parallaxObject, new Vector2(x + 0.5f, y));
					}
					// right corners top/bottom pieces
					else if (!tile && !left && leftdown && !down ||
						!tile && left && !leftdown && !down)
					{
						ParallaxObject parallaxObject = new ParallaxObject(leavesCorner, (x + y * 19) * 32, 32, 32, 32, new FloatRect(-1, -1, 2, 2), 0.0f);
						level.addEntity(parallaxObject, new Vector2(x - 0.5f, y));
					}

					// right
					if ((!tile || !down) && left && leftdown)
					{
						ParallaxObject parallaxObject = new ParallaxObject(leavesVert, 16, (y + x * 19) * 16, 16, 16, new FloatRect(-0.5f, -0.5f, 1, 1), 0.0f);
						level.addEntity(parallaxObject, new Vector2(x + 0.5f, y));
					}
					// left
					else if (tile && down && (!left || !leftdown))
					{
						ParallaxObject parallaxObject = new ParallaxObject(leavesVert, 0, (y + x * 19) * 16, 16, 16, new FloatRect(-0.5f, -0.5f, 1, 1), 0.0f);
						level.addEntity(parallaxObject, new Vector2(x - 0.5f, y));
					}
					// top corners left/right pieces
					else if (!tile && !left && !leftdown && down ||
						!tile && !left && leftdown && !down)
					{
						ParallaxObject parallaxObject = new ParallaxObject(leavesCorner, (x + y * 19) * 32, 32, 32, 32, new FloatRect(-1, -1, 2, 2), 0.0f);
						level.addEntity(parallaxObject, new Vector2(x, y - 0.5f));
					}
					// bottom corners left/right pieces
					else if (tile && !left && !leftdown && !down ||
						!tile && left && !leftdown && !down)
					{
						ParallaxObject parallaxObject = new ParallaxObject(leavesCorner, (x + y * 19) * 32, 32, 32, 32, new FloatRect(-1, -1, 2, 2), 0.0f);
						level.addEntity(parallaxObject, new Vector2(x, y + 0.5f));
					}
				}
			}
		}


		List<Item[]> items = generateItems(level.minLootValue, level.maxLootValue, DropRates.gardens);

		float keyChance = 0.25f;
		if (random.NextSingle() < keyChance)
			items.Add([new IronKey()]);

		MathHelper.ShuffleList(deadEnds, random);
		MathHelper.ShuffleList(mainRooms, random);

		lockDeadEnds(deadEnds, items);

		spawnItems(items, deadEnds);


		// Fountain
		spawnRoomObject(deadEnds, 1, false, (Vector2i tile, Random random, Room room) =>
		{
			Fountain fountain = new Fountain(random);
			level.addEntity(fountain, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Coins
		spawnRoomObject(deadEnds, 0.05f, true, (Vector2i tile, Random random, Room room) =>
		{
			int amount = MathHelper.RandomInt(2, 10, random);
			level.addEntity(new CoinStack(amount), new Vector2(tile.x + 0.5f, tile.y + 0.5f));
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

		// Sconces
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if (tile == null && down != null && up == null)
			{
				float sconceChance = 0.02f;
				if (random.NextSingle() < sconceChance)
				{
					level.addEntity(new GlowingFlower(), new Vector2(x + 0.5f, y));
					objectFlags[x + y * width] = true;
				}
			}
		});

		// Barrel
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if (tile == null && down != null && down.isSolid && down.visible)
			{
				float barrelChance = MathF.Max(simplex.sample2f(x * 0.04f, y * 0.04f) * 0.3f - 0.12f, 0);
				if (random.NextSingle() < barrelChance)
				{
					float explosiveBarrel = 0.1f;
					if (random.NextSingle() < explosiveBarrel)
					{
						level.addEntity(new ExplosivePot(), new Vector2(x + 0.5f, y));
					}
					else
					{
						level.addEntity(new Pot(null), new Vector2(x + 0.5f, y));
					}
					objectFlags[x + y * width] = true;
				}
			}
		});


		spawnEnemies(createEnemy, entrancePosition);


		// Anvil
		if (floor == GameState.instance.areaGardens.Length - 2)
		{
			spawnRoomObject(deadEnds, deadEnds.Count, false, (Vector2i tile, Random random, Room room) =>
			{
				level.addEntity(new Anvil(), new Vector2(tile.x + 0.5f, tile.y));
			});
		}


		spawnRoomObject(deadEnds, 0.1f, false, (Vector2i tile, Random random, Room room) =>
		{
			spawnNPC(tile.x, tile.y, getDungeonNPCList());
		});

		if (floor == 2)
		{
			spawnRoomObject([exitRoom], 1.0f, false, (Vector2i pos, Random random, Room room) =>
			{
				spawnNPC(pos.x, pos.y, getDungeonNPCList());
			});
		}


		level.updateLightmap(0, 0, width, height);
	}
}
