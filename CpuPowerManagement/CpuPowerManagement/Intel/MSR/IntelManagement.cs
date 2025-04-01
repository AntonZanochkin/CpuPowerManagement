using CpuPowerManagement.CLI;
using System.IO;
using System.Windows;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CpuPowerManagement.Intel.MSR
{
  public static class IntelManagement
  {
    public static void SetPl1(int pl1TDP, int pl2TDP)
    {
      try
      {
        var hexPL1 = ConvertTdpToHexMsr(pl1TDP);
        var hexPL2 = ConvertTdpToHexMsr(pl2TDP);

        hexPL1 = FormatHex(hexPL1);
        hexPL2 = FormatHex(hexPL2);

        var folderPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty;
        var commandArguments = "-s write 0x610 0x00438" + hexPL2 + " 0x00dd8" + hexPL1;
        var processMSR = Path.Combine(folderPath, "Assets\\Intel\\MSR\\msr-cmd.exe");

        RunCli.RunCommand(commandArguments, false, processMSR);
      }
      catch (Exception ex) { MessageBox.Show(ex.ToString()); }
    }

    public static MsrPowerLimit GetPowerLimits()
    {
      try
      {
        var folderPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty;
        var processPath = Path.Combine(folderPath, "Assets\\Intel\\MSR\\msr-cmd.exe");

        // Read MSR 0x610
        var output = RunCli.RunCommand("read 0x610", true, processPath);
        var msrValue = GetMsrValue(output);

        Console.WriteLine($"Raw MSR 0x610: 0x{msrValue:X16}");

        if (msrValue == 0)
        {
          MessageBox.Show("Failed to read MSR 0x610.");
          return null;
        }

        // Get Power Unit from MSR 0x606
        var powerUnit = GetPowerUnit();

        // Extract PL1 (Bits 14:0) and convert to watts
        var pl1Raw = (int)(msrValue & 0x7FFF);
        var pl1Watts = pl1Raw * powerUnit;

        // Extract PL2 (Bits 46:32) and convert to watts
        var pl2Raw = (int)((msrValue >> 32) & 0x7FFF);
        var pl2Watts = pl2Raw * powerUnit;

        // Extract PL1 Time Window (Bits 23:17)
        var pl1TimeRaw = (int)((msrValue >> 17) & 0x7F);
        var pl1TimeWindow = ConvertTimeWindow(pl1TimeRaw);

        // Extract PL2 Time Window (Bits 54:48)
        var pl2TimeRaw = (int)((msrValue >> 48) & 0x3F);
        var pl2TimeWindow = ConvertTimeWindow(pl2TimeRaw);

        // Power Limit Enable Flags
        var pl1Enabled = (msrValue & (1UL << 30)) != 0;
        var pl2Enabled = (msrValue & (1UL << 60)) != 0;

        // Create and return the MsrPowerLimit object
        return new MsrPowerLimit
        {
          PL1_Watts = pl1Watts,
          PL2_Watts = pl2Watts,
          PL1_TimeWindow = pl1TimeWindow,
          PL2_TimeWindow = pl2TimeWindow,
          PL1_Enabled = pl1Enabled,
          PL2_Enabled = pl2Enabled
        };
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Error: {ex.Message}\n{ex.StackTrace}");
        return null;
      }
    }

    private static string ConvertTdpToHexMsr(int tdp)
    {
      var tpdHex = tdp * 8;
      return tpdHex.ToString("X");
    }

    private static string FormatHex(string hexValue) 
    {
      return hexValue.PadLeft(3, '0');
    }

    private static double ConvertTimeWindow(int rawTime)
    {
      var multiplier = (rawTime & 0x3) + 1;
      var exponent = (rawTime >> 3) & 0x1F;
      return multiplier * Math.Pow(2, exponent);
    }

    private static ulong GetMsrValue(string msrOutput)
    {
      var regex = new Regex(@"0x(?<reg>[0-9A-Fa-f]+)\s+0x(?<edx>[0-9A-Fa-f]+)\s+0x(?<eax>[0-9A-Fa-f]+)");
      var match = regex.Match(msrOutput);

      if (!match.Success) return 0; // Return 0 on failures

      var edx = Convert.ToUInt64(match.Groups["edx"].Value, 16);
      var eax = Convert.ToUInt64(match.Groups["eax"].Value, 16);
      return (edx << 32) | eax; // Combine EDX and EAX to form a 64-bit MSR value

    }

    public static double GetPowerUnit()
    {
      var folderPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty; 
      var processPath = Path.Combine(folderPath, "Assets\\Intel\\MSR\\msr-cmd.exe");

      var output = RunCli.RunCommand("read 0x606", true, processPath);
      var msrValue = GetMsrValue(output);

      if (msrValue == 0)
      {
        Console.WriteLine("Failed to read MSR 0x606. Using default power unit.");
        return 0.125;  // Default = 1/8 W
      }

      // Bits 3:0 in MSR 0x606 define Power Units
      var powerUnitRaw = (int)(msrValue & 0xF);
      return 1.0 / Math.Pow(2, powerUnitRaw);
      }
    }

}
