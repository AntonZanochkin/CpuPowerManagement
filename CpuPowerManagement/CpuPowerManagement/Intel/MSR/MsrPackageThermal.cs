using CpuPowerManagement.CLI;

namespace CpuPowerManagement.Intel.MSR
{
  public class MsrPackageThermal(string processMsr)
  {
    public MsrPackageThermalData ReadPackageThermalData()
    {
      var result = RunCli.RunCommand($"read 0x1B1", true, processMsr);
      var msrValue = MsrHelpers.GetMsrValue(result);
      uint eax = (uint)(msrValue & 0xFFFFFFFF);

      return new MsrPackageThermalData
      {
        RawValue = msrValue,
        ThermalStatus = (eax & (1 << 0)) != 0,
        ThermalLog = (eax & (1 << 1)) != 0,
        PROCHOT = (eax & (1 << 2)) != 0,
        PROCHOTLog = (eax & (1 << 3)) != 0,
        CriticalTemperature = (eax & (1 << 4)) != 0,
        CriticalTemperatureLog = (eax & (1 << 5)) != 0,
        PowerLimitStatus = (eax & (1 << 10)) != 0,
        PowerLimitLog = (eax & (1 << 11)) != 0,
      };
    }

    public class MsrPackageThermalData
    {
      public ulong RawValue { get; set; }
      public bool ThermalStatus { get; set; }
      public bool ThermalLog { get; set; }
      public bool PROCHOT { get; set; }
      public bool PROCHOTLog { get; set; }
      public bool CriticalTemperature { get; set; }
      public bool CriticalTemperatureLog { get; set; }
      public bool PowerLimitStatus { get; set; }
      public bool PowerLimitLog { get; set; }
    }
  }
}
