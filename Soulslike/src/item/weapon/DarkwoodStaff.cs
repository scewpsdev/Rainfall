using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DarkwoodStaff : Staff
{
	public DarkwoodStaff()
		: base("darkwood_staff", "Darkwood Staff")
	{
		castOrigin = new Vector3(0, 0.25f, 0);
	}
}
