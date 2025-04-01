using CpuPowerManagement.CLI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows;
using CpuPowerManagement.CLI;
using System.Reflection;

namespace CpuPowerManagement.Intel
{
  public static class IntelManagement
  {
    
    public static void RunIntelTDPChangeMSR(int pl1TDP, int pl2TDP)
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

    private static string ConvertTdpToHexMsr(int tdp)
    {
      var tpdHex = (tdp * 8);
      return tpdHex.ToString("X");
    }

    private static string FormatHex(string hexValue)
    {
      return hexValue.PadLeft(3, '0');
    }
  }


}
