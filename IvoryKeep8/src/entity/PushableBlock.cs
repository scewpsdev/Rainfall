using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PushableBlock : Entity, Hittable
{
	const float pushSpeed = 1;
	const float gravity = -20;


	Sprite sprite;
	int health = 6;


	public PushableBlock()
	{
		collider = new Hitbox(-0.49f, -0.5f, 0.98f, 1);
		filterGroup = FILTER_DEFAULT | FILTER_OBJECT;
		sprite = new Sprite(tileset, 5, 3);
	}

	public override void init(Level level)
	{
		level.addEntityCollider(this);
	}

	public override void destroy()
	{
		level.removeEntityCollider(this);
	}

	void breakBlock()
	{
		TileType tile = TileType.stone;
		Audio.PlayOrganic(tile.breakSound, new Vector3(position, 0));
		GameState.instance.level.addEntity(ParticleEffects.CreateBreakEffect(20, Mathf.ARGBToVector(tile.particleColor).xyz), position);
		remove();
	}

	public override void update()
	{
		HitData[] hits = new HitData[32];
		int numHits = level.overlap(position - new Vector2(0.6f, 0.3f), position + new Vector2(0.6f, 0.3f), hits, FILTER_PLAYER | FILTER_MOB | FILTER_OBJECT | FILTER_ITEM);

		float pushDelta = 0;
		for (int i = 0; i < numHits; i++)
		{
			if (hits[i].entity is Player)
			{
				Player player = GameState.instance.player;
				if (player.isGrounded)
				{
					if (player.inputRight && player.position.x < position.x)
						pushDelta += pushSpeed * Time.deltaTime;
					if (player.inputLeft && player.position.x > position.x)
						pushDelta -= pushSpeed * Time.deltaTime;
				}
			}
		}

		velocity.y += gravity * Time.deltaTime;

		Vector2 displacement = velocity * Time.deltaTime;
		displacement.x += pushDelta;
		int collisionFlags = GameState.instance.level.doCollision(ref position, collider, ref displacement, false);
		position += displacement;

		bool collidesX = (collisionFlags & Level.COLLISION_X) != 0;
		bool collidesY = (collisionFlags & Level.COLLISION_Y) != 0;

		if (collidesX)
		{
			velocity.x = 0;
			pushDelta = 0;
		}
		if (collidesY)
		{
			velocity.y = 0;
		}
		else
		{
			if (MathF.Abs(MathF.Round(position.x) - position.x) < 0.05f)
			{
				position.x = MathF.Round(position.x);
				velocity.x = 0;
			}
		}

		if (pushDelta != 0)
		{
			position.x += pushDelta;

			for (int j = 0; j < numHits; j++)
			{
				if (hits[j].entity is not Player && hits[j].entity != this)
				{
					if (MathF.Sign(hits[j].entity.position.x - position.x) == MathF.Sign(pushDelta))
						hits[j].entity.position.x += pushDelta;
				}
			}
		}
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y - 0.5f, 1, 1, sprite);
	}

	public bool hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true, bool buffedHit = false)
	{
		if (byName == "Explosion")
		{
			breakBlock();
			return true;
		}
		else
		{
			health--;
			if (health == 0)
				breakBlock();
			return true;
		}
	}
}
