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
	float height;

	AudioSource audio;
	Sound[] sfxShoot;
	Sound sfxHit;

	long birthTime;
	List<Entity> penetratedEntities = new List<Entity>();
	int numRicochets = 0;


	public Bullet(Entity shooter, int damage, Vector2 position, Vector2 direction)
	{
		this.shooter = shooter;
		this.damage = damage;
		base.position = position;
		this.direction = direction;

		size = new Vector2(1.0f);

		height = 1.0f;

		sheet = new SpriteSheet(Resource.GetTexture("res/sprites/glyphs.png", false), 8, 8);
		sprite = new Sprite(sheet, 0, 0);
		animator = new SpriteAnimator();

		animator.addAnimation("default", Random.Shared.Next() % 16, 0, 1, 0, 16, 16, true);
		animator.setAnimation("default");

		collider = new FloatRect(-0.25f * size.x, 0.0f, 0.5f * size.x, 0.5f * size.y);

		audio = Audio.CreateSource(new Vector3(position, 0.0f));

		sfxShoot = [
			Resource.GetSound("res/sounds/shoot1.ogg"),
			Resource.GetSound("res/sounds/shoot2.ogg"),
			Resource.GetSound("res/sounds/shoot3.ogg"),
			Resource.GetSound("res/sounds/shoot4.ogg"),
			Resource.GetSound("res/sounds/shoot5.ogg"),
			Resource.GetSound("res/sounds/shoot6.ogg"),
			Resource.GetSound("res/sounds/shoot7.ogg"),
			Resource.GetSound("res/sounds/shoot8.ogg"),
			Resource.GetSound("res/sounds/shoot9.ogg"),
			Resource.GetSound("res/sounds/shoot10.ogg"),
			Resource.GetSound("res/sounds/shoot11.ogg"),
		];
		sfxHit = Resource.GetSound("res/sounds/shoot_hit.ogg");

		audio.playSoundOrganic(sfxShoot, 0.3f);

		birthTime = Time.currentTime;
	}

	public override void reset()
	{
		removed = true;
	}

	public override void destroy()
	{
		Audio.DestroySource(audio);
	}

	public override void update()
	{
		Vector2 velocity = direction * SPEED;
		Vector2 delta = velocity * Time.deltaTime;

		CollisionDetection.DoWallCollision(position, collider, ref delta, level, out bool collidesX, out bool collidesY);
		if (collidesX || collidesY)
		{
			onWallHit();
			numRicochets++;
			if (numRicochets > Gaem.instance.player.ricochet)
				removed = true;
			else
			{
				delta -= velocity.normalized * 0.05f;
				if (collidesX)
				{
					velocity.x *= -1;
					direction.x *= -1;
				}
				if (collidesY)
				{
					velocity.y *= -1;
					direction.y *= -1;
				}
			}
		}

		List<Entity> hitEntities = CollisionDetection.OverlapEntities(position, collider, level);
		if (hitEntities.Count > 0)
		{
			foreach (Entity hitEntity in hitEntities)
			{
				if (hitEntity != this && hitEntity != shooter && hitEntity is Enemy && !penetratedEntities.Contains(hitEntity))
				{
					onEnemyHit(hitEntity);
					penetratedEntities.Add(hitEntity);
					break;
				}
			}
		}

		if (penetratedEntities.Count > Gaem.instance.player.penetration)
			removed = true;

		if ((Time.currentTime - birthTime) / 1e9f >= MAX_LIFETIME)
		{
			removed = true;
		}

		position += delta;
		height = MathHelper.Lerp(height, 0.01f, 3 * Time.deltaTime);

		animator.update(sprite);

		audio.updateTransform(new Vector3(position, 0.0f));
	}

	void onWallHit()
	{
		level.addEntity(new ParticleEffect(MathHelper.RandomInt(5, 15), 0xFFAAAAFF, sfxHit, position + 0.25f * direction, -direction));
	}

	void onEnemyHit(Entity entity)
	{
		level.addEntity(new ParticleEffect(MathHelper.RandomInt(5, 15), 0xFFAAAAFF, sfxHit, position + 0.25f * direction, -direction));
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
		Renderer.DrawSprite(position.x - 0.5f * size.x, position.y - 0.5f * size.y, height, size.x, size.y, 0, sprite, false, 0xFFAAAAFF);
		Renderer.DrawLight(position + new Vector2(0.0f, height), new Vector3(1.0f, 1.0f, 1.7f), 2.0f);
	}
}
