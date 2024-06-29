using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Arrow : Entity
{
	const float SPEED = 20;
	const float GRAVITY = -5;
	const int DAMAGE = 1;


	Sprite sprite;


	public Arrow(Vector2 direction)
	{
		velocity = direction * SPEED;

		sprite = new Sprite(Item.tileset, 2, 0);
	}

	public override void update()
	{
		velocity.y += GRAVITY * Time.deltaTime;
		position += velocity * Time.deltaTime;

		HitData hit = GameState.instance.level.sample(position, FILTER_DEFAULT | FILTER_MOB | FILTER_PLAYER);
		if (hit != null)
		{
			if (hit.entity != null && hit.entity != this)
			{
				if (hit.entity is Hittable)
				{
					Hittable hittable = hit.entity as Hittable;
					hittable.hit(DAMAGE, this);
				}
			}
			remove();
			//GameState.instance.level.addEntity(new ItemEntity(new Item(ItemType.Get("arrow"))));
		}
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y - 0.5f, 0, 1, 1, 0, sprite, false, 0xFFFFFFFF);
	}
}
