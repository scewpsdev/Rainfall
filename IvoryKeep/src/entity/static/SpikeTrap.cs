using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class SpikeTrap : Entity, Hittable
{
	int damage = 4;
	float gravity = -25;
	float startVelocity = -5;

	Sprite sprite;

	bool falling = false;
	bool hitGround = false;
	List<Entity> hitEntities = new List<Entity>();


	public SpikeTrap()
	{
		displayName = "Spike Trap";

		collider = new FloatRect(-0.25f, -0.5f, 0.5f, 1.0f);
		filterGroup = FILTER_PROJECTILE;

		sprite = new Sprite(tileset, 0, 4);
	}

	public bool hit(float damage, Entity by, Item item, string byName, bool triggerInvincibility, bool buffedHit = false)
	{
		TileType tile = TileType.dirt;
		Audio.PlayOrganic(tile.breakSound, new Vector3(position, 0));
		GameState.instance.level.addEntity(ParticleEffects.CreateBreakEffect(20, Mathf.ARGBToVector(tile.particleColor).xyz), position);
		remove();
		return true;
	}

	public void trigger()
	{
		falling = true;
		velocity.y = startVelocity;
	}

	public override void update()
	{
		if (hitGround)
		{
			if (velocity.y < -2)
			{
				GameState.instance.level.addEntity(ParticleEffects.CreateImpactEffect(Vector2.Up, MathF.Abs(velocity.y), Mathf.ARGBToVector(0xFF47362a).xyz), position);
				Audio.PlayOrganic(Resource.GetSound("sounds/arrow_hit.ogg"), new Vector3(position, 0));
				velocity.y = 0;
			}
		}
		else if (!falling)
		{
			HitData hit = GameState.instance.level.raycast(position + new Vector2(0, -1), new Vector2(0, -1), 10, FILTER_PLAYER | FILTER_MOB | FILTER_ITEM | FILTER_PROJECTILE | FILTER_OBJECT);
			if (hit != null && hit.entity != null)
			{
				Console.WriteLine(hit.entity);
				trigger();
				GameState.instance.level.addEntity(ParticleEffects.CreateImpactEffect(Vector2.Down, 8, Mathf.ARGBToVector(0xFF47362a).xyz), position);
			}
		}
		else if (falling)
		{
			velocity.y += gravity * Time.deltaTime;
			position.y += velocity.y * Time.deltaTime;

			rotation = velocity.angle + MathF.PI * 0.5f;

			if (GameState.instance.level.overlapSolid(position + new Vector2(-0.25f, -0.3f), position + new Vector2(0.25f, 0.0f)))
				hitGround = true;

			HitData[] hits = new HitData[16];
			int numHits = GameState.instance.level.overlap(position + new Vector2(-0.25f, -0.3f), position + new Vector2(0.25f, 0.0f), hits, FILTER_PLAYER | FILTER_MOB | FILTER_DEFAULT);
			for (int i = 0; i < numHits; i++)
			{
				HitData hit = hits[i];
				if (hit.entity != null && hit.entity != this && !hitEntities.Contains(hit.entity))
				{
					if (hit.entity is Hittable)
					{
						Hittable hittable = hit.entity as Hittable;
						hittable.hit(damage, this, null);
						hitEntities.Add(hit.entity);
					}
				}
			}
		}

		TileType tile = GameState.instance.level.getTile(position + new Vector2(0.0f, 1.0f));
		if (tile == null && !falling)
		{
			trigger();
		}
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y - 0.5f, 0, 1, 1, rotation, sprite);
	}
}
