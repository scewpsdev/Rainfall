using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Scimitar : Weapon
{
	public Scimitar()
		: base("scimitar")
	{
		displayName = "Scimitar";

		baseDamage = 1.0f;
		baseAttackRange = 1.2f;
		baseAttackRate = 2.0f;
		attackCooldown = 0.3f;
		baseWeight = 1.5f;

		value = 16;

		sprite = new Sprite(tileset, 5, 3);
		renderOffset.x = 0.2f;
		//ingameSprite = new Sprite(Resource.GetTexture("sprites/sword.png", false));
	}
}
