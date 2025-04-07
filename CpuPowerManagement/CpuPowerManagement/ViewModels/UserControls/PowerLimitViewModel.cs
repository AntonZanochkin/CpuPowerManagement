using CommunityToolkit.Mvvm.Input;
using CpuPowerManagement.Intel.MSR;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace CpuPowerManagement.ViewModels.UserControls
{
  public class PowerLimitViewModel : INotifyPropertyChanged
  {
    private readonly IntelManagement _intelManagement;
    private MsrPowerLimit _powerLimit;

    public event PropertyChangedEventHandler? PropertyChanged;
    public double[] ValidTimeSteps { get; set; } = GenerateValidTimeSteps();

    public MsrPowerLimit PowerLimit
    {
      get => _powerLimit;
      set => SetField(ref _powerLimit, value);
    }

    public ICommand ApplyPowerLimit1Command { get; }

    public PowerLimitViewModel()
    {

    }

    public PowerLimitViewModel(IntelManagement intelManagement)
    {
      _intelManagement = intelManagement;

      ApplyPowerLimit1Command = new AsyncRelayCommand(ExecuteApplyPowerLimit1CommandAsync);

      if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
      {
        PowerLimit = MsrPowerLimit.CreateMock();
      }
      else
      {
        PowerLimit = _intelManagement.ReadMsrPowerLimit();
      }
    }

    private async Task ExecuteApplyPowerLimit1CommandAsync()
    {
      _intelManagement.WritePowerLimit(PowerLimit);
      await Task.Delay(1000);
      PowerLimit = _intelManagement.ReadMsrPowerLimit();
    }

    private static double[] GenerateValidTimeSteps()
    {
      var steps = new List<double>(); // use HashSet to avoid duplicates

      for (var y = 0; y <= 31; y++)
      {
        for (var z = 0; z <= 3; z++)
        {
          var time = Math.Pow(2, y) * (1 + z / 4.0) * 1 / 1024;

          if (time > 0 && time < 1000)
            steps.Add(Math.Round(time, 5));
        }
      }

      steps[0] = 0;

      return steps.ToArray();
    }

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
