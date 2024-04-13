using Rainfall;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;


public class Level
{
	public List<Room> rooms = new List<Room>();
	List<Entity> entities = new List<Entity>();

	public RigidBody body;


	public Level()
	{
		body = new RigidBody(null, RigidBodyType.Static);
	}

	public void addEntity(Entity entity)
	{
		entities.Add(entity);
		entity.level = this;
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

		for (int i = 0; i < entities.Count; i++)
		{
			entities[i].draw(graphics);
		}
	}
}
