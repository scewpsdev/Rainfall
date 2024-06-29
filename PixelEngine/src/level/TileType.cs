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

	public uint color = 0xFFFF00FF;
	public Sprite sprite = null;
	public Sprite left, right, top, bottom;


	static List<TileType> tileTypes = new List<TileType>();
	static Dictionary<string, int> nameMap = new Dictionary<string, int>();

	public static SpriteSheet tileset;

	static TileType()
	{
		tileset = new SpriteSheet(Resource.GetTexture("res/sprites/tiles.png", false), 16, 16);

		AddTileType(new TileType() { name = "dummy", visible = false }); // dummy collider
		AddTileType(new TileType()
		{
			name = "wall",
			color = 0xFFFFFFFF,
			sprite = new Sprite(tileset, 4, 1),
			left = new Sprite(tileset, 3, 1),
			right = new Sprite(tileset, 5, 1),
			top = new Sprite(tileset, 4, 0),
			bottom = new Sprite(tileset, 4, 2),
		});
		AddTileType(new TileType() { name = "platform", color = 0xFF4488AA, isPlatform = true, sprite = new Sprite(tileset, 1, 2) });
		AddTileType(new TileType() { name = "stone_block", color = 0xFF333333, sprite = new Sprite(tileset, 1, 1) });
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
