namespace CpuPowerManagement.Intel.MSR
{
  public class MsrPowerMultiplier
  {
    public double Time { get; private set; }
    public double Power { get; private set; }

    public MsrPowerMultiplier(double time, double power)
    {
      Time = time;
      Power = power;
    }
  }
}
