using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LightningStaff : Staff
{
	public LightningStaff()
		: base("lightning_staff")
	{
		displayName = "Lightning Staff";

		value = 30;

		sprite = new Sprite(tileset, 8, 2);
		renderOffset.x = 0.4f;

		attuneSpell(0, new LightningSpell());
	}
}
