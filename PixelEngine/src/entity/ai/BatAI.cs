using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BatAI : AI
{
	public float aggroRange = 8.0f;
	public float loseRange = 12.0f;
	public float loseTime = 3.0f;

	Mob mob;
	int walkDirection = 1;

	Entity target;
	long targetLastSeen = -1;

	List<Vector2i> currentPath = new List<Vector2i>();


	public BatAI(Mob mob)
	{
		this.mob = mob;
	}

	public void onHit(Entity by)
	{
		if (target == null)
			setTarget(by);
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
		return GameState.instance.level.astar.run(currentTile, targetTile, currentPath);
	}

	void updateTargetFollow()
	{
		Vector2 toPlayer = target.position + target.collider.center - mob.position;
		float distance = toPlayer.length;

		HitData hit = GameState.instance.level.raycastTiles(mob.position, toPlayer.normalized, distance + 0.1f);
		if (hit == null)
			targetLastSeen = Time.currentTime;

		Vector2i currentTile = (Vector2i)Vector2.Floor(mob.position);
		Vector2i targetTile = (Vector2i)Vector2.Floor(target.position + target.collider.center);

		if (!updatePath(currentTile, targetTile))
		{
			setTarget(null);
			currentPath.Clear();
			return;
		}

		if (currentPath.Count > 0)
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

		/*
		TileType forwardTile = GameState.instance.level.getTile(mob.position + new Vector2(0.5f * walkDirection, 0.0f));
		TileType forwardUpTile = GameState.instance.level.getTile(mob.position + new Vector2(0.5f * walkDirection, mob.collider.max.y + 0.05f));
		TileType forwardDownTile = GameState.instance.level.getTile(mob.position + new Vector2(0.5f * walkDirection, mob.collider.min.y - 0.05f));
		bool forwardBlocked = forwardTile != null && forwardTile.isSolid || forwardUpTile != null && forwardUpTile.isSolid || forwardDownTile != null && forwardDownTile.isSolid;

		float xdelta = target.position.x + target.collider.center.x - mob.position.x;
		float ydelta = target.position.y + 0.5f - mob.position.y;

		if (xdelta < 0)
			mob.inputLeft = true;
		else if (xdelta > 0)
			mob.inputRight = true;
		if (ydelta < 0 && (MathF.Abs(ydelta) > MathF.Abs(xdelta) || forwardBlocked))
			mob.inputDown = true;
		else if (ydelta > 0 && (MathF.Abs(ydelta) > MathF.Abs(xdelta) || forwardBlocked))
			mob.inputUp = true;
		*/
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

	public void update(Mob mob)
	{
		mob.inputRight = false;
		mob.inputLeft = false;
		mob.inputDown = false;
		mob.inputUp = false;

		if (target == null)
		{
			Player player = GameState.instance.player;
			Vector2 toPlayer = player.position + player.collider.center - mob.position;
			float distance = toPlayer.length;
			if (distance < aggroRange)
			{
				HitData hit = GameState.instance.level.raycastTiles(mob.position, toPlayer.normalized, distance + 0.1f);
				if (hit == null)
					setTarget(player);
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
