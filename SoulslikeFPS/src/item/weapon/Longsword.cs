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

		damage = 20;

		initBlade(0.1f, 1.0f);

		addAttack(new AttackData("attack1", "attack1", new Vector2i(15, 27), 40, "attack2", "heavy2"));
		addAttack(new AttackData("attack2", "attack2", new Vector2i(15, 27), 40, "attack1", "heavy1"));
		addAttack(new AttackData("heavy1", "heavy1", "heavy1_charge", new Vector2i(0, 17), 28, 15, "attack2", "heavy2"));
		addAttack(new AttackData("heavy2", "heavy2", "heavy2_charge", new Vector2i(0, 17), 26, 15, "attack1", "heavy1"));

		setBlockParams(false, 5);
		parryAttack = "attack2";
	}
}
