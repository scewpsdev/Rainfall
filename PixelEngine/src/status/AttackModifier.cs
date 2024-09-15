using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AttackModifier : StatusEffect
{
	public AttackModifier()
		: base("attack_modifier", new Sprite(tileset, 3, 0))
	{
	}
}
