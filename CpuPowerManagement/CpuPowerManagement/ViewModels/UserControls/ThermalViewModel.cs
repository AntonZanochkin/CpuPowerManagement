using CpuPowerManagement.Intel.MSR;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using CpuPowerManagement.Messages;
using static CpuPowerManagement.Intel.MSR.MsrTcc;

namespace CpuPowerManagement.ViewModels.UserControls
{
  public class ThermalViewModel : INotifyPropertyChanged
  {
    private readonly IntelManagement _intelManagement = new();
    public ICommand ApplyTccOffsetCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private MsrTccData _tcc;
    public MsrTccData Tcc
    {
      get => _tcc;
      set
      {
        SetField(ref _tcc, value);
        OnPropertyChanged(nameof(MaxTtp));
        OnPropertyChanged(nameof(MinTtp));
        OnPropertyChanged(nameof(TargetTemperature));
        OnPropertyChanged(nameof(TccOffset));
      }
    }

    public int MinTtp => Tcc.TjMax - 63;
    public int MaxTtp => Tcc.TjMax;

    public int TccOffset
    {
      get => Tcc.TccOffset;
      set
      {
        Tcc.TccOffset = value;
        OnPropertyChanged();
        OnPropertyChanged(nameof(TargetTemperature));
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
        OnPropertyChanged(nameof(Tcc.TccOffset));
      }
    }

    public ThermalViewModel()
    {
      ApplyTccOffsetCommand = new AsyncRelayCommand(ExecuteApplyCommandAsync);

      if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
        Tcc = MsrTccData.CreateMock();
      else
      {
        Tcc = _intelManagement.ReadTccData();

        WeakReferenceMessenger.Default.Register<TemperatureLimitRequestMessage>(this, (r, m) =>
        {
          SendUpdateTemperatureLimitMessage(Tcc.TjMax - Tcc.TccOffset);
        });

        SendUpdateTemperatureLimitMessage(Tcc.TjMax - Tcc.TccOffset);
      }
    }

    private async Task ExecuteApplyCommandAsync()
    {
      _intelManagement.WriteTccOffsetData(_tcc);
      await Task.Delay(1000);
      Tcc = _intelManagement.ReadTccData();

      SendUpdateTemperatureLimitMessage(Tcc.TjMax - Tcc.TccOffset);
    }

    private void SendUpdateTemperatureLimitMessage(int value)
    {
      WeakReferenceMessenger.Default.Send(new UpdateTemperatureLimitMessage(value));
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
