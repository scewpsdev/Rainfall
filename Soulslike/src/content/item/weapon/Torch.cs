using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Torch : Weapon
{
	public Torch()
		: base("torch", "Torch")
	{
		damage = 5;

		initBlade(0.25f, 0.4f);
	}
}
