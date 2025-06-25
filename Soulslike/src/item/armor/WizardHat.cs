using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WizardHat : Armor
{
	public WizardHat()
		: base(ArmorSlot.Head, "wizard_hat", "Wizard Hat")
	{
		model = Resource.GetModel("item/armor/wizard_hat/wizard_hat.gltf");
	}
}
