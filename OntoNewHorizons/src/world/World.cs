using Rainfall;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;


public class World
{
	public List<Chunk> chunks = new List<Chunk>();


	public World()
	{
		GraphicsManager.sun = new DirectionalLight(new Vector3(0.0f, -1.0f, -1.0f), new Vector3(1.0f, 0.9f, 0.7f) * 10.0f, Renderer.graphics);
		GraphicsManager.skybox = Resource.GetCubemap("res/texture/cubemap/overcast_cubemap.hdr");
		GraphicsManager.environmentMap = GraphicsManager.skybox;

		Renderer.fogColor = new Vector3(1.0f, 2.0f, 2.0f);
		Renderer.fogIntensity = 0.001f;
	}

	Chunk getChunkAtPosition(float x, float z)
	{
		for (int i = 0; i < chunks.Count; i++)
		{
			float x0 = chunks[i].x * Chunk.CHUNK_SIZE;
			float x1 = x0 + Chunk.CHUNK_SIZE;
			float z0 = chunks[i].z * Chunk.CHUNK_SIZE;
			float z1 = z0 + Chunk.CHUNK_SIZE;

			if (x >= x0 && x < x1 && z >= z0 && z < z1)
				return chunks[i];
		}
		return null;
	}

	public float getTerrainHeight(float x, float z)
	{
		Chunk chunk = getChunkAtPosition(x, z);
		if (chunk != null)
		{
			return chunk.terrain.getHeight(x - chunk.terrain.position.x, z - chunk.terrain.position.z);
		}
		return 0.0f;
	}

	public void addEntity(Entity entity)
	{
		Chunk chunk = getChunkAtPosition(entity.position.x, entity.position.z);
		if (chunk != null)
		{
			chunk.addEntity(entity);
		}
	}

	public void addEntity(Entity entity, Vector3 position, Quaternion rotation)
	{
		entity.position = position;
		entity.rotation = rotation;
		addEntity(entity);
	}

	public void addEntity(Entity entity, Vector3 position, Quaternion rotation, Vector3 scale)
	{
		entity.position = position;
		entity.rotation = rotation;
		entity.scale = scale;
		addEntity(entity);
	}

	public void update()
	{
		for (int i = 0; i < chunks.Count; i++)
		{
			chunks[i].update();
		}
	}

	public void draw(GraphicsDevice graphics)
	{
		for (int i = 0; i < chunks.Count; i++)
		{
			chunks[i].draw(graphics);
		}
	}
}
