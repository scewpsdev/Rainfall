using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TileMap
{
	public const int FLAG_ROOM = 1 << 1;
	public const int FLAG_DOORWAY = 1 << 2;
	public const int FLAG_CORRIDOR = 1 << 3;
	public const int FLAG_STRUCTURE = FLAG_ROOM | FLAG_DOORWAY | FLAG_CORRIDOR;

	public const int FLAG_ROOM_WALL = 1 << 4;
	public const int FLAG_CORRIDOR_WALL = 1 << 5;
	public const int FLAG_WALL = FLAG_ROOM_WALL | FLAG_CORRIDOR_WALL;

	public const int FLAG_ASTAR_PATH = 1 << 6;


	public Vector3i mapPosition;
	public Vector3i mapSize;
	int[] grid;


	public TileMap()
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
		int[] newGrid = new int[newWidth * newHeight * newDepth];

		int copyX = Math.Max(newX, mapPosition.x);
		int copyY = Math.Max(newY, mapPosition.y);
		int copyZ = Math.Max(newZ, mapPosition.z);
		int copyX1 = Math.Min(newX + newWidth - 1, mapPosition.x + mapSize.x - 1);
		int copyY1 = Math.Min(newY + newHeight - 1, mapPosition.y + mapSize.y - 1);
		int copyZ1 = Math.Min(newZ + newDepth - 1, mapPosition.z + mapSize.z - 1);
		int copyWidth = copyX1 - copyX + 1;
		int copyHeight = copyY1 - copyY + 1;
		int copyDepth = copyZ1 - copyZ + 1;

		Array.Fill(newGrid, 0);
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

	public void setTile(int x, int y, int z, int tile)
	{
		if (x >= mapPosition.x && x < mapPosition.x + mapSize.x && y >= mapPosition.y && y < mapPosition.y + mapSize.y && z >= mapPosition.z && z < mapPosition.z + mapSize.z)
		{
			int flags = grid[(x - mapPosition.x) + (z - mapPosition.z) * mapSize.x + (y - mapPosition.y) * mapSize.x * mapSize.z] & unchecked((int)0xFFFF0000);
			grid[(x - mapPosition.x) + (z - mapPosition.z) * mapSize.x + (y - mapPosition.y) * mapSize.x * mapSize.z] = flags | tile;
		}
	}

	public void setTile(Vector3i position, int tile)
	{
		setTile(position.x, position.y, position.z, tile);
	}

	public int getTile(int x, int y, int z)
	{
		if (x >= mapPosition.x && x < mapPosition.x + mapSize.x && y >= mapPosition.y && y < mapPosition.y + mapSize.y && z >= mapPosition.z && z < mapPosition.z + mapSize.z)
			return grid[(x - mapPosition.x) + (z - mapPosition.z) * mapSize.x + (y - mapPosition.y) * mapSize.x * mapSize.z] & 0x0000FFFF;
		return 0;
	}

	public int getTile(Vector3i position)
	{
		return getTile(position.x, position.y, position.z);
	}

	public void setFlag(int x, int y, int z, int flag, bool set)
	{
		if (x >= mapPosition.x && x < mapPosition.x + mapSize.x && y >= mapPosition.y && y < mapPosition.y + mapSize.y && z >= mapPosition.z && z < mapPosition.z + mapSize.z)
		{
			int data = grid[(x - mapPosition.x) + (z - mapPosition.z) * mapSize.x + (y - mapPosition.y) * mapSize.x * mapSize.z];
			flag <<= 16;
			if (set)
				data |= flag;
			else
				data = (data | flag) ^ flag;
			grid[(x - mapPosition.x) + (z - mapPosition.z) * mapSize.x + (y - mapPosition.y) * mapSize.x * mapSize.z] = data;
		}
	}

	public void setFlag(Vector3i position, int flag, bool set)
	{
		setFlag(position.x, position.y, position.z, flag, set);
	}

	public bool getFlag(int x, int y, int z, int flag)
	{
		flag <<= 16;
		if (x >= mapPosition.x && x < mapPosition.x + mapSize.x && y >= mapPosition.y && y < mapPosition.y + mapSize.y && z >= mapPosition.z && z < mapPosition.z + mapSize.z)
			return (grid[(x - mapPosition.x) + (z - mapPosition.z) * mapSize.x + (y - mapPosition.y) * mapSize.x * mapSize.z] & flag) != 0;
		return false;
	}

	public bool getFlag(Vector3i position, int flag)
	{
		return getFlag(position.x, position.y, position.z, flag);
	}

	public int getFlags(int x, int y, int z)
	{
		if (x >= mapPosition.x && x < mapPosition.x + mapSize.x && y >= mapPosition.y && y < mapPosition.y + mapSize.y && z >= mapPosition.z && z < mapPosition.z + mapSize.z)
			return grid[(x - mapPosition.x) + (z - mapPosition.z) * mapSize.x + (y - mapPosition.y) * mapSize.x * mapSize.z] >> 16;
		return 0;
	}

	public int getFlags(Vector3i position)
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
		Vector3 tileCenter = position + new Vector3(0.5f, 0.0f, 0.5f);
		Vector4 local = roomTransform.inverted * new Vector4(tileCenter, 1.0f);
		return (Vector3i)local.xyz;
	}

	public void placeRoom(Room room)
	{
		int x0 = room.gridPosition.x;
		int x1 = room.gridPosition.x + room.gridSize.x - 1;
		int y0 = room.gridPosition.y;
		int y1 = room.gridPosition.y + room.gridSize.y - 1;
		int z0 = room.gridPosition.z;
		int z1 = room.gridPosition.z + room.gridSize.z - 1;

		if (x0 < mapPosition.x || x1 >= mapPosition.x + mapSize.x ||
			y0 < mapPosition.y || y1 >= mapPosition.y + mapSize.y ||
			z0 < mapPosition.z || z1 >= mapPosition.z + mapSize.z)
			resize(Math.Min(x0, mapPosition.x), Math.Min(y0, mapPosition.y), Math.Min(z0, mapPosition.z), Math.Max(x1, mapPosition.x + mapSize.x - 1), Math.Max(y1, mapPosition.y + mapSize.y - 1), Math.Max(z1, mapPosition.z + mapSize.z - 1));

		for (int z = z0 - 1; z <= z1 + 1; z++)
		{
			for (int x = x0 - 1; x <= x1 + 1; x++)
			{
				for (int y = y0 - 1; y <= y1 + 1; y++)
				{
					if (z >= z0 && z <= z1 && x >= x0 && x <= x1 && y >= y0 && y <= y1)
					{
						if (room.type.mask != null)
						{
							Vector3i localPos = globalToLocal(new Vector3i(x, y, z), room.transform);
							if (!room.type.mask[localPos.x + localPos.y * room.type.size.x + localPos.z * room.type.size.x * room.type.size.y])
								continue;
						}

						grid[(x - mapPosition.x) + (z - mapPosition.z) * mapSize.x + (y - mapPosition.y) * mapSize.x * mapSize.z] = room.type.id;

						if (room.type.sectorType == SectorType.Room)
							setFlag(x, y, z, FLAG_ROOM, true);
						else
							setFlag(x, y, z, FLAG_CORRIDOR, true);

						setFlag(x, y, z, FLAG_WALL, false);
					}
					else
					{
						setFlag(x, y, z, room.type.sectorType == SectorType.Room ? FLAG_ROOM_WALL : FLAG_CORRIDOR_WALL, true);
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
				if (roomType.mask != null)
				{
					Vector3i localPos = globalToLocal(new Vector3i(x, y0, z), transform);
					if (!roomType.mask[localPos.x + localPos.y * roomType.size.x + localPos.z * roomType.size.x * roomType.size.y])
						continue;
				}
				for (int y = y0; y <= y1; y++)
				{
					int tile = getTile(x, y, z);
					int left = getTile(x - 1, y, z);
					int right = getTile(x + 1, y, z);
					int down = getTile(x, y - 1, z);
					int up = getTile(x, y + 1, z);
					int forward = getTile(x, y, z - 1);
					int back = getTile(x, y, z + 1);
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
