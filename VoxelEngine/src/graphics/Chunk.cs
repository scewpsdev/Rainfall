using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Chunk
{
	public const float CHUNK_SIZE = 16.0f;


	public readonly int resolution;
	public Vector3i position;

	Texture texture;
	ushort[] voxelData;
	List<ushort[]> mips = new List<ushort[]>();

	bool hasChanged = true;


	public Chunk(int resolution, GraphicsDevice graphics)
	{
		this.resolution = resolution;
		texture = graphics.createTexture(resolution, resolution, resolution, true, TextureFormat.RG8, (uint)SamplerFlags.Clamp | (uint)SamplerFlags.Point);
		voxelData = new ushort[resolution * resolution * resolution];
	}

	public bool update(GraphicsDevice graphics)
	{
		if (hasChanged)
		{
			graphics.setTextureData(texture, 0, 0, 0, 0, resolution, resolution, resolution, voxelData);
			generateMips(graphics);

			hasChanged = false;
			return true;
		}
		return false;
	}

	ushort encodeVoxelData(byte value, Vector3 normal, byte material)
	{
		byte nx = (byte)(normal.x < -0.38f ? 0 : normal.x > 0.38f ? 2 : 1);
		byte ny = (byte)(normal.y < -0.38f ? 0 : normal.y > 0.38f ? 2 : 1);
		byte nz = (byte)(normal.z < -0.38f ? 0 : normal.z > 0.38f ? 2 : 1);
		ushort voxel = (ushort)((material << 8) | (nx << 6) | (ny << 4) | (nz << 2) | value);
		return voxel;
	}

	void decodeVoxelData(ushort voxel, out byte value, out Vector3 normal, out byte material)
	{
		material = (byte)((voxel & 0xFF) >> 8);
		value = (byte)(voxel & 0b00000011);
		byte nx = (byte)((voxel & 0b11000000) >> 6);
		byte ny = (byte)((voxel & 0b00110000) >> 4);
		byte nz = (byte)((voxel & 0b00001100) >> 2);
		normal = new Vector3(nx - 1, ny - 1, nz - 1).normalized;
	}

	void generateMips(GraphicsDevice graphics)
	{
		int maxMip = Math.ILogB(resolution);

		int mipRes = resolution / 2;
		ushort[] lastMip = voxelData;
		for (int mip = 1; mip <= maxMip; mip++)
		{
			ushort[] mipData = new ushort[mipRes * mipRes * mipRes];
			for (int z = 0; z < mipRes; z++)
			{
				for (int y = 0; y < mipRes; y++)
				{
					for (int x = 0; x < mipRes; x++)
					{
						int xx = x * 2;
						int yy = y * 2;
						int zz = z * 2;

						decodeVoxelData(lastMip[xx + yy * mipRes * 2 + zz * mipRes * 2 * mipRes * 2], out byte value0, out Vector3 normal0, out byte material0);
						decodeVoxelData(lastMip[xx + 1 + yy * mipRes * 2 + zz * mipRes * 2 * mipRes * 2], out byte value1, out Vector3 normal1, out byte material1);
						decodeVoxelData(lastMip[xx + (yy + 1) * mipRes * 2 + zz * mipRes * 2 * mipRes * 2], out byte value2, out Vector3 normal2, out byte material2);
						decodeVoxelData(lastMip[xx + 1 + (yy + 1) * mipRes * 2 + zz * mipRes * 2 * mipRes * 2], out byte value3, out Vector3 normal3, out byte material3);
						decodeVoxelData(lastMip[xx + yy * mipRes * 2 + (zz + 1) * mipRes * 2 * mipRes * 2], out byte value4, out Vector3 normal4, out byte material4);
						decodeVoxelData(lastMip[xx + 1 + yy * mipRes * 2 + (zz + 1) * mipRes * 2 * mipRes * 2], out byte value5, out Vector3 normal5, out byte material5);
						decodeVoxelData(lastMip[xx + (yy + 1) * mipRes * 2 + (zz + 1) * mipRes * 2 * mipRes * 2], out byte value6, out Vector3 normal6, out byte material6);
						decodeVoxelData(lastMip[xx + 1 + (yy + 1) * mipRes * 2 + (zz + 1) * mipRes * 2 * mipRes * 2], out byte value7, out Vector3 normal7, out byte material7);

						bool isEmpty = value0 == 0 && value1 == 0 && value2 == 0 && value3 == 0 &&
							value4 == 0 && value5 == 0 && value6 == 0 && value7 == 0;
						bool isLeaf = value0 == 2 && value1 == 2 && value2 == 2 && value3 == 2 &&
							value4 == 2 && value5 == 2 && value6 == 2 && value7 == 2;
						byte value = (byte)(isEmpty ? 0 : isLeaf ? 2 : 1);

						Vector3 normal =
							Vector3.Lerp(
								Vector3.Lerp(
									Vector3.Lerp(normal0, normal1, 0.5f),
									Vector3.Lerp(normal2, normal3, 0.5f),
									0.5f),
								Vector3.Lerp(
									Vector3.Lerp(normal4, normal5, 0.5f),
									Vector3.Lerp(normal5, normal6, 0.5f),
									0.5f),
								0.5f).normalized;

						mipData[x + y * mipRes + z * mipRes * mipRes] = encodeVoxelData(value, normal, 0);
					}
				}
			}
			graphics.setTextureData(texture, mip, 0, 0, 0, mipRes, mipRes, mipRes, mipData);
			mipRes /= 2;
			lastMip = mipData;
			mips.Add(mipData);
		}
	}

	public void setVoxel(int x, int y, int z, Vector3 normal)
	{
		byte value = 2; // Leaf node
		byte material = 0;
		ushort voxel = encodeVoxelData(value, normal, material);
		voxelData[x + y * resolution + z * resolution * resolution] = voxel;
		hasChanged = true;
	}

	public void draw(GraphicsDevice graphics)
	{
		Renderer.DrawCube(position * CHUNK_SIZE, new Vector3(CHUNK_SIZE), texture, Vector3i.Zero, new Vector3i(texture.width, texture.height, texture.depth), 0);
	}
}
