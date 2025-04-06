namespace CpuPowerManagement.Intel.MSR
{
  public class MsrPowerLimit
  {
    public int Pl1Watts { get; set; }
    public int Pl2Watts { get; set; }
    public double Pl1TimeWindowSec { get; set; }
    public double Pl2TimeWindowSec { get; set; }
    public bool Pl1Enabled { get; set; }
    public bool Pl2Enabled { get; set; }
    public bool LockedMsr { get; set; }

    public static MsrPowerLimit CreateMock()
    {
      return new MsrPowerLimit
      {
        LockedMsr = false,
        Pl1Watts = 35,
        Pl2Watts = 45,
        Pl1TimeWindowSec = 10,
        Pl2TimeWindowSec = 5,
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
