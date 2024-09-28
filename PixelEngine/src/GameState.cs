using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public class RunStats
{
	public static readonly uint[] recordColors = [0xFFd0be69, 0xFF6cb859, 0xFF5f81cf, 0xFFb15848];

	public string seed;
	public float duration = 0.0f;
	public int floor = 0;
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

	const float GAME_OVER_SCREEN_DELAY = 2.0f;

	const float LEVEL_FADE = 0.5f;


	public static GameState instance;


	public int saveID;
	public RunStats run;
	string seed = null;

	public Level hub;
	public Level[] areaCaves;
	public Level[] areaGardens;
	public Level[] areaMines;
	List<Level> cachedLevels = new List<Level>();
	public Level level;

	Level newLevel = null;
	Vector2 newLevelSpawnPosition;
	long levelSwitchTime = -1;

	public Player player;
	public PlayerCamera camera;

	uint ambientSource;

	public Mob currentBoss;
	public float currentBossMaxHealth;

	public bool isPaused = false;
	public bool consoleOpen = false;

	long particleUpdateDelta;
	long animationUpdateDelta;
	long entityUpdateDelta;


	public GameState(int saveID, string seed)
	{
		this.saveID = saveID;
		this.seed = seed;
		instance = this;

		SaveFile.Load(saveID);

		reset();
	}

	void reset()
	{
		level?.destroy();
		level = null;

		for (int i = 0; i < cachedLevels.Count; i++)
		{
			cachedLevels[i].destroy();
		}
		cachedLevels.Clear();

		//run = new RunStats(12345678);
		run = new RunStats(seed != null ? seed : Hash.hash(Time.timestamp).ToString(), seed != null);

		LevelGenerator generator = new LevelGenerator();

		hub = new Level(-1, "Memoria Tree");
		Level tutorial = new Level(-1, "Tutorial");

		Door tutorialEntrance = new Door(hub, null);
		Door tutorialExit = new Door(hub, null);

		Door tutorialDoor = new Door(tutorial, tutorialEntrance);
		Door tutorialExitDoor = new Door(tutorial, tutorialExit);

		tutorialEntrance.otherDoor = tutorialDoor;
		tutorialExit.otherDoor = tutorialExitDoor;

		player = new Player();
		camera = new PlayerCamera(player);

		player.money = 8;

		generator.generateHub(hub);

		hub.addEntity(new ParallaxObject(Resource.GetTexture("res/level/hub/parallax1.png", false), 1.0f), new Vector2(hub.width, hub.height) * 0.5f);
		hub.addEntity(new ParallaxObject(Resource.GetTexture("res/level/hub/parallax2.png", false), 0.01f), new Vector2(hub.width, hub.height) * 0.5f);

		hub.addEntity(tutorialDoor, hub.getMarker(11) + new Vector2(10.5f, 0));
		hub.addEntity(new TutorialText("Tutorial [X]", 0xFFFFFFFF), hub.getMarker(11) + new Vector2(10.5f, 2));
		hub.addEntity(tutorialExitDoor, hub.getMarker(11) + new Vector2(10.5f, 5));

		hub.addEntity(new Fountain(FountainEffect.Mana), hub.getMarker(11) + new Vector2(5, 0));

		for (int i = 0; i < SaveFile.highscores.Length; i++)
		{
			Vector2 position = new Vector2(101 + i * 5, 24);
			hub.addEntity(new Pedestal(), position);

			if (SaveFile.highscores[i].score > 0)
			{
				string[] label = i == 0 ? ["Highest Score:", SaveFile.highscores[i].score.ToString()] :
					i == 1 ? ["Highest Floor:", SaveFile.highscores[i].floor != -1 ? (SaveFile.highscores[i].floor + 1).ToString() : "???"] :
					i == 2 ? ["Fastest Time:", SaveFile.highscores[i].time != -1 ? StringUtils.TimeToString(SaveFile.highscores[i].time) : "???"] :
					i == 3 ? ["Most kills:", SaveFile.highscores[i].kills.ToString()] : ["???"];
				uint color = RunStats.recordColors[i];
				hub.addEntity(new HighscoreDummy(SaveFile.highscores[i], label, color), position + Vector2.Up);
			}
		}

		BuilderMerchant npc = new BuilderMerchant(Random.Shared, hub);
		npc.clearShop();
#if DEBUG
		npc.addShopItem(new Revolver(), 0);
#endif
		npc.addShopItem(new Stick());
		npc.addShopItem(new Rock());
		//npc.addShopItem(new Rope());
		npc.addShopItem(new Torch());
		npc.addShopItem(new Bomb());
		npc.addShopItem(new ThrowingKnife() { stackSize = 8 }, 1);
		npc.direction = 1;
		hub.addEntity(npc, new Vector2(6.5f, 2));

		//hub.addEntity(new Golem(), new Vector2(20, 5));

		generator.generateTutorial(tutorial);
		tutorial.addEntity(tutorialEntrance, new Vector2(4, tutorial.height - 5));
		tutorial.addEntity(tutorialExit, (Vector2)tutorial.getMarker(01));

		tutorial.addEntity(new TutorialText("WASD to move", 0xFFFFFFFF), new Vector2(10, tutorial.height - 3));
		tutorial.addEntity(new TutorialText(InputManager.GetBinding("Jump").ToString() + " to jump", 0xFFFFFFFF), new Vector2(14.5f, tutorial.height - 4.5f));
		tutorial.addEntity(new TutorialText("Hold to jump higher", 0xFFFFFFFF), new Vector2(25.5f, tutorial.height - 2));
		tutorial.addEntity(new TutorialText("Down to drop", 0xFFFFFFFF), new Vector2(41, tutorial.height - 4));
		tutorial.addEntity(new TutorialText("Up to climb", 0xFFFFFFFF), new Vector2(42, tutorial.height - 15));
		tutorial.addEntity(new TutorialText(InputManager.GetBinding("Sprint").ToString() + " to sprint", 0xFFFFFFFF), (Vector2)tutorial.getMarker(04));

		tutorial.addEntity(new TutorialText("Hug wall to wall jump", 0xFFFFFFFF), (Vector2)tutorial.getMarker(02));

		tutorial.addEntity(new Chest(new Stick()), (Vector2)tutorial.getMarker(03));
		tutorial.addEntity(new TutorialText(InputManager.GetBinding("Interact").ToString() + " to interact", 0xFFFFFFFF), (Vector2)tutorial.getMarker(03) + new Vector2(0, 3.5f));
		tutorial.addEntity(new TutorialText("Down+" + InputManager.GetBinding("Interact").ToString() + " to drop", 0xFFFFFFFF), (Vector2)tutorial.getMarker(03) + new Vector2(0, 3));
		tutorial.addEntity(new TutorialText(InputManager.GetBinding("Attack").ToString() + " to attack", 0xFFFFFFFF), (Vector2)tutorial.getMarker(03) + new Vector2(-9, 6));
		tutorial.addEntity(new TutorialText(InputManager.GetBinding("UseItem").ToString() + " to use item", 0xFFFFFFFF), (Vector2)tutorial.getMarker(05) + new Vector2(0, 2));
		tutorial.addEntity(new TutorialText(InputManager.GetBinding("SwitchItem").ToString() + " to switch item", 0xFFFFFFFF), (Vector2)tutorial.getMarker(05) + new Vector2(0, 1.5f));
		tutorial.addEntity(new Chest(new PotionOfHealing(), new Bomb() { stackSize = 20 }), (Vector2)tutorial.getMarker(05));

		tutorial.addEntity(new Rat(), (Vector2)tutorial.getMarker(06));
		tutorial.addEntity(new Rat(), (Vector2)tutorial.getMarker(07));
		tutorial.addEntity(new Rat(), (Vector2)tutorial.getMarker(08));

		tutorial.addEntity(new ItemGate(), (Vector2)tutorial.getMarker(09));

		// Gode meme
		/*
		tutorial.addEntity(new TutorialText("For Gode ->", 0xFFFFFFFF), new Vector2(55, 25.5f));
		for (int i = 0; i < 50; i++)
			tutorial.addEntity(new SpikeTrap(), new Vector2(67.5f + i, 27.5f));
		tutorial.addEntity(new TutorialText("Das ist eine Spike Trap ->     <-", 0xFFFFFFFF), new Vector2(121.7f, 29.5f));
		tutorial.addEntity(new TutorialText("Ja, man kann sie sehen.", 0xFFFFFFFF), new Vector2(121, 28.5f));
		tutorial.addEntity(new TutorialText("Von denen sollte man nicht", 0xFFFFFFFF), new Vector2(121, 27.5f));
		tutorial.addEntity(new TutorialText("getroffen werden.", 0xFFFFFFFF), new Vector2(121, 27.0f));
		tutorial.addEntity(new TutorialText("Das tut weh.", 0xFFFFFFFF), new Vector2(121, 26.5f));
		tutorial.addEntity(new TutorialText("(:", 0xFFFFFFFF), new Vector2(121, 26.0f));
		tutorial.addEntity(new SpikeTrap(), new Vector2(124.5f, 29.5f));
		*/

		//tutorial.addEntity(new Snake(), new Vector2(50, 19));
		//tutorial.addEntity(new Spider(), new Vector2(48, 23));
		//tutorial.addEntity(new Bat(), new Vector2(48, 24));


		int numCaveFloors = 5;
		int numGardenFloors = 3;

		// Cave area
		{
			areaCaves = new Level[numCaveFloors];
			for (int i = 0; i < areaCaves.Length; i++)
				areaCaves[i] = new Level(i, "Caves " + StringUtils.ToRoman(i + 1));

			Door dungeonDoor = new Door(areaCaves[0], null, true);
			hub.addEntity(dungeonDoor, (Vector2)hub.getMarker(11));

			Level lastLevel = hub;
			Door lastDoor = dungeonDoor;
			for (int i = 0; i < areaCaves.Length; i++)
			{
				bool darkLevel = i == 2 || i == 3;
				bool startingRoom = i == 0;
				bool bossRoom = i == areaCaves.Length - 1;
				level = areaCaves[i];
				generator.generateCaves(run.seed, i, darkLevel, startingRoom, bossRoom, areaCaves[i], i < areaCaves.Length - 1 ? areaCaves[i + 1] : null, lastLevel, lastDoor);

				areaCaves[i].addEntity(new ParallaxObject(Resource.GetTexture("res/level/level1/parallax1.png", false), 2.0f), new Vector2(areaCaves[i].width, areaCaves[i].height) * 0.5f);
				areaCaves[i].addEntity(new ParallaxObject(Resource.GetTexture("res/level/level1/parallax2.png", false), 1.0f), new Vector2(areaCaves[i].width, areaCaves[i].height) * 0.5f);

				lastLevel = areaCaves[i];
				lastDoor = areaCaves[i].exit;
			}

			Door hubDungeonExit1 = new Door(lastLevel, lastLevel.exit, true);
			hub.addEntity(hubDungeonExit1, (Vector2)hub.getMarker(12));
			lastLevel.exit.destination = hub;
			lastLevel.exit.otherDoor = hubDungeonExit1;

			Tinkerer hubMerchant2 = new Tinkerer(new Random((int)Hash.hash(run.seed)), hub);
			hub.addEntity(hubMerchant2, (Vector2)hub.getMarker(12) + new Vector2(-12, 1));
		}

		// The Glade
		{
			areaGardens = new Level[numGardenFloors];
			for (int i = 0; i < areaGardens.Length; i++)
				areaGardens[i] = new Level(numCaveFloors + i, i < numGardenFloors - 1 ? "Lushlands " + StringUtils.ToRoman(i + 1) : "The Glade");

			// TODO different sprite for 2nd level entrance
			Door hubDungeonEntrance2 = new Door(areaGardens[0], null, true);
			hub.addEntity(hubDungeonEntrance2, (Vector2)hub.getMarker(13));

			Level lastLevel = hub;
			Door lastDoor = hubDungeonEntrance2;
			for (int i = 0; i < areaGardens.Length; i++)
			{
				bool startingRoom = false;
				bool bossRoom = i == areaGardens.Length - 1;
				level = areaGardens[i];
				generator.generateGardens(run.seed, i, startingRoom, bossRoom, areaGardens[i], i < areaGardens.Length - 1 ? areaGardens[i + 1] : null, lastLevel, lastDoor);
				lastLevel = areaGardens[i];
				lastDoor = areaGardens[i].exit;
			}

			lastDoor.finalExit = true;
		}

		/*
		// Mines area
		{
			int numMinesFloors = 3;
			areaMines = new Level[numMinesFloors];
			for (int i = 0; i < areaMines.Length; i++)
				areaMines[i] = new Level(i, "Mines " + StringUtils.ToRoman(i + 1));

			Door hubDungeonEntrance2 = new Door(areaMines[0]);
			hub.addEntity(hubDungeonEntrance2, new Vector2(39.5f, 21));

			Level lastLevel = hub;
			Door lastDoor = hubDungeonEntrance2;
			for (int i = 0; i < areaMines.Length; i++)
			{
				bool darkLevel = false;
				bool startingRoom = false;
				bool bossRoom = i == areaMines.Length - 1;
				level = areaMines[i];
				generator.generateMines(run.seed, areaMines.Length + i, darkLevel, startingRoom, bossRoom, areaMines[i], i < areaMines.Length - 1 ? areaMines[i + 1] : null, lastLevel, lastDoor);
				lastLevel = areaMines[i];
				lastDoor = areaMines[i].exit;
			}
		}
		*/


		/*
		Level bossRoom = new Level(-1, null);
		for (int y = 1; y < bossRoom.height - 1; y++)
		{
			for (int x = 1; x < bossRoom.width - 1; x++)
			{
				bossRoom.setTile(x, y, null);
			}
		}
		Door bossRoomEntrance = new Door(lastLevel, lastLevel.exit);
		lastLevel.exit.destination = bossRoom;
		lastLevel.exit.otherDoor = bossRoomEntrance;
		bossRoom.addEntity(bossRoomEntrance, new Vector2(3, 1));
		bossRoom.addEntity(new Door(null) { finalExit = true }, new Vector2(12.5f, 1));
		bossRoom.updateLightmap(0, 0, bossRoom.width, bossRoom.height);
		*/

		/*
		Level finalRoom = new Level(-1, "Thanks for playing");
		for (int y = 1; y < finalRoom.height - 1; y++)
		{
			for (int x = 1; x < finalRoom.width - 1; x++)
			{
				finalRoom.setTile(x, y, null);
			}
		}
		Door finalRoomEntrance = new Door(lastLevel, lastLevel.exit);
		lastLevel.exit.destination = finalRoom;
		lastLevel.exit.otherDoor = finalRoomEntrance;
		finalRoom.addEntity(finalRoomEntrance, new Vector2(3, 1));
		finalRoom.addEntity(new Door(null) { finalExit = true }, new Vector2(12.5f, 1));
		//finalRoom.addEntity(new TutorialText("Thanks for playing", 0xFFFFFFFF), new Vector2(10, 6));
		finalRoom.updateLightmap(0, 0, finalRoom.width, finalRoom.height);
		*/

		level = null;
		switchLevel(hub, (Vector2)hub.getMarker(10));
		levelSwitchTime = -1;

		/*
		level = hub;
		for (int i = 0; i < hub.entities.Count; i++)
		{
			hub.entities[i].onLevelSwitch(false);
		}
		*/
	}

	public override void init()
	{
	}

	public override void destroy()
	{
		Audio.StopSource(ambientSource);

		level.destroy();
		for (int i = 0; i < cachedLevels.Count; i++)
		{
			cachedLevels[i].destroy();
		}
	}

	public void switchLevel(Level newLevel, Vector2 spawnPosition)
	{
		this.newLevel = newLevel;
		this.newLevelSpawnPosition = spawnPosition;
		levelSwitchTime = Time.currentTime;
	}

	public void stopRun(bool hasWon, Entity killedBy = null, string killedByName = null)
	{
		run.active = false;
		run.endedTime = Time.currentTime;
		run.hasWon = hasWon;
		run.killedBy = killedBy;
		run.killedByName = killedByName;

		SaveFile.OnRunFinished(run, saveID);
		GameOverScreen.Init();
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
		if (isPaused)
			return;

		run.update(isPaused);

		if (newLevel != null && (Time.currentTime - levelSwitchTime) / 1e9f >= LEVEL_FADE)
		{
			if (level != null)
			{
				for (int i = 0; i < level.entities.Count; i++)
					level.entities[i].onLevelSwitch(newLevel);

				cachedLevels.Add(level);
				level.removeEntity(player);
				level.removeEntity(camera);
			}

			for (int i = 0; i < newLevel.entities.Count; i++)
				newLevel.entities[i].onLevelSwitch(newLevel);

			if (cachedLevels.Contains(newLevel))
				cachedLevels.Remove(newLevel);
			newLevel.addEntity(player, newLevelSpawnPosition, false);
			newLevel.addEntity(camera, false);

			camera.position = player.position;

			if (newLevel.floor > run.floor)
				run.floor = newLevel.floor;

			level = newLevel;
			newLevel = null;

			player.hud.onLevelSwitch(level.name);

			if (ambientSource != 0)
			{
				Audio.FadeoutSource(ambientSource, 10);
				ambientSource = 0;
			}
			if (level.ambientSound != null)
				ambientSource = Audio.PlayBackground(level.ambientSound, 0.1f, 1, true, 10);
		}

		long beforeParticleUpdate = Time.timestamp;
		//ParticleSystem.Update(Vector3.Zero);
		long afterParticleUpdate = Time.timestamp;
		particleUpdateDelta = afterParticleUpdate - beforeParticleUpdate;

		long beforeAnimationUpdate = Time.timestamp;
		Animator.Update(Matrix.Identity);
		long afterAnimationUpdate = Time.timestamp;
		animationUpdateDelta = afterAnimationUpdate - beforeAnimationUpdate;

		long beforeEntityUpdate = Time.timestamp;
		level.update();
		long afterEntityUpdate = Time.timestamp;
		entityUpdateDelta = afterEntityUpdate - beforeEntityUpdate;
	}

	public override void draw(GraphicsDevice graphics)
	{
		level.render();

		if ((Time.currentTime - levelSwitchTime) / 1e9f < 2 * LEVEL_FADE)
		{
			float fade = 1 - MathF.Abs(1 - (Time.currentTime - levelSwitchTime) / 1e9f / LEVEL_FADE);
			uint color = MathHelper.ColorAlpha(0xFF000000, fade);
			Renderer.DrawUISprite(0, 0, Renderer.UIWidth, Renderer.UIHeight, null, false, color);
		}

		if (run.endedTime != -1 && (Time.currentTime - run.endedTime) / 1e9f >= GAME_OVER_SCREEN_DELAY)
		{
			GameOverScreen.Render();

			if (InputManager.IsPressed("Interact"))
			{
				Audio.PlayBackground(UISound.uiConfirm2);
				GameOverScreen.Destroy();
				reset();
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
	}

	public override void drawDebugStats(int y, byte color, GraphicsDevice graphics)
	{
		Span<byte> str = stackalloc byte[64];

		StringUtils.WriteString(str, "Particle Systems: ");
		StringUtils.AppendInteger(str, ParticleSystem.numParticleSystems);
		graphics.drawDebugText(0, y++, color, str);

		StringUtils.WriteString(str, "Animators: ");
		StringUtils.AppendInteger(str, Animator.numAnimators);
		graphics.drawDebugText(0, y++, color, str);

		StringUtils.WriteString(str, "Particle Update: ");
		StringUtils.AppendFloat(str, (particleUpdateDelta / 1e9f) * 1000, 2);
		StringUtils.AppendString(str, " ms");
		graphics.drawDebugText(0, y++, color, str);

		StringUtils.WriteString(str, "Animation Update: ");
		StringUtils.AppendFloat(str, (animationUpdateDelta / 1e9f) * 1000, 2);
		StringUtils.AppendString(str, " ms");
		graphics.drawDebugText(0, y++, color, str);

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
