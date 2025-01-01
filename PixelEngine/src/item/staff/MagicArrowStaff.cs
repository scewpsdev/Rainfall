using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MagicArrowStaff : Staff
{
	public MagicArrowStaff()
		: base("magic_arrow_staff")
	{
		displayName = "Magic Staff";

		value = 30;

		sprite = new Sprite(tileset, 2, 6);
		renderOffset.x = 0.4f;
	}
}
