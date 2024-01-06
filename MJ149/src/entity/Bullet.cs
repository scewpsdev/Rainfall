using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Bullet : Entity
{
	const float SPEED = 25;
	const float MAX_LIFETIME = 5.0f;


	SpriteSheet sheet;
	Sprite sprite;
	SpriteAnimator animator;

	public readonly Entity shooter;
	int damage;

	Vector2 direction;

	long birthTime;


	public Bullet(Entity shooter, int damage, Vector2 position, Vector2 direction)
	{
		this.shooter = shooter;
		this.damage = damage;
		base.position = position;
		this.direction = direction;

		size = new Vector2(1.0f);

		sheet = new SpriteSheet(Resource.GetTexture("res/sprites/glyphs.png", false), 8, 8);
		sprite = new Sprite(sheet, 0, 0);
		animator = new SpriteAnimator();

		animator.addAnimation("default", Random.Shared.Next() % 16, 0, 1, 0, 16, 8, true);
		animator.setAnimation("default");

		collider = new FloatRect(-0.25f * size.x, 0.0f, 0.5f * size.x, 0.5f * size.y);

		birthTime = Time.currentTime;
	}

	public override void reset()
	{
		removed = true;
	}

	public override void update()
	{
		Vector2 velocity = direction * SPEED;
		Vector2 delta = velocity * Time.deltaTime;

		CollisionDetection.DoWallCollision(position, collider, ref delta, level, out bool collidesX, out bool collidesY);
		if (collidesX || collidesY)
		{
			onWallHit();
			removed = true;
		}

		List<Entity> hitEntities = CollisionDetection.OverlapEntities(position, collider, level);
		if (hitEntities.Count > 0)
		{
			foreach (Entity hitEntity in hitEntities)
			{
				if (hitEntity != this && hitEntity != shooter && hitEntity is Enemy)
				{
					onEnemyHit(hitEntity);
					removed = true;
					break;
				}
			}
		}

		if ((Time.currentTime - birthTime) / 1e9f >= MAX_LIFETIME)
		{
			removed = true;
		}

		position += delta;

		animator.update(sprite);
	}

	void onWallHit()
	{
		level.addEntity(new ParticleEffect(MathHelper.RandomInt(5, 15), 0xFFAAAAFF, position + 0.25f * direction, -direction));
		// sound effect
	}

	void onEnemyHit(Entity entity)
	{
		// particles
		// sound effect
		if (entity is Player)
		{
			((Player)entity).hit(this);
		}
		else if (entity is Enemy)
		{
			((Enemy)entity).hit(damage, this);
		}
	}

	public override void draw()
	{
		Renderer.DrawSprite(position.x - 0.5f * size.x, position.y - 0.5f * size.y, 1, size.x, size.y, 0, sprite, false, 0xFFAAAAFF);
	}
}
