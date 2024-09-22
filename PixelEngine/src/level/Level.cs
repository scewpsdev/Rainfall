using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net.Http.Headers;
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
	public string name;

	public int width, height;
	int[] tiles;
	bool[] walkable;
	public AStar astar;
	byte[] lightmapData;

	Texture lightmap;

	public Door entrance;
	public Door exit;

	public List<Entity> entities = new List<Entity>();

	public Vector3 ambientLight = new Vector3(1.0f);
	public Sound ambientSound = null;


	public Level(int floor, string name)
	{
		this.floor = floor;
		this.name = name;

		resize(20, 20);
	}

	public void resize(int width, int height, TileType fillTile = null)
	{
		this.width = width;
		this.height = height;
		tiles = new int[width * height];
		if (fillTile != null)
			Array.Fill(tiles, fillTile.id);
		walkable = new bool[width * height];
		Array.Fill(walkable, false);

		astar = new AStar(width, height, walkable);

		lightmapData = new byte[(width + 1) * (height + 1)];
		if (lightmap != null)
			Renderer.graphics.destroyTexture(lightmap);
		lightmap = Renderer.graphics.createTexture(width + 1, height + 1, TextureFormat.R8, (ulong)SamplerFlags.Clamp);
	}

	public unsafe void updateLightmap(int x0, int y0, int w, int h)
	{
		int x1 = x0 + w - 1;
		int y1 = y0 + h - 1;

		x0 = Math.Max(x0, 0);
		y0 = Math.Max(y0, 0);
		x1 = Math.Min(x1, width - 1);
		y1 = Math.Min(y1, height - 1);

		for (int y = y0; y <= y1; y++)
		{
			for (int x = x0; x <= x1; x++)
			{
				TileType t0 = getTile(x - 1, y - 1);
				TileType t1 = getTile(x, y - 1);
				TileType t2 = getTile(x - 1, y);
				TileType t3 = getTile(x, y);
				byte value = 0;
				if ((t0 == null || !t0.isSolid) && x > 0 && y > 0) value += 64;
				if ((t1 == null || !t1.isSolid) && y > 0) value += 64;
				if ((t2 == null || !t2.isSolid) && x > 0) value += 64;
				if ((t3 == null || !t3.isSolid)) value += 63;
				lightmapData[(x - x0) + (y - y0) * w] = value;
			}
		}

		fixed (void* data = lightmapData)
			Renderer.graphics.setTextureData(lightmap, x0, y0, w, h, Renderer.graphics.createVideoMemory(data, w * h));
	}

	public bool setTile(int x, int y, TileType tile)
	{
		if (x >= 0 && x < width && y >= 0 && y < height)
		{
			tiles[x + y * width] = tile != null ? tile.id : 0;
			walkable[x + y * width] = tile != null ? !tile.isSolid : true;
			return true;
		}
		return false;
	}

	public TileType getTile(int x, int y)
	{
		if (x >= 0 && x < width && y >= 0 && y < height)
			return TileType.Get(tiles[x + y * width]);
		return null;
	}

	public TileType getTile(Vector2i v)
	{
		return getTile(v.x, v.y);
	}

	public TileType getTile(Vector2 v)
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
			entity.init(this);
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

			if (entities[i].position.y < -1)
			{
				if (entities[i] is Hittable)
				{
					Hittable hittable = entities[i] as Hittable;
					hittable.hit(1000, null, null);
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
		Renderer.ambientLight = ambientLight;
		Renderer.bloomStrength = 0.01f;
		Renderer.vignetteFalloff = 0.1f;

		Renderer.lightMask = lightmap;
		Renderer.lightMaskRect = new FloatRect(-0.5f, -0.5f, width + 1, height + 1);

		int x0 = (int)MathF.Floor(GameState.instance.camera.left);
		int y0 = (int)MathF.Floor(GameState.instance.camera.bottom);
		int x1 = (int)MathF.Floor(GameState.instance.camera.right);
		int y1 = (int)MathF.Floor(GameState.instance.camera.top);

		for (int y = y0; y <= y1; y++)
		{
			for (int x = x0; x <= x1; x++)
			{
				TileType tile = getTile(x, y);
				if (tile != null && tile.visible)
				{
					if (tile.sprites != null)
					{
						uint h = Hash.combine(Hash.hash(x), Hash.hash(y));
						Renderer.DrawSprite(x, y, Entity.LAYER_TILE, 1.001f, 1.001f, 0, tile.sprites[h % tile.sprites.Length], false, 0xFFFFFFFF);

						TileType left = getTile(x - 1, y);
						TileType right = getTile(x + 1, y);
						TileType top = getTile(x, y + 1);
						TileType bottom = getTile(x, y - 1);

						if (left != tile && tile.left != null)
							Renderer.DrawSprite(x - 1, y, Entity.LAYER_TILE, 1, 1, 0, tile.left[h % tile.left.Length], false, 0xFFFFFFFF);
						if (right != tile && tile.right != null)
							Renderer.DrawSprite(x + 1, y, Entity.LAYER_TILE, 1, 1, 0, tile.right[h % tile.right.Length], false, 0xFFFFFFFF);
						if (top != tile && tile.top != null)
							Renderer.DrawSprite(x, y + 1, Entity.LAYER_TILE, 1, 1, 0, tile.top[h % tile.top.Length], false, 0xFFFFFFFF);
						if (bottom != tile && tile.bottom != null)
							Renderer.DrawSprite(x, y - 1, Entity.LAYER_TILE, 1, 1, 0, tile.bottom[h % tile.bottom.Length], false, 0xFFFFFFFF);
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
				TileType tile = getTile(x, y);
				if (tile != null)
				{
					if (!tile.isPlatform || tile.isPlatform && falling && !downInput && min.y - y - 1 > -0.25f)
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
		if (flags == 0 && overlapTiles(position + collider.min + displacement, position + collider.max + displacement, displacement.y < 0, downInput))
		{
			float displacementFactor = 0.5f;
			for (int i = 1; i < 10; i++)
			{
				if (overlapTiles(position + collider.min + displacement * displacementFactor, position + collider.max + displacement * displacementFactor, displacement.y < 0, downInput))
					displacementFactor -= MathF.Pow(0.5f, i + 1);
				else
				{
					if (overlapTiles(position + collider.min + new Vector2(displacement.x, displacement.y * displacementFactor), position + collider.max + new Vector2(displacement.x, displacement.y * displacementFactor), displacement.y < 0, downInput))
					{
						displacement.x *= displacementFactor;
						flags |= COLLISION_X;
						return flags;
					}
					if (overlapTiles(position + collider.min + new Vector2(0, displacement.y), position + collider.max + new Vector2(0, displacement.y), displacement.y < 0, downInput))
					{
						displacement.y *= displacementFactor;
						flags |= COLLISION_Y;
						return flags;
					}

					displacementFactor += MathF.Pow(0.5f, i + 1);
				}
			}

			displacement *= displacementFactor;
			flags |= COLLISION_X | COLLISION_Y;
			return flags;
		}
		return flags;
	}

	public Interactable getInteractable(Vector2 position, Player player)
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
				if (interactable.canInteract(player) && d2 < interactable.getRange() * interactable.getRange())
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
		bool hit = false;
		for (int i = 0; i < 128; i++)
		{
			TileType value = getTile(pos.x, pos.y);
			if (value != null && value.isSolid && !value.isPlatform)
			{
				hit = true;
				break;
			}
			mm = new Vector2i(MathHelper.Step(dis.x, dis.y), MathHelper.Step(dis.y, dis.x));
			dis += mm * rs * ri;
			pos += mm * rs;
		}

		if (!hit)
			return null;

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
			if (entities[i].collider != null && (entities[i].filterGroup & filterMask) != 0 && !entities[i].removed)
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

		if (hit != null || hitDistance != range)
			return new HitData() { position = hitPosition, distance = hitDistance, entity = hitEntity, normal = hitNormal };
		return null;
	}

	public int raycastNoBlock(Vector2 origin, Vector2 direction, float range, Span<HitData> hits, uint filterMask = 1)
	{
		int numHits = 0;
		for (int i = 0; i < entities.Count; i++)
		{
			if (entities[i].collider != null && (entities[i].filterGroup & filterMask) != 0 && !entities[i].removed)
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
			if (entities[i].collider != null && (entities[i].filterGroup & filterMask) != 0 && !entities[i].removed)
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
		TileType tile = getTile(tilePosition);
		if (tile != null && (tile.isSolid || tile.isPlatform))
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
			if (entities[i].collider != null && (entities[i].filterGroup & filterMask) != 0 && !entities[i].removed)
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
