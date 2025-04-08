using CommunityToolkit.Mvvm.Messaging.Messages;
using CpuPowerManagement.Intel.MSR;

namespace CpuPowerManagement.Messages
{
  public class UpdatePowerLimitMessage : ValueChangedMessage<MsrPowerLimit>
  {
    public UpdatePowerLimitMessage(MsrPowerLimit value) : base(value) { }
  }

  public class UpdatePowerLimit1Message : ValueChangedMessage<int>
  {
    public UpdatePowerLimit1Message(int value) : base(value) { }
  }

  public class UpdateTittleMessage : ValueChangedMessage<string>
  {
    public UpdateTittleMessage(string value) : base(value) { }
  }

}
