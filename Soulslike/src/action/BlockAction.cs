using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BlockAction : PlayerAction
{
	public Weapon weapon;


	public BlockAction(Weapon weapon, int hand)
		: base("block")
	{
		this.weapon = weapon;

		animationName = "block";
		animationData = weapon.moveset;

		lockRotation = true;

		canJump = false;

		overrideRotationLockStartTime = 0.0f;
		overrideRotationLockEndTime = weapon.parryWindow * 0.5f;

		duration = 1000;
	}

	public override void onStarted(Player player)
	{
	}

	public override void onFinished(Player player)
	{
		player.parryItem = null;
	}

	public override void update(Player player)
	{
		base.update(player);

		player.parryItem = elapsedTime < weapon.parryWindow ? weapon : null;
		if (!Input.IsMouseButtonDown(MouseButton.Right) && elapsedTime > weapon.parryWindow)
			player.actionManager.cancelAction();
	}

	public override void draw(Player player)
	{
	}
}
