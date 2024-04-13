using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;


internal class Terrain
{
	Model model;
	Model colliderMesh;
	RigidBody collider;

	public Texture splatMap;
	public Texture grassMap;

	int totalVertexCount;
	int heightmapWidth;
	float[] heights;
	float amplitude;
	float tileSize;

	Vector2 min, max;

	public Texture heightmap;
	public Texture normalmap;


	public Terrain(string name, string colliderName, string splatMapName, string grassMapName, GraphicsDevice graphics)
	{
		model = Resource.GetModel(name);
		colliderMesh = Resource.GetModel(colliderName);

		splatMap = Resource.GetTexture(splatMapName);
		grassMap = Resource.GetTexture(grassMapName);

		totalVertexCount = 0;
		for (int i = 0; i < colliderMesh.meshCount; i++)
			totalVertexCount += colliderMesh.getMeshData(i).Value.vertexCount;

		heightmapWidth = (int)(MathF.Sqrt(totalVertexCount) + 0.5f);
		heights = new float[heightmapWidth * heightmapWidth];
		uint[] normals = new uint[heightmapWidth * heightmapWidth];

		for (int k = 0; k < colliderMesh.meshCount; k++)
		{
			MeshData meshData = colliderMesh.getMeshData(0).Value;

			min = new Vector2(float.MaxValue, float.MaxValue);
			max = new Vector2(float.MinValue, float.MinValue);
			for (int i = 0; i < meshData.vertexCount; i++)
			{
				Vector3 vertex = meshData.getVertex(i);
				min = Vector2.Min(min, vertex.xz);
				max = Vector2.Max(max, vertex.xz);
			}
			float width = max.x - min.x;
			tileSize = width / (heightmapWidth - 1);


			for (int i = 0; i < meshData.vertexCount; i++)
			{
				Vector3 vertex = meshData.getVertex(i);
				int x = (int)((vertex.x - min.x) / width * (heightmapWidth - 1) + 0.5f);
				int z = (int)((vertex.z - min.y) / width * (heightmapWidth - 1) + 0.5f);
				heights[x + z * heightmapWidth] = vertex.y;
				amplitude = Math.Max(amplitude, MathF.Abs(vertex.y));
			}


			for (int i = 0; i < normals.Length; i++)
			{
				Vector3 vertex = meshData.getVertex(i);
				int x = (int)((vertex.x - min.x) / width * (heightmapWidth - 1) + 0.5f);
				int z = (int)((vertex.z - min.y) / width * (heightmapWidth - 1) + 0.5f);

				int left = Math.Max(x - 1, 0) + z * heightmapWidth;
				int right = Math.Min(x + 1, heightmapWidth - 1) + z * heightmapWidth;
				int up = x + Math.Max(z - 1, 0) * heightmapWidth;
				int down = x + Math.Min(z + 1, heightmapWidth - 1) * heightmapWidth;

				float h1 = heights[left];
				float h2 = heights[right];
				float h3 = heights[up];
				float h4 = heights[down];

				float heightDiffX = (h1 - h2) / (2.0f * tileSize);
				float heightDiffZ = (h3 - h4) / (2.0f * tileSize);

				Vector3 normal = new Vector3(heightDiffX, 1.0f, heightDiffZ).normalized;
				byte r = (byte)((normal.x * 0.5f + 0.5f) * 255);
				byte g = (byte)((normal.y * 0.5f + 0.5f) * 255);
				byte b = (byte)((normal.z * 0.5f + 0.5f) * 255);
				uint normalPacked = (uint)(r << 16) | (uint)(g << 8) | (uint)(b << 0);
				//if (MathF.Abs(heightDiffX) > 0.1f || MathF.Abs(heightDiffZ) > 0.1f)
				//	Console.WriteLine(normal);
				normals[i] = normalPacked;
			}
		}


		heightmap = graphics.createTexture(heightmapWidth, heightmapWidth, TextureFormat.R32F, graphics.createVideoMemory(heights), (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp);
		normalmap = graphics.createTexture(heightmapWidth, heightmapWidth, TextureFormat.BGRA8, graphics.createVideoMemory(normals), (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp);
	}

	public void init(Entity entity)
	{
		collider = new RigidBody(entity, RigidBodyType.Static);
		HeightFieldSample[] heightsColliderData = new HeightFieldSample[heights.Length];
		for (int i = 0; i < heights.Length; i++)
		{
			int x = i % heightmapWidth;
			int z = i / heightmapWidth;
			heightsColliderData[i].height = (short)(heights[z + x * heightmapWidth] / amplitude * short.MaxValue);
		}
		IntPtr heightField = Physics.CreateHeightField(heightmapWidth, heightmapWidth, heightsColliderData);
		collider.addHeightFieldCollider(heightField, new Vector3(tileSize, amplitude / short.MaxValue, tileSize), Matrix.CreateTranslation(min.x, 0.0f, min.y));
	}

	public void draw(GraphicsDevice graphics)
	{
		Renderer.DrawTerrain(model, Matrix.Identity, splatMap);
	}

	public float getHeight(float x, float z)
	{
		x -= min.x;
		z -= min.y;

		float width = max.x - min.x;
		float tileSize = width / (heightmapWidth - 1);

		int gridX = (int)MathF.Floor(x / tileSize - 0.01f);
		int gridZ = (int)MathF.Floor(z / tileSize - 0.01f);

		float gsX = x % tileSize;
		float gsZ = z % tileSize;

		if (gsX >= gsZ)
		{
			Vector3 p1 = new Vector3(gridX * tileSize, heights[gridX + gridZ * heightmapWidth], gridZ * tileSize);
			Vector3 p2 = new Vector3((gridX + 1) * tileSize, heights[(gridX + 1) + gridZ * heightmapWidth], gridZ * tileSize);
			Vector3 p3 = new Vector3((gridX + 1) * tileSize, heights[(gridX + 1) + (gridZ + 1) * heightmapWidth], (gridZ + 1) * tileSize);
			Vector2 pos = new Vector2(x, z);
			float interpolatedHeight = MathHelper.BarryCentric(p1, p2, p3, pos);
			return interpolatedHeight;
		}
		else
		{
			Vector3 p1 = new Vector3(gridX * tileSize, heights[gridX + gridZ * heightmapWidth], gridZ * tileSize);
			Vector3 p2 = new Vector3(gridX * tileSize, heights[gridX + (gridZ + 1) * heightmapWidth], (gridZ + 1) * tileSize);
			Vector3 p3 = new Vector3((gridX + 1) * tileSize, heights[(gridX + 1) + (gridZ + 1) * heightmapWidth], (gridZ + 1) * tileSize);
			Vector2 pos = new Vector2(x, z);
			float interpolatedHeight = MathHelper.BarryCentric(p1, p2, p3, pos);
			return interpolatedHeight;
		}
	}

	public Vector3 getNormal(float x, float z)
	{
		x -= min.x;
		z -= min.y;

		float width = max.x - min.x;
		float tileSize = width / (heightmapWidth - 1);

		int gridX = (int)MathF.Floor(x / tileSize - 0.0001f);
		int gridZ = (int)MathF.Floor(z / tileSize - 0.0001f);

		float gsX = x % tileSize;
		float gsZ = z % tileSize;

		if (gsX >= gsZ)
		{
			float h1 = heights[gridX + gridZ * heightmapWidth];
			float h2 = heights[(gridX + 1) + gridZ * heightmapWidth];
			float h3 = heights[(gridX + 1) + (gridZ + 1) * heightmapWidth];

			float heightDiffX = (h1 - h2) / tileSize;
			float heightDiffZ = (h2 - h3) / tileSize;

			Vector3 normal = new Vector3(heightDiffX, 1.0f, heightDiffZ);
			return normal.normalized;
		}
		else
		{
			float h1 = heights[gridX + gridZ * heightmapWidth];
			float h2 = heights[gridX + (gridZ + 1) * heightmapWidth];
			float h3 = heights[(gridX + 1) + (gridZ + 1) * heightmapWidth];

			float heightDiffZ = (h1 - h2) / tileSize;
			float heightDiffX = (h2 - h3) / tileSize;

			Vector3 normal = new Vector3(heightDiffX, 1.0f, heightDiffZ);
			return normal.normalized;
		}
	}

	public Vector2 position
	{
		get => min;
	}

	public float size
	{
		get => max.x - min.x;
	}
}
