using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class EditorInstance
{
	public string filename = null;

	public List<Entity> entities = new List<Entity>();
	public Camera camera;

	public Entity selectedNode = null;

	Vector2 lastViewportSize;
	public Texture frame;


	public EditorInstance()
	{
		camera = new Camera();
		camera.pitch = -0.2f * MathF.PI;
		camera.yaw = MathF.PI * 0.25f;
	}

	public void destroy()
	{

	}

	Entity getEntityByName(string name)
	{
		foreach (Entity entity in entities)
		{
			if (entity.name == name)
				return entity;
		}
		return null;
	}

	string newEntityName()
	{
		int idx = 0;
		string name = "Entity";
		while (getEntityByName(name) != null)
		{
			name = "Entity" + ++idx;
		}
		return name;
	}

	public Entity newEntity()
	{
		string name = newEntityName();
		Entity entity = new Entity(name);
		entities.Add(entity);
		return entity;
	}

	public void removeEntity(Entity entity)
	{
		entity.destroy();
		entities.Remove(entity);
	}

	public void update()
	{
		camera.update();

		foreach (Entity entity in entities)
		{
			entity.update();
		}
	}

	public void draw(GraphicsDevice graphics)
	{
		if (EditorUI.currentViewportSize != lastViewportSize)
		{
			Renderer.Resize((int)EditorUI.currentViewportSize.x, (int)EditorUI.currentViewportSize.y);
			lastViewportSize = EditorUI.currentViewportSize;
		}

		Renderer.Begin();
		Renderer.SetCamera(camera);

		foreach (Entity entity in entities)
		{
			entity.draw();
		}

		frame = Renderer.End();
	}
}
