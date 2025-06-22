using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Spear : Weapon
{
	public Spear()
		: base("spear", "Spear")
	{
		twoHanded = true;

		damage = 16;

		initBlade(0.0f, 1.0f, 0.6f, 1.0f);

		addAttack(new AttackData("attack1", "attack2", "heavy2", "attack1", new Vector2i(13, 22), 28, DamageType.Thrust));
		addAttack(new AttackData("attack2", "attack1", "heavy1", "attack2", new Vector2i(13, 22), 28, DamageType.Thrust));
		addAttack(new AttackData("heavy1", "attack2", "heavy2", "heavy1", "heavy1_charge", new Vector2i(0, 17), 28, 15));
		addAttack(new AttackData("heavy2", "attack1", "heavy1", "heavy2", "heavy2_charge", new Vector2i(0, 17), 26, 15));

		setParry(10);
		parryAttack = "attack2";
	}
}
