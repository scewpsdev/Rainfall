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
	public string seed;
	public float duration = 0.0f;
	public int floor = 0;
	public int kills = 0;
	public int chestsOpened = 0;
	public int stepsWalked = 0;
	public int hitsTaken = 0;

	public bool active = true;

	public Entity killedBy;
	public string killedByName;
	public long endedTime = -1;
	public bool hasWon = false;


	public RunStats(string seed)
	{
		this.seed = seed;
	}

	public void update()
	{
		if (active)
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


	public static GameState instance;

	public bool consoleOpen = false;

	public bool isPaused = false;

	long particleUpdateDelta;
	long animationUpdateDelta;
	long entityUpdateDelta;

	public RunStats run;
	string seed = null;

	public Level hub;
	public Level[] floors;
	List<Level> cachedLevels = new List<Level>();
	public Level level;

	Level newLevel = null;
	Vector2 newLevelSpawnPosition;

	public Player player;
	public PlayerCamera camera;

	Sound ambientSound;
	uint ambientSource;


	public GameState(string seed)
	{
		instance = this;
		this.seed = seed;

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
		run = new RunStats(seed != null ? seed : Hash.hash(Time.timestamp).ToString());

		LevelGenerator generator = new LevelGenerator();

		hub = new Level(-1, "The Glade");
		Level tutorial = new Level(-1, "Tutorial");

		int numFloors = 5;
		floors = new Level[numFloors];
		for (int i = 0; i < floors.Length; i++)
			floors[i] = new Level(i, "Caves " + StringUtils.ToRoman(i + 1));

		Door tutorialEntrance = new Door(hub);
		Door tutorialExit = new Door(hub);

		Door tutorialDoor = new Door(tutorial, tutorialEntrance);
		Door tutorialExitDoor = new Door(tutorial, tutorialExit);
		Door dungeonDoor = new Door(floors[0]);
		hub.exit = dungeonDoor;

		tutorialEntrance.otherDoor = tutorialDoor;
		tutorialExit.otherDoor = tutorialExitDoor;

		player = new Player();
		camera = new PlayerCamera(player);

		player.money = 4;

		hub.addEntity(tutorialDoor, new Vector2(15 + 16 + 7.5f, 2));
		hub.addEntity(new TutorialText("Tutorial [X]", 0xFFFFFFFF), new Vector2(15 + 16 + 7.5f, 4));
		hub.addEntity(tutorialExitDoor, new Vector2(15 + 16 + 7.5f, 6));
		hub.addEntity(dungeonDoor, new Vector2(15 + 4.5f, 4));

		hub.addEntity(new Fountain(FountainEffect.Mana), new Vector2(31.5f, 2));

		BuilderMerchant npc = new BuilderMerchant(Random.Shared);
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

		//hub.addEntity(new Golem(), new Vector2(20, 1));

		generator.generateLobby(hub);
		generator.generateTutorial(tutorial);
		tutorial.addEntity(tutorialEntrance, new Vector2(4, tutorial.height - 5));
		tutorial.addEntity(tutorialExit, new Vector2(41, 24));
		tutorial.addEntity(new TutorialText("WASD to move", 0xFFFFFFFF), new Vector2(10, tutorial.height - 3));
		tutorial.addEntity(new TutorialText(InputManager.GetBinding("Jump").ToString() + " to jump", 0xFFFFFFFF), new Vector2(14.5f, tutorial.height - 4.5f));
		tutorial.addEntity(new TutorialText("Hold to jump higher", 0xFFFFFFFF), new Vector2(25.5f, tutorial.height - 2));
		tutorial.addEntity(new TutorialText("Down to drop", 0xFFFFFFFF), new Vector2(41, tutorial.height - 4));
		tutorial.addEntity(new TutorialText("Up to climb", 0xFFFFFFFF), new Vector2(43.5f, tutorial.height - 15));
		tutorial.addEntity(new TutorialText("Hug wall to wall jump", 0xFFFFFFFF), new Vector2(18, tutorial.height - 56));
		tutorial.addEntity(new Chest(new Stick()), new Vector2(54, tutorial.height - 40));
		tutorial.addEntity(new TutorialText(InputManager.GetBinding("Interact").ToString() + " to interact", 0xFFFFFFFF), new Vector2(50, tutorial.height - 36.5f));
		tutorial.addEntity(new TutorialText("Down+" + InputManager.GetBinding("Interact").ToString() + " to drop", 0xFFFFFFFF), new Vector2(50, tutorial.height - 37.0f));
		tutorial.addEntity(new TutorialText(InputManager.GetBinding("Attack").ToString() + " to attack", 0xFFFFFFFF), new Vector2(46, 21));
		tutorial.addEntity(new TutorialText(InputManager.GetBinding("UseItem").ToString() + " to use item", 0xFFFFFFFF), new Vector2(43, 26));
		tutorial.addEntity(new TutorialText(InputManager.GetBinding("SwitchItem").ToString() + " to switch item", 0xFFFFFFFF), new Vector2(43, 25.5f));

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

		tutorial.addEntity(new Chest(new PotionOfHealing(), new Rope()), new Vector2(43, 24));
		tutorial.addEntity(new Rat(), new Vector2(42, 17));
		tutorial.addEntity(new Rat(), new Vector2(50, 19));
		tutorial.addEntity(new Rat(), new Vector2(48, 23));
		//tutorial.addEntity(new Snake(), new Vector2(50, 19));
		//tutorial.addEntity(new Spider(), new Vector2(48, 23));
		//tutorial.addEntity(new Bat(), new Vector2(48, 24));

		Level lastLevel = hub;
		for (int i = 0; i < floors.Length; i++)
		{
			bool darkLevel = i == 2 || i == 3;
			bool startingRoom = i == 0;
			bool _bossRoom = i == floors.Length - 1;
			level = floors[i];
			generator.run(run.seed, i, darkLevel, startingRoom, _bossRoom, floors[i], i < floors.Length - 1 ? floors[i + 1] : null, lastLevel);
			lastLevel = floors[i];
		}

		Door hubDungeonDoor1 = new Door(lastLevel, lastLevel.exit);
		hub.addEntity(hubDungeonDoor1, new Vector2(8, 19));
		lastLevel.exit.destination = hub;
		lastLevel.exit.otherDoor = hubDungeonDoor1;

		Tinkerer hubMerchant2 = new Tinkerer(new Random((int)Hash.hash(run.seed)));
		hub.addEntity(hubMerchant2, new Vector2(30.5f, 19));

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
		switchLevel(hub, new Vector2(10, 2));

		/*
		level = hub;
		for (int i = 0; i < hub.entities.Count; i++)
		{
			hub.entities[i].onLevelSwitch(false);
		}
		*/

		ambientSound = Resource.GetSound("res/sounds/ambience.ogg");
		ambientSource = Audio.PlayBackground(ambientSound);
		Audio.SetSourceLooping(ambientSource, true);
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

		run.update();

		if (newLevel != null)
		{
			if (level != null)
			{
				for (int i = 0; i < level.entities.Count; i++)
				{
					level.entities[i].onLevelSwitch(true);
				}

				cachedLevels.Add(level);
				level.removeEntity(player);
				level.removeEntity(camera);
			}

			if (cachedLevels.Contains(newLevel))
				cachedLevels.Remove(newLevel);
			newLevel.addEntity(player, newLevelSpawnPosition, false);
			newLevel.addEntity(camera, false);

			camera.position = player.position;

			if (newLevel.floor > run.floor)
				run.floor = newLevel.floor;

			level = newLevel;
			newLevel = null;

			for (int i = 0; i < level.entities.Count; i++)
			{
				level.entities[i].onLevelSwitch(false);
			}

			player.hud.onLevelSwitch(level.name);
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

		if (run.endedTime != -1 && (Time.currentTime - run.endedTime) / 1e9f >= GAME_OVER_SCREEN_DELAY)
		{
			GameOverScreen.Render();

			if (InputManager.IsPressed("Interact"))
				reset();
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
