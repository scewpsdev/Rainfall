using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Flail : Weapon
{
	Sprite chain;
	Sprite head;

	Vector2 headPosition;
	Vector2 headVelocity;
	Vector2 chainOrigin;
	float currentRange;


	public Flail()
		: base("flail")
	{
		displayName = "Flail";

		baseDamage = 1.5f;
		baseAttackRange = 3;
		baseAttackRate = 1.2f;
		attackCooldown = 0;
		postAttackLinger = 1;
		anim = AttackAnim.SwingOverhead;
		attackAcceleration = 1;
		twoHanded = true;
		baseWeight = 3;
		attackStartAngle = MathF.PI * 0.5f;
		attackEndAngle = MathF.PI * 2.5f;
		trigger = false;
		customAttackRender = true;

		strengthScaling = 0.5f;

		value = 34;

		sprite = new Sprite(tileset, 13, 9);
		renderOffset.x = -0.3f;
		renderOffset.y = -0.3f;

		chain = new Sprite(tileset, 14, 9);
		head = new Sprite(tileset, 15, 9);

		currentRange = 0;
	}

	protected override void getAttackAnim(Player player, int idx, out AttackAnim anim, out int swingDir, out float startAngle, out float endAngle, out float range)
	{
		base.getAttackAnim(player, idx, out anim, out swingDir, out startAngle, out endAngle, out range);
		startAngle = MathF.PI * 0.5f;
		endAngle = MathF.PI * 2.5f;

		/*
		if (player.direction == 1)
		{
			swingDir = 1;
			startAngle = MathF.PI * 2.25f;
			endAngle = MathF.PI * 0.25f;
		}
		else
		{
			swingDir = 0;
			startAngle = MathF.PI * 0.75f;
			endAngle = MathF.PI * -1.25f;
		}
		*/

		range = currentRange;
	}

	public override void update(Entity entity)
	{
		if (entity is Player && (entity as Player).isAlive)
		{
			Player player = entity as Player;

			if (player.actions.currentAction != null && player.actions.currentAction is AttackAction)
			{
				AttackAction attack = player.actions.currentAction as AttackAction;
				if (attack.weapon == this)
				{
					if (attack.elapsedTime < attack.duration)
					{
						if (currentRange < attackRange)
							currentRange = MathHelper.Lerp(currentRange, attackRange, 0.25f * Time.deltaTime);
						//currentRange = MathF.Min(currentRange + attackRange * Time.deltaTime, attackRange);
						attack.attackRange = currentRange;

						attack.charDirection = player.direction;
						attack.swingDir = player.direction == 1 ? 0 : 1;

						chainOrigin = attack.getWorldOrigin(player);
						Vector2 direction = attack.worldDirection;
						float range = attack.currentRange;

						headPosition = chainOrigin + direction * range;
						headVelocity = player.velocity + Vector2.Rotate(Vector2.Up, direction.angle) * 2 * MathF.PI * range * attackRate;
					}
					else
					{
						if (currentRange > 0.5f)
							currentRange = MathHelper.Lerp(currentRange, 0.5f, 1.5f * Time.deltaTime);
						//currentRange = MathF.Max(currentRange - attackRange * Time.deltaTime, 0.5f);
						attack.attackRange = currentRange;

						chainOrigin = player.position + player.collider.center;
						headVelocity.y += -20 * Time.deltaTime;
						headPosition += headVelocity * Time.deltaTime;

						Vector2 toHead = headPosition - chainOrigin;
						float distance = toHead.length;
						if (distance > currentRange)
						{
							toHead = toHead.normalized;
							headPosition = chainOrigin + currentRange * toHead;
							Vector2 toOut = Vector2.Dot(toHead, headVelocity) * toHead;
							headVelocity += -1.5f * toOut;
						}
					}
				}
			}
		}
	}

	public override void render(Entity entity)
	{
		if (entity is Player && (entity as Player).isAlive)
		{
			Player player = entity as Player;

			if (player.actions.currentAction != null && player.actions.currentAction is AttackAction && (player.actions.currentAction as AttackAction).weapon == this)
			{
				Vector2 chainCenter = (chainOrigin + headPosition) * 0.5f;
				Vector2 direction = headPosition - chainOrigin;
				float distance = direction.length;
				direction /= distance;

				Renderer.DrawSprite(headPosition.x - 0.5f, headPosition.y - 0.5f, 1, 1, head);
				Renderer.DrawSprite(chainCenter.x - 0.5f * distance, chainCenter.y - 0.5f, 0, distance, 1, direction.angle, chain);
			}
		}
	}
}
