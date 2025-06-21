using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Hollow : Creature
{
	public Hollow()
		: base("hollow")
	{
		addAttack(new CreatureAttack("attack1", null, "attack1", new Vector2i(16, 30), 35, DamageType.Strike));

		ai = new SimpleAI(this);
	}
}
