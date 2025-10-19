using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ArrowProjectile : Projectile
{
	public ArrowProjectile(Vector2 direction, Vector2 offset, Player player, Item bow, Item arrow)
		: base(direction * 40, Vector2.Zero, offset, player, arrow, arrow.baseDamage * bow.getAttackDamage(player))
	{
		maxSpeed = 40;
		gravity = -50;
		acceleration = 0;
		maxRicochets = 1;

		dropRange = bow.attackRange;

		sprite = new Sprite(Item.tileset, 2, 0);
	}

	public override void onHit(Vector2 normal)
	{
		HitData hit = GameState.instance.level.raycastSolid(position - velocity * Time.deltaTime, velocity.normalized, velocity.length * Time.deltaTime);
		if (hit != null)
		{
			ItemEntity entity = new ItemEntity(item, shooter);
			entity.rotation = velocity.angle + Mathf.RandomFloat(-0.05f, 0.05f);
			entity.stuck = true;
			Vector2 arrowPosition = position - velocity * Time.deltaTime + velocity.normalized * hit.distance;
			GameState.instance.level.addEntity(entity, arrowPosition);
		}

		/*
		TileType tile = GameState.instance.level.getTile(position);
		if (tile != null)
		{
			HitData hit = GameState.instance.level.raycastSolid(position - velocity * Time.deltaTime, velocity.normalized, velocity.length * Time.deltaTime);

			/*
			if (tile.breaksArrows)
			{
				ItemEntity entity = new ItemEntity(item);
				entity.rotation = velocity.angle + Mathf.RandomFloat(-0.05f, 0.05f);
				Vector2 arrowPosition = position;
				if (hit != null)
					arrowPosition = arrowPosition - velocity * Time.deltaTime + velocity.normalized * hit.distance - velocity.normalized * 0.5f;
				entity.rotationVelocity = Mathf.RandomFloat(-1, 1) * 10;
				GameState.instance.level.addEntity(entity, arrowPosition);
			}
			else
			/
			{
				ItemEntity entity = new ItemEntity(item, shooter);
				entity.rotation = velocity.angle + Mathf.RandomFloat(-0.05f, 0.05f);
				entity.stuck = true;
				Vector2 arrowPosition = position /*+ offset/;
				if (hit != null)
					arrowPosition = position - velocity * Time.deltaTime + velocity.normalized * hit.distance;
				GameState.instance.level.addEntity(entity, arrowPosition);
			}
		}
		*/
	}
}
