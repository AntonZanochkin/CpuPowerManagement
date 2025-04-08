using CpuPowerManagement.CLI;
using System.Text.RegularExpressions;

namespace CpuPowerManagement.Intel.MSR
{
  //MSR_RAPL_POWER_UNIT 0x606
  //Unit Multiplier used in RAPL Interfaces (R/O)
  public class MsrPowerMultiplierManager
  {
    private string _processMsr;

    public MsrPowerMultiplierManager(string processMsr)
    {
      _processMsr = processMsr;
    }

    public MsrPowerMultiplier ReadPowerMultiplier()
    {
      var result = RunCli.RunCommand("read 0x606", true, _processMsr);
      var msrValue = MsrHelpers.GetMsrValue(result);
      var timeUnit = GetTimeMultiplierFromMsr(msrValue);
      var powerUnit = GetPowerMultiplierFromMsr(msrValue);

      return new MsrPowerMultiplier(timeUnit, powerUnit );

    }

    private double GetTimeMultiplierFromMsr(ulong msrValue)
    {
      // Bits 19:16 = Time (as 2^(-x))
      var timeUnitBits = (int)((msrValue >> 16) & 0xF); // extract bits 19:16
      return Math.Pow(2, -timeUnitBits); // 2^(-Time)
    }

    public static double GetPowerMultiplierFromMsr(ulong msrValue)
    {
      if (msrValue == 0)
      {
        Console.WriteLine("Failed to read MSR 0x606. Using default power unit.");
        return 0.125;  // Default = 1/8 W
      }

      var powerUnitRaw = (int)(msrValue & 0xF);  // Mask only bits 3:0
      var powerUnit = 1.0 / Math.Pow(2, powerUnitRaw);
      return powerUnit;
    }
  }
}
