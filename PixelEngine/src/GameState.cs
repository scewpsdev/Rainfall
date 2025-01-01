using Microsoft.Win32;
using Rainfall;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


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

	public const float GAME_OVER_SCREEN_DELAY = 3.0f;

	const float LEVEL_FADE = 0.5f;


	public static GameState instance;


	public SaveFile save;
	public RunStats run;
	string seed = null;

	//public Level startingCave;
	public Level hub;
	public Level cliffside;
	public Level tutorial;
	public Level[] areaCaves;
	public Level[] areaDungeons;
	public Level[] areaGardens;

	List<Level> cachedLevels = new List<Level>();
	public Level level;
	public int firstCaveFloor => 0;
	public int lastCaveFloor => firstCaveFloor + areaCaves.Length - 1;
	public int firstGardenFloor => areaCaves.Length;
	public int lastGardenFloor => firstGardenFloor + areaGardens.Length - 1;

	Level newLevel = null;
	Vector2 newLevelSpawnPosition;
	long levelSwitchTime = -1;

	public Player player;
	public PlayerCamera camera;

	uint ambientSource;
	public Sound ambience;

	public Mob currentBoss;
	public float currentBossMaxHealth;

	public bool isPaused = false;
	public bool consoleOpen = false;
	public bool onscreenPrompt = false;

	long particleUpdateDelta;
	long animationUpdateDelta;
	long entityUpdateDelta;


	public GameState(int saveID, string seed, bool customRun = false, bool dailyRun = false)
	{
		this.seed = seed;
		instance = this;

		save = customRun ? SaveFile.customRun : dailyRun ? SaveFile.dailyRun : SaveFile.Load(saveID);

		reset();
	}

	void reset(StartingClass startingClass = null, bool quickRestart = false)
	{
		level?.destroy();
		level = null;

		currentBoss = null;

		for (int i = 0; i < cachedLevels.Count; i++)
		{
			cachedLevels[i].destroy();
		}
		cachedLevels.Clear();

		run = new RunStats(seed != null ? seed : Hash.hash(Time.timestamp).ToString(), seed != null);
		save.onReset();

		LevelGenerator generator = new LevelGenerator();

		hub = new Level(-1, "Hollow's Refuge");
		//tutorial = new Level(-1, "Tutorial");
		cliffside = new Level(-1, "Cliffside");
		tutorial = new Level(-1, "Abandoned Mineshaft");

		//Door tutorialEntrance = new Door(cliffside, null);
		Door tutorialExit = new Door(hub, null);

		Door tutorialExitDoor = new Door(tutorial, tutorialExit);

		tutorialExit.otherDoor = tutorialExitDoor;

		player = new Player();
		camera = new PlayerCamera(player);


		// Cliffside
		Door tutorialDoor;
		{
			//tutorialEntrance.otherDoor = cliffTutorialDoor;

			generator.generateCliffside(cliffside);
			cliffside.addEntity(new Cliffside(cliffside.rooms[0]));
			cliffside.bg = Resource.GetTexture("level/cliffside/bg.png", false);
			cliffside.ambientSound = Resource.GetSound("sounds/ambience4.ogg");

			tutorialDoor = new TutorialEntranceDoor(tutorial);
			cliffside.addEntity(tutorialDoor, (Vector2)cliffside.rooms[0].getMarker(32));

			//cliffside.addEntity(new TutorialText(InputManager.GetBinding("Interact").ToString(), 0xFFFFFFFF), cliffside.rooms[0].getMarker(32) + new Vector2(0, 1.5f));
		}

		// Tutorial
		{
			generator.generateTutorial(tutorial);
			tutorial.addEntity(new Tutorial(tutorial.rooms[0]));

			Door cliffsideDoor = new TutorialExitDoor(cliffside, tutorialDoor);
			tutorialDoor.otherDoor = cliffsideDoor;
			tutorial.addEntity(cliffsideDoor, (Vector2)tutorial.rooms[0].getMarker(0x21));
		}


		generator.generateHub(hub);

		hub.addEntity(new Hub(hub.rooms[0]));


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


		generator.generateCaves(run.seed, out areaCaves);

		Door dungeonDoor = new DungeonGate(areaCaves[0], areaCaves[0].entrance, ParallaxObject.ZToLayer(0.15f));
		dungeonDoor.collider = new FloatRect(-1, -2.5f, 2, 2);
		hub.addEntity(dungeonDoor, (Vector2)hub.rooms[0].getMarker(11));
		areaCaves[0].entrance.destination = hub;
		areaCaves[0].entrance.otherDoor = dungeonDoor;

		Door castleGate = new CastleGate(null, null);
		castleGate.finalExit = true;
		hub.addEntity(castleGate, (Vector2)hub.rooms[0].getMarker(16));

		Door cliffDungeonExit1 = new Door(areaCaves[areaCaves.Length - 1], areaCaves[areaCaves.Length - 1].exit, true);
		cliffside.addEntity(cliffDungeonExit1, (Vector2)cliffside.rooms[0].getMarker(35));

		generator.generateDungeons(run.seed, out areaDungeons);
		areaCaves[areaCaves.Length - 1].exit.destination = areaDungeons[0];
		areaCaves[areaCaves.Length - 1].exit.otherDoor = areaDungeons[0].entrance;
		areaDungeons[0].entrance.destination = areaCaves[areaCaves.Length - 1];
		areaDungeons[0].entrance.otherDoor = areaCaves[areaCaves.Length - 1].exit;

		Door hubElevator = new Door(null, null);
		hub.addEntity(hubElevator, (Vector2)hub.rooms[0].getMarker(0x12));

		areaDungeons[areaDungeons.Length - 1].exit.destination = hub;
		areaDungeons[areaDungeons.Length - 1].exit.otherDoor = hubElevator;


		/*
		areaGardens = generator.generateGardens(run.seed);

		Door cliffDungeonEntrance2 = new Door(areaGardens[0], areaGardens[0].entrance, true);
		cliffside.addEntity(cliffDungeonEntrance2, (Vector2)cliffside.rooms[0].getMarker(37));
		areaGardens[0].entrance.destination = cliffside;
		areaGardens[0].entrance.otherDoor = cliffDungeonEntrance2;

		areaGardens[areaGardens.Length - 1].exit.finalExit = true;
		*/


		if (save.isDaily)
		{
			level = null;
			switchLevel(areaCaves[0], areaCaves[0].entrance.position);
			player.setStartingClass(StartingClass.startingClasses[Hash.hash(seed) % StartingClass.startingClasses.Length]);
			levelSwitchTime = -1;
		}
		else if (quickRestart && save.hasFlag(SaveFile.FLAG_TUTORIAL_FINISHED))
		{
			level = null;
			switchLevel(areaCaves[0], areaCaves[0].entrance.position);
			if (startingClass != null)
				player.setStartingClass(startingClass);
			else
			{
				player.money = 8;
				//player.items.Add(new TravellingCloak());
				//player.passiveItems.Add(player.items[0]);
			}
			levelSwitchTime = -1;
		}
		else if (save.hasFlag(SaveFile.FLAG_TUTORIAL_FINISHED))
		{
			level = null;
			Vector2 spawnPosition = (Vector2)hub.rooms[0].getMarker(10);
			switchLevel(hub, spawnPosition);
			levelSwitchTime = -1;

			player.money = 8;
			//player.items.Add(new TravellingCloak());
			//player.passiveItems.Add(player.items[0]);
		}
		else
		{
			level = null;
			switchLevel(cliffside, (Vector2)cliffside.rooms[0].getMarker(0x22));
			levelSwitchTime = -1;

			player.actions.queueAction(new UnconciousAction());
		}

		//switchLevel(areaGardens[2], areaGardens[2].entrance.position);
		//player.giveItem(new Waraxe());
	}

	public override void init()
	{
	}

	public override void destroy()
	{
		if (save.id != -1)
			SaveFile.Save(save);

		Audio.StopSource(ambientSource);

		level.destroy();
		for (int i = 0; i < cachedLevels.Count; i++)
		{
			cachedLevels[i].destroy();
		}
	}

	public void setAmbience(Sound ambience)
	{
		if (ambientSource != 0)
		{
			Audio.FadeoutSource(ambientSource, 2);
			ambientSource = 0;
		}
		if (ambience != null)
			ambientSource = Audio.PlayBackground(ambience, 0.6f, 1, true, 2);
		this.ambience = ambience;
	}

	public void switchLevel(Level newLevel, Vector2 spawnPosition)
	{
		if (level == /*tutorial*/ cliffside && newLevel == hub)
			save.setFlag(SaveFile.FLAG_TUTORIAL_FINISHED);
		else if (newLevel == areaCaves[0])
			save.setFlag(SaveFile.FLAG_CAVES_FOUND);
		else if (newLevel == areaDungeons[0])
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

		Time.paused = isPaused || onscreenPrompt;

		run.update(isPaused || onscreenPrompt);
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
			newLevel.addEntity(player, newLevelSpawnPosition, level == null);
			newLevel.addEntity(camera, level == null);

			camera.position = player.position;

			if (newLevel.floor > run.floor)
			{
				run.floor = newLevel.floor;
				if (newLevel.name != null && newLevel.name != "")
					run.areaName = newLevel.name;
			}

			level = newLevel;
			newLevel = null;

			player.hud.onLevelSwitch(level.name);

			setAmbience(level.ambientSound);
		}

		if (!isPaused && !onscreenPrompt && newLevel == null && !(run.endedTime != -1 && (Time.currentTime - run.endedTime) / 1e9f >= GAME_OVER_SCREEN_DELAY))
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

		if (!run.active)
		{
			if (run.endedTime != -1 && (Time.currentTime - run.endedTime) / 1e9f >= GAME_OVER_SCREEN_DELAY)
			{
				GameOverScreen.Render();

				if (InputManager.IsPressed("UIConfirm"))
				{
					Audio.PlayBackground(UISound.uiConfirm2);
					GameOverScreen.Destroy();
					reset(player.startingClass, true);
				}
				if (InputManager.IsPressed("UIConfirm2"))
				{
					Audio.PlayBackground(UISound.uiConfirm2);
					GameOverScreen.Destroy();
					reset();
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
