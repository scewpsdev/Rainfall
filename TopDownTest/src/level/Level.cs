using Rainfall;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading.Tasks.Dataflow;


public class DoorwayInstance
{
	public Vector2i position;
	public Vector2i direction;
	public int height;
	public Vector3 worldPosition;
	public Quaternion worldRotation;
	public RoomInstance room;

	public List<DoorwayInstance> connectedDoorways = new List<DoorwayInstance>();
}

public class RoomInstance
{
	public Vector2i pos;
	public Vector2i size;
	public int height;

	public List<DoorwayInstance> doorways = new List<DoorwayInstance>();

	public RoomInterior interior;


	public Vector3 worldPosition
	{
		get => new Vector3(pos.x, height, pos.y) * LevelGenerator.TILE_SIZE;
	}
}

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
	public int width, height;
	public uint[] tileTypes;
	public uint[] tiles;
	public int[] heightmap;
	public List<RoomInstance> rooms;
	public Vector3 spawnPoint;

	List<Entity> entities = new List<Entity>();


	public Level()
	{
	}

	uint getTile(int x, int z)
	{
		if (x >= 0 && x < width && z >= 0 && z < height)
			return tiles[x + z * width];
		return 0;
	}

	public void addEntity(Entity entity)
	{
		entities.Add(entity);
		entity.init();
	}

	public void addEntity(Entity entity, Vector3 position)
	{
		entity.position = position;
		addEntity(entity);
	}

	public void update()
	{
		for (int i = 0; i < rooms.Count; i++)
		{
			//rooms[i].update();
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

		Renderer.ambientLight = new Vector3(0.005f);

		Renderer.DrawLight(new Vector3(11 + 2 * MathF.Sin(Time.currentTime / 1e9f * 2), 1, 5), new Vector3(1.0f, 1.0f, 1.0f) * 7.0f);
		Renderer.DrawLight(new Vector3(11, 1 + MathF.Sin(Time.currentTime / 1e9f * 2), 11), new Vector3(0.4f, 0.4f, 1.0f) * 7.0f);
		Renderer.DrawLight(new Vector3(10 + 2 * MathF.Sin(Time.currentTime / 1e9f * 2), 1, 10 + 2 * MathF.Cos(Time.currentTime / 1e9f * 2)), new Vector3(1.0f, 0.4f, 0.4f) * 7.0f);
		Renderer.DrawLight(new Vector3(12 + 2 * MathF.Cos(Time.currentTime / 1e9f * 2.5f), 1, 12 + 2 * MathF.Sin(Time.currentTime / 1e9f * 2.5f)), new Vector3(0.3f, 1.0f, 0.3f) * 7.0f);

		for (int z = 0; z < height; z++)
		{
			for (int x = 0; x < width; x++)
			{
				uint tileID = getTile(x, z);
				Tile tile = Tile.Get(tileID);
				if (tile.wall)
				{
					Sprite wallSprite = tile.selectWallSprite(x, z);
					Sprite sprite = tile.selectSprite(x, z);
					Renderer.DrawVerticalWall(new Vector3(x, 0.0f, z + 1), new Vector2(1.0f, tile.wallHeight - 0.0001f), wallSprite, tile.color);
					Renderer.DrawHorizontalWall(new Vector3(x, tile.wallHeight, z), new Vector2(1.0f, 1.0f), sprite, tile.color);
				}
				else
				{
					Sprite sprite = tile.selectSprite(x, z);
					Renderer.DrawHorizontalWall(new Vector3(x, 0.0f, z), new Vector2(1.0f), sprite, tile.color);
				}
			}
		}

		//for (int i = 0; i < rooms.Count; i++)
		//{
		//rooms[i].draw(graphics);
		//}

		//GraphicsManager.environmentMap = Resource.GetCubemap("res/texture/cubemap/dungeon_cubemap.png");
		//GraphicsManager.environmentMapIntensity = 50;

		/*
		foreach (LevelMesh mesh in levelMeshes)
		{
			Renderer.DrawModel(mesh.model, mesh.transform);
		}
		*/

		for (int i = 0; i < entities.Count; i++)
		{
			entities[i].draw(graphics);
		}
	}
}
