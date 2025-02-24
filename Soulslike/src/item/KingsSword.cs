using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class KingsSword : Weapon
{
	public KingsSword()
		: base("kings_sword", "King's Sword")
	{
		twoHanded = true;
	}
}
