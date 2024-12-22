using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MagicStaff : Staff
{
	public MagicStaff()
		: base("magic_staff")
	{
		displayName = "Magic Staff";

		canDrop = false;

		value = 10;

		sprite = new Sprite(tileset, 2, 6);
		renderOffset.x = 0.4f;

		hitSound = woodHit;
	}
}
