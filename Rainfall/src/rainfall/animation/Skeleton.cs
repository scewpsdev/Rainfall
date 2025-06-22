using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
		public int[] meshes { get; internal set; }


		internal Node(int id, string name, Matrix transform)
		{
			this.id = id;
			this.name = name;
			this.transform = transform;
		}

		public bool hasParent(Node node)
		{
			Node n = parent;
			while (n != null)
			{
				if (n.name == node.name)
					return true;
				n = n.parent;
			}
			return false;
		}
	}

	public class Skeleton
	{
		unsafe SceneData* scene;

		public readonly Node[] nodes;
		internal Dictionary<uint, int> nameMap = new Dictionary<uint, int>();

		public readonly Node rootNode;


		internal unsafe Skeleton(SceneData* scene)
		{
			unsafe
			{
				this.scene = scene;
				nodes = new Node[scene->numNodes];

				for (int i = 0; i < scene->numNodes; i++)
				{
					nodes[i] = new Node(scene->nodes[i].id, new string((sbyte*)scene->nodes[i].name), scene->nodes[i].transform);

					nodes[i].meshes = new int[scene->nodes[i].numMeshes];
					for (int j = 0; j < scene->nodes[i].numMeshes; j++)
						nodes[i].meshes[j] = scene->nodes[i].meshes[j];

					// Only add nodes that are actual bones so we dont accidentally confuse bones with scene nodes
					//if (scene->nodes[i].armatureID != -1)
					{
						uint nameHash = Hash.hash(nodes[i].name);
						if (!nameMap.ContainsKey(nameHash))
							nameMap.Add(nameHash, i);
					}
				}

				rootNode = nodes[0];
				processNodeHierarchy(rootNode, null, scene);
			}
		}

		unsafe void processNodeHierarchy(Node node, Node parent, SceneData* scene)
		{
			node.parent = parent;

			int numChildren = scene->nodes[node.id].numChildren;
			node.children = numChildren > 0 ? new Node[numChildren] : null;
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

		public unsafe Matrix getNodeTransform(Node node)
		{
			Matrix inverseBindPose = scene->skeletons[0].inverseBindPose;
			Matrix transform = node.transform;
			while (node.parent != null)
			{
				transform = node.parent.transform * transform;
				node = node.parent;
			}
			return inverseBindPose * transform;
		}

		public unsafe Matrix inverseBindPose
		{
			get => scene->skeletons[0].inverseBindPose;
		}
	}
}
