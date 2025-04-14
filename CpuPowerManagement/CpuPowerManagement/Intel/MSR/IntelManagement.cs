using System.IO;
using System.Reflection;
using static CpuPowerManagement.Intel.MSR.MsrCoreThermal;
using static CpuPowerManagement.Intel.MSR.MsrPackagePower;
using static CpuPowerManagement.Intel.MSR.MsrTcc;

namespace CpuPowerManagement.Intel.MSR
{
  public class IntelManagement
  {
    readonly MsrPowerLimit _powerLimit;
    readonly MsrTcc _msrTcc;
    readonly MsrPackageThermal _msrPackageThermal;
    readonly MsrCoreThermal _msrCoreThermal;
    readonly MsrPackagePower _msrPackagePower;

    public IntelManagement()
    {
      var folderPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty;
      var processMsr = Path.Combine(folderPath, "Assets\\Intel\\MSR\\msr-cmd.exe");
      var powerMultiplier = new MsrPowerMultiplier(processMsr);
      var powerMultiplierInfo = powerMultiplier.ReadPowerMultiplier();
      _powerLimit = new MsrPowerLimit(processMsr, powerMultiplierInfo);
      _msrTcc = new MsrTcc(processMsr);
      _msrPackageThermal = new MsrPackageThermal(processMsr);
      _msrCoreThermal = new MsrCoreThermal(processMsr);
      _msrPackagePower = new MsrPackagePower(processMsr);
    }

    public MsrPowerLimitData ReadMsrPowerLimitData()
    {
      return _powerLimit.ReadPowerLimitData();
    }

    public void WritePowerLimitData(MsrPowerLimitData limitData)
    {
      _powerLimit.WritePowerLimit(limitData);
    }

    public MsrTccData ReadTccData()
    {
      return _msrTcc.ReadTccData();
    }

    public void WriteTccOffsetData(MsrTccData tcc)
    {
      _msrTcc.WriteTccOffset(tcc.TccOffset);
    }

    public int ReadThermalStatusReadout()
    {
      return _msrCoreThermal.ReadThermalStatusReadout();
    }

    public MsrPackageThermal.MsrPackageThermalData ReadPackageThermalData()
    {
      return _msrPackageThermal.ReadPackageThermalData();
    }

    public Task<PackagePowerData> ReadPackagePowerAsync()
    {
      return _msrPackagePower.ReadPackagePowerAsync();
    }
  }
}
