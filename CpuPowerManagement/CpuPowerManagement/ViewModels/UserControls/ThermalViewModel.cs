using CpuPowerManagement.Intel.MSR;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace CpuPowerManagement.ViewModels.UserControls
{
  public class ThermalViewModel : INotifyPropertyChanged
  {
    private readonly IntelManagement _intelManagement = new();
    public ICommand ApplyTccOffsetCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private MsrTcc _tcc;
    public MsrTcc Tcc
    {
      get => _tcc;
      set
      {
        SetField(ref _tcc, value);
        OnPropertyChanged(nameof(MaxTtp));
        OnPropertyChanged(nameof(MinTtp));
        OnPropertyChanged(nameof(TargetTemperature));
      }
    }

    public int MinTtp
    {
      get
      {
        return Tcc.TjMax - 63;
      }
    }

    public int MaxTtp
    {
      get
      {
        return Tcc.TjMax;
      }
    }

    private int _targetTemperature;
    public int TargetTemperature
    {
      get => Tcc.TjMax - Tcc.TccOffset;
      set
      {
        Tcc.TccOffset = Tcc.TjMax - value;

        SetField(ref _targetTemperature, value);
        OnPropertyChanged(nameof(Tcc));
      }
    }

    public ThermalViewModel()
    {
      ApplyTccOffsetCommand = new AsyncRelayCommand(ExecuteApplyCommandAsync);

      if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
        Tcc = MsrTcc.CreateMock();
      else
        Tcc = _intelManagement.ReadTccOffset();
    }

    private async Task ExecuteApplyCommandAsync()
    {
      _intelManagement.WriteTccOffset(_tcc);
      await Task.Delay(1000);
      Tcc = _intelManagement.ReadTccOffset();
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
