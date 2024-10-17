using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class ControlsSettings
{
	public static void SetBinding(string name, string value)
	{
		InputBinding binding = InputManager.GetBinding(name);

	}
}
