using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BrokenSword : Weapon
{
	public BrokenSword()
		: base("broken_sword", "Broken Sword")
	{
		damage = 8;

		initBlade(0.1f, 0.5f);

		addAttack(new AttackData("attack1", "attack1", new Vector2i(10, 18), 18, "attack2"));
		addAttack(new AttackData("attack2", "attack2", new Vector2i(10, 18), 18, "attack1"));
		addAttack(new AttackData("riposte1", "attack_riposte1", new Vector2i(5, 13), 16));
		setBlockParams(true, 5);

		parryAttack = "riposte1";
	}
}
