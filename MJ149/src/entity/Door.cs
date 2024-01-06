using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Door : Entity, Interactable
{
	int x, y;
	int width, height;

	int cost;
	bool locked = true;


	public Door(int cost, int x, int y, int width, int height)
	{
		this.cost = cost;
		this.x = x;
		this.y = y;
		this.width = width;
		this.height = height;
		position = new Vector2(x, y);

		collider = new FloatRect(0, 0, width, height);
		hitbox = new FloatRect(0, 0, width, height);
		staticCollider = true;
	}

	public override void reset()
	{
		locked = true;
		colliderEnabled = true;

		for (int yy = y; yy < y + height; yy++)
		{
			for (int xx = x; xx < x + width; xx++)
			{
				level.setWalkable(xx, yy, false);
			}
		}
	}

	public bool canInteract(Entity entity)
	{
		return locked;
	}

	public void getInteractionPrompt(Entity entity, out string text, out uint color)
	{
		Player player = entity as Player;

		text = "[E] Open: " + cost;
		color = player.points >= cost ? 0xFFFFFFFF : 0xFFFF7777;
	}

	public void interact(Entity entity)
	{
		if (entity is Player)
		{
			Player player = entity as Player;
			if (player.points >= cost)
			{
				player.points -= cost;
				Gaem.instance.manager.pointsSpent += cost;
				locked = false;
				colliderEnabled = false;

				for (int yy = y; yy < y + height; yy++)
				{
					for (int xx = x; xx < x + width; xx++)
					{
						level.setWalkable(xx, yy, true);
					}
				}
			}
		}
	}

	public override void draw()
	{
		if (locked)
		{
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					Renderer.DrawSprite(position.x + x, position.y + y, 1, 1, 1, Gaem.instance.level.tileset, 9 * 8, 0, 8, 8);
					if (y == 0)
						Renderer.DrawVerticalSprite(position.x + x, position.y + y, 0, 1, 1, Gaem.instance.level.tileset, 9 * 8, 1 * 8, 8, 8);
				}
			}
		}
	}
}
