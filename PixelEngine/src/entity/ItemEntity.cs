using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


public class ItemEntity : Entity, Interactable, Hittable
{
	public float gravity = -20;
	public float bounciness = 0.5f;

	public int ricochets = 0;
	int pierces = 0;
	float damage;
	bool stuck = false;
	Vector2i stuckTile;

	public float rotationVelocity = 0;
	bool flipped;

	public Entity thrower = null;
	long throwTime;
	List<Entity> hitEntities = new List<Entity>();

	public Item item;
	public Vector4 color;
	uint outline = 0;

	ParticleEffect particles;


	public ItemEntity(Item item, Entity thrower = null, Vector2 velocity = default)
	{
		this.item = item;
		this.color = item.spriteColor;

		displayName = item.displayName;

		collider = item.collider;
		filterGroup = FILTER_ITEM;

		damage = item.attackDamage;

		this.thrower = thrower;
		this.velocity = velocity;
		throwTime = Time.currentTime;
	}

	public override void init(Level level)
	{
		if (item.hasParticleEffect)
		{
			particles = item.createParticleEffect(this);
			particles.layer = LAYER_INTERACTABLE - 0.01f;
			GameState.instance.level.addEntity(particles, position + item.particlesOffset);
		}
	}

	public override void destroy()
	{
		if (particles != null)
			particles.remove();
	}

	public void interact(Player player)
	{
		player.giveItem(item);
		player.hud.showMessage("Picked up " + item.fullDisplayName);
		remove();
	}

	public void onFocusEnter(Player player)
	{
		outline = OUTLINE_COLOR;
	}

	public void onFocusLeft(Player player)
	{
		outline = 0;
	}

	public bool hit(float damage, Entity by = null, Item _item = null, string byName = null, bool triggerInvincibility = true)
	{
		if (by is Projectile)
		{
			if (item.breakOnWallHit)
			{
				onHit(true, true);
			}
			else
			{
				velocity += by.velocity;
			}
			return true;
		}
		return false;
	}

	void onHit(bool x, bool y)
	{
		Vector2i pos = (Vector2i)Vector2.Floor(position + velocity.normalized * collider.size);

		if (item.projectileSticks)
		{
			stuck = true;
			stuckTile = pos;
			velocity = Vector2.Zero;
			//damage = 0;
		}
		else
		{
			if (x)
			{
				position.x -= velocity.x * Time.deltaTime;
				velocity.x = -velocity.x * bounciness;
			}
			else if (y)
			{
				position.y -= velocity.y * Time.deltaTime;
				velocity.y = -velocity.y * bounciness;
				velocity.x *= bounciness;
			}
		}

		if (damage > 0)
			damage = MathF.Max(damage - 1, 0);
		if (damage == 0)
			rotationVelocity = MathHelper.RandomFloat(-1, 1) * 20;

		if (velocity.lengthSquared > 1)
		{
			TileType tile = GameState.instance.level.getTile(pos);
			if (tile != null)
			{
				Vector2 normal = new Vector2(x ? MathF.Sign(velocity.x) : 0, y ? MathF.Sign(velocity.y) : 0);
				uint color = tile.particleColor;
				GameState.instance.level.addEntity(Effects.CreateImpactEffect(normal, velocity.length, MathHelper.ARGBToVector(color).xyz), position + velocity.normalized * 0.01f);
			}
		}

		if (item.breakOnWallHit && velocity.lengthSquared > 4 && ricochets >= item.maxRicochets && thrower != null)
		{
			item.onEntityBreak(this);
			remove();
		}
		else
		{
			ricochets++;
		}
	}

	public override void update()
	{
		if (stuck)
		{
			TileType tile = GameState.instance.level.getTile(stuckTile.x, stuckTile.y);
			if (tile == null || !tile.isSolid)
				stuck = false;
			else
				return;
		}

		velocity.y += gravity * Time.deltaTime;

		Vector2 displacement = velocity * Time.deltaTime;
		int collisionFlags = GameState.instance.level.doCollision(ref position, collider, ref displacement, true);
		position += displacement;

		{
			bool collidesX = (collisionFlags & Level.COLLISION_X) != 0;
			bool collidesY = (collisionFlags & Level.COLLISION_Y) != 0;
			if (collidesX || collidesY)
			{
				hitEntities.Clear();
				onHit(collidesX, collidesY);
			}
		}

		flipped = false;
		if (item.projectileItem && damage > 0 && thrower != null)
		{
			if (velocity.lengthSquared > 1.0f)
			{
				if (item.projectileSpins)
				{
					flipped = velocity.x < 0;
					rotation += (flipped ? -1 : 1) * rotationVelocity * Time.deltaTime;
				}
				else
				{
					rotation = MathF.Atan2(velocity.y, velocity.x) + item.projectileRotationOffset;
				}
				//flipped = velocity.x < 0;
			}
		}
		else if (item.projectileItem && damage == 0)
		{
			// Tumble
			if (velocity.lengthSquared > 0.25f)
			{
				rotation += rotationVelocity * Time.deltaTime;
				flipped = false;
			}
			else
			{
				if (MathF.Abs(rotation) < 0.5f * MathF.PI)
					rotation = MathHelper.Lerp(rotation, 0, 5 * Time.deltaTime);
				else
					rotation = MathHelper.LerpAngle(rotation, MathF.PI, 5 * Time.deltaTime);
			}
		}
		else
		{
			rotation = MathHelper.Lerp(rotation, 0, 5 * Time.deltaTime);
			flipped = false;
		}

		if (damage > 0 && item.projectileItem && thrower != null)
		{
			HitData hit = GameState.instance.level.raycast(position, velocity.normalized, 0.5f, FILTER_DEFAULT | FILTER_MOB | FILTER_PLAYER);
			if (hit != null)
			{
				bool skipHit = hit.entity == thrower && (Time.currentTime - throwTime) / 1e9f < 1.0f;

				if (!skipHit)
				{
					if (hit.entity != null && hit.entity != this)
					{
						if (hit.entity is Hittable && !hitEntities.Contains(hit.entity))
						{
							Hittable hittable = hit.entity as Hittable;
							hittable.hit(damage, this, item);
							hitEntities.Add(hit.entity);

							if (item.breakOnEnemyHit && velocity.lengthSquared > 4 && thrower != null)
							{
								item.onEntityBreak(this);
								remove();
							}
							else
							{
								if (pierces < item.maxPierces || item.maxPierces == -1)
									pierces++;
								else
								{
									damage = 0;
									bool collidesX = MathF.Abs(hit.normal.x) > 0.5f;
									bool collidesY = MathF.Abs(hit.normal.y) > 0.5f;
									if (collidesX || collidesY)
										onHit(collidesX, collidesY);
								}
							}
						}
					}
				}
			}
		}

		item.update(this);
	}

	void renderTooltip()
	{
		Vector2i pos = GameState.instance.camera.worldToScreen(position + new Vector2(0, 1));
		int direction = GameState.instance.player.position.x < position.x ? 1 : -1;

		string name = item.fullDisplayName;
		string rarityString = item.rarityString;

		int lineHeight = 16;
		int height = lineHeight + 12;
		int width = 1 + lineHeight + 5 + Math.Max(Renderer.MeasureUITextBMP(name).x, Renderer.MeasureUITextBMP(rarityString).x) + 4;
		int x = Math.Min(direction == 1 ? pos.x : pos.x - width, Renderer.UIWidth - width - 2);
		int y = Math.Max(pos.y - height, 2);

		Renderer.DrawUISprite(x - 1, y - 1, width + 2, height + 2, null, false, 0xFFAAAAAA);
		Renderer.DrawUISprite(x, y, width, height, null, false, 0xFF222222);

		Renderer.DrawUISprite(x + 1, y, lineHeight, lineHeight, item.sprite);
		Renderer.DrawUITextBMP(x + 1 + lineHeight + 5, y + 4, name, 1, 0xFFAAAAAA);

		Renderer.DrawUITextBMP(x + 1 + lineHeight + 5, y + lineHeight, rarityString, 1, 0xFF666666);
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f * item.size.x, position.y - 0.5f * item.size.y, LAYER_INTERACTABLE, item.size.x, item.size.y, rotation, item.sprite, flipped, color);

		if (outline != 0 && velocity.lengthSquared < 1 && GameState.instance.player.velocity.lengthSquared < 0.01f)
		{
			Renderer.DrawOutline(position.x - 0.5f * item.size.x, position.y - 0.5f * item.size.y, LAYER_INTERACTABLE, item.size.x, item.size.y, rotation, item.sprite, flipped, outline);

			//renderTooltip();
		}

		item.render(this);
	}
}
