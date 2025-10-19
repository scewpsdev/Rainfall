using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Club : Weapon
{
	public Club()
		: base("club")
	{
		displayName = "Club";

		baseDamage = 1;
		baseAttackRange = 1.2f;
		baseAttackRate = 1.5f;
		doubleBladed = false;
		baseWeight = 1;
		//attackAngle = MathF.PI * 0.7f;

		value = 4;
		//upgradable = false;

		strengthScaling = 0.7f;

		sprite = new Sprite(tileset, 13, 1);
		renderOffset.x = 0.2f;

		hitSound = woodHit;
	}
}
