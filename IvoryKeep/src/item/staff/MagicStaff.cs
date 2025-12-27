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

		baseValue = 12;
		//rarity = 10;

		//sprite = new Sprite(tileset, 2, 6);
		//renderOffset.x = 0.2f;

		sprite = new Sprite(tileset, 10, 11);
		renderOffset.x = -0.2f;
		renderOffset.y = 0.1f;

		hitSound = woodHit;

		intelligenceScaling = 0.6f;
	}
}
