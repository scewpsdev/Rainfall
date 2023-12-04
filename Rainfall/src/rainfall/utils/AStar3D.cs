using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


internal class ANode3D : ANode<Vector3i>
{
}

internal class AGrid3D
{
	public Vector3i size;
	public ANode3D[] nodes;


	public AGrid3D(Vector3i size)
	{
		this.size = size;
		nodes = new ANode3D[size.x * size.y * size.z];

		for (int z = 0; z < size.z; z++)
		{
			for (int y = 0; y < size.y; y++)
			{
				for (int x = 0; x < size.x; x++)
				{
					ANode3D node = new ANode3D();
					node.position = new Vector3i(x, y, z);
					nodes[x + y * size.x + z * size.x * size.y] = node;
				}
			}
		}
	}

	public ANode3D getNode(int x, int y, int z)
	{
		return nodes[x + y * size.x + z * size.x * size.y];
	}

	public ANode3D getNode(Vector3i pos)
	{
		return getNode(pos.x, pos.y, pos.z);
	}
}

public static class AStar3D
{
	static int ManhattanDistance(Vector3i a, Vector3i b)
	{
		return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y) + Math.Abs(a.z - b.z);
	}

	static void ProcessNode(Vector3i position, float gcost, Vector3i destination, ANode3D parent, Heap<Vector3i> openList, HashSet<ANode3D> closedList, AGrid3D grid)
	{
		ANode3D node = grid.getNode(position);
		bool closed = closedList.Contains(node);
		if (closed && gcost >= node.gcost)
			return;

		bool open = openList.contains(node);
		if (!open || gcost < node.gcost)
		{
			node.gcost = gcost;
			node.hcost = ManhattanDistance(position, destination);
			node.parent = parent;

			if (!open)
				openList.add(node);
		}
		else
		{
			openList.updateItem(node);
		}
	}

	public static List<Vector3i> Run(Vector3i start, Vector3i destination, Vector3i size, bool[] walkable, int[] costs = null)
	{
		AGrid3D grid = new AGrid3D(size);

		Heap<Vector3i> openList = new Heap<Vector3i>(size.x * size.y * size.z);
		HashSet<ANode3D> closedList = new HashSet<ANode3D>();

		ANode3D current = grid.getNode(start);
		openList.add(current);

		var isWalkable = (Vector3i position) => walkable[position.x + position.y * size.x + position.z * size.x * size.y];
		var getWalkCost = (Vector3i position, Vector3i direction) => (costs != null ? costs[position.x + position.y * size.x + position.z * size.x * size.y] : 1) + (current.parent != null && direction != current.position - current.parent.position && current.position.y - current.parent.position.y == 0 ? 0.001f : 0.0f);

		while (openList.count > 0)
		{
			current = (ANode3D)openList.removeFirst();
			closedList.Add(current);

			if (current.position == destination)
			{
				List<Vector3i> path = new List<Vector3i>();
				path.Add(current.position);
				while (current.parent != null)
				{
					path.Add(current.parent.position);
					current = (ANode3D)current.parent;
				}
				path.Reverse();
				return path;
			}

			if (current.position.x > 0 && isWalkable(current.position + Vector3i.Left))
				ProcessNode(current.position + Vector3i.Left, current.gcost + getWalkCost(current.position + Vector3i.Left, Vector3i.Left), destination, current, openList, closedList, grid);
			if (current.position.x < size.x - 1 && isWalkable(current.position + Vector3i.Right))
				ProcessNode(current.position + Vector3i.Right, current.gcost + getWalkCost(current.position + Vector3i.Right, Vector3i.Right), destination, current, openList, closedList, grid);
			if (current.position.y > 0 && isWalkable(current.position + Vector3i.Down))
				ProcessNode(current.position + Vector3i.Down, current.gcost + getWalkCost(current.position + Vector3i.Down, Vector3i.Down), destination, current, openList, closedList, grid);
			if (current.position.y < size.y - 1 && isWalkable(current.position + Vector3i.Up))
				ProcessNode(current.position + Vector3i.Up, current.gcost + getWalkCost(current.position + Vector3i.Up, Vector3i.Up), destination, current, openList, closedList, grid);
			if (current.position.z > 0 && isWalkable(current.position + Vector3i.Forward))
				ProcessNode(current.position + Vector3i.Forward, current.gcost + getWalkCost(current.position + Vector3i.Forward, Vector3i.Forward), destination, current, openList, closedList, grid);
			if (current.position.z < size.z - 1 && isWalkable(current.position + Vector3i.Back))
				ProcessNode(current.position + Vector3i.Back, current.gcost + getWalkCost(current.position + Vector3i.Back, Vector3i.Back), destination, current, openList, closedList, grid);
		}

		return null;
	}
}
