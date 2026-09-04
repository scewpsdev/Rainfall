using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


public class FieryPotion : Potion
{
	public FieryPotion()
		: base("fiery_potion", "Fiery Potion", PotionEffectType.Burn)
	{
	}
}
