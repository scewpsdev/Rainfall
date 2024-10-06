using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AttackSpeedModifier : StatusEffect
{
	public AttackSpeedModifier()
		: base("attack_speed_modifier", new Sprite(tileset, 3, 1))
	{
	}

	public override float getProgress()
	{
		return 1;
	}
}
