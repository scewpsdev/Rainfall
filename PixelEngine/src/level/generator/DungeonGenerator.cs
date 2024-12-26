using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public partial class LevelGenerator
{
	void generateDungeonBackground(Level level, Simplex simplex)
	{
		for (int y = 0; y < level.height; y++)
		{
			for (int x = 0; x < level.width; x++)
			{
				float progress = 1 - y / (float)level.height;
				float type = simplex.sample2f(x * 0.05f, -y * 0.05f) - progress * 0.4f;
				float mask = simplex.sample2f(-x * 0.05f, y * 0.05f);
				TileType tile = mask < 0 ? null : type > -0.1f ? TileType.dirt : TileType.stone;
				level.setBGTile(x, y, tile);
			}
		}
	}

	public Level[] generateDungeons(string seed)
	{
		Level[] areaCaves = new Level[4];
		Vector3 ambience = MathHelper.ARGBToVector(0xFF3b3159).xyz;
		areaCaves[0] = new Level(5, "Weeping Catacombs", 80, 40, TileType.stone, 15, 20) { ambientLight = ambience };
		areaCaves[1] = new Level(6, "", 40, 80, TileType.stone, 19, 24) { ambientLight = ambience };
		areaCaves[2] = new Level(7, "", 60, 60, TileType.stone, 22, 30) { ambientLight = ambience };
		areaCaves[3] = new Level(-1, "Forgotten Chamber", 40, 20, TileType.stone) { ambientLight = ambience };

		List<Mob> createEnemy()
		{
			List<Mob> mobs = new List<Mob>();
			mobs.Add(new GreenSpider());
			mobs.Add(new OrangeBat());
			mobs.Add(new BlueSlime());
			mobs.Add(new SkeletonArcher());
			mobs.Add(new Leprechaun());
			mobs.Add(new Gandalf());
			return mobs;
		};

		generateDungeonFloor(seed, true, false, areaCaves[0], areaCaves[1], null, null, () => createEnemy());
		generateDungeonFloor(seed, false, false, areaCaves[1], areaCaves[2], areaCaves[0], areaCaves[0].exit, () => createEnemy());
		generateDungeonFloor(seed, false, false, areaCaves[2], areaCaves[3], areaCaves[1], areaCaves[1].exit, () => createEnemy());

		generateDungeonBossFloor(areaCaves[3], null, areaCaves[2], areaCaves[2].exit);

		return areaCaves;
	}

	void generateDungeonBossFloor(Level level, Level nextLevel, Level lastLevel, Door lastDoor)
	{
		simplex = new Simplex(Hash.hash(seed) + (uint)level.floor, 3);

		RoomDef def = specialSet.roomDefs[4];
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

		placeRoom(room, level, (int x, int y) => TileType.stone);
		level.rooms = [room];

		level.fogFalloff = 0.1f;
		level.fogColor = new Vector3(0.0f);

		level.entrance = new Door(lastLevel, lastDoor);
		Vector2i entrancePosition = room.getMarker(0x63);
		level.addEntity(level.entrance, new Vector2(entrancePosition.x + 0.5f, entrancePosition.y));
		lastDoor.otherDoor = level.entrance;

		level.exit = new Door(nextLevel);
		Vector2i exitPosition = room.getMarker(0x67);
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

		generateCaveBackground(level, simplex);

		level.addEntity(new CavesBossRoom(room));

		level.updateLightmap(0, 0, def.width, def.height);
	}

	void generateDungeonFloor(string seed, bool spawnStartingRoom, bool spawnBossRoom, Level level, Level nextLevel, Level lastLevel, Door entrance, Func<List<Mob>> createEnemy)
	{
		this.seed = seed;
		this.level = level;
		this.nextLevel = nextLevel;
		this.lastLevel = lastLevel;
		this.entrance = entrance;

		random = new Random((int)Hash.hash(seed) + level.floor);
		simplex = new Simplex(Hash.hash(seed) + (uint)level.floor, 3);
		rooms = new List<Room>();

		int width = level.width;
		int height = level.height;

		level.rooms = rooms;
		level.ambientSound = Resource.GetSound("res/sounds/ambience.ogg");

		objectFlags = new bool[width * height];
		Array.Fill(objectFlags, false);

		lootModifier = new float[width * height];
		Array.Fill(lootModifier, 1.0f);

		rooms.Clear();
		RoomDef? startingRoomDef = spawnStartingRoom ? specialSet.roomDefs[12] : spawnBossRoom ? specialSet.roomDefs[13] : null;
		generateMainRooms(dungeonsSet, startingRoomDef);
		if (spawnBossRoom)
			rooms.Reverse();
		Room startingRoom = rooms[0];
		Room exitRoom = rooms[rooms.Count - 1];
		{
			int i = 2;
			while (exitRoom.width <= 2 || exitRoom.height <= 2)
				exitRoom = rooms[rooms.Count - i++];
		}

		generateExtraRooms(dungeonsSet);

		for (int i = 0; i < rooms.Count; i++)
		{
			placeRoom(rooms[i], level, (int x, int y) =>
			{
				float progress = 1 - y / (float)level.height;
				float type = simplex.sample2f(x * 0.05f, y * 0.05f);
				return type > -0.5f ? TileType.bricks : TileType.stone;
			});
		}

		generateCaveBackground(level, simplex);

		createDoors(spawnStartingRoom, spawnBossRoom, startingRoom, exitRoom, out Vector2i entrancePosition, out Vector2i exitPosition);

		for (int i = 0; i < rooms.Count; i++)
		{
			if (rooms[i].entity != null)
				level.addEntity(rooms[i].entity, new Vector2(rooms[i].x, rooms[i].y));
		}

		if (spawnStartingRoom)
			startingRoom.spawnEnemies = false;

		/*
		if (spawnBossRoom)
		{
			exitRoom.spawnEnemies = false;
			level.addEntity(new CavesBossRoom(exitRoom));
		}
		*/

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

			if (isDeadEnd)
			{
				for (int y = room.y; y < room.y + room.height; y++)
				{
					for (int x = room.x; x < room.x + room.width; x++)
					{
						lootModifier[x + y * width] = 3.0f;
					}
				}
			}
		}


		// Starting weapon
		List<Room> roomsWithStartingWeapon = mainRooms.Slice(0, 2);
		if (deadEnds.Count > 0)
			roomsWithStartingWeapon.Add(deadEnds[0]);
		if (deadEnds.Count > 1)
			roomsWithStartingWeapon.Add(deadEnds[1]);
		spawnRoomObject(roomsWithStartingWeapon, 1, false, (Vector2i tile, Random random, Room room) =>
		{
			TileType left = level.getTile(tile.x - 1, tile.y);
			TileType right = level.getTile(tile.x + 1, tile.y);
			Item item = Item.CreateRandom(ItemType.Weapon, random, getLootValue((Vector2)tile));
			Item[] items;
			if (item.requiredAmmo != null)
			{
				Item ammo = Item.GetItemPrototype(item.requiredAmmo).copy();
				ammo.stackSize = MathHelper.RandomInt(20, 36, random);
				items = [item, ammo];
			}
			else
			{
				items = [item];
			}
			Chest chest = new Chest(items, left != null && right == null);
			level.addEntity(chest, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Fountain
		spawnRoomObject(deadEnds, 1.0f, false, (Vector2i tile, Random random, Room room) =>
		{
			Fountain fountain = new Fountain(random);
			level.addEntity(fountain, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Coins
		spawnRoomObject(deadEnds, 0.5f, true, (Vector2i tile, Random random, Room room) =>
		{
			int amount = MathHelper.RandomInt(2, 7, random);
			level.addEntity(new Gem(amount), new Vector2(tile.x + 0.5f, tile.y + 0.5f));
		});

		// Items
		spawnRoomObject(deadEnds, 2.0f, false, (Vector2i tile, Random random, Room room) =>
		{
			spawnItem(tile.x, tile.y, getRoomLootValue(room));
		});


		MathHelper.ShuffleList(deadEnds, random);
		MathHelper.ShuffleList(mainRooms, random);

		lockDeadEnds(deadEnds, mainRooms);


		// Guaranteed key per floor
		if (lockedDoorSpawned)
		{
			spawnRoomObject(rooms, 0.5f, false, (Vector2i tile, Random random, Room room) =>
			{
				spawnItem(tile.x, tile.y, [new IronKey()]);
			});
		}


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
					level.addEntity(new Gem(amount), new Vector2(x + 0.5f, y + 0.5f));
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
						level.addEntity(new Spring(), new Vector2(x + 0.5f, y));
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

		// Torch
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if (tile == null && down == null && up == null)
			{
				TileType downDown = level.getTile(x, y - 2);
				if (downDown != null)
				{
					float torchChance = 0.01f;
					if (random.NextSingle() < torchChance)
					{
						level.addEntity(new TorchEntity(), new Vector2(x + 0.5f, y + 0.5f));
						objectFlags[x + y * width] = true;
					}
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
						level.addEntity(new ExplosiveBarrel(), new Vector2(x + 0.5f, y));
					}
					else
					{
						Item[] items = null;
						float itemChance = 0.1f;
						if (random.NextSingle() < itemChance)
							items = Item.CreateRandom(random, DropRates.barrel, getLootValue(new Vector2(x, y)));

						level.addEntity(new Barrel(items), new Vector2(x + 0.5f, y));
					}
					objectFlags[x + y * width] = true;
				}
			}
		});

		// Enemy
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if (tile == null && (left == null && right == null) && !objectFlags[x + y * width] && getRoom(x, y) != startingRoom)
			{
				TileType downLeft = level.getTile(x - 1, y - 1);
				TileType downRight = level.getTile(x + 1, y - 1);

				float distanceToEntrance = (new Vector2i(x, y) - entrancePosition).length;
				Room room = getRoom(x, y);

				if (room != null && room.spawnEnemies && (distanceToEntrance > 8 || y < entrancePosition.y) && (downLeft != null || downRight != null))
				{
					float enemyChance = 0.15f;
					if (random.NextSingle() < enemyChance)
					{
						spawnEnemy(x, y, createEnemy());

						/*
						bool flyingEnemy = random.NextSingle() < 0.15f;
						if (flyingEnemy)
						{
							if (down == null)
							{
								Mob enemy;

								float batType = random.NextSingle();
								if (batType < 0.9f)
									enemy = new Bat();
								else
									enemy = new OrangeBat();

								level.addEntity(enemy, new Vector2(x + 0.5f, y + 0.5f));
								objectFlags[x + y * width] = true;
							}
						}
						else
						{
							TileType upUp = level.getTile(x, y + 2);
							if (down != null && up == null && left == null && right == null)
							{
								float enemyType = random.NextSingle();

								Mob enemy;

								//if (enemyType > 0.9f)
								//	enemy = new Bob();
								//else 
								if (enemyType > 0.95f)
									enemy = new Gandalf();
								else if (enemyType > 0.9f)
									enemy = new SkeletonArcher();
								else if (enemyType > 0.85f && upUp == null)
									enemy = new Golem();
								else if (enemyType > 0.8f)
									enemy = new Leprechaun();
								else if (enemyType > 0.6f)
									enemy = new Snake();
								else if (enemyType > 0.3f)
								{
									float spiderType = random.NextSingle();
									if (spiderType < 0.9f)
										enemy = new Spider();
									else
										enemy = new GreenSpider();
								}
								else
								{
									enemy = new Rat();
								}

								level.addEntity(enemy, new Vector2(x + 0.5f, y));
								objectFlags[x + y * width] = true;
							}
						}
						*/
					}
				}
			}
		});


		spawnRoomObject(deadEnds, 0.1f, false, (Vector2i tile, Random random, Room room) =>
		{
			spawnNPC(tile.x, tile.y);
		});

		/*
		// Builder merchant
		spawnRoomObject(deadEnds, 0.5f, false, (Vector2i tile, Random random, Room room) =>
		{
			BuilderMerchant npc = new BuilderMerchant(random, level);
			npc.direction = random.Next() % 2 * 2 - 1;
			level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Traveller merchant
		spawnRoomObject(deadEnds, 0.2f, false, (Vector2i tile, Random random, Room room) =>
		{
			TravellingMerchant npc = new TravellingMerchant(random, level);
			npc.direction = random.Next() % 2 * 2 - 1;
			level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Logan
		spawnRoomObject(deadEnds, 0.3f, false, (Vector2i tile, Random random, Room room) =>
		{
			Logan npc = new Logan(random, level);
			npc.direction = random.Next() % 2 * 2 - 1;
			level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Blacksmith
		spawnRoomObject(deadEnds, 0.5f, false, (Vector2i tile, Random random, Room room) =>
		{
			Blacksmith npc = new Blacksmith(random, level);
			npc.direction = random.Next() % 2 * 2 - 1;
			level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Tinkerer
		spawnRoomObject(deadEnds, 0.3f, false, (Vector2i tile, Random random, Room room) =>
		{
			Tinkerer npc = new Tinkerer(random, level);
			npc.direction = random.Next() % 2 * 2 - 1;
			level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
		});

		// Rat NPC
		if (!GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_RAT_MET) || GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_RAT_QUESTLINE_COMPLETED) && !ratSpawned)
		{
			spawnRoomObject(deadEnds, !GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_RAT_MET) ? 0.7f : 0.1f, false, (Vector2i tile, Random random, Room room) =>
			{
				RatNPC npc = new RatNPC();
				npc.direction = random.Next() % 2 * 2 - 1;
				level.addEntity(npc, new Vector2(tile.x + 0.5f, tile.y));
				ratSpawned = true;
			});
		}
		*/

		level.updateLightmap(0, 0, width, height);
	}
}
