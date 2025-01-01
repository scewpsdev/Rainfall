using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Emerald : Item
{
	public Emerald()
		: base("emerald", ItemType.Gem)
	{
		displayName = "Emerald";
		stackable = true;

		value = 20;

		sprite = new Sprite(tileset, 1, 3);
	}
}
