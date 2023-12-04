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
	static Dictionary<string, int> nameMap = new Dictionary<string, int>();

	static VoxelType()
	{
		AddVoxel(new VoxelType("wall", true));
		AddVoxel(new VoxelType("floor", true));
		AddVoxel(new VoxelType("ceiling", true));
		AddVoxel(new VoxelType("floor_tiles", true));
	}

	static void AddVoxel(VoxelType type)
	{
		voxelTypes.Add(type);
		idMap.Add(type.id, voxelTypes.Count - 1);
		nameMap.Add(type.name, voxelTypes.Count - 1);
	}

	public static VoxelType Get(uint id)
	{
		if (idMap.ContainsKey(id))
			return voxelTypes[idMap[id]];
		return null;
	}

	public static VoxelType Get(string name)
	{
		if (nameMap.ContainsKey(name))
			return voxelTypes[nameMap[name]];
		return null;
	}


	public uint id;
	public string name;
	public bool isTransparent;

	public Model model;
	public Quaternion rotation;

	public VoxelType(string name, bool isTransparent)
	{
		id = (uint)voxelTypes.Count + 1;
		this.name = name;
		model = Resource.GetModel("res/models/tiles/" + name + ".gltf");
		this.isTransparent = isTransparent;
	}
}
