using Rainfall;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading.Tasks.Dataflow;


public struct LevelMesh
{
	public Model model;
	public Matrix transform;

	public LevelMesh(Model model, Matrix transform)
	{
		this.model = model;
		this.transform = transform;
	}
}

public class Level
{
	//public uint[] tiles;
	//public int[] heightmap;
	public TileMap tilemap;
	public List<Room> rooms;
	public Dictionary<int, int> roomIDMap;
	public Matrix spawnPoint;

	public List<LevelMesh> levelMeshes = new List<LevelMesh>();
	public List<ReflectionProbe> reflections = new List<ReflectionProbe>();
	List<Entity> entities = new List<Entity>();

	public RigidBody body;

	Sound ambientSound;


	public Level()
	{
		body = new RigidBody(null, RigidBodyType.Static);
	}

	public void init()
	{
		ambientSound = Resource.GetSound("res/level/sounds/dungeon.ogg");
		AudioManager.SetAmbientSound(ambientSound, 0.05f);

		Audio.SetEffect(AudioEffect.Reverb);

		Renderer.fogColor = new Vector3(1.0f, 1.0f, 1.5f);
		Renderer.fogIntensity = 0.002f;
	}

	/*
	uint getTile(int x, int z)
	{
		if (x >= 0 && x < width && z >= 0 && z < height)
			return tiles[x + z * width];
		return 0;
	}
	*/

	public int getRoomIDAtPos(Vector3 position)
	{
		return tilemap.getTile((Vector3i)position);
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

	public void addEntity(Entity entity, Vector3 position, Quaternion rotation, Vector3 scale)
	{
		entity.position = position;
		entity.rotation = rotation;
		entity.scale = scale;
		addEntity(entity);
	}

	public void addEntity(Entity entity, Matrix transform)
	{
		transform.decompose(out Vector3 position, out Quaternion rotation, out Vector3 scale);
		addEntity(entity, position, rotation, scale);
	}

	public void update()
	{
		for (int i = 0; i < rooms.Count; i++)
		{
			rooms[i].update();
		}

		for (int i = 0; i < entities.Count; i++)
		{
			entities[i].update();
			if (entities[i].removed)
			{
				foreach (var removeCallback in entities[i].removeCallbacks)
				{
					removeCallback.Invoke();
				}
				entities[i].destroy();
				entities.RemoveAt(i);
				i--;
			}
		}
	}

	public void draw(GraphicsDevice graphics)
	{
		//Renderer.DrawLight(new Vector3(0.0f, 3.0f, 8.0f), new Vector3(1.0f, 1.0f, 1.0f) * 5.0f);

		for (int i = 0; i < rooms.Count; i++)
		{
			rooms[i].draw(graphics);
		}

		//GraphicsManager.environmentMap = Resource.GetCubemap("res/texture/cubemap/dungeon_cubemap.png");
		//GraphicsManager.environmentMapIntensity = 50;

		foreach (LevelMesh mesh in levelMeshes)
		{
			if (mesh.model != null)
				Renderer.DrawModel(mesh.model, mesh.transform);
		}

		for (int i = 0; i < entities.Count; i++)
		{
			entities[i].draw(graphics);
		}

		foreach (ReflectionProbe reflection in reflections)
		{
			Renderer.DrawReflectionProbe(reflection);
		}
	}
}
