using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


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


	public string name;
	public int floor;
	public float minLootValue, maxLootValue;
	public float avgLootValue => (minLootValue + maxLootValue) * 0.5f;

	public bool infiniteEnergy = false;

	public int width, height;
	int[] tiles;
	bool[] walkable;
	public AStar astar;
	byte[] lightmapData;

	int[] backgroundTiles;

	public List<Room> rooms;

	Texture lightmap;

	public Door entrance;
	public Door exit;

	public List<Entity> entities = new List<Entity>();

	List<Entity> colliders = new List<Entity>();

	public Texture bg = null;
	public Vector3 ambientLight = new Vector3(1.0f);
	public Vector3 fogColor = new Vector3(0.0f);
	public float fogFalloff = 0.0f;
	public Sound ambientSound = null;

	public float lightLevel
	{
		get
		{
			Vector3 srgb = Vector3.Min(ambientLight, Vector3.One);
			return MathF.Max(MathF.Max(srgb.x, srgb.y), srgb.z);
		}
	}


	public Level(int floor, string name, float minLootValue = 0, float maxLootValue = 0)
	{
		this.floor = floor;
		this.name = name;
		this.minLootValue = minLootValue;
		this.maxLootValue = maxLootValue;

		resize(20, 20);
	}

	public Level(int floor, string name, int width, int height, TileType defaultTile, float minLootValue = 0, float maxLootValue = 0)
		: this(floor, name, minLootValue, maxLootValue)
	{
		resize(width, height, defaultTile);
	}

	public void resize(int width, int height, TileType fillTile = null)
	{
		this.width = width;
		this.height = height;
		tiles = new int[width * height];
		if (fillTile != null)
			Array.Fill(tiles, fillTile.id);
		backgroundTiles = new int[width * height];
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
				if ((t0 == null || !t0.isSolid || !t0.visible) && x > 0 && y > 0) value += 64;
				if ((t1 == null || !t1.isSolid || !t1.visible) && y > 0) value += 64;
				if ((t2 == null || !t2.isSolid || !t2.visible) && x > 0) value += 64;
				if ((t3 == null || !t3.isSolid || !t3.visible)) value += 63;
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
		if (x >= 0 && x < width && y < height)
		{
			if (y < 0)
				return null;
			y = Math.Max(y, 0);
			return TileType.Get(tiles[x + y * width]);
		}
		return null;
	}

	public bool setBGTile(int x, int y, TileType tile)
	{
		if (x >= 0 && x < width && y >= 0 && y < height)
		{
			backgroundTiles[x + y * width] = tile != null ? tile.id : 0;
			return true;
		}
		return false;
	}

	public TileType getBGTile(int x, int y)
	{
		if (x >= 0 && x < width && y < height)
		{
			if (y < 0)
				return null;
			y = Math.Max(y, 0);
			return TileType.Get(backgroundTiles[x + y * width]);
		}
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
		while (entities.Count > 0)
		{
			foreach (var removeCallback in entities[0].removeCallbacks)
				removeCallback.Invoke();
			entities[0].destroy();
			entities[0].level = null;
			entities.RemoveAt(0);
		}

		if (lightmap != null)
			Renderer.graphics.destroyTexture(lightmap);
	}

	public void addEntity(Entity entity, bool init = true)
	{
		Debug.Assert(!init || entity.level == null);
		Debug.Assert(!entities.Contains(entity));

		entities.Add(entity);
		entity.level = this;

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
		entity.level = null;
	}

	public void update()
	{
		for (int i = 0; i < entities.Count; i++)
		{
			entities[i].update();

			if (entities[i].position.y < -6)
			{
				if (entities[i] is Hittable)
				{
					Hittable hittable = entities[i] as Hittable;
					hittable.hit(1000, null, null, "The Void");
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

		if (infiniteEnergy)
			GameState.instance.player.mana = GameState.instance.player.maxMana;
	}

	public void render()
	{
		Renderer.ambientLight = ambientLight;
		Renderer.bloomStrength = 0.01f;
		Renderer.vignetteFalloff = 0.1f;

		Renderer.lightMask = lightmap;
		Renderer.lightMaskRect = new FloatRect(-0.5f, -0.5f, width + 1, height + 1);

		if (bg != null)
			Renderer.DrawSprite(GameState.instance.camera.left, GameState.instance.camera.bottom, 0.9999f, GameState.instance.camera.width, GameState.instance.camera.height, bg, 0, 0, bg.width, bg.height, new Vector4(1));
		else
			Renderer.DrawSprite(GameState.instance.camera.left, GameState.instance.camera.bottom, 0.9999f, GameState.instance.camera.width, GameState.instance.camera.height, 0, null, false, new Vector4(fogColor, 1));


		for (int i = 0; i < entities.Count; i++)
		{
			entities[i].render();
		}


		int x0 = (int)MathF.Floor(GameState.instance.camera.left) - 1;
		int y0 = (int)MathF.Floor(GameState.instance.camera.bottom) - 1;
		int x1 = (int)MathF.Ceiling(GameState.instance.camera.right);
		int y1 = (int)MathF.Ceiling(GameState.instance.camera.top);

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
						Renderer.DrawSprite(x, y, Entity.LAYER_TILE, 1.001f, 1.001f, null, 0, 0, 0, 0, tile.color);
					}
				}
			}
		}

		for (int y = y0; y <= y1; y++)
		{
			for (int x = x0; x <= x1; x++)
			{
				TileType tile = getBGTile(x, y);
				if (tile != null && tile.visible)
				{
					if (tile.sprites != null)
					{
						uint h = Hash.combine(Hash.hash(x), Hash.hash(y));
						Renderer.DrawSprite(x, y, Entity.LAYER_BGBG, 1.001f, 1.001f, 0, tile.sprites[h % tile.sprites.Length], false, 0xFF3F3F3F);

						TileType left = getBGTile(x - 1, y);
						TileType right = getBGTile(x + 1, y);
						TileType top = getBGTile(x, y + 1);
						TileType bottom = getBGTile(x, y - 1);

						if (left != tile && tile.left != null)
							Renderer.DrawSprite(x - 1, y, Entity.LAYER_BGBG, 1, 1, 0, tile.left[h % tile.left.Length], false, 0xFF3F3F3F);
						if (right != tile && tile.right != null)
							Renderer.DrawSprite(x + 1, y, Entity.LAYER_BGBG, 1, 1, 0, tile.right[h % tile.right.Length], false, 0xFF3F3F3F);
						if (top != tile && tile.top != null)
							Renderer.DrawSprite(x, y + 1, Entity.LAYER_BGBG, 1, 1, 0, tile.top[h % tile.top.Length], false, 0xFF3F3F3F);
						if (bottom != tile && tile.bottom != null)
							Renderer.DrawSprite(x, y - 1, Entity.LAYER_BGBG, 1, 1, 0, tile.bottom[h % tile.bottom.Length], false, 0xFF3F3F3F);
					}
				}
			}
		}
	}

	public void addEntityCollider(Entity collider)
	{
		colliders.Add(collider);
	}

	public void removeCollider(Entity collider)
	{
		colliders.Remove(collider);
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
				//if (x < 0 || x >= width /*|| y < 0*/ || y >= height)
				//	return true;
				TileType tile = getTile(x, y);
				if (tile != null)
				{
					if (tile.isSolid)
						return true;
					else if (tile.isPlatform && falling && !downInput && min.y - (y + tile.platformHeight) > -0.25f)
					{
						Vector2 t0 = new Vector2(x, y);
						Vector2 t1 = new Vector2(x + 1, y + tile.platformHeight);
						if (t1.x > min.x && t1.y > min.y && t0.x < max.x && t1.y < max.y)
							return true;
					}
				}
			}
		}

		for (int i = 0; i < colliders.Count; i++)
		{
			Vector2 cmin = colliders[i].position + colliders[i].collider.min;
			Vector2 cmax = colliders[i].position + colliders[i].collider.max;
			if (max.x > cmin.x && max.y > cmin.y && min.x < cmax.x && min.y < cmax.y)
			{
				if (!colliders[i].platformCollider || min.y - cmax.y > -0.25f && falling && !downInput)
				{
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

	public int doCollision(ref Vector2 position, FloatRect collider, ref Vector2 displacement, bool downInput = false, bool cornerCutting = false)
	{
		int flags = 0;
		if (overlapTiles(position + collider.min + new Vector2(displacement.x, 0), position + collider.max + new Vector2(displacement.x, 0), false, downInput))
		{
			displacement.x = 0;
			flags |= COLLISION_X;
		}
		if (overlapTiles(position + collider.min + new Vector2(0, displacement.y), position + collider.max + new Vector2(0, displacement.y), displacement.y < 0, downInput))
		{
			// Do corner cutting
			if (cornerCutting && displacement.y > 0.01f && !sampleTiles(position + displacement + new Vector2(0, collider.max.y)))
			{
				if (!sampleTiles(position + displacement + collider.max) && sampleTiles(position + displacement + new Vector2(collider.min.x, collider.max.y)))
				{
					displacement.x = MathF.Max(displacement.x, 1 - MathHelper.Fract(position.x + displacement.x + collider.min.x));
					return 0;
				}
				else if (sampleTiles(position + displacement + collider.max) && !sampleTiles(position + displacement + new Vector2(collider.min.x, collider.max.y)))
				{
					displacement.x = MathF.Min(displacement.x, -MathHelper.Fract(position.x + displacement.x + collider.max.x));
					return 0;
				}
			}

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
				if (entities[i].collider != null)
					p += entities[i].collider.center;
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
		//Vector2 ri = 1.0f / direction;
		Vector2 ri = 1.0f / new Vector2(direction.x != 0 ? direction.x : 0, direction.y != 0 ? direction.y : 0);
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

		Vector2 normal = (Vector2)(-mm * rs);
		Vector2i tile = pos;

		// intersect the cube	
		Vector2 mini = (pos - origin + 0.5f - 0.5f * rs) * ri;
		float distance = MathF.Max(mini.x, mini.y);

		Entity entity = null;
		for (int i = 0; i < colliders.Count; i++)
		{
			if (!colliders[i].platformCollider)
			{
				Vector2 min = colliders[i].position + colliders[i].collider.min;
				Vector2 max = colliders[i].position + colliders[i].collider.max;

				float x0 = direction.x > 0 ? min.x : max.x;
				float y0 = direction.y > 0 ? min.y : max.y;
				float x1 = direction.x > 0 ? max.x : min.x;
				float y1 = direction.y > 0 ? max.y : min.y;

				float tx0 = (x0 - origin.x) / direction.x;
				float tx1 = (x1 - origin.x) / direction.x;
				float ty0 = (y0 - origin.y) / direction.y;
				float ty1 = (y1 - origin.y) / direction.y;

				if (tx1 >= ty0 && tx0 <= ty1 && tx1 > 0 && ty1 > 0)
				{
					tx0 = MathF.Max(tx0, 0);
					ty0 = MathF.Max(ty0, 0);

					float t = MathF.Max(tx0, ty0);
					if (t < distance)
					{
						distance = t;
						normal = tx0 > ty0 ? new Vector2(-MathF.Sign(direction.x), 0) : new Vector2(0, -MathF.Sign(direction.y));
						entity = colliders[i];
						hit = true;
					}
				}
			}
		}

		if (!hit)
			return null;

		if (distance > range)
			return null;

		return new HitData() { distance = distance, position = origin + distance * direction, normal = normal, tile = tile, entity = entity };
	}

	public HitData raycastTilesDestructible(Vector2 origin, Vector2 direction, float range)
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
			if (value != null && value.health != 1 && value.visible)
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

		int whichPlane = maxT[0] != -1 && maxT[0] > maxT[1] ? 0 : 1;

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

		distance = maxT[whichPlane];

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

	// https://www.gamedev.net/tutorials/programming/general-and-gameplay-programming/swept-aabb-collision-detection-and-response-r3084/
	bool sweepAABBs(Vector2 min0, Vector2 max0, Vector2 velocity, Vector2 min1, Vector2 max1, out float distance, out Vector2 normal)
	{
		Vector2 invEntry;
		Vector2 invExit;

		if (velocity.x > 0)
		{
			invEntry.x = min1.x - max0.x;
			invExit.x = max1.x - min0.x;
		}
		else
		{
			invEntry.x = max1.x - min0.x;
			invExit.x = min1.x - max0.x;
		}
		if (velocity.y > 0)
		{
			invEntry.y = min1.y - max0.y;
			invExit.y = max1.y - min0.y;
		}
		else
		{
			invEntry.y = max1.y - min0.y;
			invExit.y = min1.y - max0.y;
		}

		Vector2 entry;
		Vector2 exit;

		if (velocity.x == 0)
		{
			//entry.x = Math.Sign(invEntry.x) * float.PositiveInfinity;
			//exit.x = Math.Sign(invExit.x) * float.PositiveInfinity;
			entry.x = float.NegativeInfinity;
			exit.x = float.PositiveInfinity;
		}
		else
		{
			entry.x = invEntry.x / velocity.x;
			exit.x = invExit.x / velocity.x;
		}
		if (velocity.y == 0)
		{
			//entry.y = Math.Sign(invEntry.y) * float.PositiveInfinity;
			//exit.y = Math.Sign(invExit.y) * float.PositiveInfinity;
			entry.y = float.NegativeInfinity;
			exit.y = float.PositiveInfinity;
		}
		else
		{
			entry.y = invEntry.y / velocity.y;
			exit.y = invExit.y / velocity.y;
		}

		float entryTime = MathF.Max(entry.x, entry.y);
		float exitTime = MathF.Min(exit.x, exit.y);

		if (entryTime > exitTime || /*entry.x < 0 && entry.y < 0 &&*/ exit.x < 0 || exit.y < 0 || entry.x > 1 || entry.y > 1 || velocity.x == 0 && Math.Sign(invEntry.x) == Math.Sign(invExit.x) || velocity.y == 0 && Math.Sign(invEntry.y) == Math.Sign(invExit.y))
		{
			normal = Vector2.Zero;
			distance = -1;
			return false;
		}
		else
		{
			if (entry.x > entry.y)
			{
				if (invEntry.x < 0)
					normal = new Vector2(1, 0);
				else
					normal = new Vector2(-1, 0);
			}
			else
			{
				if (invEntry.y < 0)
					normal = new Vector2(0, 1);
				else
					normal = new Vector2(0, -1);
			}
		}

		distance = velocity.length * entryTime;
		return entryTime <= 1;
	}

	public HitData sweep(Vector2 origin, FloatRect rect, Vector2 direction, float range, uint filterMask = 1)
	{
		Entity hitEntity = null;
		float hitDistance = range;
		Vector2 hitNormal = Vector2.Zero;
		for (int i = 0; i < entities.Count; i++)
		{
			if (entities[i].collider != null && (entities[i].filterGroup & filterMask) != 0 && !entities[i].removed)
			{
				Vector2 min = entities[i].position + entities[i].collider.min;
				Vector2 max = entities[i].position + entities[i].collider.max;
				if (sweepAABBs(origin + rect.min, origin + rect.max, direction * range, min, max, out float distance, out Vector2 normal))
				{
					if (distance < hitDistance)
					{
						hitEntity = entities[i];
						hitDistance = distance;
						hitNormal = normal;
					}
				}
			}
		}

		if (hitDistance != range)
			return new HitData() { distance = hitDistance, entity = hitEntity, normal = hitNormal };
		return null;
	}

	public int sweepNoBlock(Vector2 origin, FloatRect rect, Vector2 direction, float range, Span<HitData> hits, uint filterMask = 1)
	{
		int numHits = 0;
		for (int i = 0; i < entities.Count; i++)
		{
			if (entities[i].collider != null && (entities[i].filterGroup & filterMask) != 0 && !entities[i].removed)
			{
				Vector2 min = entities[i].position + entities[i].collider.min;
				Vector2 max = entities[i].position + entities[i].collider.max;
				if (sweepAABBs(origin + rect.min, origin + rect.max, direction * range, min, max, out float distance, out Vector2 normal))
				{
					if (numHits < hits.Length && distance < range)
						hits[numHits++] = new HitData() { entity = entities[i], distance = distance, normal = normal };
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

	public HitData hitTiles(Vector2 position)
	{
		Vector2i tilePosition = (Vector2i)Vector2.Floor(position);
		TileType tile = getTile(tilePosition);
		if (tile != null && tile.isSolid && !tile.isPlatform)
			return new HitData() { position = position };
		return null;
	}

	public bool sampleTiles(Vector2 position)
	{
		Vector2i tilePosition = (Vector2i)Vector2.Floor(position);
		TileType tile = getTile(tilePosition);
		if (tile != null && tile.isSolid && !tile.isPlatform)
			return true;

		for (int i = 0; i < colliders.Count; i++)
		{
			if (!colliders[i].platformCollider)
			{
				Vector2 min = colliders[i].position + colliders[i].collider.min;
				Vector2 max = colliders[i].position + colliders[i].collider.max;
				if (position.x >= min.x && position.x <= max.x && position.y >= min.y && position.y <= max.y)
					return true;
			}
		}

		return false;
	}

	public HitData sample(Vector2 position, uint filterMask = 0)
	{
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
		if (filterMask == 0)
			return hitTiles(position);
		return null;
	}
}
