using Rainfall;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class EditorInstance
{
	public string path = null;
	public bool unsavedChanges { get; private set; } = false;

	public readonly Stack<byte[]> undoStack = new Stack<byte[]>();
	public readonly Stack<byte[]> redoStack = new Stack<byte[]>();
	static bool pushStateToUndoStack = false;

	public List<Entity> entities = new List<Entity>();
	public Camera camera;

	public uint selectedEntity = 0;

	Vector2 lastViewportSize;
	public Texture frame;


	public EditorInstance(string path)
	{
		camera = new Camera();
		camera.pitch = -0.2f * MathF.PI;
		camera.yaw = MathF.PI * 0.25f;

		if (path != null)
		{
			this.path = path;

			RainfallEditor.instance.readScene(this, path);
		}

		undoStack.Push(SceneFormat.SerializeScene(RainfallEditor.instance.toEntityData(this)));
	}

	public void destroy()
	{
	}

	public void reset()
	{
		foreach (Entity entity in entities)
			entity.destroy();
		entities.Clear();
	}

	public void notifyEdit()
	{
		unsavedChanges = true;
		pushStateToUndoStack = true;
	}

	public void notifySave()
	{
		unsavedChanges = false;
	}

	public void undo()
	{
		if (undoStack.Count > 1)
		{
			redoStack.Push(undoStack.Pop());
			RainfallEditor.instance.fromEntityData(SceneFormat.DeserializeScene(undoStack.Peek()), this);
			unsavedChanges = true;
		}
	}

	public void redo()
	{
		if (redoStack.Count > 0)
		{
			undoStack.Push(redoStack.Pop());
			RainfallEditor.instance.fromEntityData(SceneFormat.DeserializeScene(undoStack.Peek()), this);
			unsavedChanges = true;
		}
	}

	public Entity getEntity(uint id)
	{
		foreach (Entity entity in entities)
		{
			if (entity.id == id)
				return entity;
		}
		return null;
	}

	public Entity getEntityByName(string name)
	{
		foreach (Entity entity in entities)
		{
			if (entity.name == name)
				return entity;
		}
		return null;
	}

	public Entity getSelectedEntity()
	{
		return selectedEntity != 0 ? getEntity(selectedEntity) : null;
	}

	public Entity getNextEntity(uint id)
	{
		Entity entity = getEntity(id);
		int idx = entities.IndexOf(entity);
		int nextIdx = (idx + 1) % entities.Count;
		return entities[nextIdx];
	}

	public Entity getPrevEntity(uint id)
	{
		Entity entity = getEntity(id);
		int idx = entities.IndexOf(entity);
		int nextIdx = (idx - 1 + entities.Count) % entities.Count;
		return entities[nextIdx];
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
		selectedEntity = entity.id;
		notifyEdit();
		return entity;
	}

	public void removeEntity(Entity entity)
	{
		if (selectedEntity == entity.id)
			selectedEntity = 0;
		entity.destroy();
		entities.Remove(entity);
		notifyEdit();
	}

	public void update()
	{
		if (pushStateToUndoStack)
		{
			undoStack.Push(SceneFormat.SerializeScene(RainfallEditor.instance.toEntityData(this)));
			redoStack.Clear();
			pushStateToUndoStack = false;
		}

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

		int gridSize = 10;
		Vector4 gridColor = new Vector4(0.1f, 0.1f, 0.1f, 1.0f);
		for (int i = -gridSize; i <= gridSize; i++)
		{
			Renderer.DrawDebugLine(new Vector3(-gridSize, 0, i), new Vector3(gridSize, 0, i), gridColor);
			Renderer.DrawDebugLine(new Vector3(i, 0, -gridSize), new Vector3(i, 0, gridSize), gridColor);
		}

		foreach (Entity entity in entities)
		{
			entity.draw(graphics);
		}

		frame = Renderer.End();
	}

	public string filename
	{
		get => path != null ? StringUtils.GetFilenameFromPath(path) : null;
	}
}
