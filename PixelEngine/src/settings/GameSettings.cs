using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum AimMode
{
	Directional,
	Crosshair
}

public static class GameSettings
{
	public static AimMode aimMode = AimMode.Directional;
}
