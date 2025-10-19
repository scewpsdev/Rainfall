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

	public AttackData(string name, string animation, string chargeAnimation, Vector2i damageRange, int cancelFrame, int chargeCancelFrame, string nextAttack = null, string nextCharged = null, DamageType damageType = DamageType.Slash)
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

	public AttackData(string name, string animation, Vector2i damageRange, int cancelFrame, string nextAttack = null, string nextCharged = null, DamageType damageType = DamageType.Slash)
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

	public bool canParry = false;
	public bool canBlock = false;
	public float parryWindow = 0;
	public string parryAttack;

	public Vector3 bladeBase, bladeTip;
	public Vector2 bladeEffectiveRange;


	public Weapon(string name, string displayName)
		: base(ItemType.Weapon, name, displayName)
	{
		equipSound = [equipLight];
		hitSound = hitWeapon;
		blockSound = blockWeapon;
	}

	protected void initBlade(float basePoint, float tipPoint)
	{
		bladeBase = new Vector3(0, basePoint, 0);
		bladeTip = new Vector3(0, tipPoint, 0);
		bladeEffectiveRange = new Vector2(basePoint, tipPoint);
		sfxSourcePosition = new Vector3(0, Mathf.Lerp(basePoint, tipPoint, 0.25f), 0);
	}

	protected void initBlade(float basePoint, float tipPoint, float effectiveBasePoint, float effectiveTipPoint)
	{
		bladeBase = new Vector3(0, basePoint, 0);
		bladeTip = new Vector3(0, tipPoint, 0);
		bladeEffectiveRange = new Vector2(effectiveBasePoint, effectiveTipPoint);
		sfxSourcePosition = new Vector3(0, Mathf.Lerp(effectiveBasePoint, effectiveTipPoint, 0.25f), 0);
	}

	protected void addAttack(AttackData attack)
	{
		attacks.Add(attack);
		attackNameMap.Add(attack.name, attacks.Count - 1);
	}

	protected void setBlockParams(bool canBlock = true, int parryWindow = 0)
	{
		this.canBlock = canBlock;
		if (parryWindow != 0)
		{
			canParry = true;
			this.parryWindow = parryWindow / 24.0f;
		}
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
				AttackAction attackAction = player.actionManager.currentAction as AttackAction;
				if (attackAction.attack.nextAttack != null)
				{
					nextAttack = attackNameMap[(player.actionManager.currentAction as AttackAction).attack.nextAttack];
					lastCancelledAttack = player.actionManager.currentAction;
				}
			}
			else if (player.actionManager.currentAction != null && player.actionManager.currentAction is ParryHitAction && parryAttack != null)
			{
				nextAttack = attackNameMap[parryAttack];
				//player.actionManager.cancelAction();
			}

			player.actionManager.queueAction(new AttackAction(this, attacks[nextAttack], hand));
		}
	}

	FirstPersonAction lastCancelledAttack;

	public override void useCharged(Player player, int hand)
	{
		if (getFirstChargedAttack(out int nextAttack))
		{
			Debug.Assert(player.actionManager.actionQueue.Count > 0);
			FirstPersonAction lastQueuedAction = player.actionManager.actionQueue[player.actionManager.actionQueue.Count - 1];
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

			//if (player.actionManager.currentAction != null && player.actionManager.currentAction is AttackAction && (player.actionManager.currentAction as AttackAction).attack.nextCharged != null)
			//	nextAttack = attackNameMap[(player.actionManager.currentAction as AttackAction).attack.nextCharged];
			//else if (lastCancelledAttack != null && (Time.currentTime - lastCancelledAttack.startTime) / 1e9f < lastCancelledAttack.duration / lastCancelledAttack.animationSpeed && lastCancelledAttack is AttackAction && (lastCancelledAttack as AttackAction).attack.nextCharged != null)
			//	nextAttack = attackNameMap[(lastCancelledAttack as AttackAction).attack.nextCharged];

			//AttackChargeAction chargeAction = new AttackChargeAction(this, attacks[nextAttack], hand);
			//player.actionManager.queueAction(chargeAction);
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
		if (canBlock || canParry)
			player.actionManager.queueAction(new BlockAction(this, hand));
	}
}
