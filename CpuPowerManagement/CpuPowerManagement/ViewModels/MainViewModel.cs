using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CpuPowerManagement.Intel.MSR;

namespace CpuPowerManagement.ViewModels
{
  public class MainViewModel : INotifyPropertyChanged
  {
    private PowerLimit _powerLimits;
    public List<double> ValidTimeStepsDouble { get; set; } = GenerateValidTimeStepsDouble();
    public ObservableCollection<string> ValidTimeSteps { get; set; } = GenerateValidTimeSteps();

    public PowerLimit PowerLimits
    {
      get => _powerLimits;
      set => SetField(ref _powerLimits, value);
    }

    public ICommand ApplyPowerLimit1Command { get; }

    public MainViewModel()
    {
      //;
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

    private static ObservableCollection<string> GenerateValidTimeSteps()
    {
      var steps = new List<string>();

      steps.Add("Unlimited");
      steps.AddRange(GenerateValidTimeStepsDouble().Select(x=>x.ToString()));
     
      return new ObservableCollection<string>(steps);
    }

    private static List<double> GenerateValidTimeStepsDouble()
    {
      var steps = new List<double>();

      //for (int y = 0; y <= 31; y++)
      //{
      //  var time = IntelManagement.DecodeTimeWindow(y);
      //  if (time >= 1) break;
      //  steps.Add(Math.Round(time, 5));
      //}

      for (int y = 0; y <= 31; y++)
      {
        var time = IntelManagement.DecodeTimeWindow(y, 1/1024);
       
        steps.Add(Math.Round(time, 5));
      }

      //for (int y = 0; y <= 31; y++)
      //{
      //  for (int z = 0; z <= 3; z++)
      //  {
      //    double time = Math.Pow(2, y) * (1 + z / 4.0);
      //    if (time > 0 && time < 1000) // optional max cap
      //      steps.Add((Math.Round(time, 3)));
      //  }
      //}

      return steps;
    }
  }
}