using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


public class LightOrb : Entity
{
	float speed = 5;
	float acceleration = -1;
	float rotationSpeed = 1.0f;

	Entity shooter;
	Item item;

	Sprite sprite;
	Vector2 direction;

	Vector2 offset;

	List<Entity> hitEntities = new List<Entity>();


	public LightOrb(Vector2 direction, Vector2 startVelocity, Vector2 offset, Entity shooter, Item item)
	{
		this.direction = direction;
		this.offset = offset;
		this.shooter = shooter;
		this.item = item;

		collider = new FloatRect(-0.1f, -0.1f, 0.2f, 0.2f);
		filterGroup = FILTER_PROJECTILE;

		velocity = direction * speed;
		if (MathF.Sign(velocity.x) == MathF.Sign(startVelocity.x) && MathF.Abs(startVelocity.x) > MathF.Abs(velocity.x))
			velocity.x = startVelocity.x;

		sprite = new Sprite(Item.tileset, 6, 4);
	}

	public override void update()
	{
		velocity += velocity.normalized * acceleration * Time.deltaTime;

		Vector2 displacement = velocity * Time.deltaTime;
		position += displacement;

		rotation += rotationSpeed * Time.deltaTime;

		offset = Vector2.Lerp(offset, Vector2.Zero, 3 * Time.deltaTime);

		HitData hit = GameState.instance.level.raycastTiles(position - displacement, displacement.normalized, displacement.length);
		if (hit != null)
		{
			velocity = Vector2.Reflect(velocity, hit.normal);
			velocity *= 0.7f;
			position += velocity * 0.01f;
		}
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f + offset.x, position.y - 0.5f + offset.y, 0, 1, 1, rotation, sprite, false, new Vector4(1.0f), true);
		Renderer.DrawLight(position, MathHelper.ARGBToVector(0xFFffecb5).xyz * 4, 10);
	}
}
