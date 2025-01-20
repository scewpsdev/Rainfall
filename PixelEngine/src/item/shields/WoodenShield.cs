using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WoodenShield : Shield
{
	public WoodenShield()
		: base("wooden_shield")
	{
		displayName = "Wooden Shield";

		baseArmor = 1;
		value = 5;
		baseWeight = 1;

		blockDuration = 0.3f;
		blockCharge = 0.08f;
		actionMovementSpeed = 0.5f;
		blockAbsorption = 0.8f;

		sprite = new Sprite(tileset, 9, 8);

		blockSound = woodHit;
	}
}
