using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ProceduralBiomeGenerator : BiomeGenerator
{
	Simplex simplex2;


	public override void setup(Level level, byte[] biomes, int biome, float[] lootValues, string seed)
	{
		base.setup(level, biomes, biome, lootValues, seed);

		simplex = new Simplex(Hash.hash(seed), 3);
		simplex2 = new Simplex(Hash.hash(Hash.hash(seed)), 3);
	}

	public override TileType getBackgroundTile(int x, int y)
	{
		float progress = 1 - y / (float)level.height;
		float type = simplex.sample2f(x * 0.05f, y * 0.05f) - progress * 0.4f;
		return type > -0.1f ? TileType.rock : TileType.stone;
	}

	public override void generateBaseLevel()
	{
		for (int y = 0; y < level.height; y++)
		{
			for (int x = 0; x < level.width; x++)
			{
				if (getMask(x, y))
				{
					float type = simplex2.sample2f(x * 0.05f, y * 0.05f);
					if (MathF.Abs(type) < 0.3f)
						level.setTile(x, y, null);
				}
			}
		}

		for (int y = 0; y < level.height; y++)
		{
			for (int x = 0; x < level.width; x++)
			{
				if (getMask(x, y))
				{
					if (level.getTile(x + 1, y) == null && level.getTile(x, y + 1) == null)
						level.setTile(x, y, null);
				}
			}
		}
		for (int y = 0; y < level.height; y++)
		{
			for (int x = level.width - 1; x >= 0; x--)
			{
				if (getMask(x, y))
				{
					if (level.getTile(x - 1, y) == null && level.getTile(x, y + 1) == null)
						level.setTile(x, y, null);
				}
			}
		}
	}

	public override void spawnNPC(Vector2i tile)
	{
		throw new NotImplementedException();
	}

	public override Container createContainer(Item[] items)
	{
		throw new NotImplementedException();
	}

	public override ExplosiveObject createExplosiveObject()
	{
		throw new NotImplementedException();
	}
}
