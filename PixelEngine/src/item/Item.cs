using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum ItemType
{
	Tool,
	Active,
	Passive,
}

public abstract class Item
{
	public static SpriteSheet tileset = new SpriteSheet(Resource.GetTexture("res/sprites/items.png", false), 16, 16);


	public int id;

	public string name;
	public string displayName = "???";
	public ItemType type = ItemType.Tool;

	public int attackDamage = 1;
	public float attackRange = 1;
	public int maxPierces = 0;

	public bool projectileItem = false;
	public bool breakOnHit = true;

	public Sprite sprite = null;
	public Sprite ingameSprite = null;

	// modifiers


	public Item(string name)
	{
		id = (int)Hash.hash(name);
		this.name = name;
	}

	public virtual void use(Player player)
	{
	}
}
