using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public enum Biome : byte
{
	None = 0,

	Caves,
	Mines,
	Catacombs,
	Gardens,
	Tree,
	Castle1,
	Castle2,
	Castle3,
	Archives,

	Count
}

public class WorldManager
{
	public static WorldManager instance => GameState.instance.world;

	public static uint[] biomeColors = new uint[(int)Biome.Count]
	{
		0x0,
		0xFF71413b,
		0xFF403353,
		0xFF423934,
		0xFF24523b,
		0xFF482622,
		0xFF4a5462,
		0xFF363e48,
		0xFF282d34,
		0xFF3c31a6,
	};

	public static bool GetBiomeFromColor(uint color, out Biome biome)
	{
		for (int i = 0; i < biomeColors.Length; i++)
		{
			if (biomeColors[i] == color)
			{
				biome = (Biome)i;
				return true;
			}
		}
		biome = Biome.None;
		return false;
	}

	public static string[] biomeDisplayNames = new string[(int)Biome.Count]
	{
		"???",
		"Shiverstone Cavern",
		"The Mines",
		"Castle Catacombs",
		"Royal Gardens",
		"Hollow Tree",
		"The Castlegrounds",
		"",
		"",
		"The Archives",
	};

	public static int[] biomeLootValues = new int[(int)Biome.Count] {
		0,
		10,
		20,
		30,
		40,
		50,
		60,
		70,
		80,
		90,
	};


	public LevelGenerator generator;

	//public Level startingCave;
	public Level introBridge;
	public Level graveyard;
	Level cemetary;
	public Level hub;
	public Level cliffside;
	public Level tutorial;
	public Level[] areaCaves;
	public Level[] areaDungeons;
	public Level[] areaMines;
	public Level[] areaGardens;

	public Level seamlessLevel;
	Dictionary<int, Vector2i> markers = new Dictionary<int, Vector2i>();
	Vector2 playerSpawn;
	Vector2i generatorAttachmentPosition, generatorAttachmentDirection;

	public List<WorldEventListener> worldEventListeners = new List<WorldEventListener>();


	public WorldManager()
	{
		generator = new LevelGenerator();
	}

	public void destroy()
	{
		hub?.destroy();
		cliffside?.destroy();
		tutorial?.destroy();
		if (areaCaves != null)
		{
			foreach (Level level in areaCaves)
				level.destroy();
		}
		if (areaMines != null)
		{
			foreach (Level level in areaMines)
				level.destroy();
		}
		if (areaDungeons != null)
		{
			foreach (Level level in areaDungeons)
				level.destroy();
		}
		if (areaGardens != null)
		{
			foreach (Level level in areaGardens)
				level.destroy();
		}

		worldEventListeners.Clear();
	}

	public Level entryLevel => seamlessLevel; // areaCaves[0];
	public Vector2 entryLevelSpawn => playerSpawn;

	public Level hubLevel => seamlessLevel; // hub;
	public Vector2 hubLevelSpawn => playerSpawn; // (Vector2)hub.rooms[0].getMarker(10);

	public Level introLevel => cemetary;
	public Vector2 introLevelSpawn => cemetary.getMarker(0x1);

	public void init(string seed, StartingClass startingClass, bool quickRestart)
	{
		generateSeamlessWorld(seed);
		//generateRoguelikeWorld(seed, startingClass, quickRestart);
		//generateRPGWorld();
	}

	void generateSeamlessWorld(string seed)
	{
		Texture worldmap = Resource.GetTexture("level/worldmap.png", false, true);
		Texture markersTexture = Resource.GetTexture("level/worldmap_markers.png", false, true);
		Texture lootVal = Resource.GetTexture("level/worldmap_lootvalue.png", false, true);
		if (worldmap.getImageData(out ImageData worldmapImg) && markersTexture.getImageData(out ImageData markersImg) && lootVal.getImageData(out ImageData lootValuesImg))
		{
			seamlessLevel = new Level(-1, "world", "", worldmapImg.width, worldmapImg.height, null);
			byte[] biomes = new byte[worldmapImg.width * worldmapImg.height];
			float[] lootValues = new float[worldmapImg.width * worldmapImg.height];

			loadWorldmap(worldmapImg, markersImg, lootValuesImg, seamlessLevel, biomes, lootValues);

			playerSpawn = markers[0x1] + new Vector2(0.5f, 0);
			generatorAttachmentPosition = markers[0x2];
			generatorAttachmentDirection = new Vector2i(1, 0);

			seamlessLevel.addEntity(new CavesBossRoom(), (Vector2)markers[0x5]);

			generateBiome(seamlessLevel, biomes, 1, lootValues, seed, new CaveBiomeGenerator(generatorAttachmentPosition, generatorAttachmentDirection));
			//generateBiome(seamlessLevel, biomes, 2, seed, new ProceduralBiomeGenerator());

			worldmapImg.free();
			markersImg.free();
			lootValuesImg.free();
		}
	}

	void generateBiome(Level level, byte[] biomes, int biome, float[] lootValues, string seed, BiomeGenerator generator)
	{
		generator.setup(level, biomes, biome, lootValues, seed);

		for (int y = 0; y < level.height; y++)
		{
			for (int x = 0; x < level.width; x++)
			{
				bool mask = biomes[x + y * level.width] == biome;
				if (mask)
					level.setTile(x, y, generator.getBackgroundTile(x, y));
			}
		}

		generator.generateBaseLevel();

		level.updateLightmap(0, 0, level.width, level.height);
	}

	int countLadderHeight(int x, int y, ImageData image)
	{
		int result = 0;
		while (true)
		{
			uint color = image.getPixel(x, image.height - (y + result) - 1);
			if (color == 0xFF00FF00 || color == 0xFF00FFFF)
				result++;
			else
				break;
		}
		return result;
	}

	void loadWorldmap(ImageData main, ImageData markers, ImageData lootValuesImg, Level level, byte[] biomes, float[] lootValues)
	{
		for (int y = 0; y < main.height; y++)
		{
			for (int x = 0; x < main.width; x++)
			{
				uint pixel = main.getPixel(x, main.height - y - 1);
				uint marker = markers.getPixel(x, markers.height - y - 1);
				uint lootVal = lootValuesImg.getPixel(x, lootValuesImg.height - y - 1);

				if (pixel == 0xFFFFFFFF)
				{
					level.setTile(x, y, TileType.dirt);
				}
				else if (pixel == 0xFF7F7F7F)
				{
					level.setTile(x, y, TileType.stone);
				}
				else if (pixel == 0xFF0000FF || pixel == 0xFF00FFFF)
				{
					level.setTile(x, y, TileType.platform);
				}
				else if (pixel == 0xFF00FF00)
				{
					if (main.getPixel(x, main.height - (y - 1) - 1) != 0xFF00FF00 && main.getPixel(x, main.height - (y - 1) - 1) != 0xFF00FFFF)
						level.addEntity(new Ladder(countLadderHeight(x, y, main)), new Vector2(x, y));
				}
				else if (GetBiomeFromColor(pixel, out Biome biome))
				{
					biomes[x + y * main.width] = (byte)biome;
				}
				else
				{
					Entity entity = EntityType.CreateInstance(pixel);
					if (entity != null)
						level.addEntity(entity, new Vector2(x + 0.5f, y));
				}

				{
					byte r = (byte)((marker & 0x000000FF) >> 0);
					byte g = (byte)((marker & 0x0000FF00) >> 8);
					byte b = (byte)((marker & 0x00FF0000) >> 16);
					if (r == 0xFF && b == 0xFF)
					{
						int id = g;
						this.markers.Add(id, new Vector2i(x, y));
					}
				}


				float lootValue = lootVal & 0x000000FF;
				lootValues[x + y * main.width] = lootValue;
			}
		}

		loadScene("level/worldmap.gltf", level);
	}

	void generateRoguelikeWorld(string seed, StartingClass startingClass, bool quickRestart)
	{
		introBridge = new Level(-1, "intro", "");
		graveyard = new Level(-1, "graveyard", "");
		hub = new Level(-1, "hub", "Hollow's Refuge");
		//tutorial = new Level(-1, "Tutorial");
		cliffside = new Level(-1, "cliffside", "Cliffside");
		tutorial = new Level(-1, "tutorial", "Abandoned Mineshaft");

		//Door tutorialEntrance = new Door(cliffside, null);
		Door tutorialExit = new Door(hub, null);

		Door tutorialExitDoor = new Door(tutorial, tutorialExit);

		tutorialExit.otherDoor = tutorialExitDoor;


		generator.generateHub(hub);
		Door dungeonEntrance = new DungeonGate(null, null, ParallaxObject.ZToLayer(0.15f));
		hub.addEntity(dungeonEntrance, hub.getMarker(0x0b, 0, 0));


		generator.generateCaves(seed, out areaCaves);
		generator.generateMines(seed, out areaMines);
		generator.generateDungeons(seed, out areaDungeons);
		//generator.generateGardens(run.seed, out areaGardens);


		cemetary = new Level(-1, "cemetary", "");
		Level smallCave = new Level(-1, "cemetary_cave", "");
		//hub = new Level(-1, "cemetary_outskirts", "Cemetary Outskirts");
		RoomDefSet cemetarySet = new RoomDefSet("level/graveyard/rooms.png", false);

		generator.generateSingleRoomLevel(cemetary, new Room(cemetarySet, 0), null /*new Room(cemetarySet, 1)*/, TileType.stone, TileType.bricks);
		generator.generateSingleRoomLevel(smallCave, new Room(cemetarySet, 1), null, TileType.stone, TileType.bricks);

		cemetary.addEntity(new Rat(), cemetary.rooms[0].getMarker(0x4) + 0.5f);
		cemetary.addEntity(new Rat(), cemetary.rooms[0].getMarker(0x5) + 0.5f);


		generator.connectDoors(generator.generateDoor(cemetary, 0x2), generator.generateDoor(smallCave, 0x1));
		generator.connectDoors(generator.generateDoor(smallCave, 0x2), generator.generateDoor(cemetary, 0x3));
		generator.connectDoors(cemetary.entrance, hub.entrance);

		generator.connectDoors(dungeonEntrance, areaCaves[0].entrance);
		generator.connectDoors(areaCaves[areaCaves.Length - 1].exit, areaMines[0].entrance);
		generator.connectDoors(areaMines[areaMines.Length - 1].exit, areaDungeons[0].entrance);
		areaDungeons[areaDungeons.Length - 1].exit.finalExit = true;


		// Hub
		{





		}


		//generator.generateIntroBridge(introBridge);
		//generator.generateSingleRoomLevel(graveyard, new Room("level/graveyard/graveyard.png"), null, TileType.bricks, TileType.stone);
		//generator.generateCliffside(cliffside);
		//generator.generateTutorial(tutorial);

		// Intro
		/*
		{
			introBridge.addEntity(new IntroBridge());
			introBridge.bg = Resource.GetTexture("level/cliffside/bg.png", false);
			introBridge.ambientSound = Resource.GetSound("sounds/ambience2.ogg");

			loadScene("level/intro/bridge.gltf", introBridge);
		}
		*/

		// Graveyard
		/*
		{
			graveyard.bg = Resource.GetTexture("level/graveyard/layers/bg.png", false);

			loadScene("level/graveyard/graveyard.gltf", graveyard);
		}
		*/

		// Cliffside
		/*
		Door tutorialDoor;
		{
			//tutorialEntrance.otherDoor = cliffTutorialDoor;

			cliffside.addEntity(new Cliffside(cliffside.rooms[0]));
			cliffside.bg = Resource.GetTexture("level/cliffside/bg.png", false);
			cliffside.ambientSound = Resource.GetSound("sounds/ambience4.ogg");

			tutorialDoor = new TutorialEntranceDoor(tutorial);
			cliffside.addEntity(tutorialDoor, (Vector2)cliffside.rooms[0].getMarker(32));

			//cliffside.addEntity(new TutorialText(InputManager.GetBinding("Interact").ToString(), 0xFFFFFFFF), cliffside.rooms[0].getMarker(32) + new Vector2(0, 1.5f));
		}

		// Tutorial
		{
			tutorial.addEntity(new Tutorial(tutorial.rooms[0]));

			Door cliffsideDoor = new TutorialExitDoor(cliffside, tutorialDoor);
			tutorialDoor.otherDoor = cliffsideDoor;
			tutorial.addEntity(cliffsideDoor, (Vector2)tutorial.rooms[0].getMarker(0x21));
		}

		//generator.connectDoors(areaDungeons[areaDungeons.Length - 1].exit, areaGardens[0].entrance);
		//areaGardens[areaGardens.Length - 1].exit.finalExit = true;
		*/
	}

	void generateRPGWorld(string seed, SaveFile save)
	{
		introBridge = new Level(-1, "intro", "");
		graveyard = new Level(-1, "graveyard", "");
		hub = new Level(-1, "hub", "Hollow's Refuge");
		//tutorial = new Level(-1, "Tutorial");
		cliffside = new Level(-1, "cliffside", "Cliffside");
		tutorial = new Level(-1, "tutorial", "Abandoned Mineshaft");

		//Door tutorialEntrance = new Door(cliffside, null);
		Door tutorialExit = new Door(hub, null);

		Door tutorialExitDoor = new Door(tutorial, tutorialExit);

		tutorialExit.otherDoor = tutorialExitDoor;

		Level cemetary = new Level(-1, "cemetary", "Cemetary");
		Level smallCave = new Level(-1, "cemetary_cave", "");
		Level outskirts = new Level(-1, "cemetary_outskirts", "Cemetary Outskirts");
		RoomDefSet cemetarySet = new RoomDefSet("level/graveyard/rooms.png", false);

		generator.generateSingleRoomLevel(cemetary, new Room(cemetarySet, 0), null /*new Room(cemetarySet, 1)*/, TileType.stone, TileType.bricks);
		generator.generateSingleRoomLevel(smallCave, new Room(cemetarySet, 1), null, TileType.stone, TileType.bricks);
		generator.generateSingleRoomLevel(outskirts, new Room(cemetarySet, 2), null, TileType.dirt, TileType.stone, TileType.grass);

		cemetary.addEntity(new Rat(), cemetary.rooms[0].getMarker(0x4) + 0.5f);
		cemetary.addEntity(new Rat(), cemetary.rooms[0].getMarker(0x5) + 0.5f);

		generateOutskirts(outskirts, save);
		Level[] caves = generateCaves(outskirts);

		generator.connectDoors(generator.generateDoor(cemetary, 0x2), generator.generateDoor(smallCave, 0x1));
		generator.connectDoors(generator.generateDoor(smallCave, 0x2), generator.generateDoor(cemetary, 0x3));
		generator.connectDoors(cemetary.entrance, outskirts.entrance);

		generator.connectDoors(outskirts.exit, caves[0].entrance);


		generator.generateCaves(seed, out areaCaves);
		generator.generateDungeons(seed, out areaDungeons);
		generator.generateMines(seed, out areaMines);
		//generator.generateGardens(run.seed, out areaGardens);


		/*
		if (save.currentCheckpointLevel != null && Level.GetByName(save.currentCheckpointLevel) != null)
		{
			level = null;
			switchLevel(Level.GetByName(save.currentCheckpointLevel), save.currentCheckpoint);
			levelSwitchTime = -1;
		}
		else
		{
			level = null;
			//switchLevel(outskirts, outskirts.getMarker(0x2));
			switchLevel(cemetary, cemetary.getMarker(0x1));
			levelSwitchTime = -1;
		}
		*/
	}

	void generateOutskirts(Level outskirts, SaveFile save)
	{
		outskirts.addEntity(new Checkpoint(), outskirts.rooms[0].getMarker(0x1) + new Vector2(0.5f, 0));
		{
			BrokenWanderer npc = new BrokenWanderer(); // NPCManager.brokenWanderer;
			npc.clearShop();
			outskirts.addEntity(npc, outskirts.rooms[0].getMarker(0x3) + new Vector2(0.5f, 0));
		}

		{
			NPC blacksmith = new Blacksmith(); // NPCManager.blacksmith;
			blacksmith.direction = -1;
			blacksmith.addShopItem(new Torch());
			blacksmith.addShopItem(new Bomb(), 7);
			blacksmith.addShopItem(new IronKey(), 8);
			blacksmith.addShopItem(new ThrowingKnife() { stackSize = 8 }, 1);
			outskirts.addEntity(blacksmith, outskirts.rooms[0].getMarker(0x4) + new Vector2(0.5f, 0));
		}

		//level.addEntity(new IronDoor(save.hasFlag(SaveFile.FLAG_NPC_RAT_MET) ? null : "dummy_key"), new Vector2(38.5f, 23));
		if (save.hasFlag(SaveFile.FLAG_NPC_RAT_MET) && !save.hasFlag(SaveFile.FLAG_NPC_RAT_QUESTLINE_COMPLETED))
		{
			RatNPC rat = new RatNPC(); // NPCManager.rat;
			rat.clearShop();
			rat.direction = 1;
			outskirts.addEntity(rat, (Vector2)outskirts.rooms[0].getMarker(0x0e));

			outskirts.addEntity(new RopeEntity(13), outskirts.rooms[0].getMarker(0x0e) + new Vector2(6, -1));
		}

		if (save.hasFlag(SaveFile.FLAG_CAVES_FOUND) && !save.hasFlag(SaveFile.FLAG_NPC_GATEKEEPER_MET))
		{
			TravellingMerchant gatekeeper = new TravellingMerchant(null, outskirts, 20);
			outskirts.addEntity(gatekeeper, (Vector2)outskirts.rooms[0].getMarker(17));
		}

		if (QuestManager.tryGetQuest("logan", "logan_quest", out Quest loganQuest) && (loganQuest.state == QuestState.InProgress || loganQuest.state == QuestState.Completed))
		{
			outskirts.addEntity(new Logan() /*NPCManager.logan*/, outskirts.rooms[0].getMarker(0x5) + new Vector2(0.5f, 0));
		}

		for (int i = 0; i < save.highscores.Length; i++)
		{
			Vector2 position = outskirts.rooms[0].getMarker(15) + new Vector2(i * 5, 0);
			outskirts.addEntity(new Pedestal(), position);

			if (save.highscores[i].score > 0)
			{
				string[] label =
					i == 0 ? ["Fastest Time:", save.highscores[i].time != -1 ? StringUtils.TimeToString(save.highscores[i].time) : "???"] :
					i == 1 ? ["Highest Score:", save.highscores[i].score.ToString()] :
					i == 2 ? ["Highest Floor:", save.highscores[i].floor != -1 ? (save.highscores[i].floor + 1).ToString() : "???"] :
					i == 3 ? ["Most kills:", save.highscores[i].kills.ToString()] : ["???"];
				uint color = RunStats.recordColors[i];
				outskirts.addEntity(new HighscoreDummy(save.highscores[i], label, color), position + Vector2.Up);
			}
		}

		Door dungeonDoor = new TutorialExitDoor(areaCaves[0], areaCaves[0].entrance)
		{
			sprite = new Sprite(Entity.tileset, 6, 9, 3, 2),
			rect = new FloatRect(-1.5f, 0.0f, 3.0f, 2.0f),
			collider = new FloatRect(-1.5f, 0.0f, 3, 2),
		};
		// new DungeonGate(areaCaves[0], areaCaves[0].entrance, ParallaxObject.ZToLayer(0.15f));
		outskirts.exit = dungeonDoor;
		dungeonDoor.collider = new FloatRect(-1, -2.5f, 2, 2);
		outskirts.addEntity(dungeonDoor, outskirts.getMarker(0x10));
		generator.connectDoors(dungeonDoor, areaCaves[0].entrance);

		//Door castleGate = new CastleGate(null, null);
		//outskirts.addEntity(castleGate, (Vector2)outskirts.rooms[0].getMarker(16));

		generator.connectDoors(areaCaves[areaCaves.Length - 1].exit, areaDungeons[0].entrance);

		generator.connectDoors(areaCaves[areaCaves.Length - 2].rooms[0].doorways[0].door, areaMines[0].entrance);
		generator.connectDoors(areaCaves[areaCaves.Length - 1].rooms[0].doorways[1].door, areaMines[areaMines.Length - 1].exit);

		/*
		Door hubElevator = new Door(null, null);
		outskirts.addEntity(hubElevator, (Vector2)outskirts.rooms[0].getMarker(0x12));
		generator.connectDoors(areaCaves[areaCaves.Length - 1].rooms[0].doorways[0].door, hubElevator);
		hubElevator.locked = true;
		*/
		areaCaves[areaCaves.Length - 1].rooms[0].doorways[1].door.locked = true;

		areaDungeons[areaDungeons.Length - 1].exit.finalExit = true;

		outskirts.bg = Resource.GetTexture("level/hub/bg.png");
	}

	Level[] generateCaves(Level lastLevel)
	{
		Level[] caves = new Level[6];

		RoomDefSet cavesSet = new RoomDefSet("level/caves/rooms.png", false);

		{
			caves[0] = new Level(-1, "caves0", "Cave Entrance");

			Simplex simplex = new Simplex(Hash.hash("abcdfdfdf") + 0, 3);
			Simplex bgSimplex = new Simplex(Hash.hash("abcdfdfdf") + 0, 3);

			generator.generateSingleRoomLevel(caves[0], new Room(cavesSet, 1), null, (int x, int y, int idx) =>
			{
				if (idx == 0)
				{
					float progress = 1 - y / (float)caves[0].height;
					float type = simplex.sample2f(x * 0.05f, y * 0.05f) - progress * 0.4f;
					return type > -0.1f ? TileType.dirt : TileType.stone;
				}
				return TileType.stone;
			},
			0x1, 0, new TutorialExitDoor(null));
			generator.generateCaveBackground(caves[0], bgSimplex, TileType.dirt, TileType.stone);

			int x0 = (int)MathF.Floor(caves[0].entrance.position.x) - 1;
			int y0 = (int)MathF.Floor(caves[0].entrance.position.y + 0.001f);
			for (int y = y0; y < y0 + 2; y++)
			{
				for (int x = x0; x < x0 + 3; x++)
				{
					caves[0].setBGTile(x, y, null);
				}
			}

			generator.connectDoors(caves[0].entrance, lastLevel.exit);

			loadScene("level/caves/caves0_level.gltf", caves[0]);

			caves[0].ambientSound = Resource.GetSound("sounds/ambience.ogg");
			caves[0].ambientLight = new Vector3(1.0f);
		}

		{
			caves[1] = new Level(-1, "caves1", "");

			Simplex simplex = new Simplex(Hash.hash(caves[1].name), 3);
			Simplex bgSimplex = new Simplex(Hash.hash(caves[1].name + "lfdslkjf"), 3);

			generator.generateSingleRoomLevel(caves[1], new Room(cavesSet, 2), null, (int x, int y, int idx) =>
			{
				if (idx == 0)
				{
					float progress = 1 - y / (float)caves[1].height;
					float type = simplex.sample2f(x * 0.05f, y * 0.05f) - progress * 0.4f;
					return type > -0.1f ? TileType.dirt : TileType.stone;
				}
				return TileType.stone;
			}, 0x1);
			generator.generateCaveBackground(caves[1], bgSimplex, TileType.dirt, TileType.stone);

			generator.connectDoors(caves[1].entrance, caves[0].exit);

			loadScene("level/caves/caves1_level.gltf", caves[1]);

			caves[1].ambientSound = Resource.GetSound("sounds/ambience.ogg");
			caves[1].ambientLight = new Vector3(0.3f);
		}

		{
			caves[2] = new Level(-1, "caves2", "");

			Simplex simplex = new Simplex(Hash.hash(caves[2].name), 3);
			Simplex bgSimplex = new Simplex(Hash.hash(caves[2].name + "lfdslkjf"), 3);

			generator.generateSingleRoomLevel(caves[2], new Room(cavesSet, 3), null, (int x, int y, int idx) =>
			{
				if (idx == 0)
				{
					float progress = 1 - y / (float)caves[2].height;
					float type = simplex.sample2f(x * 0.05f, y * 0.05f) - progress * 0.4f;
					return type > -0.1f ? TileType.dirt : TileType.stone;
				}
				return TileType.stone;
			});
			generator.generateCaveBackground(caves[2], bgSimplex, TileType.dirt, TileType.stone);

			generator.connectDoors(caves[2].entrance, caves[1].exit);

			loadScene("level/caves/caves2_level.gltf", caves[2]);

			caves[2].ambientSound = Resource.GetSound("sounds/ambience.ogg");
			caves[2].ambientLight = new Vector3(0.1f);
		}

		{
			caves[3] = new Level(-1, "caves3", "");

			Simplex simplex = new Simplex(Hash.hash(caves[3].name), 3);
			Simplex bgSimplex = new Simplex(Hash.hash(caves[3].name + "lfdslkjf"), 3);

			generator.generateSingleRoomLevel(caves[3], new Room(cavesSet, 4), null, (int x, int y, int idx) =>
			{
				if (idx == 0)
				{
					float progress = 1 - y / (float)caves[3].height;
					float type = simplex.sample2f(x * 0.05f, y * 0.05f) - progress * 0.4f;
					return type > -0.1f ? TileType.dirt : TileType.stone;
				}
				return TileType.stone;
			});
			generator.generateCaveBackground(caves[3], bgSimplex, TileType.dirt, TileType.stone);

			generator.connectDoors(caves[3].entrance, caves[2].exit);

			loadScene("level/caves/caves3_level.gltf", caves[3]);

			caves[3].ambientSound = Resource.GetSound("sounds/ambience.ogg");
			caves[3].ambientLight = new Vector3(0.3f);
		}

		{
			caves[4] = new Level(-1, "caves4", "");

			Simplex simplex = new Simplex(Hash.hash(caves[4].name), 3);
			Simplex bgSimplex = new Simplex(Hash.hash(caves[4].name + "lfdslkjf"), 3);

			generator.generateSingleRoomLevel(caves[4], new Room(cavesSet, 5), null, (int x, int y, int idx) =>
			{
				if (idx == 0)
				{
					float progress = 1 - y / (float)caves[4].height;
					float type = simplex.sample2f(x * 0.05f, y * 0.05f) - progress * 0.4f;
					return type > -0.1f ? TileType.dirt : TileType.stone;
				}
				return TileType.stone;
			});
			generator.generateCaveBackground(caves[4], bgSimplex, TileType.dirt, TileType.stone);

			caves[4].addEntity(caves[4].exit = new Door(null), caves[4].getMarker(0x1, 0.5f));

			generator.connectDoors(caves[4].entrance, caves[3].exit);

			loadScene("level/caves/caves4_level.gltf", caves[4]);

			caves[4].ambientSound = Resource.GetSound("sounds/ambience.ogg");
			caves[4].ambientLight = new Vector3(1.0f);
		}

		{
			caves[5] = new Level(-1, "caves_boss", "");

			Simplex simplex = new Simplex(Hash.hash(caves[5].name), 3);
			Simplex bgSimplex = new Simplex(Hash.hash(caves[5].name + "lfdslkjf"), 3);

			generator.generateSingleRoomLevel(caves[5], new Room(cavesSet, 6), null, (int x, int y, int idx) =>
			{
				if (idx == 0)
				{
					float progress = 1 - y / (float)caves[5].height;
					float type = simplex.sample2f(x * 0.05f, y * 0.05f) - progress * 0.4f;
					return type > -0.1f ? TileType.dirt : TileType.stone;
				}
				return TileType.stone;
			});
			generator.generateCaveBackground(caves[5], bgSimplex, TileType.dirt, TileType.stone);

			//caves[5].addEntity(new CavesBossRoom(caves[5].rooms[0]));

			generator.connectDoors(caves[5].entrance, caves[4].exit);

			//loadScene("level/caves/caves5_level.gltf", caves[5]);

			//caves[5].ambientSound = Resource.GetSound("sounds/ambience.ogg");
			caves[5].ambientLight = new Vector3(1.0f);
		}

		return caves;
	}

	unsafe void loadScene(string path, Level level)
	{
		Model scene = Resource.GetModel(path, false);
		foreach (Node node in scene.skeleton.nodes)
		{
			if (node.name == "tilemap")
				continue;

			if (node.name == "background")
			{
				if (node.meshes.Length == 0)
					continue;

				int meshID = node.meshes[0];
				MeshData* mesh = &scene.scene->meshes[meshID];
				if (mesh->materialID == -1)
					continue;

				MaterialData* material = &scene.scene->materials[mesh->materialID];
				if (material->diffuse == null)
					continue;

				string texturePath = new string((sbyte*)material->diffuse->path);
				texturePath = StringUtils.AbsolutePath(texturePath, path);
				Texture texture = Resource.GetTexture(texturePath, false);
				level.bg = texture;

				continue;
			}

			string name = node.name;
			if (name.StartsWith("object") || name.StartsWith("item"))
			{
				string nodeType = name.Substring(0, name.IndexOf(' '));

				name = name.Substring(name.IndexOf(' ') + 1);
				if (name.Length > 4 && name[name.Length - 4] == '.' && int.TryParse(name.Substring(name.Length - 3), out _))
					name = name.Substring(0, name.Length - 4);
				string[] args = name.Trim().Split(" ");

				if (nodeType == "object")
				{
					if (args[0].StartsWith("type="))
					{
						string type = args[0].Substring(5);
						Entity entity = EntityType.CreateInstance(type);

						for (int i = 1; i < args.Length; i++)
						{
							if (type == "barrel")
							{
								Barrel barrel = entity as Barrel;
								if (args[i].StartsWith("item="))
								{
									barrel.items = [Item.GetItemPrototype(args[i].Substring(5)).copy()];
								}
								else if (args[i].StartsWith("coins="))
								{
									barrel.coins = int.Parse(args[i].Substring(6));
								}
							}
							else if (type == "chest")
							{
								Chest chest = entity as Chest;
								if (args[i].StartsWith("item="))
								{
									chest.items = [Item.GetItemPrototype(args[i].Substring(5)).copy()];
								}
								else if (args[i].StartsWith("coins="))
								{
									chest.coins = int.Parse(args[i].Substring(6));
								}
							}
							else if (entity is NPC)
							{
								NPC npc = entity as NPC;
								if (args[i].StartsWith("direction="))
								{
									npc.direction = int.Parse(args[i].Substring(10));
								}
							}
						}

						level.addEntity(entity, node.transform.translation.xy);
					}
				}

				if (nodeType == "item")
				{
					if (args[0].StartsWith("type="))
					{
						string type = args[0].Substring(5);
						Item item = Item.GetItemPrototype(type);
						ItemEntity entity = new ItemEntity(item.copy());
						level.addEntity(entity, node.transform.translation.xy);
					}
				}
			}

			for (int i = 0; i < node.meshes.Length; i++)
			{
				int meshID = node.meshes[i];
				MeshData* mesh = &scene.scene->meshes[meshID];

				if (mesh->materialID != -1)
				{
					MaterialData* material = &scene.scene->materials[mesh->materialID];
					if (material->diffuse != null)
					{
						string texturePath = new string((sbyte*)material->diffuse->path);
						texturePath = StringUtils.AbsolutePath(texturePath, path);
						Texture texture = Resource.GetTexture(texturePath, false);

						int numSubMeshes = mesh->vertexCount / 4;
						for (int k = 0; k < numSubMeshes; k++)
						{
							Vector2 min = new Vector2(float.MaxValue);
							Vector2 max = new Vector2(float.MinValue);
							float z = 0;
							Vector2 uv0 = new Vector2(float.MaxValue);
							Vector2 uv1 = new Vector2(float.MinValue);
							for (int j = k * 4; j < k * 4 + 4; j++)
							{
								PositionNormalTangent* vertex = &mesh->vertices[j];
								Vector3 position = node.transform.translation + node.transform.scale * vertex->position;
								min = Vector2.Min(min, position.xy);
								max = Vector2.Max(max, position.xy);
								z = position.z;

								Vector2 uv = mesh->texcoords[j];
								uv0 = Vector2.Min(uv0, uv);
								uv1 = Vector2.Max(uv1, uv);
							}

							Vector2 center = (min + max) * 0.5f;
							Vector2 size = max - min;

							ParallaxObject entity = new ParallaxObject();

							int u0 = (int)MathF.Round(uv0.x * texture.width);
							int v0 = (int)MathF.Round(uv0.y * texture.height);
							int w = (int)MathF.Round((uv1.x - uv0.x) * texture.width);
							int h = (int)MathF.Round((uv1.y - uv0.y) * texture.height);
							entity.sprite = new Sprite(texture, u0, v0, w, h);
							entity.rect = new FloatRect(-0.5f * size, size);
							entity.z = z;
							entity.rotation = node.transform.rotation.angle;

							//entity.rect = new FloatRect(-0.5f * size, size);
							level.addEntity(entity, center);
						}
					}
				}
			}
		}
	}

	public void onLevelSwitch(Level newLevel)
	{
		if (/*level == /*tutorial/ cliffside &&*/ newLevel == hub)
			GameState.instance.save.setFlag(SaveFile.FLAG_TUTORIAL_FINISHED);
		else if (areaCaves != null && newLevel == areaCaves[0])
			GameState.instance.save.setFlag(SaveFile.FLAG_CAVES_FOUND);
		else if (areaMines != null && newLevel == areaMines[0])
			GameState.instance.save.setFlag(SaveFile.FLAG_MINES_FOUND);
		else if (areaDungeons != null && newLevel == areaDungeons[0])
			GameState.instance.save.setFlag(SaveFile.FLAG_DUNGEONS_FOUND);
	}
}
