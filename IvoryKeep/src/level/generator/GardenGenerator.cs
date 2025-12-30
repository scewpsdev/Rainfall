using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public class GardenGenerator : LevelGenerator
{
	public GardenGenerator(string seed)
		: base("garden", "Garden", gardensSet, TileType.dirt, TileType.stone, seed)
	{
		bgPrimaryTile = TileType.leaf;
		bgSecondaryTile = TileType.tree;
	}

	public override int getNumFloors()
	{
		return 3;
	}

	public override string getLevelName(int floor)
	{
		return floor == 0 ? "Royal Gardens" : "";
	}

	public override int getLootValue(int floor)
	{
		return 36 + (floor + 1) * 4;
	}

	public override void getLevelSize(int floor, out int width, out int height)
	{
		width = 70;
		height = 30;

		float wideLevelChance = 0.2f;
		if (random.NextSingle() < wideLevelChance)
			Mathf.Swap(ref width, ref height);

		float squareLevelChance = 0.2f;
		if (random.NextSingle() < squareLevelChance)
		{
			width = 50;
			height = 50;
		}
	}

	public override Sound getAmbientSound()
	{
		return Resource.GetSound("sounds/ambience2.ogg");
	}

	public override MultilayerTrack getAmbientTrack()
	{
		return null;
	}

	public override int getAmbientLight()
	{
		return 4; // Mathf.ARGBToVector(0xFFdcffb5).xyz * 0.3f;
	}

	public override Door createEntranceDoor(Level lastLevel, Door lastExit)
	{
		return new CaveEntranceDoor(lastLevel, lastExit);
	}

	public override BossRoom createBossRoom(Room room)
	{
		return new GardensBossRoom(room);
	}

	public override RoomDef[] getSecretRoomDefs()
	{
		RoomDef[] secretRooms = new RoomDef[6];
		secretRooms[0] = specialSet.roomDefs[6];
		secretRooms[1] = specialSet.roomDefs[random.Next() % 2 == 0 ? 7 : 8];
		secretRooms[2] = specialSet.roomDefs[random.Next() % 2 == 0 ? 9 : 10];
		secretRooms[3] = specialSet.roomDefs[11];
		secretRooms[4] = !GameState.instance.save.areAllStartingClassesUnlocked() ? cavesSpecialSet.roomDefs[0] : specialSet.roomDefs[6];
		secretRooms[5] = specialSet.roomDefs[20];

		return secretRooms;
	}

	public override Entity createSecretRoomEntity(int type, Room room)
	{
		if (type == 0) return new CavesSpecialRoom1(room, this);
		if (type == 1) return new CavesSpecialRoom2(room, this);
		if (type == 2) return new CavesSpecialRoom3(room, this);
		if (type == 3) return new CavesPlatformingRoom1(room, this);
		if (type == 4) return !GameState.instance.save.areAllStartingClassesUnlocked() ? new PrisonCellRoom(room, this) : new CavesSpecialRoom1(room, this);
		return null;
	}

	public override TileType getTile(int x, int y, int idx, TileType tile)
	{
		TileType left = level.getTile(x - 1, y);
		TileType right = level.getTile(x + 1, y);
		TileType down = level.getTile(x, y - 1);
		TileType up = level.getTile(x, y + 1);

		bool edgeTile = /*left == null || right == null || down == null ||*/ up == null;
		float type = simplex.sample2f(x * 0.05f, y * 0.05f);
		return edgeTile ? (type > -0.3f ? TileType.grass : TileType.path) : TileType.dirt;
	}

	public override float[] getDroprates()
	{
		return DropRates.minesDroprates;
	}

    public override int getNumItems(int floor)
    {
		return Mathf.RollDice(2, 2, random) - 1;
	}

	public override Container createContainer(Item[] items)
	{
		return new Pot(items);
	}

	public override ExplosiveObject createExplosiveObject()
	{
		return new ExplosivePot();
	}

	public override List<Mob> createEnemy(Level level)
	{
		List<Mob> mobs = new List<Mob>();
		mobs.Add(new Snake());
		if (level.lightLevel <= 3)
			mobs.Add(new GreenSpider());
		else
			mobs.Add(new Snake());
		mobs.Add(new BlueSlime());
		mobs.Add(new Leprechaun());
		mobs.Add(new Gandalf());
		return mobs;
	}

	public override NPC createNPC(int type, Level level)
	{
		List<NPC> npcs = new List<NPC>();
		npcs.Add(new TravellingMerchant(random, level));
		if (!QuestManager.tryGetQuest("logan", "logan_quest", out Quest loganQuest) || loganQuest.state != QuestState.InProgress)
			npcs.Add(new Logan() /*NPCManager.logan*/);
		//npcs.Add(new Tinkerer() /*NPCManager.tinkerer*/);

		return npcs[random.Next() % npcs.Count];
	}

	public override void onFloorFinish(Level level)
	{
		// Leaves
		{
			//level.addEntity(new ParallaxObject(Resource.GetTexture("level/level2/parallax1.png", false), 2.0f), new Vector2(level.width, level.height) * 0.5f);
			//level.addEntity(new ParallaxObject(Resource.GetTexture("level/level2/parallax2.png", false), 0.2f), new Vector2(level.width, level.height) * 0.5f);

			Texture leavesHoriz = Resource.GetTexture("level/level4/leaves_horiz.png", false);
			Texture leavesVert = Resource.GetTexture("level/level4/leaves_vert.png", false);
			Texture leavesCorner = Resource.GetTexture("level/level4/leaves_corner.png", false);

			for (int y = 0; y < level.height; y++)
			{
				for (int x = 0; x < level.width; x++)
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

		// Flowers
		spawnTileObject((int x, int y, TileType tile, TileType left, TileType right, TileType down, TileType up) =>
		{
			if (tile == null && down != null && up == null)
			{
				float flowerChance = 0.02f;
				if (random.NextSingle() < flowerChance)
				{
					level.addEntity(new GlowingFlower(), new Vector2(x + 0.5f, y));
					setObjectFlag(x, y);
				}
			}
		});
	}
}
