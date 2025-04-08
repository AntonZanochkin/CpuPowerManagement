
namespace CpuPowerManagement.Intel.MSR
{
  public class MsrTcc
  {
    public ulong RawValue { get; set; }
    public int TjMax { get; set; }
    public int TccOffset { get; set; }

    public static MsrTcc CreateMock()
    {
      return new MsrTcc
      {
        TjMax = 95,
        TccOffset = 10
      };
    }
  }
}
