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

		baseDamage = 1.5f;
		baseAttackRate = 0.7f;
		baseAttackRange = 1.7f;
		manaCost = 2;
		//trigger = false;
		secondaryChargeTime = 0;
		knockback = 2.0f;
		twoHanded = true;
		anim = AttackAnim.SwingOverhead;

		baseValue = 55;

		intelligenceScaling = 1.0f;

		sprite = new Sprite(tileset, 5, 7, 2, 1);
		size = new Vector2(2, 1);
		renderOffset.x = 0.4f;
		backRotation = 0.5f * MathF.PI;
		weaponTipMargin = 2 / 16.0f;

		castSound = Resource.GetSounds("sounds/cast", 3);
	}
}
