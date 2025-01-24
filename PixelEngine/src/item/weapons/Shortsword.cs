using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Shortsword : Weapon
{
	public Shortsword()
		: base("shortsword")
	{
		displayName = "Shortsword";

		baseDamage = 1.25f;
		baseAttackRange = 1.0f;
		baseAttackRate = 2.2f;
		anim = AttackAnim.SwingSideways;
		baseWeight = 1;

		value = 9;

		sprite = new Sprite(tileset, 2, 1);
		renderOffset.x = 0.2f;
		//ingameSprite = new Sprite(Resource.GetTexture("sprites/items/weapon/shortsword.png", false), 0, 0, 32, 32);
		//ingameSpriteSize = 2;
		//ingameSpriteLayer = Entity.LAYER_PLAYER_ITEM_SECONDARY;
	}

	protected override void getAttackAnim(int idx, out AttackAnim anim, out int swingDir, out float startAngle, out float endAngle)
	{
		base.getAttackAnim(idx, out anim, out swingDir, out startAngle, out endAngle);

		swingDir = 0;
		if (idx % 3 == 0)
		{
			startAngle = 0.75f * MathF.PI;
			endAngle = -0.75f * MathF.PI;
		}
		else if (idx % 3 == 1)
		{
			startAngle = MathF.PI;
			endAngle = -0.25f * MathF.PI;
		}
		else
		{
			anim = AttackAnim.Stab;
		}
	}
}
