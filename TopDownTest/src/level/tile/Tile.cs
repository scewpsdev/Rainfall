using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Tile
{
	public static Tile STONE_FLOOR;
	public static Tile STONE_WALL;

	public static Tileset tileset;

	static List<Tile> tiles = new List<Tile>();
	static Dictionary<uint, int> idMap = new Dictionary<uint, int>();

	public static void Init()
	{
		tileset = new Tileset(Resource.GetTexture("res/level/tiles.png"), 16, 16);
		STONE_FLOOR = new StoneFloor();
		STONE_WALL = new StoneWall();
	}

	public static Tile Get(uint id)
	{
		if (idMap.ContainsKey(id))
			return tiles[idMap[id]];
		return null;
	}


	public readonly uint id;
	public Vector4 color = new Vector4(1.0f);
	public List<Sprite> sprites = new List<Sprite>();
	public List<Sprite> wallSprites = new List<Sprite>();
	public bool wall = false;
	public int wallHeight = 2;


	public Tile(uint id)
	{
		this.id = id;

		tiles.Add(this);
		idMap.Add(id, tiles.Count - 1);
	}

	public Sprite selectSprite(int x, int z)
	{
		uint hash = Hash.hash(new Vector2i(x, z));
		return sprites[(int)(hash % sprites.Count)];
	}

	public Sprite selectWallSprite(int x, int z)
	{
		uint hash = Hash.hash(new Vector2i(x, z));
		return wallSprites[(int)(hash % wallSprites.Count)];
	}
}

public class StoneFloor : Tile
{
	public StoneFloor()
		: base(1)
	{
		sprites.Add(new Sprite(tileset, 0, 3, 1, 1));
		sprites.Add(new Sprite(tileset, 1, 3, 1, 1));
		sprites.Add(new Sprite(tileset, 2, 3, 1, 1));
		sprites.Add(new Sprite(tileset, 0, 4, 1, 1));
		sprites.Add(new Sprite(tileset, 1, 4, 1, 1));
		sprites.Add(new Sprite(tileset, 0, 5, 1, 1));
		sprites.Add(new Sprite(tileset, 1, 5, 1, 1));
	}
}

public class StoneWall : Tile
{
	public StoneWall()
		: base(2)
	{
		sprites.Add(new Sprite(tileset, 0, 0, 1, 1));
		wallSprites.Add(new Sprite(tileset, 0, 1, 1, 2));
		wallSprites.Add(new Sprite(tileset, 1, 1, 1, 2));
		wall = true;
		wallHeight = 2;
	}
}
