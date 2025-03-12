using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Player : Entity
{
	public int mana = 10;
	public int maxMana = 10;

	public List<Item> items = new List<Item>();
	public Item[] hotbar = new Item[8];


	public Player()
	{
		sprite = new Sprite(sprites, 0, 0);

		health = 30;
		maxHealth = 30;

		giveItem(new MagicStaff());
	}

	public void giveItem(Item item)
	{
		items.Add(item);
		for (int i = 0; i < hotbar.Length; i++)
		{
			if (hotbar[i] == null)
			{
				hotbar[i] = item;
				break;
			}
		}
	}
}
