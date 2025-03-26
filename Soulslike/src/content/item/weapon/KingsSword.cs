using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class KingsSword : Weapon
{
	public KingsSword()
		: base("kings_sword", "King's Sword")
	{
		//twoHanded = true;

		damage = 15;

		initBlade(0.1f, 0.85f);

		addAttack(new AttackData("attack1", "attack2", null, "attack1", new Vector2i(10, 18), 18));
		addAttack(new AttackData("attack2", "attack1", null, "attack2", new Vector2i(10, 18), 18));
		setParry(10);

		parryAttack = "attack1";
	}
}
