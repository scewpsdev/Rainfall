using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum ArmorSlot
{
	Head,
	Body,
	Hands,
	Feet,

	Count
}

public class Armor : Item
{
	public ArmorSlot armorSlot;


	public Armor(ArmorSlot armorSlot, string name, string displayName)
		: base(ItemType.Armor, name, displayName)
	{
		this.armorSlot = armorSlot;
	}
}
