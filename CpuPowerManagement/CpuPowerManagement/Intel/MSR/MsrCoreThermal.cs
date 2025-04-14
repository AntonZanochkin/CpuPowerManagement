using CpuPowerManagement.CLI;

namespace CpuPowerManagement.Intel.MSR
{
  //IA32_THERM_STATUS
  public class MsrCoreThermal(string processMsr)
  {
    public MsrCoreThermalData ReadCoreThermalInfo()
    {
      var result = RunCli.RunCommand($"read 0x19C", true, processMsr);
      var msrValue = MsrHelpers.GetMsrValue(result);
      uint eax = (uint)(msrValue & 0xFFFFFFFF);

      return new MsrCoreThermalData
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

    public int ReadThermalStatusReadout()
    {
      var result = RunCli.RunCommand("read 0x19C", true, processMsr);
      var msrValue = MsrHelpers.GetMsrValue(result);
      var eax = (uint)(msrValue & 0xFFFFFFFF);

      if ((eax & (1 << 31)) == 0)
      {
        // Reading is invalid
        return -1;
      }

      var tjMaxDelta = (eax >> 16) & 0x7F;
      return (int)tjMaxDelta;
    }

    public class MsrCoreThermalData
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
