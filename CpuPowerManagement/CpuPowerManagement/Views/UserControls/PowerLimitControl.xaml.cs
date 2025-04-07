using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CpuPowerManagement.ViewModels.UserControls;
using CpuPowerManagement.ViewModels.Windows;

namespace CpuPowerManagement.Views.UserControls
{
  public partial class PowerLimitControl : UserControl
  {
    public PowerLimitControl()
    {
      InitializeComponent();

      Loaded += async (s, e) =>
      {
        await Dispatcher.InvokeAsync(() =>
        {
          Console.WriteLine("MainViewModel: " + (MainViewModel != null));
          Console.WriteLine("ViewModel: " + (ViewModel != null));
        }, DispatcherPriority.Loaded);
      };
    }

    public MainWindowViewModel MainViewModel
    {
      get => (MainWindowViewModel)GetValue(MainViewModelProperty);
      set => SetValue(MainViewModelProperty, value);
    }

    public PowerLimitViewModel ViewModel
    {
      get => (PowerLimitViewModel)GetValue(ViewModelProperty);
      set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty MainViewModelProperty =
      DependencyProperty.Register(
        nameof(MainViewModel),
        typeof(MainWindowViewModel),
        typeof(PowerLimitControl),
        new PropertyMetadata(null, OnMainViewModelChanged));

    public static readonly DependencyProperty ViewModelProperty =
      DependencyProperty.Register("ViewModel", typeof(PowerLimitViewModel), typeof(PowerLimitControl), new PropertyMetadata(null, OnViewModelChanged));

    private static void OnMainViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var control = (PowerLimitControl)d;
      var vm = e.NewValue as MainWindowViewModel;
      Debug.WriteLine("✅ MainViewModel set via binding: " + (vm != null));
    }

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var control = (PowerLimitControl)d;
      var vm = e.NewValue as PowerLimitViewModel;
      Debug.WriteLine("✅ PowerLimitViewModel set via binding: " + (vm != null));
    }

  

  }
}
