namespace CpuPowerManagement.Intel.MSR
{
  public class PowerLimit
  {
    public double Pl1Watts { get; set; }
    public double Pl2Watts { get; set; }
    public double Pl1TimeWindow { get; set; }
    public double Pl2TimeWindow { get; set; }
    public bool Pl1Enabled { get; set; }
    public bool Pl2Enabled { get; set; }

    public static PowerLimit CreateMock()
    {
      return new PowerLimit
      {
        Pl1Watts = 35,
        Pl2Watts = 45,
        Pl1TimeWindow = 40,
        Pl2TimeWindow = 20,
        Pl1Enabled = false,
        Pl2Enabled = true,
      };
    }
    public override string ToString()
    {
      return $"PL1: {Pl1Watts} W (Enabled: {Pl1Enabled}, Time: {Pl1TimeWindow} sec)\n" +
             $"PL2: {Pl2Watts} W (Enabled: {Pl2Enabled}, Time: {Pl2TimeWindow} sec)";
    }
  }
}
