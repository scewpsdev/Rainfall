using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public struct GrassBladeData
{
	public Vector4 positionRotation;
}

internal class GrassPatch
{
	public const int NUM_GRASS_BLADES = 4096 * 16;
	public const float TILE_SIZE = 20.0f;

	public static readonly GrassBladeData[] grassData;


	Vector2 position;
	Terrain terrain;


	static GrassPatch()
	{
		grassData = new GrassBladeData[NUM_GRASS_BLADES];

		for (int i = 0; i < NUM_GRASS_BLADES; i++)
		{
			//int xx = i % 256 % 16 * 4 + i / 256 % 4 % 2 * 2 + i / 1024 % 2;
			//int zz = i % 256 / 16 * 4 + i / 256 % 4 / 2 * 2 + i / 1024 / 2;
			int xx = i % 256;
			int zz = i / 256;
			float x = xx / 256.0f * TILE_SIZE + MathHelper.RandomFloat(-0.2f, 0.2f);
			float z = zz / 256.0f * TILE_SIZE + MathHelper.RandomFloat(-0.2f, 0.2f);
			float rotation = MathHelper.RandomFloat(0.0f, MathF.PI * 2.0f);

			grassData[i].positionRotation = new Vector4(x, 0.0f, z, rotation);
		}
	}

	public GrassPatch(Terrain terrain, Vector2 position)
	{
		this.terrain = terrain;
		this.position = position;
	}

	public void draw(GraphicsDevice graphics)
	{
		Renderer.DrawGrassPatch(grassData, terrain, position);
	}
}
