using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Door : Entity, Interactable
{
	int width, height;
	int cost;

	bool locked = true;


	public Door(int cost, int x, int y, int width, int height)
	{
		this.cost = cost;
		position = new Vector2(x, y);
		this.width = width;
		this.height = height;

		collider = new FloatRect(0, 0, width, height);
		staticCollider = true;
	}

	public bool canInteract(Entity entity)
	{
		return locked;
	}

	public string getInteractionPrompt(Entity entity)
	{
		return "Open [" + cost + "]";
	}

	public void interact(Entity entity)
	{
		if (entity is Player)
		{
			Player player = entity as Player;
			if (player.points >= cost)
			{
				player.points -= cost;
				locked = false;
				staticCollider = false;
			}
		}
	}

	public override void draw()
	{
		if (locked)
		{
			Renderer.DrawSprite(position.x, position.y, width, height, null, 0xFF222222);
		}
	}
}
