using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TileType
{
	static Dictionary<int, TileType> tileTypes = new Dictionary<int, TileType>();

	static SpriteSheet sprites = new SpriteSheet(Resource.GetTexture("sprites/tiles.png", false), 16, 16);

	static TileType()
	{
		InitTileType(new TileType(1) { sprite = new Sprite(sprites, 0, 0), color = 0xFF999999 });
	}

	static void InitTileType(TileType tile)
	{
		tileTypes.Add(tile.id, tile);
	}

	public static TileType Get(int id)
	{
		if (tileTypes.TryGetValue(id, out TileType tile))
			return tile;
		return null;
	}


	public int id;
	public bool wall = true;

	public Sprite sprite;
	public Sprite topSprite;
	public Vector4 color = Vector4.One;
	public Vector4 topColor = Vector4.One;


	public TileType(int id)
	{
		this.id = id;
	}
}
