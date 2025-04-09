using CpuPowerManagement.CLI;
using System.Text.RegularExpressions;

namespace CpuPowerManagement.Intel.MSR
{
  //MSR_RAPL_POWER_UNIT 0x606
  //Unit Multiplier used in RAPL Interfaces (R/O)
  public class MsrPowerMultiplier(string processMsr)
  {
    public MsrPowerMultiplierData ReadPowerMultiplier()
    {
      var result = RunCli.RunCommand("read 0x606", true, processMsr);
      var msrValue = MsrHelpers.GetMsrValue(result);
      var timeUnit = GetTimeMultiplierFromMsr(msrValue);
      var powerUnit = GetPowerMultiplierFromMsr(msrValue);

      return new MsrPowerMultiplierData(timeUnit, powerUnit );
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
  public class MsrPowerLimitData
  {
    public int Pl1Watts { get; set; }
    public int Pl2Watts { get; set; }
    public double Pl1TimeWindowSec { get; set; }
    public double Pl2TimeWindowSec { get; set; }
    public bool Pl1Enabled { get; set; }
    public bool Pl2Enabled { get; set; }
    public bool LockedMsr { get; set; }

    public static MsrPowerLimitData CreateMock()
    {
      return new MsrPowerLimitData
      {
        LockedMsr = false,
        Pl1Watts = 35,
        Pl2Watts = 45,
        Pl1TimeWindowSec = 10,
        Pl2TimeWindowSec = 5,
        Pl1Enabled = false,
        Pl2Enabled = true,
      };
    }
    public override string ToString()
    {
      return $"PL1: {Pl1Watts} W (Enabled: {Pl1Enabled}, Time: {Pl1TimeWindowSec} sec)\n" +
             $"PL2: {Pl2Watts} W (Enabled: {Pl2Enabled}, Time: {Pl2TimeWindowSec} sec)";
    }
  }

  public class MsrPowerMultiplierData(double time, double power)
  {
    public double Time { get; private set; } = time;
    public double Power { get; private set; } = power;
  }
}
