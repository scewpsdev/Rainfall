using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	public class Node
	{
		public readonly int id;
		public readonly string name;
		public readonly Matrix transform;

		public Node parent { get; internal set; }
		public Node[] children { get; internal set; }


		internal Node(int id, string name, Matrix transform)
		{
			this.id = id;
			this.name = name;
			this.transform = transform;
		}
	}

	public class Skeleton
	{
		IntPtr model;

		public readonly Node[] nodes;
		internal Dictionary<uint, int> nameMap = new Dictionary<uint, int>();

		public readonly Node rootNode;


		internal Skeleton(IntPtr model)
		{
			unsafe
			{
				this.model = model;

				SceneData* scene = (SceneData*)Native.Resource.Resource_ModelGetSceneData(model);
				nodes = new Node[scene->numNodes];
				for (int i = 0; i < scene->numNodes; i++)
				{
					nodes[i] = new Node(scene->nodes[i].id, Marshal.PtrToStringAnsi((IntPtr)scene->nodes[i].name), scene->nodes[i].transform);
					uint nameHash = Hash.hash(nodes[i].name);
					if (!nameMap.ContainsKey(nameHash))
						nameMap.Add(nameHash, i);
				}

				rootNode = nodes[0];
				processNodeHierarchy(rootNode, null, scene);
			}
		}

		unsafe void processNodeHierarchy(Node node, Node parent, SceneData* scene)
		{
			node.parent = parent;

			int numChildren = scene->nodes[node.id].numChildren;
			node.children = new Node[numChildren];
			for (int i = 0; i < numChildren; i++)
			{
				int childID = scene->nodes[node.id].children[i];
				node.children[i] = nodes[childID];
				processNodeHierarchy(node.children[i], node, scene);
			}
		}

		public Node getNode(int id)
		{
			return nodes[id];
		}

		public Node getNode(string name)
		{
			uint nameHash = Hash.hash(name);
			if (nameMap.ContainsKey(nameHash))
				return nodes[nameMap[nameHash]];
			return null;
		}

		public Node getNode(Span<byte> name)
		{
			uint nameHash = Hash.hash(name);
			if (nameMap.ContainsKey(nameHash))
				return nodes[nameMap[nameHash]];
			return null;
		}

		public Matrix inverseBindPose
		{
			get
			{
				unsafe
				{
					return ((SceneData*)Native.Resource.Resource_ModelGetSceneData(model))->skeletons[0].inverseBindPose;
				}
			}
		}
	}
}
