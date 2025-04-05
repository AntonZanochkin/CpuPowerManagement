using CpuPowerManagement.CLI;
using System.IO;
using System.Windows;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CpuPowerManagement.Intel.MSR
{
  public static class IntelManagement
  {
    static readonly string FolderPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty;
    static readonly string ProcessMsr = Path.Combine(FolderPath, "Assets\\Intel\\MSR\\msr-cmd.exe");
    static readonly double Msr606TimeUnit;
    static IntelManagement()
    {
      var output606 = RunCli.RunCommand("read 0x606", true, ProcessMsr);
      var msr606Value = GetMsrValue(output606);
      Msr606TimeUnit = GetTimeUnitFromMsr(msr606Value);
    }

    public static void SetPl(PowerLimit limits)
    {
      try
      {
        // Convert Power Limits (Watts → Hex → UInt)
        ulong pl1Limit = Convert.ToUInt64(ConvertTdpToHexMsr((double)limits.Pl1Watts), 16);
        ulong pl2Limit = Convert.ToUInt64(ConvertTdpToHexMsr((double)limits.Pl2Watts), 16);

        // Convert Time Windows (Seconds → Encoded)
        int encodedTime1 = EncodeTimeWindow(limits.Pl1TimeWindowSec * 1000);
        int encodedTime2 = EncodeTimeWindow(limits.Pl2TimeWindowSec * 1000);

        //var d1 = DecodeTimeWindow(encodedTime1);
        //var d2 = DecodeTimeWindow(encodedTime2);

        // Construct the full 64-bit MSR value
        ulong msrValue = 0;
        msrValue |= pl1Limit & 0x7FFF;                 // PL1 Limit (Bits 14:0)
        msrValue |= (limits.Pl1Enabled ? 1UL : 0) << 15;  // PL1 Enable (Bit 15)
        msrValue |= (limits.Pl1Enabled ? 1UL : 0) << 16;  // PL1 Clamping (Bit 16)
        msrValue |= ((ulong)encodedTime1 & 0x7F) << 17;   // PL1 Time Window (Bits 23:17)

        msrValue |= pl2Limit << 32;                     // PL2 Limit (Bits 46:32)
        msrValue |= (limits.Pl2Enabled ? 1UL : 0) << 47;  // PL2 Enable (Bit 47)
        msrValue |= (limits.Pl2Enabled ? 1UL : 0) << 48;  // PL2 Clamping (Bit 48)
        msrValue |= ((ulong)encodedTime2 & 0x7F) << 49;   // PL2 Time Window (Bits 55:49)

        if (limits.LockedMsr)                            // MSR Lock (Bit 63)
          msrValue |= (1UL << 63);

        // Convert MSR Value to hex string
        string hexMsr = msrValue.ToString("X16"); // 16-digit hex

        // Write to MSR
        string commandArguments = $"-s write 0x610 0x{hexMsr.Substring(0, 8)} 0x{hexMsr.Substring(8, 8)}";
        RunCli.RunCommand(commandArguments, false, ProcessMsr);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString());
      }
    }

    private static int EncodeTimeWindow(double seconds)
    {
      int bestL = 0;
      int bestB = 0;
      double minError = double.MaxValue;

      // Try all possible L and B values
      for (int L = 0; L < 32; L++)  // L is 5 bits (0 to 31)
      {
        for (int B = 0; B < 4; B++) // B is 2 bits (0 to 3)
        {
          double candidateTime = Math.Pow(2, L) * (1 + (B / 4.0)); // Formula from Intel Docs
          double error = Math.Abs(candidateTime - seconds);

          if (error < minError)
          {
            minError = error;
            bestL = L;
            bestB = B;
          }
        }
      }

      return (bestB << 5) | bestL;  // Store B in bits [6:5] and L in bits [4:0]
    }

    public static double DecodeTimeWindow(int encodedValue, double timeUnit)
    {
      int L = encodedValue & 0x1F;  // Extract lower 5 bits
      int B = (encodedValue >> 5) & 0x3; // Extract upper 2 bits

      return (Math.Pow(2, L) * (1 + B / 4.0)) * timeUnit;
    }


    private static string ConvertTdpToHexMsr(double tdp)
    {
      // Intel MSR uses a fixed Power Unit. Typically, it's 1/8 watts.
      const int powerUnit = 8;

      // Convert watts to MSR format
      int msrValue = (int)(tdp * powerUnit);

      // Convert to uppercase hex string
      return msrValue.ToString("X");
    }

    public static void SetPl1TimeWindow(double pl1TimeSeconds)
    {
      try
      {
        // Convert time in seconds to encoded format
        int encodedTime = ConvertTimeWindowToEncoded(pl1TimeSeconds);

        // Read current MSR 0x610 value to avoid overwriting other bits
        var output = RunCli.RunCommand("read 0x610", true, ProcessMsr);
        var msrValue = GetMsrValue(output);

        // Clear existing PL1 Time Window bits (Bits 23:17) - cast mask to ulong
        msrValue &= ~((ulong)0x7F << 17);

        // Set new PL1 Time Window value
        msrValue |= ((ulong)(encodedTime & 0x7F) << 17);

        // Write the updated value back to MSR 0x610
        var commandArguments = $"-s write 0x610 0x{(msrValue >> 32):X8} 0x{(msrValue & 0xFFFFFFFF):X8}";
        RunCli.RunCommand(commandArguments, false, ProcessMsr);
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Failed to set PL1 Time Window: {ex.Message}");
      }
    }

    /// <summary>
    /// Sets the PL2 Time Window (in seconds) in MSR 0x610.
    /// </summary>
    /// <param name="pl2TimeSeconds">The desired PL2 time window in seconds.</param>
    public static void SetPl2TimeWindow(double pl2TimeSeconds)
    {
      try
      {
        // Convert time in seconds to encoded format
        var encodedTime = ConvertTimeWindowToEncoded(pl2TimeSeconds);

        // Read current MSR 0x610 value to avoid overwriting other bits
        var output = RunCli.RunCommand("read 0x610", true, ProcessMsr);
        var msrValue = GetMsrValue(output);

        // Clear existing PL2 Time Window bits (Bits 54:48) - cast mask to ulong
        msrValue &= ~((ulong)0x7F << 48);

        // Set new PL2 Time Window value
        msrValue |= ((ulong)(encodedTime & 0x7F) << 48);

        // Write the updated value back to MSR 0x610
        var commandArguments = $"-s write 0x610 0x{(msrValue >> 32):X8} 0x{(msrValue & 0xFFFFFFFF):X8}";
        RunCli.RunCommand(commandArguments, false, ProcessMsr);
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Failed to set PL2 Time Window: {ex.Message}");
      }
    }

    public static PowerLimit GetPowerLimits()
    {
      try
      {
        // Read MSR 0x610
        var output610 = RunCli.RunCommand("read 0x610", true, ProcessMsr);
        var msrValue610 = GetMsrValue(output610);

        if (msrValue610 == 0)
        {
          MessageBox.Show("Failed to read MSR 0x610.");
          return null;
        }

        // Get Power Unit from MSR 0x606
        var powerUnit = GetPowerUnit();

        // Extract PL1 (Bits 14:0) and convert to watts
        var pl1Raw = (int)(msrValue610 & 0x7FFF);
        var pl1Watts = pl1Raw * powerUnit;

        // Extract PL2 (Bits 46:32) and convert to watts
        var pl2Raw = (int)((msrValue610 >> 32) & 0x7FFF);
        var pl2Watts = pl2Raw * powerUnit;

        var encodedTime1 = (int)((msrValue610 >> 17) & 0x7F); // Bits 23:17
        var pl1TimeWindow = DecodeTimeWindow(encodedTime1, Msr606TimeUnit);

        // Extract PL2 Time Window (Bits 55:49)
        var encodedTime2 = (int)((msrValue610 >> 49) & 0x7F); // Bits 55:49
        var pl2TimeWindow = DecodeTimeWindow(encodedTime2, Msr606TimeUnit);
        // Extract enable bits for PL1 and PL2 (Bits 15 and 47)
        var pl1Enabled = (msrValue610 & (1UL << 15)) != 0; // Bit 15
        var pl2Enabled = (msrValue610 & (1UL << 47)) != 0; // Bit 47

        var lockedMsr = (msrValue610 & (1UL << 63)) != 0;
        // Create and return the PowerLimit object
        return new PowerLimit
        {
          LockedMsr = lockedMsr,
          Pl1Watts = (int)pl1Watts,
          Pl2Watts = (int)pl2Watts,
          Pl1TimeWindowSec = pl1TimeWindow,
          Pl2TimeWindowSec = pl2TimeWindow,
          Pl1Enabled = pl1Enabled,
          Pl2Enabled = pl2Enabled
        };
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Error: {ex.Message}\n{ex.StackTrace}");
        return null;
      }
    }

    public static double GetTimeUnitFromMsr(ulong msrValue)
    {
      // MSR_RAPL_POWER_UNIT format:
      // Bits 19:16 = TimeUnit (as 2^(-x))
      int timeUnitBits = (int)((msrValue >> 16) & 0xF); // extract bits 19:16
      return Math.Pow(2, -timeUnitBits); // 2^(-TimeUnit)
    }

    //private static double DecodeTimeWindow(int encodedValue)
    //{
    //  int E = encodedValue & 0x1F;  // Extract lower 5 bits (Exponent)
    //  int F = (encodedValue >> 5) & 0x3; // Extract upper 2 bits (Fraction)

    //  // Formula: Time Window = 2^E * (1.0 + F / 4.0) * Timeunit
    //  double timeWindowSeconds = Math.Pow(2, E) * (1.0 + (F / 4.0));

    //  return timeWindowSeconds; // Return in seconds
    //}



    private static ulong ConvertTdpToHexMsr(int tdp)
    {
      var tpdHex = (ulong)(tdp * 8); // Ensure it's a ulong
      return tpdHex;
    }

    private static ulong FormatHex(int value)
    {
      return Convert.ToUInt64(value); // Convert int to ulong
    }

    private static ulong HexStringToUlong(string hex)
    {
      return Convert.ToUInt64(hex, 16); // Convert hex string to ulong
    }


    private static double ConvertTimeWindow(int x, int y)
    {
      return (1 + (x / 4.0)) * Math.Pow(2, y);
    }

    private static ulong GetMsrValue(string msrOutput)
    {
      var regex = new Regex(@"0x(?<reg>[0-9A-Fa-f]+)\s+0x(?<edx>[0-9A-Fa-f]+)\s+0x(?<eax>[0-9A-Fa-f]+)");
      var match = regex.Match(msrOutput);

      if (!match.Success) return 0; // Return 0 on failures

      var edx = Convert.ToUInt64(match.Groups["edx"].Value, 16);
      var eax = Convert.ToUInt64(match.Groups["eax"].Value, 16);
      return (edx << 32) | eax; // Combine EDX and EAX to form a 64-bit MSR value
    }

    public static double GetPowerUnit()
    {
      var output = RunCli.RunCommand("read 0x606", true, ProcessMsr);
      var msrValue = GetMsrValue(output);

      if (msrValue == 0)
      {
        Console.WriteLine("Failed to read MSR 0x606. Using default power unit.");
        return 0.125;  // Default = 1/8 W
      }

      var powerUnitRaw = (int)(msrValue & 0xF);  // Mask only bits 3:0
      var powerUnit = 1.0 / Math.Pow(2, powerUnitRaw);
      return powerUnit;
    }

    /// <summary>
    /// Converts a time window (in seconds) to the MSR encoded format.
    /// </summary>
    private static int ConvertTimeWindowToEncoded(double timeSeconds)
    {
      int L = 0;
      int B = 0;

      // Try different B values to find the best fit for L
      for (B = 0; B < 4; B++)
      {
        L = (int)(timeSeconds / Math.Pow(2, B * 0.25));

        if (L >= 1 && L <= 31) // L must be in range 1-31
          break;
      }

      // Ensure L is in range
      if (L < 1) L = 1;
      if (L > 31) L = 31;

      // Encode (B << 5) | L (7-bit format)
      return ((B & 0x3) << 5) | (L & 0x1F);
    }
  }
}
