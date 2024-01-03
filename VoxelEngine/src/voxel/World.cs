using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class World
{
	const int BRICKGRID_RES = 256;


	Texture brickgrid;
	unsafe uint* brickgridData;

	Texture brickgridLod;
	unsafe byte* brickgridLodData1;

	Texture brickgridLod2;
	unsafe byte* brickgridLodData2;

	Texture brickgridLod3;
	unsafe byte* brickgridLodData3;


	public World(GraphicsDevice graphics)
	{
		unsafe
		{
			brickgrid = graphics.createTexture(BRICKGRID_RES, BRICKGRID_RES, BRICKGRID_RES, false, TextureFormat.RGBA8U, (uint)SamplerFlags.Clamp | (uint)SamplerFlags.Point);
			brickgridData = (uint*)graphics.allocNativeMemory(BRICKGRID_RES * BRICKGRID_RES * BRICKGRID_RES * sizeof(uint));

			brickgridLod = graphics.createTexture(BRICKGRID_RES / 4, BRICKGRID_RES / 4, BRICKGRID_RES / 4, false, TextureFormat.R8U, (uint)SamplerFlags.Clamp | (uint)SamplerFlags.Point);
			brickgridLodData1 = (byte*)graphics.allocNativeMemory(BRICKGRID_RES * BRICKGRID_RES * BRICKGRID_RES / 4 / 4 / 4 * sizeof(byte));

			brickgridLod2 = graphics.createTexture(BRICKGRID_RES / 16, BRICKGRID_RES / 16, BRICKGRID_RES / 16, false, TextureFormat.R8U, (uint)SamplerFlags.Clamp | (uint)SamplerFlags.Point);
			brickgridLodData2 = (byte*)graphics.allocNativeMemory(BRICKGRID_RES * BRICKGRID_RES * BRICKGRID_RES / 16 / 16 / 16 * sizeof(byte));

			brickgridLod3 = graphics.createTexture(BRICKGRID_RES / 64, BRICKGRID_RES / 64, BRICKGRID_RES / 64, false, TextureFormat.R8U, (uint)SamplerFlags.Clamp | (uint)SamplerFlags.Point);
			brickgridLodData3 = (byte*)graphics.allocNativeMemory(BRICKGRID_RES * BRICKGRID_RES * BRICKGRID_RES / 64 / 64 / 64 * sizeof(byte));

			generateBrickgrid(graphics);
		}
	}

	void generateBrickgrid(GraphicsDevice graphics)
	{
		unsafe
		{
			Simplex simplex = new Simplex();
			for (int z = 0; z < BRICKGRID_RES; z++)
			{
				for (int x = 0; x < BRICKGRID_RES; x++)
				{
					float scale = 0.02f;
					int height = (int)(20 + 5 * simplex.sample2f(x * scale, z * scale));
					for (int y = 0; y < BRICKGRID_RES; y++)
					{
						uint value = 0;
						if (y < height)
						{
							byte r = 100;
							byte g = 100;
							byte b = 255;
							byte flags = 0xFF;
							value = (uint)((flags << 24) | (b << 16) | (g << 8) | r);
						}
						brickgridData[x + y * BRICKGRID_RES + z * BRICKGRID_RES * BRICKGRID_RES] = value;
					}
				}
			}
			graphics.setTextureData(brickgrid, 0, 0, 0, 0, BRICKGRID_RES, BRICKGRID_RES, BRICKGRID_RES, graphics.createVideoMemory(brickgridData, BRICKGRID_RES * BRICKGRID_RES * BRICKGRID_RES * sizeof(uint)));

			updateBrickgridLods(graphics);
		}
	}

	void updateBrickgridLods(GraphicsDevice graphics)
	{
		unsafe
		{
			for (int z = 0; z < BRICKGRID_RES / 4; z++)
			{
				for (int y = 0; y < BRICKGRID_RES / 4; y++)
				{
					for (int x = 0; x < BRICKGRID_RES / 4; x++)
					{
						bool hasValidBrickmapPointer = false;
						for (int zz = 0; zz < 4; zz++)
						{
							for (int yy = 0; yy < 4; yy++)
							{
								for (int xx = 0; xx < 4; xx++)
								{
									int idx = (x * 4 + xx) + (y * 4 + yy) * BRICKGRID_RES + (z * 4 + zz) * BRICKGRID_RES * BRICKGRID_RES;
									byte flags = (byte)((brickgridData[idx] & 0xFF000000) >> 24);
									if (flags != 0)
										hasValidBrickmapPointer = true;
								}
							}
						}
						byte value = (byte)(hasValidBrickmapPointer ? 1 : 0);
						brickgridLodData1[x + y * BRICKGRID_RES / 4 + z * BRICKGRID_RES / 4 * BRICKGRID_RES / 4] = value;
					}
				}
			}
			graphics.setTextureData(brickgridLod, 0, 0, 0, 0, BRICKGRID_RES / 4, BRICKGRID_RES / 4, BRICKGRID_RES / 4, graphics.createVideoMemory(brickgridLodData1, BRICKGRID_RES * BRICKGRID_RES * BRICKGRID_RES / 4 / 4 / 4));

			for (int z = 0; z < BRICKGRID_RES / 16; z++)
			{
				for (int y = 0; y < BRICKGRID_RES / 16; y++)
				{
					for (int x = 0; x < BRICKGRID_RES / 16; x++)
					{
						bool hasValidBrickmapPointer = false;
						for (int zz = 0; zz < 4; zz++)
						{
							for (int yy = 0; yy < 4; yy++)
							{
								for (int xx = 0; xx < 4; xx++)
								{
									int idx = (x * 4 + xx) + (y * 4 + yy) * BRICKGRID_RES / 4 + (z * 4 + zz) * BRICKGRID_RES / 4 * BRICKGRID_RES / 4;
									byte v = brickgridLodData1[idx];
									if (v != 0)
										hasValidBrickmapPointer = true;
								}
							}
						}
						byte value = (byte)(hasValidBrickmapPointer ? 1 : 0);
						brickgridLodData2[x + y * BRICKGRID_RES / 16 + z * BRICKGRID_RES / 16 * BRICKGRID_RES / 16] = value;
					}
				}
			}
			graphics.setTextureData(brickgridLod2, 0, 0, 0, 0, BRICKGRID_RES / 16, BRICKGRID_RES / 16, BRICKGRID_RES / 16, graphics.createVideoMemory(brickgridLodData2, BRICKGRID_RES * BRICKGRID_RES * BRICKGRID_RES / 16 / 16 / 16));

			for (int z = 0; z < BRICKGRID_RES / 64; z++)
			{
				for (int y = 0; y < BRICKGRID_RES / 64; y++)
				{
					for (int x = 0; x < BRICKGRID_RES / 64; x++)
					{
						bool hasValidBrickmapPointer = false;
						for (int zz = 0; zz < 4; zz++)
						{
							for (int yy = 0; yy < 4; yy++)
							{
								for (int xx = 0; xx < 4; xx++)
								{
									int idx = (x * 4 + xx) + (y * 4 + yy) * BRICKGRID_RES / 4 + (z * 4 + zz) * BRICKGRID_RES / 16 * BRICKGRID_RES / 16;
									byte v = brickgridLodData2[idx];
									if (v != 0)
										hasValidBrickmapPointer = true;
								}
							}
						}
						byte value = (byte)(hasValidBrickmapPointer ? 1 : 0);
						brickgridLodData3[x + y * BRICKGRID_RES / 64 + z * BRICKGRID_RES / 64 * BRICKGRID_RES / 64] = value;
					}
				}
			}
			graphics.setTextureData(brickgridLod3, 0, 0, 0, 0, BRICKGRID_RES / 64, BRICKGRID_RES / 64, BRICKGRID_RES / 64, graphics.createVideoMemory(brickgridLodData3, BRICKGRID_RES * BRICKGRID_RES * BRICKGRID_RES / 64 / 64 / 64));
		}
	}

	public void destroy(GraphicsDevice graphics)
	{
		unsafe
		{
			graphics.destroyTexture(brickgrid);
			graphics.destroyTexture(brickgridLod);
			graphics.destroyTexture(brickgridLod2);
			graphics.destroyTexture(brickgridLod3);
			graphics.freeNativeMemory(brickgridData);
			graphics.freeNativeMemory(brickgridLodData1);
			graphics.freeNativeMemory(brickgridLodData2);
			graphics.freeNativeMemory(brickgridLodData3);
		}
	}

	public void update()
	{
	}

	public void draw(GraphicsDevice graphics)
	{
		Renderer.DrawVoxels(brickgrid, brickgridLod, brickgridLod2, brickgridLod3, Vector3.Zero);
	}
}
