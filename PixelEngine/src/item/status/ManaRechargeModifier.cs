using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ManaRechargeModifier : StatusEffect
{
	public ManaRechargeModifier()
		: base("mana_recharge_modifier", new Sprite(tileset, 0, 1))
	{
		iconColor = 0xFF76adff;
	}

	public override float getProgress()
	{
		return 1;
	}
}
