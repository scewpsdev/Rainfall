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

	Texture octree;
	unsafe byte* octreeData;

	Shader mipmapShader;

	bool hasChanged = true;


	public Chunk(int resolution, GraphicsDevice graphics)
	{
		this.resolution = resolution;

		unsafe
		{
			octree = graphics.createTexture(resolution, resolution, resolution, true, TextureFormat.R8, (ulong)TextureFlags.ComputeWrite | (uint)SamplerFlags.Clamp | (uint)SamplerFlags.Point);
			octreeData = (byte*)graphics.allocNativeMemory(resolution * resolution * resolution);

			mipmapShader = Resource.GetShader("res/shaders/chunk/mipmap.cs.shader");
		}
	}

	public void destroy(GraphicsDevice graphics)
	{
		unsafe
		{
			graphics.destroyTexture(octree);
			graphics.freeNativeMemory(octreeData);
			graphics.destroyShader(mipmapShader);
		}
	}

	public void setVoxel(int x, int y, int z, bool value)
	{
		unsafe
		{
			octreeData[x + y * resolution + z * resolution * resolution] = (byte)(value ? 2 : 0); // Leaf node

			hasChanged = true;
		}
	}

	public bool update(GraphicsDevice graphics)
	{
		unsafe
		{
			if (hasChanged)
			{
				graphics.setTextureData(octree, 0, 0, 0, 0, resolution, resolution, resolution, graphics.createVideoMemoryReference(octreeData, resolution * resolution * resolution));
				generateMips(graphics);

				hasChanged = false;
				return true;
			}
			return false;
		}
	}

	void generateMips(GraphicsDevice graphics)
	{
		int maxMip = Math.ILogB(resolution);
		int mipRes = resolution / 2;
		for (int mip = 1; mip <= maxMip; mip++)
		{
			graphics.setComputeTexture(0, octree, mip - 1, ComputeAccess.Read);
			graphics.setComputeTexture(1, octree, mip, ComputeAccess.Write);

			int numBatches = (mipRes + 8 - 1) / 8;
			graphics.setPass(mip - 1);
			graphics.computeDispatch(mipmapShader, numBatches, numBatches, numBatches);

			mipRes /= 2;
		}

		graphics.completeFrame();
	}

	public void draw(Vector3i pos)
	{
		Renderer.DrawChunk(pos * CHUNK_SIZE, new Vector3(CHUNK_SIZE), octree);
	}
}
