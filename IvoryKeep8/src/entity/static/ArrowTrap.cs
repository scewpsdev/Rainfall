using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ArrowTrap : Entity, Hittable
{
	const float SPEED = 20;
	const float RANGE = 8;


	Vector2 direction;
	bool hasAmmo = true;

	Sprite sprite;


	public ArrowTrap(Vector2 direction)
	{
		this.direction = direction;

		sprite = new Sprite(tileset, 2, 1);

		collider = new Hitbox(0, 0, 1, 1);
	}

	public bool hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true, bool buffedHit = false)
	{
		if (by is Projectile && (by as Projectile).shooter == this)
			return true;
		shoot();
		return true;
	}

	public override void update()
	{
		if (hasAmmo)
		{
			if (GameState.instance.level.raycast(position + new Vector2(0.5f) + direction, direction, RANGE, out HitData hit, FILTER_PLAYER | FILTER_MOB | FILTER_ITEM | FILTER_PROJECTILE))
			{
				if (hit.entity != null)
					shoot();
			}
		}
	}

	void shoot()
	{
		Vector2 velocity = (direction + new Vector2(0, 0.1f)) * SPEED;
		GameState.instance.level.addEntity(new ArrowProjectile(direction.normalized, Vector2.Zero, this, null, null) { damage = 2 }, position + new Vector2(0.5f) + direction.normalized * 0.7f);
		hasAmmo = false;
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x, position.y, 0, 1, 1, 0, sprite, direction.x < 0, 0xFFFFFFFF);
	}
}
