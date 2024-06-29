using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ArrowTrap : Entity
{
	const float RANGE = 8;


	Vector2 direction;
	bool hasAmmo = true;

	Sprite sprite;


	public ArrowTrap(Vector2 direction)
	{
		this.direction = direction;

		sprite = new Sprite(TileType.tileset, 2, 1);
	}

	public override void update()
	{
		if (hasAmmo)
		{
			HitData hit = GameState.instance.level.raycast(position + new Vector2(0.5f) + direction, direction, RANGE, FILTER_PLAYER | FILTER_MOB);
			if (hit != null)
			{
				if (hit.entity != null)
					shoot();
			}
		}
	}

	void shoot()
	{
		GameState.instance.level.addEntity(new Arrow(direction + new Vector2(0.0f, 0.1f)), position + new Vector2(0.5f) + direction);
		hasAmmo = false;
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x, position.y, 0, 1, 1, 0, sprite, direction.x < 0, 0xFFFFFFFF);
	}
}
