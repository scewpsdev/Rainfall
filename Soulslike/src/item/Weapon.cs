using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public struct AttackData
{
	public string name;
	public string nextAttack;
	public int damageFrame;
	public int cancelFrame;

	public AttackData(string name, string nextAttack, int damageFrame, int cancelFrame)
	{
		this.name = name;
		this.nextAttack = nextAttack;
		this.damageFrame = damageFrame;
		this.cancelFrame = cancelFrame;
	}
}

public class Weapon : Item
{
	List<AttackData> attacks = new List<AttackData>();
	Dictionary<string, int> attackNameMap = new Dictionary<string, int>();

	bool canParry = false;
	public float parryWindow = 0;


	public Weapon(string name, string displayName)
		: base(ItemType.Weapon, name, displayName)
	{
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

	public override void use(Player player, int hand)
	{
		if (attacks.Count > 0)
		{
			int nextAttack = 0;
			if (player.actionManager.currentAction != null && player.actionManager.currentAction is AttackAction)
				nextAttack = attackNameMap[(player.actionManager.currentAction as AttackAction).attack.nextAttack];
			player.actionManager.queueAction(new AttackAction(this, attacks[nextAttack], hand));
		}
	}

	public override void useSecondary(Player player, int hand)
	{
		if (canParry)
			player.actionManager.queueAction(new ParryAction(this, hand));
	}
}
