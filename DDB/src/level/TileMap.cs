using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TileMap
{
	int x, z;
	int width, height;
	int[] grid;


	public TileMap()
	{
		x = 0;
		z = 0;
		width = 0;
		height = 0;
		grid = null;
	}

	void resize(int x0, int z0, int x1, int z1)
	{
		int newX = x0;
		int newZ = z0;
		int newWidth = x1 - x0 + 1;
		int newHeight = z1 - z0 + 1;
		int[] newGrid = new int[newWidth * newHeight];

		int copyX = Math.Max(newX, x);
		int copyZ = Math.Max(newZ, z);
		int copyWidth = Math.Min(newWidth, width);
		int copyHeight = Math.Min(newHeight, height);

		Array.Fill(newGrid, 0);
		for (int z = copyZ; z < copyZ + copyHeight; z++)
		{
			for (int x = copyX; x < copyX + copyWidth; x++)
			{
				newGrid[(x - newX) + (z - newZ) * newWidth] = grid[(x - this.x) + (z - this.z) * width];
			}
		}

		x = newX;
		z = newZ;
		width = newWidth;
		height = newHeight;
		grid = newGrid;
	}

	public void placeRoom(Vector2i position, Vector2i size, RoomType room)
	{
		int x0 = position.x;
		int z0 = position.y;
		int x1 = position.x + size.x - 1;
		int z1 = position.y + size.y - 1;
		if (x0 < x || z0 < z || x1 > x + width || z1 > z + height)
			resize(Math.Min(x0, x), Math.Min(z0, z), Math.Max(x1, x + width - 1), Math.Max(z1, z + height - 1));

		for (int z = z0; z <= z1; z++)
		{
			for (int x = x0; x <= x1; x++)
			{
				grid[(x - this.x) + (z - this.z) * width] = room.id;
			}
		}
	}

	public bool overlapsRoom(Vector2i position, Vector2i size)
	{
		for (int z = position.y; z < position.y + size.y; z++)
		{
			for (int x = position.x; x < position.x + size.x; x++)
			{
				bool outOfBounds = x < this.x || x >= this.x + width || z < this.z || z >= this.z + height;
				if (!outOfBounds && grid[(x - this.x) + (z - this.z) * width] != 0)
					return true;
			}
		}
		return false;
	}
}
