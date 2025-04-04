using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CpuPowerManagement.Intel.MSR;

namespace CpuPowerManagement.ViewModels
{
  public class MainViewModel : INotifyPropertyChanged
  {
    private PowerLimit _powerLimits;

    public PowerLimit PowerLimits
    {
      get => _powerLimits;
      set => SetField(ref _powerLimits, value);
    }

    public ICommand ApplyPowerLimit1Command { get; }
    //public ICommand ApplyPowerLimit2Command { get; }

    public MainViewModel()
    {
      ApplyPowerLimit1Command = new AsyncRelayCommand(ExecuteApplyPowerLimit1CommandAsync);
      //ApplyPowerLimit2Command = new RelayCommand(ExecuteApplyPowerLimit2Command);

      if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
      {
        // В режиме дизайна подставляем мок-данные
        PowerLimits = PowerLimit.CreateMock();
      }
      else
      {
      
        // В рантайме получаем реальные данные
        PowerLimits = IntelManagement.GetPowerLimits();
      }
    }

    private async Task ExecuteApplyPowerLimit1CommandAsync()
    {
      IntelManagement.SetPl(PowerLimits);
      await Task.Delay(1000);
      PowerLimits = IntelManagement.GetPowerLimits();
      //IntelManagement.SetPl1TimeWindow(PowerLimits.Pl1TimeWindowSec);
      //IntelManagement.SetPl2TimeWindow(PowerLimits.Pl2TimeWindowSec);
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