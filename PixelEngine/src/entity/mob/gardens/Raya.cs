using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Raya : Mob
{
	public Raya()
		: base("raya")
	{
		displayName = "Raya the Explorer";

		health = 65;
		poise = 10;
		speed = 1.5f;
		damage = 2;
		jumpPower = 16;
		gravity = -30;
		awareness = 1;
		itemDropChance = 2;

		sprite = new Sprite(Resource.GetTexture("res/sprites/mob/gardens/raya.png", false), 0, 0, 64, 64);
		collider = new FloatRect(-0.25f, 0, 0.5f, 1.4f);
		rect = new FloatRect(-2, 0, 4, 4);

		animator = new SpriteAnimator();
		animator.addAnimation("idle", 0, 0, 64, 0, 2, 1, true);
		animator.addAnimation("run", 2 * 64, 0, 64, 0, 8, 8, true);
		animator.addAnimation("attack_dash_charge", 10 * 64, 0, 64, 0, 3, 3, false);
		animator.addAnimation("attack_dash", 13 * 64, 0, 64, 0, 2, 6, false);
		animator.addAnimation("attack_dash_cooldown", 15 * 64, 0, 64, 0, 4, 3, false);
		animator.setAnimation("idle");

		AdvancedAI ai = new AdvancedAI(this);
		this.ai = ai;

		const float dashDuration = 0.33f;
		const float dashDistance = 8;
		const float dashSpeed = dashDistance / dashDuration;
		const float dashTriggerDistance = 6;
		const float dashCharge = 0.5f;
		const float dashCooldown = 0.7f;
		AIAction dashAttack = ai.addAction("attack_dash", dashDuration, "attack_dash_charge", dashCharge, "attack_dash_cooldown", dashCooldown, dashSpeed, (AIAction action, Vector2 toTarget, float targetDistance) => targetDistance < dashTriggerDistance);
	}
}
