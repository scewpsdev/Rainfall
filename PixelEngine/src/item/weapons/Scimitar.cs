using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Scimitar : Weapon
{
	public Scimitar()
		: base("scimitar")
	{
		displayName = "Scimitar";

		baseDamage = 1.0f;
		baseAttackRange = 1.2f;
		baseAttackRate = 2.0f;
		attackCooldown = 0.3f;
		baseWeight = 1.5f;

		value = 16;

		sprite = new Sprite(tileset, 5, 3);
		renderOffset.x = 0.2f;
		renderOffset.y = -0.2f;
		//ingameSprite = new Sprite(Resource.GetTexture("sprites/sword.png", false));
	}

	protected override void getAttackAnim(Player player, int idx, out AttackAnim anim, out int swingDir, out float startAngle, out float endAngle, out float range)
	{
		base.getAttackAnim(player, idx, out anim, out swingDir, out startAngle, out endAngle, out range);
		if (idx % 2 == 0)
		{
			startAngle = 0.75f * MathF.PI;
			endAngle = -0.75f * MathF.PI;
			swingDir = 1;
		}
		else
		{
			startAngle = 0.25f * MathF.PI;
			endAngle = -1 * MathF.PI;
			swingDir = 1;
		}
	}
}
