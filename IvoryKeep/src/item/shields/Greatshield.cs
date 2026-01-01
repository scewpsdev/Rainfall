using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Greatshield : Shield
{
	public Greatshield()
		: base("greatshield")
	{
		displayName = "Greatshield";

		baseArmor = 4;
		baseValue = 52;
		baseWeight = 3;
		blockAbsorption = 1.0f;
		knockbackAbsorption = 0.8f;
		blockMovementSpeed = 0.2f;

		sprite = new Sprite(tileset, 14, 11);
	}
}
