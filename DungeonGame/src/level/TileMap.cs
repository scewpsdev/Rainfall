using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TileMap
{
	public const ushort FLAG_ROOM = 1 << 1;
	public const ushort FLAG_DOORWAY = 1 << 2;
	public const ushort FLAG_CORRIDOR = 1 << 3;
	public const ushort FLAG_STRUCTURE = FLAG_ROOM | FLAG_DOORWAY | FLAG_CORRIDOR;

	public const ushort FLAG_ROOM_WALL = 1 << 4;
	public const ushort FLAG_CORRIDOR_WALL = 1 << 5;
	public const ushort FLAG_WALL = FLAG_ROOM_WALL | FLAG_CORRIDOR_WALL;

	public const ushort FLAG_ASTAR_PATH = 1 << 6;

	public const ushort FLAG_LADDER = 1 << 7;


	public Vector3i mapPosition;
	public Vector3i mapSize;
	ulong[] grid;

	Material levelMaterial;
	Vector2i atlasSize;


	public TileMap()
	{
		levelMaterial = new Material(0xFFFFFFFF, 0.0f, 1.0f, Vector3.Zero, 0.0f,
			Resource.GetTexture("res/level/level1/level1_diffuse.png", false),
			null,
			Resource.GetTexture("res/level/level1/level1_occlusionRoughnessMetallic.png", false),
			Resource.GetTexture("res/level/level1/level1_occlusionRoughnessMetallic.png", false),
			null);
		atlasSize = new Vector2i(8, 8);

		reset();
	}

	public void reset()
	{
		mapPosition = Vector3i.Zero;
		mapSize = Vector3i.Zero;
		grid = null;
	}

	public void resize(int x0, int y0, int z0, int x1, int y1, int z1)
	{
		int newX = x0;
		int newY = y0;
		int newZ = z0;
		int newWidth = x1 - x0 + 1;
		int newHeight = y1 - y0 + 1;
		int newDepth = z1 - z0 + 1;
		ulong[] newGrid = new ulong[newWidth * newHeight * newDepth];

		int copyX = Math.Max(newX, mapPosition.x);
		int copyY = Math.Max(newY, mapPosition.y);
		int copyZ = Math.Max(newZ, mapPosition.z);
		int copyX1 = Math.Min(newX + newWidth - 1, mapPosition.x + mapSize.x - 1);
		int copyY1 = Math.Min(newY + newHeight - 1, mapPosition.y + mapSize.y - 1);
		int copyZ1 = Math.Min(newZ + newDepth - 1, mapPosition.z + mapSize.z - 1);
		int copyWidth = copyX1 - copyX + 1;
		int copyHeight = copyY1 - copyY + 1;
		int copyDepth = copyZ1 - copyZ + 1;

		Array.Fill(newGrid, 0u);
		for (int z = copyZ; z < copyZ + copyDepth; z++)
		{
			for (int x = copyX; x < copyX + copyWidth; x++)
			{
				for (int y = copyY; y < copyY + copyHeight; y++)
				{
					newGrid[(x - newX) + (z - newZ) * newWidth + (y - newY) * newWidth * newDepth] = grid[(x - mapPosition.x) + (z - mapPosition.z) * mapSize.x + (y - mapPosition.y) * mapSize.x * mapSize.z];
				}
			}
		}

		mapPosition = new Vector3i(newX, newY, newZ);
		mapSize = new Vector3i(newWidth, newHeight, newDepth);
		grid = newGrid;
	}

	public void makeSpaceFor(int x0, int y0, int z0, int x1, int y1, int z1)
	{
		if (!(x0 >= mapPosition.x && x1 < mapPosition.x + mapSize.x && y0 >= mapPosition.y && y1 < mapPosition.y + mapSize.y && z0 >= mapPosition.z && z1 < mapPosition.z + mapSize.z))
			resize(Math.Min(x0, mapPosition.x), Math.Min(y0, mapPosition.y), Math.Min(z0, mapPosition.z), Math.Max(x1, mapPosition.x + mapSize.x - 1), Math.Max(y1, mapPosition.y + mapSize.y - 1), Math.Max(z1, mapPosition.z + mapSize.z - 1));
	}

	public void setRoomID(int x, int y, int z, ushort roomID)
	{
		makeSpaceFor(x, y, z, x, y, z);

		ulong data = grid[(x - mapPosition.x) + (z - mapPosition.z) * mapSize.x + (y - mapPosition.y) * mapSize.x * mapSize.z] & unchecked(0x0000FFFFFFFFFFFF);
		grid[(x - mapPosition.x) + (z - mapPosition.z) * mapSize.x + (y - mapPosition.y) * mapSize.x * mapSize.z] = data | ((ulong)roomID << 48);
	}

	public void setRoomID(Vector3i position, ushort roomID)
	{
		setRoomID(position.x, position.y, position.z, roomID);
	}

	public ushort getRoomID(int x, int y, int z)
	{
		if (x >= mapPosition.x && x < mapPosition.x + mapSize.x && y >= mapPosition.y && y < mapPosition.y + mapSize.y && z >= mapPosition.z && z < mapPosition.z + mapSize.z)
			return (ushort)((grid[(x - mapPosition.x) + (z - mapPosition.z) * mapSize.x + (y - mapPosition.y) * mapSize.x * mapSize.z] & 0xFFFF000000000000) >> 48);
		return 0;
	}

	public ushort getRoomID(Vector3i position)
	{
		return getRoomID(position.x, position.y, position.z);
	}

	public void setTile(int x, int y, int z, uint tile)
	{
		makeSpaceFor(x, y, z, x, y, z);

		ulong data = grid[(x - mapPosition.x) + (z - mapPosition.z) * mapSize.x + (y - mapPosition.y) * mapSize.x * mapSize.z] & unchecked(0xFFFFFFFF00000000);
		grid[(x - mapPosition.x) + (z - mapPosition.z) * mapSize.x + (y - mapPosition.y) * mapSize.x * mapSize.z] = data | tile;
	}

	public void setTile(Vector3i position, uint tile)
	{
		setTile(position.x, position.y, position.z, tile);
	}

	public uint getTile(int x, int y, int z)
	{
		if (x >= mapPosition.x && x < mapPosition.x + mapSize.x && y >= mapPosition.y && y < mapPosition.y + mapSize.y && z >= mapPosition.z && z < mapPosition.z + mapSize.z)
			return (uint)(grid[(x - mapPosition.x) + (z - mapPosition.z) * mapSize.x + (y - mapPosition.y) * mapSize.x * mapSize.z] & 0xFFFFFFFF);
		return 0;
	}

	public uint getTile(Vector3i position)
	{
		return getTile(position.x, position.y, position.z);
	}

	public void setFlag(int x, int y, int z, ushort flag, bool set)
	{
		makeSpaceFor(x, y, z, x, y, z);

		ulong data = grid[(x - mapPosition.x) + (z - mapPosition.z) * mapSize.x + (y - mapPosition.y) * mapSize.x * mapSize.z];
		ulong lflag = (ulong)flag << 32;
		if (set)
			data |= lflag;
		else
			data = (data | lflag) ^ lflag;
		grid[(x - mapPosition.x) + (z - mapPosition.z) * mapSize.x + (y - mapPosition.y) * mapSize.x * mapSize.z] = data;
	}

	public void setFlag(Vector3i position, ushort flag, bool set)
	{
		setFlag(position.x, position.y, position.z, flag, set);
	}

	public bool getFlag(int x, int y, int z, ushort flag)
	{
		ulong lflag = (ulong)flag << 32;
		if (x >= mapPosition.x && x < mapPosition.x + mapSize.x && y >= mapPosition.y && y < mapPosition.y + mapSize.y && z >= mapPosition.z && z < mapPosition.z + mapSize.z)
			return (grid[(x - mapPosition.x) + (z - mapPosition.z) * mapSize.x + (y - mapPosition.y) * mapSize.x * mapSize.z] & lflag) != 0;
		return false;
	}

	public bool getFlag(Vector3i position, ushort flag)
	{
		return getFlag(position.x, position.y, position.z, flag);
	}

	public ushort getFlags(int x, int y, int z)
	{
		if (x >= mapPosition.x && x < mapPosition.x + mapSize.x && y >= mapPosition.y && y < mapPosition.y + mapSize.y && z >= mapPosition.z && z < mapPosition.z + mapSize.z)
			return (ushort)((grid[(x - mapPosition.x) + (z - mapPosition.z) * mapSize.x + (y - mapPosition.y) * mapSize.x * mapSize.z] & 0x0000FFFF00000000) >> 32);
		return 0;
	}

	public ushort getFlags(Vector3i position)
	{
		return getFlags(position.x, position.y, position.z);
	}

	public bool isWall(int x, int y, int z)
	{
		return (getFlags(x, y, z) & FLAG_STRUCTURE) == 0;
	}

	public bool isWall(Vector3i position)
	{
		return isWall(position.x, position.y, position.z);
	}

	void generateFloorMesh(int x, int y, int z, ModelBatch batch)
	{
		Tile tile = Tile.Get(getTile(x, y - 1, z));
		Vector2i atlasPos = tile.atlasPositionTop != Vector2i.Zero ? tile.atlasPositionTop : tile.atlasPosition;

		int i0 = batch.addVertex(new Vector3(x, y, z), Vector3.Up, Vector3.Right, new Vector2(atlasPos.x, atlasPos.y) / atlasSize);
		int i1 = batch.addVertex(new Vector3(x, y, z + 1), Vector3.Up, Vector3.Right, new Vector2(atlasPos.x, atlasPos.y + 1) / atlasSize);
		int i2 = batch.addVertex(new Vector3(x + 1, y, z + 1), Vector3.Up, Vector3.Right, new Vector2(atlasPos.x + 1, atlasPos.y + 1) / atlasSize);
		int i3 = batch.addVertex(new Vector3(x + 1, y, z), Vector3.Up, Vector3.Right, new Vector2(atlasPos.x + 1, atlasPos.y) / atlasSize);

		batch.addTriangle(i0, i1, i2);
		batch.addTriangle(i2, i3, i0);
	}

	void generateCeilingMesh(int x, int y, int z, ModelBatch batch)
	{
		Tile tile = Tile.Get(getTile(x, y + 1, z));
		Vector2i atlasPos = tile.atlasPositionBottom != Vector2i.Zero ? tile.atlasPositionBottom : tile.atlasPosition;

		int i0 = batch.addVertex(new Vector3(x, y + 1, z), Vector3.Down, Vector3.Right, new Vector2(atlasPos.x, atlasPos.y + 1) / atlasSize);
		int i1 = batch.addVertex(new Vector3(x, y + 1, z + 1), Vector3.Down, Vector3.Right, new Vector2(atlasPos.x, atlasPos.y) / atlasSize);
		int i2 = batch.addVertex(new Vector3(x + 1, y + 1, z + 1), Vector3.Down, Vector3.Right, new Vector2(atlasPos.x + 1, atlasPos.y) / atlasSize);
		int i3 = batch.addVertex(new Vector3(x + 1, y + 1, z), Vector3.Down, Vector3.Right, new Vector2(atlasPos.x + 1, atlasPos.y + 1) / atlasSize);

		batch.addTriangle(i0, i3, i2);
		batch.addTriangle(i2, i1, i0);
	}

	void generateWallMeshNorth(int x, int y, int z, ModelBatch batch)
	{
		Tile tile = Tile.Get(getTile(x, y, z - 1));
		Vector2i atlasPos = tile.atlasPosition;

		int i0 = batch.addVertex(new Vector3(x, y, z), Vector3.Back, Vector3.Right, new Vector2(atlasPos.x, atlasPos.y + 1) / atlasSize);
		int i1 = batch.addVertex(new Vector3(x + 1, y, z), Vector3.Back, Vector3.Right, new Vector2(atlasPos.x + 1, atlasPos.y + 1) / atlasSize);
		int i2 = batch.addVertex(new Vector3(x + 1, y + 1, z), Vector3.Back, Vector3.Right, new Vector2(atlasPos.x + 1, atlasPos.y) / atlasSize);
		int i3 = batch.addVertex(new Vector3(x, y + 1, z), Vector3.Back, Vector3.Right, new Vector2(atlasPos.x, atlasPos.y) / atlasSize);

		batch.addTriangle(i0, i1, i2);
		batch.addTriangle(i2, i3, i0);
	}

	void generateWallMeshSouth(int x, int y, int z, ModelBatch batch)
	{
		Tile tile = Tile.Get(getTile(x, y, z + 1));
		Vector2i atlasPos = tile.atlasPosition;

		int i0 = batch.addVertex(new Vector3(x + 1, y, z + 1), Vector3.Forward, Vector3.Left, new Vector2(atlasPos.x, atlasPos.y + 1) / atlasSize);
		int i1 = batch.addVertex(new Vector3(x, y, z + 1), Vector3.Forward, Vector3.Left, new Vector2(atlasPos.x + 1, atlasPos.y + 1) / atlasSize);
		int i2 = batch.addVertex(new Vector3(x, y + 1, z + 1), Vector3.Forward, Vector3.Left, new Vector2(atlasPos.x + 1, atlasPos.y) / atlasSize);
		int i3 = batch.addVertex(new Vector3(x + 1, y + 1, z + 1), Vector3.Forward, Vector3.Left, new Vector2(atlasPos.x, atlasPos.y) / atlasSize);

		batch.addTriangle(i0, i1, i2);
		batch.addTriangle(i2, i3, i0);
	}

	void generateWallMeshWest(int x, int y, int z, ModelBatch batch)
	{
		Tile tile = Tile.Get(getTile(x - 1, y, z));
		Vector2i atlasPos = tile.atlasPosition;

		int i0 = batch.addVertex(new Vector3(x, y, z + 1), Vector3.Right, Vector3.Forward, new Vector2(atlasPos.x, atlasPos.y + 1) / atlasSize);
		int i1 = batch.addVertex(new Vector3(x, y, z), Vector3.Right, Vector3.Forward, new Vector2(atlasPos.x + 1, atlasPos.y + 1) / atlasSize);
		int i2 = batch.addVertex(new Vector3(x, y + 1, z), Vector3.Right, Vector3.Forward, new Vector2(atlasPos.x + 1, atlasPos.y) / atlasSize);
		int i3 = batch.addVertex(new Vector3(x, y + 1, z + 1), Vector3.Right, Vector3.Forward, new Vector2(atlasPos.x, atlasPos.y) / atlasSize);

		batch.addTriangle(i0, i1, i2);
		batch.addTriangle(i2, i3, i0);
	}

	void generateWallMeshEast(int x, int y, int z, ModelBatch batch)
	{
		Tile tile = Tile.Get(getTile(x + 1, y, z));
		Vector2i atlasPos = tile.atlasPosition;

		int i0 = batch.addVertex(new Vector3(x + 1, y, z), Vector3.Left, Vector3.Back, new Vector2(atlasPos.x, atlasPos.y + 1) / atlasSize);
		int i1 = batch.addVertex(new Vector3(x + 1, y, z + 1), Vector3.Left, Vector3.Back, new Vector2(atlasPos.x + 1, atlasPos.y + 1) / atlasSize);
		int i2 = batch.addVertex(new Vector3(x + 1, y + 1, z + 1), Vector3.Left, Vector3.Back, new Vector2(atlasPos.x + 1, atlasPos.y) / atlasSize);
		int i3 = batch.addVertex(new Vector3(x + 1, y + 1, z), Vector3.Left, Vector3.Back, new Vector2(atlasPos.x, atlasPos.y) / atlasSize);

		batch.addTriangle(i0, i1, i2);
		batch.addTriangle(i2, i3, i0);
	}

	public void updateMesh(ModelBatch batch, Level level)
	{
		batch.setMaterial(levelMaterial);

		for (int z = mapPosition.z; z < mapPosition.z + mapSize.z; z++)
		{
			for (int x = mapPosition.x; x < mapPosition.x + mapSize.x; x++)
			{
				for (int y = mapPosition.y; y < mapPosition.y + mapSize.y; y++)
				{
					uint left = getRoomID(x - 1, y, z);
					uint right = getRoomID(x + 1, y, z);
					uint bottom = getRoomID(x, y - 1, z);
					uint top = getRoomID(x, y + 1, z);
					uint front = getRoomID(x, y, z - 1);
					uint back = getRoomID(x, y, z + 1);

					if (isWall(x, y, z))
						continue;

					Vector3i p = new Vector3i(x, y, z);
					Matrix tileTransform = Matrix.CreateTranslation(new Vector3(x + 0.5f, y, z + 0.5f));

					//Room room = findRoomAtPosition(p);

					if (isWall(p + Vector3i.Down))
					{
						//if (room == null || room.type.generateWallMeshes)
						generateFloorMesh(x, y, z, batch);
						//batch.addModel(floor, tileTransform, mod(x, 3) + mod(z, 3) * 3, new Vector2i(3));
						level.body.addBoxCollider(
							new Vector3(0.5f),
							tileTransform.translation + new Vector3(0, -0.5f, 0),
							Quaternion.Identity);
					}
					if (isWall(p + Vector3i.Up))
					{
						//if (room == null || room.type.generateWallMeshes)
						generateCeilingMesh(x, y, z, batch);
						//ceilingBatch.addModel(ceiling, Matrix.CreateTranslation(0, 1, 0) * tileTransform, mod(-x, 3) + mod(z, 3) * 3, new Vector2i(3));
						level.body.addBoxCollider(
							new Vector3(0.5f),
							tileTransform.translation + new Vector3(0, 1.5f, 0),
							Quaternion.Identity);
					}
					if (isWall(p + Vector3i.Forward))
					{
						//if (room == null || room.type.generateWallMeshes)
						generateWallMeshNorth(x, y, z, batch);
						//wallBatch.addModel(wall, tileTransform, mod(x + z - 1, 3) + mod(-y, 3) * 3, new Vector2i(3));
						level.body.addBoxCollider(new Vector3(0.5f), tileTransform.translation + new Vector3(0.0f, 0.5f, -1.0f), Quaternion.Identity);
					}
					if (isWall(p + Vector3i.Back))
					{
						//if (room == null || room.type.generateWallMeshes)
						generateWallMeshSouth(x, y, z, batch);
						//wallBatch.addModel(wall, tileTransform * Matrix.CreateRotation(Vector3.Up, MathF.PI), mod(-x + z + 1, 3) + mod(-y, 3) * 3, new Vector2i(3));
						level.body.addBoxCollider(new Vector3(0.5f), tileTransform.translation + new Vector3(0.0f, 0.5f, 1.0f), Quaternion.Identity);
					}
					if (isWall(p + Vector3i.Left))
					{
						//if (room == null || room.type.generateWallMeshes)
						generateWallMeshWest(x, y, z, batch);
						//wallBatch.addModel(wall, tileTransform * Matrix.CreateRotation(Vector3.Up, MathF.PI * 0.5f), mod(x - 1 - z, 3) + mod(-y, 3) * 3, new Vector2i(3));
						level.body.addBoxCollider(new Vector3(0.5f), tileTransform.translation + new Vector3(-1.0f, 0.5f, 0.0f), Quaternion.Identity);
					}
					if (isWall(p + Vector3i.Right))
					{
						//if (room == null || room.type.generateWallMeshes)
						generateWallMeshEast(x, y, z, batch);
						//wallBatch.addModel(wall, tileTransform * Matrix.CreateRotation(Vector3.Up, MathF.PI * -0.5f), mod(x + 1 + z, 3) + mod(-y, 3) * 3, new Vector2i(3));
						level.body.addBoxCollider(new Vector3(0.5f), tileTransform.translation + new Vector3(1.0f, 0.5f, 0.0f), Quaternion.Identity);
					}
				}
			}
		}
	}

	BoundingBox transformBoundingBox(BoundingBox boundingBox, Matrix transform)
	{
		Vector4 p000 = transform * new Vector4(boundingBox.x0, boundingBox.y0, boundingBox.z0, 1.0f);
		Vector4 p001 = transform * new Vector4(boundingBox.x0, boundingBox.y0, boundingBox.z1, 1.0f);
		Vector4 p010 = transform * new Vector4(boundingBox.x0, boundingBox.y1, boundingBox.z0, 1.0f);
		Vector4 p011 = transform * new Vector4(boundingBox.x0, boundingBox.y1, boundingBox.z1, 1.0f);
		Vector4 p100 = transform * new Vector4(boundingBox.x1, boundingBox.y0, boundingBox.z0, 1.0f);
		Vector4 p101 = transform * new Vector4(boundingBox.x1, boundingBox.y0, boundingBox.z1, 1.0f);
		Vector4 p110 = transform * new Vector4(boundingBox.x1, boundingBox.y1, boundingBox.z0, 1.0f);
		Vector4 p111 = transform * new Vector4(boundingBox.x1, boundingBox.y1, boundingBox.z1, 1.0f);

		float x0 = MathF.Min(MathF.Min(MathF.Min(p000.x, p001.x), MathF.Min(p010.x, p011.x)), MathF.Min(MathF.Min(p100.x, p101.x), MathF.Min(p110.x, p111.x)));
		float x1 = MathF.Max(MathF.Max(MathF.Max(p000.x, p001.x), MathF.Max(p010.x, p011.x)), MathF.Max(MathF.Max(p100.x, p101.x), MathF.Max(p110.x, p111.x)));
		float y0 = MathF.Min(MathF.Min(MathF.Min(p000.y, p001.y), MathF.Min(p010.y, p011.y)), MathF.Min(MathF.Min(p100.y, p101.y), MathF.Min(p110.y, p111.y)));
		float y1 = MathF.Max(MathF.Max(MathF.Max(p000.y, p001.y), MathF.Max(p010.y, p011.y)), MathF.Max(MathF.Max(p100.y, p101.y), MathF.Max(p110.y, p111.y)));
		float z0 = MathF.Min(MathF.Min(MathF.Min(p000.z, p001.z), MathF.Min(p010.z, p011.z)), MathF.Min(MathF.Min(p100.z, p101.z), MathF.Min(p110.z, p111.z)));
		float z1 = MathF.Max(MathF.Max(MathF.Max(p000.z, p001.z), MathF.Max(p010.z, p011.z)), MathF.Max(MathF.Max(p100.z, p101.z), MathF.Max(p110.z, p111.z)));

		return new BoundingBox() { x0 = x0, x1 = x1, y0 = y0, y1 = y1, z0 = z0, z1 = z1 };
	}

	Vector3i globalToLocal(Vector3i position, Matrix roomTransform)
	{
		Vector3 tileCenter = position + new Vector3(0.5f, 0.5f, 0.5f);
		Vector4 local = roomTransform.inverted * new Vector4(tileCenter, 1.0f);
		return (Vector3i)Vector3.Floor(local.xyz);
	}

	public void placeRoom(Room room)
	{
		int x0 = room.gridPosition.x;
		int x1 = room.gridPosition.x + room.gridSize.x - 1;
		int y0 = room.gridPosition.y;
		int y1 = room.gridPosition.y + room.gridSize.y - 1;
		int z0 = room.gridPosition.z;
		int z1 = room.gridPosition.z + room.gridSize.z - 1;

		makeSpaceFor(x0, y0, z0, x1, y1, z1);

		for (int z = z0 - 1; z <= z1 + 1; z++)
		{
			for (int x = x0 - 1; x <= x1 + 1; x++)
			{
				for (int y = y0 - 1; y <= y1 + 1; y++)
				{
					Vector3i localPos = globalToLocal(new Vector3i(x, y, z), room.transform);

					bool insideRoom = false;
					if (z >= z0 && z <= z1 && x >= x0 && x <= x1 && y >= y0 && y <= y1)
					{
						insideRoom = true;
						if (room.type.tiles != null)
						{
							if (room.type.tiles[localPos.x + localPos.y * room.type.size.x + localPos.z * room.type.size.x * room.type.size.y] != 0)
								insideRoom = false;
						}
					}
					if (insideRoom)
					{
						//grid[(x - mapPosition.x) + (z - mapPosition.z) * mapSize.x + (y - mapPosition.y) * mapSize.x * mapSize.z] = (ushort)room.id;
						setRoomID(x, y, z, (ushort)room.id);
						Debug.Assert(room.id >= 0 && room.id <= ushort.MaxValue);

						if (room.type.sectorType == SectorType.Room)
							setFlag(x, y, z, FLAG_ROOM, true);
						else
							setFlag(x, y, z, FLAG_CORRIDOR, true);

						setFlag(x, y, z, FLAG_WALL, false);
						setFlag(x, y, z, FLAG_CORRIDOR_WALL, false);
					}
					else
					{
						if (room.type.getTile(localPos + Vector3i.Left) == 0 ||
							room.type.getTile(localPos + Vector3i.Right) == 0 ||
							room.type.getTile(localPos + Vector3i.Down) == 0 ||
							room.type.getTile(localPos + Vector3i.Up) == 0 ||
							room.type.getTile(localPos + Vector3i.Forward) == 0 ||
							room.type.getTile(localPos + Vector3i.Back) == 0)
						{
							setFlag(x, y, z, room.type.sectorType == SectorType.Room ? FLAG_ROOM_WALL : FLAG_CORRIDOR_WALL, true);

							// Wall
							if (room.type.getTile(localPos + Vector3i.Left) == 0 ||
								room.type.getTile(localPos + Vector3i.Right) == 0 ||
								room.type.getTile(localPos + Vector3i.Forward) == 0 ||
								room.type.getTile(localPos + Vector3i.Back) == 0)
							{
								setTile(x, y, z, Tile.bricks.id);
							}
							// Floor
							else if (room.type.getTile(localPos + Vector3i.Up) == 0)
							{
								setTile(x, y, z, Tile.dirt.id);
							}
							// Ceiling
							else if (room.type.getTile(localPos + Vector3i.Down) == 0)
							{
								setTile(x, y, z, Tile.cobblestone.id);
							}
						}
					}
				}
			}
		}
	}

	public void removeRoom(Room room)
	{
		int x0 = room.gridPosition.x;
		int x1 = room.gridPosition.x + room.gridSize.x - 1;
		int y0 = room.gridPosition.y;
		int y1 = room.gridPosition.y + room.gridSize.y - 1;
		int z0 = room.gridPosition.z;
		int z1 = room.gridPosition.z + room.gridSize.z - 1;

		for (int z = z0 - 1; z <= z1 + 1; z++)
		{
			for (int x = x0 - 1; x <= x1 + 1; x++)
			{
				for (int y = y0 - 1; y <= y1 + 1; y++)
				{
					Vector3i localPos = globalToLocal(new Vector3i(x, y, z), room.transform);

					bool insideRoom = false;
					if (z >= z0 && z <= z1 && x >= x0 && x <= x1 && y >= y0 && y <= y1)
					{
						insideRoom = true;
						if (room.type.tiles != null)
						{
							if (room.type.tiles[localPos.x + localPos.y * room.type.size.x + localPos.z * room.type.size.x * room.type.size.y] != 0)
								insideRoom = false;
						}
					}
					if (insideRoom)
					{
						grid[(x - mapPosition.x) + (z - mapPosition.z) * mapSize.x + (y - mapPosition.y) * mapSize.x * mapSize.z] = 0;

						setFlag(x, y, z, FLAG_ROOM, false);
						setFlag(x, y, z, FLAG_CORRIDOR, false);

						setFlag(x, y, z, FLAG_WALL, false);
						setFlag(x, y, z, FLAG_CORRIDOR_WALL, false);
					}
					else
					{
						if (room.type.getTile(localPos + Vector3i.Left) == 0 ||
							room.type.getTile(localPos + Vector3i.Right) == 0 ||
							room.type.getTile(localPos + Vector3i.Down) == 0 ||
							room.type.getTile(localPos + Vector3i.Up) == 0 ||
							room.type.getTile(localPos + Vector3i.Forward) == 0 ||
							room.type.getTile(localPos + Vector3i.Back) == 0)
						{
							setFlag(x, y, z, FLAG_WALL, false);
							setFlag(x, y, z, FLAG_CORRIDOR_WALL, false);
						}
					}
				}
			}
		}
	}

	public bool overlapsRoom(RoomType roomType, Matrix transform)
	{
		BoundingBox boundingBox = new BoundingBox(0.0f, 0.0f, 0.0f, roomType.size.x, roomType.size.y, roomType.size.z);
		boundingBox = transformBoundingBox(boundingBox, transform);

		int x0 = (int)MathF.Floor(boundingBox.x0 + 0.1f);
		int x1 = (int)MathF.Floor(boundingBox.x1 - 0.1f);
		int y0 = (int)MathF.Floor(boundingBox.y0 + 0.1f);
		int y1 = (int)MathF.Floor(boundingBox.y1 - 0.1f);
		int z0 = (int)MathF.Floor(boundingBox.z0 + 0.1f);
		int z1 = (int)MathF.Floor(boundingBox.z1 - 0.1f);

		for (int z = z0; z <= z1; z++)
		{
			for (int x = x0; x <= x1; x++)
			{
				if (roomType.tiles != null)
				{
					Vector3i localPos = globalToLocal(new Vector3i(x, y0, z), transform);
					if (roomType.tiles[localPos.x + localPos.y * roomType.size.x + localPos.z * roomType.size.x * roomType.size.y] != 0)
						continue;
				}
				for (int y = y0; y <= y1; y++)
				{
					int tile = getRoomID(x, y, z);
					int left = getRoomID(x - 1, y, z);
					int right = getRoomID(x + 1, y, z);
					int down = getRoomID(x, y - 1, z);
					int up = getRoomID(x, y + 1, z);
					int forward = getRoomID(x, y, z - 1);
					int back = getRoomID(x, y, z + 1);
					if (tile != 0 || left != 0 || right != 0 || down != 0 || up != 0 || forward != 0 || back != 0)
						return true;
				}
			}
		}
		return false;
	}

	public void getRelativeTilePosition(Vector3 position, out Vector3i tilePos)
	{
		int x = (int)MathF.Floor(position.x) - mapPosition.x;
		int y = (int)MathF.Floor(position.y) - mapPosition.y;
		int z = (int)MathF.Floor(position.z) - mapPosition.z;
		tilePos = new Vector3i(x, y, z);
	}
}
