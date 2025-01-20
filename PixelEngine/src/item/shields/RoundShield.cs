using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RoundShield : Shield
{
	public RoundShield()
		: base("round_shield")
	{
		displayName = "Round Shield";

		baseArmor = 0.5f;
		value = 2;
		baseWeight = 0.5f;

		blockDuration = 0.3f;
		blockCharge = 0.08f;
		actionMovementSpeed = 0.5f;
		blockAbsorption = 0.6f;

		sprite = new Sprite(tileset, 7, 7);

		blockSound = woodHit;
	}
}
