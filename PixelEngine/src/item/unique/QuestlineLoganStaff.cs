using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class QuestlineLoganStaff : Item
{
	public QuestlineLoganStaff()
		: base("questline_logan_staff", ItemType.Relic)
	{
		displayName = "Gatekeeper's Spectral Staff";

		canDrop = false;
		value = 1000;

		sprite = new Sprite(tileset, 0, 0);
	}
}
