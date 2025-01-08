using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


public class HitData
{
	public float distance;
	public Vector2 position;
	public Vector2 normal;
	public Vector2i tile;
	public Entity entity;
}

public class Level
{
	public const int COLLISION_X = 1 << 0;
	public const int COLLISION_Y = 1 << 1;


	public List<Entity> entities = new List<Entity>();

	public Texture bg = null;
	public Vector3 ambientLight = new Vector3(1.0f);
	public Vector3 fogColor = new Vector3(0.0f);
	public float fogFalloff = 0.0f;
	public Sound ambientSound = null;

	public float lightLevel
	{
		get
		{
			Vector3 srgb = Vector3.Min(ambientLight, Vector3.One);
			return MathF.Max(MathF.Max(srgb.x, srgb.y), srgb.z);
		}
	}

	public void destroy()
	{
		while (entities.Count > 0)
		{
			foreach (var removeCallback in entities[0].removeCallbacks)
				removeCallback.Invoke();
			entities[0].destroy();
			entities[0].level = null;
			entities.RemoveAt(0);
		}
	}

	void addEntity(Entity entity, bool init = true)
	{
		Debug.Assert(!init || entity.level == null);
		Debug.Assert(!entities.Contains(entity));

		entities.Add(entity);
		entity.level = this;

		if (init)
			entity.init(this);
	}

	public void addEntity(Entity entity, Vector3 position, bool init = true)
	{
		entity.position = position;
		addEntity(entity, init);
	}

	public void addEntity(Entity entity, Vector3 position, float rotation, bool init = true)
	{
		entity.position = position;
		entity.rotation = rotation;
		addEntity(entity, init);
	}

	public void addEntity(Entity entity, Matrix transform, bool init = true)
	{
		transform.decompose(out Vector3 position, out Quaternion rotation, out Vector3 _);
		entity.position = position;
		entity.rotation = rotation.angle;
		addEntity(entity, init);
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
				entities[i].level = null;
				entities.RemoveAt(i);
				i--;
			}
		}
	}

	public void render()
	{
		Renderer.ambientLight = ambientLight;
		Renderer.bloomStrength = 0.01f;
		Renderer.vignetteFalloff = 0.1f;

		for (int i = 0; i < entities.Count; i++)
		{
			entities[i].render();
		}
	}
}
