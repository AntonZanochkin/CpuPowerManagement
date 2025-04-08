using CpuPowerManagement.CLI;
using System.Text.RegularExpressions;

namespace CpuPowerManagement.Intel.MSR
{
  public class MsrTccManager
  {
    private readonly string _processMsr;

    public MsrTccManager(string processMsr)
    {
      _processMsr = processMsr;
    }

    public MsrTcc ReadTcc()
    {
      var result = RunCli.RunCommand("read 0x1A2", true, _processMsr);
      var msrValue = MsrHelpers.GetMsrValue(result);
      var eax = (uint)(msrValue & 0xFFFFFFFF);

      int tjMax = (int)((eax >> 16) & 0xFF);
      int tccOffset = (int)((eax >> 24) & 0x3F);

      return new MsrTcc
      {
        RawValue = msrValue,
        TjMax = tjMax,
        TccOffset = tccOffset
      };
    }

    public void WriteTccOffset(int offset)
    {
      if (offset < 0 || offset > 63)
        throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be between 0 and 63");

      // Read existing value
      var result = RunCli.RunCommand("read 0x1A2", true, _processMsr);
      var msrValue = MsrHelpers.GetMsrValue(result);

      // Clear bits [29:24]
      msrValue &= ~(0x3FUL << 24);

      // Set new offset
      msrValue |= ((ulong)offset & 0x3F) << 24;

      // Convert to hex string
      var hexMsr = msrValue.ToString("X16");

      // Write to MSR
      var commandArguments = $"-s write 0x1A2 0x{hexMsr.Substring(0, 8)} 0x{hexMsr.Substring(8, 8)}";
      RunCli.RunCommand(commandArguments, false, _processMsr);
    } 
  }
}
