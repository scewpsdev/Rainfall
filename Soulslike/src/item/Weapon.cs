using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum DamageType
{
	Slash,
	Thrust,
	Blunt,
	Strike,
	Projectile,
	Magic,
	Fire,
}

public struct AttackData
{
	public string name;
	public string nextAttack;
	public string nextCharged;
	public string animation;
	public string chargeAnimation;
	public Vector2i damageRange;
	public int cancelFrame;
	public int chargeCancelFrame;
	public DamageType damageType;

	public AttackData(string name, string nextAttack, string nextCharged, string animation, string chargeAnimation, Vector2i damageRange, int cancelFrame, int chargeCancelFrame, DamageType damageType = DamageType.Slash)
	{
		this.name = name;
		this.nextAttack = nextAttack;
		this.nextCharged = nextCharged;
		this.animation = animation;
		this.chargeAnimation = chargeAnimation;
		this.damageRange = damageRange;
		this.cancelFrame = cancelFrame;
		this.chargeCancelFrame = chargeCancelFrame;
		this.damageType = damageType;
	}

	public AttackData(string name, string nextAttack, string nextCharged, string animation, Vector2i damageRange, int cancelFrame, DamageType damageType = DamageType.Slash)
	{
		this.name = name;
		this.nextAttack = nextAttack;
		this.nextCharged = nextCharged;
		this.animation = animation;
		this.damageRange = damageRange;
		this.cancelFrame = cancelFrame;
		this.damageType = damageType;
	}
}

public class Weapon : Item
{
	List<AttackData> attacks = new List<AttackData>();
	Dictionary<string, int> attackNameMap = new Dictionary<string, int>();

	public string parryAttack;

	bool canParry = false;
	public float parryWindow = 0;

	public Vector3 bladeBase, bladeTip;


	public Weapon(string name, string displayName)
		: base(ItemType.Weapon, name, displayName)
	{
	}

	protected void initBlade(float basePoint, float tipPoint)
	{
		bladeBase = new Vector3(0, basePoint, 0);
		bladeTip = new Vector3(0, tipPoint, 0);
		sfxSourcePosition = bladeTip;
	}

	protected void addAttack(AttackData attack)
	{
		attacks.Add(attack);
		attackNameMap.Add(attack.name, attacks.Count - 1);
	}

	protected void setParry(int window)
	{
		canParry = true;
		parryWindow = window / 24.0f;
	}

	bool getFirstAttack(out int idx)
	{
		for (int i = 0; i < attacks.Count; i++)
		{
			if (attacks[i].chargeAnimation == null)
			{
				idx = i;
				return true;
			}
		}
		idx = -1;
		return false;
	}

	bool getFirstChargedAttack(out int idx)
	{
		for (int i = 0; i < attacks.Count; i++)
		{
			if (attacks[i].chargeAnimation != null)
			{
				idx = i;
				return true;
			}
		}
		idx = -1;
		return false;
	}

	public override void use(Player player, int hand)
	{
		if (getFirstAttack(out int nextAttack))
		{
			if (player.actionManager.currentAction != null && player.actionManager.currentAction is AttackAction)
			{
				nextAttack = attackNameMap[(player.actionManager.currentAction as AttackAction).attack.nextAttack];
				lastCancelledAttack = player.actionManager.currentAction;
			}
			else if (player.actionManager.currentAction != null && player.actionManager.currentAction is ParryHitAction && parryAttack != null)
			{
				nextAttack = attackNameMap[parryAttack];
			}

			player.actionManager.queueAction(new AttackAction(this, attacks[nextAttack], hand));
		}
	}

	PlayerAction lastCancelledAttack;

	public override void useCharged(Player player, int hand)
	{
		if (getFirstChargedAttack(out int nextAttack))
		{
			/*
			Debug.Assert(player.actionManager.actionQueue.Count > 0);
			PlayerAction lastQueuedAction = player.actionManager.actionQueue[player.actionManager.actionQueue.Count - 1];
			Debug.Assert(lastQueuedAction is AttackAction);
			if (lastQueuedAction.hasStarted)
				lastQueuedAction.cancel();
			else
				player.actionManager.actionQueue.RemoveAt(player.actionManager.actionQueue.Count - 1);

			if (player.actionManager.currentAction != null && player.actionManager.currentAction != lastQueuedAction && player.actionManager.currentAction is AttackAction && (player.actionManager.currentAction as AttackAction).attack.nextCharged != null)
				nextAttack = attackNameMap[(player.actionManager.currentAction as AttackAction).attack.nextCharged];
			else if (lastCancelledAttack != null && (Time.currentTime - lastCancelledAttack.startTime) / 1e9f < lastCancelledAttack.duration / lastCancelledAttack.animationSpeed && lastCancelledAttack is AttackAction && (lastCancelledAttack as AttackAction).attack.nextCharged != null)
				nextAttack = attackNameMap[(lastCancelledAttack as AttackAction).attack.nextCharged];

			AttackChargeAction chargeAction = new AttackChargeAction(this, attacks[nextAttack], hand);
			player.actionManager.queueAction(chargeAction);
			if (lastQueuedAction.hasStarted)
				chargeAction.animationTransitionDuration = 0;
			*/

			if (player.actionManager.currentAction != null && player.actionManager.currentAction is AttackAction && (player.actionManager.currentAction as AttackAction).attack.nextCharged != null)
				nextAttack = attackNameMap[(player.actionManager.currentAction as AttackAction).attack.nextCharged];
			else if (lastCancelledAttack != null && (Time.currentTime - lastCancelledAttack.startTime) / 1e9f < lastCancelledAttack.duration / lastCancelledAttack.animationSpeed && lastCancelledAttack is AttackAction && (lastCancelledAttack as AttackAction).attack.nextCharged != null)
				nextAttack = attackNameMap[(lastCancelledAttack as AttackAction).attack.nextCharged];

			AttackChargeAction chargeAction = new AttackChargeAction(this, attacks[nextAttack], hand);
			player.actionManager.queueAction(chargeAction);
			//if (lastQueuedAction.hasStarted)
			//	chargeAction.animationTransitionDuration = 0;
		}
		else
		{
			use(player, hand);
		}
	}

	public override void useSecondary(Player player, int hand)
	{
		if (canParry)
			player.actionManager.queueAction(new ParryAction(this, hand));
	}
}
