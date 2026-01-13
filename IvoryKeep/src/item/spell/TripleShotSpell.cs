using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TripleShotSpell : MagicArrow
{
	public TripleShotSpell()
	{
		name = "triple_shot";
		displayName = "Triple Shot";
		description = "Discharges a wide spread of magic bolts at close range.";

		bulletCount = 3;

		baseValue = 19;
		baseDamage = 0.7f;

		baseAttackRate = 1;
		manaCost *= 3;

		spellIcon = new Sprite(tileset, 5, 8);
	}
}
