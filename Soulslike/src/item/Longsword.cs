using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Longsword : Weapon
{
	public Longsword()
		: base("longsword", "Longsword")
	{
		twoHanded = true;

		addAttack(new AttackData("attack1", "attack2", 15, 40));
		addAttack(new AttackData("attack2", "attack1", 15, 40));
		setParry(15);
	}
}
