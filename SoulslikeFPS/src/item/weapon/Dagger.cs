using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Dagger : Weapon
{
	public Dagger()
		: base("dagger", "Steel Dagger")
	{
		initBlade(0.05f, 0.4f);

		damage = 10;

		addAttack(new AttackData("attack1", "attack1", new Vector2i(5, 15), 20, "attack2", null, DamageType.Thrust));
		addAttack(new AttackData("attack2", "attack2", new Vector2i(5, 15), 20, "attack1", null, DamageType.Thrust));

		setBlockParams(true, 5);
		parryAttack = "attack2";
	}
}
