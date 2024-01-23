using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GameManager
{
	const float WAVE_BEGINNING_DELAY = 5.0f;


	public Level level;
	List<Vector2i> spawnPoints = new List<Vector2i>();
	List<Shop> shops = new List<Shop>();

	public Player player;

	public int upgradeCost;
	public int enemyHealth;
	public int enemySpeed;

	// Stats
	public float timeSurvived = 0.0f;
	public int enemiesKilled = 0;
	public int hitsTaken = 0;
	public int bulletsFired = 0;
	public int pointsEarned = 0;
	public int pointsSpent = 0;


	public void resetGameState()
	{
		foreach (Entity entity in level.entities)
		{
			entity.reset();
		}

		player.position = level.spawnPoint;

		upgradeCost = 800;
		enemyHealth = 10;
		enemySpeed = 4;

		timeSurvived = 0.0f;
		enemiesKilled = 0;
		hitsTaken = 0;
		bulletsFired = 0;
		pointsEarned = 0;
		pointsSpent = 0;
	}

	public void addSpawnPoint(int x, int y)
	{
		spawnPoints.Add(new Vector2i(x, y));
	}

	public void addShop(Shop shop)
	{
		shops.Add(shop);
	}

	public void clearShops()
	{
		foreach (Shop shop in shops)
			shop.clear();
	}

	public void refillShops()
	{
		foreach (Shop shop in shops)
			shop.refill();
	}

	public void update()
	{
		if (player.health == 0)
		{
			if (Input.IsKeyPressed(KeyCode.KeyE))
			{
				resetGameState();
			}
			else
			{
				return;
			}
		}

		timeSurvived += Time.deltaTime;
	}

	Vector2i getSpawnPoint()
	{
		List<Vector2i> possibleSpawnPoints = new List<Vector2i>();
		possibleSpawnPoints.AddRange(spawnPoints);

		for (int i = 0; i < possibleSpawnPoints.Count; i++)
		{
			if (!level.astar.run((Vector2i)player.position, possibleSpawnPoints[i], null))
				possibleSpawnPoints.RemoveAt(i--);
		}
		possibleSpawnPoints.Sort((Vector2i a, Vector2i b) =>
		{
			int da = Math.Abs(a.x - (int)player.position.x) + Math.Abs(a.y - (int)player.position.y);
			int db = Math.Abs(b.x - (int)player.position.x) + Math.Abs(b.y - (int)player.position.y);
			return da < db ? -1 : da > db ? 1 : 0;
		});
		if (possibleSpawnPoints.Count > 0)
		{
			int idx = Random.Shared.Next() % (possibleSpawnPoints.Count > 3 ? possibleSpawnPoints.Count / 2 : possibleSpawnPoints.Count);
			return possibleSpawnPoints[idx];
		}
		Debug.Assert(false);
		return Vector2i.Zero;
	}

	public void draw()
	{
		if (player.health == 0)
			drawGameOverScreen();
	}

	void drawGameOverScreen()
	{
		Renderer.DrawUISprite(Display.width / 8, Display.height / 8, Display.width / 8 * 6, Display.height / 8 * 6, null, false, 0xFF111111);

		{
			string text = "Ya ded son";
			int width = text.Length * 32;
			int height = 32;
			Renderer.DrawUIText(Display.width / 2 - width / 2, Display.height / 4 - height / 2, text, 2, 0xFFAAAAAA);
		}

		int leftBound = Display.width / 8 + 50;
		int rightBound = Display.width / 8 * 7 - 50;
		int yscroll = Display.height / 4 + 100;

		var drawLeftInfo = (string text, uint color) =>
		{
			Renderer.DrawUIText(leftBound, yscroll, text, 2, color);
		};
		var drawRightInfo = (string text, uint color) =>
		{
			int width = text.Length * 32;
			Renderer.DrawUIText(rightBound - width, yscroll, text, 2, color);
			yscroll += 48;
		};

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

		{
			string text = "[E] Quick Restart";
			int width = text.Length * 32;
			int height = 32;
			Renderer.DrawUIText(Display.width / 2 - width / 2, Display.height / 16 * 13 - height / 2, text, 2, 0xFFAAAAAA);
		}
	}
}
