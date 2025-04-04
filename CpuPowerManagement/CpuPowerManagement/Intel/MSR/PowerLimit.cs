namespace CpuPowerManagement.Intel.MSR
{
  public class PowerLimit
  {
    public int Pl1Watts { get; set; }
    public int Pl2Watts { get; set; }
    public double Pl1TimeWindowSec { get; set; }
    public double Pl2TimeWindowSec { get; set; }
    public bool Pl1Enabled { get; set; }
    public bool Pl2Enabled { get; set; }
    public bool LockedMsr { get; set; }

    public static PowerLimit CreateMock()
    {
      return new PowerLimit
      {
        LockedMsr = false,
        Pl1Watts = 35,
        Pl2Watts = 45,
        Pl2TimeWindowSec = 28,
        Pl1TimeWindowSec = 0.003,
        Pl1Enabled = false,
        Pl2Enabled = true,
      };
    }
    public override string ToString()
    {
      return $"PL1: {Pl1Watts} W (Enabled: {Pl1Enabled}, Time: {Pl1TimeWindowSec} sec)\n" +
             $"PL2: {Pl2Watts} W (Enabled: {Pl2Enabled}, Time: {Pl2TimeWindowSec} sec)";
    }
  }
}
