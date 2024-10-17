using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Diamond : Item
{
	public Diamond()
		: base("diamond", ItemType.Gem)
	{
		displayName = "Diamond";
		stackable = true;

		value = 40;

		sprite = new Sprite(tileset, 3, 0);
	}
}
