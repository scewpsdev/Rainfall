using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Halberd : Weapon
{
	public Halberd()
		: base("halberd")
	{
		displayName = "Halberd";

		baseDamage = 1.6f;
		baseAttackRange = 1.8f;
		baseAttackRate = 1.6f;
		twoHanded = true;
		baseWeight = 2.5f;
		doubleBladed = false;

		value = 16;

		sprite = new Sprite(tileset, 7, 4, 2, 1);
		icon = new Sprite(tileset.texture, 7 * 16 + 12, 4 * 16, 16, 16);
		size = new Vector2(2, 1);
		renderOffset.x = 0.2f;
	}

	protected override void getAttackAnim(int idx, out bool stab, out int swingDir, out float startAngle, out float endAngle)
	{
		base.getAttackAnim(idx, out stab, out swingDir, out startAngle, out endAngle);
		stab = idx % 2 == 0;
	}
}
