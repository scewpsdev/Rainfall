using Rainfall;
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
	public bool destructible = false;

	public uint color = 0xFFFF00FF;
	public uint particleColor = 0xFFFFFFFF;
	public Sprite[] sprites = null;
	public Sprite[] left, right, top, bottom;


	static List<TileType> tileTypes = new List<TileType>();
	static Dictionary<string, int> nameMap = new Dictionary<string, int>();

	public static SpriteSheet tileset;

	public static TileType dummy;
	public static TileType wall;
	public static TileType platform;
	public static TileType stone;

	static TileType()
	{
		tileset = new SpriteSheet(Resource.GetTexture("res/sprites/tiles.png", false), 16, 16);

		AddTileType(dummy = new TileType() { name = "dummy", visible = false }); // dummy collider
		AddTileType(wall = new TileType()
		{
			name = "wall",
			color = 0xFFFFFFFF,
			particleColor = 0xFF5c4637,
			sprites = [new Sprite(tileset, 4, 1), new Sprite(tileset, 9, 1), new Sprite(tileset, 10, 1), new Sprite(tileset, 11, 1), new Sprite(tileset, 12, 1)],
			left = [new Sprite(tileset, 3, 1)],
			right = [new Sprite(tileset, 5, 1)],
			top = [new Sprite(tileset, 4, 0), new Sprite(tileset, 9, 0), new Sprite(tileset, 10, 0), new Sprite(tileset, 11, 0), new Sprite(tileset, 12, 0)],
			bottom = [new Sprite(tileset, 4, 2), new Sprite(tileset, 9, 2), new Sprite(tileset, 10, 2), new Sprite(tileset, 11, 2), new Sprite(tileset, 12, 2)],
			destructible = true
		});
		AddTileType(stone = new TileType()
		{
			name = "stone_block",
			color = 0xFF333333,
			particleColor = 0xFF50504c,
			sprites = [new Sprite(tileset, 4, 4), new Sprite(tileset, 9, 4), new Sprite(tileset, 10, 4), new Sprite(tileset, 11, 4), new Sprite(tileset, 12, 4)],
			left = [new Sprite(tileset, 3, 4), new Sprite(tileset, 13, 3), new Sprite(tileset, 13, 4), new Sprite(tileset, 13, 5)],
			right = [new Sprite(tileset, 5, 4), new Sprite(tileset, 15, 3), new Sprite(tileset, 15, 4), new Sprite(tileset, 15, 5)],
			top = [new Sprite(tileset, 4, 3), new Sprite(tileset, 9, 3), new Sprite(tileset, 10, 3), new Sprite(tileset, 11, 3), new Sprite(tileset, 12, 3)],
			bottom = [new Sprite(tileset, 4, 5), new Sprite(tileset, 9, 5), new Sprite(tileset, 10, 5), new Sprite(tileset, 11, 5), new Sprite(tileset, 12, 5)],
		});
		AddTileType(platform = new TileType() { name = "platform", color = 0xFF4488AA, particleColor = 0xFF2e2121, isPlatform = true, isSolid = false, sprites = [new Sprite(tileset, 1, 2)] });
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
