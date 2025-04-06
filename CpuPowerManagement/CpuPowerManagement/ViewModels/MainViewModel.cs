using System.Collections.ObjectModel;
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
    private IntelManagement _intelManagement = new IntelManagement();

    private MsrPowerLimit _powerLimit;
    //public List<double> ValidTimeStepsDouble { get; set; } = GenerateValidTimeStepsDouble();
    public double[] ValidTimeSteps { get; set; } = GenerateValidTimeSteps();
    public double MinValidTime => ValidTimeSteps?.FirstOrDefault() ?? 0.000;
    public double MaxValidTime => ValidTimeSteps?.LastOrDefault() ?? 1000;

    public MsrPowerLimit PowerLimit
    {
      get => _powerLimit;
      set => SetField(ref _powerLimit, value);
    }

    public ICommand ApplyPowerLimit1Command { get; }

    public MainViewModel()
    {
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

    private static double[] GenerateValidTimeSteps()
    {
      var steps = new List<double>(); // use HashSet to avoid duplicates

      for (var y = 0; y <= 31; y++)
      {
        for (var z = 0; z <= 3; z++)
        {
          var time = Math.Pow(2, y) * (1 + z / 4.0) * 1/1024;

          if (time > 0 && time < 1000)
            steps.Add(Math.Round(time, 5));
        }
      }

      steps[0] = 0;

      return steps.ToArray();
    }
  }
}