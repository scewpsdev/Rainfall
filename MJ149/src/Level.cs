using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Level
{
	public readonly int width, height;
	uint[] tiles;
	public readonly AStar astar;
	public readonly bool[] walkable;

	public readonly Vector2 spawnPoint;

	public readonly List<Entity> entities = new List<Entity>();


	public Level(string path)
	{
		byte[] levelData = Resource.ReadImage(path, out TextureInfo levelInfo);
		width = levelInfo.width;
		height = levelInfo.height;
		tiles = new uint[width * height];
		walkable = new bool[width * height];
		Array.Fill(walkable, true);
		astar = new AStar(width, height, walkable, null);

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				int i = x + y * width;
				byte b = levelData[(x + (height - y - 1) * width) * 4 + 0];
				byte g = levelData[(x + (height - y - 1) * width) * 4 + 1];
				byte r = levelData[(x + (height - y - 1) * width) * 4 + 2];
				byte a = levelData[(x + (height - y - 1) * width) * 4 + 3];
				uint tile = (uint)((a << 24) | (r << 16) | (g << 8) | b);

				if (tile == 0xfffbf236)
				{
					spawnPoint = new Vector2(x + 0.5f, y + 0.5f);
				}
				else if (tile == 0xffac3232)
				{
					Gaem.instance.manager.addSpawnPoint(x, y);
				}
				else if (tile == 0xff6abe30)
				{
					UpgradeType upgradeType = (UpgradeType)(Random.Shared.Next() % (int)UpgradeType.Count);
					addEntity(new Upgrade(upgradeType, 1000, x + 0.5f, y + 0.5f));
				}
				else if (tile == 0xFF000000)
				{
					int doorWidth = 1, doorHeight = 1;
					for (int xx = x + 1; xx < width; xx++)
					{
						byte rb = levelData[(xx + (height - y - 1) * width) * 4 + 0];
						byte rg = levelData[(xx + (height - y - 1) * width) * 4 + 1];
						byte rr = levelData[(xx + (height - y - 1) * width) * 4 + 2];
						byte ra = levelData[(xx + (height - y - 1) * width) * 4 + 3];
						uint rtile = (uint)((ra << 24) | (rr << 16) | (rg << 8) | rb);
						if (rtile == tile)
						{
							doorWidth = xx - x + 1;
							levelData[(xx + (height - y - 1) * width) * 4 + 0] = 0;
							levelData[(xx + (height - y - 1) * width) * 4 + 1] = 0;
							levelData[(xx + (height - y - 1) * width) * 4 + 2] = 0;
							levelData[(xx + (height - y - 1) * width) * 4 + 3] = 0;
						}
					}
					for (int yy = y + 1; yy < height; yy++)
					{
						byte db = levelData[(x + (height - yy - 1) * width) * 4 + 0];
						byte dg = levelData[(x + (height - yy - 1) * width) * 4 + 1];
						byte dr = levelData[(x + (height - yy - 1) * width) * 4 + 2];
						byte da = levelData[(x + (height - yy - 1) * width) * 4 + 3];
						uint dtile = (uint)((da << 24) | (dr << 16) | (dg << 8) | db);
						if (dtile == tile)
						{
							doorHeight = yy - y + 1;
							levelData[(x + (height - yy - 1) * width) * 4 + 0] = 0;
							levelData[(x + (height - yy - 1) * width) * 4 + 1] = 0;
							levelData[(x + (height - yy - 1) * width) * 4 + 2] = 0;
							levelData[(x + (height - yy - 1) * width) * 4 + 3] = 0;
						}
					}
					addEntity(new Door(500, x, y, doorWidth, doorHeight));

					for (int yy = y; yy < y + doorHeight; yy++)
					{
						for (int xx = x; xx < x + doorWidth; xx++)
						{
							setWalkable(xx, yy, false);
						}
					}
				}
				else if (tile != 0)
				{
					tiles[i] = tile;
					walkable[i] = false;
				}
			}
		}
	}

	public void addEntity(Entity entity)
	{
		entities.Add(entity);
		entity.level = this;
	}

	public T findEntity<T>() where T : Entity
	{
		foreach (Entity entity in entities)
		{
			if (entity is T)
				return (T)entity;
		}
		return null;
	}

	public uint getTile(int x, int y)
	{
		if (x < 0 || y < 0 || x >= width || y >= height)
			return 0;
		return tiles[x + y * width];
	}

	public void setWalkable(int x, int y, bool isWalkable)
	{
		if (x < 0 || y < 0 || x >= width || y >= height)
			return;
		walkable[x + y * width] = isWalkable;
	}

	public void update()
	{
		for (int i = 0; i < entities.Count; i++)
		{
			entities[i].update();
			if (entities[i].removed)
			{
				entities[i].destroy();
				entities.RemoveAt(i--);
			}
		}
	}

	public void draw(int x0, int x1, int y0, int y1)
	{
		for (int y = y0; y <= y1; y++)
		{
			for (int x = x0; x <= x1; x++)
			{
				uint tile = getTile(x, y);
				if (tile != 0)
				{
					Renderer.DrawSprite(x, y, 1, 1, null, tile);
				}
			}
		}

		for (int i = 0; i < entities.Count; i++)
			entities[i].draw();
	}
}
