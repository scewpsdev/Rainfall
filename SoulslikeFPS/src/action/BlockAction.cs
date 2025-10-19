using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BlockAction : FirstPersonAction
{
	public Weapon weapon;

	public BlockAction(Weapon weapon, int hand)
		: base("block", hand)
	{
		this.weapon = weapon;

		animationName[hand] = "parry";
		animationSet[hand] = weapon.moveset;

		if (weapon.twoHanded)
		{
			animationName[hand ^ 1] = "parry";
			animationSet[hand ^ 1] = weapon.moveset;
		}

		mirrorAnimation = hand == 1;

		if (weapon.canParry)
			followUpCancelTime = weapon.parryWindow;

		//lockYaw = true;

		viewmodelAim = 1;

		if (weapon.canBlock)
			duration = 1000;
	}

	public override void onStarted(Player player)
	{
	}

	public override void onFinished(Player player)
	{
		player.parryItem = null;
		player.blockItem = null;
	}

	public override void update(Player player)
	{
		base.update(player);

		if (elapsedTime < weapon.parryWindow && weapon.canParry)
		{
			player.parryItem = weapon;
			player.parryItemHand = hand;
		}
		else
		{
			player.parryItem = null;
		}

		if (elapsedTime >= weapon.parryWindow && weapon.canBlock)
		{
			player.blockItem = weapon;
			player.blockItemHand = hand;
		}
		else
		{
			player.blockItem = null;
		}

		if (!Input.IsMouseButtonDown(MouseButton.Right) && elapsedTime > weapon.parryWindow)
			player.actionManager.cancelAction();
	}

	public override void draw(Player player)
	{
	}
}
