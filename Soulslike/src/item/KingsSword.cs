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

		addAttack(new AttackData("attack1", "attack2", 10, 18));
		addAttack(new AttackData("attack2", "attack1", 10, 18));
	}
}
