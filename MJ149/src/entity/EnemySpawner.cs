using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class EnemySpawner : Entity
{
	const float SPAWN_DELAY = 3;


	long lastSpawnTime = 0;


	public EnemySpawner(Vector2 position)
	{
		this.position = position;
	}

	public override void update()
	{
		if ((Time.currentTime - lastSpawnTime) / 1e9f > SPAWN_DELAY)
		{
			spawnEnemy();
			lastSpawnTime = Time.currentTime;
		}
	}

	void spawnEnemy()
	{
		Enemy enemy = new Enemy(position);
		level.addEntity(enemy);
	}
}
