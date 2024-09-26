using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BossGate : Entity
{
	Sprite sprite;
	int height;

	Mob boss;
	Room room;
	bool isOpen = false;


	public BossGate(Mob boss, Room room, bool isOpen)
	{
		this.boss = boss;
		this.room = room;
		this.isOpen = isOpen;

		sprite = new Sprite(TileType.tileset, 2, 7);
	}

	public override void init(Level level)
	{
		Vector2i tile = (Vector2i)Vector2.Round(position);
		HitData hit = level.raycastTiles(tile + 0.5f, Vector2.Down, 10);
		Debug.Assert(hit != null);
		height = (int)MathF.Ceiling(hit.distance);

		if (!isOpen)
			close();
	}

	void open()
	{
		Vector2i tile = (Vector2i)Vector2.Floor(position + 0.5f);
		for (int i = 0; i < height; i++)
		{
			GameState.instance.level.setTile(tile.x, tile.y - i, null);
		}
		isOpen = true;
	}

	void close()
	{
		Vector2i tile = (Vector2i)Vector2.Floor(position + 0.5f);
		for (int i = 0; i < height; i++)
		{
			GameState.instance.level.setTile(tile.x, tile.y - i, TileType.dummy);
		}
		isOpen = false;
	}

	public override void update()
	{
		if (boss.isAlive && boss.ai.target != null && room.containsEntity(boss.ai.target) && isOpen)
		{
			close();
		}
		if (!boss.isAlive && !isOpen)
		{
			open();

			// update hub area
			GameState.instance.hub.addEntity(new Ladder(17), new Vector2(4, 2));
		}
	}

	public override void render()
	{
		if (!isOpen)
		{
			for (int i = 0; i < height; i++)
			{
				Renderer.DrawSprite(position.x, position.y - i, 1, 1, sprite);
			}
		}
	}
}
