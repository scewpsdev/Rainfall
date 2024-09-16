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
	bool isOpen = false;


	public BossGate(Mob boss, int height = 3)
	{
		this.boss = boss;
		this.height = height;

		sprite = new Sprite(TileType.tileset, 2, 7);
	}

	void open()
	{
		Vector2i tile = (Vector2i)Vector2.Floor(position + new Vector2(0, 0.5f));
		for (int i = 0; i < height; i++)
		{
			GameState.instance.level.setTile(tile.x, tile.y + i, null);
		}
		isOpen = true;
	}

	void close()
	{
		Vector2i tile = (Vector2i)Vector2.Floor(position + new Vector2(0, 0.5f));
		for (int i = 0; i < height; i++)
		{
			GameState.instance.level.setTile(tile.x, tile.y + i, TileType.dummy);
		}
		isOpen = false;
	}

	public override void init(Level level)
	{
		close();
	}

	public override void update()
	{
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
				Renderer.DrawSprite(position.x - 0.5f, position.y + i, 1, 1, sprite);
			}
		}
	}
}
