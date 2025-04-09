using System.IO;
using System.Reflection;
using static CpuPowerManagement.Intel.MSR.MsrCoreThermal;
using static CpuPowerManagement.Intel.MSR.MsrTccManager;

namespace CpuPowerManagement.Intel.MSR
{
  public class IntelManagement
  {
    readonly MsrPowerLimitManager _powerLimitManager;
    readonly MsrTccManager _msrThermalTarget;
    readonly MsrPackageThermal _msrPackageThermal;
    readonly MsrCoreThermal _msrCoreThermal;

    public IntelManagement()
    {
      var folderPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty;
      var processMsr = Path.Combine(folderPath, "Assets\\Intel\\MSR\\msr-cmd.exe");
      var powerMultiplier = new MsrPowerMultiplier(processMsr);
      var powerMultiplierInfo = powerMultiplier.ReadPowerMultiplier();
      _powerLimitManager = new MsrPowerLimitManager(processMsr, powerMultiplierInfo);
      _msrThermalTarget = new MsrTccManager(processMsr);
      _msrPackageThermal = new MsrPackageThermal(processMsr);
      _msrCoreThermal = new MsrCoreThermal(processMsr);
    }

    public MsrPowerLimitData ReadMsrPowerLimitData()
    {
      return _powerLimitManager.ReadPowerLimitData();
    }

    public void WritePowerLimitData(MsrPowerLimitData limitData)
    {
      _powerLimitManager.WritePowerLimit(limitData);
    }

    public MsrTccData ReadTccOffsetData()
    {
      return _msrThermalTarget.ReadTccData();
    }

    public void WriteTccOffsetData(MsrTccData tcc)
    {
      _msrThermalTarget.WriteTccOffset(tcc.TccOffset);
    }

    //public MsrCoreThermalData ReadCoreThermalData()
    //{
    //  return _msrCoreThermal.ReadCoreThermalInfo();
    //}

    public MsrPackageThermal.MsrPackageThermalData ReadPackageThermalData()
    {
      return _msrPackageThermal.ReadPackageThermalData();
    }
  }
}
