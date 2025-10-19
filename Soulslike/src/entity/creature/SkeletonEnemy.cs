using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SkeletonEnemy : Creature
{
	public SkeletonEnemy()
		: base("skeleton")
	{
		addAttack(new CreatureAttack("attack1", "attack1", new Vector2i(16, 30), 35, DamageType.Strike, "attack2"));
		addAttack(new CreatureAttack("attack2", "attack2", new Vector2i(16, 30), 35, DamageType.Strike, "attack1"));

		ai = new CreatureAI(this);
	}
}
