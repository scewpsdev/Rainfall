﻿using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TileType
{
	public int id;

	public string name = "???";

	public bool visible = true;
	public bool isSolid = true;
	public bool isPlatform = false;
	public float platformHeight = 1.0f;
	public bool isLiquid = false;
	public int health = 0;
	public bool breaksArrows = false;

	public uint color = 0xFFFF00FF;
	public uint particleColor = 0xFFFFFFFF;
	public Sprite[] sprites = null;
	public Sprite[] left, right, top, bottom;


	static List<TileType> tileTypes = new List<TileType>();
	static Dictionary<string, int> nameMap = new Dictionary<string, int>();

	public static SpriteSheet tileset;

	public static TileType dummy;
	public static TileType dirt;
	public static TileType platform;
	public static TileType dummyPlatform;
	public static TileType stone;
	public static TileType sand;
	public static TileType path;
	public static TileType grass;
	public static TileType water;
	public static TileType wood;
	public static TileType bricks;
	public static TileType rock;
	public static TileType leaf;
	public static TileType tree;


	static TileType()
	{
		tileset = new SpriteSheet(Resource.GetTexture("sprites/tiles/tiles.png", false), 16, 16);

		AddTileType(dummy = new TileType() { name = "dummy", visible = false }); // dummy collider
		AddTileType(dirt = new TileType()
		{
			name = "dirt",
			color = 0xFFFFFFFF,
			particleColor = 0xFF5c4637,
			health = 1,
			sprites = [new Sprite(tileset, 4, 1), new Sprite(tileset, 9, 1), new Sprite(tileset, 10, 1), new Sprite(tileset, 11, 1), new Sprite(tileset, 12, 1)],
			left = [new Sprite(tileset, 3, 1)],
			right = [new Sprite(tileset, 5, 1)],
			top = [new Sprite(tileset, 4, 0), new Sprite(tileset, 9, 0), new Sprite(tileset, 10, 0), new Sprite(tileset, 11, 0), new Sprite(tileset, 12, 0)],
			bottom = [new Sprite(tileset, 4, 2), new Sprite(tileset, 9, 2), new Sprite(tileset, 10, 2), new Sprite(tileset, 11, 2), new Sprite(tileset, 12, 2)],
		});
		AddTileType(platform = new TileType() { name = "platform", color = 0xFF4488AA, particleColor = 0xFF2e2121, isPlatform = true, isSolid = false, sprites = [new Sprite(tileset, 1, 19)], left = [new Sprite(tileset, 0, 19)], right = [new Sprite(tileset, 2, 19)] });
		AddTileType(dummyPlatform = new TileType() { name = "dummy_platform", isPlatform = true, platformHeight = 0.75f, isSolid = false, visible = false });
		AddTileType(stone = new TileType()
		{
			name = "stone",
			breaksArrows = true,
			color = 0xFF333333,
			particleColor = 0xFF50504c,
			health = 2,
			sprites = [new Sprite(tileset, 4, 4), new Sprite(tileset, 9, 4), new Sprite(tileset, 10, 4), new Sprite(tileset, 11, 4), new Sprite(tileset, 12, 4)],
			left = [new Sprite(tileset, 3, 4), new Sprite(tileset, 13, 3), new Sprite(tileset, 13, 4), new Sprite(tileset, 13, 5)],
			right = [new Sprite(tileset, 5, 4), new Sprite(tileset, 15, 3), new Sprite(tileset, 15, 4), new Sprite(tileset, 15, 5)],
			top = [new Sprite(tileset, 4, 3), new Sprite(tileset, 9, 3), new Sprite(tileset, 10, 3), new Sprite(tileset, 11, 3), new Sprite(tileset, 12, 3)],
			bottom = [new Sprite(tileset, 4, 5), new Sprite(tileset, 9, 5), new Sprite(tileset, 10, 5), new Sprite(tileset, 11, 5), new Sprite(tileset, 12, 5)],
		});
		AddTileType(sand = new TileType()
		{
			name = "sand",
			color = 0xFF333333,
			particleColor = 0xFFb0a885,
			sprites = [new Sprite(tileset, 4, 16)],
			left = [new Sprite(tileset, 3, 16)],
			right = [new Sprite(tileset, 5, 16)],
			top = [new Sprite(tileset, 4, 15), new Sprite(tileset, 3, 17), new Sprite(tileset, 4, 17), new Sprite(tileset, 5, 17), new Sprite(tileset, 6, 17)],
			//bottom = [new Sprite(tileset, 4, 17)],
		});
		AddTileType(path = new TileType()
		{
			name = "stone_path",
			color = 0xFF333333,
			particleColor = 0xFF838dad,
			health = 5,
			sprites = [new Sprite(tileset, 4, 7)],
			left = [new Sprite(tileset, 3, 7)],
			right = [new Sprite(tileset, 5, 7)],
			top = [new Sprite(tileset, 4, 6)],
			bottom = [new Sprite(tileset, 4, 8)],
		});
		AddTileType(grass = new TileType()
		{
			name = "grass",
			color = 0xFF333333,
			particleColor = 0xFF678343,
			health = 1,
			sprites = [new Sprite(tileset, 4, 10)],
			left = [new Sprite(tileset, 3, 10)],
			right = [new Sprite(tileset, 5, 10)],
			top = [new Sprite(tileset, 4, 9)],
			bottom = [new Sprite(tileset, 4, 11)],
		});
		AddTileType(water = new TileType()
		{
			name = "water",
			color = 0xFF007FFF,
			isSolid = false,
			isLiquid = true,
			breaksArrows = true,
			particleColor = 0xFF007FFF,
			sprites = [new Sprite(tileset, 4, 13)]
		});
		AddTileType(wood = new TileType()
		{
			name = "wood",
			color = 0xFF8a6d61,
			isSolid = true,
			health = 1,
			particleColor = 0xFF8a6d61,
			sprites = [new Sprite(tileset, 4, 19)]
		});
		AddTileType(bricks = new TileType()
		{
			name = "bricks",
			color = 0xFF3a3a3a,
			isSolid = true,
			breaksArrows = true,
			health = 3,
			particleColor = 0xFF3a3a3a,
			sprites = [
				new Sprite(tileset, 4, 22),
				new Sprite(tileset, 6, 22),
				new Sprite(tileset, 7, 22),
				new Sprite(tileset, 8, 22),
				new Sprite(tileset, 9, 22),
				new Sprite(tileset, 10, 22),
				new Sprite(tileset, 11, 22),
				new Sprite(tileset, 12, 22),
				new Sprite(tileset, 13, 22),
				new Sprite(tileset, 14, 22),
				new Sprite(tileset, 15, 22),
			],
			left = [new Sprite(tileset, 3, 22)],
			right = [new Sprite(tileset, 5, 22)],
			bottom = [new Sprite(tileset, 4, 23)],
		});
		AddTileType(rock = new TileType()
		{
			name = "rock",
			color = 0xFF4a5062,
			isSolid = true,
			breaksArrows = true,
			health = 3,
			particleColor = 0xFF4a5062,
			sprites = [
				new Sprite(tileset, 4, 25),
			],
			left = [new Sprite(tileset, 3, 25)],
			right = [new Sprite(tileset, 5, 25)],
			bottom = [new Sprite(tileset, 4, 26)],
			top = [
				new Sprite(tileset, 4, 24),
				new Sprite(tileset, 6, 24),
				new Sprite(tileset, 7, 24),
				new Sprite(tileset, 8, 24),
				new Sprite(tileset, 9, 24),
				new Sprite(tileset, 10, 24),
				new Sprite(tileset, 11, 24),
				new Sprite(tileset, 12, 24),
				new Sprite(tileset, 13, 24),
				new Sprite(tileset, 14, 24),
				new Sprite(tileset, 15, 24),
			],
		});
		AddTileType(leaf = new TileType()
		{
			name = "leaf",
			color = 0xFF328459,
			isSolid = true,
			health = 1,
			particleColor = 0xFF328459,
			sprites = [
				new Sprite(tileset, 4, 28),
			],
			left = [new Sprite(tileset, 3, 28)],
			right = [new Sprite(tileset, 5, 28)],
			bottom = [new Sprite(tileset, 4, 29)],
			top = [new Sprite(tileset, 4, 27)],
		});
		AddTileType(tree = new TileType()
		{
			name = "tree",
			color = 0xFF452925,
			isSolid = true,
			health = 4,
			particleColor = 0xFF452925,
			sprites = [
				new Sprite(tileset, 4, 31),
			],
			left = [new Sprite(tileset, 3, 31)],
			right = [new Sprite(tileset, 5, 31)],
			bottom = [new Sprite(tileset, 4, 32)],
			top = [new Sprite(tileset, 4, 30)],
		});
	}

	static void AddTileType(TileType type)
	{
		tileTypes.Add(type);
		nameMap.Add(type.name, tileTypes.Count - 1);
		type.id = tileTypes.Count;
	}

	public static TileType Get(string name)
	{
		if (nameMap.ContainsKey(name))
			return tileTypes[nameMap[name]];
		return null;
	}

	public static TileType Get(int id)
	{
		if (id > 0 && id <= tileTypes.Count)
			return tileTypes[id - 1];
		return null;
	}
}
