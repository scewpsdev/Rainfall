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
		//isSecondaryItem = true;
		secondaryChargeTime = 0;
		knockback = 2.0f;
		twoHanded = true;

		value = 75;

		sprite = new Sprite(tileset, 5, 7, 2, 1);
		size = new Vector2(2, 1);
		renderOffset.x = 0.4f;

		castSound = Resource.GetSounds("sounds/cast", 3);
	}

	public override bool useSecondary(Player player)
	{
		int attackIdx = 0;
		if (player.actions.currentAction != null && player.actions.currentAction is AttackAction && (player.actions.currentAction as AttackAction).weapon == this)
			attackIdx = (player.actions.currentAction as AttackAction).attackIdx + 1;
		bool stab = attackIdx % 2 == 0;
		player.actions.queueAction(new AttackAction(this, player.handItem == this, stab, 2.0f, 1.5f, 1.2f));
		return false;
	}
}
