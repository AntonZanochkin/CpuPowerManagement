using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CpuPowerManagement.CLI
{
	public static class RunCli
	{
		public static string RunCommand(string arguments, bool readOutput, string processName = "cmd.exe", int waitExit = 6000, bool runasadmin = true)
		{
			try
			{
				System.Diagnostics.Process process = new System.Diagnostics.Process();
				System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
				startInfo.UseShellExecute = false;
				if (readOutput) { startInfo.RedirectStandardOutput = true; } else { startInfo.RedirectStandardOutput = false; }

				startInfo.FileName = processName;
				startInfo.Arguments = arguments;
				startInfo.CreateNoWindow = true;
				if (runasadmin) { startInfo.Verb = "runas"; }
				startInfo.RedirectStandardError = readOutput;
				startInfo.RedirectStandardOutput = readOutput;

				process.EnableRaisingEvents = true;
				process.StartInfo = startInfo;
				process.Start();

				process.WaitForExit(waitExit);
				if (readOutput)
				{
					string output = process.StandardOutput.ReadToEnd();
					process.Close();
					return output;
				}
				else
				{
					process.Close();
					return "COMPLETE";
				}
			}
			catch (Exception ex)
			{
				string error = "Error running CLI: " + ex.Message + " " + arguments;
				return error;
			}
		}
	}
}
