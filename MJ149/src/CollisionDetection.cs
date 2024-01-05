using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class CollisionDetection
{
	public static bool OverlapWalls(Vector2 position, Vector2 size, Level level)
	{
		int x0 = (int)MathF.Floor(position.x);
		int y0 = (int)MathF.Floor(position.y);
		int x1 = (int)MathF.Floor(position.x + size.x);
		int y1 = (int)MathF.Floor(position.y + size.y);
		for (int y = y0; y <= y1; y++)
		{
			for (int x = x0; x <= x1; x++)
			{
				uint tile = level.getTile(x, y);
				if (tile != 0)
					return true;
			}
		}
		foreach (Entity entity in level.entities)
		{
			if (entity.staticCollider)
			{
				if (position.x + size.x > entity.position.x + entity.collider.min.x &&
					position.x < entity.position.x + entity.collider.max.x &&
					position.y + size.y > entity.position.y + entity.collider.min.y &&
					position.y < entity.position.y + entity.collider.max.y)
					return true;
			}
		}
		return false;
	}

	public static List<Entity> OverlapEntities(Vector2 position, Vector2 size, Level level)
	{
		List<Entity> result = new List<Entity>();

		foreach (Entity entity in level.entities)
		{
			if (entity.collider != null)
			{
				if (entity.position.x + entity.collider.max.x > position.x &&
					entity.position.y + entity.collider.max.y > position.y &&
					entity.position.x + entity.collider.min.x < position.x + size.x &&
					entity.position.y + entity.collider.min.y < position.y + size.y)
					result.Add(entity);
			}
		}

		return result;
	}

	public static List<Entity> OverlapEntities(Vector2 position, FloatRect collider, Level level)
	{
		return OverlapEntities(position + collider.position, collider.size, level);
	}

	public static void DoWallCollision(Vector2 position, FloatRect collider, ref Vector2 delta, Level level, out bool collidesX, out bool collidesY)
	{
		collidesX = false;
		collidesY = false;
		if (OverlapWalls(position + collider.position + delta * Vector2.UnitX, collider.size, level))
		{
			collidesX = true;
			if (delta.x < -0.001f)
			{
				float forwardEdge = position.x + collider.position.x;
				delta.x = MathF.Floor(forwardEdge) - forwardEdge + 0.001f;
			}
			else if (delta.x > 0.001f)
			{
				float forwardEdge = position.x + collider.position.x + collider.size.x;
				delta.x = MathF.Ceiling(forwardEdge) - forwardEdge - 0.001f;
			}
		}
		if (OverlapWalls(position + collider.position + delta * Vector2.UnitY, collider.size, level))
		{
			collidesY = true;
			if (delta.y < -0.001f)
			{
				float forwardEdge = position.y + collider.position.y;
				delta.y = MathF.Floor(forwardEdge) - forwardEdge + 0.001f;
			}
			else if (delta.y > 0.001f)
			{
				float forwardEdge = position.y + collider.position.y + collider.size.y;
				delta.y = MathF.Ceiling(forwardEdge) - forwardEdge - 0.001f;
			}
		}
	}
}
