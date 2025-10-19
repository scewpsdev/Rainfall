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

		baseDamage = 2.5f;
		baseAttackRange = 1.6f;
		baseAttackRate = 1.0f;
		anim = AttackAnim.SwingOverhead;
		attackAcceleration = 1;
		twoHanded = true;
		baseWeight = 2.5f;

		canBlock = true;

		attackEndAngle = -0.25f * MathF.PI;
		attackStartAngle = MathF.PI;
		//doubleBladed = true;

		strengthScaling = 0.3f;
		dexterityScaling = 0.2f;

		value = 18;

		sprite = new Sprite(tileset, 7, 3, 2, 1);
		icon = new Sprite(tileset.texture, 7 * 16 + 8, 3 * 16, 16, 16);
		size = new Vector2(2, 1);
		renderOffset.x = 0.4f;
		//ingameSprite = new Sprite(Resource.GetTexture("sprites/sword.png", false));
	}
}
