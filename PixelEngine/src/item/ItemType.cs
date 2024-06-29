using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ItemType
{
	public int id;

	public string name;
	public string displayName;

	public bool throwable;


	public virtual void use(Item item, Player player)
	{
		if (throwable)
		{
			player.throwItem(item);
			player.handItem = null;
		}
	}


	static List<ItemType> itemTypes = new List<ItemType>();
	static Dictionary<string, int> nameMap = new Dictionary<string, int>();

	static ItemType()
	{
		AddItemType(new ItemType() { name = "debug_item", displayName = "Debug Item", throwable = true });
	}

	static void AddItemType(ItemType type)
	{
		itemTypes.Add(type);
		nameMap.Add(type.name, itemTypes.Count - 1);
		type.id = itemTypes.Count;
	}

	public static ItemType Get(string name)
	{
		if (nameMap.ContainsKey(name))
			return itemTypes[nameMap[name]];
		return null;
	}

	public static ItemType Get(int id)
	{
		if (id > 0 && id <= itemTypes.Count)
			return itemTypes[id - 1];
		return null;
	}
}
