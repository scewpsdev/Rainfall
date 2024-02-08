using Rainfall;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading.Tasks.Dataflow;


public struct LevelMesh
{
	public Model model;
	public int meshID;
	public Matrix transform;

	public LevelMesh(Model model, int meshID, Matrix transform)
	{
		this.model = model;
		this.meshID = meshID;
		this.transform = transform;
	}

	public LevelMesh(Model model, Matrix transform)
		: this(model, -1, transform)
	{
	}

	public LevelMesh(Model model, int meshID, Room room)
		: this(model, meshID, room.transform)
	{
	}

	public LevelMesh(Model model, Room room)
		: this(model, -1, room.transform)
	{
	}
}

public class Level
{
	//public uint[] tiles;
	//public int[] heightmap;
	public TileMap tilemap;
	public List<Room> rooms = new List<Room>();
	public Dictionary<int, int> roomIDMap = new Dictionary<int, int>();
	public Matrix spawnPoint;

	public List<LevelMesh> levelMeshes = new List<LevelMesh>();
	public List<ReflectionProbe> reflections = new List<ReflectionProbe>();
	List<Entity> entities = new List<Entity>();

	public RigidBody body;

	Sound ambientSound;


	public Level()
	{
		tilemap = new TileMap();

		body = new RigidBody(null, RigidBodyType.Static);

		ambientSound = Resource.GetSound("res/level/sounds/dungeon.ogg");
	}

	public void reset()
	{
		tilemap.reset();
		rooms.Clear();
		roomIDMap.Clear();
		spawnPoint = Matrix.Identity;

		levelMeshes.Clear();
		reflections.Clear();

		foreach (Entity entity in entities)
		{
			entity.destroy();
		}
		entities.Clear();

		body.clearColliders();
	}

	public void init()
	{
		AudioManager.SetAmbientSound(ambientSound, 0.05f);

		Audio.SetEffect(AudioEffect.Reverb);

		Renderer.fogColor = new Vector3(1.0f, 1.0f, 1.5f);
		Renderer.fogIntensity = 0.0001f;
	}

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
			{
				if (mesh.meshID != -1)
					Renderer.DrawSubModel(mesh.model, mesh.meshID, mesh.transform);
				else
					Renderer.DrawModel(mesh.model, mesh.transform);
			}
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
