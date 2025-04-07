using System.ComponentModel;
using System.Runtime.CompilerServices;
using CpuPowerManagement.Intel.MSR;
using CpuPowerManagement.ViewModels.UserControls;

namespace CpuPowerManagement.ViewModels.Windows
{
  public class MainWindowViewModel : INotifyPropertyChanged
  {
    private IntelManagement _intelManagement = new ();

    public PowerLimitViewModel PowerLimitViewModel { get; private set; }

    public MainWindowViewModel()
    {
      PowerLimitViewModel = new PowerLimitViewModel(_intelManagement);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

  }
}