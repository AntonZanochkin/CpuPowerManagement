using CpuPowerManagement.CLI;
using System.Diagnostics;

namespace CpuPowerManagement.Intel.MSR
{
  public class MsrPackagePower(string processMsr)
  {
    public async Task<PackagePowerData> ReadPackagePowerAsync(int delayMilliseconds = 1000)
    {
      // Step 1: Read MSR_RAPL_POWER_UNIT (0x606) and extract energy unit
      var unitRaw = MsrHelpers.GetMsrValue(RunCli.RunCommand("read 0x606", true, processMsr));
      var energyUnit = Math.Pow(0.5, (unitRaw >> 8) & 0x1F); // Bits 12:8

      
      // Step 2: Read energy status MSR_PKG_ENERGY_STATUS (0x611)
      var energy1Raw = MsrHelpers.GetMsrValue(RunCli.RunCommand("read 0x611", true, processMsr));
      var energy1 = energy1Raw * energyUnit;
      var stopwatch = Stopwatch.StartNew();
      // Step 3: Wait a bit
      await Task.Delay(delayMilliseconds);

     
      // Step 4: Read again
      var energy2Raw = MsrHelpers.GetMsrValue(RunCli.RunCommand("read 0x611", true, processMsr));
      var energy2 = energy2Raw * energyUnit;

      stopwatch.Stop();

      // Step 5: Handle 32-bit overflow
      var deltaEnergy = energy2 - energy1;
      if (deltaEnergy < 0)
        deltaEnergy += Math.Pow(2, 32) * energyUnit;

      var deltaTime = stopwatch.Elapsed.TotalSeconds;
      //var deltaTime = (delayMilliseconds) / 1000.0;
      var power = deltaEnergy / deltaTime;

      return new PackagePowerData
      {
        PowerWatts = power,
        Energy = energy2,
      };
    }

    public class PackagePowerData
    {
      public double PowerWatts;
      public double Energy;
    }
  }
}
