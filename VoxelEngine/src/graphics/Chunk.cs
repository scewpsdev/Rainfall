using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Chunk
{
	public readonly int resolution;
	public Vector3i position;

	Texture texture;
	Vector4[] voxelData;
	List<Vector4[]> mips = new List<Vector4[]>();

	bool hasChanged = true;


	public Chunk(int resolution, GraphicsDevice graphics)
	{
		this.resolution = resolution;
		texture = graphics.createTexture(resolution, resolution, resolution, true, TextureFormat.RGBA32F, (uint)SamplerFlags.Clamp | (uint)SamplerFlags.Point);
		voxelData = new Vector4[resolution * resolution * resolution];
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

	void generateMips(GraphicsDevice graphics)
	{
		int maxMip = Math.ILogB(resolution);

		int mipRes = resolution / 2;
		Vector4[] lastMip = voxelData;
		for (int mip = 1; mip <= maxMip; mip++)
		{
			Vector4[] mipData = new Vector4[mipRes * mipRes * mipRes];
			for (int z = 0; z < mipRes; z++)
			{
				for (int y = 0; y < mipRes; y++)
				{
					for (int x = 0; x < mipRes; x++)
					{
						int xx = x * 2;
						int yy = y * 2;
						int zz = z * 2;

						Vector4 value0 = lastMip[xx + yy * mipRes * 2 + zz * mipRes * 2 * mipRes * 2];
						Vector4 value1 = lastMip[xx + 1 + yy * mipRes * 2 + zz * mipRes * 2 * mipRes * 2];
						Vector4 value2 = lastMip[xx + (yy + 1) * mipRes * 2 + zz * mipRes * 2 * mipRes * 2];
						Vector4 value3 = lastMip[xx + 1 + (yy + 1) * mipRes * 2 + zz * mipRes * 2 * mipRes * 2];
						Vector4 value4 = lastMip[xx + yy * mipRes * 2 + (zz + 1) * mipRes * 2 * mipRes * 2];
						Vector4 value5 = lastMip[xx + 1 + yy * mipRes * 2 + (zz + 1) * mipRes * 2 * mipRes * 2];
						Vector4 value6 = lastMip[xx + (yy + 1) * mipRes * 2 + (zz + 1) * mipRes * 2 * mipRes * 2];
						Vector4 value7 = lastMip[xx + 1 + (yy + 1) * mipRes * 2 + (zz + 1) * mipRes * 2 * mipRes * 2];

						float w = (value0.w > 0.5 || value1.w > 0.5 || value2.w > 0.5 || value3.w > 0.5 ||
							value4.w > 0.5 || value5.w > 0.5 || value6.w > 0.5 || value7.w > 0.5) ? 1 : 0;
						Vector3 xyz =
							Vector3.Lerp(
								Vector3.Lerp(
									Vector3.Lerp(value0.xyz, value1.xyz, 0.5f),
									Vector3.Lerp(value2.xyz, value3.xyz, 0.5f),
									0.5f),
								Vector3.Lerp(
									Vector3.Lerp(value4.xyz, value5.xyz, 0.5f),
									Vector3.Lerp(value5.xyz, value6.xyz, 0.5f),
									0.5f),
								0.5f);
						xyz = xyz.normalized;

						mipData[x + y * mipRes + z * mipRes * mipRes] = new Vector4(xyz, w);
					}
				}
			}
			graphics.setTextureData(texture, mip, 0, 0, 0, mipRes, mipRes, mipRes, mipData);
			mipRes /= 2;
			lastMip = mipData;
			mips.Add(mipData);
		}
	}

	public void setVoxel(int x, int y, int z, Vector4 data)
	{
		voxelData[x + y * resolution + z * resolution * resolution] = data;
		hasChanged = true;
	}

	public void draw(GraphicsDevice graphics)
	{
		Renderer.DrawCube((Vector3)position, new Vector3(1), texture, Vector3i.Zero, new Vector3i(texture.width, texture.height, texture.depth), 0);
	}
}
