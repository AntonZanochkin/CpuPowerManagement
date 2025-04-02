namespace CpuPowerManagement.Intel.MSR
{
  public class PowerLimit
  {
    public double PL1_Watts { get; set; }
    public double PL2_Watts { get; set; }
    public double PL1_TimeWindow { get; set; }
    public double PL2_TimeWindow { get; set; }
    public bool PL1_Enabled { get; set; }
    public bool PL2_Enabled { get; set; }

    public static PowerLimit CreateMock()
    {
      return new PowerLimit
      {
        PL1_Watts = 35,
        PL2_Watts = 45,
        PL1_TimeWindow = 40,
        PL2_TimeWindow = 20,
        PL1_Enabled = false,
        PL2_Enabled = true,
      };
    }
    public override string ToString()
    {
      return $"PL1: {PL1_Watts} W (Enabled: {PL1_Enabled}, Time: {PL1_TimeWindow} sec)\n" +
             $"PL2: {PL2_Watts} W (Enabled: {PL2_Enabled}, Time: {PL2_TimeWindow} sec)";
    }
  }
}
