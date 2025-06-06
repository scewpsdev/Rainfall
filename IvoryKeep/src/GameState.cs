using Rainfall;
using System;
using System.Drawing;


public class RunStats
{
	public static readonly uint[] recordColors = [0xFF5f81cf, 0xFFd0be69, 0xFF6cb859, 0xFFb15848];

	public string seed;
	public float duration = 0.0f;
	public int floor = -1;
	public string areaName;
	public int kills = 0;
	public int chestsOpened = 0;
	public int stepsWalked = 0;
	public int hitsTaken = 0;

	public bool active = true;
	public bool isCustomRun = false;

	public Entity killedBy;
	public string killedByName;
	public long endedTime = -1;
	public bool hasWon = false;
	public bool scoreRecord, floorRecord, timeRecord, killRecord;


	public RunStats(string seed, bool isCustomRun)
	{
		this.seed = seed;
		this.isCustomRun = isCustomRun;
	}

	public void update(bool paused)
	{
		if (active && !paused)
		{
			duration += Time.deltaTime;
		}
	}

	public int score
	{
		get
		{
			int result = 0;

			result += hasWon ? 5000 : 0;
			result += floor * 1000;
			result += kills * 200;
			result += chestsOpened * 17;
			result += stepsWalked * 5;
			result += (int)(MathF.Exp(-hitsTaken * 0.2f) * duration);

			return result;
		}
	}
}

public class GameState : State
{
	const float AREA_TEXT_DURATION = 7.0f;
	const float AREA_TEXT_FADE = 2.0f;

	public const float GAME_OVER_SCREEN_DELAY = 2.0f;

	const float LEVEL_FADE = 0.5f;


	public static GameState instance;


	public SaveFile save;
	public RunStats run;
	string seed = null;
	bool customRun;
	public LevelGenerator generator;

	//public Level startingCave;
	public Level introBridge;
	public Level graveyard;
	public Level hub;
	public Level cliffside;
	public Level tutorial;
	public Level[] areaCaves;
	public Level[] areaDungeons;
	public Level[] areaMines;
	public Level[] areaGardens;

	public Level level;

	public List<WorldEventListener> worldEventListeners = new List<WorldEventListener>();

	Level newLevel = null;
	Vector2 newLevelSpawnPosition;
	long levelSwitchTime = -1;

	public Player player;
	public PlayerCamera camera;

	uint ambientSource;
	public Sound ambience;

	public Mob currentBoss { get; private set; }
	public float currentBossMaxHealth;
	public BossRoom currentBossRoom;
	public long bossFightStarted = -1;

	public string currentCheckpointLevel;
	public Vector2 currentCheckpoint;

	public bool isPaused = false;
	public bool consoleOpen = false;
	public bool onscreenPrompt = false;

	long entityUpdateDelta;

	long lastFreezeTime = -1;
	float freezeDuration;


	public GameState(int saveID, string seed, bool customRun = false, bool dailyRun = false)
	{
		instance = this;

		save = customRun ? SaveFile.customRun : dailyRun ? SaveFile.dailyRun : SaveFile.Load(saveID);
		QuestManager.Init();
		NPCManager.Init();

		reset(seed, customRun);
	}

	public void reset(string seed, bool customRun, StartingClass startingClass = null, bool quickRestart = false)
	{
		destroy();

		this.seed = seed;
		this.customRun = customRun;

		seed = seed != null ? seed : "12345" /*Hash.hash(Time.timestamp).ToString()*/;

		currentBoss = null;

		run = new RunStats(seed, customRun);

		QuestManager.Init();
		NPCManager.Init();

		generator = new LevelGenerator();

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

		player = new Player();
		camera = new PlayerCamera(player);


		Level cemetary = new Level(-1, "cemetary", "Cemetary");
		Level smallCave = new Level(-1, "cemetary_cave", "");
		Level outskirts = new Level(-1, "cemetary_outskirts", "Cemetary Outskirts");
		RoomDefSet cemetarySet = new RoomDefSet("level/graveyard/rooms.png", false);

		Level caves1 = new Level(-1, "caves1", "Cave Entrance");
		RoomDefSet cavesSet = new RoomDefSet("level/caves/rooms.png", false);

		generator.generateSingleRoomLevel(cemetary, new Room(cemetarySet, 0), null /*new Room(cemetarySet, 1)*/, TileType.stone, TileType.bricks);
		generator.generateSingleRoomLevel(smallCave, new Room(cemetarySet, 1), null, TileType.stone, TileType.bricks);
		generator.generateSingleRoomLevel(outskirts, new Room(cemetarySet, 2), null, TileType.dirt, TileType.stone, TileType.grass);

		generator.generateSingleRoomLevel(caves1, new Room(cavesSet, 0), null, TileType.dirt, TileType.stone);
		generator.generateCaveBackground(caves1, new Simplex(Hash.hash(caves1.name + "fdjflkdjflkj"), 3), TileType.dirt, TileType.stone);

		generator.generateCaves(seed, out areaCaves);
		generator.generateDungeons(seed, out areaDungeons);
		generator.generateMines(seed, out areaMines);
		//generator.generateGardens(run.seed, out areaGardens);

		cemetary.addEntity(new Rat(), cemetary.rooms[0].getMarker(0x4) + 0.5f);
		cemetary.addEntity(new Rat(), cemetary.rooms[0].getMarker(0x5) + 0.5f);

		generateOutskirts(outskirts);
		generateCaves1(caves1, outskirts);

		generator.connectDoors(generator.generateDoor(cemetary, 0x2), generator.generateDoor(smallCave, 0x1));
		generator.connectDoors(generator.generateDoor(smallCave, 0x2), generator.generateDoor(cemetary, 0x3));
		generator.connectDoors(cemetary.entrance, outskirts.entrance);

		generator.connectDoors(outskirts.exit, caves1.entrance);


		{


			/*
			generator.generateHub(hub);


			// Hub
			{
				hub.addEntity(new Hub(hub.rooms[0]));
			}
			*/



		}


		if (currentCheckpointLevel != null)
		{
			level = null;
			switchLevel(Level.GetByName(currentCheckpointLevel), currentCheckpoint);
			levelSwitchTime = -1;
			return;
		}
		else
		{
			level = null;
			switchLevel(outskirts, outskirts.getMarker(0x2));
			levelSwitchTime = -1;
			return;
		}


		generator.generateIntroBridge(introBridge);
		generator.generateSingleRoomLevel(graveyard, new Room("level/graveyard/graveyard.png"), null, TileType.bricks, TileType.stone);
		generator.generateCliffside(cliffside);
		generator.generateTutorial(tutorial);

		// Intro
		{
			introBridge.addEntity(new IntroBridge());
			introBridge.bg = Resource.GetTexture("level/cliffside/bg.png", false);
			introBridge.ambientSound = Resource.GetSound("sounds/ambience2.ogg");

			loadScene("level/intro/bridge.gltf", introBridge);
		}

		// Graveyard
		{
			graveyard.bg = Resource.GetTexture("level/graveyard/layers/bg.png", false);

			loadScene("level/graveyard/graveyard.gltf", graveyard);
		}

		// Cliffside
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


		/*
#if DEBUG
		level = null;
		switchLevel(areaCaves[0], areaCaves[0].entrance.getSpawnPoint());
		levelSwitchTime = -1;
		return;
#else
		*/
		level = null;
		switchLevel(graveyard, (Vector2)graveyard.rooms[0].getMarker(0x22));
		levelSwitchTime = -1;
		return;
		//#endif


		if (save.isDaily)
		{
			level = null;
			switchLevel(areaCaves[0], areaCaves[0].entrance.getSpawnPoint());
			player.setStartingClass(StartingClass.startingClasses[Hash.hash(seed) % StartingClass.startingClasses.Length]);
			levelSwitchTime = -1;
		}
		else if (quickRestart && save.hasFlag(SaveFile.FLAG_TUTORIAL_FINISHED))
		{
			level = null;
			switchLevel(areaCaves[0], areaCaves[0].entrance.getSpawnPoint());
			if (startingClass != null)
				player.setStartingClass(startingClass);
			else
			{
				/*
				Item startingWeapon = Item.CreateRandom(ItemType.Weapon, generator.random, 3);
				player.giveItem(startingWeapon);
				if (startingWeapon.requiredAmmo != null)
				{
					Item ammo = Item.GetItemPrototype(startingWeapon.requiredAmmo).copy();
					ammo.stackSize = 30;
					player.giveItem(ammo);
				}
				*/
				player.money = 8;
			}
			levelSwitchTime = -1;
		}
		else if (save.hasFlag(SaveFile.FLAG_TUTORIAL_FINISHED))
		{
			level = null;
			Vector2 spawnPosition = (Vector2)hub.rooms[0].getMarker(10);
			switchLevel(hub, spawnPosition);
			levelSwitchTime = -1;

			/*
			Item startingWeapon = Item.CreateRandom(ItemType.Weapon, generator.random, 3);
			player.giveItem(startingWeapon);
			if (startingWeapon.requiredAmmo != null)
			{
				Item ammo = Item.GetItemPrototype(startingWeapon.requiredAmmo).copy();
				ammo.stackSize = 30;
				player.giveItem(ammo);
			}
			*/

			player.money = 8;
		}
		else
		{
			level = null;
			switchLevel(cliffside, (Vector2)cliffside.rooms[0].getMarker(0x22));
			levelSwitchTime = -1;

			player.actions.queueAction(new UnconciousAction());
		}
	}

	void generateOutskirts(Level outskirts)
	{
		outskirts.addEntity(new Checkpoint(), outskirts.rooms[0].getMarker(0x1) + new Vector2(0.5f, 0));
		{
			BrokenWanderer npc = NPCManager.brokenWanderer;
			npc.clearShop();
			outskirts.addEntity(npc, outskirts.rooms[0].getMarker(0x3) + new Vector2(0.5f, 0));
		}

		{
			NPC blacksmith = NPCManager.blacksmith;
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
			RatNPC rat = NPCManager.rat;
			rat.clearShop();
			rat.direction = 1;
			outskirts.addEntity(rat, (Vector2)outskirts.rooms[0].getMarker(0x0e));

			outskirts.addEntity(new RopeEntity(13), outskirts.rooms[0].getMarker(0x0e) + new Vector2(6, -1));
		}

		if (GameState.instance.save.hasFlag(SaveFile.FLAG_CAVES_FOUND) && !GameState.instance.save.hasFlag(SaveFile.FLAG_NPC_GATEKEEPER_MET))
		{
			TravellingMerchant gatekeeper = new TravellingMerchant(null, outskirts);
			outskirts.addEntity(gatekeeper, (Vector2)outskirts.rooms[0].getMarker(17));
		}

		if (QuestManager.tryGetQuest("logan", "logan_quest", out Quest loganQuest) && (loganQuest.state == QuestState.InProgress || loganQuest.state == QuestState.Completed))
		{
			level.addEntity(NPCManager.logan, outskirts.rooms[0].getMarker(0x5) + new Vector2(0.5f, 0));
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

		Door dungeonDoor = new DungeonGate(areaCaves[0], areaCaves[0].entrance, ParallaxObject.ZToLayer(0.15f));
		outskirts.exit = dungeonDoor;
		dungeonDoor.collider = new FloatRect(-1, -2.5f, 2, 2);
		outskirts.addEntity(dungeonDoor, outskirts.getMarker(0x2, 0, 2));
		generator.connectDoors(dungeonDoor, areaCaves[0].entrance);

		Door castleGate = new CastleGate(null, null);
		outskirts.addEntity(castleGate, (Vector2)outskirts.rooms[0].getMarker(16));

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

	void generateCaves1(Level caves1, Level lastLevel)
	{
		Door entranceDoor = new TutorialExitDoor(null); // new CaveEntranceDoor(lastLevel, lastLevel.exit);
		caves1.entrance = entranceDoor;
		caves1.addEntity(entranceDoor, caves1.getMarker(0x1, 0.5f));
		generator.connectDoors(entranceDoor, lastLevel.exit);

		caves1.ambientLight = new Vector3(0.3f);
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

	public override void init()
	{
	}

	public override void destroy()
	{
		if (save != null && save.id != -1)
			SaveFile.Save(save);

		if (ambientSource != 0)
			Audio.StopSource(ambientSource);

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

	public void freeze(float duration)
	{
		lastFreezeTime = Time.timestamp;
		freezeDuration = duration;
	}

	public void setAmbience(Sound ambience)
	{
		if (ambientSource != 0)
		{
			Audio.FadeoutSource(ambientSource, 2);
			ambientSource = 0;
		}
		if (ambience != null)
		{
			ambientSource = Audio.PlayBackground(ambience, 0.6f, 1, true, 2);
			Audio.SetInaudibleBehavior(ambientSource, true, false);
			Audio.SetProtect(ambientSource, true);
		}
		this.ambience = ambience;
	}

	public void switchLevel(Level newLevel, Vector2 spawnPosition)
	{
		if (level == /*tutorial*/ cliffside && newLevel == hub)
			save.setFlag(SaveFile.FLAG_TUTORIAL_FINISHED);
		else if (areaCaves != null && newLevel == areaCaves[0])
			save.setFlag(SaveFile.FLAG_CAVES_FOUND);
		else if (areaMines != null && newLevel == areaMines[0])
			save.setFlag(SaveFile.FLAG_MINES_FOUND);
		else if (areaDungeons != null && newLevel == areaDungeons[0])
			save.setFlag(SaveFile.FLAG_DUNGEONS_FOUND);

		this.newLevel = newLevel;
		this.newLevelSpawnPosition = spawnPosition;
		levelSwitchTime = Time.currentTime;

		if (currentBoss != null)
			currentBoss = null;
	}

	public void moveEntityToLevel(Entity entity, Level newLevel)
	{
		entity.level.removeEntity(entity);
		newLevel.addEntity(entity, false);
	}

	public void stopRun(bool hasWon, Entity killedBy = null, string killedByName = null)
	{
		run.active = false;
		run.endedTime = Time.currentTime;
		run.hasWon = hasWon;
		run.killedBy = killedBy;
		run.killedByName = killedByName;

		SaveFile.OnRunFinished(run, save);
		GameOverScreen.Init();
	}

	public void setBoss(Mob boss, BossRoom bossRoom)
	{
		currentBoss = boss;
		currentBossRoom = bossRoom;
		if (boss != null)
		{
			currentBossMaxHealth = boss.health;
			bossFightStarted = Time.currentTime;
		}
		else
		{
			bossFightStarted = -1;
		}
	}

	public override void onKeyEvent(KeyCode key, KeyModifier modifiers, bool down)
	{
#if DEBUG
		if (key == KeyCode.Semicolon && modifiers == KeyModifier.None && down)
		{
			consoleOpen = !consoleOpen;
			if (consoleOpen)
				DebugConsole.OnOpen();
			else
				DebugConsole.OnClose();
			Input.ConsumeKeyEvent(key);
		}
#endif
		if (consoleOpen)
		{
			DebugConsole.OnKeyEvent(key, modifiers, down);
			Input.ConsumeKeyEvent(key);
		}
		else if (isPaused)
			PauseMenu.OnKeyEvent(key, modifiers, down);
	}

	public override void onCharEvent(byte length, uint value)
	{
		char c = (char)value;
		if (c == 'ö')
			return;
		if (consoleOpen)
			DebugConsole.OnCharEvent(c);
	}

	public override void onMouseButtonEvent(MouseButton button, bool down)
	{
		if (isPaused)
			PauseMenu.OnMouseButtonEvent(button, down);
	}

	public override void onGamepadButtonEvent(GamepadButton button, bool down)
	{
		if (isPaused)
			PauseMenu.OnGamepadButtonEvent(button, down);
	}

	public override void update()
	{
		if (!isPaused && InputManager.IsPressed("UIQuit", true) && player.numOverlaysOpen == 0)
		{
			isPaused = true;
			PauseMenu.OnPause();
		}
		else if (isPaused && InputManager.IsPressed("UIQuit", true))
		{
			isPaused = false;
			PauseMenu.OnUnpause();
		}

		bool freeze = lastFreezeTime != -1 && (Time.timestamp - lastFreezeTime) / 1e9f < freezeDuration;

		Time.paused = isPaused || onscreenPrompt || freeze;

		run.update(isPaused || onscreenPrompt);
		QuestManager.Update();

		if (newLevel != null && (Time.currentTime - levelSwitchTime) / 1e9f >= LEVEL_FADE)
		{
			List<Entity> newLevelEntitiesCopy = new List<Entity>(newLevel.entities);

			if (level != null)
			{
				List<Entity> levelEntitiesCopy = new List<Entity>(level.entities);

				for (int i = 0; i < levelEntitiesCopy.Count; i++)
					levelEntitiesCopy[i].onLevelSwitch(newLevel);

				level.removeEntity(player);
				level.removeEntity(camera);
			}

			for (int i = 0; i < newLevelEntitiesCopy.Count; i++)
				newLevelEntitiesCopy[i].onLevelSwitch(newLevel);

			newLevel.addEntity(player, newLevelSpawnPosition, level == null);
			newLevel.addEntity(camera, level == null);

			camera.position = player.position;

			if (newLevel.floor > run.floor)
			{
				run.floor = newLevel.floor;
				if (newLevel.displayName != null && newLevel.displayName != "")
					run.areaName = newLevel.displayName;
			}

			level = newLevel;
			newLevel = null;

			player.hud.onLevelSwitch(level.displayName);

			setAmbience(level.ambientSound);
		}

		if (!isPaused && !onscreenPrompt && newLevel == null && !(run.endedTime != -1 && (Time.currentTime - run.endedTime) / 1e9f >= GAME_OVER_SCREEN_DELAY))
		{
			if (!freeze)
			{
				long beforeEntityUpdate = Time.timestamp;
				level.update();
				long afterEntityUpdate = Time.timestamp;
				entityUpdateDelta = afterEntityUpdate - beforeEntityUpdate;
			}
		}
	}

	public override void draw(GraphicsDevice graphics)
	{
		if (level != null)
			level.render();

		if ((Time.currentTime - levelSwitchTime) / 1e9f < 2 * LEVEL_FADE)
		{
			float fade = 1 - MathF.Abs(1 - (Time.currentTime - levelSwitchTime) / 1e9f / LEVEL_FADE);
			uint color = MathHelper.ColorAlpha(0xFF000000, fade);
			Renderer.DrawUISprite(0, 0, Renderer.UIWidth, Renderer.UIHeight, null, false, color);
		}

		if (!run.active)
		{
			if (run.endedTime != -1 && (Time.currentTime - run.endedTime) / 1e9f >= GAME_OVER_SCREEN_DELAY)
			{
				GameOverScreen.Render();

				if (InputManager.IsPressed("UIConfirm"))
				{
					Audio.PlayBackground(UISound.uiConfirm2);
					GameOverScreen.Destroy();
					reset(seed, customRun, player.startingClass, true);
				}
				if (InputManager.IsPressed("UIConfirm2"))
				{
					Audio.PlayBackground(UISound.uiConfirm2);
					GameOverScreen.Destroy();
					reset(seed, customRun);
				}
			}
			else
			{
				//Renderer.DrawUISprite(0, 0, Renderer.UIWidth, Renderer.UIHeight, 0, null, MathHelper.ColorAlpha(0xFF000000, MathF.Pow((Time.currentTime - run.endedTime) / 1e9f / GAME_OVER_SCREEN_DELAY, 3)));
			}
		}

		if (isPaused)
		{
			if (!PauseMenu.Render(this))
			{
				isPaused = false;
				PauseMenu.OnUnpause();
			}
		}

		if (consoleOpen)
			DebugConsole.Render();

#if DEBUG
		Renderer.DrawUITextBMP(0, Renderer.UIHeight - 8, "x: " + player.position.x.ToString("0.0") + ", y: " + player.position.y.ToString("0.0"));
#endif
	}

	public override void drawDebugStats(int y, byte color, GraphicsDevice graphics)
	{
		Span<byte> str = stackalloc byte[64];

		StringUtils.WriteString(str, "Entity Update: ");
		StringUtils.AppendFloat(str, (entityUpdateDelta / 1e9f) * 1000, 2);
		StringUtils.AppendString(str, " ms");
		graphics.drawDebugText(0, y++, color, str);

		y++;

		StringUtils.WriteString(str, "Grounded = ");
		StringUtils.AppendBool(str, player.isGrounded);
		graphics.drawDebugText(0, y++, color, str);
	}
}
