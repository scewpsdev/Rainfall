using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class UISound
{
	public static Sound uiClick;
	public static Sound uiSwitch;
	public static Sound uiConfirm;
	public static Sound uiConfirm2;
	public static Sound uiBack;

	static UISound()
	{
		uiClick = Resource.GetSound("sounds/ui_back.ogg");
		uiSwitch = Resource.GetSound("sounds/ui_switch.ogg");
		uiConfirm = Resource.GetSound("sounds/ui_confirm.ogg");
		uiConfirm2 = Resource.GetSound("sounds/ui_confirm2.ogg");
		uiBack = Resource.GetSound("sounds/ui_back.ogg");

		//uiClick.singleInstance = true;
		//uiSwitch.singleInstance = true;
		//uiConfirm.singleInstance = true;
		//uiConfirm2.singleInstance = true;
		//uiBack.singleInstance = true;
	}
}
