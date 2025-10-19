using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class StalkerAI : AI
{
	Vector2i walkDirection;
	Vector2i downVector;

	float walkSpeed;
	float patrolSpeed;


	public StalkerAI(Mob mob)
		: base(mob)
	{
		//aggroRange = 0;

		walkSpeed = mob.speed;
		patrolSpeed = 0.25f;
	}

	public override void update()
	{
		mob.inputRight = false;
		mob.inputLeft = false;
		mob.inputDown = false;
		mob.inputUp = false;

		mob.gravity = 0;

		mob.speed = target != null ? walkSpeed : patrolSpeed;

		if (walkDirection == Vector2i.Zero)
		{
			walkDirection = new Vector2i(mob.direction, 0);
			if (mob.level.raycastTiles(mob.position, Vector2.Down, 1) != null)
				downVector = Vector2i.Down;
			else
				downVector = Vector2i.Up;
		}

		Vector2 position = mob.position + mob.collider.center;
		Vector2 delta = walkDirection * mob.speed * Time.deltaTime;
		bool forward = mob.level.sampleTiles(position + walkDirection * 0.25f + delta + downVector * 0.2f) || mob.level.sampleTiles(position + walkDirection * 0.25f + delta - downVector * 0.2f);
		bool down = mob.level.sampleTiles(position - walkDirection * 0.25f + downVector * 0.5f);
		bool backDown = mob.level.sampleTiles(position - walkDirection * 0.5f + downVector * 0.5f);
		if (forward)
		{
			MathHelper.Swap(ref walkDirection, ref downVector);
			walkDirection *= -1;
		}
		else if (!down && backDown)
		{
			MathHelper.Swap(ref walkDirection, ref downVector);
			downVector *= -1;
		}
		else if (!mob.level.overlapTiles(position - 0.5f, position + 0.5f))
		{
			mob.gravity = -10;
			walkDirection = Vector2i.Right;
			downVector = Vector2i.Down;
		}

		if (walkDirection.x == 1)
			mob.inputRight = true;
		else if (walkDirection.x == -1)
			mob.inputLeft = true;
		if (walkDirection.y == 1)
			mob.inputUp = true;
		else if (walkDirection.y == -1)
			mob.inputDown = true;

		mob.rotation = MathHelper.LinearAngle(mob.rotation, ((Vector2)downVector).angle + MathF.PI * 0.5f, 10 * Time.deltaTime);
		//mob.direction = walkDirection.x == -downVector.y ? 1 : -1;

		mob.animator.setAnimation("idle");
	}
}
