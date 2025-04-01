using System.Diagnostics;

namespace CpuPowerManagement.CLI
{
	public static class RunCli
	{
		public static string RunCommand(string arguments, bool readOutput, string processName = "cmd.exe", int waitExit = 6000, bool runAsAdmin = true)
		{
			try
			{
				var process = new Process();
				var startInfo = new ProcessStartInfo();
				startInfo.UseShellExecute = false;

				if (readOutput) { startInfo.RedirectStandardOutput = true; } else { startInfo.RedirectStandardOutput = false; }

				startInfo.FileName = processName;
				startInfo.Arguments = arguments;
				startInfo.CreateNoWindow = true;
				if (runAsAdmin) { startInfo.Verb = "runas"; }
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
			
        process.Close();
        return "COMPLETE";
			}
			catch (Exception ex)
			{
				var error = "Error running CLI: " + ex.Message + " " + arguments;

        Debug.WriteLine(error);
				return error;
			}
		}
	}
}
