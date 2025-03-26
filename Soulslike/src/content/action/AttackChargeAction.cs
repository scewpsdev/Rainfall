using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AttackChargeAction : PlayerAction
{
	Weapon weapon;
	AttackData attack;

	bool attackQueued = false;

	public AttackChargeAction(Weapon weapon, AttackData attack, int hand)
		: base("attack_charge", hand)
	{
		this.weapon = weapon;
		this.attack = attack;

		animationName[hand] = attack.chargeAnimation;
		animationSet[hand] = weapon.moveset;

		if (weapon.twoHanded)
		{
			animationName[hand ^ 1] = attack.chargeAnimation;
			animationSet[hand ^ 1] = weapon.moveset;
		}

		mirrorAnimation = hand == 1;

		followUpCancelTime = attack.chargeCancelFrame / 24.0f;

		viewmodelAim = 1;
		lockYaw = true;
		movementSpeedMultiplier = 0.5f;
	}

	public override void onStarted(Player player)
	{
		// play audio cue
	}

	public override void onFinished(Player player)
	{
		if (!attackQueued)
			player.actionManager.queueAction(new AttackAction(weapon, attack, hand, 1));
	}

	public override void update(Player player)
	{
		base.update(player);

		InputBinding input = InputManager.GetBinding(hand == 0 ? "Attack1" : "Attack2");
		if (!input.isDown())
		{
			player.actionManager.queueAction(new AttackAction(weapon, attack, hand, MathHelper.Remap(elapsedTime, followUpCancelTime, duration, 0, 1)));
			attackQueued = true;
		}
	}
}
