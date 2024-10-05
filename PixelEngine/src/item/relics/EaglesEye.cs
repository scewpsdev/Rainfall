using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class EaglesEye : Item
{
	public EaglesEye()
		: base("eagles_eye", ItemType.Relic)
	{
		displayName = "Eagle's Eye";

		description = "Increases view distance";

		value = 30;

		sprite = new Sprite(tileset, 0, 4);
	}

	public override void onEquip(Player player)
	{
		player.aimDistance *= 2;
	}

	public override void onUnequip(Player player)
	{
		player.aimDistance /= 2;
	}
}
