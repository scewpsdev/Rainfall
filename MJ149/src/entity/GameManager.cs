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

	public Player player;

	int wave = 0;
	int enemiesToSpawn = 0;
	public int enemiesRemaining;
	float spawnRate;

	float waveBeginningTimer = 0.0f;
	long lastSpawnTime = 0;

	// Stats
	public float timeSurvived = 0.0f;
	public int enemiesKilled = 0;
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

		wave = 0;
		enemiesToSpawn = 0;
		enemiesRemaining = 0;
		spawnRate = 0.3f;

		waveBeginningTimer = 0.0f;
		lastSpawnTime = 0;

		timeSurvived = 0.0f;
		enemiesKilled = 0;
		bulletsFired = 0;
		pointsEarned = 0;
		pointsSpent = 0;
	}

	public void addSpawnPoint(int x, int y)
	{
		spawnPoints.Add(new Vector2i(x, y));
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

		if (enemiesToSpawn == 0 && enemiesRemaining == 0)
		{
			wave++;
			enemiesToSpawn = wave * 10;
			spawnRate *= 1.5f;
			waveBeginningTimer = 0.0f;
		}
		if (waveBeginningTimer < WAVE_BEGINNING_DELAY)
		{
			waveBeginningTimer += Time.deltaTime;
		}
		else
		{
			if (enemiesToSpawn > 0)
			{
				if ((Time.currentTime - lastSpawnTime) / 1e9f > 1.0f / spawnRate)
				{
					Vector2i spawnPoint = getSpawnPoint();
					level.addEntity(new Enemy(spawnPoint + 0.5f));
					enemiesToSpawn--;
					enemiesRemaining++;
					lastSpawnTime = Time.currentTime;
				}
			}
		}
	}

	Vector2i getSpawnPoint()
	{
		List<Vector2i> possibleSpawnPoints = new List<Vector2i>();
		possibleSpawnPoints.AddRange(spawnPoints);

		for (int i = 0; i < possibleSpawnPoints.Count; i++)
		{
			if (!level.astar.run((Vector2i)player.position, spawnPoints[i], null))
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
			int idx = Random.Shared.Next() % (possibleSpawnPoints.Count >= 4 ? possibleSpawnPoints.Count / 2 : possibleSpawnPoints.Count);
			return possibleSpawnPoints[idx];
		}
		Debug.Assert(false);
		return Vector2i.Zero;
	}

	public void draw()
	{
		if (waveBeginningTimer < WAVE_BEGINNING_DELAY)
		{
			string text = "Wave " + wave;
			int width = text.Length * 32;
			int height = 32;
			Renderer.DrawUIText(Display.width / 2 - width / 2, Display.height / 3 - height / 2, text);
		}
		if (player.health == 0)
			drawGameOverScreen();
	}

	void drawGameOverScreen()
	{
		Renderer.DrawUISprite(Display.width / 8, Display.height / 8, Display.width / 8 * 6, Display.height / 8 * 6, null, 0xFF111111);

		{
			string text = "Ya ded son";
			int width = text.Length * 32;
			int height = 32;
			Renderer.DrawUIText(Display.width / 2 - width / 2, Display.height / 4 - height / 2, text, 0xFFAAAAAA);
		}

		int leftBound = Display.width / 8 + 50;
		int rightBound = Display.width / 8 * 7 - 50;
		int yscroll = Display.height / 4 + 100;

		var drawLeftInfo = (string text, uint color) =>
		{
			Renderer.DrawUIText(leftBound, yscroll, text, color);
		};
		var drawRightInfo = (string text, uint color) =>
		{
			int width = text.Length * 32;
			Renderer.DrawUIText(rightBound - width, yscroll, text, color);
			yscroll += 48;
		};

		drawLeftInfo("Time survived: ", 0xFFFFFFFF);
		drawRightInfo(((int)timeSurvived).ToString(), 0xFFAAAAAA);

		drawLeftInfo("Enemies killed: ", 0xFFFFFFFF);
		drawRightInfo(enemiesKilled.ToString(), 0xFFAAAAAA);

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
			Renderer.DrawUIText(Display.width / 2 - width / 2, Display.height / 16 * 13 - height / 2, text, 0xFFAAAAAA);
		}
	}
}
