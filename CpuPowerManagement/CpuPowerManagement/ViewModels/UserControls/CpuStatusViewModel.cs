using CommunityToolkit.Mvvm.Input;
using CpuPowerManagement.Intel.MSR;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using LiveCharts;
using LiveCharts.Wpf;

namespace CpuPowerManagement.ViewModels.UserControls
{
  public class CpuStatusViewModel : INotifyPropertyChanged
  {
    private readonly IntelManagement _intelManagement = new();
    public event PropertyChangedEventHandler? PropertyChanged;
    private readonly DispatcherTimer _timer;
    private int _time = 0;

    private MsrCoreThermal.MsrCoreThermalData _coreThermalData;
    private MsrPackageThermal.MsrPackageThermalData _packageThermalData;

    public ChartValues<int> CoreThermalPoints { get; set; } = new();
    public ChartValues<int> PackagePowerPoints { get; set; } = new();

    public ObservableCollection<string> Labels { get; set; } = new();

    public SeriesCollection Series { get; set; }

    public MsrCoreThermal.MsrCoreThermalData CoreThermalData
    {
      get => _coreThermalData;
      set => SetField(ref _coreThermalData, value);
    }

    public MsrPackageThermal.MsrPackageThermalData PackageThermalData
    {
      get => _packageThermalData;
      set => SetField(ref _packageThermalData, value);
    }

    public CpuStatusViewModel()
    {
      if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
      {
        //Tcc = MsrTcc.CreateMock();

      }
      else
      {
        Series = new SeriesCollection
        {
          new LineSeries
          {
            Title = "Core Throttle",
            Values = CoreThermalPoints
          },
          new LineSeries
          {
            Title = "Package Power Limit",
            Values = PackagePowerPoints
          }
        };

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (s, e) => Tick();
        _timer.Start();
      }
    }

    private void Tick()
    {
      CoreThermalData = _intelManagement.ReadCoreThermalData();
      PackageThermalData = _intelManagement.ReadPackageThermalData();

      CoreThermalPoints.Add(CoreThermalData.ThermalStatus ? 1 : 0);
      PackagePowerPoints.Add(PackageThermalData.PowerLimitStatus ? 1 : 0);

      Labels.Add(_time.ToString());
      _time++;

      if (CoreThermalPoints.Count > 60)
      {
        CoreThermalPoints.RemoveAt(0);
        PackagePowerPoints.RemoveAt(0);
        Labels.RemoveAt(0);
      }
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
