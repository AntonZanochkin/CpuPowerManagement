using System.IO;
using System.Reflection;
using static CpuPowerManagement.Intel.MSR.MsrCoreThermal;
using static CpuPowerManagement.Intel.MSR.MsrTcc;

namespace CpuPowerManagement.Intel.MSR
{
  public class IntelManagement
  {
    readonly MsrPowerLimit _powerLimit;
    readonly MsrTcc _msrTcc;
    readonly MsrPackageThermal _msrPackageThermal;
    readonly MsrCoreThermal _msrCoreThermal;

    private int? _tjMax;

    public int TjMax
    {
      get
      {
        _tjMax ??= _msrTcc.ReadTccData().TjMax;
        return _tjMax.Value;
      }
    }

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
    }

    public MsrPowerLimitData ReadMsrPowerLimitData()
    {
      return _powerLimit.ReadPowerLimitData();
    }

    public void WritePowerLimitData(MsrPowerLimitData limitData)
    {
      _powerLimit.WritePowerLimit(limitData);
    }

    public MsrTccData ReadTccOffsetData()
    {
      return _msrTcc.ReadTccData();
    }

    public void WriteTccOffsetData(MsrTccData tcc)
    {
      _msrTcc.WriteTccOffset(tcc.TccOffset);
    }

    public int ReadCurrentTemperature()
    {
      return _msrCoreThermal.ReadCurrentTemperature(TjMax);
    }

    public MsrPackageThermal.MsrPackageThermalData ReadPackageThermalData()
    {
      return _msrPackageThermal.ReadPackageThermalData();
    }
  }
}
