using System.Text.RegularExpressions;

namespace CpuPowerManagement.Intel.MSR
{
  public static class MsrHelpers
  {
    public static ulong GetMsrValue(string msrOutput)
    {
      var regex = new Regex(@"0x(?<reg>[0-9A-Fa-f]+)\s+0x(?<edx>[0-9A-Fa-f]+)\s+0x(?<eax>[0-9A-Fa-f]+)");
      var match = regex.Match(msrOutput);
      if (!match.Success) return 0;

      var edx = Convert.ToUInt64(match.Groups["edx"].Value, 16);
      var eax = Convert.ToUInt64(match.Groups["eax"].Value, 16);
      return (edx << 32) | eax;// Combine EDX and EAX to form a 64-bit MSR value
    }
  }
}
