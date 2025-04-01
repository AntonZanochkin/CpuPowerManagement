using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace CpuPowerManagement.ViewModel
{
  public class MainViewModel : INotifyPropertyChanged
  {
    private int _plLimit1;

    public int PlLimit1
    {
      get => _plLimit1;
      set
      {
        _plLimit1 = value;
        OnPropertyChanged(nameof(PlLimit1));
      }
    }

    public ICommand ApplyPlLimit1Command { get; }
    public MainViewModel()
    {
      ApplyPlLimit1Command = new AsyncRelayCommand<int>(async (i) => await ExecuteApplyPlLimit1Command(i));
    }

    private async Task ExecuteApplyPlLimit1Command(int item)
    {
    

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
