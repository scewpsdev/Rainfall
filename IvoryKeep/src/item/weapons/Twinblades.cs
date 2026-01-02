using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Twinblades : Weapon
{
	public Twinblades()
		: base("twinblades")
	{
		displayName = "Twinblades";

		baseDamage = 1.3f;
		baseAttackRange = 1.3f;
		baseAttackRate = 1.0f;
		attackStartAngle = 2.75f * MathF.PI;
		attackEndAngle = -1.25f * MathF.PI;
		twoHanded = true;
		doubleBladed = true;
		baseWeight = 2;
		//stab = false;
		//attackAngle = MathF.PI * 0.7f;

		dexterityScaling = 0.8f;

		baseValue = 46;

		sprite = new Sprite(tileset, 10, 8, 2, 1);
		size = new Vector2(2, 1);
		renderOffset.x = 0.2f;
	}

	protected override void getAttackAnim(Player player, int idx, out AttackAnim anim, out int swingDir, out float startAngle, out float endAngle, out float range)
	{
		base.getAttackAnim(player, idx, out anim, out swingDir, out startAngle, out endAngle, out range);
		if (idx % 2 == 1)
			anim = AttackAnim.SwingSideways;
	}
}
