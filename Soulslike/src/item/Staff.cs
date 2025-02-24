using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Staff : Item
{
	public Vector3 castOrigin;


	public Staff(string name, string displayName)
		: base(ItemType.Weapon, name, displayName)
	{
		moveset = Resource.GetModel("item/staff_moveset.gltf");
	}

	public override void use(Player player, int hand)
	{
		player.actionManager.queueAction(new SpellCastAction(this));
	}
}
