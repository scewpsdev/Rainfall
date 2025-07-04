﻿using Rainfall;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading.Tasks.Dataflow;
using static System.Formats.Asn1.AsnWriter;


public class Scene
{
	const float ENTITY_BUCKET_REGION_SIZE = 20.0f;


	Dictionary<Vector3i, List<Entity>> entities = new Dictionary<Vector3i, List<Entity>>();
	List<Entity> reorderBuffer = new List<Entity>();


	public Scene()
	{
	}

	public void destroy()
	{
		foreach (List<Entity> entityList in entities.Values)
		{
			foreach (Entity entity in entityList)
			{
				foreach (var removeCallback in entity.removeCallbacks)
					removeCallback.Invoke();
				entity.destroy();
			}
			entityList.Clear();
		}
		entities.Clear();
	}

	public bool load(string path, Func<Entity> createEntityCallback = null)
	{
		if (SceneFormat.Read(path, out List<SceneFormat.EntityData> entities, out _))
		{
			for (int i = 0; i < entities.Count; i++)
			{
				SceneFormat.EntityData entityData = entities[i];

				if (entityData.name.StartsWith("__"))
				{
					string entityName = entityData.name.Substring(2);
					if (int.TryParse(entityName.Substring(entityName.LastIndexOf('_') + 1), out int _))
						entityName = entityName.Substring(0, entityName.LastIndexOf('_'));

					Entity entity = null;

					if (entity != null)
						addEntity(entity, entityData.position, entityData.rotation, entityData.scale);
				}
				else
				{
					Entity entity = createEntityCallback != null ? createEntityCallback() : new Entity();
					addEntity(entity, entityData.position, entityData.rotation, entityData.scale);
					entity.load(entityData);
				}
			}
			return true;
		}
		return false;
	}

	Vector3i getEntityTile(Vector3 position)
	{
		return (Vector3i)Vector3.Floor(position / ENTITY_BUCKET_REGION_SIZE);
	}

	public void getEntitiesInRange(Vector3 position, float range, List<Entity> list)
	{
		Vector3i min = getEntityTile(position - range);
		Vector3i max = getEntityTile(position + range);
		for (int z = min.z; z <= max.z; z++)
		{
			for (int y = min.y; y <= max.y; y++)
			{
				for (int x = min.x; x <= max.x; x++)
				{
					Vector3i tile = new Vector3i(x, y, z);
					if (entities.TryGetValue(tile, out List<Entity> entityList))
					{
						foreach (Entity entity in entityList)
						{
							float distanceSq = (entity.position - position).lengthSquared;
							if (distanceSq < range * range)
								list.Add(entity);
						}
					}
				}
			}
		}
	}

	void addEntity(Entity entity, Vector3i tile, bool init = true)
	{
		if (!entities.TryGetValue(tile, out List<Entity> entityList))
		{
			entityList = new List<Entity>();
			entities.Add(tile, entityList);
		}
		entityList.Add(entity);

		entity.scene = this;

		if (init)
			entity.init();
	}

	public void addEntity(Entity entity, bool init = true)
	{
		addEntity(entity, Vector3i.Zero, init);
	}

	public void addEntity(Entity entity, Vector3 position, bool init = true)
	{
		entity.position = position;
		addEntity(entity, getEntityTile(position), init);
	}

	public void addEntity(Entity entity, Vector3 position, Quaternion rotation, bool init = true)
	{
		entity.position = position;
		entity.rotation = rotation;
		addEntity(entity, getEntityTile(position), init);
	}

	public void addEntity(Entity entity, Vector3 position, Quaternion rotation, Vector3 scale, bool init = true)
	{
		entity.position = position;
		entity.rotation = rotation;
		entity.scale = scale;
		addEntity(entity, getEntityTile(position), init);
	}

	public void addEntity(Entity entity, Matrix transform, bool init = true)
	{
		transform.decompose(out Vector3 position, out Quaternion rotation, out Vector3 scale);
		entity.position = position;
		entity.rotation = rotation;
		entity.scale = scale;
		addEntity(entity, getEntityTile(position), init);
	}

	public void removeEntity(Entity entity)
	{
		Vector3i tile = getEntityTile(entity.position);
		if (entities.TryGetValue(tile, out List<Entity> entityList))
		{
			if (entityList.Contains(entity))
			{
				entityList.Remove(entity);
				return;
			}
		}
		foreach (List<Entity> list in entities.Values)
		{
			if (list.Contains(entity))
			{
				list.Remove(entity);
				return;
			}
		}
		Debug.Assert(false);
	}

	public void update()
	{
		for (int j = 0; j < entities.Count; j++)
		{
			List<Entity> entityList = entities.Values.ElementAt(j);
			for (int i = 0; i < entityList.Count; i++)
			{
				entityList[i].update();
				if (entityList[i].removed)
				{
					foreach (var removeCallback in entityList[i].removeCallbacks)
						removeCallback.Invoke();

					entityList[i].destroy();
					entityList.RemoveAt(i);
					i--;
				}
			}
		}
		foreach (var pair in entities)
		{
			Vector3i tile = pair.Key;
			List<Entity> entityList = pair.Value;
			for (int i = 0; i < entityList.Count; i++)
			{
				if (getEntityTile(entityList[i].position) != tile)
				{
					reorderBuffer.Add(entityList[i]);
					entityList.RemoveAt(i);
					i--;
				}
			}
			if (entityList.Count == 0)
				entities.Remove(tile);
		}
		for (int i = 0; i < reorderBuffer.Count; i++)
		{
			Vector3i tile = getEntityTile(reorderBuffer[i].position);
			addEntity(reorderBuffer[i], tile, false);
		}
		reorderBuffer.Clear();
	}

	public void fixedUpdate(float delta)
	{
		foreach (List<Entity> entityList in entities.Values)
		{
			for (int i = 0; i < entityList.Count; i++)
			{
				entityList[i].fixedUpdate(delta);
			}
		}
	}

	public unsafe void draw(GraphicsDevice graphics)
	{
		foreach (List<Entity> entityList in entities.Values)
		{
			for (int i = 0; i < entityList.Count; i++)
			{
				entityList[i].draw(graphics);
			}
		}
	}
}
