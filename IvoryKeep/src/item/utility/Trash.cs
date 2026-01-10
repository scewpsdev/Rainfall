using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Trash : Item
{
	public Trash()
		: base("trash", ItemType.Utility)
	{
		displayName = "Trash";
		description = "Trash.";

		baseValue = 0;
		canDrop = false;

		isActiveItem = true;
		isHandItem = false;

		sprite = new Sprite(tileset, 9, 12);
	}
}
