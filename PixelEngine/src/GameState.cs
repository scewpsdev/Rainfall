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

	public bool isPaused = false;

	long particleUpdateDelta;
	long animationUpdateDelta;
	long entityUpdateDelta;

	public RunStats run;
	string seed = null;

	List<Level> cachedLevels = new List<Level>();
	public Level level;

	Level newLevel = null;
	Door newLevelDoor = null;

	public Player player;
	public PlayerCamera camera;


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

		level = new Level(-1, "");
		Level tutorial = new Level(-1, "Tutorial");

		int numFloors = 5;
		Level[] floors = new Level[numFloors];
		for (int i = 0; i < floors.Length; i++)
			floors[i] = new Level(i, "Caves " + StringUtils.ToRoman(i + 1));

		Door tutorialEntrance = new Door(level);
		Door tutorialExit = new Door(level);

		Door tutorialDoor = new Door(tutorial, tutorialEntrance);
		Door tutorialExitDoor = new Door(tutorial, tutorialExit);
		Door dungeonDoor = new Door(floors[0]);
		level.exit = dungeonDoor;

		tutorialEntrance.otherDoor = tutorialDoor;
		tutorialExit.otherDoor = tutorialExitDoor;

		level.addEntity(player = new Player(), new Vector2(15 + 2, 1));
		level.addEntity(camera = new PlayerCamera(player));

		player.money = 4;

		level.addEntity(tutorialDoor, new Vector2(15 + 7.5f, 1));
		level.addEntity(new TutorialText("Tutorial [X]", 0xFFFFFFFF), new Vector2(15 + 7.5f, 3));
		level.addEntity(tutorialExitDoor, new Vector2(15 + 7.5f, 5));
		level.addEntity(dungeonDoor, new Vector2(15 + 4.5f, 1));

		level.addEntity(new Fountain(FountainEffect.Mana), new Vector2(33.5f, 3));

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
		npc.addShopItem(new Bomb());
		npc.addShopItem(new Bomb());
		npc.direction = 1;
		level.addEntity(npc, new Vector2(4.5f, 3));

		//level.addEntity(new Tinkerer(Random.Shared), new Vector2(6.5f, 3));

		generator.generateLobby(level);
		generator.generateTutorial(tutorial);
		tutorial.addEntity(tutorialEntrance, new Vector2(4, tutorial.height - 5));
		tutorial.addEntity(tutorialExit, new Vector2(41, 24));
		tutorial.addEntity(new TutorialText("Arrow Keys to move", 0xFFFFFFFF), new Vector2(10, tutorial.height - 3));
		tutorial.addEntity(new TutorialText("C to jump", 0xFFFFFFFF), new Vector2(14.5f, tutorial.height - 5));
		tutorial.addEntity(new TutorialText("Hold to jump higher", 0xFFFFFFFF), new Vector2(25.5f, tutorial.height - 2));
		tutorial.addEntity(new TutorialText("Down to drop", 0xFFFFFFFF), new Vector2(41, tutorial.height - 4));
		tutorial.addEntity(new TutorialText("Up to climb", 0xFFFFFFFF), new Vector2(43.5f, tutorial.height - 15));
		tutorial.addEntity(new TutorialText("Hug wall to wall jump", 0xFFFFFFFF), new Vector2(18, tutorial.height - 56));
		tutorial.addEntity(new Chest(new Stick()), new Vector2(54, tutorial.height - 40));
		tutorial.addEntity(new TutorialText("X to interact", 0xFFFFFFFF), new Vector2(52, tutorial.height - 37));
		tutorial.addEntity(new TutorialText("Down+X to drop", 0xFFFFFFFF), new Vector2(52, tutorial.height - 37.5f));
		tutorial.addEntity(new TutorialText("Y to attack", 0xFFFFFFFF), new Vector2(43, 19));
		tutorial.addEntity(new TutorialText("F to use item", 0xFFFFFFFF), new Vector2(43, 26));
		tutorial.addEntity(new TutorialText("V to switch item", 0xFFFFFFFF), new Vector2(43, 25.5f));

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

		Level lastLevel = level;
		for (int i = 0; i < floors.Length; i++)
		{
			generator.run(run.seed, i, floors[i], i < floors.Length - 1 ? floors[i + 1] : null, lastLevel);
			lastLevel = floors[i];
		}

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

		for (int i = 0; i < level.entities.Count; i++)
		{
			level.entities[i].onLevelSwitch(false);
		}
	}

	public override void init()
	{
	}

	public override void destroy()
	{
		level.destroy();
		for (int i = 0; i < cachedLevels.Count; i++)
		{
			cachedLevels[i].destroy();
		}
	}

	public void switchLevel(Level newLevel, Door door)
	{
		this.newLevel = newLevel;
		this.newLevelDoor = door;
	}

	public override void update()
	{
		if (!isPaused && InputManager.IsPressed("UIQuit") && player.numOverlaysOpen == 0)
		{
			isPaused = true;
			PauseMenu.OnPause();
		}
		else if (isPaused && InputManager.IsPressed("UIQuit"))
		{
			isPaused = false;
			PauseMenu.OnUnpause();
		}
		if (isPaused)
			return;

		run.update();

		if (newLevel != null)
		{
			for (int i = 0; i < level.entities.Count; i++)
			{
				level.entities[i].onLevelSwitch(true);
			}

			cachedLevels.Add(level);
			level.removeEntity(player);
			level.removeEntity(camera);

			if (cachedLevels.Contains(newLevel))
				cachedLevels.Remove(newLevel);
			newLevel.addEntity(player, newLevelDoor.position, false);
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
			PauseMenu.Render(this);
		}
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
