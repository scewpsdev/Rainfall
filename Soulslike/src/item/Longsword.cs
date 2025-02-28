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

		initBlade(0.1f, 1.0f);

		addAttack(new AttackData("attack1", "attack2", "heavy2", "attack1", 15, 40));
		addAttack(new AttackData("attack2", "attack1", "heavy1", "attack2", 15, 40));
		addAttack(new AttackData("heavy1", "attack2", "heavy2", "heavy1", "heavy1_charge", 0, 28, 15));
		addAttack(new AttackData("heavy2", "attack1", "heavy1", "heavy2", "heavy2_charge", 0, 26, 15));
		setParry(15);
	}
}
