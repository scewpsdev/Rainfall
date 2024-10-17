using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ArrowProjectile : Projectile
{
	public ArrowProjectile(Vector2 direction, Vector2 offset, Entity shooter, Item bow, Item item)
		: base(bow != null ? direction * bow.attackRange : direction * 10, Vector2.Zero, offset, shooter, item)
	{
		maxSpeed = bow != null ? bow.attackRange : 10;
		maxRange = 15;
		acceleration = 0;
		maxRicochets = 0;
		damage = bow != null ? bow.attackDamage : 1;

		sprite = new Sprite(Item.tileset, 2, 0);
	}

	public override void onHit(Vector2 normal)
	{
		TileType tile = GameState.instance.level.getTile(position);
		if (tile != null)
		{
			HitData hit = GameState.instance.level.raycastTiles(position - velocity * Time.deltaTime, velocity.normalized, velocity.length * Time.deltaTime);

			if (item != null)
			{
				if (tile.breaksArrows)
				{
					ItemEntity entity = new ItemEntity(item);
					entity.rotation = velocity.angle + MathHelper.RandomFloat(-0.05f, 0.05f);
					Vector2 arrowPosition = position;
					if (hit != null)
						arrowPosition = arrowPosition - velocity * Time.deltaTime + velocity.normalized * hit.distance - velocity.normalized * 0.5f;
					entity.rotationVelocity = MathHelper.RandomFloat(-1, 1) * 10;
					GameState.instance.level.addEntity(entity, arrowPosition);
				}
				else
				{
					ItemEntity entity = new ItemEntity(item, shooter);
					entity.rotation = velocity.angle + MathHelper.RandomFloat(-0.05f, 0.05f);
					Vector2 arrowPosition = position;
					if (hit != null)
						arrowPosition = arrowPosition - velocity * Time.deltaTime + velocity.normalized * hit.distance;
					GameState.instance.level.addEntity(entity, arrowPosition);
				}
			}
		}
	}
}
