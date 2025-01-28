using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Garran : Mob
{
	public Garran()
		: base("garran")
	{
		displayName = "Garran";
		nameSubtitle = "Fallen Throne Sentinel";

		health = 110;
		poise = 10;
		speed = 2.5f;
		damage = 2;
		jumpPower = 18;
		gravity = -35;
		awareness = 1;
		itemDropChance = 2;
		itemDropValueMultiplier = 2;

		sprite = new Sprite(Resource.GetTexture("sprites/mob/castle/garran.png", false), 0, 0, 128, 64);
		collider = new FloatRect(-0.25f, 0, 0.5f, 2.0f);
		rect = new FloatRect(-4, -1, 8, 4);

		animator = new SpriteAnimator();
		animator.addAnimation("idle", 2, 1, 1, true);
		animator.addAnimation("run", 6, 1, true);
		animator.addAnimation("thrust0", 3, 1);
		animator.addAnimation("thrust1", 1, 1);
		animator.addAnimation("thrust2", 2, 1);
		animator.setAnimation("idle");

		AdvancedAI ai = new AdvancedAI(this);
		this.ai = ai;

		ai.hesitation = 0;

		{
			const float charge = 1.0f;
			const float duration = 0.2f;
			const float cooldown = 0.5f;
			const float distance = 10;
			const float speed = distance / duration;
			AIAction thrust = ai.addAction("thrust", charge, duration, cooldown, speed, distance, 4);
			thrust.actionColliders = [new FloatRect(0, 0.5f, 3, 1)];
		}
	}
}
