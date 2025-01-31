using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class QuestlineLoganStaff : ElderwoodStaff
{
	public QuestlineLoganStaff()
	{
		name = "questline_logan_staff";
		displayName = "Gatekeeper's Spectral Staff";
		description = "A long forgotten staff once wielded by one of the royal knights. What is it doing in these catacombs?";

		canDrop = false;
		value = 1000;
	}
}
