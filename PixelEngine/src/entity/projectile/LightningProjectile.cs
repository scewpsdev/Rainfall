using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LightningProjectile : Entity
{
	float speed = 200;
	int maxRicochets = 3;
	float maxDistance = 10;

	Entity shooter;
	Item item;

	Sprite sprite;
	Sprite trailHoriz, trailDiag;

	List<Vector2> cornerPoints = new List<Vector2>();
	Vector2 direction;

	int ricochets = 0;
	float distance = 0;
	Vector2 offset;

	List<Entity> hitEntities = new List<Entity>();


	public LightningProjectile(Vector2 direction, Vector2 offset, Entity shooter, Item item)
	{
		this.direction = direction;
		this.offset = offset;
		this.shooter = shooter;
		this.item = item;

		collider = new FloatRect(-0.1f, -0.1f, 0.2f, 0.2f);
		filterGroup = FILTER_PROJECTILE;

		velocity = direction * speed;

		sprite = new Sprite(Item.tileset, 9, 2);
		trailHoriz = new Sprite(new SpriteSheet(Resource.GetTexture("res/sprites/effects.png", false), 16, 16), 2, 0);
		trailDiag = new Sprite(new SpriteSheet(Resource.GetTexture("res/sprites/effects.png", false), 16, 16), 3, 0);
	}

	public override void init()
	{
		cornerPoints.Add(position);
	}

	public override void update()
	{
		Vector2 lastPosition = position;

		Vector2 displacement = velocity * Time.deltaTime;
		position += displacement;
		distance += displacement.length;

		if (distance >= maxDistance)
			remove();

		rotation = MathF.Atan2(velocity.y, velocity.x);

		offset = Vector2.Lerp(offset, Vector2.Zero, 3 * Time.deltaTime);

		HitData hit = GameState.instance.level.raycast(lastPosition, displacement.normalized, displacement.length, FILTER_MOB | FILTER_PLAYER | FILTER_DEFAULT);
		if (hit != null)
		{
			if (hit.entity != null)
			{
				Player player = shooter as Player;
				if (/*hit.entity != shooter &&*/ hit.entity is Hittable && !hitEntities.Contains(hit.entity))
				{
					Hittable hittable = hit.entity as Hittable;
					hittable.hit(item.attackDamage * player.attack, shooter, item);
					hitEntities.Add(hit.entity);
					//remove();
				}
			}
			else
			{
				if (ricochets >= maxRicochets)
					remove();
				else
				{
					cornerPoints.Add(position);
					Vector2 reflected = Vector2.Reflect(velocity, hit.normal);
					if (Vector2.Dot(velocity.normalized, hit.normal) < -0.9f)
					{
						float deviation = MathHelper.RandomFloat(-1, 1);
						deviation = MathF.Sign(deviation) * (1 - MathF.Pow(MathF.Abs(deviation), 2));
						reflected = Vector2.Rotate(reflected, MathF.PI * 0.25f * deviation);
					}
					position -= displacement;
					velocity = reflected;
					position += velocity * Time.deltaTime;
					ricochets++;
				}
			}
		}
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f + offset.x, position.y - 0.5f + offset.y, 0, 1, 1, rotation, sprite, false);

		for (int i = 0; i < cornerPoints.Count; i++)
		{
			Vector2 start = cornerPoints[i];
			Vector2 end = i < cornerPoints.Count - 1 ? cornerPoints[i + 1] : position;
			Vector2 direction = (end - start).normalized;
			float angle = direction.angle;
			float length = (end - start).length;

			for (int j = 0; j < (int)MathF.Ceiling(length); j++)
			{
				float fraction = MathF.Min(length - j, 1);
				Matrix transform = Matrix.CreateTranslation(start.x + direction.x * j, start.y + direction.y * j, 0) * Matrix.CreateRotation(Vector3.UnitZ, angle) * Matrix.CreateScale(fraction, 1, 1) * Matrix.CreateTranslation(0.5f, 0.0f, 0.0f);

				int u0 = trailHoriz.position.x;
				int v0 = trailHoriz.position.y;
				int w = (int)MathF.Round(fraction * trailHoriz.size.x);
				int h = trailHoriz.size.y;

				Renderer.DrawSprite(1, 1, transform, trailHoriz.spriteSheet.texture, u0, v0, w, h);
			}
		}
	}
}
