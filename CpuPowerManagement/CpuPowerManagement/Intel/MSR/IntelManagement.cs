using System.IO;
using System.Reflection;

namespace CpuPowerManagement.Intel.MSR
{
  public class IntelManagement
  {
    readonly MsrPowerLimitManager _powerLimitManager;

    public IntelManagement()
    {
      var folderPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty;
      var processMsr = Path.Combine(folderPath, "Assets\\Intel\\MSR\\msr-cmd.exe");
      var powerMultiplierManager = new MsrPowerMultiplierManager(processMsr);
      var powerMultiplier = powerMultiplierManager.ReadPowerMultiplier();
      _powerLimitManager = new MsrPowerLimitManager(processMsr, powerMultiplier);
    }

    public MsrPowerLimit ReadMsrPowerLimit()
    {
      return _powerLimitManager.ReadPowerLimit();
    }

    public void WritePowerLimit(MsrPowerLimit limit)
    {
      _powerLimitManager.WritePowerLimit(limit);
    }
  }
}
