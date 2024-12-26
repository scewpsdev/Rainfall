using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Longsword : Weapon
{
	public Longsword()
		: base("longsword")
	{
		displayName = "Longsword";

		baseDamage = 2.0f;
		baseAttackRange = 1.2f;
		baseAttackRate = 1.4f;
		stab = false;

		value = 14;

		sprite = new Sprite(tileset, 1, 1);
		renderOffset.x = 0.2f;
		//ingameSprite = new Sprite(Resource.GetTexture("res/sprites/sword.png", false));
	}
}
