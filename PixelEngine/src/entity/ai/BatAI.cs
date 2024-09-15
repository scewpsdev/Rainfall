using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BatAI : AI
{
	float shootCooldown = 1.0f;

	public bool preferVerticalMovement = false;
	public bool canShoot = false;

	int walkDirection = 1;
	long targetLastSeen = -1;
	long lastShot = -1;

	List<Vector2i> currentPath = new List<Vector2i>();


	public BatAI(Mob mob)
		: base(mob)
	{
		aggroRange = 8.0f;
		loseRange = 12.0f;
		loseTime = 3.0f;
	}

	public override void onHit(Entity by)
	{
		if (target == null)
		{
			if (by is ItemEntity)
				by = ((ItemEntity)by).thrower;
			setTarget(by);
		}
	}

	void setTarget(Entity newTarget)
	{
		if (newTarget != null && target == null)
		{
			mob.speed *= 1.5f;
		}
		else if (newTarget == null && target != null)
		{
			mob.speed *= 1.0f / 1.5f;
		}
		target = newTarget;
	}

	bool updatePath(Vector2i currentTile, Vector2i targetTile)
	{
		currentPath.Clear();
		return GameState.instance.level.astar.run(currentTile, targetTile, currentPath, preferVerticalMovement);
	}

	void updateTargetFollow()
	{
		if (canSeeEntity(target, out Vector2 toTarget, out float distance))
			targetLastSeen = Time.currentTime;

		Vector2i currentTile = (Vector2i)Vector2.Floor(mob.position);
		Vector2i targetTile = (Vector2i)Vector2.Floor(target.position + target.collider.center);

		if (!updatePath(currentTile, targetTile))
		{
			setTarget(null);
			currentPath.Clear();
			return;
		}

		if (currentPath.Count > (canShoot ? 3 : 0))
		{
			Vector2i nextTile = currentPath.Count > 1 ? currentPath[1] : currentPath[0];
			float xdelta = nextTile.x + 0.5f - mob.position.x;
			float ydelta = nextTile.y + 0.5f - mob.position.y;

			if (xdelta < -0.1f)
				mob.inputLeft = true;
			else if (xdelta > 0.1f)
				mob.inputRight = true;
			if (ydelta < -0.1f)
				mob.inputDown = true;
			else if (ydelta > 0.1f)
				mob.inputUp = true;
		}

		if (canShoot && distance < 3.0f && target.position.y < mob.position.y - 0.1f)
		{
			if (lastShot == -1 || (Time.currentTime - lastShot) / 1e9f > shootCooldown)
			{
				Vector2 offset = Vector2.Zero;
				Vector2 direction = (toTarget + new Vector2(mob.direction, 0) * 0.1f).normalized;
				FireProjectile projectile = new FireProjectile(direction, mob.velocity, offset, mob, null);
				GameState.instance.level.addEntity(projectile, mob.position);

				lastShot = Time.currentTime;
			}
		}
	}

	void updatePatrol()
	{
		TileType forwardTile = GameState.instance.level.getTile(mob.position + new Vector2(0.5f * walkDirection, 0.0f));
		TileType forwardUpTile = GameState.instance.level.getTile(mob.position + new Vector2(0.5f * walkDirection, mob.collider.max.y + 0.05f));
		TileType forwardDownTile = GameState.instance.level.getTile(mob.position + new Vector2(0.5f * walkDirection, mob.collider.min.y - 0.05f));
		bool forwardBlocked = forwardTile != null && forwardTile.isSolid || forwardUpTile != null && forwardUpTile.isSolid || forwardDownTile != null && forwardDownTile.isSolid;
		if (forwardBlocked)
			walkDirection *= -1;

		if (walkDirection == 1)
			mob.inputRight = true;
		else if (walkDirection == -1)
			mob.inputLeft = true;
	}

	public override void update()
	{
		mob.inputRight = false;
		mob.inputLeft = false;
		mob.inputDown = false;
		mob.inputUp = false;

		if (target == null)
		{
			if (canSeeEntity(GameState.instance.player, out Vector2 toTarget, out float distance))
			{
				if (distance < aggroRange)
				{
					setTarget(GameState.instance.player);
				}
			}
		}

		if (target != null)
		{
			if ((target.position - mob.position).lengthSquared > loseRange * loseRange ||
				targetLastSeen != -1 && (Time.currentTime - targetLastSeen) / 1e9f > loseTime)
			{
				setTarget(null);
				targetLastSeen = -1;
			}
		}

		if (target != null)
			updateTargetFollow();
		else
			updatePatrol();
	}
}
