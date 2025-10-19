using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum ArmorType
{
	Helmet,
	Body,
	Gloves,
	Boots,
}

public class Armor : Item
{
	public ArmorType armorType;


	public Armor(ArmorType armorType, string name, string displayName)
		: base(ItemType.Armor, name, displayName)
	{
		this.armorType = armorType;
	}
}
