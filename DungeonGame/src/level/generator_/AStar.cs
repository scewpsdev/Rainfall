using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


internal class Heap
{
	ANode[] items;
	int currentItemCount;


	public Heap(int maxHeapSize)
	{
		items = new ANode[maxHeapSize];
		currentItemCount = 0;
	}

	void swap(ANode a, ANode b)
	{
		items[a.heapIndex] = b;
		items[b.heapIndex] = a;
		int tmp = a.heapIndex;
		a.heapIndex = b.heapIndex;
		b.heapIndex = tmp;
	}

	void sortUp(ANode item)
	{
		int parentIndex = (item.heapIndex - 1) / 2;

		while (true)
		{
			ANode parentItem = items[parentIndex];
			if (item > parentItem)
				swap(item, parentItem);
			else
				break;
			parentIndex = (item.heapIndex - 1) / 2;
		}
	}

	public void add(ANode item)
	{
		item.heapIndex = currentItemCount;
		items[currentItemCount] = item;
		sortUp(item);
		currentItemCount++;
	}

	void sortDown(ANode item)
	{
		while (true)
		{
			int childLeftIndex = item.heapIndex * 2 + 1;
			int childRightIndex = item.heapIndex * 2 + 2;
			int swapIndex;

			if (childLeftIndex < currentItemCount)
			{
				swapIndex = childLeftIndex;
				if (childRightIndex < currentItemCount)
				{
					if (items[childLeftIndex] < items[childRightIndex])
						swapIndex = childRightIndex;
				}

				if (item < items[swapIndex])
					swap(item, items[swapIndex]);
				else
					break;
			}
			else
				break;
		}
	}

	public ANode removeFirst()
	{
		ANode firstItem = items[0];
		currentItemCount--;
		items[0] = items[currentItemCount];
		items[0].heapIndex = 0;
		sortDown(items[0]);
		return firstItem;
	}

	public bool contains(ANode item)
	{
		return items[item.heapIndex] == item;
	}

	public int count { get => currentItemCount; }

	public void updateItem(ANode item)
	{
		sortUp(item);
		sortDown(item);
	}
}

internal class ANode
{
	public Vector2i position;
	public int gcost, hcost;
	public ANode parent;

	internal int heapIndex;


	public int fcost => gcost + hcost;

	/*
	public bool Equals(ANode b)
	{
		return position == b.position;
	}
	*/

	public static bool operator <(ANode a, ANode b)
	{
		if (a.fcost == b.fcost)
			return a.hcost > b.hcost;
		return a.fcost > b.fcost;
	}

	public static bool operator >(ANode a, ANode b)
	{
		if (a.fcost == b.fcost)
			return a.hcost < b.hcost;
		return a.fcost < b.fcost;
	}
}

internal class AGrid
{
	public int width, height;
	public ANode[] nodes;


	public AGrid(int width, int height)
	{
		this.width = width;
		this.height = height;
		nodes = new ANode[width * height];

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				ANode node = new ANode();
				node.position = new Vector2i(x, y);
				nodes[x + y * width] = node;
			}
		}
	}

	public ANode getNode(int x, int y)
	{
		return nodes[x + y * width];
	}

	public ANode getNode(Vector2i pos)
	{
		return getNode(pos.x, pos.y);
	}
}

internal static class AStar
{
	static int ManhattanDistance(Vector2i a, Vector2i b)
	{
		return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
	}

	static void ProcessNode(Vector2i position, int gcost, Vector2i destination, ANode parent, Heap openList, HashSet<ANode> closedList, AGrid grid)
	{
		ANode node = grid.getNode(position);
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

	static int GetWalkCost(int x, int y, int width, int[] costs)
	{
		if (costs != null)
			return costs[x + y * width];
		return 1;
	}

	public static List<Vector2i> Run(Vector2i start, Vector2i destination, int width, int height, bool[] walkable, int[] costs = null)
	{
		AGrid grid = new AGrid(width, height);

		Heap openList = new Heap(width * height);
		HashSet<ANode> closedList = new HashSet<ANode>();

		ANode current = grid.getNode(start);
		openList.add(current);

		while (openList.count > 0)
		{
			current = openList.removeFirst();
			closedList.Add(current);

			if (current.position == destination)
			{
				List<Vector2i> path = new List<Vector2i>();
				while (current.parent != null)
				{
					path.Add(current.parent.position);
					current = current.parent;
				}
				return path;
			}

			if (current.position.x > 0 && walkable[current.position.x - 1 + (current.position.y) * width])
				ProcessNode(current.position + new Vector2i(-1, 0), current.gcost + GetWalkCost(current.position.x - 1, current.position.y, width, costs), destination, current, openList, closedList, grid);
			if (current.position.x < width - 1 && walkable[current.position.x + 1 + (current.position.y) * width])
				ProcessNode(current.position + new Vector2i(1, 0), current.gcost + GetWalkCost(current.position.x + 1, current.position.y, width, costs), destination, current, openList, closedList, grid);
			if (current.position.y > 0 && walkable[current.position.x + (current.position.y - 1) * width])
				ProcessNode(current.position + new Vector2i(0, -1), current.gcost + GetWalkCost(current.position.x, current.position.y - 1, width, costs), destination, current, openList, closedList, grid);
			if (current.position.y < height - 1 && walkable[current.position.x + (current.position.y + 1) * width])
				ProcessNode(current.position + new Vector2i(0, 1), current.gcost + GetWalkCost(current.position.x, current.position.y + 1, width, costs), destination, current, openList, closedList, grid);
		}

		return null;
	}
}
