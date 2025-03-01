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
		//twoHanded = true;

		initBlade(0.05f, 0.4f);

		//addAttack(new AttackData("attack1", "attack2", null, "attack1", new Vector2i(10, 18), 18));
		//addAttack(new AttackData("attack2", "attack1", null, "attack2", new Vector2i(10, 18), 18));
	}
}
