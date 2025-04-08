using System.IO;
using System.Reflection;

namespace CpuPowerManagement.Intel.MSR
{
  public class IntelManagement
  {
    readonly MsrPowerLimitManager _powerLimitManager;
    readonly MsrTccManager _msrThermalTargetManager;

    public IntelManagement()
    {
      var folderPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty;
      var processMsr = Path.Combine(folderPath, "Assets\\Intel\\MSR\\msr-cmd.exe");
      var powerMultiplierManager = new MsrPowerMultiplierManager(processMsr);
      var powerMultiplier = powerMultiplierManager.ReadPowerMultiplier();
      _powerLimitManager = new MsrPowerLimitManager(processMsr, powerMultiplier);
      _msrThermalTargetManager = new MsrTccManager(processMsr);
    }

    public MsrPowerLimit ReadMsrPowerLimit()
    {
      return _powerLimitManager.ReadPowerLimit();
    }

    public void WritePowerLimit(MsrPowerLimit limit)
    {
      _powerLimitManager.WritePowerLimit(limit);
    }

    public MsrTcc ReadTccOffset()
    {
      return _msrThermalTargetManager.ReadTcc();
    }

    public void WriteTccOffset(MsrTcc tcc)
    {
      _msrThermalTargetManager.WriteTccOffset(tcc.TccOffset);
    }
  }
}
