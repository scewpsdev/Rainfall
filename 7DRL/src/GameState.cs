using Rainfall;
using System;


public struct TileData
{
	public int value;
	public bool solid;
	public Entity entity;
}

public class GameState : State
{
	public static GameState instance;


	public int width, height;
	public TileData[] tiles;

	public Player player;

	Vector2 cameraPosition;


	public GameState()
	{
		instance = this;

		width = 20;
		height = 20;

		tiles = new TileData[width * height];

		spawnEntity(player = new Player(), 5, 5);
		spawnEntity(new SkeletonEnemy(), 10, 10);

		for (int x = 0; x < width; x++)
		{
			setTile(x, 0, 1);
			setTile(x, height - 1, 1);
		}

		for (int y = 0; y < height; y++)
		{
			setTile(0, y, 1);
			setTile(width - 1, y, 1);
		}
	}

	public override void destroy()
	{
	}

	public ref TileData getTile(int x, int y)
	{
		return ref tiles[x + y * width];
	}

	public void setTile(int x, int y, int value)
	{
		ref TileData tile = ref getTile(x, y);
		tile.value = value;
		tile.solid = value != 0;
	}

	public void spawnEntity(Entity entity, int x, int y)
	{
		getTile(x, y).entity = entity;
		entity.x = x;
		entity.y = y;
		entity.init();
	}

	public bool moveEntity(Entity entity, int x, int y)
	{
		Debug.Assert(getTile(entity.x, entity.y).entity == entity);
		if (!getTile(x, y).solid)
		{
			if (getTile(x, y).entity != null)
			{
				if (entity is Player)
				{
					getTile(x, y).entity.interact(entity as Player);
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				getTile(entity.x, entity.y).entity = null;
				getTile(x, y).entity = entity;
				entity.x = x;
				entity.y = y;
			}
			return true;
		}
		else
		{
			if (!getTile(entity.x, y).solid && getTile(entity.x, y).entity == null)
			{
				getTile(entity.x, entity.y).entity = null;
				getTile(entity.x, y).entity = entity;
				entity.y = y;
				return true;
			}
			else if (!getTile(x, entity.y).solid && getTile(x, entity.y).entity == null)
			{
				getTile(entity.x, entity.y).entity = null;
				getTile(x, entity.y).entity = entity;
				entity.x = x;
				return true;
			}
		}
		return false;
	}

	static List<Entity> updateList = new List<Entity>();
	public void advance(float amount = 1)
	{
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				Entity entity = getTile(x, y).entity;
				if (entity != null && entity is not Player)
				{
					entity.turn += amount;

					float entityTurn = 1.0f / entity.speed;
					while (entity.turn - entityTurn >= -0.0001f)
					{
						entity.turn -= entityTurn;
						updateList.Add(entity);
					}
				}
			}
		}

		for (int i = 0; i < updateList.Count; i++)
		{
			Entity entity = updateList[i];
			entity.update();
			if (entity.remove)
			{
				entity.destroy();
				getTile(entity.x, entity.y).entity = null;
				updateList.RemoveAt(i--);
			}
		}
		updateList.Clear();
	}

	public override void update()
	{
		Vector2i movement = Vector2i.Zero;
		if (Input.IsKeyPressed(KeyCode.D) || Input.IsKeyPressed(KeyCode.NumPad6))
			movement.x++;
		if (Input.IsKeyPressed(KeyCode.A) || Input.IsKeyPressed(KeyCode.NumPad4))
			movement.x--;
		if (Input.IsKeyPressed(KeyCode.W) || Input.IsKeyPressed(KeyCode.NumPad8))
			movement.y++;
		if (Input.IsKeyPressed(KeyCode.S) || Input.IsKeyPressed(KeyCode.NumPad2))
			movement.y--;
		if (Input.IsKeyPressed(KeyCode.Q) || Input.IsKeyPressed(KeyCode.NumPad7))
			movement += new Vector2i(-1, 1);
		if (Input.IsKeyPressed(KeyCode.E) || Input.IsKeyPressed(KeyCode.NumPad9))
			movement += new Vector2i(1, 1);
		if (Input.IsKeyPressed(KeyCode.Z) || Input.IsKeyPressed(KeyCode.NumPad1))
			movement += new Vector2i(-1, -1);
		if (Input.IsKeyPressed(KeyCode.C) || Input.IsKeyPressed(KeyCode.NumPad3))
			movement += new Vector2i(1, -1);

		if (Math.Abs(movement.x) > 1)
			movement.x = MathF.Sign(movement.x);
		if (Math.Abs(movement.y) > 1)
			movement.y = MathF.Sign(movement.y);

		if (movement != Vector2i.Zero)
		{
			if (moveEntity(player, player.x + movement.x, player.y + movement.y))
			{
				if (movement.x != 0)
					player.direction = MathF.Sign(movement.x);
				player.update();
				advance(1.0f / player.speed);
			}
		}
		else if (Input.IsKeyPressed(KeyCode.Space) || Input.IsKeyPressed(KeyCode.NumPad5))
		{
			player.update();
			advance(1.0f / player.speed);
		}

		cameraPosition = new Vector2(player.displayX + 0.5f, player.displayY + 0.5f);
	}

	public override void draw(GraphicsDevice graphics)
	{
		float cameraWidth = Program.instance.width / 16.0f;
		float cameraHeight = Program.instance.height / 16.0f;
		float viewportWidth = (Renderer.UIWidth - HUD.Width) / (float)Renderer.UIWidth * cameraWidth;
		Renderer.SetCamera(new Vector2(cameraPosition.x + (0.5f * cameraWidth - 0.5f * viewportWidth), cameraPosition.y), cameraWidth, cameraHeight);

		Renderer.ambientLight = Vector3.One;

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				TileType tile = TileType.Get(getTile(x, y).value);
				if (tile != null)
				{
					if (tile.wall)
					{
						float z = (height - y) * -0.001f;
						Renderer.DrawSprite(x, y, z, 1, 1, 0, tile.sprite, false, tile.color);
						Renderer.DrawSprite(x, y + 1, z, 1, 1, 0, tile.topSprite != null ? tile.topSprite : tile.sprite, false, tile.topColor);
					}
					else
					{
						Renderer.DrawSprite(x, y, 1, 1, tile.sprite, false, tile.color);
					}
				}

				Entity entity = getTile(x, y).entity;
				if (entity != null)
				{
					float z = (height - y - 0.5f) * -0.001f;
					Renderer.DrawSprite(entity.displayX + entity.rect.position.x, entity.displayY + entity.rect.position.y, z, entity.rect.size.x, entity.rect.size.y, 0, entity.sprite, entity.direction == -1, entity.color, entity.additive);

					entity.render();
				}
			}
		}

		HUD.Render();
	}
}
