using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public struct AttackData
{
	public string name;
	public string nextAttack;
	public int cancelFrame;

	public AttackData(string name, string nextAttack, int cancelFrame)
	{
		this.name = name;
		this.nextAttack = nextAttack;
		this.cancelFrame = cancelFrame;
	}
}

public class Weapon : Item
{
	List<AttackData> attacks = new List<AttackData>();
	Dictionary<string, int> nameMap = new Dictionary<string, int>();


	public Weapon(string name, string displayName)
		: base(ItemType.Weapon, name, displayName)
	{
	}

	protected void addAttack(AttackData attack)
	{
		attacks.Add(attack);
		nameMap.Add(attack.name, attacks.Count - 1);
	}

	public override void use(Player player, int hand)
	{
		if (attacks.Count > 0)
		{
			int nextAttack = 0;
			if (player.actionManager.currentAction != null && player.actionManager.currentAction is AttackAction)
				nextAttack = nameMap[(player.actionManager.currentAction as AttackAction).attack.nextAttack];
			player.actionManager.queueAction(new AttackAction(this, attacks[nextAttack], hand));
		}
	}
}
