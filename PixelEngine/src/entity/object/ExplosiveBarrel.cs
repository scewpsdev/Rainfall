using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ExplosiveBarrel : Entity, Hittable
{
	float fuseTime = 0.5f;
	float blastRadius = 3.0f;
	float damage = 8;

	Sprite sprite;

	long ignitedTime = -1;


	public ExplosiveBarrel()
	{
		sprite = new Sprite(TileType.tileset, 1, 1);
		collider = new FloatRect(-0.4f, 0.0f, 0.8f, 0.75f);
	}

	public void hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true)
	{
		breakBarrel();
		if (ignitedTime == -1)
			ignitedTime = Time.currentTime;
	}

	void breakBarrel()
	{
		GameState.instance.level.addEntity(Effects.CreateDestroyWoodEffect(0xFF4c3f46), position);
		GameState.instance.level.addEntity(Effects.CreateSparkEffect(), position);
		sprite = null;
	}

	void explode()
	{
		Effects.Explode(position, blastRadius, damage, this, null);
		remove();
	}

	public override void update()
	{
		if (ignitedTime != -1 && (Time.currentTime - ignitedTime) / 1e9f > fuseTime)
		{
			explode();
			return;
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
			Renderer.DrawSprite(position.x - 0.5f, position.y, LAYER_BG, 1, 1, 0, sprite);
	}
}
