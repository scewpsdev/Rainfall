using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Flamberge : Weapon
{
	public Flamberge()
		: base("flamberge")
	{
		displayName = "Flamberge";

		baseDamage = 2;
		baseAttackRange = 1.7f;
		baseAttackRate = 1.0f;
		twoHanded = true;
		attackAngleOffset = -0.25f * MathF.PI;
		attackAngle = MathF.PI;

		value = 29;

		sprite = new Sprite(tileset, 12, 6, 2, 1);
		icon = new Sprite(tileset.texture, 12 * 16, 6 * 16, 16, 16);
		size = new Vector2(2, 1);
		renderOffset.x = 0.7f;
		//ingameSprite = new Sprite(Resource.GetTexture("sprites/sword.png", false));
	}
}
