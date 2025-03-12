using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract class Item
{
	public static SpriteSheet sprites = new SpriteSheet(Resource.GetTexture("sprites/items.png", false), 8, 8);


	public string name;
	public string displayName;
	public Sprite icon;


	public Item(string name, string displayName, Sprite icon)
	{
		this.name = name;
		this.displayName = displayName;
		this.icon = icon;
	}
}
