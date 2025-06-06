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

		value = 30;
		rarity = 3;

		sprite = new Sprite(tileset, 2, 6);
		renderOffset.x = 0.2f;

		hitSound = woodHit;

		intelligenceScaling = 0.6f;
	}
}
