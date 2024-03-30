using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Item
{
	public string name;
	public string displayName;
	public Sprite sprite;
	public Sprite icon;


	static SpriteSheet spriteSheet;
	static Dictionary<string, Item> items = new Dictionary<string, Item>();

	public static void Init()
	{
		spriteSheet = new SpriteSheet(Resource.GetTexture("res/item/items.png", false), 16, 16);

		ReadItem("staff");
	}

	static void ReadItem(string name)
	{
		string path = "res/item/" + name + ".dat";
		string src = File.ReadAllText(path + ".bin");
		DatFile file = new DatFile(src, path);

		Item item = new Item();
		file.getStringContent("name", out item.name);
		file.getStringContent("displayName", out item.displayName);

		if (file.getStringContent("sprite", out string spritePath))
			item.sprite = new Sprite(Resource.GetTexture("res/item/" + spritePath, false));
		if (file.getStringContent("icon", out string iconPath))
			item.icon = new Sprite(Resource.GetTexture("res/item/" + iconPath, false));

		items.Add(item.name, item);
	}

	public static Item Get(string name)
	{
		if (items.ContainsKey(name))
			return items[name];
		return null;
	}
}
