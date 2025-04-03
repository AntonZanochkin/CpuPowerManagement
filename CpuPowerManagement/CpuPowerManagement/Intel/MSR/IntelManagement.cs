using System.Diagnostics;
using CpuPowerManagement.CLI;
using System.IO;
using System.Windows;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CpuPowerManagement.Intel.MSR
{
  public static class IntelManagement
  {
    public static void SetPl(int pl1Limit, int pl2Limit)
    {
      try
      {
        var hexPl1 = ConvertTdpToHexMsr(pl1Limit);
        var hexPl2 = ConvertTdpToHexMsr(pl2Limit);

        hexPl1 = FormatHex(hexPl1);
        hexPl2 = FormatHex(hexPl2);

        var folderPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty;
        var commandArguments = "-s write 0x610 0x00438" + hexPl2 + " 0x00dd8" + hexPl1;
        var processMsr = Path.Combine(folderPath, "Assets\\Intel\\MSR\\msr-cmd.exe");

        RunCli.RunCommand(commandArguments, false, processMsr);
      }
      catch (Exception ex) { MessageBox.Show(ex.ToString()); }
    }

    public static void SetPl2(int pl)
    {
      try
      {
        var hexPl = ConvertTdpToHexMsr(pl);
        hexPl = FormatHex(hexPl); ;

        var folderPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty;
        var commandArguments = "-s write 0x610 0x00438" + hexPl;
        var processMSR = Path.Combine(folderPath, "Assets\\Intel\\MSR\\msr-cmd.exe");

        RunCli.RunCommand(commandArguments, false, processMSR);
      }
      catch (Exception ex) { MessageBox.Show(ex.ToString()); }
    }

    public static PowerLimit GetPowerLimits()
    {
      try
      { 
        var folderPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty;
        var processPath = Path.Combine(folderPath, "Assets\\Intel\\MSR\\msr-cmd.exe");

        // Read MSR 0x610
        var output = RunCli.RunCommand("read 0x610", true, processPath);
        var msrValue = GetMsrValue(output);

        //ulong newValue = msrValue | (1UL << 30); // Force-enable PL1 (Bit 30)
        //RunCli.RunCommand($"-s write 0x610 0x{newValue:X16}", true, processPath);

       // Console.WriteLine($"Raw MSR 0x610: 0x{msrValue:X16}");

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

        // Extract PL1 Time Window (Bits 23:22 for X, Bits 21:17 for Y)
        var pl1X = (int)((msrValue >> 22) & 0x3); // Extract X (Bits 23:22)
        var pl1Y = (int)((msrValue >> 17) & 0x1F); // Extract Y (Bits 21:17)
        var pl1TimeWindow = ConvertTimeWindow(pl1X, pl1Y);

        // Extract PL2 Time Window (Bits 54:53 for X, Bits 52:48 for Y)
        var pl2X = (int)((msrValue >> 53) & 0x3); // Extract X (Bits 54:53)
        var pl2Y = (int)((msrValue >> 48) & 0x1F); // Extract Y (Bits 52:48)
        var pl2TimeWindow = ConvertTimeWindow(pl2X, pl2Y);

        // ✅ Corrected PL1 & PL2 Enable Bits
        var pl1Enabled = (msrValue & (1UL << 15)) != 0; // Bit 15
        var pl2Enabled = (msrValue & (1UL << 47)) != 0; // Bit 47
        var lockedMsr = (msrValue & (1UL << 63)) != 0;

        Debug.WriteLine($"MSR 0x610 Raw Value: {Convert.ToString((long)msrValue, 2).PadLeft(64, '0')}");

        // Convert using your function
        Debug.WriteLine($"PL1 Time Window (converted): {pl1TimeWindow} sec");
        Debug.WriteLine($"PL2 Time Window (converted): {pl2TimeWindow} sec");

        // Create and return the PowerLimit object
        return new PowerLimit
        {
          Pl1Watts = pl1Watts,
          Pl2Watts = pl2Watts,
          Pl1TimeWindow = pl1TimeWindow,
          Pl2TimeWindow = pl2TimeWindow,
          Pl1Enabled = pl1Enabled,
          Pl2Enabled = pl2Enabled
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

    private static double ConvertTimeWindow(int x, int y)
    {
      return (1 + (x / 4.0)) * Math.Pow(2, y);
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


      var powerUnitRaw = (int)(msrValue & 0xF);  // Mask only bits 3:0
      var powerUnit = 1.0 / Math.Pow(2, powerUnitRaw);
      return powerUnit;
      // Bits 3:0 in MSR 0x606 define Power Units
      //var powerUnitRaw = (int)(msrValue & 0xF);
      //  return 1.0 / Math.Pow(2, powerUnitRaw);
    }
  }

}
