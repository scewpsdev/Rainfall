using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class HitData
{
	public float distance;
	public Vector2 position;
	public Vector2 normal;
	public Vector2i tile;
	public Entity entity;
}

public class Level
{
	public const int COLLISION_X = 1 << 0;
	public const int COLLISION_Y = 1 << 1;


	public int floor;

	public int width, height;
	int[] tiles;

	public Door entrance;
	public Door exit;

	public List<Entity> entities = new List<Entity>();


	public Level(int floor)
	{
		this.floor = floor;

		width = 20;
		height = 20;
		tiles = new int[width * height];
		Array.Fill(tiles, 0);

		for (int y = 0; y < height; y++)
		{
			setTile(0, y, 2);
			setTile(width - 1, y, 2);
		}
		for (int x = 0; x < width; x++)
		{
			setTile(x, 0, 2);
			setTile(x, height - 1, 2);
		}
	}

	public void resize(int width, int height)
	{
		this.width = width;
		this.height = height;
		tiles = new int[width * height];
		Array.Fill(tiles, 2);
	}

	public bool setTile(int x, int y, int tile)
	{
		if (x >= 0 && x < width && y >= 0 && y < height)
		{
			tiles[x + y * width] = tile;
			return true;
		}
		return false;
	}

	public int getTile(int x, int y)
	{
		if (x >= 0 && x < width && y >= 0 && y < height)
			return tiles[x + y * width];
		return 0;
	}

	public int getTile(Vector2i v)
	{
		return getTile(v.x, v.y);
	}

	public int getTile(Vector2 v)
	{
		return getTile((Vector2i)Vector2.Floor(v));
	}

	public void reset()
	{
		foreach (Entity entity in entities)
		{
			foreach (var removeCallback in entity.removeCallbacks)
				removeCallback.Invoke();
			entity.destroy();
		}
		entities.Clear();
	}

	public void destroy()
	{
		foreach (Entity entity in entities)
		{
			foreach (var removeCallback in entity.removeCallbacks)
				removeCallback.Invoke();
			entity.destroy();
		}
		entities.Clear();
	}

	public void addEntity(Entity entity, bool init = true)
	{
		entities.Add(entity);

		if (init)
			entity.init();
	}

	public void addEntity(Entity entity, Vector2 position, bool init = true)
	{
		entity.position = position;
		addEntity(entity, init);
	}

	public void addEntity(Entity entity, Vector2 position, float rotation, bool init = true)
	{
		entity.position = position;
		entity.rotation = rotation;
		addEntity(entity, init);
	}

	public void addEntity(Entity entity, Matrix transform, bool init = true)
	{
		transform.decompose(out Vector3 position, out Quaternion rotation, out Vector3 _);
		entity.position = position.xy;
		entity.rotation = rotation.angle;
		addEntity(entity, init);
	}

	public void removeEntity(Entity entity)
	{
		entities.Remove(entity);
	}

	public void update()
	{
		for (int i = 0; i < entities.Count; i++)
		{
			entities[i].update();

			if (entities[i].position.y < 0)
			{
				if (entities[i] is Hittable)
				{
					Hittable hittable = entities[i] as Hittable;
					hittable.hit(1000, null);
				}
				else if (entities[i] is Destructible)
				{
					Destructible destructible = entities[i] as Destructible;
					destructible.onDestroyed(null, null);
				}
				entities[i].remove();
			}

			if (entities[i].removed)
			{
				foreach (var removeCallback in entities[i].removeCallbacks)
					removeCallback.Invoke();

				entities[i].destroy();
				entities.RemoveAt(i);
				i--;
			}
		}
	}

	public void render()
	{
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				TileType tile = TileType.Get(getTile(x, y));
				if (tile != null && tile.visible)
				{
					if (tile.sprite != null)
					{
						Renderer.DrawSprite(x, y, 1.001f, 1.001f, tile.sprite, false, 0xFFFFFFFF);

						TileType left = TileType.Get(getTile(x - 1, y));
						TileType right = TileType.Get(getTile(x + 1, y));
						TileType top = TileType.Get(getTile(x, y + 1));
						TileType bottom = TileType.Get(getTile(x, y - 1));

						if (left != tile && tile.left != null)
							Renderer.DrawSprite(x - 1, y, 1, 1, tile.left, false, 0xFFFFFFFF);
						if (right != tile && tile.right != null)
							Renderer.DrawSprite(x + 1, y, 1, 1, tile.right, false, 0xFFFFFFFF);
						if (top != tile && tile.top != null)
							Renderer.DrawSprite(x, y + 1, 1, 1, tile.top, false, 0xFFFFFFFF);
						if (bottom != tile && tile.bottom != null)
							Renderer.DrawSprite(x, y - 1, 1, 1, tile.bottom, false, 0xFFFFFFFF);
					}
					else
					{
						Renderer.DrawSprite(x, y, 0, 1.001f, 1.001f, null, 0, 0, 0, 0, tile.color);
					}
				}
			}
		}
		for (int i = 0; i < entities.Count; i++)
		{
			entities[i].render();
		}
	}

	bool overlapTiles(Vector2 min, Vector2 max, bool falling, bool downInput)
	{
		int x0 = (int)MathF.Floor(min.x + 0.01f);
		int x1 = (int)MathF.Floor(max.x - 0.01f);
		int y0 = (int)MathF.Floor(min.y + 0.01f);
		int y1 = (int)MathF.Floor(max.y - 0.01f);
		for (int y = y0; y <= y1; y++)
		{
			for (int x = x0; x <= x1; x++)
			{
				if (x < 0 || x >= width /*|| y < 0*/ || y >= height)
					return true;
				TileType tile = TileType.Get(getTile(x, y));
				if (tile != null)
				{
					if (!tile.isPlatform || tile.isPlatform && falling && !downInput && min.y - y - 1 > -0.1f)
						return true;
				}
			}
		}
		return false;
	}

	public bool overlapTiles(Vector2 min, Vector2 max)
	{
		return overlapTiles(min, max, false, false);
	}

	public int doCollision(ref Vector2 position, FloatRect collider, ref Vector2 displacement, bool downInput = false)
	{
		int flags = 0;
		if (overlapTiles(position + collider.min + new Vector2(displacement.x, 0), position + collider.max + new Vector2(displacement.x, 0), displacement.y < 0, downInput))
		{
			displacement.x = 0;
			flags |= COLLISION_X;
		}
		if (overlapTiles(position + collider.min + new Vector2(0, displacement.y), position + collider.max + new Vector2(0, displacement.y), displacement.y < 0, downInput))
		{
			displacement.y = 0;
			flags |= COLLISION_Y;
		}
		return flags;
	}

	public Interactable getInteractable(Vector2 position)
	{
		Interactable result = null;
		float resultD2 = float.MaxValue;
		for (int i = 0; i < entities.Count; i++)
		{
			if (entities[i] is Interactable)
			{
				Interactable interactable = entities[i] as Interactable;
				Vector2 p = entities[i].position;
				Vector2 delta = p - position;
				float d2 = Vector2.Dot(delta, delta);
				if (d2 < interactable.getRange() * interactable.getRange())
				{
					if (d2 < resultD2)
					{
						result = interactable;
						resultD2 = d2;
					}
				}
			}
		}
		return result;
	}

	public Climbable getClimbable(Vector2 position)
	{
		for (int i = 0; i < entities.Count; i++)
		{
			if (entities[i] is Climbable)
			{
				Climbable climbable = entities[i] as Climbable;
				Vector2 min = entities[i].position + climbable.getArea().min;
				Vector2 max = entities[i].position + climbable.getArea().max;
				if (position.x >= min.x && position.x <= max.x && position.y >= min.y && position.y <= max.y)
					return climbable;
			}
		}
		return null;
	}

	public HitData raycastTiles(Vector2 origin, Vector2 direction, float range)
	{
		if (direction.x == 0)
			direction.x = 0.00001f;
		if (direction.y == 0)
			direction.y = 0.00001f;

		Vector2i pos = (Vector2i)Vector2.Floor(origin);
		Vector2 ri = 1.0f / direction;
		Vector2i rs = Vector2.Sign(direction);
		Vector2 dis = (pos - origin + 0.5f + rs * 0.5f) * ri;

		Vector2i mm = Vector2i.Zero;
		for (int i = 0; i < 128; i++)
		{
			TileType value = TileType.Get(getTile(pos.x, pos.y));
			if (value != null && value.isSolid && !value.isPlatform)
				break;
			mm = new Vector2i(MathHelper.Step(dis.x, dis.y), MathHelper.Step(dis.y, dis.x));
			dis += mm * rs * ri;
			pos += mm * rs;
		}

		Vector2 normal = (Vector2)(-mm * rs);
		Vector2i tile = pos;

		// intersect the cube	
		Vector2 mini = (pos - origin + 0.5f - 0.5f * rs) * ri;
		float distance = MathF.Max(mini.x, mini.y);

		if (distance > range)
			return null;

		return new HitData() { distance = distance, position = origin + distance * direction, normal = normal, tile = tile };
	}

	bool hitBoundingBox(Vector2 origin, Vector2 direction, Vector2 minB, Vector2 maxB, out Vector2 position, out float distance, out Vector2 normal)
	{
		position = origin;
		distance = 0;
		normal = Vector2.Zero;

		const int RIGHT = 0;
		const int LEFT = 1;
		const int MIDDLE = 2;

		Span<int> quadrant = stackalloc int[2];
		Span<float> maxT = stackalloc float[2];
		Span<float> candidatePlane = stackalloc float[2];
		bool inside = true;

		for (int i = 0; i < 2; i++)
		{
			if (origin[i] < minB[i])
			{
				quadrant[i] = LEFT;
				candidatePlane[i] = minB[i];
				inside = false;
			}
			else if (origin[i] > maxB[i])
			{
				quadrant[i] = RIGHT;
				candidatePlane[i] = maxB[i];
				inside = false;
			}
			else
			{
				quadrant[i] = MIDDLE;
			}
		}

		if (inside)
		{
			normal = -direction;
			return true;
		}

		for (int i = 0; i < 2; i++)
		{
			if (quadrant[i] != MIDDLE && direction[i] != 0)
				maxT[i] = (candidatePlane[i] - origin[i]) / direction[i];
			else
				maxT[i] = -1;
		}

		int whichPlane = maxT[0] < maxT[1] ? 1 : 0;

		if (maxT[whichPlane] < 0)
			return false;

		for (int i = 0; i < 2; i++)
		{
			if (whichPlane != i)
			{
				position[i] = origin[i] + maxT[whichPlane] * direction[i];
				if (position[i] < minB[i] || position[i] > maxB[i])
					return false;
			}
			else
			{
				position[i] = candidatePlane[i];
			}
		}

		if (direction.x != 0)
			distance = (position.x - origin.x) / direction.x;
		else
			distance = (position.y - origin.y) / direction.y;

		normal = new Vector2(0);
		normal[whichPlane] = Math.Sign(direction[whichPlane]);

		return true;
	}

	public HitData raycast(Vector2 origin, Vector2 direction, float range, uint filterMask = 1)
	{
		HitData hit = raycastTiles(origin, direction, range);

		Entity hitEntity = null;
		Vector2 hitPosition = hit != null ? hit.position : Vector2.Zero;
		float hitDistance = hit != null ? hit.distance : range;
		Vector2 hitNormal = hit != null ? hit.normal : Vector2.Zero;
		for (int i = 0; i < entities.Count; i++)
		{
			if (entities[i].collider != null && (entities[i].filterGroup & filterMask) != 0)
			{
				Vector2 min = entities[i].position + entities[i].collider.min;
				Vector2 max = entities[i].position + entities[i].collider.max;
				if (hitBoundingBox(origin, direction, min, max, out Vector2 position, out float distance, out Vector2 normal))
				{
					if (distance < hitDistance)
					{
						hitEntity = entities[i];
						hitPosition = position;
						hitDistance = distance;
						hitNormal = normal;
					}
				}
			}
		}

		return new HitData() { position = hitPosition, distance = hitDistance, entity = hitEntity, normal = hitNormal };
	}

	public int raycastNoBlock(Vector2 origin, Vector2 direction, float range, Span<HitData> hits, uint filterMask = 1)
	{
		int numHits = 0;
		for (int i = 0; i < entities.Count; i++)
		{
			if (entities[i].collider != null && (entities[i].filterGroup & filterMask) != 0)
			{
				Vector2 min = entities[i].position + entities[i].collider.min;
				Vector2 max = entities[i].position + entities[i].collider.max;
				if (hitBoundingBox(origin, direction, min, max, out Vector2 position, out float distance, out Vector2 normal))
				{
					if (numHits < hits.Length && distance < range)
						hits[numHits++] = new HitData() { entity = entities[i], position = position, distance = distance, normal = normal };
				}
			}
		}
		return numHits;
	}

	public int overlap(Vector2 bmin, Vector2 bmax, Span<HitData> hits, uint filterMask = 1)
	{
		int numHits = 0;
		for (int i = 0; i < entities.Count; i++)
		{
			if (entities[i].collider != null && (entities[i].filterGroup & filterMask) != 0)
			{
				Vector2 min = entities[i].position + entities[i].collider.min;
				Vector2 max = entities[i].position + entities[i].collider.max;
				if (bmax.x >= min.x && bmin.x <= max.x && bmax.y >= min.y && bmin.y <= max.y)
				{
					if (numHits < hits.Length)
						hits[numHits++] = new HitData() { entity = entities[i] };
				}
			}
		}
		return numHits;
	}

	public HitData sampleTiles(Vector2 position)
	{
		Vector2i tilePosition = (Vector2i)Vector2.Floor(position);
		TileType tile = TileType.Get(getTile(tilePosition));
		if (tile != null && tile.isSolid && !tile.isPlatform)
			return new HitData() { position = position };
		return null;
	}

	public HitData sample(Vector2 position, uint filterMask = 1)
	{
		HitData hit = sampleTiles(position);
		if (hit != null)
			return hit;
		for (int i = 0; i < entities.Count; i++)
		{
			if (entities[i].collider != null && (entities[i].filterGroup & filterMask) != 0)
			{
				Vector2 min = entities[i].position + entities[i].collider.min;
				Vector2 max = entities[i].position + entities[i].collider.max;
				if (position.x >= min.x && position.x <= max.x && position.y >= min.y && position.y <= max.y)
					return new HitData() { position = position, entity = entities[i] };
			}
		}
		return null;
	}
}
