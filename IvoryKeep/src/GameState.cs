using Rainfall;
using System;
using System.Drawing;
using System.Formats.Asn1;


public class RunStats
{
	public static readonly uint[] recordColors = [0xFF5f81cf, 0xFFd0be69, 0xFF6cb859, 0xFFb15848];

	public string seed;
	public float duration = 0.0f;
	public int floor = -1;
	public int kills = 0;
	public int chestsOpened = 0;
	public int stepsWalked = 0;
	public int hitsTaken = 0;
	public int coinsCollected = 0;
	public int levelUps = 0;

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

			result += hasWon ? 10000 : 0;
			result += floor * 100;
			result += kills * 50;
			result += chestsOpened * 17;
			result += stepsWalked * 1;
			result += coinsCollected * 10;
			result += levelUps * 100;

			if (hasWon)
			{
				result += (int)MathF.Round(Math.Min(300.0f / duration, 1) * 5000);
				result += (int)MathF.Round(1.0f / (1 + hitsTaken) * 5000);
			}

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

	public bool isPaused = false;
	public bool consoleOpen = false;
	public bool onscreenPrompt = false;

	//public Level startingCave;
	public Level introBridge;
	public Level graveyard;
	public Level hub;
	public Level hub2;
	public Level cliffside;
	public Level tutorial;
	public Level[] areaCaves;
	public Level[] areaDungeons;
	public Level[] areaMines;
	public Level[] areaGardens;
	public Level finalRoom;

	public Level level;

	public List<WorldEventListener> worldEventListeners = new List<WorldEventListener>();

	public Level newLevel = null;
	Vector2 newLevelSpawnPosition;
	long levelSwitchTime = -1;

	public Player player;
	public PlayerCamera camera;

	public Mob currentBoss { get; private set; }
	public float currentBossMaxHealth;
	public BossRoom currentBossRoom;
	public long bossFightStarted = -1;

	public Item currentlyStashedItem = null;

	public HashSet<string> identifiedPotions = new HashSet<string>();

	long entityUpdateDelta;

	long lastFreezeTime = -1;
	float freezeDuration;


	public GameState(int saveID, string seed, bool customRun = false, bool dailyRun = false)
	{
		instance = this;

		save = customRun ? SaveFile.customRun : dailyRun ? SaveFile.dailyRun : IvoryKeep.instance.saves[saveID];
		//QuestManager.Init(save);
		//NPCManager.Init();

		reset(seed, customRun || dailyRun);
	}

	public GameState(int saveID, string seed, string name)
	{
		instance = this;

		save = SaveFile.Create(saveID, name);
		//QuestManager.Init(save);
		//NPCManager.Init();

		reset(seed, false);
	}

	public void reset(string seed, bool customRun, StartingClass startingClass = null, bool quickRestart = false)
	{
		destroy();

		this.seed = seed;
		this.customRun = customRun;

		seed = seed != null ? seed : Hash.hash(Time.timestamp).ToString(); // "afljaskldfd"; // "abcdfdfdf" /*Hash.hash(Time.timestamp).ToString()*/;

		currentBoss = null;

		run = new RunStats(seed, customRun);

		QuestManager.LoadNPCSaves(save);

		//QuestManager.Init(save);
		//NPCManager.Init();

		generator = new DefaultLevelGenerator(seed);

		player = new Player();
		camera = new PlayerCamera(player);

		generateRoguelikeWorld(seed, startingClass, quickRestart);
		//generateRPGWorld();

		QuestManager.InitNPCSaves(save);
	}

	void generateRoguelikeWorld(string seed, StartingClass startingClass, bool quickRestart)
	{
		introBridge = new Level(-1, "intro", "");
		graveyard = new Level(-1, "graveyard", "");
		hub = new Level(-1, "hub", "Hollow's Refuge");
		hub2 = new Level(-1, "hub2", "");
		//tutorial = new Level(-1, "Tutorial");
		cliffside = new Level(-1, "cliffside", "Cliffside");
		tutorial = new Level(-1, "tutorial", "Abandoned Mineshaft");
		finalRoom = new Level(-1, "final_room", "Hall of Victory");

		//Door tutorialEntrance = new Door(cliffside, null);
		Door tutorialExit = new Door(hub, null);

		Door tutorialExitDoor = new Door(tutorial, tutorialExit);

		tutorialExit.otherDoor = tutorialExitDoor;


		generator.generateSingleRoomLevel(hub, new Room(LevelGenerator.hubSet, 0), null, TileType.dirt, TileType.stone);
		hub.addEntity(new Hub(hub.rooms[0]));
		hub.getEntity<DungeonGate>().layer = ParallaxObject.ZToLayer(0.15f);
		hub.isSafeLevel = true;
		hub.ambientTrack = MainMenuState.menuTrack;
		hub.ambientTrackHasIdleLayer = true;

		generator.generateSingleRoomLevel(hub2, new Room(LevelGenerator.hubSet, 1), null, TileType.dirt, TileType.stone);
		hub2.addEntity(new HubPedestalRoom(hub2.rooms[0]));
		hub2.isSafeLevel = true;

		generator.connectDoors(hub.getEntity<Hub>().pedestalRoomEntrance, hub2.entrance);

		generator.generateSingleRoomLevel(finalRoom, new Room(LevelGenerator.hubSet, 2), null, TileType.bricks, TileType.rock, null, 0, 0x1);

		new CaveGenerator(seed).generateArea(out areaCaves);
		new MineGenerator(seed).generateArea(out areaMines);

		generator.connectDoors(hub.getEntity<DungeonGate>(), areaCaves[0].entrance);
		generator.connectDoors(areaCaves[areaCaves.Length - 1].exit, areaMines[0].entrance);
		generator.connectDoors(areaMines[areaMines.Length - 1].exit, hub.getEntity<HubSpawn>());


		new GardenGenerator(seed).generateArea(out areaGardens);
		new DungeonGenerator(seed).generateArea(out areaDungeons);

		generator.connectDoors(hub.getEntity<CastleGate>(), areaGardens[0].entrance);
		generator.connectDoors(areaGardens[areaGardens.Length - 1].exit, areaDungeons[0].entrance);
		generator.connectDoors(areaDungeons[areaDungeons.Length - 1].exit, finalRoom.entrance);
		finalRoom.exit.finalExit = true;


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
				//player.money = 12;
				player.giveItem(new Dagger());
				player.giveItem(new TravellingCloak());
			}
			levelSwitchTime = -1;
		}
		else if (save.hasFlag(SaveFile.FLAG_TUTORIAL_FINISHED))
		{
			level = null;
			Vector2 spawnPosition = (Vector2)hub.rooms[0].getMarker(10);
			switchLevel(hub, spawnPosition);
			levelSwitchTime = -1;

			//player.money = 12;
			player.giveItem(new Dagger());
			player.giveItem(new TravellingCloak());
		}
		else
		{
			level = null;
			Vector2 spawnPosition = (Vector2)hub.rooms[0].getMarker(10);
			switchLevel(hub, spawnPosition);
			levelSwitchTime = -1;

			player.actions.queueAction(new UnconciousAction());
		}
	}

	public override void destroy()
	{
		if (save != null && save.id != -1)
			SaveFile.Save(save);

		/*
		if (player.level != null)
			player.level.removeEntity(player);
		if (camera.level != null)
			camera.level.removeEntity(camera);*/

		AudioManager.SetAmbience(null);

		//AudioManager.SetAmbientTrack(null, false);

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

		if (player != null)
		{
			player.destroy();
			if (player.level != null)
				player.level.removeEntity(player);
			player = null;

			camera.destroy();
			if (camera.level != null)
				camera.level.removeEntity(camera);
			camera = null;
		}

		currentlyStashedItem = null;

		identifiedPotions.Clear();

		worldEventListeners.Clear();
	}

	public void freeze(float duration)
	{
		lastFreezeTime = Time.timestamp;
		freezeDuration = duration;
	}

	public void switchLevel(Level newLevel, Vector2 spawnPosition)
	{
		if (/*level == /*tutorial/ cliffside &&*/ newLevel == hub)
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

		currentBoss = null;
		bossFightStarted = -1;

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
		Time.timeScale = run.endedTime != -1 && (Time.currentTime - run.endedTime) / 1e9f < 1.0f ? 0.5f : 1.0f;

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

		run.update(isPaused || onscreenPrompt || freeze);
		QuestManager.Update(save);

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
			}

			AudioManager.SetAmbientTrack(newLevel.ambientTrack, newLevel.ambientTrackHasIdleLayer);

			level = newLevel;
			newLevel = null;

			player.hud.onLevelSwitch(level.displayName);

			AudioManager.SetAmbience(level.ambientSound);
		}

		if (!isPaused && !onscreenPrompt && newLevel == null && !(run.endedTime != -1 && (Time.currentTime - run.endedTime) / 1e9f >= 2 * GAME_OVER_SCREEN_DELAY))
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

	public override void onSwitchFrom(State to)
	{
		base.onSwitchFrom(to);
		Time.paused = false;
	}

	public override void draw(GraphicsDevice graphics)
	{
		if (level != null)
			level.render();

		if ((Time.currentTime - levelSwitchTime) / 1e9f < 2 * LEVEL_FADE)
		{
			float fade = 1 - MathF.Abs(1 - (Time.currentTime - levelSwitchTime) / 1e9f / LEVEL_FADE);
			uint color = Mathf.ColorAlpha(0xFF000000, fade);
			Renderer.DrawUISprite(0, 0, Renderer.UIWidth, Renderer.UIHeight, null, false, color);
		}

		if (!run.active)
		{
			if (run.endedTime != -1 && (Time.currentTime - run.endedTime) / 1e9f >= GAME_OVER_SCREEN_DELAY && (player.velocity.lengthSquared < 0.1f || (Time.currentTime - run.endedTime) / 1e9f >= 2 * GAME_OVER_SCREEN_DELAY))
			{
				GameOverScreen.Render();

				if (InputManager.IsPressed("UIConfirm"))
				{
					Console.WriteLine("Won: " + run.hasWon);
					Console.WriteLine("Floor: " + run.floor);
					Console.WriteLine("Kills: " + run.kills);
					Console.WriteLine("Chests opened: " + run.chestsOpened);
					Console.WriteLine("Steps walked: " + run.stepsWalked);
					Console.WriteLine("Coins collected: " + run.coinsCollected);
					Console.WriteLine("Level Ups: " + run.levelUps);
					Console.WriteLine("Time: " + run.duration);
					Console.WriteLine("Hits taken: " + run.hitsTaken);
					Console.WriteLine("Score: " + run.score);

					Audio.PlayBackground(UISound.uiConfirm2);
					GameOverScreen.Destroy();
					if (!customRun)
						SaveFile.Save(save);
					if (!run.hasWon)
						reset(seed, customRun, player.startingClass, true);
				}
				if (InputManager.IsPressed("UIConfirm2"))
				{
					Audio.PlayBackground(UISound.uiConfirm2);
					GameOverScreen.Destroy();
					SaveFile.Save(save);
					if (!run.hasWon)
						reset(seed, customRun);
				}
			}
			else
			{
				//Renderer.DrawUISprite(0, 0, Renderer.UIWidth, Renderer.UIHeight, 0, null, Mathf.ColorAlpha(0xFF000000, MathF.Pow((Time.currentTime - run.endedTime) / 1e9f / GAME_OVER_SCREEN_DELAY, 3)));
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
		if (player != null)
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
