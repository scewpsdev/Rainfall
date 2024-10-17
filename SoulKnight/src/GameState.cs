using Microsoft.Win32;
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


	public SaveFile save;
	public RunStats run;
	string seed = null;

	public Level hub;
	public Level tutorial;
	public Level cliffside;
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
		this.seed = seed;
		instance = this;

		save = SaveFile.Load(saveID);

		reset();
	}

	void reset(StartingClass startingClass = null)
	{
		level?.destroy();
		level = null;

		for (int i = 0; i < cachedLevels.Count; i++)
		{
			cachedLevels[i].destroy();
		}
		cachedLevels.Clear();

		run = new RunStats(seed != null ? seed : Hash.hash(Time.timestamp).ToString(), seed != null);
		save.onReset();

		player = new Player();
		camera = new PlayerCamera(player);

		player.money = 8;


		LevelGenerator generator = new LevelGenerator();

		hub = new Level(-1, "Memory Tree");
		tutorial = new Level(-1, "Tutorial");
		cliffside = new Level(-1, "Cliffside");

		Door tutorialEntrance = new Door(cliffside, null);
		Door tutorialExit = new Door(hub, null);

		//Door tutorialDoor = new Door(tutorial, tutorialEntrance);
		Door tutorialExitDoor = new Door(tutorial, tutorialExit);

		//tutorialEntrance.otherDoor = tutorialDoor;
		tutorialExit.otherDoor = tutorialExitDoor;

		generator.generateHub(hub);

		hub.addEntity(new HubRoom(hub.rooms[0]));

		hub.addEntity(new ParallaxObject(Resource.GetTexture("res/level/hub/parallax1.png", false), 1.0f), new Vector2(hub.width, hub.height) * 0.5f);
		hub.addEntity(new ParallaxObject(Resource.GetTexture("res/level/hub/parallax2.png", false), 0.01f), new Vector2(hub.width, hub.height) * 0.5f);

		//hub.addEntity(tutorialDoor, new Vector2(43.5f + 13, 23));
		//hub.addEntity(new TutorialText("Tutorial [X]", 0xFFFFFFFF), new Vector2(43.5f + 13, 25));
		hub.addEntity(tutorialExitDoor, hub.rooms[0].getMarker(01) + new Vector2(0.5f, 0));

		hub.addEntity(new Fountain(FountainEffect.None), hub.rooms[0].getMarker(11) + new Vector2(7, 0));

		ArmorStand barbarianClass, knightClass, hunterClass, thiefClass, wizardClass, foolClass, devClass;

		hub.addEntity(barbarianClass = new ArmorStand(StartingClass.barbarian), hub.rooms[0].getMarker(10) + new Vector2(-2, 0));
		hub.addEntity(knightClass = new ArmorStand(StartingClass.knight, -1), hub.rooms[0].getMarker(10) + new Vector2(2, 0));
		hub.addEntity(thiefClass = new ArmorStand(StartingClass.thief), hub.rooms[0].getMarker(10) + new Vector2(-3.5f, 0));
		hub.addEntity(hunterClass = new ArmorStand(StartingClass.hunter, -1), hub.rooms[0].getMarker(10) + new Vector2(3.5f, 0));
		hub.addEntity(foolClass = new ArmorStand(StartingClass.fool), hub.rooms[0].getMarker(10) + new Vector2(-5, 0));
		hub.addEntity(wizardClass = new ArmorStand(StartingClass.wizard, -1), hub.rooms[0].getMarker(10) + new Vector2(5, 0));

#if DEBUG
		hub.addEntity(devClass = new ArmorStand(StartingClass.dev, -1), hub.rooms[0].getMarker(10) + new Vector2(6.5f, 0));
#endif

		BuilderMerchant npc = new BuilderMerchant(Random.Shared, hub);
		npc.clearShop();
		//npc.addShopItem(new Stick());
		npc.addShopItem(new Rock());
		npc.addShopItem(new Torch());
		npc.addShopItem(new Bomb());
		npc.addShopItem(new ThrowingKnife() { stackSize = 8 }, 1);
		npc.direction = 1;
		npc.buysItems = false;
		hub.addEntity(npc, (Vector2)hub.rooms[0].getMarker(10) + new Vector2(-20, 0));

		hub.addEntity(new IronDoor(save.hasFlag(SaveFile.FLAG_NPC_RAT_MET) ? null : "dummy_key"), new Vector2(38.5f, 23));
		if (save.hasFlag(SaveFile.FLAG_NPC_RAT_MET) && !save.hasFlag(SaveFile.FLAG_NPC_RAT_QUESTLINE_COMPLETED))
		{
			RatNPC rat = new RatNPC();
			rat.clearShop();
			rat.direction = 1;
			hub.addEntity(rat, (Vector2)hub.rooms[0].getMarker(0x0e));
		}

		for (int i = 0; i < save.highscores.Length; i++)
		{
			Vector2 position = new Vector2(101 + i * 5, 24);
			hub.addEntity(new Pedestal(), position);

			if (save.highscores[i].score > 0)
			{
				string[] label = i == 0 ? ["Highest Score:", save.highscores[i].score.ToString()] :
					i == 1 ? ["Highest Floor:", save.highscores[i].floor != -1 ? (save.highscores[i].floor + 1).ToString() : "???"] :
					i == 2 ? ["Fastest Time:", save.highscores[i].time != -1 ? StringUtils.TimeToString(save.highscores[i].time) : "???"] :
					i == 3 ? ["Most kills:", save.highscores[i].kills.ToString()] : ["???"];
				uint color = RunStats.recordColors[i];
				hub.addEntity(new HighscoreDummy(save.highscores[i], label, color), position + Vector2.Up);
			}
		}

		generator.generateTutorial(tutorial);
		tutorial.addEntity(tutorialEntrance, (Vector2)tutorial.rooms[0].getMarker(33));
		tutorial.addEntity(tutorialExit, (Vector2)tutorial.rooms[0].getMarker(01));

		tutorial.addEntity(new TutorialText(InputManager.GetBinding("Up").ToString() + InputManager.GetBinding("Left").ToString() + InputManager.GetBinding("Down").ToString() + InputManager.GetBinding("Right").ToString() + " to move", 0xFFFFFFFF), new Vector2(10, tutorial.height - 8));
		tutorial.addEntity(new TutorialText(InputManager.GetBinding("Jump").ToString() + " to jump", 0xFFFFFFFF), new Vector2(14.5f, tutorial.height - 9.5f));
		tutorial.addEntity(new TutorialText("Hold to jump higher", 0xFFFFFFFF), new Vector2(25.5f, tutorial.height - 7));
		tutorial.addEntity(new TutorialText(InputManager.GetBinding("Down").ToString() + " to drop", 0xFFFFFFFF), new Vector2(41, tutorial.height - 9));
		tutorial.addEntity(new TutorialText(InputManager.GetBinding("Up").ToString() + " to climb", 0xFFFFFFFF), new Vector2(42, tutorial.height - 20));
		tutorial.addEntity(new TutorialText(InputManager.GetBinding("Sprint").ToString() + " to sprint", 0xFFFFFFFF), (Vector2)tutorial.rooms[0].getMarker(04));

		tutorial.addEntity(new TutorialText("Hug wall to wall jump", 0xFFFFFFFF), (Vector2)tutorial.rooms[0].getMarker(02));

		tutorial.addEntity(new Chest(new Stick(), new IronShield()), (Vector2)tutorial.rooms[0].getMarker(03));
		tutorial.addEntity(new TutorialText(InputManager.GetBinding("Interact").ToString() + " to interact", 0xFFFFFFFF), (Vector2)tutorial.rooms[0].getMarker(03) + new Vector2(0, 3.5f));
		tutorial.addEntity(new TutorialText("Down+" + InputManager.GetBinding("Interact").ToString() + " to drop", 0xFFFFFFFF), (Vector2)tutorial.rooms[0].getMarker(03) + new Vector2(0, 3));
		tutorial.addEntity(new TutorialText(InputManager.GetBinding("Attack").ToString() + " to attack", 0xFFFFFFFF), (Vector2)tutorial.rooms[0].getMarker(03) + new Vector2(-9, 6));
		tutorial.addEntity(new TutorialText(InputManager.GetBinding("Attack2").ToString() + " to block", 0xFFFFFFFF), (Vector2)tutorial.rooms[0].getMarker(03) + new Vector2(-9, 5.5f));
		tutorial.addEntity(new TutorialText(InputManager.GetBinding("UseItem").ToString() + " to use item", 0xFFFFFFFF), (Vector2)tutorial.rooms[0].getMarker(05) + new Vector2(0, 2.5f));
		tutorial.addEntity(new TutorialText(InputManager.GetBinding("SwitchItem").ToString() + " to switch item", 0xFFFFFFFF), (Vector2)tutorial.rooms[0].getMarker(05) + new Vector2(0, 2.0f));
		tutorial.addEntity(new TutorialText(InputManager.GetBinding("Inventory").ToString() + " to open inventory", 0xFFFFFFFF), (Vector2)tutorial.rooms[0].getMarker(05) + new Vector2(0, 1.5f));
		tutorial.addEntity(new Chest(new PotionOfHealing(), new Bomb() { stackSize = 200 }), (Vector2)tutorial.rooms[0].getMarker(05));

		tutorial.addEntity(new Rat() { itemDropChance = 0, coinDropChance = 0, relicDropChance = 0 }, (Vector2)tutorial.rooms[0].getMarker(06));
		tutorial.addEntity(new Rat() { itemDropChance = 0, coinDropChance = 0, relicDropChance = 0 }, (Vector2)tutorial.rooms[0].getMarker(07));
		tutorial.addEntity(new Rat() { itemDropChance = 0, coinDropChance = 0, relicDropChance = 0 }, (Vector2)tutorial.rooms[0].getMarker(08));

		tutorial.addEntity(new ItemGate(), (Vector2)tutorial.rooms[0].getMarker(09));


		// Cliffside
		{
			Door cliffTutorialDoor = new Door(tutorial, tutorialEntrance);
			tutorialEntrance.otherDoor = cliffTutorialDoor;

			generator.generateCliffside(cliffside);
			cliffside.addEntity(cliffTutorialDoor, (Vector2)cliffside.rooms[0].getMarker(32));

			cliffside.addEntity(new TutorialText(InputManager.GetBinding("Interact").ToString(), 0xFFFFFFFF), cliffside.rooms[0].getMarker(32) + new Vector2(0, 1.5f));
		}

		int numCaveFloors = 5;
		int numGardenFloors = 3;

		// Cave area
		{
			areaCaves = new Level[numCaveFloors];
			for (int i = 0; i < areaCaves.Length; i++)
				areaCaves[i] = new Level(i, "Caves " + StringUtils.ToRoman(i + 1));

			Door dungeonDoor = new Door(areaCaves[0], null, true, ParallaxObject.ZToLayer(0.75f));
			dungeonDoor.collider = new FloatRect(-1, -2.5f, 2, 2);
			hub.addEntity(dungeonDoor, (Vector2)hub.rooms[0].getMarker(11));

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

			Door cliffDungeonExit1 = new Door(lastLevel, lastLevel.exit, true);
			cliffside.addEntity(cliffDungeonExit1, (Vector2)cliffside.rooms[0].getMarker(35));
			lastLevel.exit.destination = cliffside;
			lastLevel.exit.otherDoor = cliffDungeonExit1;
		}

		// The Glade
		{
			areaGardens = new Level[numGardenFloors];
			for (int i = 0; i < areaGardens.Length; i++)
				areaGardens[i] = new Level(numCaveFloors + i, "The Glade " + StringUtils.ToRoman(i + 1));

			Door cliffDungeonEntrance2 = new Door(areaGardens[0], null, true);
			cliffside.addEntity(cliffDungeonEntrance2, (Vector2)cliffside.rooms[0].getMarker(37));

			Level lastLevel = cliffside;
			Door lastDoor = cliffDungeonEntrance2;
			for (int i = 0; i < areaGardens.Length; i++)
			{
				bool startingRoom = false;
				bool bossRoom = i == areaGardens.Length - 1;
				int floor = numCaveFloors + i;
				level = areaGardens[i];
				generator.generateGardens(run.seed, floor, startingRoom, bossRoom, areaGardens[i], i < areaGardens.Length - 1 ? areaGardens[i + 1] : null, lastLevel, lastDoor);

				lastLevel = areaGardens[i];
				lastDoor = areaGardens[i].exit;
			}

			lastDoor.finalExit = true;
		}


		if (startingClass != null)
		{
			level = null;
			switchLevel(areaCaves[0], areaCaves[0].entrance.position);
			player.setStartingClass(startingClass);
			levelSwitchTime = -1;
		}
		else if (save.hasFlag(SaveFile.FLAG_TUTORIAL_FINISHED))
		{
			level = null;
			switchLevel(hub, (Vector2)hub.rooms[0].getMarker(10));
			levelSwitchTime = -1;
		}
		else
		{
			level = null;
			switchLevel(cliffside, (Vector2)cliffside.rooms[0].getMarker(34));
			levelSwitchTime = -1;
		}


		/*
		level = new Level(-1, "abc");
		LevelGenerator generator = new LevelGenerator();
		generator.generateCaves("1234567", 0, false, true, false, level, null, null, null);
		switchLevel(level, level.entrance.position);

		for (int i = 0; i < 10; i++)
		{
			Vector2 position = new Vector2(10, 10) + Vector2.Rotate(Vector2.Right, i / 5.0f * 2 * MathF.PI) * 4;
			level.addEntity(new Snake(), position);
		}

		player.giveItem(new Longsword());
		*/
	}

	public override void init()
	{
	}

	public override void destroy()
	{
		SaveFile.Save(save);

		Audio.StopSource(ambientSource);

		level.destroy();
		for (int i = 0; i < cachedLevels.Count; i++)
		{
			cachedLevels[i].destroy();
		}
	}

	public void switchLevel(Level newLevel, Vector2 spawnPosition)
	{
		if (level == tutorial && newLevel == hub)
			save.setFlag(SaveFile.FLAG_TUTORIAL_FINISHED);

		this.newLevel = newLevel;
		this.newLevelSpawnPosition = spawnPosition;
		levelSwitchTime = Time.currentTime;
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

		run.update(isPaused);
		SaveFile.Update(save);

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
				ambientSource = Audio.PlayBackground(level.ambientSound, 0.6f, 1, true, 10);
		}

		if (!isPaused && newLevel == null && !(run.endedTime != -1 && (Time.currentTime - run.endedTime) / 1e9f >= GAME_OVER_SCREEN_DELAY))
		{
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

			if (InputManager.IsPressed("UIConfirm"))
			{
				Audio.PlayBackground(UISound.uiConfirm2);
				GameOverScreen.Destroy();
				reset(player.startingClass);
			}
			if (InputManager.IsPressed("UIConfirm2"))
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
