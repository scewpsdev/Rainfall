using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
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
			if (entity.staticCollider && entity.colliderEnabled)
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

	static bool BoxIntersection(Vector2 origin, Vector2 dir, Vector2 offset, Vector2 size, out float tmin, out float tmax)
	{
		Vector2 dirinv = 1.0f / dir;

		Vector2 t1 = (offset - origin) * dirinv;
		Vector2 t2 = (offset + size - origin) * dirinv;

		Vector2 mn = Vector2.Min(t1, t2);
		Vector2 mx = Vector2.Max(t1, t2);

		bool sx = mn.x > mn.y;
		bool sy = !sx;
		Vector2i mask = new Vector2i(sx ? 1 : 0, sy ? 1 : 0);
		Vector2i step = (Vector2i)Vector2.Sign(dir);

		tmin = Math.Max(mn.x, mn.y);
		tmax = Math.Min(mx.x, mx.y);

		return tmin < tmax && tmax > 0;
	}

	public static bool Linecast(Vector2 start, Vector2 end, Level level)
	{
		float len = (end - start).length;
		Vector2 dir = (end - start) / len;
		foreach (Entity entity in level.entities)
		{
			if (entity.staticCollider && entity.colliderEnabled)
			{
				if (BoxIntersection(start, dir, entity.collider.position, entity.collider.size, out float tmin, out float tmax))
				{
					if (tmin < len)
						return true;
				}
			}
		}

		var intBound = (Vector2 s, Vector2 ds) =>
		{
			s *= Vector2.Sign(ds);
			ds = Vector2.Abs(ds);
			s = Vector2.Fract(s);
			return (1.0f - s) / ds;
		};

		Vector2i ip = (Vector2i)Vector2.Floor(start);
		Vector2i step = Vector2.Sign(dir);
		Vector2 tMax = intBound(start, dir);
		Vector2 tDelta = step / dir;

		int maxSteps = 256 * 3;
		for (int i = 0; i < maxSteps; i++)
		{
			if (level.getTile(ip.x, ip.y) != 0)
			{
				return true;
			}
			else
			{
				bool sx = tMax.x < tMax.y;
				bool sy = !sx;
				Vector2i mask = new Vector2i(sx ? 1 : 0, sy ? 1 : 0);
				float t = (tMax * mask).x + (tMax * mask).y;
				tMax += tDelta * mask;
				ip += mask;

				if (t > len || ip.x < 0 || ip.x >= level.width || ip.y < 0 || ip.y >= level.height)
				{
					break;
				}
			}
		}

		return false;
	}

	public static List<Entity> OverlapEntities(Vector2 position, Vector2 size, Level level)
	{
		List<Entity> result = new List<Entity>();

		foreach (Entity entity in level.entities)
		{
			if (entity.hitbox != null && entity.colliderEnabled)
			{
				if (entity.position.x + entity.hitbox.max.x > position.x &&
					entity.position.y + entity.hitbox.max.y > position.y &&
					entity.position.x + entity.hitbox.min.x < position.x + size.x &&
					entity.position.y + entity.hitbox.min.y < position.y + size.y)
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
