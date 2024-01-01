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
	byte[] octreeData;
	Texture normals;
	byte[] normalData;

	Shader mipmapShader;

	bool hasChanged = true;


	public Chunk(int resolution, GraphicsDevice graphics)
	{
		this.resolution = resolution;

		octree = graphics.createTexture(resolution, resolution, resolution, true, TextureFormat.R8, (ulong)TextureFlags.ComputeWrite | (uint)SamplerFlags.Clamp | (uint)SamplerFlags.Point);
		octreeData = new byte[resolution * resolution * resolution];

		normals = graphics.createTexture(resolution, resolution, resolution, false, TextureFormat.RGBA8, (uint)SamplerFlags.Clamp | (uint)SamplerFlags.Point);
		normalData = new byte[resolution * resolution * resolution * 4];

		mipmapShader = Resource.GetShader("res/shaders/chunk/mipmap.cs.shader");
	}

	public void setVoxel(int x, int y, int z, bool value)
	{
		octreeData[x + y * resolution + z * resolution * resolution] = (byte)(value ? 2 : 0); // Leaf node

		hasChanged = true;
	}

	public void setNormal(int x, int y, int z, Vector3 normal)
	{
		normalData[(x + y * resolution + z * resolution * resolution) * 4 + 0] = (byte)((normal.x * 0.5 + 0.5) * 255);
		normalData[(x + y * resolution + z * resolution * resolution) * 4 + 1] = (byte)((normal.y * 0.5 + 0.5) * 255);
		normalData[(x + y * resolution + z * resolution * resolution) * 4 + 2] = (byte)((normal.z * 0.5 + 0.5) * 255);

		hasChanged = true;
	}

	public bool update(GraphicsDevice graphics)
	{
		if (hasChanged)
		{
			graphics.setTextureData(octree, 0, 0, 0, 0, resolution, resolution, resolution, octreeData);
			generateMips(graphics);

			graphics.setTextureData(normals, 0, 0, 0, 0, resolution, resolution, resolution, normalData);

			hasChanged = false;
			return true;
		}
		return false;
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
			graphics.computeDispatch(mip - 1, mipmapShader, numBatches, numBatches, numBatches);

			mipRes /= 2;
		}

		graphics.completeFrame();
	}

	public void draw(GraphicsDevice graphics)
	{
		Renderer.DrawChunk(position * CHUNK_SIZE, new Vector3(CHUNK_SIZE), octree, normals);
	}
}
