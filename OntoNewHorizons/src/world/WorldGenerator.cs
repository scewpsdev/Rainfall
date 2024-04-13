using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;


internal class WorldGenerator
{
	static float SampleContinentalness(float x, float z, Simplex simplex)
	{
		float totalAmplitude = 0.0f;
		for (int i = 0; i < simplex.octaves; i++)
		{
			float amplitude = MathF.Pow(simplex.persistence, i);
			totalAmplitude += amplitude;
		}

		float scale = 0.004f;
		float noise = simplex.sample2f(x * scale, z * scale);
		noise /= totalAmplitude;

		noise = noise * 0.5f + 0.5f;
		noise = MathF.Pow(noise, 3.0f);

		//result = MathHelper.Remap(result, min, max, 0.0f, 1.0f);
		return noise;

		//{
		//	float islandSize = 64;
		//	float islandFalloff = 16;
		//	float xx = MathF.Abs(x - 0.5f * islandSize);
		//	float zz = MathF.Abs(z - 0.5f * islandSize);
		//	float aa = MathF.Max(xx, zz);
		//	float border = 0.5f * islandSize - islandFalloff;
		//	float falloff = aa > border ? MathF.Min(MathF.Pow((aa - border) / islandFalloff, 2.0f), 1.0f) : 0.0f;
		//	result -= falloff * 0.1f;
		//}
	}

	static float SampleErosion(float x, float z, Simplex simplex)
	{
		float scale = 0.001f;
		float lacunarity = 2.0f;
		float persistence = 0.5f;
		int octaves = 4;

		float result = 0.0f;
		float totalAmplitude = 0.0f;
		for (int i = 0; i < octaves; i++)
		{
			float amplitude = MathF.Pow(persistence, i);
			float frequency = MathF.Pow(lacunarity, i);

			float noise = simplex.sample2f(x * frequency * scale, z * frequency * scale) * amplitude;
			result += noise;

			totalAmplitude += amplitude;
		}

		result /= totalAmplitude;

		result = result * 0.5f + 0.5f;

		return result;
	}

	static float SampleHeightmap(float x, float z, Simplex simplex)
	{
		return SampleContinentalness(x, z, simplex) * 30.0f;



		float totalHeight = 0.0f;

		{
			float continentalness = SampleContinentalness(x, z, simplex);

			Span<Vector2> points = stackalloc Vector2[8];
			points[0] = new Vector2(0.0f, 200.0f);
			points[1] = new Vector2(0.1f, 0.0f);
			points[2] = new Vector2(0.35f, 5.0f);
			points[3] = new Vector2(0.375f, 50.0f);
			points[4] = new Vector2(0.5f, 55.0f);
			points[5] = new Vector2(0.625f, 100.0f);
			points[6] = new Vector2(0.7f, 120.0f);
			points[7] = new Vector2(1.0f, 125.0f);

			float height = 0.000069f;
			for (int i = 0; i < points.Length - 1; i++)
			{
				Vector2 currentPoint = points[i];
				Vector2 nextPoint = points[i + 1];

				if (continentalness >= currentPoint.x && continentalness < nextPoint.x)
				{
					float blend = MathHelper.Remap(continentalness, currentPoint.x, nextPoint.x, 0.0f, 1.0f);
					height = MathHelper.Lerp(currentPoint.y, nextPoint.y, blend);
					height *= 0.5f;
					break;
				}
			}
			if (height == 0.000069f)
				Debug.Assert(false);

			totalHeight += height;
		}

		{
			float erosion = SampleErosion(x, z, simplex);

			Span<Vector2> points = stackalloc Vector2[9];
			points[0] = new Vector2(0.0f, 100.0f);
			points[1] = new Vector2(0.15f, 75.0f);
			points[2] = new Vector2(0.3f, 50.0f);
			points[3] = new Vector2(0.35f, 55.0f);
			points[4] = new Vector2(0.45f, 10.0f);
			points[5] = new Vector2(0.7f, 8.0f);
			points[6] = new Vector2(0.75f, 30.0f);
			points[7] = new Vector2(0.8f, 8.0f);
			points[8] = new Vector2(1.0f, 3.0f);

			float height = 0.000069f;
			for (int i = 0; i < points.Length - 1; i++)
			{
				Vector2 currentPoint = points[i];
				Vector2 nextPoint = points[i + 1];

				if (erosion >= currentPoint.x && erosion < nextPoint.x)
				{
					float blend = MathHelper.Remap(erosion, currentPoint.x, nextPoint.x, 0.0f, 1.0f);
					height = MathHelper.Lerp(currentPoint.y, nextPoint.y, blend);
					height *= 0.5f;
					break;
				}
			}
			if (height == 0.000069f)
				Debug.Assert(false);

			totalHeight += height;
		}

		totalHeight /= 2;

		return totalHeight;
	}

	static void GenerateHeightmap(Chunk chunk, Simplex simplex)
	{
		for (int z = 0; z < Chunk.CHUNK_RESOLUTION + 1; z++)
		{
			for (int x = 0; x < Chunk.CHUNK_RESOLUTION + 1; x++)
			{
				float worldX = chunk.x * Chunk.CHUNK_SIZE + x / (float)Chunk.CHUNK_RESOLUTION * Chunk.CHUNK_SIZE;
				float worldZ = chunk.z * Chunk.CHUNK_SIZE + z / (float)Chunk.CHUNK_RESOLUTION * Chunk.CHUNK_SIZE;

				float height = SampleHeightmap(worldX, worldZ, simplex);
				if (float.IsNaN(height) || float.IsInfinity(height) || !float.IsRealNumber(height))
					Debug.Assert(false);

				float left = SampleHeightmap(worldX - 0.1f, worldZ, simplex);
				float right = SampleHeightmap(worldX + 0.1f, worldZ, simplex);
				float dx = (left - right) / 0.2f;

				float top = SampleHeightmap(worldX, worldZ - 0.1f, simplex);
				float bottom = SampleHeightmap(worldX, worldZ + 0.1f, simplex);
				float dz = (top - bottom) / 0.2f;

				Vector3 normal = new Vector3(dx, 1.0f, dz).normalized;

				unsafe
				{
					float* heights = (float*)chunk.terrain.heights.data;
					Vector4* normals = (Vector4*)chunk.terrain.normals.data;
					heights[x + z * (Chunk.CHUNK_RESOLUTION + 1)] = height;
					normals[x + z * (Chunk.CHUNK_RESOLUTION + 1)].xyz = normal;
				}
			}
		}

		for (int z = 0; z < Terrain.SPLATMAP_RES; z++)
		{
			for (int x = 0; x < Terrain.SPLATMAP_RES; x++)
			{
				float dirtFactor = 1.0f;
				float grassFactor = 0.0f;
				float rockFactor = 0.0f;

				float xx = (x / (float)Terrain.SPLATMAP_RES + 0.5f / Terrain.SPLATMAP_RES) * Chunk.CHUNK_SIZE;
				float zz = (z / (float)Terrain.SPLATMAP_RES + 0.5f / Terrain.SPLATMAP_RES) * Chunk.CHUNK_SIZE;

				// TODO use bilinear sampling instead of barycentric
				Vector3 normal = chunk.terrain.getNormal(xx, zz);
				float steepness = MathF.Acos(Vector3.Dot(normal, Vector3.Up)) / (MathF.PI * 0.5f);
				float rockBoundary = 0.5f;
				float dirtBoundary = 0.2f;
				if (steepness > rockBoundary)
				{
					rockFactor = MathF.Pow((steepness - rockBoundary) / (1.0f - rockBoundary), 0.5f);
					dirtFactor = 1.0f - rockFactor;
					grassFactor = 0.0f;
				}
				else if (steepness > dirtBoundary)
				{
					dirtFactor = MathF.Pow((steepness - dirtBoundary) / (rockBoundary - dirtBoundary), 0.5f);
					grassFactor = 1.0f - dirtFactor;
					rockFactor = 0.0f;
				}
				else if (steepness > 0.0f)
				{
					dirtFactor = 0.0f;
					grassFactor = 1.0f;
					rockFactor = 0.0f;
				}

				unsafe
				{
					uint* splat = (uint*)chunk.terrain.splat.data;
					splat[x + z * Terrain.SPLATMAP_RES] = 0xFF000000 | ((uint)(dirtFactor * 255) << 16) | ((uint)(grassFactor * 255) << 8) | ((uint)(rockFactor * 255) << 0);
				}
			}
		}

		chunk.terrain.updateHeightmap(Renderer.graphics);
	}

	static void GenerateTrees(Chunk chunk, Simplex simplex, Random random)
	{
		int treeDensity = 64;
		for (int i = 0; i < treeDensity; i++)
		{
			float localX = random.NextSingle() * Chunk.CHUNK_SIZE;
			float localZ = random.NextSingle() * Chunk.CHUNK_SIZE;
			float worldX = chunk.x * Chunk.CHUNK_SIZE + localX;
			float worldZ = chunk.z * Chunk.CHUNK_SIZE + localZ;
			float frequency = 0.01f;
			float treeNoise = simplex.sample2f(worldX * frequency, worldZ * frequency);
			if (treeNoise > 0.0f)
			{
				float height = chunk.terrain.getHeight(localX, localZ);
				chunk.addEntity(new Tree(), new Vector3(worldX, height, worldZ), Quaternion.FromAxisAngle(Vector3.Up, random.NextSingle() * MathF.PI * 2));
			}
		}
	}

	static Chunk GenerateChunk(int chunkX, int chunkZ, Simplex heightSimplex, Simplex treeSimplex, Random random)
	{
		Chunk chunk = new Chunk(chunkX, chunkZ);

		GenerateHeightmap(chunk, heightSimplex);
		GenerateTrees(chunk, treeSimplex, random);

		return chunk;
	}

	public static void Generate(World world, uint seed)
	{
		Simplex heightSimplex = new Simplex(seed, 5, 2.0f, 0.5f);
		Simplex treeSimplex = new Simplex(seed + 1000000);
		Random random = new Random((int)seed);

		for (int z = -2; z < 2; z++)
		{
			for (int x = -2; x < 2; x++)
			{
				world.chunks.Add(GenerateChunk(x, z, heightSimplex, treeSimplex, random));
			}
		}
	}
}
