using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledCS;

public enum MapTile
{
	None,
	Tile0,
	Tile1,
	Tile2,
	PushableBlock,
	Platform,
	ArrowTrap,
	Ladder,
	Trampoline,
	LadderPlatform,
	Water,
	Spike,
	ExplosiveObject,
	ItemSpawn,
	EnemySpawn,
	RandomTile,
	Placeholder,

	Count
}

public struct DoorDef
{
	public Vector2i position;
	public Vector2i direction;
}

public struct EntitySpawnDef
{
	public Vector2 position;
	public string name;
}

public struct MarkerDef
{
	public Vector2 position;
	public uint id;
}

public struct RoomDef
{
	public int id;
	public RoomDefSet set;

	//public int x;
	//public int y;
	public int width;
	public int height;
	public MapTile[] data;
	public bool mirrored;
	public int mirroredFrom;
	public List<DoorDef> doorDefs;
	public List<EntitySpawnDef> entities;
	public List<MarkerDef> markers;


	public MapTile getTile(int x, int y)
	{
		if (mirrored)
			x = width - x - 1;
		if (x >= 0 && x < width && y >= 0 && y < height)
			return data[x + y * width];
		return MapTile.None;

		/*
		if (mirrored)
			x = width - x - 1;
		y = height - y - 1;
		x += this.x;
		y += this.y;
		return set.rooms[x + y * set.roomsInfo.width];
		*/
	}

	public Vector2 getEntityPosition(Room room, int idx)
	{
		Vector2 localpos = entities[idx].position;
		if (mirrored)
			localpos.x = width - localpos.x;
		return new Vector2(room.x, room.y) + localpos;
	}
}

public class RoomDefSet
{
	public uint[] rooms;
	public TextureInfo roomsInfo;

	public int width { get => roomsInfo.width; }
	public int height { get => roomsInfo.height; }

	public List<RoomDef> roomDefs = new List<RoomDef>();

	public RoomDefSet(string path, bool createMirroredRooms = true)
	{
		if (path != null)
			loadRoomImage(path);

		if (createMirroredRooms)
		{
			// mirrored defs
			int numRoomDefs = roomDefs.Count;
			for (int i = 0; i < numRoomDefs; i++)
			{
				RoomDef def = roomDefs[i];
				def.id = roomDefs.Count;
				def.mirrored = true;
				def.mirroredFrom = roomDefs[i].id;
				def.doorDefs = new List<DoorDef>(roomDefs[i].doorDefs);
				for (int j = 0; j < def.doorDefs.Count; j++)
				{
					DoorDef doorDef = def.doorDefs[j];
					doorDef.position.x = def.width - doorDef.position.x - 1;
					doorDef.direction.x *= -1;
					def.doorDefs[j] = doorDef;
				}
				roomDefs.Add(def);
			}
		}
	}

	MapTile translateMapTileColor(uint color)
	{
		switch (color)
		{
			case 0x00000000:
			case 0xFF000000:
			case 0xFFFF0000:
				return MapTile.None;
			case 0xFFFFFFFF:
				return MapTile.Tile0;
			case 0xFF7F7F7F:
				return MapTile.Tile1;
			case 0xFFAFAFAF:
				return MapTile.Tile2;
			case 0xFF0000FF:
				return MapTile.Platform;
			case 0xFFFF7F7F:
				return MapTile.ArrowTrap;
			case 0xFF00FF00:
				return MapTile.Ladder;
			case 0xFFFF7F00:
				return MapTile.Trampoline;
			case 0xFF00FFFF:
				return MapTile.LadderPlatform;
			case 0xFF007fff:
				return MapTile.Water;
			case 0xFFff6100:
				return MapTile.Spike;
			case 0xFFff9600:
				return MapTile.ExplosiveObject;
			case 0xFF00cf5f:
				return MapTile.ItemSpawn;
			case 0xFFFFFF00:
				return MapTile.RandomTile;
			default:
				return MapTile.None;
		}
	}

	unsafe void loadRoomImage(string path)
	{
		Texture roomsTexture = Resource.GetTexture(path, false, true);
		roomsTexture.getImageData(out ImageData image);
		rooms = new uint[image.width * image.height];
		for (int i = 0; i < image.width * image.height; i++)
			rooms[i] = image.data[i];
		roomsInfo = roomsTexture.info;
		image.free();

		for (int y = 0; y < roomsInfo.height; y++)
		{
			for (int x = 0; x < roomsInfo.width; x++)
			{
				uint pixel = rooms[x + y * roomsInfo.width];
				if (pixel != 0xFFFF00FF)
				{
					uint top = y > 0 ? rooms[x + (y - 1) * roomsInfo.width] : 0xFFFF00FF;
					uint left = x > 0 ? rooms[x - 1 + y * roomsInfo.width] : 0xFFFF00FF;
					if (top == 0xFFFF00FF && left == 0xFFFF00FF)
					{
						int roomWidth = 0, roomHeight = 0;
						for (int xx = x; xx < roomsInfo.width; xx++)
						{
							if (rooms[xx + y * roomsInfo.width] != 0xFFFF00FF)
								roomWidth++;
							else
								break;
						}
						for (int yy = y; yy < roomsInfo.height; yy++)
						{
							if (rooms[x + yy * roomsInfo.width] != 0xFFFF00FF)
								roomHeight++;
							else
								break;
						}

						MapTile[] data = new MapTile[roomWidth * roomHeight];
						List<DoorDef> doorDefs = new List<DoorDef>();
						List<MarkerDef> markers = new List<MarkerDef>();

						for (int yy = y; yy < y + roomHeight; yy++)
						{
							for (int xx = x; xx < x + roomWidth; xx++)
							{
								if (rooms[xx + yy * roomsInfo.width] == 0xFFFF0000)
								{
									Vector2i doorPosition = new Vector2i(xx - x, roomHeight - (yy - y) - 1);
									Vector2i doorDirection =
										yy == y ? Vector2i.Up :
										yy == y + roomHeight - 1 ? Vector2i.Down :
										xx == x ? Vector2i.Left :
										xx == x + roomWidth - 1 ? Vector2i.Right :
										Vector2i.Zero;
									doorDefs.Add(new DoorDef { position = doorPosition, direction = doorDirection });
								}
								else
								{
									uint color = rooms[xx + yy * roomsInfo.width];
									if ((color | 0x0000FF00) == 0xFFFFFFFF && color != 0xFFFF00FF && color != 0xFFFFFFFF) // marker
									{
										uint markerID = (color & 0x0000FF00) >> 8;
										markers.Add(new MarkerDef() { id = markerID, position = new Vector2(xx - x, roomHeight - (yy - y) - 1) });
									}
									else
									{
										data[(xx - x) + (roomHeight - (yy - y) - 1) * roomWidth] = translateMapTileColor(color);
									}
								}
							}
						}

						roomDefs.Add(new RoomDef { id = roomDefs.Count, set = this, /*x = x, y = y,*/ width = roomWidth, height = roomHeight, data = data, doorDefs = doorDefs, markers = markers });
					}
				}
			}
		}
	}

	MapTile translateMapTileID(int id)
	{
		switch (id)
		{
			case 0:
				return MapTile.None;
			case 1:
				return MapTile.Tile0;
			case 2:
				return MapTile.PushableBlock;
			case 3:
				return MapTile.Placeholder;
			case 17:
				return MapTile.Platform;
			case 18:
				return MapTile.Ladder;
			case 19:
				return MapTile.Spike;
			case 20:
				return MapTile.Trampoline;
			case 25:
				return MapTile.ItemSpawn;
			case 26:
				return MapTile.EnemySpawn;
			default:
				return MapTile.None;
		}
	}

	public void loadTmx(string path)
	{
		string xml = Resource.GetText(path);

		MemoryStream stream = new MemoryStream();
		StreamWriter writer = new StreamWriter(stream);
		writer.Write(xml);
		writer.Flush();
		stream.Position = 0;
		TiledMap map = new TiledMap(stream);

		int width = map.Width;
		int height = map.Height;

		RoomDef roomDef = new RoomDef();
		roomDef.id = roomDefs.Count;
		roomDef.set = this;
		roomDef.width = width;
		roomDef.height = height;
		roomDef.data = new MapTile[width * height];
		roomDef.doorDefs = new List<DoorDef>();
		roomDef.entities = new List<EntitySpawnDef>();
		roomDef.markers = new List<MarkerDef>();

		for (int i = 0; i < map.Layers.Length; i++)
		{
			TiledLayer layer = map.Layers[i];
			if (layer.type == TiledLayerType.TileLayer)
			{
				Debug.Assert(layer.width == width && layer.height == height);
				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						int data = layer.data[x + (height - y - 1) * width];
						if (data >= 8 && data < 12)
						{
							Vector2i doorPosition = new Vector2i(x, y);
							Vector2i doorDirection =
								data == 9 ? Vector2i.Right :
								data == 10 ? Vector2i.Left :
								data == 11 ? Vector2i.Down :
								data == 12 ? Vector2i.Up :
								Vector2i.Zero;
							roomDef.doorDefs.Add(new DoorDef { position = doorPosition, direction = doorDirection });
						}
						else if (data != 0)
						{
							MapTile tile = translateMapTileID(data);
							roomDef.data[x + y * width] = tile;
						}
					}
				}
			}
			else if (layer.type == TiledLayerType.ObjectLayer)
			{
				for (int j = 0; j < layer.objects.Length; j++)
				{
					TiledObject obj = layer.objects[j];

					Vector2 position = new Vector2(obj.x, obj.y) / new Vector2(map.TileWidth, map.TileHeight);
					position.y = height - position.y;

					if (obj.type == "marker")
					{
						uint id = Convert.ToUInt32(obj.name, 16);
						roomDef.markers.Add(new MarkerDef() { id = id, position = position });
					}
					else
					{
						EntitySpawnDef entityDef = new EntitySpawnDef();
						entityDef.position = position;
						entityDef.name = obj.name;
						roomDef.entities.Add(entityDef);
					}
				}
			}
		}

		roomDefs.Add(roomDef);

		stream.Close();
	}
}