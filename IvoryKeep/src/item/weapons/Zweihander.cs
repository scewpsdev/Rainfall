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
		baseAttackRate = 0.9f;
		anim = AttackAnim.SwingSideways;
		attackAcceleration = 1;
		twoHanded = true;
		baseWeight = 2.5f;

		canBlock = true;

		attackEndAngle = -0.75f * MathF.PI;
		attackStartAngle = 0.75f * MathF.PI;
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

	protected override void getAttackAnim(Player player, int idx, out AttackAnim anim, out int swingDir, out float startAngle, out float endAngle, out float range)
	{
		base.getAttackAnim(player, idx, out anim, out swingDir, out startAngle, out endAngle, out range);

		if (idx % 2 == 1)
		{
			startAngle = MathF.PI * 0.75f;
			endAngle = -0.85f * MathF.PI;
		}
	}
}
