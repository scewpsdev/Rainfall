using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


public class LightningProjectile : Entity
{
	const float LIFETIME = 1.0f;


	float speed = 200;
	int maxRicochets = 3;
	float maxDistance = 25;

	float damage;

	Entity shooter;
	Item staff;
	Item spell;

	Sprite sprite;
	Texture lightning;
	Sprite trail, trailFade;

	bool active = true;
	long endTime;

	List<Vector3> cornerPoints = new List<Vector3>();
	Vector2 direction;

	int ricochets = 0;
	float distance = 0;
	Vector2 offset;

	List<Entity> hitEntities = new List<Entity>();


	public LightningProjectile(Vector2 direction, Vector2 offset, Entity shooter, Item staff, Item spell)
	{
		displayName = "Lightning";

		this.direction = direction;
		this.offset = offset;
		this.shooter = shooter;
		this.staff = staff;
		this.spell = spell;

		collider = new FloatRect(-0.1f, -0.1f, 0.2f, 0.2f);
		filterGroup = FILTER_PROJECTILE;

		velocity = direction * speed;

		damage = spell.attackDamage * staff.attackDamage;
		if (shooter is Player)
			damage *= (shooter as Player).getMeleeDamageModifier();

		sprite = new Sprite(Item.tileset, 9, 2);
		lightning = Resource.GetTexture("res/sprites/lightning.png", false);
		trail = new Sprite(new SpriteSheet(Resource.GetTexture("res/sprites/effects.png", false), 16, 16), 2, 0);
		trailFade = new Sprite(new SpriteSheet(Resource.GetTexture("res/sprites/effects.png", false), 16, 16), 3, 0);
	}

	public override void init(Level level)
	{
		cornerPoints.Add(new Vector3(position, Time.currentTime / 1e9f));

		endTime = Time.currentTime;
	}

	public override void update()
	{
		if (!active)
		{
			if (Time.currentTime / 1e9f - cornerPoints[0].z >= LIFETIME)
				remove();
			return;
		}

		endTime = Time.currentTime;

		Vector2 lastPosition = position;

		Vector2 displacement = velocity * Time.deltaTime;
		position += displacement;
		distance += displacement.length;

		Vector2 tangent = new Vector2(displacement.y, -displacement.x).normalized;

		if (distance >= maxDistance)
			active = false;

		rotation = MathF.Atan2(velocity.y, velocity.x);

		offset = Vector2.Lerp(offset, Vector2.Zero, 3 * Time.deltaTime);

		HitData hit = GameState.instance.level.raycast(lastPosition, displacement.normalized, displacement.length, FILTER_MOB | FILTER_PLAYER | FILTER_DEFAULT);
		if (hit == null)
			hit = GameState.instance.level.raycast(lastPosition + tangent * 0.1f, displacement.normalized, displacement.length, FILTER_MOB | FILTER_PLAYER | FILTER_DEFAULT);
		if (hit == null)
			hit = GameState.instance.level.raycast(lastPosition - tangent * 0.1f, displacement.normalized, displacement.length, FILTER_MOB | FILTER_PLAYER | FILTER_DEFAULT);
		if (hit != null)
		{
			if (hit.entity != null)
			{
				if (!(ricochets == 0 && hit.entity == shooter) && !hitEntities.Contains(hit.entity) && hit.entity is Hittable)
				{
					Hittable hittable = hit.entity as Hittable;
					hittable.hit(damage, this, spell);
					hitEntities.Add(hit.entity);
					damage = MathF.Max(damage - 1, 0);
					if (damage == 0)
						remove();
				}
			}
			else
			{
				if (ricochets >= maxRicochets)
					active = false;
				else
				{
					Vector2 reflected = Vector2.Reflect(velocity, hit.normal);

					if (Vector2.Dot(velocity.normalized, hit.normal) < -0.9f)
					{
						float deviation = MathHelper.RandomFloat(-1, 1);
						deviation = MathF.Sign(deviation) * (1 - MathF.Pow(MathF.Abs(deviation), 2));
						reflected = Vector2.Rotate(reflected, MathF.PI * 0.25f * deviation);
					}

					position = lastPosition + velocity.normalized * (hit.distance - 0.1f) + reflected.normalized * 0.1f;
					velocity = reflected;
					//position += velocity * Time.deltaTime;
					cornerPoints.Add(new Vector3(position, Time.currentTime / 1e9f));
					ricochets++;
				}
			}
		}
	}

	public override void render()
	{
		//if (active)
		//	Renderer.DrawSprite(position.x - 0.5f + offset.x, position.y - 0.5f + offset.y, 0, 1, 1, rotation, sprite, false, new Vector4(3.0f));

		for (int i = 0; i < cornerPoints.Count; i++)
		{
			Vector3 start = cornerPoints[i];
			Vector3 end = i < cornerPoints.Count - 1 ? cornerPoints[i + 1] : new Vector3(position, endTime / 1e9f);
			Vector2 direction = (end - start).xy.normalized;
			float angle = direction.angle;
			float length = (end - start).length;
			Vector2 center = (start.xy + end.xy) * 0.5f;

			float startTime = MathHelper.Lerp(start.z, end.z, 0.5f);
			float elapsed = Time.currentTime / 1e9f - startTime;
			float fade = MathF.Exp(-elapsed * 10);

			for (int j = 0; j < 3; j++)
			{
				uint seed = (uint)(i * 19 + j);// (uint)(Time.currentTime / 100000000);
				uint h = Hash.hash(seed);
				int u0 = (int)(h % lightning.width);
				int width = (int)(length * lightning.height);

				uint colorSelection = h % 3;
				Vector4 color = MathHelper.ARGBToVector(colorSelection == 0 ? 0xFF5b98ff : colorSelection == 1 ? 0xFF6edcff : 0xFFd6eeff) * new Vector4(1.5f);

				color.w *= fade;
				color.w *= MathF.Exp(-j * 3);

				Renderer.DrawSprite(center.x - 0.5f * length, center.y - 0.5f, 0, length, 1, angle, lightning, u0, 0, width, lightning.height, color, true);
			}

			Renderer.DrawLight(start.xy, new Vector3(0.5f, 0.9f, 1.0f) * 3 * fade, 0.5f * length);
			if (i == cornerPoints.Count - 1)
				Renderer.DrawLight(end.xy, new Vector3(0.5f, 0.9f, 1.0f) * 3 * fade, 0.5f * length);

			/*
			for (int j = 0; j < (int)MathF.Ceiling(length); j++)
			{
				float fraction = MathF.Min(length - j, 1);
				Matrix transform = Matrix.CreateTranslation(start.x + direction.x * j, start.y + direction.y * j, 0.0f) * Matrix.CreateRotation(Vector3.UnitZ, angle) * Matrix.CreateScale(fraction, 1, 1) * Matrix.CreateTranslation(0.5f, 0.0f, 0.0f);

				Sprite sprite = i < cornerPoints.Count - 1 || j < (int)MathF.Ceiling(length) - 1 ? trail : trailFade;

				int u0 = sprite.position.x;
				int v0 = sprite.position.y;
				int w = (int)MathF.Round(fraction * sprite.size.x);
				int h = sprite.size.y;

				float progress = (j + 0.5f) / MathF.Ceiling(length);
				float startTime = MathHelper.Lerp(start.z, end.z, progress);
				float elapsed = Time.currentTime / 1e9f - startTime;
				float alpha = MathF.Exp(-elapsed * 3.0f);
				Vector4 color = new Vector4(3, 3, 3, alpha);

				Renderer.DrawSprite(1, 1, transform, sprite.spriteSheet.texture, u0, v0, w, h, color, true);
			}
			*/
		}
	}
}
