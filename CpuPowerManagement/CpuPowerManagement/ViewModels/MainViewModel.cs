using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CpuPowerManagement.Intel;

namespace CpuPowerManagement.ViewModels
{
  public class MainViewModel : INotifyPropertyChanged
  {
    private int _plLimit1 = 45;

    public int PlLimit1
    {
      get => _plLimit1;
      set
      {
        _plLimit1 = value;
        OnPropertyChanged(nameof(PlLimit1));
      }
    }

    public ICommand ApplyPowerLimit1Command { get; }
    public MainViewModel()
    {
      ApplyPowerLimit1Command = new AsyncRelayCommand<int>(async (i) => await ExecuteApplyPowerLimitCommand(i));
    }

    private async Task ExecuteApplyPowerLimitCommand(int limit)
    {
      IntelManagement.RunIntelTDPChangeMSR(limit, limit);

      //OnPropertyChanged(nameof(WasHiredCountString));
      //OnPropertyChanged(nameof(NoAvailableCountString));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
      if (EqualityComparer<T>.Default.Equals(field, value)) return false;
      field = value;
      OnPropertyChanged(propertyName);
      return true;
    }
  }
}
