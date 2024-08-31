using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


internal class ANode<T>
{
	public T position;
	public float gcost, hcost;
	public ANode<T> parent;

	internal int heapIndex;


	public float fcost => gcost + hcost;

	/*
	public bool Equals(ANode3D b)
	{
		return position == b.position;
	}
	*/

	public static bool operator <(ANode<T> a, ANode<T> b)
	{
		if (a.fcost == b.fcost)
			return a.hcost > b.hcost;
		return a.fcost > b.fcost;
	}

	public static bool operator >(ANode<T> a, ANode<T> b)
	{
		if (a.fcost == b.fcost)
			return a.hcost < b.hcost;
		return a.fcost < b.fcost;
	}
}

internal class ANode2 : ANode<Vector2i>
{
}

class Heap<T>
{
	ANode<T>[] items;
	int currentItemCount;


	public Heap(int maxHeapSize)
	{
		items = new ANode<T>[maxHeapSize];
		currentItemCount = 0;
	}

	public void reset()
	{
		Array.Fill(items, null);
		currentItemCount = 0;
	}

	void swap(ANode<T> a, ANode<T> b)
	{
		items[a.heapIndex] = b;
		items[b.heapIndex] = a;
		int tmp = a.heapIndex;
		a.heapIndex = b.heapIndex;
		b.heapIndex = tmp;
	}

	void sortUp(ANode<T> item)
	{
		int parentIndex = (item.heapIndex - 1) / 2;

		while (true)
		{
			ANode<T> parentItem = items[parentIndex];
			if (item > parentItem)
				swap(item, parentItem);
			else
				break;
			parentIndex = (item.heapIndex - 1) / 2;
		}
	}

	public void add(ANode<T> item)
	{
		item.heapIndex = currentItemCount;
		items[currentItemCount] = item;
		sortUp(item);
		currentItemCount++;
	}

	void sortDown(ANode<T> item)
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

	public ANode<T> removeFirst()
	{
		ANode<T> firstItem = items[0];
		currentItemCount--;
		items[0] = items[currentItemCount];
		items[0].heapIndex = 0;
		sortDown(items[0]);
		return firstItem;
	}

	public bool contains(ANode<T> item)
	{
		return items[item.heapIndex] == item;
	}

	public int count { get => currentItemCount; }

	public void updateItem(ANode<T> item)
	{
		sortUp(item);
		sortDown(item);
	}
}

internal class AGrid
{
	internal int width, height;
	internal ANode2[] nodes;


	internal AGrid(int width, int height)
	{
		this.width = width;
		this.height = height;
		nodes = new ANode2[width * height];

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				ANode2 node = new ANode2();
				node.position = new Vector2i(x, y);
				nodes[x + y * width] = node;
			}
		}
	}

	public void reset()
	{
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				nodes[x + y * width].gcost = 0;
				nodes[x + y * width].hcost = 0;
				nodes[x + y * width].parent = null;
				nodes[x + y * width].heapIndex = 0;
			}
		}
	}

	internal ANode2 getNode(int x, int y)
	{
		return nodes[x + y * width];
	}

	internal ANode2 getNode(Vector2i pos)
	{
		return getNode(pos.x, pos.y);
	}
}

public class AStar
{
	AGrid grid;
	Heap<Vector2i> openList;
	bool[] walkable;
	int[] costs;


	public AStar(int width, int height, bool[] walkable, int[] costs = null)
	{
		grid = new AGrid(width, height);
		openList = new Heap<Vector2i>(width * height);
		this.walkable = walkable;
		this.costs = costs;
	}


	static int ManhattanDistance(Vector2i a, Vector2i b)
	{
		return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
	}

	static void ProcessNode(Vector2i position, float gcost, Vector2i destination, ANode2 parent, Heap<Vector2i> openList, HashSet<ANode2> closedList, AGrid grid)
	{
		ANode2 node = grid.getNode(position);
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

	public bool run(Vector2i start, Vector2i destination, List<Vector2i> path, bool preferVerticalMovement = false)
	{
		grid.reset();
		openList.reset();

		HashSet<ANode2> closedList = new HashSet<ANode2>();

		ANode2 current = grid.getNode(start);
		openList.add(current);

		var isWalkable = (Vector2i position) => walkable[position.x + position.y * grid.width];
		var getWalkCost = (Vector2i position) => costs != null ? costs[position.x + position.y * grid.width] : 1;

		while (openList.count > 0)
		{
			current = (ANode2)openList.removeFirst();
			closedList.Add(current);

			if (current.position == destination)
			{
				if (path != null)
				{
					path.Clear();
					path.Add(current.position);
					while (current.parent != null)
					{
						path.Add(current.parent.position);
						current = (ANode2)current.parent;
					}
					path.Reverse();
				}
				return true;
			}

			if (preferVerticalMovement)
			{
				if (current.position.y > 0 && isWalkable(current.position + Vector2i.Down))
					ProcessNode(current.position + Vector2i.Down, current.gcost + getWalkCost(current.position + Vector2i.Down), destination, current, openList, closedList, grid);
				if (current.position.y < grid.height - 1 && isWalkable(current.position + Vector2i.Up))
					ProcessNode(current.position + Vector2i.Up, current.gcost + getWalkCost(current.position + Vector2i.Up), destination, current, openList, closedList, grid);
				if (current.position.x > 0 && isWalkable(current.position + Vector2i.Left))
					ProcessNode(current.position + Vector2i.Left, current.gcost + getWalkCost(current.position + Vector2i.Left), destination, current, openList, closedList, grid);
				if (current.position.x < grid.width - 1 && isWalkable(current.position + Vector2i.Right))
					ProcessNode(current.position + Vector2i.Right, current.gcost + getWalkCost(current.position + Vector2i.Right), destination, current, openList, closedList, grid);
			}
			else
			{
				if (current.position.x > 0 && isWalkable(current.position + Vector2i.Left))
					ProcessNode(current.position + Vector2i.Left, current.gcost + getWalkCost(current.position + Vector2i.Left), destination, current, openList, closedList, grid);
				if (current.position.x < grid.width - 1 && isWalkable(current.position + Vector2i.Right))
					ProcessNode(current.position + Vector2i.Right, current.gcost + getWalkCost(current.position + Vector2i.Right), destination, current, openList, closedList, grid);
				if (current.position.y > 0 && isWalkable(current.position + Vector2i.Down))
					ProcessNode(current.position + Vector2i.Down, current.gcost + getWalkCost(current.position + Vector2i.Down), destination, current, openList, closedList, grid);
				if (current.position.y < grid.height - 1 && isWalkable(current.position + Vector2i.Up))
					ProcessNode(current.position + Vector2i.Up, current.gcost + getWalkCost(current.position + Vector2i.Up), destination, current, openList, closedList, grid);
			}
		}

		return false;
	}
}
