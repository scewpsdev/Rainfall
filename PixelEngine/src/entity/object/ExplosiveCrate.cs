using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;


public class ExplosiveCrate : Crate
{
	float fuseTime = 0.75f;
	float blastRadius = 3.0f;
	float damage = 8;

	Sound fuseSound;
	uint source;

	long ignitedTime = -1;


	public ExplosiveCrate()
	{
		sprite = new Sprite(tileset, 9, 1);

		fuseSound = Resource.GetSound("sounds/fuse.ogg");

		health = 2;
	}

	public override void destroy()
	{
		base.destroy();

		if (source != 0)
			Audio.Stop(source);
	}

	public override bool hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true, bool buffedHit = false)
	{
		if (base.hit(damage, by, item, byName, triggerInvincibility, buffedHit))
		{
			if ((item != null && item.canIgnite || byName != null && byName == "Explosion" || by != null && by is Projectile) && ignitedTime == -1)
			{
				ignitedTime = Time.currentTime;
				source = Audio.PlayOrganic(fuseSound, new Vector3(position, 0));
				GameState.instance.level.addEntity(ParticleEffects.CreateDestroyWoodEffect(0xFF4c3f46), position);
			}
			return true;
		}
		return false;
	}

	protected override void breakBarrel()
	{
		ignitedTime = Time.currentTime;
		GameState.instance.level.addEntity(ParticleEffects.CreateDestroyWoodEffect(0xFF4c3f46), position);
		GameState.instance.level.addEntity(ParticleEffects.CreateSparkEffect(), position);
		Audio.PlayOrganic(breakSound, new Vector3(position, 0));
		sprite = null;
	}

	void explode()
	{
		SpellEffects.Explode(position + Vector2.Up * 0.5f, blastRadius, damage, this, null);
		remove();
	}

	public override void update()
	{
		base.update();

		if (ignitedTime != -1)
		{
			if ((Time.currentTime - ignitedTime) / 1e9f > fuseTime)
			{
				explode();
				return;
			}

			if ((Time.currentTime - ignitedTime) / 1e9f % 1.0f * 10 < 1)
				GameState.instance.level.addEntity(ParticleEffects.CreateSparkEffect(), position);
		}
	}

	public override void render()
	{
		base.render();

		if (sprite != null)
		{
			if (ignitedTime != -1 && (Time.currentTime - ignitedTime) / 1e9f * 20 % 1 > 0.5f)
				Renderer.DrawSpriteSolid(position.x + rect.position.x, position.y + rect.position.y, LAYER_BG - 0.001f, rect.size.x, rect.size.y, rotation, sprite, false, 0xFFFFFFFF);
		}
	}
}
