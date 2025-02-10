using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ExplosiveObject : Object
{
	static Sound fuseSound;

	static ExplosiveObject()
	{
		fuseSound = Resource.GetSound("sounds/fuse.ogg");
		fuseSound.singleInstance = true;
	}


	float fuseTime = 0.75f;
	float blastRadius = 3.0f;
	float blastDamage = 8;

	uint source;

	long ignitedTime = -1;

	protected Sound[] breakSound;


	public ExplosiveObject()
	{
		health = 2;
		damage = 1;
	}

	public override bool hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true, bool buffedHit = false)
	{
		base.hit(damage, by, item, byName, triggerInvincibility, buffedHit);

		if (health <= 0)
			breakObject();
		if ((item != null && item.canIgnite || byName != null && byName == "Explosion" || by != null && by is Projectile) && ignitedTime == -1)
		{
			ignitedTime = Time.currentTime;
			source = Audio.PlayOrganic(fuseSound, new Vector3(position, 0));
		}

		return true;
	}

	protected virtual void breakObject()
	{
		if (breakSound != null)
			Audio.PlayOrganic(breakSound, new Vector3(position, 0));

		ignitedTime = Time.currentTime;
		GameState.instance.level.addEntity(ParticleEffects.CreateSparkEffect(), position);
		sprite = null;
	}

	void explode()
	{
		SpellEffects.Explode(position + Vector2.Up * 0.5f, blastRadius, blastDamage, this, null);
		remove();
	}

	protected override void onCollision(bool x, bool y, bool isEntity)
	{
		if (MathF.Abs(velocity.y) > 5)
			explode();

		base.onCollision(x, y, isEntity);
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

		if (sprite != null && ignitedTime != -1 && (Time.currentTime - ignitedTime) / 1e9f * 20 % 1 > 0.5f)
			Renderer.DrawSpriteSolid(position.x + rect.position.x, position.y + rect.position.y, LAYER_BG - 0.001f, rect.size.x, rect.size.y, rotation, sprite, false, 0xFFFFFFFF);
	}
}
