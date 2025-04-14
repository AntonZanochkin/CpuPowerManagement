using CpuPowerManagement.Intel.MSR;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using LiveCharts;
using LiveCharts.Wpf;
using CommunityToolkit.Mvvm.Messaging;
using CpuPowerManagement.Messages;

namespace CpuPowerManagement.ViewModels.UserControls
{
  public class CpuStatusViewModel : INotifyPropertyChanged
  {
    private readonly IntelManagement _intelManagement = new();
    public event PropertyChangedEventHandler? PropertyChanged;
    private readonly DispatcherTimer _timer;
    private int _time = 0;

    private MsrPackageThermal.MsrPackageThermalData _packageThermalData;

    public ChartValues<int> CpuTemperaturePoints { get; set; } = new();
    public ChartValues<int> CpuTemperatureLimitPoints { get; set; } = new();
    public ChartValues<int> CpuThermalThrottlePoints { get; set; } = new();


    public ChartValues<int> TdpPoints { get; set; } = new();
    public ChartValues<int> TdpLimitPoints { get; set; } = new();
    public ChartValues<int> TdpThrottlePoints { get; set; } = new();

    public ObservableCollection<string> Labels { get; set; } = new();

    public SeriesCollection Series { get; set; }


    private int? _tjMax;
    
    public int TjMax
    {
      get
      {
        _tjMax ??= _intelManagement.ReadTccData().TjMax;
        return _tjMax.Value;
      }
    }

    private Brush _thermalThrottleTextBrush = Brushes.Gray;
    public Brush ThermalThrottleTextBrush
    {
      get => _thermalThrottleTextBrush;
      set => SetField(ref _thermalThrottleTextBrush, value);
    }

    private string _cpuTemperatureText = "CPU: 50 °C";
    public string CpuTemperatureText
    {
      get => _cpuTemperatureText;
      set => SetField(ref _cpuTemperatureText, value);
    }

    private int _cpuTemperatureLimit;
    public int CpuTemperatureLimit
    {
      get => _cpuTemperatureLimit;
      set => SetField(ref _cpuTemperatureLimit, value);
    }

    private MsrPowerLimitData _powerLimitData;
    public MsrPowerLimitData PowerLimitData
    {
      get => _powerLimitData;
      set => SetField(ref _powerLimitData, value);
    }

    public int TpdMax => PowerLimitData.Pl1Enabled ? PowerLimitData.Pl1Watts : 250;

    private Brush _packageThrottleTextBrush = Brushes.Gray;
    public Brush PackageThrottleTextBrush
    {
      get => _packageThrottleTextBrush;
      set => SetField(ref _packageThrottleTextBrush, value);
    }

    private string _tdpText = "TDP: 50 W";
    public string TdpText
    {
      get => _tdpText;
      set => SetField(ref _tdpText, value);
    }

    public CpuStatusViewModel()
    {
      if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
        return;

      Series = new SeriesCollection
      {
        new LineSeries
        {
          Title = "Cpu \u00b0C",
          Values = CpuTemperaturePoints,
          Stroke = Brushes.Red,
          Fill = Brushes.Transparent,
          LabelPoint = point => $"{point.Y * TjMax / 100:F1} °C"
        },
        new LineSeries
        {
          Title = "Cpu Limit °C",
          Values = CpuTemperatureLimitPoints,
          Stroke = Brushes.Red,
          LineSmoothness = 0,
          StrokeThickness = 2,
          StrokeDashArray = new DoubleCollection { 1, 1 },
          PointGeometry = null,
          Fill = Brushes.Transparent,
          LabelPoint = point => $"{point.Y * TjMax / 100:F1} °C"
        },
        // new LineSeries
        // {
        //   Title = "Thermal Throttle",
        //   Values = CpuThermalThrottlePoints,
        //   Stroke = Brushes.DarkRed,
        //   Fill = Brushes.Transparent
        // },

        new LineSeries
        {
          Title = "Package TDP",
          Values = TdpPoints,
          Stroke = Brushes.DeepSkyBlue,
          Fill = Brushes.Transparent,
          LabelPoint = point => $"{point.Y * TpdMax / 100:F1} W"
        },
        new LineSeries
        {
          Title = "Package Limit TDP",
          Values = TdpLimitPoints,
          Stroke = Brushes.DeepSkyBlue,
          StrokeDashArray = new DoubleCollection { 1, 1 },
          PointGeometry = null,
          Fill = Brushes.Transparent,
          LabelPoint = point => $"{point.Y * TpdMax / 100:F1} W"
        },

        // new LineSeries
        // {
        //   Title = "Package Throttle",
        //   Values = TdpThrottlePoints,
        //   Stroke = Brushes.Blue,
        //   Fill = Brushes.Transparent
        // },
        
      };

      WeakReferenceMessenger.Default.Register<UpdateTemperatureLimitMessage>(this, (r, m) =>
      {
        CpuTemperatureLimit = m.Value;
      });

      WeakReferenceMessenger.Default.Send(new TemperatureLimitRequestMessage());

      WeakReferenceMessenger.Default.Register<UpdatePowerLimitDataMessage>(this, (r, m) =>
      {
        PowerLimitData = m.Value;
      });

      WeakReferenceMessenger.Default.Send(new TdpLimitRequestMessage());

      _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
      _timer.Tick += (s, e) => _ = Tick();
      _timer.Start();
    }

    private async Task Tick()
    {
      var packageThermalData = _intelManagement.ReadPackageThermalData();
      var currentTemperature = TjMax - _intelManagement.ReadThermalStatusReadout();
      var packagePowerData = await _intelManagement.ReadPackagePowerAsync();
      
      CpuTemperaturePoints.Add((int)((double)currentTemperature / TjMax * 100));
      CpuTemperatureLimitPoints.Add((int)((double)CpuTemperatureLimit / TjMax * 100d));
      //CpuThermalThrottlePoints.Add(PackageThermalData.ThermalStatus ? 100 : 0);

      TdpPoints.Add((int)((double)packagePowerData.PowerWatts / TpdMax * 100d));
      
      TdpLimitPoints.Add((int)((double)TpdMax / TpdMax * 100d));//:)
      //TdpThrottlePoints.Add(PackageThermalData.PowerLimitStatus ? 100 : 0);
      
      Labels.Add(_time.ToString());
      _time++;

      if (Labels.Count > 60)
      {
        CpuTemperaturePoints.RemoveAt(0);
        CpuTemperatureLimitPoints.RemoveAt(0);
        //CpuThermalThrottlePoints.RemoveAt(0);

        TdpPoints.RemoveAt(0);
        TdpLimitPoints.RemoveAt(0);
        //TdpThrottlePoints.RemoveAt(0);
       
        Labels.RemoveAt(0);
      }

      ThermalThrottleTextBrush = packageThermalData.ThermalStatus ? Brushes.Red : Brushes.Gray;
      CpuTemperatureText = $"CPU: { currentTemperature} °C";

      PackageThrottleTextBrush = packageThermalData.PowerLimitStatus ? Brushes.DeepSkyBlue : Brushes.Gray;

      TdpText = $"TPD: {(int)packagePowerData.PowerWatts} W";
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