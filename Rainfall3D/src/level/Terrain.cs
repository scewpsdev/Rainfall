using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;


public class Terrain
{
	public const int SPLATMAP_RES = 128;


	public readonly Vector3 position;
	public readonly Matrix transform;

	int resolution;
	public readonly float size;
	float precision;

	int heightmapWidth;
	int totalVertexCount;

	public VideoMemory heights;
	public VideoMemory normals;
	public VideoMemory splat;

	float tileSize;

	IntPtr physicsHeightField;
	RigidBody collider;

	public Texture heightmap;
	public Texture normalmap;
	public Texture splatMap;
	public Texture grassMap;

	Texture diffuse0, diffuse1, diffuse2, diffuse3;


	public Terrain(Vector3 position, int resolution, float size, float precision, GraphicsDevice graphics)
	{
		this.position = position;
		this.resolution = resolution;
		this.precision = precision;
		this.size = size;

		heightmapWidth = resolution + 1;
		totalVertexCount = heightmapWidth * heightmapWidth;
		tileSize = size / resolution;

		unsafe
		{
			heights = graphics.createVideoMemory(heightmapWidth * heightmapWidth * sizeof(float));
			normals = graphics.createVideoMemory(heightmapWidth * heightmapWidth * sizeof(Vector4));
			splat = graphics.createVideoMemory(SPLATMAP_RES * SPLATMAP_RES * sizeof(uint));
		}


		heightmap = graphics.createTexture(heightmapWidth, heightmapWidth, TextureFormat.R32F, (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp);
		normalmap = graphics.createTexture(heightmapWidth, heightmapWidth, TextureFormat.RGBA32F, (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp);
		splatMap = graphics.createTexture(SPLATMAP_RES, SPLATMAP_RES, TextureFormat.BGRA8, (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp);

		transform = Matrix.CreateTranslation(position);


		diffuse0 = Resource.GetTexture("res/texture/material0_diffuse.png");
		diffuse1 = Resource.GetTexture("res/texture/material1_diffuse.png");
		diffuse2 = Resource.GetTexture("res/texture/material2_diffuse.png");
		diffuse3 = Resource.GetTexture("res/texture/material3_diffuse.png");
	}

	float getHeightSample(int i)
	{
		unsafe
		{
			return ((float*)heights.data)[i];
		}
	}

	public void init()
	{
		collider = new RigidBody(null, RigidBodyType.Static);

		unsafe
		{
			HeightFieldSample[] heightsColliderData = new HeightFieldSample[heightmapWidth * heightmapWidth];
			for (int i = 0; i < heightmapWidth * heightmapWidth; i++)
			{
				int x = i % heightmapWidth;
				int z = i / heightmapWidth;
				heightsColliderData[i].height = (short)(getHeightSample(z + x * heightmapWidth) / precision);
				heightsColliderData[i].tesselationBit = true;
			}
			physicsHeightField = Physics.CreateHeightField(heightmapWidth, heightmapWidth, heightsColliderData);
			collider.addHeightFieldCollider(physicsHeightField, new Vector3(tileSize, precision, tileSize), transform);
		}
	}

	public void updateHeightmap(GraphicsDevice graphics)
	{
		graphics.setTextureData(heightmap, 0, 0, heightmapWidth, heightmapWidth, heights);
		graphics.setTextureData(normalmap, 0, 0, heightmapWidth, heightmapWidth, normals);
		graphics.setTextureData(splatMap, 0, 0, SPLATMAP_RES, SPLATMAP_RES, splat);

		Physics.DestroyHeightField(physicsHeightField);
		collider.clearColliders();

		HeightFieldSample[] heightsColliderData = new HeightFieldSample[heightmapWidth * heightmapWidth];
		for (int i = 0; i < heightmapWidth * heightmapWidth; i++)
		{
			int x = i % heightmapWidth;
			int z = i / heightmapWidth;
			heightsColliderData[i].height = (short)(getHeightSample(z + x * heightmapWidth) / precision);
			heightsColliderData[i].tesselationBit = true;
		}
		physicsHeightField = Physics.CreateHeightField(heightmapWidth, heightmapWidth, heightsColliderData);
		collider.addHeightFieldCollider(physicsHeightField, new Vector3(tileSize, precision, tileSize), transform);
	}

	public void draw(GraphicsDevice graphics)
	{
		Renderer.DrawTerrain(heightmap, normalmap, splatMap, transform, diffuse0, diffuse1, diffuse2, diffuse3);
		//Renderer.DrawGrassPatch(GrassPatch.grassData, this, position.xz);
	}

	public float getHeight(float localX, float localZ)
	{
		int gridX = (int)MathF.Floor(localX / tileSize - 0.01f);
		int gridZ = (int)MathF.Floor(localZ / tileSize - 0.01f);

		float gsX = localX % tileSize;
		float gsZ = localZ % tileSize;

		if (gsX >= gsZ)
		{
			Vector3 p1 = new Vector3(gridX * tileSize, getHeightSample(gridX + gridZ * heightmapWidth), gridZ * tileSize);
			Vector3 p2 = new Vector3((gridX + 1) * tileSize, getHeightSample((gridX + 1) + gridZ * heightmapWidth), gridZ * tileSize);
			Vector3 p3 = new Vector3((gridX + 1) * tileSize, getHeightSample((gridX + 1) + (gridZ + 1) * heightmapWidth), (gridZ + 1) * tileSize);
			Vector2 pos = new Vector2(localX, localZ);
			float interpolatedHeight = MathHelper.BarryCentric(p1, p2, p3, pos);
			return interpolatedHeight;
		}
		else
		{
			Vector3 p1 = new Vector3(gridX * tileSize, getHeightSample(gridX + gridZ * heightmapWidth), gridZ * tileSize);
			Vector3 p2 = new Vector3(gridX * tileSize, getHeightSample(gridX + (gridZ + 1) * heightmapWidth), (gridZ + 1) * tileSize);
			Vector3 p3 = new Vector3((gridX + 1) * tileSize, getHeightSample((gridX + 1) + (gridZ + 1) * heightmapWidth), (gridZ + 1) * tileSize);
			Vector2 pos = new Vector2(localX, localZ);
			float interpolatedHeight = MathHelper.BarryCentric(p1, p2, p3, pos);
			return interpolatedHeight;
		}
	}

	public Vector3 getNormal(float localX, float localZ)
	{
		int gridX = (int)MathF.Floor(localX / tileSize - 0.0001f);
		int gridZ = (int)MathF.Floor(localZ / tileSize - 0.0001f);

		float gsX = localX % tileSize;
		float gsZ = localZ % tileSize;

		if (gsX >= gsZ)
		{
			float h1 = getHeightSample(gridX + gridZ * heightmapWidth);
			float h2 = getHeightSample((gridX + 1) + gridZ * heightmapWidth);
			float h3 = getHeightSample((gridX + 1) + (gridZ + 1) * heightmapWidth);

			float heightDiffX = (h1 - h2) / tileSize;
			float heightDiffZ = (h2 - h3) / tileSize;

			Vector3 normal = new Vector3(heightDiffX, 1.0f, heightDiffZ);
			return normal.normalized;
		}
		else
		{
			float h1 = getHeightSample(gridX + gridZ * heightmapWidth);
			float h2 = getHeightSample(gridX + (gridZ + 1) * heightmapWidth);
			float h3 = getHeightSample((gridX + 1) + (gridZ + 1) * heightmapWidth);

			float heightDiffZ = (h1 - h2) / tileSize;
			float heightDiffX = (h2 - h3) / tileSize;

			Vector3 normal = new Vector3(heightDiffX, 1.0f, heightDiffZ);
			return normal.normalized;
		}
	}
}
