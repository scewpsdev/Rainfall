using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class VoxelType
{
	static List<VoxelType> voxelTypes = new List<VoxelType>();
	static Dictionary<uint, int> idMap = new Dictionary<uint, int>();

	public static readonly VoxelType WALL;
	public static readonly VoxelType FLOOR;
	public static readonly VoxelType CEILING;
	public static readonly VoxelType FLOOR_TILES;
	public static readonly VoxelType STAIRS_NORTH;
	public static readonly VoxelType STAIRS_SOUTH;
	public static readonly VoxelType STAIRS_WEST;
	public static readonly VoxelType STAIRS_EAST;
	public static readonly VoxelType STAIRS_SHALLOW_NORTH;
	public static readonly VoxelType STAIRS_SHALLOW_SOUTH;
	public static readonly VoxelType STAIRS_SHALLOW_WEST;
	public static readonly VoxelType STAIRS_SHALLOW_EAST;

	static VoxelType()
	{
		AddVoxel(WALL = new VoxelType("wall"));
		AddVoxel(FLOOR = new VoxelType("floor"));
		AddVoxel(CEILING = new VoxelType("ceiling"));
		AddVoxel(FLOOR_TILES = new VoxelType("floor_tiles"));
		AddVoxel(STAIRS_NORTH = new VoxelType("stairs") { rotation = Quaternion.Identity });
		AddVoxel(STAIRS_SOUTH = new VoxelType("stairs") { rotation = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI) });
		AddVoxel(STAIRS_WEST = new VoxelType("stairs") { rotation = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI * 0.5f) });
		AddVoxel(STAIRS_EAST = new VoxelType("stairs") { rotation = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI * -0.5f) });
		AddVoxel(STAIRS_SHALLOW_NORTH = new VoxelType("stairs_shallow") { rotation = Quaternion.Identity });
		AddVoxel(STAIRS_SHALLOW_SOUTH = new VoxelType("stairs_shallow") { rotation = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI) });
		AddVoxel(STAIRS_SHALLOW_WEST = new VoxelType("stairs_shallow") { rotation = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI * 0.5f) });
		AddVoxel(STAIRS_SHALLOW_EAST = new VoxelType("stairs_shallow") { rotation = Quaternion.FromAxisAngle(Vector3.Up, MathF.PI * -0.5f) });
	}

	static void AddVoxel(VoxelType type)
	{
		voxelTypes.Add(type);
		idMap.Add(type.id, voxelTypes.Count - 1);
	}

	public static VoxelType Get(uint id)
	{
		if (idMap.ContainsKey(id))
			return voxelTypes[idMap[id]];
		return null;
	}

	/*
	public static VoxelType Get(string name)
	{
		if (nameMap.ContainsKey(name))
			return voxelTypes[nameMap[name]];
		return null;
	}
	*/


	public uint id;
	public string name;
	public bool isTransparent;

	public Model model;
	public Quaternion rotation = Quaternion.Identity;

	public VoxelType(string name, bool isTransparent = false)
	{
		id = (uint)voxelTypes.Count + 1;
		this.name = name;
		model = Resource.GetModel("res/models/tiles/" + name + ".gltf");
		this.isTransparent = isTransparent;
	}
}
