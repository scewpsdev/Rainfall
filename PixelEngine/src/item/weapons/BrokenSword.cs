using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BrokenSword : Weapon
{
	public BrokenSword()
		: base("broken_sword")
	{
		displayName = "Broken Sword";

		baseDamage = 0.8f;
		baseAttackRange = 1;
		baseAttackRate = 2;
		stab = false;
		baseWeight = 1;

		value = 2;

		sprite = new Sprite(tileset, 15, 3);
		renderOffset.x = 0.2f;

		//ingameSprite = new Sprite("sprites/items/weapon/broken_sword.png", 0, 0, 32, 32);
		//ingameSpriteSize = 2;
	}
}
