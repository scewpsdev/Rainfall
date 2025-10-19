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

		static Dictionary<uint, long> timers = new Dictionary<uint, long>();

		public static bool RunEverySeconds(float seconds, string sid)
		{
			uint h = Hash.hash(sid);
			if (timers.ContainsKey(h))
			{
				long lastInvoke = timers[h];
				if ((Time.currentTime - lastInvoke) / 1e9f >= seconds)
				{
					timers[h] = Time.currentTime;
					return true;
				}
				return false;
			}
			else
			{
				timers.Add(h, Time.currentTime);
				return true;
			}
		}
	}
}
