using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GameManager
{
	public Level level;
	public Camera camera;
	public Player player;
	Creature currentBoss;

	LevelGenerator levelGenerator;

	public bool mapUnlocked = false;
	public HashSet<int> exploredRooms = new HashSet<int>();

	bool gameOver = false;


	public GameManager()
	{
		RoomType.Init();

		levelGenerator = new LevelGenerator();
	}

	public void resetGameState()
	{
		level.reset();

		player = new Player(camera, Renderer.graphics);
		player.stats.reset();

		int seed = (int)(Time.currentTime / 1000000);
		if (File.Exists("seed.txt"))
		{
			string seedStr = File.ReadAllText("seed.txt");
			if (seedStr.Length > 0)
				seed = int.Parse(seedStr);
		}

		levelGenerator.reset(seed, level);
		levelGenerator.generateLevel();

		level.addEntity(player.camera);
		level.addEntity(player, level.spawnPoint);
	}

	public void initiateBoss(Creature boss)
	{
		currentBoss = boss;
	}

	public void terminateBoss()
	{
		currentBoss = null;
	}

	public void onDeath()
	{
		gameOver = true;
	}

	public void onSpawn()
	{
		gameOver = false;
	}

	public void update()
	{
		if (currentBoss != null)
		{
			if (currentBoss.isDead)
			{
				player.wonTime = Time.currentTime;
				terminateBoss();
			}
			else if (!player.isAlive)
			{
				currentBoss.stats.health = currentBoss.stats.maxHealth;
				terminateBoss();
			}
		}

		mapUnlocked = player.inventory.findItem(Item.Get("map")) != null;
		int currentRoomID = level.getRoomIDAtPos(player.position);
		exploredRooms.Add(currentRoomID);

		if (gameOver)
		{
			if (InputManager.IsPressed("UIConfirm"))
			{
				resetGameState();
			}
		}
	}

	public void draw()
	{
		if (currentBoss != null)
		{
			int width = Display.width / 8 * 7;
			int height = 16;
			Renderer.DrawUIRect(Display.width / 2 - width / 2 - 2, Display.height - 128 - 2, width + 4, height + 4, 0xFF111111);
			Renderer.DrawUIRect(Display.width / 2 - width / 2, Display.height - 128, (int)(width * currentBoss.stats.health / (float)currentBoss.stats.maxHealth), height, 0xFFFF1111);
			Renderer.DrawText(Display.width / 2 - width / 2, Display.height - 128 - (int)Renderer.promptFont.size - 8, 1, currentBoss.name, Renderer.promptFont, 0xFFAAAAAA);
		}
		if (player.hasWon)
		{
			float timeSinceWin = (Time.currentTime - player.wonTime) / 1e9f;
			float intensity = timeSinceWin < 1 ? timeSinceWin : timeSinceWin > 4 && timeSinceWin < 5 ? 1 - (timeSinceWin - 4) : timeSinceWin >= 1 && timeSinceWin <= 4 ? 1 : 0;
			if (intensity > 0)
			{
				string text = "V I C T O R Y";
				uint color = (uint)(((byte)(intensity * 255) << 24) | 0xCCAA66);
				int width = Renderer.victoryFont.measureText(text);
				Renderer.DrawText(Display.viewportSize.x / 2 - width / 2, Display.viewportSize.y / 2 - (int)Renderer.victoryFont.size / 2, 1.0f, text, Renderer.victoryFont, color);
			}
		}
		if (gameOver)
		{
			drawGameOverScreen();
		}
	}

	void drawGameOverScreen()
	{
		Renderer.DrawUIRect(Display.width / 8, Display.height / 8, Display.width / 8 * 6, Display.height / 8 * 6, 0xFF111111);

		{
			string text = "Ya ded son";
			int width = text.Length * 32;
			int height = 32;
			Renderer.DrawText(Display.width / 2 - width / 2, Display.height / 4 - height / 2, 1, text, Renderer.uiFontMedium, 0xFFAAAAAA);
		}

		int leftBound = Display.width / 8 + 50;
		int rightBound = Display.width / 8 * 7 - 50;
		int yscroll = Display.height / 4 + 100;

		var drawLeftInfo = (string text, uint color) =>
		{
			Renderer.DrawText(leftBound, yscroll, 1, text, Renderer.uiFontMedium, color);
		};
		var drawRightInfo = (string text, uint color) =>
		{
			int width = text.Length * 32;
			Renderer.DrawText(rightBound - width, yscroll, 1, text, Renderer.uiFontMedium, color);
			yscroll += 48;
		};

		/*
		drawLeftInfo("Time survived: ", 0xFFFFFFFF);
		drawRightInfo(((int)timeSurvived).ToString(), 0xFFAAAAAA);

		drawLeftInfo("Enemies killed: ", 0xFFFFFFFF);
		drawRightInfo(enemiesKilled.ToString(), 0xFFAAAAAA);

		drawLeftInfo("Hits taken: ", 0xFFFFFFFF);
		drawRightInfo(hitsTaken.ToString(), 0xFFAAAAAA);

		drawLeftInfo("Bullets fired: ", 0xFFFFFFFF);
		drawRightInfo(bulletsFired.ToString(), 0xFFAAAAAA);

		drawLeftInfo("Points earned: ", 0xFFFFFFFF);
		drawRightInfo(pointsEarned.ToString(), 0xFFAAAAAA);

		drawLeftInfo("Points spent: ", 0xFFFFFFFF);
		drawRightInfo(pointsSpent.ToString(), 0xFFAAAAAA);
		*/

		{
			string text = "[E] Quick Restart";
			int width = text.Length * 32;
			int height = 32;
			Renderer.DrawText(Display.width / 2 - width / 2, Display.height / 16 * 13 - height / 2, 1, text, Renderer.uiFontMedium, 0xFFAAAAAA);
		}
	}
}
