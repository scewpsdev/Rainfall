using Rainfall;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Tile
{
	public uint id = 0;
	public Vector2i atlasPosition = new Vector2i(0, 0);
	public Vector2i atlasPositionTop = new Vector2i(0, 0);
	public Vector2i atlasPositionBottom = new Vector2i(0, 0);

	public bool solid = true;

	public bool hasTop = true;
	public bool hasBottom = true;
	public bool hasSides = true;
	public bool isFullMesh { get => hasTop && hasBottom && hasSides; }


	public Tile()
	{
	}


	static List<Tile> tiles = new List<Tile>();
	static Dictionary<uint, int> idMap = new Dictionary<uint, int>();

	public static Tile bricks;
	public static Tile dirt;
	public static Tile cobblestone;


	public static void Init()
	{
		AddTile(new Tile { atlasPosition = new Vector2i(0, 0), solid = false, hasTop = false, hasBottom = false, hasSides = false });
		AddTile(bricks = new Tile { atlasPosition = new Vector2i(0, 2), atlasPositionTop = new Vector2i(1, 2), atlasPositionBottom = new Vector2i(2, 2) });
		AddTile(dirt = new Tile { atlasPosition = new Vector2i(0, 1) });
		AddTile(cobblestone = new Tile { atlasPosition = new Vector2i(3, 2) });
	}

	static void AddTile(Tile tile)
	{
		tile.id = (uint)tiles.Count;
		tiles.Add(tile);
		idMap.Add(tile.id, tiles.Count - 1);
	}

	public static Tile Get(uint id)
	{
		if (idMap.ContainsKey(id))
			return tiles[idMap[id]];
		return null;
	}
}
