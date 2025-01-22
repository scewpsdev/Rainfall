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
		twoHanded = true;
		baseWeight = 3;

		value = 50;

		sprite = new Sprite(tileset, 10, 5, 2, 1);
		icon = new Sprite(tileset, 10.5f, 5);
		size = new Vector2(2, 1);
		renderOffset.x = 0.7f;
		//ingameSprite = new Sprite(Resource.GetTexture("sprites/sword.png", false));
	}

	protected override void getAttackAnim(int idx, out AttackAnim anim, out int swingDir, out float startAngle, out float endAngle)
	{
		base.getAttackAnim(idx, out anim, out swingDir, out startAngle, out endAngle);
		anim = idx % 2 == 0 ? AttackAnim.SwingSideways : AttackAnim.Stab;
	}
}
