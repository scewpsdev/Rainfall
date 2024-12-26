using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RoyalGreatsword : Weapon
{
	public RoyalGreatsword()
		: base("royal_greatsword")
	{
		displayName = "Royal Greatsword";

		baseDamage = 4;
		baseAttackRange = 1.8f;
		baseAttackRate = 1.5f;
		stab = false;
		twoHanded = true;
		baseWeight = 3;

		value = 50;

		sprite = new Sprite(tileset, 10, 5, 2, 1);
		icon = new Sprite(tileset, 10.5f, 5);
		size = new Vector2(2, 1);
		renderOffset.x = 0.7f;
		//ingameSprite = new Sprite(Resource.GetTexture("res/sprites/sword.png", false));
	}

	protected override void getAttackAnim(int idx, out bool stab, out int swingDir, out float startAngle, out float endAngle)
	{
		base.getAttackAnim(idx, out stab, out swingDir, out startAngle, out endAngle);
		stab = idx % 2 == 1;
	}
}
