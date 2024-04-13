using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Chunk
{
	public const int CHUNK_RESOLUTION = 128;
	public const float CHUNK_SIZE = 128;
	public const float CHUNK_HEIGHT_PRECISION = 0.25f;


	public readonly int x, z;
	public readonly Terrain terrain;

	List<Entity> entities = new List<Entity>();


	public Chunk(int x, int z)
	{
		this.x = x;
		this.z = z;

		terrain = new Terrain(new Vector3(x * CHUNK_SIZE, 0.0f, z * CHUNK_SIZE), CHUNK_RESOLUTION, CHUNK_SIZE, CHUNK_HEIGHT_PRECISION, Renderer.graphics);
		terrain.init();
	}

	public void addEntity(Entity entity)
	{
		entities.Add(entity);
		entity.init();
	}

	public void addEntity(Entity entity, Vector3 position, Quaternion rotation)
	{
		entity.position = position;
		entity.rotation = rotation;
		addEntity(entity);
	}

	public void update()
	{
		for (int i = 0; i < entities.Count; i++)
		{
			entities[i].update();
			if (entities[i].removed)
			{
				entities[i].destroy();
				entities.RemoveAt(i);
				i--;
			}
		}
	}

	public void draw(GraphicsDevice graphics)
	{
		terrain.draw(graphics);
		//Renderer.DrawWater(new Vector3(x * CHUNK_SIZE, WATER_LEVEL, z * CHUNK_SIZE), CHUNK_SIZE);

		for (int i = 0; i < entities.Count; i++)
		{
			entities[i].draw(graphics);
		}
	}
}