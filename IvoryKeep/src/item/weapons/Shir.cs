using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Shir : Weapon
{
	public Shir()
		: base("shir")
	{
		displayName = "Shir";

		baseDamage = 1.8f;
		baseAttackRange = 1.2f;
		baseAttackRate = 1;
		attackCooldown = 1.2f;
		baseWeight = 2.5f;

		strengthScaling = 0.5f;
		dexterityScaling = 0.7f;

		value = 26;

		sprite = new Sprite(tileset, 6, 10, 2, 1);
		icon = new Sprite(tileset, 6, 10);
		size = new Vector2(2, 1);
		renderOffset.x = 0.6f;
		renderOffset.y = -0.1f;
		//ingameSprite = new Sprite(Resource.GetTexture("sprites/sword.png", false));
	}

	protected override void getAttackAnim(Player player, int idx, out AttackAnim anim, out int swingDir, out float startAngle, out float endAngle, out float range)
	{
		base.getAttackAnim(player, idx, out anim, out swingDir, out startAngle, out endAngle, out range);
		if (idx % 2 == 0)
		{
			//anim = AttackAnim.SwingOverhead;
			startAngle = MathF.PI * 0.75f;
			endAngle = MathF.PI * -0.75f;
		}
		else
		{
			startAngle = MathF.PI * 0.5f;
			endAngle = MathF.PI * -1.0f;
		}
	}
}
