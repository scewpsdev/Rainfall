using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ExplosiveBarrel : Entity, Hittable
{
	public float health = 4;

	float fuseTime = 0.75f;
	float blastRadius = 3.0f;
	float damage = 8;

	Sprite sprite;
	Sound fuseSound;
	uint source;

	long ignitedTime = -1;


	public ExplosiveBarrel()
	{
		sprite = new Sprite(TileType.tileset, 1, 1);
		collider = new FloatRect(-0.4f, 0.0f, 0.8f, 0.75f);

		fuseSound = Resource.GetSound("res/sounds/fuse.ogg");
	}

	public override void init(Level level)
	{
		Vector2i tile = (Vector2i)(position + Vector2.Up * 0.5f);
		level.setTile(tile.x, tile.y, TileType.dummyPlatform);
	}

	public override void destroy()
	{
		Vector2i tile = (Vector2i)(position + Vector2.Up * 0.5f);
		level.setTile(tile.x, tile.y, null);

		if (source != 0)
			Audio.Stop(source);
	}

	public bool hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true, bool buffedHit = false)
	{
		health -= damage;
		if (health <= 0)
			breakBarrel();
		else
		{
			//GameState.instance.level.addEntity(Effects.CreateDestroyWoodEffect(0xFF4c3f46), position);
			//GameState.instance.level.addEntity(Effects.CreateSparkEffect(), position);
		}
		if ((item != null && item.canIgnite || byName != null && byName == "Explosion" || by != null && by is Projectile) && ignitedTime == -1)
		{
			ignitedTime = Time.currentTime;
			source = Audio.PlayOrganic(fuseSound, new Vector3(position, 0));
			GameState.instance.level.addEntity(Effects.CreateDestroyWoodEffect(0xFF4c3f46), position);
		}
		return health <= 0;
	}

	void breakBarrel()
	{
		ignitedTime = Time.currentTime;
		GameState.instance.level.addEntity(Effects.CreateDestroyWoodEffect(0xFF4c3f46), position);
		GameState.instance.level.addEntity(Effects.CreateSparkEffect(), position);
		sprite = null;
	}

	void explode()
	{
		SpellEffects.Explode(position + Vector2.Up * 0.5f, blastRadius, damage, this, null);
		remove();
	}

	public override void update()
	{
		if (ignitedTime != -1)
		{
			if ((Time.currentTime - ignitedTime) / 1e9f > fuseTime)
			{
				explode();
				return;
			}

			if ((Time.currentTime - ignitedTime) / 1e9f % 1.0f * 10 < 1)
				GameState.instance.level.addEntity(Effects.CreateSparkEffect(), position);
		}

		TileType tile = GameState.instance.level.getTile(position - new Vector2(0, 0.01f));
		if (!(tile != null && (tile.isSolid || tile.isPlatform)))
		{
			velocity.y += -10 * Time.deltaTime;

			float displacement = velocity.y * Time.deltaTime;
			position.y += displacement;
		}
		else
		{
			if (MathF.Abs(velocity.y) > 5)
			{
				explode();
				return;
			}
		}
	}

	public override void render()
	{
		if (sprite != null)
		{
			Renderer.DrawSprite(position.x - 0.5f, position.y, LAYER_BG, 1, 1, 0, sprite);
			if (ignitedTime != -1 && (Time.currentTime - ignitedTime) / 1e9f * 20 % 1 > 0.5f)
				Renderer.DrawSpriteSolid(position.x - 0.5f, position.y, LAYER_BG - 0.001f, 1, 1, 0, sprite, false, 0xFFFFFFFF);
		}
	}
}
