using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Zweihander : Weapon
{
	public Zweihander()
		: base("zweihander")
	{
		displayName = "Zweihander";

		baseDamage = 3.5f;
		baseAttackRange = 1.8f;
		baseAttackRate = 1.0f;
		stab = false;
		twoHanded = true;
		baseWeight = 2.5f;
		attackAngleOffset = 0;
		attackAngle = MathF.PI;

		value = 18;

		sprite = new Sprite(tileset, 7, 3, 2, 1);
		icon = new Sprite(tileset.texture, 7 * 16 + 8, 3 * 16, 16, 16);
		size = new Vector2(2, 1);
		renderOffset.x = 0.7f;
		//ingameSprite = new Sprite(Resource.GetTexture("sprites/sword.png", false));
	}
}
