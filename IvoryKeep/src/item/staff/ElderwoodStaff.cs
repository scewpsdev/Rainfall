using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ElderwoodStaff : Staff
{
	public ElderwoodStaff()
		: base("elderwood_staff")
	{
		displayName = "Elderwood Staff";

		baseDamage = 0.6f;
		baseAttackRate = 2;
		manaCost = 0.7f;
		trigger = false;
		//isSecondaryItem = true;
		secondaryChargeTime = 0;

		intelligenceScaling = 0.8f;

		value = 37;

		sprite = new Sprite(tileset, 1, 6);
		renderOffset.x = 0.4f;

		castSound = Resource.GetSounds("sounds/cast", 3);
	}
}
