using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SpeedModifier : StatusEffect
{
	public SpeedModifier()
		: base("speed_modifier", new Sprite(tileset, 3, 1))
	{
	}
}
