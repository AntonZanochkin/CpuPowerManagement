using CpuPowerManagement.CLI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace CpuPowerManagement.Intel.MSR
{
  public class MsrPowerLimitManager
  {
    private readonly string _processMsr;
    private readonly MsrPowerMultiplier _msrPowerMultiplier;
    public MsrPowerLimitManager(string processMsr, MsrPowerMultiplier msrPowerMultiplier)
    {
      _processMsr = processMsr;
      _msrPowerMultiplier = msrPowerMultiplier;
    }

    public MsrPowerLimit ReadPowerLimit()
    {
      var result = RunCli.RunCommand("read 0x610", true, _processMsr);
      var msrValue = GetMsrValue(result);
      
      // Extract PL1 (Bits 14:0) and convert to watts
      var pl1Raw = (int)(msrValue & 0x7FFF);
      var pl1Watts = pl1Raw * _msrPowerMultiplier.Power;
      
      // Extract PL2 (Bits 46:32) and convert to watts
      var pl2Raw = (int)((msrValue >> 32) & 0x7FFF);
      var pl2Watts = pl2Raw * _msrPowerMultiplier.Power;
      
      var encodedTime1 = (int)((msrValue >> 17) & 0x7F); // Bits 23:17
      var pl1TimeWindow = Math.Round(DecodeTimeWindow(encodedTime1, _msrPowerMultiplier.Time), 5);
      
      // Extract PL2 Time Window (Bits 55:49)
      var encodedTime2 = (int)((msrValue >> 49) & 0x7F); // Bits 55:49
      var pl2TimeWindow = Math.Round(DecodeTimeWindow(encodedTime2, _msrPowerMultiplier.Time), 5);
      // Extract enable bits for PL1 and PL2 (Bits 15 and 47)
      var pl1Enabled = (msrValue & (1UL << 15)) != 0; // Bit 15
      var pl2Enabled = (msrValue & (1UL << 47)) != 0; // Bit 47
      
      var lockedMsr = (msrValue & (1UL << 63)) != 0;

      var pl1Clamping = (msrValue & (1UL << 16)) != 0; // Bit 16
      var pl2Clamping = (msrValue & (1UL << 48)) != 0; // Bit 48
      
      return new MsrPowerLimit
      {
        LockedMsr = lockedMsr,
        Pl1Watts = (int)pl1Watts,
        Pl2Watts = (int)pl2Watts,
        Pl1TimeWindowSec = pl1TimeWindow,
        Pl2TimeWindowSec = pl2TimeWindow,
        Pl1Enabled = pl1Enabled,
        Pl2Enabled = pl2Enabled,
      };
    }

    public void WritePowerLimit(MsrPowerLimit limit)
    {
      // Convert Power Limits (Watts → Hex → UInt)
      var pl1Limit = Convert.ToUInt64(ConvertTdpToHexMsr(limit.Pl1Watts), 16);
      var pl2Limit = Convert.ToUInt64(ConvertTdpToHexMsr(limit.Pl2Watts), 16);

      // Convert Time Windows (Seconds → Encoded)
      var encodedTime1 = EncodeTimeWindow(limit.Pl1TimeWindowSec * 1000);
      var encodedTime2 = EncodeTimeWindow(limit.Pl2TimeWindowSec * 1000);

      // Construct the full 64-bit MSR value
      ulong msrValue = 0;
      msrValue |= pl1Limit & 0x7FFF;                 // PL1 Limit (Bits 14:0)
      msrValue |= (limit.Pl1Enabled ? 1UL : 0) << 15;  // PL1 Enable (Bit 15)
      msrValue |= (limit.Pl2Enabled ? 1UL : 0) << 16;  // PL1 Clamping (Bit 16)
      msrValue |= ((ulong)encodedTime1 & 0x7F) << 17;   // PL1 Time Window (Bits 23:17)

      msrValue |= pl2Limit << 32;                     // PL2 Limit (Bits 46:32)
      msrValue |= (limit.Pl2Enabled ? 1UL : 0) << 47;  // PL2 Enable (Bit 47)
      msrValue |= (limit.Pl2Enabled ? 1UL : 0) << 48;  // PL2 Clamping (Bit 48)
      msrValue |= ((ulong)encodedTime2 & 0x7F) << 49;   // PL2 Time Window (Bits 55:49)

      if (limit.LockedMsr)                            // MSR Lock (Bit 63)
        msrValue |= (1UL << 63);

      // Convert MSR Value to hex string
      var hexMsr = msrValue.ToString("X16"); // 16-digit hex

      // Write to MSR
      var commandArguments = $"-s write 0x610 0x{hexMsr.Substring(0, 8)} 0x{hexMsr.Substring(8, 8)}";
      RunCli.RunCommand(commandArguments, false, _processMsr);
    }
    private int EncodeTimeWindow(double seconds)
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

    private double DecodeTimeWindow(int encodedValue, double timeUnit)
    {
      int L = encodedValue & 0x1F;  // Extract lower 5 bits
      int B = (encodedValue >> 5) & 0x3; // Extract upper 2 bits

      return (Math.Pow(2, L) * (1 + B / 4.0)) * timeUnit;
    }


    private string ConvertTdpToHexMsr(double tdp)
    {
      // Intel MSR uses a fixed Power Unit. Typically, it's 1/8 watts.
      const int powerUnit = 8;

      // Convert watts to MSR format
      int msrValue = (int)(tdp * powerUnit);

      // Convert to uppercase hex string
      return msrValue.ToString("X");
    }

    private ulong GetMsrValue(string msrOutput)
    {
      var regex = new Regex(@"0x(?<reg>[0-9A-Fa-f]+)\s+0x(?<edx>[0-9A-Fa-f]+)\s+0x(?<eax>[0-9A-Fa-f]+)");
      var match = regex.Match(msrOutput);

      if (!match.Success) return 0; // Return 0 on failures

      var edx = Convert.ToUInt64(match.Groups["edx"].Value, 16);
      var eax = Convert.ToUInt64(match.Groups["eax"].Value, 16);
      return (edx << 32) | eax; // Combine EDX and EAX to form a 64-bit MSR value
    }
  }
}
