using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class StaffOfIllumination : Staff
{
	public StaffOfIllumination()
		: base("staff_of_illumination")
	{
		displayName = "Staff of Illumination";

		isSecondaryItem = true;

		baseDamage = 0;
		staffCharges = 10;
		maxStaffCharges = 10;

		value = 9;
		canDrop = false;

		sprite = new Sprite(tileset, 5, 4);
		renderOffset.x = 0.2f;

		attuneSpell(0, new IlluminationSpell());
	}

	public override void render(Entity entity)
	{
		Renderer.DrawLight(entity.position + entity.collider.center, MathHelper.ARGBToVector(0xFFffecb5).xyz * 2, 7);
	}
}
