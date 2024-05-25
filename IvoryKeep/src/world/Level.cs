using Rainfall;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading.Tasks.Dataflow;


public class Level
{
	public Model mesh;
	public RigidBody body;

	public Matrix spawnPoint = Matrix.Identity;

	List<Entity> entities = new List<Entity>();


	public Level()
	{
	}

	public void reset()
	{
		spawnPoint = Matrix.Identity;

		if (mesh != null)
		{
			mesh.destroy();
			mesh = null;
		}

		foreach (Entity entity in entities)
		{
			foreach (var removeCallback in entity.removeCallbacks)
				removeCallback.Invoke();
			entity.destroy();
		}
		entities.Clear();

		body.clearColliders();
	}

	public void init()
	{
	}

	public void destroy()
	{
		if (mesh != null)
		{
			mesh.destroy();
			mesh = null;
		}

		foreach (Entity entity in entities)
		{
			foreach (var removeCallback in entity.removeCallbacks)
				removeCallback.Invoke();
			entity.destroy();
		}
		entities.Clear();
	}

	public void addEntity(Entity entity, bool init = true)
	{
		entities.Add(entity);

		if (init)
			entity.init();
	}

	public void addEntity(Entity entity, Vector3 position, bool init = true)
	{
		entity.position = position;
		addEntity(entity, init);
	}

	public void addEntity(Entity entity, Vector3 position, Quaternion rotation, bool init = true)
	{
		entity.position = position;
		entity.rotation = rotation;
		addEntity(entity, init);
	}

	public void addEntity(Entity entity, Vector3 position, Quaternion rotation, Vector3 scale, bool init = true)
	{
		entity.position = position;
		entity.rotation = rotation;
		entity.scale = scale;
		addEntity(entity, init);
	}

	public void addEntity(Entity entity, Matrix transform, bool init = true)
	{
		transform.decompose(out Vector3 position, out Quaternion rotation, out Vector3 scale);
		entity.position = position;
		entity.rotation = rotation;
		entity.scale = scale;
		addEntity(entity, init);
	}

	public void removeEntity(Entity entity)
	{
		entities.Remove(entity);
	}

	public void update()
	{
		for (int i = 0; i < entities.Count; i++)
		{
			entities[i].update();
			if (entities[i].removed)
			{
				foreach (var removeCallback in entities[i].removeCallbacks)
					removeCallback.Invoke();

				entities[i].destroy();
				entities.RemoveAt(i);
				i--;
			}
		}
	}

	public unsafe void draw(GraphicsDevice graphics)
	{
		for (int i = 0; i < entities.Count; i++)
		{
			entities[i].draw(graphics);
		}
	}
}
