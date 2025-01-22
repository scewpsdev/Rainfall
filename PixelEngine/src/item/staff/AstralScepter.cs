using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AstralScepter : Staff
{
	public AstralScepter()
		: base("astral_scepter")
	{
		displayName = "Astral Scepter";

		baseDamage = 2;
		baseAttackRate = 0.7f;
		manaCost = 2;
		trigger = false;
		secondaryChargeTime = 0;
		knockback = 2.0f;
		twoHanded = true;

		value = 75;

		sprite = new Sprite(tileset, 5, 7, 2, 1);
		size = new Vector2(2, 1);
		renderOffset.x = 0.4f;

		castSound = Resource.GetSounds("sounds/cast", 3);
	}

	protected override void getAttackAnim(int idx, out AttackAnim anim, out int swingDir, out float startAngle, out float endAngle)
	{
		base.getAttackAnim(idx, out anim, out swingDir, out startAngle, out endAngle);
		anim = idx % 2 == 0 ? AttackAnim.Stab : AttackAnim.SwingSideways;
	}
}
