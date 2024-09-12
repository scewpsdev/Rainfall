using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Rainfall
{
	public static class Utils
	{
		public static T ParseEnum<T>(string identifier) where T : struct, Enum
		{
			foreach (T t in Enum.GetValues<T>())
			{
				if (t.ToString().ToLower() == identifier.ToLower())
					return t;
			}
			return default;
		}

		public static int RunCommand(string file, string args)
		{
			System.Diagnostics.Process process = System.Diagnostics.Process.Start(file, args);
			process.WaitForExit();
			return process.ExitCode;
		}
	}
}
