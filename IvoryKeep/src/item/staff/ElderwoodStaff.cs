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

		baseDamage = 0.8f;
		baseAttackRate = 1.25f;
		manaCost = 0.8f;
		//trigger = false;
		//isSecondaryItem = true;
		secondaryChargeTime = 0;

		intelligenceScaling = 0.8f;

		baseValue = 18;

		//sprite = new Sprite(tileset, 1, 6);
		//renderOffset.x = 0.2f;

		sprite = new Sprite(tileset, 11, 11);
		renderOffset.x = -0.2f;
		renderOffset.y = 0.1f;

		castSound = Resource.GetSounds("sounds/cast", 3);
	}
}
