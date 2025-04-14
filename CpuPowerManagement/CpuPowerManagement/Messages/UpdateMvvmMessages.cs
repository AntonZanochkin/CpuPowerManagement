using CommunityToolkit.Mvvm.Messaging.Messages;
using CpuPowerManagement.Intel.MSR;

namespace CpuPowerManagement.Messages
{
  public class UpdateTemperatureLimitMessage(int value) : ValueChangedMessage<int>(value);
  public class TemperatureLimitRequestMessage();

  public class UpdatePowerLimitDataMessage(MsrPowerLimitData value) : ValueChangedMessage<MsrPowerLimitData>(value);
  public class TdpLimitRequestMessage();


}
