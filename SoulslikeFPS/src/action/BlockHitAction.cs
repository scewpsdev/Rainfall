using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BlockHitAction : FirstPersonAction
{
	public Weapon weapon;

	public BlockHitAction(Weapon weapon, int hand)
		: base("parry_hit", hand)
	{
		this.weapon = weapon;

		animationName[hand] = "parry_hit";
		animationSet[hand] = weapon.moveset;

		if (weapon.twoHanded)
		{
			animationName[hand ^ 1] = "parry_hit";
			animationSet[hand ^ 1] = weapon.moveset;
		}

		mirrorAnimation = hand == 1;

		animationTransitionDuration = 1 / 24.0f;
		followUpCancelTime = 13 / 24.0f; // weapon.parryWindow;

		viewmodelAim = 1;

		addSoundEffect(new ActionSfx(weapon.blockSound));
	}

	public override void update(Player player)
	{
		base.update(player);

		if (Input.IsMouseButtonDown(MouseButton.Right) && elapsedTime > followUpCancelTime)
		{
			player.actionManager.setAction(new BlockAction(weapon, hand) { elapsedTime = 10, animationTransitionDuration = 0.4f });
		}
	}
}
